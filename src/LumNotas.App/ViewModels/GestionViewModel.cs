using System.Collections.ObjectModel;
using System.IO;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Tablero de gestión: una columna por proyecto, una tarjeta por sección pendiente.
/// Los proyectos se detectan escaneando una carpeta, normalmente la de OneDrive.
/// </summary>
public sealed class GestionViewModel : ObservableObject
{
    private readonly ExploradorDeProyectos _explorador;
    private readonly RepositorioDeProyectos _repositorio;
    private readonly PlantillaEnsayos _porDefecto;

    /// <summary>
    /// Las normas instaladas, por id. Cada proyecto se mide contra <b>las suyas</b>: si un
    /// servicio de luminarias lleva además módulos LED, sus apartados cuentan igual.
    /// </summary>
    private readonly IReadOnlyDictionary<string, PlantillaEnsayos> _normasInstaladas;

    private string _carpeta = Ajustes.Cargar().CarpetaDeProyectos;
    private CancellationTokenSource? _enCurso;
    private bool _explorando;
    private string _mensaje = "";
    private string _estado = FiltroDeEstado.EnDesarrollo;
    private string _tecnico = Cualquiera;
    private string _norma = Cualquiera;
    private string _ip = Cualquiera;
    private string _ik = Cualquiera;
    private string _acreditacion = Cualquiera;

    /// <summary>Lo último que se leyó del disco, sin filtrar. Filtrar no vuelve a escanear.</summary>
    private IReadOnlyList<ResumenDeProyecto> _ultimos = [];
    private Vista _vista = Vista.Tablero;

    public GestionViewModel(PlantillaEnsayos porDefecto, RepositorioDeProyectos repositorio)
    {
        _porDefecto = porDefecto;
        _normasInstaladas = LeerNormasInstaladas(porDefecto);
        _repositorio = repositorio;
        _explorador = new ExploradorDeProyectos(repositorio);
        Calendario = new CalendarioViewModel(Planificar, ruta => AbrirProyecto?.Invoke(ruta), Guardar);
        Bbdd.Abrir = ruta => AbrirProyecto?.Invoke(ruta);
        Bbdd.AlExportar = () => ExportarListado?.Invoke(Bbdd.Filas);

        Refrescar = new Comando(Explorar);
        VerTablero = new Comando(() => VistaActual = Vista.Tablero);
        VerCalendario = new Comando(() => VistaActual = Vista.Calendario);
        VerCarga = new Comando(() => VistaActual = Vista.Carga);
        VerBbdd = new Comando(() => VistaActual = Vista.Bbdd);
        ElegirCarpeta = new Comando(() =>
        {
            var elegida = PedirCarpeta?.Invoke(Carpeta);
            if (string.IsNullOrWhiteSpace(elegida)) return;
            Carpeta = elegida;
        });

        if (!string.IsNullOrWhiteSpace(Carpeta)) Explorar();
    }

    /// <summary>
    /// Lo que enseña el tablero. En bloque y no elemento a elemento: con 250 proyectos, un
    /// aviso por cada uno costaba más de un segundo de ventana congelada.
    /// </summary>
    public ColeccionEnBloque<ResumenDeProyecto> Proyectos { get; } = [];

    /// <summary>La otra vista de los mismos proyectos: la línea de tiempo.</summary>
    public CalendarioViewModel Calendario { get; }

    /// <summary>Y la tercera: cuánta carga soporta cada técnico cada mes.</summary>
    public CargaViewModel Carga { get; } = new();

    /// <summary>Y la cuarta: el listado de todo, para buscar un servicio de hace meses.</summary>
    public BbddViewModel Bbdd { get; } = new();

    /// <summary>Abrir un proyecto en una pestaña; lo resuelve la ventana.</summary>
    public Action<string>? AbrirProyecto { get; set; }

    /// <summary>
    /// Sacar en papel el listado de la BBDD; lo resuelve la ventana, que es quien sabe pedir
    /// un fichero y abrir el visor.
    /// <para>
    /// Van solo las filas. Llegó a mandarse también el detalle de los filtros, para que el
    /// papel dijera qué estaba apartando, y el laboratorio lo quitó al verlo: el listado se
    /// mira en el momento y junto a la pantalla de la que sale.
    /// </para>
    /// </summary>
    public Action<IReadOnlyList<FilaDeBbdd>>? ExportarListado { get; set; }

