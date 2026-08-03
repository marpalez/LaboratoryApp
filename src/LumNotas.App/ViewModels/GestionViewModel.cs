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

        Refrescar = new Comando(Explorar);
        VerTablero = new Comando(() => VistaActual = Vista.Tablero);
        VerCalendario = new Comando(() => VistaActual = Vista.Calendario);
        VerCarga = new Comando(() => VistaActual = Vista.Carga);
        ElegirCarpeta = new Comando(() =>
        {
            var elegida = PedirCarpeta?.Invoke(Carpeta);
            if (string.IsNullOrWhiteSpace(elegida)) return;
            Carpeta = elegida;
        });

        if (!string.IsNullOrWhiteSpace(Carpeta)) Explorar();
    }

    public ObservableCollection<ResumenDeProyecto> Proyectos { get; } = [];

    /// <summary>La otra vista de los mismos proyectos: la línea de tiempo.</summary>
    public CalendarioViewModel Calendario { get; }

    /// <summary>Y la tercera: cuánta carga soporta cada técnico cada mes.</summary>
    public CargaViewModel Carga { get; } = new();

    /// <summary>Abrir un proyecto en una pestaña; lo resuelve la ventana.</summary>
    public Action<string>? AbrirProyecto { get; set; }

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
        }
    }

    public bool VistaTablero => _vista == Vista.Tablero;
    public bool VistaCalendario => _vista == Vista.Calendario;
    public bool VistaCarga => _vista == Vista.Carga;

    public Comando VerTablero { get; }
    public Comando VerCalendario { get; }
    public Comando VerCarga { get; }

    /// <summary>Abre el diálogo de planificación de un servicio y guarda lo que se decida.</summary>
    private void Planificar(ResumenDeProyecto proyecto)
    {
        if (PedirPlanificacion is null) return;

        var titulo = string.IsNullOrWhiteSpace(proyecto.CodigoServicio) ? proyecto.Nombre : proyecto.CodigoServicio;
        if (PedirPlanificacion(titulo, proyecto.Planificacion.Copia()) is { } nueva) Guardar(proyecto, nueva);
    }

    /// <summary>
    /// Guarda las fechas y el estado de un servicio. Escribe <b>solo</b> la planificación
    /// releyendo el fichero, así que no toca ni un dato de ensayo aunque el proyecto lo
    /// esté editando otro técnico. Lo usan el diálogo y el arrastre de las barras.
    /// </summary>
    private void Guardar(ResumenDeProyecto proyecto, Planificacion nueva)
    {
        try
        {
            _repositorio.ActualizarPlanificacion(proyecto.Ruta, nueva);
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar la planificación: {ex.Message}";
            return;
        }

        // No hace falta vaciar la caché: el fichero acaba de cambiar de fecha, así que
        // solo ese se relee. Tirarla entera obligaría a releer cientos por uno.
        Explorar();
    }

    // ---- como pestaña ------------------------------------------------------

    /// <summary>El tablero es una pestaña más, así que necesita rótulo y saber si manda.</summary>
    public string Rotulo => "Gestión de proyectos";

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
        set { if (Establecer(ref _estado, value)) Repartir(reencuadrar: true); }
    }

    /// <summary>Técnicos y normas que hay en los proyectos, con «(todos)» al principio.</summary>
    public ObservableCollection<string> Tecnicos { get; } = [Cualquiera];

    public ObservableCollection<string> Normas { get; } = [Cualquiera];

    /// <summary>
    /// Filtrar por responsable. Vale para las tres vistas: en el tablero enseña lo que
    /// lleva esa persona, en el calendario deja solo su fila y en la carga su carga.
    /// </summary>
    public string Tecnico
    {
        get => _tecnico;
        set { if (Establecer(ref _tecnico, value)) Repartir(reencuadrar: true); }
    }

    public string Norma
    {
        get => _norma;
        set { if (Establecer(ref _norma, value)) Repartir(reencuadrar: true); }
    }

    /// <summary>Lo que se ofrece cuando no se quiere filtrar por eso.</summary>
    public const string Cualquiera = "(todos)";

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
            Proyectos.Clear();
            Calendario.Cargar([]);
            Carga.Cargar([]);
            Mensaje = "Elige la carpeta donde el laboratorio guarda los proyectos.";
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
        Rellenar(Normas, todos.SelectMany(p => p.Normas), ref _norma, nameof(Norma));

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
        var visibles = _ultimos.Where(Pasa).ToList();

        Calendario.Cargar(visibles, reencuadrar);
        Carga.Cargar(visibles);

        Proyectos.Clear();
        foreach (var proyecto in visibles) Proyectos.Add(proyecto);

        var ocultos = _ultimos.Count - visibles.Count;

        Mensaje = _ultimos.Count == 0
            ? $"No hay proyectos en {Carpeta}"
            : $"{visibles.Count} proyecto{(visibles.Count == 1 ? "" : "s")}"
              + (ocultos == 0 ? "" : $" · {ocultos} fuera del filtro")
              + $" · leídos en {_leidoEn.TotalSeconds:0.0} s"
              + $" · actualizado a las {DateTime.Now:HH:mm}";
    }

    private bool Pasa(ResumenDeProyecto proyecto)
    {
        if (!FiltroDeEstado.Pasa(proyecto.Planificacion, Estado)) return false;

        if (Tecnico != Cualquiera && !EsSuyo(proyecto)) return false;

        return Norma == Cualquiera || proyecto.Normas.Contains(Norma);
    }

    /// <summary>
    /// Si el servicio lo lleva el técnico elegido. <b>«(sin técnico)» es una opción más</b>:
    /// pedirlo enseña justo los que están sin asignar, que es lo que hay que repartir.
    /// </summary>
    private bool EsSuyo(ResumenDeProyecto proyecto)
        => Tecnico == CargaPorTecnico.SinTecnico
            ? string.IsNullOrWhiteSpace(proyecto.Tecnico)
            : string.Equals(proyecto.Tecnico, Tecnico, StringComparison.CurrentCultureIgnoreCase);

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
        if (etiquetaSiFalta is not null && todos.Any(string.IsNullOrWhiteSpace))
            lista.Add(etiquetaSiFalta);

        foreach (var valor in lista) destino.Add(valor);

        if (destino.Contains(elegido)) return;

        elegido = Cualquiera;
        Notificar(propiedad);
    }
}

/// <summary>Las tres preguntas del responsable sobre la misma carpeta de proyectos.</summary>
public enum Vista
{
    /// <summary>Qué falta por rellenar.</summary>
    Tablero,

    /// <summary>Cuándo toca cada servicio.</summary>
    Calendario,

    /// <summary>Si cabe: cuánta carga soporta cada técnico cada mes.</summary>
    Carga
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
