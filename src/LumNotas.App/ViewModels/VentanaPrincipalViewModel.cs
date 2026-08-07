using System.Collections.ObjectModel;
using System.IO;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;

namespace LumNotas.App.ViewModels;

/// <summary>
/// La ventana: una pestaña por proyecto abierto, más el tablero de gestión. El técnico
/// suele llevar dos o tres servicios a la vez —mientras una muestra pasa 48 h en la
/// cámara de humedad ensaya otra— y antes había que cerrar uno para mirar el otro.
/// <para>El trabajo de cada proyecto vive en <see cref="DocumentoViewModel"/>.</para>
/// </summary>
public sealed class VentanaPrincipalViewModel : ObservableObject
{
    private readonly RepositorioDeProyectos _repositorio = new();
    private readonly ProyectosRecientes _recientes = new();
    private object? _activo;

    public VentanaPrincipalViewModel()
    {
        Normas = LeerNormas();
        Gestion = new GestionViewModel(PlantillaDeReferencia(), _repositorio)
        {
            // Desde el calendario se salta al proyecto sin pasar por el explorador.
            AbrirProyecto = AbrirEnPestana,
            AlCambiarCarpeta = AdoptarCarpetaDelLaboratorio,
            // Los ficheros ilegibles solo se saben tras escanear.
            AlExplorar = RefrescarAvisos
        };

        // Desde el panel de planificación de una toma de notas se salta al calendario.
        Servicios.VerEnElCalendario = VerEnElCalendario;

        NuevaPestana = new Comando(() => AbrirPestana());
        CerrarPestana = new ComandoCon<object>(Cerrar);
        ActivarPestana = new ComandoCon<object>(p => Activo = p);
        Abrir = new Comando(AbrirProyecto);
        AbrirReciente = new ComandoCon<string>(ruta => AbrirEnPestana(ruta));
        EmpezarConNorma = new ComandoCon<NormaDisponible>(EmpezarCon);
        IrAGestion = new Comando(() => AbrirGestion(null));
        IrAVistaDeGestion = new ComandoCon<Vista>(vista => AbrirGestion(vista));
        EditarTecnicos = new Comando(AbrirEditorDeTecnicos);
        EditarCapacidad = new Comando(() =>
        {
            // La tabla de carga se calcula con estos números: hay que rehacerla.
            if (Servicios.EditarCapacidad?.Invoke() is true) Gestion.Carga.Recalcular();
        });
        VerPlantillas = new Comando(() => Servicios.VerPlantillas?.Invoke());
        ElegirCarpetaDelLaboratorio = new Comando(() => Servicios.ElegirCarpetaDelLaboratorio?.Invoke());
        ReportarProblema = new Comando(() => Servicios.ReportarProblema?.Invoke());
        NuevoProyecto = new Comando(DarDeAltaUnProyecto);
        VerInicio = new Comando(IrAInicio);
        VerAcercaDe = new Comando(() =>
        {
            Servicios.VerAcercaDe?.Invoke();
            Notificar(nameof(HayVersionMasNueva));
        });
        OcultarAvisoDeVersion = new Comando(() =>
        {
            _avisoDescartado = true;
            Notificar(nameof(HayVersionMasNueva));
        });

        RefrescarRecientes();
        RefrescarAvisos();
        AbrirPestana();
    }

    /// <summary>Normas instaladas en la carpeta de plantillas.</summary>
    public IReadOnlyList<NormaDisponible> Normas { get; }

    /// <summary>Servicios de ventana, que rellena <c>MainWindow</c> al arrancar.</summary>
    public ServiciosDeVentana Servicios { get; } = new();

    /// <summary>Pestañas abiertas: documentos y, si se ha pedido, el tablero de gestión.</summary>
    public ObservableCollection<object> Pestanas { get; } = [];

    public object? Activo
    {
        get => _activo;
        set
        {
            if (!Establecer(ref _activo, value)) return;

            foreach (var pestana in Pestanas)
                switch (pestana)
                {
                    case DocumentoViewModel d: d.EsActivo = d == value; break;
                    case GestionViewModel g: g.EsActivo = g == value; break;
                }

            Notificar(nameof(Titulo));
            Notificar(nameof(HayDocumento));
            Notificar(nameof(ActivoDocumento));
        }
    }