    /// <summary>
    /// Proyectos de la carpeta del laboratorio que ya usan ese código de servicio.
    /// <para>
    /// <b>Relee el disco</b> en vez de mirar lo último escaneado: el proyecto con el que
    /// se choca puede haberlo dado de alta el responsable hace cinco minutos desde otro
    /// equipo, que es justo el caso que hay que cazar. Con la caché de resúmenes cuesta
    /// una décima de segundo, y solo se consulta al guardar un proyecto <b>nuevo</b>,
    /// una vez en la vida de cada uno.
    /// </para>
    /// </summary>
    public IReadOnlyList<ResumenDeProyecto> BuscarPorCodigo(string? codigo, string? rutaPropia = null)
    {
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(Carpeta)) return [];

        try
        {
            var todos = _explorador.Explorar(Carpeta, _normasInstaladas, _porDefecto);
            return ProyectosRepetidos.ConElMismoCodigo(todos, codigo, rutaPropia);
        }
        catch (Exception)
        {
            // Sin carpeta accesible no se puede comprobar, y decirlo sería ruido: no
            // impide guardar ni le sirve de nada al técnico.
            return [];
        }
    }

    /// <summary>
    /// Diálogo de planificación: recibe el título y una copia de lo que hay, y devuelve
    /// lo editado, o <c>null</c> si el técnico canceló. Lo inyecta la ventana.
    /// </summary>
    public Func<string, Planificacion, Planificacion?>? PedirPlanificacion { get; set; }

    // ---- las dos vistas del tablero ---------------------------------------

    /// <summary>
    /// Tablero (qué falta por rellenar) o calendario (cuándo toca cada servicio). Son
    /// dos preguntas distintas sobre la misma carpeta.
    /// </summary>
    public Vista VistaActual
    {
        get => _vista;
        set
        {
            if (!Establecer(ref _vista, value)) return;
            Notificar(nameof(VistaTablero));
            Notificar(nameof(VistaCalendario));
            Notificar(nameof(VistaCarga));
            Notificar(nameof(VistaBbdd));
        }
    }

    public bool VistaTablero => _vista == Vista.Tablero;
    public bool VistaCalendario => _vista == Vista.Calendario;
    public bool VistaCarga => _vista == Vista.Carga;
    public bool VistaBbdd => _vista == Vista.Bbdd;

    public Comando VerTablero { get; }
    public Comando VerCalendario { get; }
    public Comando VerCarga { get; }
    public Comando VerBbdd { get; }

    /// <summary>Abre el diálogo de planificación de un servicio y guarda lo que se decida.</summary>
    private void Planificar(ResumenDeProyecto proyecto)
    {
        if (PedirPlanificacion is null) return;

        // El mismo rótulo que en el tablero y el calendario: el diálogo se abre desde
        // ellos, y que la ventana se llame de otra forma hace dudar de si es el mismo.
        var titulo = proyecto.Rotulo;
        if (PedirPlanificacion(titulo, proyecto.Planificacion.Copia()) is { } nueva) Guardar(proyecto, nueva);
    }

    /// <summary>
    /// Guarda las fechas y el estado de un servicio. Escribe <b>solo</b> la planificación
    /// releyendo el fichero, así que no toca ni un dato de ensayo aunque el proyecto lo
    /// esté editando otro técnico. Lo usan el diálogo y el arrastre de las barras.
    /// </summary>
    private void Guardar(ResumenDeProyecto proyecto, Planificacion nueva)
        => Guardar([(proyecto, nueva)]);

    /// <summary>
    /// Guarda de una vez todo lo que ha cambiado un mismo gesto. El arrastre de un trabajo
    /// enlazado mueve varias familias a la vez, y escribirlas una a una haría que la cadena
    /// se recolocara contra datos a medio guardar — peleándose consigo misma.
    /// </summary>
    private void Guardar(IReadOnlyList<(ResumenDeProyecto Proyecto, Planificacion Plan)> cambios)
    {
        if (cambios.Count == 0) return;

        Aviso = "";

        try
        {
            foreach (var (proyecto, plan) in cambios)
                _repositorio.ActualizarPlanificacion(proyecto.Ruta, plan);
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar la planificación: {ex.Message}";
            return;
        }

        // Un solo cambio es alguien tecleando; varios de golpe son un arrastre, que mueve
        // el trabajo entero sin cambiar el orden. Solo en el primer caso hay una «recién
        // editada» que deba ganar los empates de fecha.
        Recolocar(cambios, editada: cambios.Count == 1 ? cambios[0].Proyecto : null);

        // No hace falta vaciar la caché: los ficheros acaban de cambiar de fecha, así que
        // solo esos se releen. Tirarla entera obligaría a releer cientos por uno.
        Explorar();
    }

    /// <summary>
    /// Pone en fila el trabajo cuando se guarda una toma de notas enlazada (DD‑123): cada
    /// una empieza al día siguiente de que acabe la anterior, y el orden lo dan las fechas
    /// de inicio.
    /// <para>
    /// <b>Escribe en ficheros que nadie ha abierto</b>, así que se dice: el aviso nombra las
    /// que se han movido. Que el programa recoloque por su cuenta y en silencio sería la
    /// forma más rápida de que nadie se fíe del calendario.
    /// </para>
    /// </summary>
    private void Recolocar(
        IReadOnlyList<(ResumenDeProyecto Proyecto, Planificacion Plan)> cambios,
        ResumenDeProyecto? editada)
    {
        var grupo = cambios.Select(c => c.Plan.Grupo).FirstOrDefault(g => !string.IsNullOrWhiteSpace(g));
        if (grupo is null) return;

        // Lo del último escaneo es de antes de guardar, así que se le aplica encima lo que
        // se acaba de escribir.
        var reciente = cambios.ToDictionary(c => c.Proyecto.Ruta, c => c.Plan, StringComparer.OrdinalIgnoreCase);

        var miembros = _ultimos
            .Select(m => reciente.TryGetValue(m.Ruta, out var plan) ? m with { Planificacion = plan } : m)
            .Where(m => EnlaceDeTomasDeNotas.EsElMismoGrupo(m.Planificacion.Grupo, grupo))
            .ToList();

        if (miembros.Count < 2) return;

        var suya = editada is null ? null : miembros.FirstOrDefault(m => m.Ruta == editada.Ruta);
        var movidas = new List<string>();

        foreach (var (otro, plan) in CadenaDelGrupo.Recolocar(miembros, suya, DateTime.Today))
        {
            try
            {
                _repositorio.ActualizarPlanificacion(otro.Ruta, plan);
                movidas.Add(otro.Rotulo);
            }
            catch (Exception ex)
            {
                Mensaje = $"No se pudo recolocar «{otro.Rotulo}»: {ex.Message}";
                return;
            }
        }

        if (movidas.Count > 0)
            Aviso = $"Se {(movidas.Count == 1 ? "ha recolocado" : "han recolocado")} " +
                    $"{string.Join(", ", movidas)}: cada toma de notas del trabajo empieza al " +
                    "día siguiente de que acabe la anterior.";
    }

    private string _aviso = "";

    /// <summary>
    /// Lo que el programa ha hecho por su cuenta en ficheros que nadie había abierto. Va
    /// aparte de <see cref="Mensaje"/> porque el escaneo lo pisaría al terminar de leer, y
    /// entonces nadie se enteraría de que se le han movido fechas a tres tomas de notas.
    /// </summary>
    public string Aviso
    {
        get => _aviso;
        private set { if (Establecer(ref _aviso, value)) Notificar(nameof(HayAviso)); }
    }

    public bool HayAviso => !string.IsNullOrWhiteSpace(Aviso);

    // ---- como pestaña ------------------------------------------------------

    /// <summary>El tablero es una pestaña más, así que necesita rótulo y saber si manda.</summary>
    /// <summary>
    /// Cómo se llama la pestaña. <b>Nombra las dos cosas que se ven dentro</b>: se planifican
    /// tomas de notas —cada una con sus fechas y su importe— y se miran servicios, que es
    /// como el laboratorio agrupa lo que le encarga un cliente.
    /// </summary>
    public string Rotulo => "Planificación de TdN y servicios";

    private bool _esActivo;

    public bool EsActivo
    {
        get => _esActivo;
        set => Establecer(ref _esActivo, value);
    }

    /// <summary>
    /// Carga las plantillas del laboratorio una vez por sesión. Son cuatro ficheros; se
    /// leen aquí y no en cada proyecto porque el tablero recorre cientos de ellos.
    /// </summary>
    private static IReadOnlyDictionary<string, PlantillaEnsayos> LeerNormasInstaladas(PlantillaEnsayos porDefecto)
    {
        var normas = new Dictionary<string, PlantillaEnsayos>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var norma in ServicioDePlantillas.Normas())
            {
                // Una plantilla rota no puede dejar el tablero sin medir las demás.
                try
                {
                    var plantilla = PlantillaEnsayos.Cargar(norma.Ruta);
                    if (!string.IsNullOrWhiteSpace(plantilla.Meta.Id)) normas[plantilla.Meta.Id] = plantilla;
                }
                catch (Exception) { }
            }
        }
        catch (Exception) { }

        if (!string.IsNullOrWhiteSpace(porDefecto.Meta.Id)) normas.TryAdd(porDefecto.Meta.Id, porDefecto);

        return normas;
    }

    public Comando Refrescar { get; }
    public Comando ElegirCarpeta { get; }

    /// <summary>Diálogo de carpeta; lo inyecta la ventana para no atar el modelo a WPF.</summary>
    public Func<string, string?>? PedirCarpeta { get; set; }

    /// <summary>
    /// La carpeta del laboratorio. No es solo dónde están los proyectos: de ella salen
    /// también las normas, la lista de técnicos, la tarifa y la versión publicada, así
    /// que al cambiarla hay que refrescar todo eso — de ahí <see cref="AlCambiarCarpeta"/>.
    /// </summary>
    public string Carpeta
    {
        get => _carpeta;
        set
        {
            // Se relee y se guarda del tirón: la carpeta compartida se escribe desde otro
            // sitio y no puede perderse por guardar aquí una copia vieja de los ajustes.
            Ajustes.Actualizar(a => a.CarpetaDeProyectos = value);
            _carpeta = value;

            Notificar();

            // La caché va por ruta completa, así que cambiar de carpeta no la invalida:
            // volver a la anterior sigue siendo inmediato.
            Explorar();
            AlCambiarCarpeta?.Invoke();
        }
    }

    /// <summary>Lo que hay que rehacer cuando cambia la carpeta; lo resuelve la ventana.</summary>
    public Action? AlCambiarCarpeta { get; set; }

    /// <summary>Se avisa al terminar de leer la carpeta: los avisos de la portada cuentan
    /// los ficheros que no se pudieron leer, y eso solo se sabe tras escanear.</summary>
    public Action? AlExplorar { get; set; }

    /// <summary>Tomas de notas del último escaneo que no se pudieron leer.</summary>
    public int Ilegibles => _ultimos.Count(p => p.Error is not null);

    /// <summary>
    /// Qué proyectos se están mirando, para las tres vistas a la vez. Cambiarlo
    /// <b>no vuelve a escanear</b>: se filtra lo que ya se leyó, así que es instantáneo.
    /// </summary>
    public IReadOnlyList<string> Estados { get; } = FiltroDeEstado.Opciones;

    public string Estado
    {
        get => _estado;
        set { if (Establecer(ref _estado, value)) TrasFiltrar(); }
    }

    /// <summary>Técnicos y normas que hay en los proyectos, con «(todos)» al principio.</summary>
    public ObservableCollection<string> Tecnicos { get; } = [Cualquiera];

    /// <summary>
    /// Las normas que hay en los proyectos, <b>por su designación</b> —«EN IEC 60598‑1:2024
    /// + A11:2024»— y no por su id interno.
    /// <para>
    /// El desplegable ofrecía <c>60598-1_2024</c> y <c>62262_2002_A1</c>, que es como se
    /// llaman los ficheros de plantilla y no como se llama una norma para nadie. Es el mismo
    /// criterio que ya seguían la columna NORMA de la BBDD y la cabecera de la toma de
    /// notas: el id no le dice nada a quien filtra.
    /// </para>
    /// <para>
    /// <b>Se enseña la designación pero se sigue filtrando por el id</b>, que es la
    /// identidad estable de una norma. Las designaciones cambian —la de la 60529 se
    /// corrigió entera el 2026‑08‑06 (DD‑134)— y filtrar por un texto que cambia habría
    /// dejado de encontrar los proyectos guardados.
    /// </para>
    /// </summary>
    public ObservableCollection<OpcionDeNorma> Normas { get; } = [OpcionDeNorma.Todas];

    /// <summary>
    /// Filtrar por responsable. Vale para las tres vistas: en el tablero enseña lo que
    /// lleva esa persona, en el calendario deja solo su fila y en la carga su carga.
    /// </summary>
    public string Tecnico
    {
        get => _tecnico;
        set { if (Establecer(ref _tecnico, value)) TrasFiltrar(); }
    }

    public string Norma
    {
        get => _norma;
        set { if (Establecer(ref _norma, value)) TrasFiltrar(); }
    }

    /// <summary>
    /// Grado IP, grado IK y acreditación. Estaban solo en la BBDD, y eran igual de útiles
    /// en las otras tres: «los IP65 que están en marcha» es una pregunta de tablero. Los
    /// datos ya estaban leídos, así que traerlos aquí no cuesta un solo fichero más.
    /// </summary>
    public string Ip
    {
        get => _ip;
        set { if (Establecer(ref _ip, value)) TrasFiltrar(); }
    }

    public string Ik
    {
        get => _ik;
        set { if (Establecer(ref _ik, value)) TrasFiltrar(); }
    }

    public string Acreditacion
    {
        get => _acreditacion;
        set { if (Establecer(ref _acreditacion, value)) TrasFiltrar(); }
    }

    /// <summary>Lo que ofrecen los tres desplegables, según lo que haya en los proyectos.</summary>
    public ObservableCollection<string> OpcionesIp { get; } = [Cualquiera];
    public ObservableCollection<string> OpcionesIk { get; } = [Cualquiera];
    public ObservableCollection<string> OpcionesAcreditacion { get; } = [Cualquiera];

    private string _busqueda = "";

    /// <summary>
    /// La caja de buscar del tablero, el calendario y la carga. Busca en lo mismo que la
    /// del listado —código, técnicos, norma, acreditación, colaboradores— porque quien
    /// recuerda un servicio no sabe por qué dato lo recuerda.
    /// <para>
    /// <b>Es un filtro más</b>, no una vista aparte: se suma a estado, técnico y norma en
    /// vez de sustituirlos. Buscar «antar» con «En desarrollo» puesto enseña los de
    /// Antares que están en marcha, que es lo que se pregunta desde el tablero. Para
    /// buscar entre todo, incluido lo archivado, está la BBDD.
    /// </para>
    /// <para>
    /// La caja va a la vista, y por eso no cuenta en el rótulo «Filtros (2)»: ese número
    /// avisa de lo que aparta trabajo <b>sin verse</b>, y esto se está viendo.
    /// </para>
    /// </summary>
    public string Busqueda
    {
        get => _busqueda;
        set { if (Establecer(ref _busqueda, value ?? "")) TrasFiltrar(); }
    }

    private DateTime? _desde;
    private DateTime? _hasta;

    /// <summary>
    /// Periodo de ensayo, para responder «¿qué se hizo en el primer trimestre?». Se compara
    /// contra las fechas que la toma de notas apuntó sola al darse por terminada, así que
    /// solo encuentra servicios cerrados — que es justo lo que se busca al preguntar por un
    /// periodo pasado.
    /// </summary>
    public DateTime? Desde
    {
        get => _desde;
        set { if (Establecer(ref _desde, value)) TrasFiltrar(); }
    }

    public DateTime? Hasta
    {
        get => _hasta;
        set { if (Establecer(ref _hasta, value)) TrasFiltrar(); }
    }

    /// <summary>Todos los filtros y la búsqueda, tal y como están ahora mismo.</summary>
    private FiltrosDeGestion Filtros => new()
    {
        Estado = Estado,
        Tecnico = Tecnico,
        Norma = Norma,
        Ip = Ip,
        Ik = Ik,
        Acreditacion = Acreditacion,
        Texto = Busqueda,
        Desde = Desde,
        Hasta = Hasta
    };

    /// <summary>Lo que se ofrece cuando no se quiere filtrar por eso.</summary>
    public const string Cualquiera = ResumenDeFiltros.Cualquiera;

    // ---- lo que delata el botón de filtros ---------------------------------
    //
    // Los tres filtros viven dentro de un diálogo, así que desde la barra no se ve qué
    // hay puesto. Sin esto, alguien mira el tablero, no encuentra su servicio y cree que
    // se ha perdido —cuando lo que hay es un técnico elegido la semana pasada.

    /// <summary>«Filtros», o «Filtros (2)» cuando alguno está apartando trabajo.</summary>
    public string RotuloFiltros => ResumenDeFiltros.Rotulo(Filtros);

    /// <summary>Si hay que destacar el botón por estar filtrando.</summary>
    public bool HayFiltros => ResumenDeFiltros.Cuantos(Filtros) > 0;

    /// <summary>Qué se está viendo, para el consejo emergente y la línea de estado.</summary>
    public string DetalleFiltros => ResumenDeFiltros.Detalle(Filtros, DesignacionDe);

    /// <summary>
    /// Deja los filtros como al abrir el programa. <b>La caja de buscar también</b>: quien
    /// pulsa «Quitar filtros» quiere volver a verlo todo, y dejarle un texto escondiendo
    /// media carpeta sería justo lo que venía a arreglar.
    /// </summary>
    public void QuitarFiltros()
    {
        _estado = FiltroDeEstado.EnDesarrollo;
        _tecnico = Cualquiera;
        _norma = Cualquiera;
        _ip = Cualquiera;
        _ik = Cualquiera;
        _acreditacion = Cualquiera;
        _busqueda = "";
        _desde = null;
        _hasta = null;

        Notificar(nameof(Estado));
        Notificar(nameof(Tecnico));
        Notificar(nameof(Norma));
        Notificar(nameof(Ip));
        Notificar(nameof(Ik));
        Notificar(nameof(Acreditacion));
        Notificar(nameof(Busqueda));
        Notificar(nameof(Desde));
        Notificar(nameof(Hasta));
        TrasFiltrar();
    }

    /// <summary>Refiltra y refresca lo que enseña el botón. Los tres pasan por aquí.</summary>
    private void TrasFiltrar()
    {
        Repartir(reencuadrar: true);
        Notificar(nameof(RotuloFiltros));
        Notificar(nameof(HayFiltros));
        Notificar(nameof(DetalleFiltros));
    }

    public string Mensaje
    {
        get => _mensaje;
        private set => Establecer(ref _mensaje, value);
    }

    public bool HayCarpeta => !string.IsNullOrWhiteSpace(Carpeta);

    /// <summary>Si hay un escaneo en marcha, para poder avisarlo y cancelarlo.</summary>
    public bool Explorando
    {
        get => _explorando;
        private set => Establecer(ref _explorando, value);
    }

    /// <summary>
    /// Recorre la carpeta y calcula el estado de cada proyecto.
    /// <para>
    /// Va <b>en segundo plano</b>: en el laboratorio los proyectos cuelgan de una
    /// matrioska de carpetas de clientes con años de historia, y hacerlo en el hilo de la
    /// ventana la dejaba congelada. Mientras dura se sigue viendo lo anterior y se cuenta
    /// por dónde va; los resultados sustituyen a lo viejo solo al terminar.
    /// </para>
    /// </summary>
    private async void Explorar()
    {
        Notificar(nameof(HayCarpeta));

        if (!HayCarpeta)
        {
            _ultimos = [];
            Proyectos.Vaciar();
            Calendario.Cargar([]);
            Carga.Cargar([]);
            Bbdd.Cargar([]);
            Mensaje = "Elige la carpeta donde el laboratorio guarda las tomas de notas.";
            return;
        }

        // Un escaneo nuevo manda sobre el que estuviera en marcha: si se cambia de
        // carpeta a media faena, lo que llegue de la anterior ya no interesa.
        _enCurso?.Cancel();
        var cancelacion = _enCurso = new CancellationTokenSource();

        Explorando = true;
        var aviso = new Progress<AvanceDeExploracion>(a => Mensaje = a.Texto);
        var reloj = System.Diagnostics.Stopwatch.StartNew();

        IReadOnlyList<ResumenDeProyecto> todos;

        try
        {
            var carpeta = Carpeta;

            todos = await Task.Run(
                () => _explorador.Explorar(carpeta, _normasInstaladas, _porDefecto, true,
                                           aviso, cancelacion.Token),
                cancelacion.Token);
        }
        catch (OperationCanceledException)
        {
            return;   // manda el escaneo que lo canceló
        }
        catch (Exception ex)
        {
            if (!cancelacion.IsCancellationRequested)
            {
                Explorando = false;
                Mensaje = "No se pudo leer la carpeta: " + ex.Message;
            }
            return;
        }
        finally
        {
            if (_enCurso == cancelacion) Explorando = false;
        }

        if (cancelacion.IsCancellationRequested) return;

        _ultimos = todos;
        _leidoEn = reloj.Elapsed;

        Rellenar(Tecnicos, todos.Select(p => p.Tecnico), ref _tecnico, nameof(Tecnico),
                 CargaPorTecnico.SinTecnico);
        RellenarNormas(todos.SelectMany(p => p.Normas));
        Rellenar(OpcionesIp, todos.Select(p => p.GradoIp), ref _ip, nameof(Ip));
        Rellenar(OpcionesIk, todos.Select(p => p.GradoIk), ref _ik, nameof(Ik));
        Rellenar(OpcionesAcreditacion, todos.SelectMany(p => p.Acreditaciones),
                 ref _acreditacion, nameof(Acreditacion));

        Repartir(reencuadrar: false);
        AlExplorar?.Invoke();
    }

    private TimeSpan _leidoEn;

    /// <summary>
    /// Reparte a las tres vistas lo que pasa el filtro. Es lo que se rehace al cambiar de
    /// filtro, sin volver a tocar el disco.
    /// </summary>
    /// <param name="reencuadrar">
    /// Si el calendario debe reajustar su eje. Al cambiar de filtro sí —se está mirando
    /// otra cosa—; al refrescar los datos no, para que no se mueva bajo el ratón.
    /// </param>
    private void Repartir(bool reencuadrar)
    {
        var filtros = Filtros;
        var visibles = _ultimos.Where(filtros.Pasa).ToList();

        Calendario.Cargar(visibles, reencuadrar);
        Carga.Cargar(visibles);

        // El listado recibe lo mismo que las otras tres. Antes recibía todo, saltándose el
        // filtro; ahora los filtros son un solo juego para las cuatro vistas, y eso
        // incluye el estado: para buscar algo archivado hay que poner «Estado» en
        // «(todos)», y el botón lo dice sin abrirlo.
        Bbdd.Cargar(visibles);
        Proyectos.Reemplazar(visibles);

        _visibles = visibles.Count;
        _actualizadoA = DateTime.Now;
        Mensaje = ResumenDeLoLeido();
    }

    private int _visibles;
    private DateTime _actualizadoA;

    /// <summary>
    /// La línea de estado de debajo de la barra. Vale igual para las cuatro vistas: desde
    /// que los filtros son un solo juego, todas enseñan lo mismo.
    /// </summary>
    private string ResumenDeLoLeido()
    {
        if (_ultimos.Count == 0) return $"No hay tomas de notas en {Carpeta}";

        var ocultos = _ultimos.Count - _visibles;

        // «TdN» y no «tomas de notas»: la línea cuenta cuatro datos separados por barras y
        // el rótulo largo se comería el sitio de los otros tres.
        return $"{_visibles} TdN"
               + (ocultos == 0 ? "" : $" | {ocultos} fuera del filtro")
               + $" | leídos en {_leidoEn.TotalSeconds:0.0} s"
               + $" | actualizado a las {_actualizadoA:HH:mm}";
    }

    /// <summary>
    /// Rehace la lista de un desplegable con los valores que hay en los proyectos. Si el
    /// elegido ya no existe —se archivó el último servicio de ese técnico— se vuelve a
    /// «(todos)» en vez de dejar el tablero vacío sin explicar por qué.
    /// </summary>
    /// <param name="etiquetaSiFalta">
    /// Qué ofrecer cuando <b>hay proyectos sin ese dato</b>. En técnicos es «(sin
    /// técnico)», el mismo rótulo con el que el calendario y la carga los agrupan: sin
    /// esta opción no había manera de pedir los que están sin asignar, que es justo lo
    /// que el responsable quiere ver para repartirlos.
    /// </param>
    private void Rellenar(ObservableCollection<string> destino, IEnumerable<string> valores,
                          ref string elegido, string propiedad, string? etiquetaSiFalta = null)
    {
        var todos = valores.ToList();

        var lista = todos.Where(v => !string.IsNullOrWhiteSpace(v))
                         .Distinct(StringComparer.CurrentCultureIgnoreCase)
                         .OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase)
                         .ToList();

        destino.Clear();
        destino.Add(Cualquiera);

        // Al final, después de las personas: es un cajón, no un compañero más.
        //
        // Y solo si no está ya: desde que el catálogo arranca con «(sin técnico)» dentro,
        // puede haber proyectos que lo lleven escrito. Sin esta comprobación el
        // desplegable ofrecía la misma opción dos veces.
        if (etiquetaSiFalta is not null && todos.Any(string.IsNullOrWhiteSpace)
            && !lista.Contains(etiquetaSiFalta, StringComparer.CurrentCultureIgnoreCase))
            lista.Add(etiquetaSiFalta);

        foreach (var valor in lista) destino.Add(valor);

        if (destino.Contains(elegido)) return;

        elegido = Cualquiera;
        Notificar(propiedad);
    }

    /// <summary>
    /// Igual que <see cref="Rellenar"/> pero traduciendo cada id a su designación. Va aparte
    /// porque es el único desplegable donde lo que se enseña y lo que se filtra no son la
    /// misma cadena.
    /// </summary>
    /// <remarks>
    /// Una norma que ya no esté instalada —o un proyecto viejo con un id retirado— se queda
    /// <b>con su id como rótulo</b> en vez de desaparecer del desplegable. Feo, pero honrado:
    /// esos proyectos existen y hay que poder filtrarlos; borrarlos de la lista los volvería
    /// inalcanzables sin decir por qué.
    /// </remarks>
    private void RellenarNormas(IEnumerable<string> ids)
    {
        var lista = ids.Where(id => !string.IsNullOrWhiteSpace(id))
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .Select(id => new OpcionDeNorma(id, DesignacionDe(id)))
                       .OrderBy(o => o.Rotulo, StringComparer.CurrentCultureIgnoreCase)
                       .ToList();

        Normas.Clear();
        Normas.Add(OpcionDeNorma.Todas);
        foreach (var opcion in lista) Normas.Add(opcion);

        if (Normas.Any(o => o.Id.Equals(_norma, StringComparison.OrdinalIgnoreCase))) return;

        _norma = Cualquiera;
        Notificar(nameof(Norma));
    }

    /// <summary>Cómo se llama esa norma, o su id si el laboratorio ya no la tiene instalada.</summary>
    private string DesignacionDe(string id)
        => _normasInstaladas.TryGetValue(id, out var plantilla)
            ? plantilla.Meta.ComoSeLlamaLaNorma
            : id;
}

