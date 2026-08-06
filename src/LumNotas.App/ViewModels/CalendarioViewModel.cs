using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Guarda todo lo que ha cambiado <b>un mismo gesto</b>. Arrastrar un trabajo enlazado mueve
/// varias familias a la vez, y entregarlas de una en una haría que la cadena del grupo se
/// recolocara contra datos a medio guardar (DD‑123).
/// </summary>
public delegate void CambiosDePlanificacion(
    IReadOnlyList<(ResumenDeProyecto Proyecto, Planificacion Plan)> cambios);

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
    private readonly CambiosDePlanificacion _guardar;

    /// <summary>Semanas vacías que se añaden cada vez que se pide ver más allá.</summary>
    private const int Paso = 8;

    private int _zoom = 1;
    private int _extraAntes;
    private int _extraDespues;
    private bool _agrupar = true;
    private EjeDeSemanas _eje = EjeDeSemanas.Para([], DateTime.Today, Anchos[1]);

    public CalendarioViewModel(Action<ResumenDeProyecto> planificar, Action<string> abrir,
                               CambiosDePlanificacion guardar)
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

    public ColeccionEnBloque<TarjetaPlanViewModel> Tarjetas { get; } = [];

    /// <summary>
    /// Lo que se dibuja, fila a fila: cabeceras de técnico y carriles de barras. Las dos
    /// columnas del calendario —nombres y barras— recorren esta misma lista, y por eso
    /// van alineadas.
    /// <para>
    /// Una fila ya <b>no es un trabajo</b>, sino un carril donde caben todos los que no se
    /// pisan entre sí. Ver <see cref="CarrilViewModel"/>.
    /// </para>
    /// </summary>
    public ColeccionEnBloque<object> Filas { get; } = [];

    /// <summary>
    /// Servicios todavía sin fechas. Van en una lista aparte en vez de no dibujarse:
    /// un proyecto invisible es un proyecto que se olvida.
    /// </summary>
    public ColeccionEnBloque<TarjetaPlanViewModel> SinFechas { get; } = [];

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

    // ---- ajustes de la vista -----------------------------------------------

    /// <summary>
    /// Agrupar los servicios por técnico responsable. Es la vista que pidió el
    /// laboratorio: al responsable no le interesa una lista de servicios —el código ya
    /// va escrito en la barra— sino <b>cuántos lleva cada técnico y cuánto tiempo le
    /// ocupan</b>.
    /// </summary>
    public bool AgruparPorTecnico
    {
        get => _agrupar;
        set
        {
            if (!Establecer(ref _agrupar, value)) return;

            Notificar(nameof(AnchoDeNombres));
            Recalcular(rehacerEje: false);
        }
    }

    /// <summary>
    /// Lo que ocupa la columna de la izquierda. <b>Sin agrupar por técnico no hay nada que
    /// escribir en ella</b>: desde que las filas son carriles compartidos, lo único que
    /// lleva son las cabeceras, y el código de cada trabajo va dentro de su propia barra.
    /// Dejarla vacía serían 230 píxeles de calendario tirados.
    /// </summary>
    public double AnchoDeNombres => AgruparPorTecnico ? 230 : 0;

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

    /// <summary>
    /// Recibe los proyectos que ha dejado pasar el filtro del tablero y vuelve a dibujar.
    /// </summary>
    /// <param name="reencuadrar">
    /// Si el eje debe reajustarse. Al cambiar de filtro sí, porque se está mirando otra
    /// cosa; al refrescar los datos no, para que no se mueva bajo el ratón.
    /// </param>
    public void Cargar(IReadOnlyList<ResumenDeProyecto> proyectos, bool reencuadrar = false)
    {
        _proyectos = proyectos;
        Recalcular(reencuadrar);
    }

    /// <param name="rehacerEje">
    /// Reencuadrar la línea de tiempo. Verdadero cuando cambia lo que se está mirando
    /// —filtros, zoom—; falso cuando solo se han releído los ficheros.
    /// </param>
    private void Recalcular(bool rehacerEje = true)
    {
        var hoy = DateTime.Today;

        // El filtro —estado, técnico y norma— lo aplica el tablero antes de llamar aquí:
        // uno solo para las tres vistas.
        // Las tomas de notas enlazadas —las familias de un mismo servicio— se juntan en
        // una sola línea: el jefe planifica un trabajo, no cuatro. Manda la cabecera,
        // que es la que lleva las fechas.
        var visibles = EnlaceDeTomasDeNotas.Agrupar(_proyectos);

        // Las fechas que encuadran el eje son las del **trabajo entero**, no las de su
        // cabecera. Con las de la cabecera, un trabajo de cuatro familias estiraba el
        // calendario solo lo que ocupaba la primera: al arrastrarlo hasta el borde no se
        // dibujaba el año siguiente y no había dónde soltarlo.
        var conFechas = visibles.Where(e => e.Inicio is not null && e.Fin is not null).ToList();

        var necesario = EjeDeSemanas.Para(
            conFechas.Select(e => (e.Inicio!.Value, e.Fin!.Value)),
            hoy, Anchos[_zoom], extraAntes: _extraAntes, extraDespues: _extraDespues);

        // Al refrescar los datos, si el eje que ya había sigue valiendo, se conserva: al
        // soltar una barra el calendario no debe desplazarse bajo el ratón por haber
        // crecido dos semanas. Cambiar de filtro o de zoom sí lo reencuadra.
        if (rehacerEje || !_eje.Cubre(necesario)) _eje = necesario;

        // Un proyecto con una fecha disparatada queda fuera del eje. No se pierde: baja a
        // la banda de abajo, que es justo donde se ve que hay que corregirlo.
        bool Dibujable(EntradaDeCalendario e)
            => e.Inicio is { } inicio && e.Fin is { } fin && _eje.Contiene(inicio, fin);

        Tarjetas.Reemplazar(conFechas.Where(Dibujable)
                                     .OrderBy(e => e.Inicio)
                                     .Select(e => new TarjetaPlanViewModel(e, _eje, hoy, _planificar, _abrir, _guardar)));

        RehacerFilas();

        SinFechas.Reemplazar(visibles.Where(e => !Dibujable(e))
                                     .OrderBy(e => e.Cabecera.CodigoServicio)
                                     .Select(e => new TarjetaPlanViewModel(e, _eje, hoy, _planificar, _abrir, _guardar)));

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

    /// <summary>
    /// Ordena las barras en filas. Agrupadas, cada técnico abre con una cabecera que dice
    /// cuántos servicios lleva; debajo van sus carriles.
    /// <para>
    /// <b>Los carriles son por técnico</b>, no del calendario entero: si se repartieran
    /// todos juntos, dos técnicos que trabajan las mismas semanas compartirían fila y la
    /// cabecera dejaría de decir de quién es lo que hay debajo.
    /// </para>
    /// </summary>
    private void RehacerFilas()
    {
        if (!AgruparPorTecnico)
        {
            Filas.Reemplazar(Repartir(Tarjetas));
            return;
        }

        var grupos = Tarjetas
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Tecnico) ? SinTecnico : t.Tecnico.Trim(),
                     StringComparer.CurrentCultureIgnoreCase)
            // Los servicios sin responsable al final: son los que hay que asignar.
            .OrderBy(g => g.Key == SinTecnico)
            .ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase);

        // Se arma la lista entera y se entrega de una vez: plegar un técnico rehace todas
        // las filas, y con veinte técnicos eso eran cientos de avisos por un solo clic.
        var filas = new List<object>();

        foreach (var grupo in grupos)
        {
            var suyos = grupo.OrderBy(t => t.Inicio).ToList();
            var desplegado = !_plegados.Contains(grupo.Key);

            filas.Add(new GrupoDeTecnicoViewModel(grupo.Key, suyos, desplegado, AlPlegarUnTecnico));
            if (desplegado) filas.AddRange(Repartir(suyos));
        }

        Filas.Reemplazar(filas);
    }

    /// <summary>
    /// Coloca unas tarjetas en carriles. El reparto está en el núcleo —<see
    /// cref="CarrilesDelCalendario"/>—, que es donde se puede probar; aquí solo se le dice
    /// de dónde a dónde ocupa cada trabajo.
    /// <para>
    /// Se miran las fechas que se están <b>enseñando</b>, no las guardadas, y nunca son
    /// nulas: a esta lista solo llega lo que se puede dibujar sobre el eje.
    /// </para>
    /// </summary>
    private static IEnumerable<CarrilViewModel> Repartir(IEnumerable<TarjetaPlanViewModel> tarjetas)
        => CarrilesDelCalendario
            .Repartir(tarjetas, t => (t.Inicio!.Value, t.Fin!.Value))
            .Select(carril => new CarrilViewModel(carril));

    /// <summary>
    /// Técnicos plegados. Se recuerda por nombre y no por objeto: las cabeceras se rehacen
    /// en cada escaneo, y con referencias se volvería a desplegar todo cada dos minutos.
    /// </summary>
    private readonly HashSet<string> _plegados = new(StringComparer.CurrentCultureIgnoreCase);

    private void AlPlegarUnTecnico(GrupoDeTecnicoViewModel grupo)
    {
        if (grupo.Desplegado) _plegados.Remove(grupo.Tecnico);
        else _plegados.Add(grupo.Tecnico);

        RehacerFilas();
    }

    private bool _sinFechasDesplegado = true;

    /// <summary>
    /// Si se ve la lista de servicios sin fechas. Empieza desplegada —lo que no está
    /// planificado es justo lo que hay que planificar— pero se puede cerrar para recuperar
    /// la mitad de la pantalla cuando hay muchos.
    /// </summary>
    public bool SinFechasDesplegado
    {
        get => _sinFechasDesplegado;
        set { if (Establecer(ref _sinFechasDesplegado, value)) Notificar(nameof(MarcaSinFechas)); }
    }

    public string MarcaSinFechas => SinFechasDesplegado ? "▾" : "▸";

    public Comando AlternarSinFechas => _alternarSinFechas ??=
        new Comando(() => SinFechasDesplegado = !SinFechasDesplegado);

    private Comando? _alternarSinFechas;

    /// <summary>
    /// Etiqueta de los servicios que aún no tienen responsable asignado. Es la misma que
    /// usan la carga y el filtro del tablero: si cada vista se inventara la suya, filtrar
    /// por «(sin técnico)» dejaría de casar con lo que enseña el calendario.
    /// </summary>
    public const string SinTecnico = CargaPorTecnico.SinTecnico;

}