    /// <summary>La pestaña de delante, si es un proyecto. El tablero no se guarda.</summary>
    public DocumentoViewModel? ActivoDocumento => Activo as DocumentoViewModel;

    public bool HayDocumento => Activo is DocumentoViewModel { SinProyecto: false };

    public GestionViewModel Gestion { get; }
    /// <summary>Todos los recientes. Los enseña el menú <c>Archivo</c>.</summary>
    public ObservableCollection<ProyectoReciente> Recientes { get; } = [];

    /// <summary>Los tres últimos, que es lo que cabe en la portada sin estorbar.</summary>
    public ObservableCollection<ProyectoReciente> UltimosAbiertos { get; } = [];

    /// <summary>
    /// Lo que este equipo tiene mal configurado. <b>Vacía casi siempre</b>: si no hay nada
    /// que hacer, el recuadro de la portada no existe.
    /// </summary>
    public ObservableCollection<AvisoViewModel> Avisos { get; } = [];

    public bool HayAvisos => Avisos.Count > 0;

    /// <summary>
    /// Vuelve a mirar cómo está el equipo. Se llama al arrancar, al cambiar de carpeta y
    /// al terminar cada escaneo — que es cuando se sabe si hay ficheros ilegibles.
    /// </summary>
    public void RefrescarAvisos()
    {
        var ajustes = Ajustes.Cargar();
        var compartida = ServicioDeCarpetas.Compartida();

        var pendientes = PlantillasCompartidas.Comparar(
            PlantillasCompartidas.LocalSiExiste(), compartida);

        var estado = new AvisosDeInicio.Estado(
            CarpetaDeProyectos: ajustes.CarpetaDeProyectos,
            ProyectosAccesible: Existe(ajustes.CarpetaDeProyectos),
            // La elegida, no la que se use de respaldo: el aviso habla de lo configurado.
            CarpetaCompartida: ajustes.CarpetaCompartida,
            CompartidaAccesible: Existe(ajustes.CarpetaCompartida),
            HayNormasPublicadas: ServicioDePlantillas.Origen.EsCompartida,
            NormasSinPublicar: pendientes.Nuevas,
            NormasMasNuevas: pendientes.MasNuevas,
            ProyectosIlegibles: Gestion.Ilegibles);

        Avisos.Clear();
        foreach (var aviso in AvisosDeInicio.Revisar(estado))
            Avisos.Add(new AvisoViewModel(aviso, Atender));

        Notificar(nameof(HayAvisos));
    }

    private static bool Existe(string? carpeta)
        => !string.IsNullOrWhiteSpace(carpeta) && Directory.Exists(carpeta);

    private void Atender(AccionDeAviso accion)
    {
        switch (accion)
        {
            case AccionDeAviso.ElegirCarpetas: ElegirCarpetaDelLaboratorio.Execute(null); break;
            case AccionDeAviso.VerNormas: VerPlantillas.Execute(null); break;
            case AccionDeAviso.IrAlTablero: IrAVistaDeGestion.Execute(Vista.Tablero); break;
        }

        RefrescarAvisos();
    }

    public bool HayRecientes => Recientes.Count > 0;

    public Comando NuevaPestana { get; }
    public ComandoCon<object> CerrarPestana { get; }
    public ComandoCon<object> ActivarPestana { get; }
    public Comando Abrir { get; }
    public ComandoCon<string> AbrirReciente { get; }
    public ComandoCon<NormaDisponible> EmpezarConNorma { get; }
    public Comando IrAGestion { get; }