/// <summary>
/// Una norma en el desplegable de filtros: se <b>enseña</b> su designación y se
/// <b>filtra</b> por su id.
/// <para>
/// Son dos cosas distintas y por eso van en dos campos. El id es la identidad estable de
/// una norma; la designación es cómo se llama hoy, y cambia — la de la 60529 se corrigió
/// entera (DD‑134). Guardar el filtro por designación habría dejado de encontrar los
/// proyectos el día que se corrigiera un texto.
/// </para>
/// </summary>
public sealed record OpcionDeNorma(string Id, string Rotulo)
{
    /// <summary>La opción de no filtrar. Se llama igual por los dos lados.</summary>
    public static OpcionDeNorma Todas { get; } =
        new(FiltrosDeGestion.Cualquiera, FiltrosDeGestion.Cualquiera);
}

/// <summary>Las tres preguntas del responsable sobre la misma carpeta de proyectos.</summary>
public enum Vista
{
    /// <summary>Qué falta por rellenar.</summary>
    Tablero,

    /// <summary>Cuándo toca cada servicio.</summary>
    Calendario,

    /// <summary>Si cabe: cuánta carga soporta cada técnico cada mes.</summary>
    Carga,

    /// <summary>El listado de todo, con buscador. Solo lee.</summary>
    Bbdd
}

