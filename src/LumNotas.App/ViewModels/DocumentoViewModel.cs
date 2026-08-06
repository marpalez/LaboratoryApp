using System.Collections.ObjectModel;
using System.IO;
using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;
using LumNotas.Report;
using LumNotas.Storage;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Diálogos y avisos que necesita un documento y que solo sabe resolver la ventana.
/// Se comparten entre todas las pestañas para no atar el modelo a WPF.
/// </summary>
public sealed class ServiciosDeVentana
{
    public Func<string?>? PedirFicheroParaAbrir { get; set; }
    public Func<string, string?>? PedirFicheroParaGuardar { get; set; }
    public Func<string, string?>? PedirFicheroParaInforme { get; set; }
    public Func<RespuestaCambios>? ConfirmarDescartarCambios { get; set; }
    public Action<string>? AbrirEnElVisor { get; set; }

    /// <summary>
    /// Decir que algo <b>no</b> ha salido: no se pudo abrir, guardar o exportar, o falta
    /// algo para poder guardar.
    /// <para>
    /// <b>Interrumpe a propósito</b> (2026‑08‑06). Antes esto se escribía en una franja
    /// oscura al pie de la ventana, que se quitó por ocupar sitio permanente para no decir
    /// nada casi siempre. Pero un «no se pudo guardar» no puede quedarse sin decir, y una
    /// franja que casi siempre está vacía es justo la que nadie mira el día que importa.
    /// Con una ventana no hay forma de pasarlo por alto.
    /// </para>
    /// <para>
    /// Solo para lo que falla. <b>Las confirmaciones se quitaron</b> —«Guardado en…»,
    /// «Abierto…»— porque ya se ven por otro lado: la ruta está bajo el título y el punto
    /// de cambios sin guardar desaparece solo. Un diálogo cada vez que se guarda sería un
    /// estorbo en un trabajo donde se guarda cada pocos minutos.
    /// </para>
    /// </summary>
    public Action<string>? Avisar { get; set; }

    /// <summary>
    /// Editor de la lista de técnicos. Devuelve las correcciones de nombre hechas, o
    /// <c>null</c> si no se tocó nada.
    /// </summary>
    public Func<IReadOnlyList<(string Viejo, string Nuevo)>?>? EditarTecnicos { get; set; }

    /// <summary>Editor de la tarifa y la capacidad mensual. Devuelve si se ha guardado.</summary>
    public Func<bool>? EditarCapacidad { get; set; }

    /// <summary>Qué normas hay instaladas y de dónde salen.</summary>
    public Action? VerPlantillas { get; set; }

    /// <summary>Versión del programa y de las normas, y publicarla como la del laboratorio.</summary>
    public Action? VerAcercaDe { get; set; }

    /// <summary>Elegir la carpeta compartida del laboratorio.</summary>
    public Action? ElegirCarpetaDelLaboratorio { get; set; }

    /// <summary>A quién avisar cuando algo falla, y con qué datos.</summary>
    public Action? ReportarProblema { get; set; }

    /// <summary>
    /// Alta de un proyecto para planificarlo. Recibe la carpeta donde abrir el examinador
    /// y devuelve la ruta del <c>.lmnlab</c> creado, o <c>null</c> si se canceló.
    /// </summary>
    public Func<string?, string?>? CrearProyecto { get; set; }

    /// <summary>
    /// Comprueba si ese código de servicio ya existe en la carpeta del laboratorio y, si
    /// lo hay, pregunta qué hacer. Devuelve <c>null</c> cuando no hay con qué chocar.
    /// </summary>
    /// <remarks>
    /// Si se elige abrir el que ya existe, <b>lo abre la ventana</b>: es quien lleva las
    /// pestañas, y así la del técnico se queda intacta con lo que llevara anotado.
    /// </remarks>
    public Func<string, string?, RespuestaRepetido?>? ComprobarSiYaExiste { get; set; }

    /// <summary>
    /// Salta a gestión, vista calendario, buscando ese servicio. Lo resuelve la ventana,
    /// que es quien lleva las pestañas y conoce el tablero.
    /// </summary>
    public Action<DestinoDelCalendario>? VerEnElCalendario { get; set; }

    /// <summary>
    /// Diálogo de planificación: recibe el título y una copia de lo que hay, y devuelve lo
    /// editado o <c>null</c> si se canceló. Es el mismo que abre el tablero.
    /// </summary>
    public Func<string, Planificacion, Planificacion?>? PedirPlanificacion { get; set; }

