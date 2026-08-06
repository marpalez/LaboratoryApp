using System.Globalization;
using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;
using LumNotas.Storage;

namespace LumNotas.App.ViewModels;

/// <summary>
/// La planificación del servicio, dentro de la toma de notas: cuándo empieza, cuándo
/// acaba, si llegaron las muestras, en qué estado está y cuánto se ofertó.
/// <para>
/// <b>Está aquí para que el técnico no tenga que irse a gestión a mirarlo.</b> Es lo que
/// se pregunta con el ensayo delante: «¿para cuándo era esto?», «¿han llegado ya las
/// muestras?».
/// </para>
/// <para>
/// <b>No sale en el informe.</b> <c>ExportadorDeInforme</c> no conoce la planificación —
/// cero referencias—, y este panel no cambia eso: son fechas de gestión, no datos de
/// ensayo, y no tienen nada que hacer en un documento que se firma.
/// </para>
/// <para>
/// <b>Escribe con <see cref="RepositorioDeProyectos.ActualizarPlanificacion"/></b>, que
/// relee el fichero y cambia solo esa parte. Nunca con <c>Guardar</c>: así, cambiar el
/// estado desde aquí no pisa un dato de ensayo que esté escribiendo otro técnico, igual
/// que mover una fecha desde el calendario no pisa lo que hay abierto aquí (DD‑53).
/// </para>
/// </summary>
public sealed class PlanificacionViewModel : ObservableObject
{
    private readonly RepositorioDeProyectos _repositorio;
    private readonly Func<string?> _ruta;
    private readonly Func<string> _codigo;
    private readonly Func<string> _tecnico;
    private readonly Action<DestinoDelCalendario> _verEnCalendario;
    private readonly Func<string, Planificacion, Planificacion?> _pedirPlanificacion;
    private readonly Func<(DateTime? Desde, DateTime? Hasta)> _fechasDelEnsayo;

    private Planificacion _plan = new();

    public PlanificacionViewModel(RepositorioDeProyectos repositorio, Func<string?> ruta,
                                  Func<string> codigo, Func<string> tecnico,
                                  Action<DestinoDelCalendario> verEnCalendario,
                                  Func<string, Planificacion, Planificacion?> pedirPlanificacion,
                                  Func<(DateTime? Desde, DateTime? Hasta)> fechasDelEnsayo)
    {
        _repositorio = repositorio;
        _ruta = ruta;
        _codigo = codigo;
        _tecnico = tecnico;
        _verEnCalendario = verEnCalendario;
        _pedirPlanificacion = pedirPlanificacion;
        _fechasDelEnsayo = fechasDelEnsayo;

        VerEnCalendario = new Comando(() => _verEnCalendario(Destino()), () => HayCodigo);
        Planificar = new Comando(AbrirDialogo, () => HayProyecto);
    }

    /// <summary>
    /// Abre el mismo diálogo de planificación que el tablero. Las fechas, el importe y la
    /// agrupación se editan ahí y no aquí: dos editores de lo mismo acaban discrepando, y
    /// ese ya sabe de fechas.
    /// </summary>
    public Comando Planificar { get; }

    private void AbrirDialogo()
    {
        // El mismo rótulo que en el tablero y el calendario: es el mismo servicio, y que
        // la ventana se llame de otra forma hace dudar de si se edita lo mismo.
        if (_pedirPlanificacion(CodigoDeBusqueda, Copia()) is { } nueva) Aplicar(nueva);
    }

    /// <summary>Lo que se lee en el árbol y encabeza el panel.</summary>
    public string Titulo => "Planificación";

    /// <summary>
    /// Estado del apartado en el índice. Nunca pinta color: la planificación no es un
    /// ensayo que haya que completar, así que el punto se queda apagado.
    /// </summary>
    public EstadoApartado Estado => EstadoApartado.SinReglas;

    // ---- lo que hay guardado -----------------------------------------------

    /// <summary>
    /// Relee la planificación del fichero. Se llama al abrir y <b>cada vez que se entra al
    /// apartado</b>: el responsable puede haber movido las fechas desde el calendario
    /// mientras esta pestaña llevaba media hora abierta, y un panel que enseñe lo de hace
    /// media hora es peor que no tenerlo.
    /// </summary>
    public void Recargar()
    {
        _plan = _ruta() is { } ruta && !string.IsNullOrWhiteSpace(ruta)
            ? _repositorio.LeerPlanificacion(ruta)
            : new Planificacion();

        Refrescar();
    }

