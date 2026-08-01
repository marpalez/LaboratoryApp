using System.Collections.ObjectModel;
using System.IO;
using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Línea de tiempo de los servicios: una tarjeta por toma de notas, colocada sobre un
/// eje de semanas. Responde a la pregunta que no contesta el tablero —«¿qué entra esta
/// semana y qué se me ha pasado de plazo?»— sin abrir un proyecto tras otro.
/// <para>
/// La aritmética del eje está en <see cref="EjeDeSemanas"/>, dentro del núcleo, para
/// poder probarla. Aquí solo queda filtrar y traducir a píxeles.
/// </para>
/// </summary>
public sealed class CalendarioViewModel : ObservableObject
{
    /// <summary>Anchos de semana entre los que alterna el zoom, de apretado a amplio.</summary>
    private static readonly double[] Anchos = [26, 46, 78];

    private const string Todos = "(todos)";

    private IReadOnlyList<ResumenDeProyecto> _proyectos = [];
    private readonly Action<ResumenDeProyecto> _planificar;
    private readonly Action<string> _abrir;
    private readonly Action<ResumenDeProyecto, Planificacion> _guardar;

    /// <summary>Semanas vacías que se añaden cada vez que se pide ver más allá.</summary>
    private const int Paso = 8;

    private int _zoom = 1;
    private int _extraAntes;
    private int _extraDespues;
    private string _tecnico = Todos;
    private string _estado = Todos;
    private string _norma = Todos;
    private bool _verArchivados;
    private EjeDeSemanas _eje = EjeDeSemanas.Para([], DateTime.Today, Anchos[1]);

    public CalendarioViewModel(Action<ResumenDeProyecto> planificar, Action<string> abrir,
                               Action<ResumenDeProyecto, Planificacion> guardar)
    {
        _planificar = planificar;
        _abrir = abrir;
        _guardar = guardar;

        Acercar = new Comando(() => Zoom++, () => _zoom < Anchos.Length - 1);
        Alejar = new Comando(() => Zoom--, () => _zoom > 0);

        // El calendario no está atado a ningún año: se pide sitio hacia donde haga falta
        // y allí se planifica. Así se llega a 2027 o a 2030 sin dibujarlo todo de golpe.
        VerAntes = new Comando(() => { _extraAntes += Paso; Recalcular(); });
        VerDespues = new Comando(() => { _extraDespues += Paso; Recalcular(); });
        VolverAHoy = new Comando(() => { _extraAntes = _extraDespues = 0; Recalcular(); },
                                 () => _extraAntes > 0 || _extraDespues > 0);
    }

    public ObservableCollection<TarjetaPlanViewModel> Tarjetas { get; } = [];

    /// <summary>
    /// Servicios todavía sin fechas. Van en una lista aparte en vez de no dibujarse:
    /// un proyecto invisible es un proyecto que se olvida.
    /// </summary>
    public ObservableCollection<TarjetaPlanViewModel> SinFechas { get; } = [];

    public ObservableCollection<string> Tecnicos { get; } = [Todos];
    public ObservableCollection<string> Normas { get; } = [Todos];
    public IReadOnlyList<string> Estados { get; } = [Todos, .. Planificacion.Estados.Select(Planificacion.EtiquetaDe)];

    public Comando Acercar { get; }
    public Comando Alejar { get; }
    public Comando VerAntes { get; }
    public Comando VerDespues { get; }
    public Comando VolverAHoy { get; }

    /// <summary>Qué periodo se está enseñando, para saber dónde se está sin contar semanas.</summary>
    public string Periodo => $"{_eje.Desde:MMM yyyy} – {_eje.Hasta.AddDays(-1):MMM yyyy}";

    // ---- eje ---------------------------------------------------------------

    public IReadOnlyList<CeldaDeSemana> Semanas => _eje.Celdas;
    public IReadOnlyList<CeldaDeMes> Meses => _eje.Meses;
    public double Ancho => _eje.Ancho;
    public double AnchoSemana => _eje.AnchoSemana;
    public double PosicionDeHoy => _eje.PosicionDeHoy;
    public bool HoyVisible => _eje.HoyEstaDentro;

    public bool HayTarjetas => Tarjetas.Count > 0;
    public bool HaySinFechas => SinFechas.Count > 0;

    // ---- filtros -----------------------------------------------------------

    public string Tecnico
    {
        get => _tecnico;
        set { if (Establecer(ref _tecnico, value)) Recalcular(); }
    }

    public string Estado
    {
        get => _estado;
        set { if (Establecer(ref _estado, value)) Recalcular(); }
    }

    public string Norma
    {
        get => _norma;
        set { if (Establecer(ref _norma, value)) Recalcular(); }
    }

    /// <summary>Un proyecto archivado sigue en su carpeta; solo se ha quitado de la vista.</summary>
    public bool VerArchivados
    {
        get => _verArchivados;
        set { if (Establecer(ref _verArchivados, value)) Recalcular(); }
    }