    /// <summary>
    /// Pregunta si el servicio pasa a terminado o archivado. Sale al acabar de exportar y
    /// cuando la toma de notas se completa.
    /// </summary>
    public Func<string, RespuestaCierre>? PreguntarSiSeCierra { get; set; }
}

/// <summary>
/// A qué va el calendario cuando se salta desde una toma de notas: qué buscar y con qué
/// filtros, para que el servicio <b>aparezca de verdad</b>.
/// <para>
/// Al principio esto solo llevaba el código y no tocaba nada más. Daba pie a confusión
/// (2026‑08‑05): un servicio terminado o archivado no pasa el filtro de «En desarrollo»,
/// así que el botón llevaba a un calendario vacío y parecía roto.
/// </para>
/// </summary>
/// <param name="Codigo">Las once primeras del código, que es lo que se escribe en la caja de buscar.</param>
/// <param name="Estado">
/// El estado que hay que poner para que se vea, o <c>null</c> si con el que haya vale. Solo
/// se fuerza en los dos casos que esconden: terminado y archivado.
/// </param>
/// <param name="Tecnico">El responsable, para dejar su fila a la vista.</param>
public sealed record DestinoDelCalendario(string Codigo, string? Estado, string? Tecnico);

/// <summary>Qué se hace con un servicio que ya está listo.</summary>
public enum RespuestaCierre
{
    /// <summary>Dejarlo como está.</summary>
    Cancelar,

    /// <summary>Terminado, pero sigue en el calendario.</summary>
    Terminado,

    /// <summary>Terminado y fuera del calendario.</summary>
    Archivado
}

/// <summary>
/// Un proyecto abierto, es decir <b>una pestaña</b>. Nace vacío —enseñando la portada
/// para elegir norma— y pasa a ser una toma de notas en cuanto se elige una o se abre
/// un fichero. Cada pestaña lleva sus datos, su motor y sus cambios sin guardar.
/// </summary>
public sealed class DocumentoViewModel : ObservableObject
{
    private readonly RepositorioDeProyectos _repositorio;
    private readonly ServiciosDeVentana _servicios;
    private readonly Action<string> _alAbrirFichero;

    private DatosProyecto _datos = ProyectoVacio();
    private MotorDeReglas? _motor;
    private List<MotorDeReglas> _motoresAdicionales = [];
    private ProyectoViewModel _cabecera = null!;

    /// <summary>Apartado abierto, para recuperarlo tras reconstruir los paneles.</summary>
    private string? _idApartadoActual;
    private IndicadorDeAvance.Resultado? _avance;
    private object? _panelActual;
    private string? _ruta;
    private bool _hayCambiosSinGuardar;

    public DocumentoViewModel(IReadOnlyList<NormaDisponible> normas, RepositorioDeProyectos repositorio,
                              ServiciosDeVentana servicios, Action<string> alAbrirFichero)
    {
        Normas = normas;
        _repositorio = repositorio;
        _servicios = servicios;
        _alAbrirFichero = alAbrirFichero;

        Guardar = new Comando(() => GuardarProyecto(pedirRuta: false), () => !SinProyecto);
        GuardarComo = new Comando(() => GuardarProyecto(pedirRuta: true), () => !SinProyecto);
        ExportarInforme = new Comando(Exportar, () => !SinProyecto);
        AlternarIndice = new Comando(() => IndiceVisible = !IndiceVisible);

        Planificacion = new PlanificacionViewModel(
            repositorio,
            () => _ruta,
            () => _datos.CodigoTomaDeNotas,
            () => _datos.Tecnico1 ?? "",
            destino => _servicios.VerEnElCalendario?.Invoke(destino),
            (titulo, actual) => _servicios.PedirPlanificacion?.Invoke(titulo, actual),
            () => FechasDelEnsayo.De(_datos));
    }

    /// <summary>
    /// La planificación del servicio, para verla con el ensayo delante. Es una pestaña más
    /// del índice, encima de la cabecera: lo primero que se mira al abrir es para cuándo
    /// era y si han llegado las muestras.
    /// </summary>
    public PlanificacionViewModel Planificacion { get; }

    /// <summary>Lleva al panel de planificación desde la barra de arriba.</summary>
    public Comando VerPlanificacion => _verPlanificacion ??= new Comando(
        () => PanelActual = Planificacion, () => !SinProyecto);