    /// <summary>
    /// Entra en gestión directamente por una de sus tres vistas. Desde la portada se
    /// elige a qué se va, que es lo que se tenía en la cabeza al pulsar; el menú sigue
    /// usando <see cref="IrAGestion"/>, que respeta la vista en la que se estaba.
    /// </summary>
    public ComandoCon<Vista> IrAVistaDeGestion { get; }
    public Comando EditarTecnicos { get; }
    public Comando EditarCapacidad { get; }
    public Comando VerPlantillas { get; }
    public Comando ElegirCarpetaDelLaboratorio { get; }
    public Comando ReportarProblema { get; }
    public Comando VerAcercaDe { get; }

    /// <summary>Alta de un proyecto para planificarlo, sin abrir su toma de notas.</summary>
    public Comando NuevoProyecto { get; }

    /// <summary>Volver a la portada sin tener que cerrar lo que se esté haciendo.</summary>
    public Comando VerInicio { get; }

    /// <summary>
    /// Crea el proyecto y <b>se queda en gestión</b>, en el calendario. No se abre su toma
    /// de notas: quien da de alta un proyecto lo hace para planificarlo, y rellenarla es
    /// cosa del técnico cuando empiece.
    /// </summary>
    private void DarDeAltaUnProyecto()
    {
        if (Servicios.CrearProyecto?.Invoke(Gestion.Carpeta) is null) return;

        // El proyecto nuevo tiene que aparecer sin que nadie pulse «Actualizar»: si no,
        // parecería que no se ha creado.
        Gestion.Refrescar.Execute(null);
        AbrirGestion(Vista.Calendario);
    }

    /// <summary>
    /// De la carpeta del laboratorio salen los proyectos, las normas, los técnicos, la
    /// tarifa y la versión publicada. Al cambiarla hay que releerlo todo, o el programa
    /// se queda enseñando datos de la carpeta anterior.
    /// <para>
    /// Las <b>normas no se recargan en caliente</b> a propósito: se resuelven una vez por
    /// sesión, así que cambiar de carpeta con proyectos abiertos dejaría unas pestañas
    /// con una versión de la norma y otras con otra. Se avisa de que hay que reiniciar.
    /// </para>
    /// </summary>
    public void AdoptarCarpetaCompartida() => AdoptarCarpetaDelLaboratorio();

    private void AdoptarCarpetaDelLaboratorio()
    {
        ServicioDeTecnicos.Recargar();
        ServicioDeCapacidad.Recargar();
        ServicioDeVersion.Olvidar();

        foreach (var documento in Pestanas.OfType<DocumentoViewModel>()) documento.RefrescarTecnicos();

        Gestion.Carga.Recalcular();
        Notificar(nameof(HayVersionMasNueva));
        RefrescarAvisos();
    }
    public Comando OcultarAvisoDeVersion { get; }

    private bool _avisoDescartado;

    /// <summary>
    /// Este equipo se ha quedado con una versión anterior a la que el laboratorio da por
    /// buena. Se avisa, no se bloquea: dejar sin trabajar a un laboratorio porque un
    /// fichero de OneDrive dice otra cosa sería peor que el problema.
    /// </summary>
    public bool HayVersionMasNueva => !_avisoDescartado && ServicioDeVersion.HayMasNueva;

    /// <summary>
    /// Nombre y versión, para la portada. La versión se enseña ahí y no solo en «Acerca
    /// de» porque es lo primero que hay que preguntar cuando alguien llama diciendo que
    /// algo no le funciona: ahora se lee sin abrir ningún menú.
    /// </summary>
    public static string NombreDelPrograma => ServicioDeVersion.Nombre;

    public static string VersionEnEjecucion => "v" + ServicioDeVersion.EnEjecucion;

    public string TextoDeVersionMasNueva =>
        $"Hay una versión más nueva del programa: {ServicioDeVersion.Publicada?.Version}. "
        + $"Estás usando la {ServicioDeVersion.EnEjecucion}.";

    /// <summary>
    /// Abre el editor de técnicos y, si se ha tocado la lista, refresca los desplegables
    /// de <b>todas</b> las pestañas abiertas: si no, seguirían ofreciendo la lista vieja.
    /// </summary>
    private void AbrirEditorDeTecnicos()
    {
        if (Servicios.EditarTecnicos?.Invoke() is not { } renombrados) return;

        foreach (var documento in Pestanas.OfType<DocumentoViewModel>())
            documento.RefrescarTecnicos(renombrados);
    }