/// <summary>
/// Una fila de la línea de tiempo con <b>todos los trabajos que caben en ella</b> sin
/// pisarse.
/// <para>
/// Es lo que sustituye a «una fila por trabajo». Un técnico con veinte proyectos seguidos
/// gastaba veinte renglones para dibujar veinte barras que nunca coinciden; ahora esos
/// veinte van en uno solo y <b>bajar una fila significa que dos trabajos se solapan</b>.
/// </para>
/// <para>
/// La lista no se toca después de construirla: rehacer los carriles a mitad de un arrastre
/// destruiría la tarjeta que tiene cogida el ratón. Se recolocan al soltar, cuando el
/// calendario se vuelve a dibujar entero.
/// </para>
/// </summary>
public sealed class CarrilViewModel(IReadOnlyList<TarjetaPlanViewModel> tarjetas)
{
    public IReadOnlyList<TarjetaPlanViewModel> Tarjetas { get; } = tarjetas;
}

/// <summary>
/// Cabecera de un técnico en la línea de tiempo: quién es, cuántos lleva y si se puede
/// plegar.
/// <para>
/// <b>Plegable porque con veinte proyectos no cabe nada.</b> Un responsable mira de uno
/// en uno; los demás solo estorban. Lo plegado se recuerda mientras dure la sesión, así
/// que refrescar el escaneo no vuelve a desplegar lo que se acababa de cerrar.
/// </para>
/// </summary>
public sealed class GrupoDeTecnicoViewModel : ObservableObject
{
    private readonly Action<GrupoDeTecnicoViewModel> _alPlegar;
    private bool _desplegado;