    private Comando? _verPlanificacion;

    /// <summary>Normas instaladas, para la portada de la pestaña vacía.</summary>
    public IReadOnlyList<NormaDisponible> Normas { get; }

    public PlantillaEnsayos? Plantilla { get; private set; }
    public CatalogoDeEquipos Catalogo { get; private set; } = CatalogoDeEquipos.Vacio;

    /// <summary>Pestaña recién abierta: todavía no se ha elegido norma ni fichero.</summary>
    public bool SinProyecto => Plantilla is null;

    public bool HayProyecto => !SinProyecto;

    private bool _esActivo;

    /// <summary>Si es la pestaña de delante. Lo lleva la ventana; aquí solo se pinta.</summary>
    public bool EsActivo
    {
        get => _esActivo;
        set => Establecer(ref _esActivo, value);
    }

    public string? Ruta => _ruta;
    public bool HayCambiosSinGuardar => _hayCambiosSinGuardar;

    public Comando Guardar { get; }
    public Comando GuardarComo { get; }
    public Comando ExportarInforme { get; }
    public Comando AlternarIndice { get; }

    /// <summary>Índice jerárquico: la cabecera del proyecto y una rama por sección.</summary>
    public ObservableCollection<object> Arbol { get; } = [];

    /// <summary>Los mismos elementos en plano, para recalcular y para recordar la selección.</summary>
    public ObservableCollection<object> Paneles { get; } = [];

    // ---- rótulos -----------------------------------------------------------

    /// <summary>
    /// Lo que se lee en la lengüeta de la pestaña: el código de la toma de notas, que es
    /// lo que distingue una familia de otra cuando hay varias del mismo trabajo abiertas.
    /// </summary>
    public string Rotulo
        => SinProyecto
            ? RotulosDeTomaDeNotas.PestanaVacia
            : RotulosDeTomaDeNotas.Pestana(_datos.CodigoTomaDeNotas, _hayCambiosSinGuardar);

    /// <summary>
    /// El título que encabeza la toma de notas, y también el de la ventana de Windows.
    /// <para>
    /// Los dos dicen lo mismo a propósito: la designación de la norma y el código son
    /// exactamente lo que ya lleva dentro el nombre del fichero
    /// —<c>TdN_60598_TECNO260201-00.lmnlab</c>—, así que la barra de tareas no pierde
    /// nada y se lee mejor.
    /// </para>
    /// </summary>
    public string Titulo
    {
        get
        {
            // Sin proyecto abierto la barra de título dice el nombre del programa, que
            // se lee del ejecutable para no repetirlo escrito en otro sitio más.
            if (SinProyecto) return ServicioDeVersion.Nombre;

            return RotulosDeTomaDeNotas.Titulo(
                Plantilla!.Meta.ComoSeLlamaLaNorma, _datos.CodigoTomaDeNotas, _hayCambiosSinGuardar);
        }
    }

    public string Ubicacion => _ruta is null
        ? "El proyecto todavía no se ha guardado en ningún sitio"
        : Path.GetDirectoryName(_ruta) ?? "";

    /// <summary>Apartados completados sobre aplicables, sumando todas las normas.</summary>
    public string Contador => _avance is null ? "" : $"{_avance.Contador} apartados";

    // ---- índice de secciones ----------------------------------------------

    private bool _indiceVisible = true;

    /// <summary>
    /// El índice de la izquierda se puede plegar. Con la tabla de 30 muestras, esos
    /// 360 px son la diferencia entre ver tres columnas o siete.
    /// </summary>
    public bool IndiceVisible
    {
        get => _indiceVisible;
        private set
        {
            _indiceVisible = value;
            Notificar();
            Notificar(nameof(IconoIndice));
            Notificar(nameof(TituloIndice));
        }
    }

    public string IconoIndice => IndiceVisible ? "◀" : "▶";

    public string TituloIndice => IndiceVisible ? "Ocultar el índice de secciones" : "Mostrar el índice de secciones";

    public object? PanelActual
    {
        get => _panelActual;
        set
        {
            if (!Establecer(ref _panelActual, value)) return;
            _idApartadoActual = (value as BloqueViewModel)?.Codigo;

            // Al entrar se relee del disco: el responsable puede haber movido las fechas
            // desde el calendario mientras esta pestaña llevaba media hora abierta.
            if (value is PlanificacionViewModel plan) plan.Recargar();
        }
    }

