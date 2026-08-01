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
    private PlantillaEnsayos _plantilla;
    private readonly Ajustes _ajustes = Ajustes.Cargar();
    private string _mensaje = "";
    private bool _soloPendientes = true;
    private bool _vistaCalendario;

    public GestionViewModel(PlantillaEnsayos plantilla, RepositorioDeProyectos repositorio)
    {
        _plantilla = plantilla;
        _repositorio = repositorio;
        _explorador = new ExploradorDeProyectos(repositorio);
        Calendario = new CalendarioViewModel(Planificar, ruta => AbrirProyecto?.Invoke(ruta), Guardar);

        // El tablero mide el avance con la norma que esté cargada.

        Refrescar = new Comando(Explorar);
        VerTablero = new Comando(() => VistaCalendario = false);
        VerCalendario = new Comando(() => VistaCalendario = true);
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

    /// <summary>Abrir un proyecto en una pestaña; lo resuelve la ventana.</summary>
    public Action<string>? AbrirProyecto { get; set; }

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
    public bool VistaCalendario
    {
        get => _vistaCalendario;
        set
        {
            if (!Establecer(ref _vistaCalendario, value)) return;
            Notificar(nameof(VistaTablero));
        }
    }

    public bool VistaTablero => !_vistaCalendario;

    public Comando VerTablero { get; }
    public Comando VerCalendario { get; }

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

        // El explorador cachea por fecha de modificación; el fichero acaba de cambiar.
        _explorador.OlvidarCache();
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
    /// Al cambiar de norma el tablero tiene que medir con la nueva: los apartados y el
    /// número de secciones son distintos.
    /// </summary>
    public void CambiarPlantilla(PlantillaEnsayos plantilla)
    {
        _plantilla = plantilla;
        if (!string.IsNullOrWhiteSpace(Carpeta)) Explorar();
    }

    public Comando Refrescar { get; }
    public Comando ElegirCarpeta { get; }

    /// <summary>Diálogo de carpeta; lo inyecta la ventana para no atar el modelo a WPF.</summary>
    public Func<string, string?>? PedirCarpeta { get; set; }

    public string Carpeta
    {
        get => _ajustes.CarpetaDeProyectos;
        set
        {
            _ajustes.CarpetaDeProyectos = value;
            _ajustes.Guardar();
            Notificar();
            _explorador.OlvidarCache();
            Explorar();
        }
    }

    /// <summary>Ocultar los proyectos ya terminados: al PM le interesa lo que queda.</summary>
    public bool SoloPendientes
    {
        get => _soloPendientes;
        set { if (Establecer(ref _soloPendientes, value)) Explorar(); }
    }

    public string Mensaje
    {
        get => _mensaje;
        private set => Establecer(ref _mensaje, value);
    }

    public bool HayCarpeta => !string.IsNullOrWhiteSpace(Carpeta);

    private void Explorar()
    {
        Proyectos.Clear();
        Notificar(nameof(HayCarpeta));

        if (!HayCarpeta)
        {
            Calendario.Cargar([]);
            Mensaje = "Elige la carpeta donde el laboratorio guarda los proyectos.";
            return;
        }

        var todos = _explorador.Explorar(Carpeta, _plantilla);

        // El calendario recibe todos: «solo pendientes» es un filtro del tablero, y en la
        // línea de tiempo interesa ver también lo ya terminado para saber qué hueco queda.
        Calendario.Cargar(todos);

        var mostrados = SoloPendientes ? todos.Where(p => !p.Terminado).ToList() : [.. todos];

        foreach (var proyecto in mostrados) Proyectos.Add(proyecto);

        var terminados = todos.Count(p => p.Terminado);
        Mensaje = todos.Count == 0
            ? $"No hay proyectos en {Carpeta}"
            : $"{todos.Count} proyectos · {terminados} terminados · actualizado a las {DateTime.Now:HH:mm}";
    }
}

/// <summary>Ajustes de la aplicación, guardados en el perfil del usuario.</summary>
public sealed class Ajustes
{
    private static string Ruta => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LumNotas", "ajustes.json");

    public string CarpetaDeProyectos { get; set; } = "";

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