    public GrupoDeTecnicoViewModel(string tecnico, IReadOnlyList<TarjetaPlanViewModel> servicios,
                                   bool desplegado, Action<GrupoDeTecnicoViewModel> alPlegar)
    {
        Tecnico = tecnico;
        _desplegado = desplegado;
        _alPlegar = alPlegar;

        // El número de proyectos va pegado al nombre, en la misma línea. Las semanas que
        // ocupaba se quitaron: gastaban un renglón por técnico —y con veinte proyectos eso
        // son veinte renglones— para un dato que no se mira, porque el tiempo ya se ve
        // dibujado a la derecha.
        Proyectos = servicios.Count == 1 ? "1 proyecto" : $"{servicios.Count} proyectos";
        Retrasados = servicios.Count(s => s.Retrasado);

        Alternar = new Comando(() => Desplegado = !Desplegado);
    }

    public string Tecnico { get; }

    /// <summary>«3 proyectos». Va a continuación del nombre, no debajo.</summary>
    public string Proyectos { get; }

    public int Retrasados { get; }

    public bool HayRetrasados => Retrasados > 0;

    public string AvisoDeRetraso => Retrasados == 1 ? "1 fuera de plazo" : $"{Retrasados} fuera de plazo";

    /// <summary>Si se ven sus proyectos. Al cambiarlo, el calendario rehace las filas.</summary>
    public bool Desplegado
    {
        get => _desplegado;
        set
        {
            if (!Establecer(ref _desplegado, value)) return;
            Notificar(nameof(Marca));
            _alPlegar(this);
        }
    }