    // ---- empezar y abrir ---------------------------------------------------

    /// <summary>Convierte una pestaña vacía en una toma de notas de esa norma.</summary>
    public void EmpezarCon(NormaDisponible norma)
    {
        if (!ConfirmarSiHayCambios()) return;

        CambiarNorma(norma);
        _datos = ProyectoVacio();
        _ruta = null;
        _hayCambiosSinGuardar = false;
        _idApartadoActual = null;
        Plantilla!.AplicarA(_datos);
        _adicionales.Clear();
        RefrescarNormasAnadibles();
        CrearCabecera();
        ReconstruirPaneles();
        Anunciar();
    }

    /// <summary>
    /// Abre un proyecto por ruta. Lo usan el diálogo, la lista de recientes y el
    /// arranque con un fichero como argumento (doble clic sobre el .lmnlab).
    /// </summary>
    /// <summary>
    /// Vuelve a leer la lista de técnicos en la ficha de proyecto. Lo llama la ventana
    /// tras editarla, para que las pestañas ya abiertas no sigan con la lista vieja.
    /// </summary>
    public void RefrescarTecnicos(IReadOnlyList<(string Viejo, string Nuevo)>? renombrados = null)
    {
        foreach (var cabecera in Paneles.OfType<ProyectoViewModel>())
            cabecera.RefrescarTecnicos(renombrados);
    }

    public bool CargarDesde(string ruta)
    {
        if (!ConfirmarSiHayCambios()) return false;

        try
        {
            var datos = _repositorio.Cargar(ruta);

            // El proyecto guarda con qué norma nació: se carga esa, no la que estuviera
            // abierta. Abrir un IK con la plantilla de luminarias no mostraría nada.
            //
            // Manda la que el proyecto dice que es la principal. El resto es el rescate
            // para los guardados antes de que se apuntara, y ahí el orden de un HashSet
            // no es de fiar: con dos normas podía abrirse por la añadida, y entonces
            // guardar reescribía el patrón de muestras —EBP_SAFE por EBP_CLIM— y con él
            // el identificador de todas las muestras del servicio.
            var norma = Normas.FirstOrDefault(n => n.Responde(datos.NormaPrincipal))
                        ?? datos.Normas.Select(id => Normas.FirstOrDefault(n => n.Responde(id)))
                                       .FirstOrDefault(n => n is not null)
                        ?? Normas.FirstOrDefault(n => n.CodigoDeFichero == "60598")
                        ?? Normas.FirstOrDefault();

            if (norma is null) throw new InvalidOperationException("No hay ninguna norma instalada.");

            _datos = datos;
            CambiarNorma(norma);

            _ruta = ruta;
            _hayCambiosSinGuardar = false;
            _idApartadoActual = null;
            Plantilla!.AplicarA(_datos);
            RecuperarNormasAnadidas();
            CrearCabecera();
            ReconstruirPaneles();
            _alAbrirFichero(ruta);
            Anunciar();

            // Que se ha abierto ya se ve: está en pantalla. Lo que sí hay que decir es que
            // se registró con otra versión de la norma, porque eso cambia lo que se pide.
            if (AvisoDeVersionDePlantilla() is { Length: > 0 } aviso) _servicios.Avisar?.Invoke(aviso);

            return true;
        }
        catch (Exception ex)
        {
            _servicios.Avisar?.Invoke($"No se pudo abrir:\n\n{ex.Message}\n\n{ruta}");
            return false;
        }
    }

    private void CambiarNorma(NormaDisponible norma)
    {
        Plantilla = PlantillaEnsayos.Cargar(norma.Ruta);
        Catalogo = CatalogoDeEquipos.Junto(norma.Ruta, Plantilla);
        _adicionales.Clear();
        RefrescarNormasAnadibles();
    }

    private void Anunciar()
    {
        Notificar(nameof(SinProyecto));
        Notificar(nameof(HayProyecto));
        Notificar(nameof(Rotulo));
        Notificar(nameof(Titulo));
        Notificar(nameof(Ubicacion));
        Guardar.Revisar();
        GuardarComo.Revisar();
        ExportarInforme.Revisar();
        VerPlanificacion.Revisar();
        Cambio?.Invoke();
    }

    /// <summary>Avisa a la ventana de que hay que refrescar menús y título.</summary>
    public Action? Cambio { get; set; }