    public string Titulo => ActivoDocumento?.Titulo ?? NombreDelPrograma;

    // La franja oscura del pie de ventana se quitó el 2026‑08‑06 (DD‑133), y con ella la
    // propiedad que la alimentaba. Ocupaba sitio en todas las pantallas para no decir nada
    // casi nunca. Lo que se decía por ahí se repartió: **lo que falla va a una ventana**
    // —no se pudo abrir, guardar o exportar, y qué falta para poder guardar— y **las
    // confirmaciones se quitaron**, porque ya se veían por otro lado.

    // ---- pestañas ----------------------------------------------------------

    /// <summary>
    /// Vuelve a la portada. Si ya hay una pestaña vacía se salta a ella en vez de abrir
    /// otra: la portada se enseña en cualquier pestaña sin proyecto, y volver a Inicio
    /// tres veces no debería dejar tres pestañas iguales abiertas.
    /// </summary>
    private void IrAInicio()
    {
        var vacia = Pestanas.OfType<DocumentoViewModel>().FirstOrDefault(d => d.SinProyecto);
        Activo = vacia ?? AbrirPestana();
    }

    /// <summary>Abre una pestaña nueva, que arranca enseñando la portada.</summary>
    public DocumentoViewModel AbrirPestana()
    {
        var documento = new DocumentoViewModel(Normas, _repositorio, Servicios, Registrar);

        documento.Cambio = () =>
        {
            Notificar(nameof(Titulo));
            Notificar(nameof(HayDocumento));
        };

        Pestanas.Add(documento);
        Activo = documento;
        return documento;
    }

    /// <summary>
    /// Cierra una pestaña avisando si tiene cambios. Si era la última, se deja una vacía:
    /// una ventana sin pestañas no tiene nada que enseñar.
    /// </summary>
    private void Cerrar(object pestana)
    {
        if (pestana is DocumentoViewModel documento && !documento.ConfirmarSiHayCambios()) return;

        var posicion = Pestanas.IndexOf(pestana);
        Pestanas.Remove(pestana);

        if (Pestanas.Count == 0) AbrirPestana();
        else Activo = Pestanas[Math.Min(posicion, Pestanas.Count - 1)];
    }

    /// <summary>
    /// El tablero es una pestaña más, pero solo una: si ya está abierto se salta a ella
    /// en vez de repetirlo.
    /// </summary>
    /// <param name="vista">
    /// A qué vista se entra. <c>null</c> deja la que estuviera: volver desde el menú a
    /// media planificación no debería devolverte al tablero.
    /// </param>
    private void AbrirGestion(Vista? vista)
    {
        if (!Pestanas.Contains(Gestion)) Pestanas.Add(Gestion);
        if (vista is { } elegida) Gestion.VistaActual = elegida;
        Activo = Gestion;
    }

    /// <summary>
    /// Salta al calendario buscando un servicio. Lo pide la toma de notas, desde su panel
    /// de planificación: es el camino de «lo tengo delante, enséñame cuándo tocaba».
    /// <para>
    /// <b>Deja los filtros como haga falta para que se vea.</b> Antes no los tocaba, y un
    /// servicio terminado o archivado llevaba a un calendario vacío: el botón parecía roto
    /// (2026‑08‑05). El estado solo se cambia en esos dos casos; el técnico, siempre, para
    /// dejar su fila a la vista.
    /// </para>
    /// </summary>
    public void VerEnElCalendario(DestinoDelCalendario destino)
    {
        if (string.IsNullOrWhiteSpace(destino.Codigo)) return;

        Gestion.Busqueda = destino.Codigo;

        if (destino.Estado is { } estado) Gestion.Estado = estado;
        Gestion.Tecnico = TecnicoQueSePuedeElegir(destino.Tecnico);

        AbrirGestion(Vista.Calendario);
    }

