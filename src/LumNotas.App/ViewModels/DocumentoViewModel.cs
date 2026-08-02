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
    /// y devuelve la ruta del <c>.lumproj</c> creado, o <c>null</c> si se canceló.
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
    }

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
    /// Las partes se separan con «|» y el punto de «sin guardar» va al final. Antes el
    /// separador era también un punto y se leía «ALVEI2306 • · Luminarias», que parecen
    /// dos separadores seguidos.
    /// </summary>
    private const string Separador = " | ";

    private string MarcaDeCambios => _hayCambiosSinGuardar ? " •" : "";

    private string Codigo => string.IsNullOrWhiteSpace(_datos.CodigoServicio)
        ? "(sin código)"
        : _datos.CodigoServicio;

    /// <summary>Lo que se lee en la lengüeta de la pestaña.</summary>
    public string Rotulo
    {
        get
        {
            if (SinProyecto) return "Nueva pestaña";

            var norma = Plantilla!.Meta.TituloCorto ?? Plantilla.Meta.Id;
            return $"{Codigo}{Separador}{norma}{MarcaDeCambios}";
        }
    }

    public string Titulo
    {
        get
        {
            // Sin proyecto abierto la barra de título dice el nombre del programa, que
            // se lee del ejecutable para no repetirlo escrito en otro sitio más.
            if (SinProyecto) return ServicioDeVersion.Nombre;

            var nombre = _ruta is null ? "sin guardar" : Path.GetFileName(_ruta);
            return $"Toma de notas{Separador}{Codigo}{Separador}{nombre}{MarcaDeCambios}";
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
    /// arranque con un fichero como argumento (doble clic sobre el .lumproj).
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
            var norma = Normas.FirstOrDefault(n => n.Id == datos.NormaPrincipal)
                        ?? datos.Normas.Select(id => Normas.FirstOrDefault(n => n.Id == id))
                                       .FirstOrDefault(n => n is not null)
                        ?? Normas.FirstOrDefault(n => n.Id == "60598")
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
            Mensaje = $"Abierto {ruta}";
            return true;
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo abrir: {ex.Message}";
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
        Cambio?.Invoke();
    }

    /// <summary>Avisa a la ventana de que hay que refrescar menús y título.</summary>
    public Action? Cambio { get; set; }

    public string Mensaje { get; private set; } = "";

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
        => Plantilla!.Meta.NormasCompatibles is not { } admitidas || admitidas.Contains(norma.Id);

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

            var titulo = prefijo is null ? seccion.Titulo : $"{prefijo} · {seccion.Titulo}";
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
        if (PanelActual is BloqueViewModel actual && (bloquear || !actual.Visible))
            PanelActual = Paneles.FirstOrDefault();

        Notificar(nameof(Contador));
        Notificar(nameof(Titulo));
        Notificar(nameof(Rotulo));
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

        try
        {
            if (pedirRuta || _ruta is null)
            {
                // Antes de crear un fichero: ¿no estará ya ese servicio en la carpeta?
                // Desde que el responsable da de alta los proyectos, un técnico puede
                // haberse puesto a tomar notas sin saber que el suyo ya existía.
                if (!ProsigueAunqueYaExista()) return;

                // El nombre lo fija el laboratorio: TdN_60598_LEDC42502xx-00.lumproj
                var sugerido = NombreDeTomaDeNotas.ConExtension(
                    Plantilla!.Meta.Id, _datos.CodigoServicio, RepositorioDeProyectos.Extension);

                var elegida = _servicios.PedirFicheroParaGuardar?.Invoke(sugerido);
                if (string.IsNullOrWhiteSpace(elegida)) return;
                _ruta = elegida;
            }

            _repositorio.Guardar(_datos, _ruta, Plantilla!.Meta.Version);
            _hayCambiosSinGuardar = false;
            _alAbrirFichero(_ruta);
            Mensaje = $"Guardado en {_ruta}  ({DateTime.Now:HH:mm:ss})";
            Notificar(nameof(Titulo));
            Notificar(nameof(Rotulo));
            Notificar(nameof(Ubicacion));
            Cambio?.Invoke();
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar: {ex.Message}";
            Cambio?.Invoke();
        }
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
        return _servicios.ComprobarSiYaExiste(_datos.CodigoServicio, _ruta)
            is null or RespuestaRepetido.CrearIgualmente;
    }

    private void Exportar()
    {
        if (SinProyecto) return;

        try
        {
            var sugerido = $"Toma de notas {_datos.CodigoServicio}{ExportadorDeInforme.Extension}";
            var destino = _servicios.PedirFicheroParaInforme?.Invoke(sugerido);
            if (string.IsNullOrWhiteSpace(destino)) return;

            new ExportadorDeInforme(Plantilla!, Catalogo)
            {
                Adicionales = [.. _adicionales.Select(a =>
                    new ExportadorDeInforme.NormaAdicional(a.Plantilla, a.Catalogo))]
            }.Exportar(_datos, destino);

            Mensaje = $"Informe generado en {destino}. Ábrelo en Word o pulsa Ctrl+P para guardarlo como PDF.";
            _servicios.AbrirEnElVisor?.Invoke(destino);
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo generar el informe: {ex.Message}";
        }

        Cambio?.Invoke();
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