    // ---- normas añadidas al proyecto ---------------------------------------

    private readonly List<(NormaDisponible Norma, PlantillaEnsayos Plantilla, CatalogoDeEquipos Catalogo)> _adicionales = [];

    /// <summary>Casillas para añadir o quitar normas, en la cabecera del proyecto.</summary>
    public ObservableCollection<NormaAnadibleViewModel> NormasAnadibles { get; } = [];

    public bool HayNormasAnadibles => NormasAnadibles.Count > 0;

    private void RefrescarNormasAnadibles()
    {
        NormasAnadibles.Clear();

        if (Plantilla is not null)
            foreach (var norma in Normas.Where(n => n.Id != Plantilla.Meta.Id && EsCompatible(n)))
                NormasAnadibles.Add(new NormaAnadibleViewModel(
                    norma,
                    activa: _adicionales.Any(a => a.Norma.Id == norma.Id),
                    CambiarNormaAnadida));

        Notificar(nameof(HayNormasAnadibles));
        _cabecera?.RefrescarNormas();
    }

    /// <summary>
    /// Qué normas admite la principal lo dice su plantilla (<c>meta.normasCompatibles</c>):
    /// luminarias no admite IP ni IK porque ya los lleva dentro. Si una plantilla no lo
    /// declara, se admiten todas.
    /// </summary>
    private bool EsCompatible(NormaDisponible norma)
        => Plantilla!.Meta.NormasCompatibles is not { } admitidas
           // La lista puede citar el id de ahora o uno anterior: una plantilla vieja que
           // nombre «60529» sigue refiriéndose a la misma norma.
           || admitidas.Any(norma.Responde);

    private void CambiarNormaAnadida(NormaDisponible norma, bool activa)
    {
        if (activa && _adicionales.All(a => a.Norma.Id != norma.Id))
        {
            var plantilla = PlantillaEnsayos.Cargar(norma.Ruta);
            plantilla.AplicarA(_datos, principal: false);
            _adicionales.Add((norma, plantilla, CatalogoDeEquipos.Junto(norma.Ruta, plantilla)));
        }
        else if (!activa)
        {
            _adicionales.RemoveAll(a => a.Norma.Id == norma.Id);
            _datos.Normas.Remove(norma.Id);
        }

        _hayCambiosSinGuardar = true;
        _idApartadoActual = null;
        ReconstruirPaneles();
    }

    /// <summary>Vuelve a cargar las normas añadidas que trae un proyecto al abrirlo.</summary>
    private void RecuperarNormasAnadidas()
    {
        _adicionales.Clear();

        foreach (var id in _datos.Normas.Where(id => id != Plantilla!.Meta.Id))
        {
            if (Normas.FirstOrDefault(n => n.Id == id) is not { } norma) continue;
            if (!EsCompatible(norma)) continue;
            var plantilla = PlantillaEnsayos.Cargar(norma.Ruta);
            _adicionales.Add((norma, plantilla, CatalogoDeEquipos.Junto(norma.Ruta, plantilla)));
        }

        RefrescarNormasAnadibles();
    }

    // ---- construcción de paneles ------------------------------------------

    private void CrearCabecera()
    {
        _cabecera = new ProyectoViewModel(Plantilla!, _datos, AlCambiarUnDato, AlCambiarNumeroDeMuestras)
        {
            NormasAnadibles = NormasAnadibles
        };
        _cabecera.RefrescarNormas();
    }

    private void ReconstruirPaneles()
    {
        _motor = new MotorDeReglas(Plantilla!, _datos);
        _motoresAdicionales = [.. _adicionales.Select(a => new MotorDeReglas(a.Plantilla, _datos))];

        Paneles.Clear();
        Arbol.Clear();

        // La planificación es un panel más, pero NO va en el árbol: se llega por su botón
        // de la barra de arriba, junto a «Guardar» y «Exportar». El árbol es el índice del
        // ensayo, y esto no es ensayo — colgarlo ahí lo hacía parecer un apartado que hay
        // que rellenar.
        Paneles.Add(Planificacion);
        Planificacion.Recargar();

        Paneles.Add(_cabecera);
        Arbol.Add(_cabecera);

        AnadirSecciones(Plantilla!, _motor, Catalogo, prefijo: null);

        // Las normas añadidas van detrás, con su nombre delante de cada sección para
        // que se vea de un vistazo a qué norma pertenece cada apartado.
        for (var i = 0; i < _adicionales.Count; i++)
            AnadirSecciones(_adicionales[i].Plantilla, _motoresAdicionales[i], _adicionales[i].Catalogo,
                            prefijo: _adicionales[i].Norma.Id);

        // Se recupera el apartado que estaba abierto por su código, no por posición:
        // los apartados que no aplican entran y salen de la lista.
        PanelActual = Paneles.OfType<BloqueViewModel>().FirstOrDefault(b => b.Codigo == _idApartadoActual)
                      ?? (object)_cabecera;

        DesplegarSeccionDe(PanelActual);
        Recalcular();
        Notificar(nameof(Ubicacion));
    }