    /// <summary>
    /// El técnico del servicio, si está entre los que ofrece el desplegable. Si no lo está
    /// —el proyecto vive fuera de la carpeta que se escanea, o su responsable ya no tiene
    /// ningún otro— se pone «(todos)»: filtrar por alguien que no está en la lista dejaría
    /// el calendario vacío, que es justo lo que este botón viene a evitar.
    /// </summary>
    private string TecnicoQueSePuedeElegir(string? tecnico)
    {
        var suyo = (tecnico ?? "").Trim();

        if (string.IsNullOrEmpty(suyo))
            return Gestion.Tecnicos.Contains(CargaPorTecnico.SinTecnico)
                ? CargaPorTecnico.SinTecnico
                : GestionViewModel.Cualquiera;

        return Gestion.Tecnicos.FirstOrDefault(
            t => string.Equals(t, suyo, StringComparison.CurrentCultureIgnoreCase))
            ?? GestionViewModel.Cualquiera;
    }

    private void EmpezarCon(NormaDisponible norma)
    {
        var documento = ActivoDocumento ?? AbrirPestana();
        documento.EmpezarCon(norma);
        Activo = documento;
    }

    private void AbrirProyecto()
    {
        var ruta = Servicios.PedirFicheroParaAbrir?.Invoke();
        if (!string.IsNullOrWhiteSpace(ruta)) AbrirEnPestana(ruta);
    }

    /// <summary>
    /// Abre un fichero. Si ya está abierto en otra pestaña, salta a ella en vez de
    /// duplicarlo: dos pestañas sobre el mismo fichero se pisan los guardados.
    /// </summary>
    public void AbrirEnPestana(string ruta)
    {
        if (Pestanas.OfType<DocumentoViewModel>()
                    .FirstOrDefault(d => string.Equals(d.Ruta, ruta, StringComparison.OrdinalIgnoreCase))
            is { } yaAbierto)
        {
            // No se dice nada: la pestaña salta sola a la que ya estaba abierta, y eso se
            // ve. Antes salía escrito en la franja del pie, que ya no existe (DD‑133).
            Activo = yaAbierto;
            return;
        }

        // Una pestaña sin estrenar se aprovecha; si no, se abre otra.
        var documento = ActivoDocumento is { SinProyecto: true } vacia ? vacia : AbrirPestana();

        if (documento.CargarDesde(ruta))
        {
            Activo = documento;
        }
    }

    private void Registrar(string ruta)
    {
        _recientes.Registrar(ruta);
        RefrescarRecientes();
    }

    /// <summary>
    /// Cuántos se enseñan en la portada. Tres: ahí es un atajo para volver a lo de ayer,
    /// no un historial — una lista larga empuja hacia abajo lo que de verdad se usa. El
    /// menú <c>Archivo | Proyectos recientes</c> los sigue enseñando todos, que para eso
    /// hay que ir a buscarlo.
    /// </summary>
    /// <summary>
    /// Cuántas tomas de notas recientes salen en la portada: <b>cuatro</b>, en dos columnas
    /// de dos. La lista completa sigue en <c>Archivo</c>.
    /// <para>
    /// <b>Tiene que ser múltiplo del número de columnas.</b> Con cinco quedaba una fila a
    /// medias y un hueco a la vista; es lo único que decide este número, más que cuántas
    /// recientes convenga ofrecer. Si algún día la rejilla pasa a una columna o a tres,
    /// hay que volver a cuadrarlo o reaparece el hueco.
    /// </para>
    /// </summary>
    private const int RecientesEnPortada = 4;

    private void RefrescarRecientes()
    {
        Recientes.Clear();
        foreach (var r in _recientes.Existentes) Recientes.Add(new ProyectoReciente(r));

        UltimosAbiertos.Clear();
        foreach (var r in _recientes.Existentes.Take(RecientesEnPortada))
            UltimosAbiertos.Add(new ProyectoReciente(r));

        Notificar(nameof(HayRecientes));
    }

    // ---- cierre ------------------------------------------------------------