    /// <summary>El triángulo de plegar, que es lo que dice si hay algo escondido.</summary>
    public string Marca => Desplegado ? "▾" : "▸";

    public Comando Alternar { get; }
}

/// <summary>
/// Una tarjeta de la barra: la que le toca a <b>una familia</b> del trabajo.
/// <para>
/// Antes esto era decoración pintada encima de una barra única, y por eso el trabajo
/// entero salía del color de su cabecera y se abría siempre la misma toma de notas. Ahora
/// cada familia es una tarjeta de verdad —su color, su consejo emergente y su
/// planificación—, pegadas unas a otras porque el trabajo es uno solo.
/// </para>
/// <para>
/// <b>Estas tarjetas no se rehacen</b> mientras se arrastra el trabajo: solo se les
/// recalcula el tramo. Si se sustituyeran, WPF destruiría el elemento que tiene cogido el
/// ratón y el arrastre se cancelaría solo a mitad del gesto.
/// </para>
/// </summary>
public sealed class TrozoDeBarraViewModel : ObservableObject
{
    private readonly EntradaDeCalendario _entrada;
    private readonly DateTime _hoy;
    private TramoDelGrupo _tramo;
    private double _ancho;

    public TrozoDeBarraViewModel(TarjetaPlanViewModel tarjeta, EntradaDeCalendario entrada,
                                 TramoDelGrupo tramo, double anchoDeLaBarra, DateTime hoy,
                                 bool esPrimera, bool esUltima,
                                 Action<ResumenDeProyecto> planificar, Action<string> abrir)
    {
        Tarjeta = tarjeta;
        _entrada = entrada;
        _tramo = tramo;
        _hoy = hoy;
        _ancho = AnchoDe(tramo, anchoDeLaBarra);
        EsPrimera = esPrimera;
        EsUltima = esUltima;

        Planificar = new Comando(() => planificar(Miembro));
        Abrir = new Comando(() => abrir(Miembro.Ruta));
    }

    /// <summary>
    /// El trabajo al que pertenece. El arrastre se lo pide a la tarjeta que se coge, porque
    /// mover una familia mueve el trabajo entero.
    /// </summary>
    public TarjetaPlanViewModel Tarjeta { get; }

    public ResumenDeProyecto Miembro => _tramo.Miembro;

    /// <summary>Si es la de más a la izquierda, cuyo borde exterior fija el inicio del trabajo.</summary>
    public bool EsPrimera { get; }