    /// <summary>Despliega la rama del árbol que contiene el apartado abierto.</summary>
    private void DesplegarSeccionDe(object? panel)
    {
        if (panel is not BloqueViewModel apartado) return;

        foreach (var seccion in Arbol.OfType<SeccionViewModel>())
            if (seccion.Apartados.Contains(apartado))
            {
                seccion.Expandida = true;
                return;
            }
    }

    private void AnadirSecciones(PlantillaEnsayos plantilla, MotorDeReglas motor,
                                 CatalogoDeEquipos catalogo, string? prefijo)
    {
        foreach (var seccion in plantilla.Secciones)
        {
            var apartados = seccion.Bloques
                .Select(b => new BloqueViewModel(motor, _datos, seccion, b, AlCambiarUnDato, catalogo))
                .ToList();

            foreach (var apartado in apartados) Paneles.Add(apartado);

            var titulo = prefijo is null ? seccion.Titulo : $"{prefijo} | {seccion.Titulo}";
            Arbol.Add(new SeccionViewModel(titulo, apartados));
        }
    }

    public void Recalcular()
    {
        if (_motor is null || Plantilla is null) return;

        _motor.Invalidar();
        foreach (var motor in _motoresAdicionales) motor.Invalidar();

        // El contador de la cabecera suma todas las normas del proyecto.
        _avance = IndicadorDeAvance.Resultado.Sumar(
            _motoresAdicionales.Prepend(_motor).Select(m => new IndicadorDeAvance(m).Calcular()));

        foreach (var panel in Paneles.OfType<BloqueViewModel>()) panel.Refrescar();
        foreach (var cabecera in Paneles.OfType<ProyectoViewModel>()) cabecera.Refrescar();

        // Sin la cabecera rellena no se muestran las secciones de ensayo.
        var bloquear = !RequisitosDelProyecto.Completo(Plantilla, _datos);
        foreach (var seccion in Arbol.OfType<SeccionViewModel>())
        {
            seccion.Bloqueada = bloquear;
            seccion.Refrescar();
        }

        // Si el apartado abierto acaba de dejar de aplicar, se vuelve a la cabecera: pasa
        // al desmarcar una parte -2, al cambiar la clase o al vaciar un dato de cabecera.
        // Se nombra, y no se coge el primero: desde que la planificación va delante, el
        // primero ya no es la cabecera.
        if (PanelActual is BloqueViewModel actual && (bloquear || !actual.Visible))
            PanelActual = _cabecera;

        Notificar(nameof(Contador));
        Notificar(nameof(Titulo));
        Notificar(nameof(Rotulo));

        AvisarSiSeAcabo();
    }

    /// <summary>
    /// Cuando no queda ni un apartado por rellenar, se ofrece cerrar el servicio. Es el
    /// momento en que el técnico levanta la vista, y también el momento en que se olvida
    /// de que el calendario sigue diciendo que esto está en curso.
    /// </summary>
    private void AvisarSiSeAcabo()
    {
        if (_avance is not { } avance) return;
        if (avance.ApartadosAplicables == 0 || avance.ApartadosCompletados < avance.ApartadosAplicables) return;

        PreguntarSiSeCierra("Ya no queda ningún apartado por rellenar.", soloUnaVez: true);
    }

    private void AlCambiarUnDato()
    {
        _hayCambiosSinGuardar = true;
        Recalcular();
        Cambio?.Invoke();
    }

    private void AlCambiarNumeroDeMuestras()
    {
        _hayCambiosSinGuardar = true;
        ReconstruirPaneles();
    }

    // ---- guardar y exportar ------------------------------------------------