    public int Zoom
    {
        get => _zoom;
        set
        {
            var acotado = Math.Clamp(value, 0, Anchos.Length - 1);
            if (!Establecer(ref _zoom, acotado)) return;
            Acercar.Revisar();
            Alejar.Revisar();
            Recalcular();
        }
    }

    // ---- carga -------------------------------------------------------------

    /// <summary>Recibe lo que ha encontrado el explorador y vuelve a dibujar.</summary>
    public void Cargar(IReadOnlyList<ResumenDeProyecto> proyectos)
    {
        _proyectos = proyectos;

        Rellenar(Tecnicos, proyectos.Select(p => p.Tecnico), ref _tecnico, nameof(Tecnico));
        Rellenar(Normas, proyectos.SelectMany(p => p.Normas), ref _norma, nameof(Norma));

        Recalcular(rehacerEje: false);
    }

    /// <summary>
    /// Rehace la lista de un desplegable con los valores que hay en los proyectos. Si el
    /// que estaba elegido ya no existe —se archivó el último servicio de ese técnico— se
    /// vuelve a «todos» en vez de dejar el calendario vacío sin explicar por qué.
    /// </summary>
    private void Rellenar(ObservableCollection<string> destino, IEnumerable<string> valores,
                          ref string elegido, string propiedad)
    {
        var lista = valores.Where(v => !string.IsNullOrWhiteSpace(v))
                           .Distinct(StringComparer.CurrentCultureIgnoreCase)
                           .OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase)
                           .ToList();

        destino.Clear();
        destino.Add(Todos);
        foreach (var valor in lista) destino.Add(valor);

        if (destino.Contains(elegido)) return;
        elegido = Todos;
        Notificar(propiedad);
    }

    /// <param name="rehacerEje">
    /// Reencuadrar la línea de tiempo. Verdadero cuando cambia lo que se está mirando
    /// —filtros, zoom—; falso cuando solo se han releído los ficheros.
    /// </param>
    private void Recalcular(bool rehacerEje = true)
    {
        var hoy = DateTime.Today;
        var visibles = _proyectos.Where(Pasa).ToList();

        var conFechas = visibles.Where(p => p.Planificacion.HayFechas).ToList();

        var necesario = EjeDeSemanas.Para(
            conFechas.Select(p => (p.Planificacion.Inicio!.Value, p.Planificacion.FinEfectivo!.Value)),
            hoy, Anchos[_zoom], extraAntes: _extraAntes, extraDespues: _extraDespues);

        // Al refrescar los datos, si el eje que ya había sigue valiendo, se conserva: al
        // soltar una barra el calendario no debe desplazarse bajo el ratón por haber
        // crecido dos semanas. Cambiar de filtro o de zoom sí lo reencuadra.
        if (rehacerEje || !_eje.Cubre(necesario)) _eje = necesario;

        // Un proyecto con una fecha disparatada queda fuera del eje. No se pierde: baja a
        // la banda de abajo, que es justo donde se ve que hay que corregirlo.
        bool Dibujable(ResumenDeProyecto p)
            => _eje.Contiene(p.Planificacion.Inicio!.Value, p.Planificacion.FinEfectivo!.Value);

        Tarjetas.Clear();
        foreach (var proyecto in conFechas.Where(Dibujable).OrderBy(p => p.Planificacion.Inicio))
            Tarjetas.Add(new TarjetaPlanViewModel(proyecto, _eje, hoy, _planificar, _abrir, _guardar));

        SinFechas.Clear();
        foreach (var proyecto in visibles.Where(p => !p.Planificacion.HayFechas || !Dibujable(p))
                                         .OrderBy(p => p.CodigoServicio))
            SinFechas.Add(new TarjetaPlanViewModel(proyecto, _eje, hoy, _planificar, _abrir, _guardar));

        Notificar(nameof(Semanas));
        Notificar(nameof(Meses));
        Notificar(nameof(Ancho));
        Notificar(nameof(AnchoSemana));
        Notificar(nameof(PosicionDeHoy));
        Notificar(nameof(HoyVisible));
        Notificar(nameof(HayTarjetas));
        Notificar(nameof(HaySinFechas));
        Notificar(nameof(Periodo));
        VolverAHoy.Revisar();
    }

    private bool Pasa(ResumenDeProyecto proyecto)
    {
        if (proyecto.Planificacion.Archivado && !_verArchivados) return false;

        if (_tecnico != Todos && !string.Equals(proyecto.Tecnico, _tecnico,
                StringComparison.CurrentCultureIgnoreCase)) return false;

        if (_norma != Todos && !proyecto.Normas.Contains(_norma)) return false;

        if (_estado != Todos &&
            Planificacion.EtiquetaDe(proyecto.Planificacion.Estado) != _estado) return false;

        return true;
    }
}