    /// <summary>Y la de más a la derecha, cuyo borde exterior fija el fin.</summary>
    public bool EsUltima { get; }

    /// <summary>Lo que ocupa en píxeles, que es lo que necesita la plantilla.</summary>
    public double Ancho
    {
        get => _ancho;
        private set => Establecer(ref _ancho, value);
    }

    /// <summary>
    /// Redondeadas solo por fuera: entre dos familias las esquinas van a escuadra para que
    /// se lean como un tren y no como una fila de pastillas sueltas.
    /// </summary>
    public CornerRadius Esquinas => new(EsPrimera ? 7 : 0, EsUltima ? 7 : 0,
                                        EsUltima ? 7 : 0, EsPrimera ? 7 : 0);

    public string Codigo => Miembro.Rotulo;

    public bool MuestrasRecibidas => Miembro.Planificacion.MuestrasRecibidas;

    /// <summary>
    /// Fechas blindadas. Lo dibuja un candado y lo respeta el arrastre: coger esta tarjeta
    /// no mueve nada.
    /// </summary>
    public bool FechasBloqueadas => Miembro.Planificacion.FechasBloqueadas;

    /// <summary>
    /// Fuera de plazo. Se mira contra el fin que <b>se está dibujando</b>, no contra el
    /// guardado, para que la tarjeta cambie de color mientras se arrastra.
    /// </summary>
    public bool Retrasado => Miembro.Planificacion.Estado != EstadoDeProyecto.Terminado
                             && _tramo.Hasta.Date < _hoy.Date;

    /// <summary>
    /// El de <b>su</b> estado, no el del trabajo. Con una barra única, un trabajo con la
    /// primera familia terminada y la segunda en curso salía todo de un color.
    /// </summary>
    public string Color => Retrasado ? "#DC2626" : Planificacion.ColorDe(Miembro.Planificacion.Estado);

    public string Detalle => string.Join("\n",
        new[]
        {
            // Con varias enlazadas hay que decir de qué trabajo es esta tarjeta.
            !_entrada.EsGrupo ? null
                : $"Grupo «{_entrada.Grupo}» | {_entrada.Miembros.Count} tomas de notas",
            Codigo + (Miembro.Normas.Count == 0 ? "" : "  |  " + string.Join(" + ", Miembro.Normas)),
            string.IsNullOrWhiteSpace(Miembro.Tecnico) ? null : "Técnico: " + Miembro.Tecnico,
            "Estado: " + Planificacion.EtiquetaDe(Miembro.Planificacion.Estado)
                       + (Retrasado ? "  |  fuera de plazo" : ""),
            $"Fechas: {_tramo.Desde:dd/MM} → {_tramo.Hasta:dd/MM}",
            // Sin fechas propias el tramo es un reparto, no un dato: hay que decirlo o la
            // tarjeta parecería planificada cuando nadie la ha planificado.
            Miembro.Planificacion.HayFechas ? null : "Sin fechas propias: se reparte el tramo del trabajo",
            Miembro.Planificacion.RecepcionMuestras is { } fecha
                ? $"Muestras recibidas el {fecha:dd/MM/yyyy}"
                : "Muestras pendientes de recibir",
            "Avance: " + Miembro.Avance,
            Path.GetDirectoryName(Miembro.Ruta) ?? ""
        }.Where(l => l is not null));

    /// <summary>Abre la planificación <b>de esta familia</b>, no la de la cabecera.</summary>
    public Comando Planificar { get; }

    /// <summary>
    /// Abre <b>esta</b> toma de notas. Cuelga del menú contextual de la barra desde que las
    /// filas son carriles compartidos: la columna de la izquierda ya no puede llevar un
    /// nombre por trabajo, y con ella se iba la única forma de abrirlo desde la línea de
    /// tiempo. Ahora abre además la familia que se pulsa, no la cabecera del grupo.
    /// </summary>
    public Comando Abrir { get; }