    private void GuardarProyecto(bool pedirRuta)
    {
        if (SinProyecto) return;

        // Sin código ni técnico el fichero no se puede ni nombrar ni atribuir, así que no
        // se escribe. Se lleva la vista a la cabecera: ahí están los dos, en rojo — decir
        // que falta algo sin enseñar dónde obliga a buscarlo.
        if (!RequisitosParaGuardar.SePuede(_datos))
        {
            PanelActual = Paneles.FirstOrDefault(p => p is ProyectoViewModel) ?? PanelActual;
            Cambio?.Invoke();
            _servicios.Avisar?.Invoke(RequisitosParaGuardar.Aviso(_datos));
            return;
        }

        try
        {
            if (pedirRuta || _ruta is null)
            {
                // Antes de crear un fichero: ¿no estará ya ese servicio en la carpeta?
                // Desde que el responsable da de alta los proyectos, un técnico puede
                // haberse puesto a tomar notas sin saber que el suyo ya existía.
                if (!ProsigueAunqueYaExista()) return;

                // El nombre lo fija el laboratorio: TdN_60598_TECNO260201-00.lmnlab
                // El código corto de la norma, no el id: el id lleva el año y el
                // laboratorio quiere el nombre del fichero como estaba (DD‑95).
                var sugerido = NombreDeTomaDeNotas.ConExtension(
                    Plantilla!.Meta.CodigoParaFichero, _datos.CodigoTomaDeNotas,
                    RepositorioDeProyectos.Extension);

                var elegida = _servicios.PedirFicheroParaGuardar?.Invoke(sugerido);
                if (string.IsNullOrWhiteSpace(elegida)) return;
                _ruta = elegida;
            }

            _repositorio.Guardar(_datos, _ruta, Plantilla!.Meta.Version);
            _hayCambiosSinGuardar = false;
            _alAbrirFichero(_ruta);

            // Que se ha guardado se ve sin decirlo: desaparece el punto de «sin guardar»
            // de la pestaña y del título, y la ruta está debajo del título.
            Notificar(nameof(Titulo));
            Notificar(nameof(Rotulo));
            Notificar(nameof(Ubicacion));
            Cambio?.Invoke();
        }
        catch (Exception ex)
        {
            Cambio?.Invoke();
            _servicios.Avisar?.Invoke($"No se pudo guardar:\n\n{ex.Message}\n\n{_ruta}");
        }
    }

    /// <summary>
    /// Aviso cuando el proyecto se registró con una versión de la plantilla distinta de la
    /// instalada. <b>No bloquea nada</b>: solo evita que el técnico vea cambiar el avance
    /// de un día para otro y lo tome por un fallo suyo — fue una corrección de la norma.
    /// </summary>
    private string AvisoDeVersionDePlantilla()
    {
        var guardada = _datos.VersionDePlantillaGuardada;
        var actual = Plantilla?.Meta.Version;

        return string.IsNullOrWhiteSpace(guardada) || string.IsNullOrWhiteSpace(actual)
               || guardada == actual
            ? ""
            : $"Esta toma de notas se registró con la plantilla {guardada} y la instalada "
              + $"es la {actual}.\n\nPuede cambiar lo que se pide o lo que aplica.";
    }

    /// <summary>
    /// Si hay que seguir guardando. Cuando el servicio ya existe en la carpeta se
    /// pregunta, y el técnico puede abrir el que hay —lo habitual— o crear otro a
    /// sabiendas: un reensayo o un servicio partido pueden repetir código legítimamente.
    /// </summary>
    private bool ProsigueAunqueYaExista()
    {
        if (_servicios.ComprobarSiYaExiste is null) return true;

        // Si elige abrir el que ya existe, lo abre la ventana en una pestaña aparte —esta
        // se queda como está, con lo que hubiera anotado— y aquí solo hay que no seguir.
        return _servicios.ComprobarSiYaExiste(_datos.CodigoTomaDeNotas, _ruta)
            is null or RespuestaRepetido.CrearIgualmente;
    }