    private void Refrescar()
    {
        Notificar(nameof(HayProyecto));
        Notificar(nameof(SinProyecto));
        Notificar(nameof(EstadoElegido));
        Notificar(nameof(Inicio));
        Notificar(nameof(Fin));
        Notificar(nameof(RecepcionMuestras));
        Notificar(nameof(Grupo));
        Notificar(nameof(HayGrupo));
        Notificar(nameof(Importe));
        Notificar(nameof(Archivado));
        Notificar(nameof(Aviso));
        Notificar(nameof(HayAviso));
        Notificar(nameof(SinPlanificar));
        Notificar(nameof(CodigoDeBusqueda));
        Notificar(nameof(HayCodigo));
        VerEnCalendario.Revisar();
        Planificar.Revisar();
    }

    /// <summary>Sin fichero no hay planificación que leer ni sitio donde escribirla.</summary>
    public bool HayProyecto => !string.IsNullOrWhiteSpace(_ruta());

    /// <summary>Lo contrario, porque el convertidor de WPF a visibilidad no sabe invertir.</summary>
    public bool SinProyecto => !HayProyecto;

    /// <summary>Guardado pero sin planificar: se dice, en vez de enseñar seis huecos.</summary>
    public bool SinPlanificar => HayProyecto && _plan.EsVacia;

    public string Inicio => Fecha(_plan.Inicio);
    public string Fin => Fecha(_plan.Fin);
    public string RecepcionMuestras => _plan.RecepcionMuestras is { } fecha
        ? Fecha(fecha)
        : "Sin recibir";

    public string Grupo => _plan.Grupo?.Trim() ?? "";
    public bool HayGrupo => !string.IsNullOrWhiteSpace(Grupo);

    /// <summary>
    /// El importe de la oferta. Es dato comercial y no de ensayo, así que se enseña aquí
    /// pero no se exporta.
    /// </summary>
    public string Importe => _plan.Importe is { } importe
        ? importe.ToString("N2", CultureInfo.CurrentCulture) + " €"
        : "Sin importe";

    public string Archivado => _plan.Archivado ? "Sí" : "No";

    private static string Fecha(DateTime? fecha)
        => fecha is { } valor ? valor.ToString("dd/MM/yyyy") : "Sin fecha";

    /// <summary>
    /// Lo que hay que saber sin buscarlo: que se pasó la fecha de entrega, o que se está
    /// trabajando sobre unas muestras que todavía no han llegado.
    /// </summary>
    public string Aviso
    {
        get
        {
            if (!HayProyecto || _plan.EsVacia) return "";
            if (_plan.Archivado) return "Este servicio está archivado: no sale en el calendario.";
            if (_plan.Retrasado(DateTime.Today)) return "La fecha de entrega ya pasó y el servicio no está terminado.";
            if (_plan.HayFechas && !_plan.MuestrasRecibidas) return "Hay fechas puestas pero las muestras no constan como recibidas.";
            return "";
        }
    }

    public bool HayAviso => !string.IsNullOrWhiteSpace(Aviso);

    // ---- lo único que se edita aquí ----------------------------------------

    public IReadOnlyList<string> Estados { get; } =
        [.. Planificacion.Estados.Select(Planificacion.EtiquetaDe)];

    /// <summary>
    /// El estado del servicio, que es lo que cambia mientras se ensaya —«por hacer» pasa a
    /// «en curso» el día que se empieza—. Se escribe en el fichero <b>en cuanto se elige</b>:
    /// guardarlo para luego lo dejaría a merced de que el técnico se acuerde, y además
    /// pelearía con el calendario, que escribe el mismo sitio.
    /// <para>
    /// Las fechas y el importe no se editan aquí: para eso está «Planificar», que abre el
    /// mismo diálogo del tablero. Dos editores de lo mismo acaban discrepando.
    /// </para>
    /// </summary>
    public string EstadoElegido
    {
        get => Planificacion.EtiquetaDe(_plan.Estado);
        set
        {
            var elegido = Planificacion.Estados.FirstOrDefault(e => Planificacion.EtiquetaDe(e) == value);
            if (elegido == _plan.Estado || string.IsNullOrWhiteSpace(value)) return;

            Escribir(plan => plan.Estado = elegido);
        }
    }

    /// <summary>
    /// Cambia una cosa de la planificación y la escribe. Se trabaja sobre lo que hay
    /// <b>en el disco ahora mismo</b>, no sobre lo que se leyó al abrir la pestaña: entre
    /// una cosa y otra el responsable puede haber movido las fechas.
    /// </summary>
    private void Escribir(Action<Planificacion> cambio)
    {
        if (_ruta() is not { } ruta || string.IsNullOrWhiteSpace(ruta)) return;

        try
        {
            var plan = _repositorio.LeerPlanificacion(ruta);
            cambio(plan);
            SellarLasFechasDelEnsayo(plan);
            _repositorio.ActualizarPlanificacion(ruta, plan);
            _plan = plan;
            Mensaje = "";
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar la planificación: {ex.Message}";
        }

        Refrescar();
        Guardada?.Invoke();
    }