    /// <summary>
    /// Vuelve a colocarse con el tramo nuevo. Se llama en cada latido del arrastre, así que
    /// se cambia el objeto por dentro en vez de crear otro.
    /// </summary>
    public void Redibujar(TramoDelGrupo tramo, double anchoDeLaBarra)
    {
        _tramo = tramo;
        Ancho = AnchoDe(tramo, anchoDeLaBarra);

        Notificar(nameof(Retrasado));
        Notificar(nameof(Color));
        Notificar(nameof(Detalle));
    }

    private static double AnchoDe(TramoDelGrupo tramo, double anchoDeLaBarra)
        => Math.Max(0, anchoDeLaBarra * tramo.Fraccion);
}

/// <summary>Una toma de notas dibujada sobre la línea de tiempo.</summary>
public sealed class TarjetaPlanViewModel : ObservableObject
{
    private readonly ResumenDeProyecto _proyecto;
    private readonly DateTime _hoy;
    private readonly CambiosDePlanificacion _guardar;
    private readonly BarraDePlanificacion _barra;

    /// <param name="entrada">
    /// La toma de notas que se dibuja y, si está enlazada, las que van con ella. El trabajo
    /// se arrastra entero; cada familia se planifica por su cuenta desde su propia tarjeta.
    /// </param>
    public TarjetaPlanViewModel(EntradaDeCalendario entrada, EjeDeSemanas eje, DateTime hoy,
                                Action<ResumenDeProyecto> planificar, Action<string> abrir,
                                CambiosDePlanificacion guardar)
    {
        _entrada = entrada;
        _proyecto = entrada.Cabecera;
        _hoy = hoy;
        _guardar = guardar;

        // La barra abarca el trabajo entero —de la primera familia a la última—, no solo
        // las fechas de la cabecera. Se le da al gesto una planificación con ese tramo; al
        // soltar, RepartoDelArrastre decide a qué familias hay que escribírselo.
        var delGrupo = _proyecto.Planificacion.Copia();
        delGrupo.Inicio = entrada.Inicio;
        delGrupo.Fin = entrada.Fin;
        _barra = new BarraDePlanificacion(delGrupo, eje);

        var tramos = entrada.Tramos;
        Trozos = [.. tramos.Select((t, i) => new TrozoDeBarraViewModel(
            this, entrada, t, _barra.Ancho, hoy,
            esPrimera: i == 0, esUltima: i == tramos.Count - 1, planificar, abrir))];

        Planificar = new Comando(() => planificar(_proyecto));
        Abrir = new Comando(() => abrir(_proyecto.Ruta));
    }

    private readonly EntradaDeCalendario _entrada;

    // ---- arrastre ----------------------------------------------------------
    // El gesto lo lleva BarraDePlanificacion, en el núcleo. Aquí solo se avisa a la
    // vista de que hay que repintar.

    /// <summary>
    /// Si el trabajo se puede mover con el ratón.
    /// <para>
    /// <b>Basta con que una familia tenga las fechas bloqueadas</b> para que no se pueda
    /// arrastrar ninguna: coger cualquier tarjeta mueve el trabajo entero, así que
    /// permitirlo movería también la bloqueada. Para desbloquearla se entra por su
    /// diálogo, que es donde se puso el candado.
    /// </para>
    /// </summary>
    public bool SePuedeArrastrar
        => _barra.SePuedeArrastrar && !_entrada.Miembros.Any(m => m.Planificacion.FechasBloqueadas);

    public void EmpezarArrastre(ModoArrastre modo) => _barra.Empezar(modo);

    public void Arrastrar(double pixeles)
    {
        _barra.Arrastrar(pixeles);
        Redibujar();
    }

    /// <summary>
    /// Suelta la barra. Si las fechas no han cambiado no se guarda nada: arrastrar y
    /// volver al sitio no debe tocar el fichero.
    /// <para>
    /// Se entrega <b>el gesto entero de una vez</b>, no familia a familia: quien guarda
    /// tiene que poder recolocar la cadena contra los datos ya completos y no a medias.
    /// </para>
    /// </summary>
    public void SoltarArrastre()
    {
        if (!_barra.HayCambio) return;

        _guardar(RepartoDelArrastre.Aplicar(_entrada, _barra.Inicio, _barra.Fin));
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
        Notificar(nameof(Retrasado));

        // Las tarjetas se recalculan, no se rehacen: la que tiene cogido el ratón tiene que
        // seguir existiendo hasta que se suelte.
        var tramos = TramosDelGrupo.Calcular(_entrada.EnOrden, _barra.Inicio, _barra.Fin);

        for (var i = 0; i < Trozos.Count && i < tramos.Count; i++)
            Trozos[i].Redibujar(tramos[i], Ancho);
    }