/// <summary>Una toma de notas dibujada sobre la línea de tiempo.</summary>
public sealed class TarjetaPlanViewModel : ObservableObject
{
    private readonly ResumenDeProyecto _proyecto;
    private readonly DateTime _hoy;
    private readonly Action<ResumenDeProyecto, Planificacion> _guardar;
    private readonly BarraDePlanificacion _barra;

    public TarjetaPlanViewModel(ResumenDeProyecto proyecto, EjeDeSemanas eje, DateTime hoy,
                                Action<ResumenDeProyecto> planificar, Action<string> abrir,
                                Action<ResumenDeProyecto, Planificacion> guardar)
    {
        _proyecto = proyecto;
        _hoy = hoy;
        _guardar = guardar;
        _barra = new BarraDePlanificacion(proyecto.Planificacion, eje);

        Planificar = new Comando(() => planificar(proyecto));
        Abrir = new Comando(() => abrir(proyecto.Ruta));
    }

    // ---- arrastre ----------------------------------------------------------
    // El gesto lo lleva BarraDePlanificacion, en el núcleo. Aquí solo se avisa a la
    // vista de que hay que repintar.

    public bool SePuedeArrastrar => _barra.SePuedeArrastrar;

    public void EmpezarArrastre(ModoArrastre modo) => _barra.Empezar(modo);

    public void Arrastrar(double pixeles)
    {
        _barra.Arrastrar(pixeles);
        Redibujar();
    }

    /// <summary>
    /// Suelta la barra. Si las fechas no han cambiado no se guarda nada: arrastrar y
    /// volver al sitio no debe tocar el fichero.
    /// </summary>
    public void SoltarArrastre()
    {
        if (_barra.HayCambio) _guardar(_proyecto, _barra.Resultado());
    }

    public void CancelarArrastre()
    {
        _barra.Cancelar();
        Redibujar();
    }

    private void Redibujar()
    {
        Notificar(nameof(Izquierda));
        Notificar(nameof(Ancho));
        Notificar(nameof(Fechas));
        Notificar(nameof(Detalle));
        Notificar(nameof(Retrasado));
        Notificar(nameof(Color));
    }

    public string Ruta => _proyecto.Ruta;

    /// <summary>El código del servicio, o el nombre del fichero si aún no lo tiene.</summary>
    public string Codigo => string.IsNullOrWhiteSpace(_proyecto.CodigoServicio)
        ? _proyecto.Nombre
        : _proyecto.CodigoServicio;

    public string Tecnico => _proyecto.Tecnico;
    public string Normas => string.Join(" + ", _proyecto.Normas);
    public string Avance => _proyecto.Avance;
    public string Carpeta => Path.GetDirectoryName(_proyecto.Ruta) ?? "";

    /// <summary>Se llama <c>Plan</c> y no <c>Planificacion</c> para no tapar al tipo.</summary>
    public Planificacion Plan => _proyecto.Planificacion;

    public bool Archivado => Plan.Archivado;

    public string EstadoTexto => Planificacion.EtiquetaDe(Plan.Estado);

    /// <summary>Rojo si se ha pasado de plazo; si no, el color de su estado.</summary>
    public string Color => Retrasado ? "#DC2626" : Planificacion.ColorDe(Plan.Estado);

    /// <summary>
    /// Fuera de plazo. Se calcula sobre la fecha que se esté enseñando, no sobre la
    /// guardada, para que la barra cambie de color mientras se arrastra.
    /// </summary>
    public bool Retrasado => Plan.Estado != EstadoDeProyecto.Terminado
                             && _barra.Fin is { } fin && fin.Date < _hoy.Date;

    public double Izquierda => _barra.Izquierda;
    public double Ancho => _barra.Ancho;

    public string Fechas => _barra.Inicio is { } inicio && _barra.Fin is { } fin
        ? $"{inicio:dd/MM} → {fin:dd/MM}  (S{System.Globalization.ISOWeek.GetWeekOfYear(inicio):00}" +
          $"–S{System.Globalization.ISOWeek.GetWeekOfYear(fin):00})"
        : "sin fechas";

    public string Muestras => Plan.RecepcionMuestras is { } fecha
        ? $"Muestras recibidas el {fecha:dd/MM/yyyy}"
        : "Muestras pendientes de recibir";

    public bool MuestrasRecibidas => Plan.MuestrasRecibidas;

    public string Detalle => string.Join("\n",
        new[]
        {
            Codigo + (string.IsNullOrWhiteSpace(Normas) ? "" : "  |  " + Normas),
            string.IsNullOrWhiteSpace(Tecnico) ? null : "Técnico: " + Tecnico,
            "Estado: " + EstadoTexto + (Retrasado ? "  ·  fuera de plazo" : ""),
            "Fechas: " + Fechas,
            Muestras,
            "Avance: " + Avance,
            Carpeta
        }.Where(l => l is not null));

    public Comando Planificar { get; }
    public Comando Abrir { get; }
}
