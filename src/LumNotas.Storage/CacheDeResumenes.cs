using System.Text.Json;
using LumNotas.Core.Gestion;

namespace LumNotas.Storage;

/// <summary>
/// Lo que ya se sabe de cada proyecto, guardado entre sesiones.
/// <para>
/// El tablero **lee entero** cada <c>.lumproj</c> para calcular su estado, y en el
/// laboratorio los proyectos cuelgan de una matrioska de carpetas de clientes con años
/// de historia. Sin esto, cada arranque volvía a leer y analizar todos los proyectos,
/// aunque no hubiera cambiado ninguno.
/// </para>
/// <para>
/// Se guarda en el perfil del usuario, no en la carpeta compartida: es una caché de este
/// equipo, y varios técnicos escribiéndola a la vez sobre OneDrive solo daría conflictos.
/// </para>
/// </summary>
public sealed class CacheDeResumenes
{
    private readonly Dictionary<string, Entrada> _entradas;
    private readonly string _ruta;

    private CacheDeResumenes(string ruta, Dictionary<string, Entrada> entradas)
    {
        _ruta = ruta;
        _entradas = entradas;
    }

    public static string RutaPorDefecto => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LumNotas", "resumenes.cache.json");

    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static CacheDeResumenes Cargar(string? ruta = null)
    {
        ruta ??= RutaPorDefecto;

        try
        {
            if (File.Exists(ruta))
            {
                var leidas = JsonSerializer.Deserialize<List<Entrada>>(File.ReadAllText(ruta), Opciones);
                if (leidas is not null)
                    return new CacheDeResumenes(ruta,
                        leidas.Where(e => e is { Ruta.Length: > 0, Resumen: not null })
                              .GroupBy(e => e.Ruta, StringComparer.OrdinalIgnoreCase)
                              .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase));
            }
        }
        catch
        {
            // Una caché corrupta se tira y se rehace: no puede impedir abrir el tablero.
        }

        return new CacheDeResumenes(ruta, new(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// El resumen guardado de un fichero, si sigue valiendo. Se comprueba la fecha, el
    /// tamaño <b>y la plantilla</b>: el resumen sale de aplicar las reglas de una norma,
    /// así que al publicar una versión nueva deja de valer.
    /// </summary>
    public ResumenDeProyecto? Recuperar(FileInfo fichero, string plantilla)
        => _entradas.TryGetValue(fichero.FullName, out var entrada)
           && entrada.Ticks == fichero.LastWriteTimeUtc.Ticks
           && entrada.Tamano == fichero.Length
           && entrada.Plantilla == plantilla
            ? entrada.Resumen
            : null;

    /// <summary>Si hay algo que no esté ya escrito en el disco.</summary>
    private bool _sucia;

    public void Anotar(FileInfo fichero, string plantilla, ResumenDeProyecto resumen)
    {
        _entradas[fichero.FullName] = new Entrada
        {
            Ruta = fichero.FullName,
            Ticks = fichero.LastWriteTimeUtc.Ticks,
            Tamano = fichero.Length,
            Plantilla = plantilla,
            Resumen = resumen
        };

        _sucia = true;
    }

    /// <summary>Olvida los proyectos que ya no están, para que la caché no crezca sin fin.</summary>
    public void ConservarSolo(IEnumerable<string> rutas)
    {
        var vivos = new HashSet<string>(rutas, StringComparer.OrdinalIgnoreCase);

        foreach (var muerto in _entradas.Keys.Where(r => !vivos.Contains(r)).ToList())
        {
            _entradas.Remove(muerto);
            _sucia = true;
        }
    }

    public void Vaciar()
    {
        _entradas.Clear();
        _sucia = true;
    }

    /// <summary>
    /// Escribe la caché <b>solo si ha cambiado algo</b>. Sin esta comprobación, un
    /// refresco en el que no hubiera cambiado ningún proyecto reescribía igualmente el
    /// fichero entero —cientos de kilobytes— y se comía buena parte de lo ahorrado.
    /// </summary>
    public void Guardar()
    {
        if (!_sucia) return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_ruta)!);
            File.WriteAllText(_ruta, JsonSerializer.Serialize(_entradas.Values, Opciones));
            _sucia = false;
        }
        catch
        {
            // Si no se puede escribir, el programa sigue: solo será más lento al arrancar.
        }
    }

    public int Cuantos => _entradas.Count;

    private sealed class Entrada
    {
        public string Ruta { get; set; } = "";
        public long Ticks { get; set; }
        public long Tamano { get; set; }
        public string Plantilla { get; set; } = "";
        public ResumenDeProyecto? Resumen { get; set; }
    }
}