    public string Ruta => _proyecto.Ruta;

    /// <summary>
    /// Cómo se encabeza la barra: el mismo rótulo que en el tablero, para que un servicio
    /// se llame igual en las dos vistas.
    /// </summary>
    public string Codigo => _proyecto.Rotulo;

    /// <summary>
    /// Cómo se encabeza la <b>fila</b> del calendario. Con varias familias enlazadas es el
    /// nombre del grupo —tal como se tecleó— seguido de <c>(agrupación)</c>: la fila ya no
    /// es una toma de notas, son todas, y poner el código de una sola —la cabecera— hacía
    /// pensar que las demás no estaban ahí. Cada familia lleva el suyo escrito en su propia
    /// tarjeta.
    /// <para>
    /// La coletilla hace falta porque el nombre del grupo lo pone una persona y puede
    /// parecerse a cualquier cosa: sin ella, una fila que dice «ANTAR2504» no se distingue
    /// de una toma de notas suelta que se llamara así.
    /// </para>
    /// </summary>
    public string Titulo => _entrada.EsGrupo && !string.IsNullOrWhiteSpace(_entrada.Grupo)
        ? $"{_entrada.Grupo!.Trim()} {MarcaDeGrupo}"
        : Codigo;

    /// <summary>Lo que distingue una fila de varias familias de una toma de notas suelta.</summary>
    public const string MarcaDeGrupo = "(agrupación)";

    public string Tecnico => _proyecto.Tecnico;

    /// <summary>
    /// Las tarjetas del trabajo, <b>una por familia</b> y pegadas unas a otras. Una toma de
    /// notas suelta da una sola que ocupa la línea entera, así que la plantilla no distingue
    /// casos.
    /// <para>
    /// La lista se construye una vez y <b>no se sustituye nunca</b>: rehacerla a mitad de un
    /// arrastre destruiría la tarjeta que tiene cogido el ratón, WPF daría el gesto por
    /// perdido y el trabajo volvería solo a su sitio.
    /// </para>
    /// </summary>
    public IReadOnlyList<TrozoDeBarraViewModel> Trozos { get; }

    /// <summary>Se llama <c>Plan</c> y no <c>Planificacion</c> para no tapar al tipo.</summary>
    public Planificacion Plan => _proyecto.Planificacion;

    /// <summary>Fechas que se están enseñando, que durante el arrastre no son las guardadas.</summary>
    public DateTime? Inicio => _barra.Inicio;
    public DateTime? Fin => _barra.Fin;

    public bool Archivado => Plan.Archivado;

    /// <summary>
    /// Fuera de plazo. Es del <b>trabajo entero</b> —lo usa la cabecera del técnico para
    /// contar cuántos van tarde—; el color de cada tarjeta lo decide su propia familia.
    /// Se calcula sobre la fecha que se esté enseñando, no sobre la guardada.
    /// </summary>
    public bool Retrasado => Plan.Estado != EstadoDeProyecto.Terminado
                             && _barra.Fin is { } fin && fin.Date < _hoy.Date;

    public double Izquierda => _barra.Izquierda;
    public double Ancho => _barra.Ancho;

    public bool MuestrasRecibidas => Plan.MuestrasRecibidas;

    /// <summary>
    /// Planifica la cabecera. Solo lo usa la banda de los que están sin fechas, donde no
    /// hay tarjetas por familia que pulsar; sobre la línea de tiempo se planifica cada una
    /// desde la suya.
    /// </summary>
    public Comando Planificar { get; }
    public Comando Abrir { get; }
}