/// <summary>Ajustes de la aplicación, guardados en el perfil del usuario.</summary>
public sealed class Ajustes
{
    private static string Ruta => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LumNotas", "ajustes.json");

    /// <summary>Dónde están los proyectos. Se escanea entera, con sus subcarpetas.</summary>
    public string CarpetaDeProyectos { get; set; } = "";

    /// <summary>
    /// Dónde está lo compartido: normas, técnicos, tarifa y versión publicada.
    /// <para>
    /// Va aparte de los proyectos porque en el laboratorio <b>no son la misma carpeta</b>:
    /// los proyectos cuelgan de la de clientes, cada uno en su rama, y la configuración
    /// vive en otro sitio. Si se deja en blanco se usa la de proyectos, para no romper a
    /// quien las tenga juntas.
    /// </para>
    /// </summary>
    public string CarpetaCompartida { get; set; } = "";

    /// <summary>
    /// Si ya se ha preguntado por las carpetas al arrancar. Se pregunta una vez y no se
    /// vuelve a insistir: quien las deje sin elegir lo hará a sabiendas, y siempre puede
    /// ponerlas desde «Configuración».
    /// </summary>
    public bool CarpetaYaPreguntada { get; set; }

    /// <summary>
    /// Lee, cambia y guarda de una vez. Hace falta porque hay dos sitios que escriben
    /// ajustes: si cada uno guardara su copia en memoria, el último borraría lo que
    /// hubiera cambiado el otro.
    /// </summary>
    public static void Actualizar(Action<Ajustes> cambio)
    {
        var ajustes = Cargar();
        cambio(ajustes);
        ajustes.Guardar();
    }

    public static Ajustes Cargar()
    {
        try
        {
            return File.Exists(Ruta)
                ? System.Text.Json.JsonSerializer.Deserialize<Ajustes>(File.ReadAllText(Ruta)) ?? new Ajustes()
                : new Ajustes();
        }
        catch
        {
            return new Ajustes();   // unos ajustes corruptos no deben impedir arrancar
        }
    }

    public void Guardar()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
            File.WriteAllText(Ruta, System.Text.Json.JsonSerializer.Serialize(this,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Si el perfil no es escribible, se sigue trabajando sin recordar la carpeta.
        }
    }
}