    private void Exportar()
    {
        if (SinProyecto) return;

        try
        {
            // El mismo nombre que el .lmnlab, cambiando solo la extensión: el HTML y el
            // fichero de trabajo son el mismo ensayo, y con nombres distintos —«Toma de
            // notas ANTAR2504» frente a «TdN_60598_ANTAR250401-00»— no se emparejaban ni
            // en la carpeta ni de un vistazo.
            var sugerido = NombreDeTomaDeNotas.ConExtension(
                Plantilla!.Meta.CodigoParaFichero, _datos.CodigoTomaDeNotas,
                ExportadorDeInforme.Extension);

            var destino = _servicios.PedirFicheroParaInforme?.Invoke(sugerido);
            if (string.IsNullOrWhiteSpace(destino)) return;

            new ExportadorDeInforme(Plantilla!, Catalogo)
            {
                Adicionales = [.. _adicionales.Select(a =>
                    new ExportadorDeInforme.NormaAdicional(a.Plantilla, a.Catalogo))]
            }.Exportar(_datos, destino);

            // Que se ha exportado se ve: el informe se abre solo, a continuación.

            // Se pregunta con el fichero ya escrito pero ANTES de abrirlo en el visor.
            // Al revés no funcionaba: abrir el HTML lanza el navegador, el navegador se
            // queda en primer plano y Windows no deja que otra aplicación se ponga
            // delante, así que la ventana salía detrás y parecía que no salía.
            PreguntarSiSeCierra("Acabas de exportar la toma de notas.", soloUnaVez: false);

            _servicios.AbrirEnElVisor?.Invoke(destino);
        }
        catch (Exception ex)
        {
            _servicios.Avisar?.Invoke($"No se pudo exportar:\n\n{ex.Message}");
        }

        Cambio?.Invoke();
    }

    /// <summary>
    /// Si ya se ha preguntado en esta pestaña. <b>Se pregunta una vez</b>: el estado se
    /// recalcula con cada tecla, y sin esto la ventana saltaría sola una y otra vez
    /// mientras se rellena el último apartado.
    /// </summary>
    private bool _yaSePreguntoElCierre;

    /// <summary>
    /// Ofrece dejar el servicio terminado o archivado.
    /// </summary>
    /// <param name="soloUnaVez">
    /// Cierto cuando lo dispara que la toma de notas se complete: ahí hay que frenarse,
    /// porque el estado se recalcula con cada tecla. <b>Falso al exportar</b>, y entonces
    /// se pregunta siempre, esté la toma de notas terminada o a medias: exportar es el
    /// momento en que el trabajo sale por la puerta, y es justo cuando se olvida dejar el
    /// estado puesto — que era el motivo de todo esto. Cancelar sigue estando.
    /// </param>
    private void PreguntarSiSeCierra(string motivo, bool soloUnaVez)
    {
        // Sin fichero no hay dónde escribir el estado. Se dice, porque callarse aquí es
        // exactamente el fallo que esta pregunta viene a evitar: el técnico entrega el
        // informe y el calendario sigue diciendo que el trabajo está en curso.
        if (_ruta is null)
        {
            if (!soloUnaVez)
                _servicios.Avisar?.Invoke(
                    "Guarda la toma de notas para poder dejar el servicio como terminado.");
            return;
        }

        if (soloUnaVez && _yaSePreguntoElCierre) return;

        Planificacion.Recargar();
        if (soloUnaVez && Planificacion.YaEstaCerrado) return;
        if (_servicios.PreguntarSiSeCierra is not { } preguntar) return;

        if (soloUnaVez) _yaSePreguntoElCierre = true;

        switch (preguntar(motivo))
        {
            case RespuestaCierre.Terminado: Planificacion.Cerrar(archivar: false); break;
            case RespuestaCierre.Archivado: Planificacion.Cerrar(archivar: true); break;
        }
    }

    /// <summary>
    /// Puerta por la que pasa todo lo que abandona este proyecto: cerrar la pestaña,
    /// cerrar la aplicación o abrir otra cosa encima. Al elegir «Guardar cambios» se
    /// guarda primero, y si ese guardado se cancela no se sigue adelante.
    /// </summary>
    public bool ConfirmarSiHayCambios()
    {
        if (!_hayCambiosSinGuardar) return true;

        switch (_servicios.ConfirmarDescartarCambios?.Invoke() ?? RespuestaCambios.Descartar)
        {
            case RespuestaCambios.Guardar:
                GuardarProyecto(pedirRuta: false);
                return !_hayCambiosSinGuardar;

            case RespuestaCambios.Descartar:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Cabecera realmente vacía: sin código y sin número de muestras. Así los campos
    /// obligatorios se ven en blanco y en rojo, en lugar de con valores de relleno.
    /// </summary>
    private static DatosProyecto ProyectoVacio()
        => new() { CodigoServicio = "", NumeroMuestras = 0 };
}