    /// <summary>
    /// Al dar el servicio por terminado se apunta cuándo se ensayó de verdad: la primera y
    /// la última fecha escritas en la toma de notas. De ahí sale el filtro por periodo de
    /// la BBDD, sin que el técnico teclee un dato más.
    /// <para>
    /// Se recalcula cada vez que se pasa a terminado, así que corregir una fecha del ensayo
    /// y volver a cerrarlo lo pone al día. Mientras no esté terminado se dejan en blanco:
    /// un ensayo a medias todavía no tiene un «cuándo se hizo».
    /// </para>
    /// </summary>
    private void SellarLasFechasDelEnsayo(Planificacion plan)
    {
        if (plan.Estado != EstadoDeProyecto.Terminado)
        {
            plan.EnsayoDesde = null;
            plan.EnsayoHasta = null;
            return;
        }

        var (desde, hasta) = _fechasDelEnsayo();
        plan.EnsayoDesde = desde;
        plan.EnsayoHasta = hasta;
    }

    /// <summary>Se avisa cuando algo se ha escrito, para que el tablero se entere.</summary>
    public event Action? Guardada;

    private string _mensaje = "";

    public string Mensaje
    {
        get => _mensaje;
        private set { if (Establecer(ref _mensaje, value)) Notificar(nameof(HayMensaje)); }
    }

    public bool HayMensaje => !string.IsNullOrWhiteSpace(Mensaje);

    // ---- ir a gestión -------------------------------------------------------

    /// <summary>
    /// Con lo que se busca en el calendario: las once primeras del código, que es
    /// justamente el rótulo con el que el calendario dibuja cada barra.
    /// </summary>
    public string CodigoDeBusqueda => CodigoDeServicio.ConFamilia(_codigo());

    public bool HayCodigo => !string.IsNullOrWhiteSpace(CodigoDeBusqueda);

    /// <summary>
    /// Con qué filtros hay que llegar al calendario para que este servicio <b>se vea</b>.
    /// <para>
    /// El estado solo se fuerza en los dos casos que esconden: <b>archivado</b> —que no sale
    /// con ninguna otra opción— y <b>terminado</b> —que el filtro de diario deja fuera—. En
    /// cualquier otro, el servicio ya pasa el filtro que hubiera puesto y no hay motivo para
    /// cambiárselo al técnico.
    /// </para>
    /// <para>
    /// El archivado manda sobre el terminado: un servicio archivado y terminado a la vez no
    /// pasa el filtro «Terminado», que excluye lo archivado a propósito.
    /// </para>
    /// </summary>
    private DestinoDelCalendario Destino() => new(
        CodigoDeBusqueda,
        _plan.Archivado
            ? FiltroDeEstado.Archivados
            : _plan.Estado == EstadoDeProyecto.Terminado
                ? Planificacion.EtiquetaDe(EstadoDeProyecto.Terminado)
                : null,
        _tecnico());

    public Comando VerEnCalendario { get; }

    /// <summary>
    /// Aplica una planificación entera, la que devuelve el diálogo de «Planificar».
    /// </summary>
    public void Aplicar(Planificacion nueva)
        => Escribir(plan =>
        {
            plan.Inicio = nueva.Inicio;
            plan.Fin = nueva.Fin;
            plan.Estado = nueva.Estado;
            plan.RecepcionMuestras = nueva.RecepcionMuestras;
            plan.Archivado = nueva.Archivado;
            plan.Importe = nueva.Importe;
            plan.Grupo = nueva.Grupo;
        });

    /// <summary>Lo que hay ahora, para llevárselo al diálogo sin dejar que lo toque.</summary>
    public Planificacion Copia() => _plan.Copia();

    /// <summary>Deja el servicio terminado o archivado. Lo usa el diálogo de la exportación.</summary>
    public void Cerrar(bool archivar)
        => Escribir(plan =>
        {
            plan.Estado = EstadoDeProyecto.Terminado;
            if (archivar) plan.Archivado = true;
        });

    /// <summary>Si ya está cerrado, no hay nada que preguntar al exportar.</summary>
    public bool YaEstaCerrado => _plan.Archivado || _plan.Estado == EstadoDeProyecto.Terminado;
}
