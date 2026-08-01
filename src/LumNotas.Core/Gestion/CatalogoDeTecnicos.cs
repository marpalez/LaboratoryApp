using System.Text.Json;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Los técnicos del laboratorio, para que el nombre se elija de una lista en vez de
/// escribirse. Escribirlo a mano producía la misma persona con tres grafías distintas,
/// lo que rompe cualquier filtro y cualquier recuento por técnico.
/// <para>
/// El fichero vive en la <b>carpeta de proyectos</b>, que es la única carpeta compartida
/// que conoce la aplicación: así añadir un técnico se hace una vez y lo ven todos, en
/// lugar de tener que repetirlo en cada equipo.
/// </para>
/// </summary>
public sealed class CatalogoDeTecnicos
{
    public const string NombreDeFichero = "tecnicos.json";

    /// <summary>Plantilla de partida, la del laboratorio a 2026‑08‑06.</summary>
    public static IReadOnlyList<string> Iniciales =>
    [
        "Daniel Martínez",
        "Daniel Pastor",
        "Javier Ibor",
        "Javier Salvador",
        "Mario Madrigal",
        "Raúl González"
    ];

    private readonly List<string> _tecnicos;

    private CatalogoDeTecnicos(IEnumerable<string> tecnicos) => _tecnicos = Normalizar(tecnicos);

    public IReadOnlyList<string> Tecnicos => _tecnicos;

    public static CatalogoDeTecnicos DePartida() => new(Iniciales);

    /// <summary>
    /// Lee el catálogo de una carpeta. Si no hay fichero todavía —instalación nueva—
    /// devuelve la lista de partida; si el fichero está roto, también, porque quedarse
    /// sin técnicos impediría abrir proyectos.
    /// </summary>
    public static CatalogoDeTecnicos Cargar(string carpeta)
    {
        try
        {
            var ruta = Path.Combine(carpeta, NombreDeFichero);
            if (!File.Exists(ruta)) return DePartida();

            var guardado = JsonSerializer.Deserialize<Documento>(File.ReadAllText(ruta));
            return guardado is { Tecnicos.Count: > 0 } ? new CatalogoDeTecnicos(guardado.Tecnicos) : DePartida();
        }
        catch
        {
            return DePartida();
        }
    }

    public void Guardar(string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        var texto = JsonSerializer.Serialize(new Documento { Tecnicos = _tecnicos },
                                             new JsonSerializerOptions { WriteIndented = true });

        // Escritura atómica, igual que los proyectos: la carpeta está sincronizada.
        var temporal = Path.Combine(carpeta, Path.GetRandomFileName());
        File.WriteAllText(temporal, texto, new System.Text.UTF8Encoding(false));

        var ruta = Path.Combine(carpeta, NombreDeFichero);
        if (File.Exists(ruta)) File.Replace(temporal, ruta, destinationBackupFileName: null);
        else File.Move(temporal, ruta);
    }

    /// <summary>Añade un técnico. Devuelve falso si está vacío o ya estaba.</summary>
    public bool Anadir(string nombre)
    {
        var limpio = (nombre ?? "").Trim();
        if (limpio.Length == 0 || Contiene(limpio)) return false;

        _tecnicos.Add(limpio);
        _tecnicos.Sort(Comparador);
        return true;
    }

    /// <summary>
    /// Quita un técnico de la lista. <b>No toca ningún proyecto</b>: quien ya estuviera
    /// firmado por él sigue estándolo, porque el ensayo lo hizo esa persona.
    /// </summary>
    public bool Quitar(string nombre) => _tecnicos.RemoveAll(t => Igual(t, nombre)) > 0;

    /// <summary>
    /// Corrige el nombre de un técnico. A diferencia de quitarlo, esto <b>sí</b> hay que
    /// propagarlo a los proyectos: una errata no es una persona distinta.
    /// </summary>
    public bool Renombrar(string viejo, string nuevo)
    {
        var limpio = (nuevo ?? "").Trim();
        if (limpio.Length == 0 || Igual(viejo, limpio)) return false;
        if (Contiene(limpio)) return false;

        var posicion = _tecnicos.FindIndex(t => Igual(t, viejo));
        if (posicion < 0) return false;

        _tecnicos[posicion] = limpio;
        _tecnicos.Sort(Comparador);
        return true;
    }

    public bool Contiene(string nombre) => _tecnicos.Any(t => Igual(t, nombre));

    /// <summary>
    /// La lista con un nombre extra al principio si no estuviera. Lo usan las fichas de
    /// proyecto: un servicio guardado antes de existir la lista lleva el técnico escrito
    /// a mano, y el desplegable <b>no puede dejarlo en blanco</b>.
    /// </summary>
    public IReadOnlyList<string> ConNombreSuelto(params string?[] nombres)
    {
        var lista = new List<string>(_tecnicos);

        foreach (var nombre in nombres)
        {
            var limpio = (nombre ?? "").Trim();
            if (limpio.Length > 0 && !lista.Any(t => Igual(t, limpio))) lista.Insert(0, limpio);
        }

        return lista;
    }

    private static bool Igual(string a, string? b)
        => string.Equals(a, (b ?? "").Trim(), StringComparison.CurrentCultureIgnoreCase);

    private static List<string> Normalizar(IEnumerable<string> tecnicos)
    {
        var lista = tecnicos.Select(t => (t ?? "").Trim())
                            .Where(t => t.Length > 0)
                            .Distinct(StringComparer.CurrentCultureIgnoreCase)
                            .ToList();
        lista.Sort(Comparador);
        return lista;
    }

    private static int Comparador(string a, string b)
        => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);

    private sealed class Documento
    {
        public List<string> Tecnicos { get; init; } = [];
    }
}
