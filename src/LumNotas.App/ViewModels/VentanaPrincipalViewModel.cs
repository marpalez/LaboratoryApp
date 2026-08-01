using System.Collections.ObjectModel;
using System.IO;
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
            AbrirProyecto = AbrirEnPestana
        };

        NuevaPestana = new Comando(() => AbrirPestana());
        CerrarPestana = new ComandoCon<object>(Cerrar);
        ActivarPestana = new ComandoCon<object>(p => Activo = p);
        Abrir = new Comando(AbrirProyecto);
        AbrirReciente = new ComandoCon<string>(ruta => AbrirEnPestana(ruta));
        EmpezarConNorma = new ComandoCon<NormaDisponible>(EmpezarCon);
        IrAGestion = new Comando(AbrirGestion);
        EditarTecnicos = new Comando(AbrirEditorDeTecnicos);

        RefrescarRecientes();
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
    public ObservableCollection<ProyectoReciente> Recientes { get; } = [];
    public bool HayRecientes => Recientes.Count > 0;

    public Comando NuevaPestana { get; }
    public ComandoCon<object> CerrarPestana { get; }
    public ComandoCon<object> ActivarPestana { get; }
    public Comando Abrir { get; }
    public ComandoCon<string> AbrirReciente { get; }
    public ComandoCon<NormaDisponible> EmpezarConNorma { get; }
    public Comando IrAGestion { get; }
    public Comando EditarTecnicos { get; }

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

    public string Titulo => ActivoDocumento?.Titulo ?? "Toma de notas de ensayos";

    private string _mensaje = "";

    public string Mensaje
    {
        get => _mensaje;
        private set => Establecer(ref _mensaje, value);
    }

    // ---- pestañas ----------------------------------------------------------

    /// <summary>Abre una pestaña nueva, que arranca enseñando la portada.</summary>
    public DocumentoViewModel AbrirPestana()
    {
        var documento = new DocumentoViewModel(Normas, _repositorio, Servicios, Registrar);

        documento.Cambio = () =>
        {
            Mensaje = documento.Mensaje;
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
    private void AbrirGestion()
    {
        if (!Pestanas.Contains(Gestion)) Pestanas.Add(Gestion);
        Activo = Gestion;
    }

    private void EmpezarCon(NormaDisponible norma)
    {
        var documento = ActivoDocumento ?? AbrirPestana();
        documento.EmpezarCon(norma);
        Gestion.CambiarPlantilla(documento.Plantilla ?? PlantillaDeReferencia());
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
            Activo = yaAbierto;
            Mensaje = $"Ya estaba abierto en otra pestaña: {ruta}";
            return;
        }

        // Una pestaña sin estrenar se aprovecha; si no, se abre otra.
        var documento = ActivoDocumento is { SinProyecto: true } vacia ? vacia : AbrirPestana();

        if (documento.CargarDesde(ruta))
        {
            Activo = documento;
            Gestion.CambiarPlantilla(documento.Plantilla ?? PlantillaDeReferencia());
        }

        Mensaje = documento.Mensaje;
    }

    private void Registrar(string ruta)
    {
        _recientes.Registrar(ruta);
        RefrescarRecientes();
    }

    private void RefrescarRecientes()
    {
        Recientes.Clear();
        foreach (var r in _recientes.Existentes) Recientes.Add(new ProyectoReciente(r));
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
        try { return CatalogoDeNormas.Disponibles(); }
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

/// <summary>Entrada de la lista de proyectos recientes.</summary>
public sealed class ProyectoReciente(string ruta)
{
    public string Ruta { get; } = ruta;
    public string Nombre { get; } = Path.GetFileNameWithoutExtension(ruta);
    public string Carpeta { get; } = Path.GetDirectoryName(ruta) ?? "";
    public string Etiqueta => $"{Nombre}  |  {Carpeta}";
}