    /// <summary>
    /// Si se puede cerrar la aplicación: pregunta por <b>cada</b> pestaña con cambios,
    /// no solo por la de delante. Basta con que una se cancele para no cerrar.
    /// </summary>
    public bool PuedeSalir()
    {
        foreach (var documento in Pestanas.OfType<DocumentoViewModel>().ToList())
        {
            if (documento.HayCambiosSinGuardar) Activo = documento;
            if (!documento.ConfirmarSiHayCambios()) return false;
        }

        return true;
    }

    private static IReadOnlyList<NormaDisponible> LeerNormas()
    {
        // Si no se encuentra la carpeta, la aplicación arranca igual: simplemente no
        // hay normas que ofrecer en la portada.
        try { return ServicioDePlantillas.Normas(); }
        catch (Exception) { return []; }
    }

    /// <summary>
    /// El tablero necesita una plantilla para medir el avance. Se le da la de
    /// luminarias, que es la de uso más frecuente, hasta que se abra un proyecto.
    /// </summary>
    private PlantillaEnsayos PlantillaDeReferencia()
    {
        var norma = Normas.FirstOrDefault(n => n.Id == "60598") ?? Normas.FirstOrDefault();
        return norma is null ? new PlantillaEnsayos() : PlantillaEnsayos.Cargar(norma.Ruta);
    }
}

/// <summary>
/// Casilla para añadir una norma al proyecto. Marcarla trae al árbol los apartados de
/// esa norma; desmarcarla los retira, pero <b>no borra lo ya anotado</b>: si se vuelve a
/// marcar, los datos siguen ahí.
/// </summary>
public sealed class NormaAnadibleViewModel(
    NormaDisponible norma, bool activa, Action<NormaDisponible, bool> alCambiar) : ObservableObject
{
    private bool _activa = activa;

    public NormaDisponible Norma { get; } = norma;
    public string Titulo => Norma.Titulo;

    public bool Activa
    {
        get => _activa;
        set
        {
            if (_activa == value) return;
            _activa = value;
            Notificar();
            alCambiar(Norma, value);
        }
    }
}

/// <summary>Qué decide el técnico ante un proyecto con cambios sin guardar.</summary>
public enum RespuestaCambios
{
    Guardar,
    Descartar,

    /// <summary>Cerró el aviso: se queda donde estaba y no se pierde nada.</summary>
    Cancelar
}

/// <summary>
/// Qué se hace cuando el servicio que se va a guardar ya existe en la carpeta.
/// </summary>
public enum RespuestaRepetido
{
    /// <summary>Abrir el que ya está, que casi siempre es lo que se quería.</summary>
    Abrir,

    /// <summary>A sabiendas: un reensayo o un servicio partido pueden repetir código.</summary>
    CrearIgualmente,

    Cancelar
}

/// <summary>Un aviso de la portada, con el botón que lo resuelve.</summary>
public sealed class AvisoViewModel(AvisoDeInicio aviso, Action<AccionDeAviso> atender)
{
    public string Texto => aviso.Texto;
    public string? Detalle => aviso.Detalle;
    public bool HayDetalle => !string.IsNullOrWhiteSpace(aviso.Detalle);
    public string Boton => aviso.Boton;

    public bool EsProblema => aviso.Nivel == NivelDeAviso.Problema;

    /// <summary>Rojo si algo no funciona, ámbar si solo está descuadrado.</summary>
    public string Color => EsProblema ? "#B91C1C" : "#92400E";
    public string Fondo => EsProblema ? "#FEE2E2" : "#FEF3C7";
    public string Borde => EsProblema ? "#FECACA" : "#FDE68A";

    public Comando Resolver { get; } = new(() => atender(aviso.Accion));
}

/// <summary>Entrada de la lista de proyectos recientes.</summary>
public sealed class ProyectoReciente(string ruta)
{
    public string Ruta { get; } = ruta;
    public string Nombre { get; } = Path.GetFileNameWithoutExtension(ruta);
    public string Carpeta { get; } = Path.GetDirectoryName(ruta) ?? "";
    public string Etiqueta => $"{Nombre}  |  {Carpeta}";
}
