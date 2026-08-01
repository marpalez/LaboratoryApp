using System.Text.Json;

namespace LumNotas.Storage;

/// <summary>
/// Lista de proyectos abiertos recientemente, para no tener que navegar carpetas
/// cada vez. Se guarda en el perfil del usuario, no junto a los proyectos.
/// </summary>
public sealed class ProyectosRecientes
{
    private const int Maximo = 10;
    private readonly string _ruta;
    private List<string> _rutas = [];

    public ProyectosRecientes(string? rutaFichero = null)
    {
        _ruta = rutaFichero ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LumNotas", "recientes.json");
        Cargar();
    }

    public IReadOnlyList<string> Rutas => _rutas;

    /// <summary>Solo devuelve los que siguen existiendo: OneDrive y las carpetas cambian.</summary>
    public IReadOnlyList<string> Existentes => [.. _rutas.Where(File.Exists)];

    public void Registrar(string ruta)
    {
        var completa = Path.GetFullPath(ruta);
        _rutas.RemoveAll(r => string.Equals(r, completa, StringComparison.OrdinalIgnoreCase));
        _rutas.Insert(0, completa);
        if (_rutas.Count > Maximo) _rutas = [.. _rutas.Take(Maximo)];
        Guardar();
    }

    private void Cargar()
    {
        try
        {
            if (!File.Exists(_ruta)) return;
            _rutas = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_ruta)) ?? [];
        }
        catch
        {
            _rutas = [];   // una lista de recientes corrupta no debe impedir arrancar
        }
    }

    private void Guardar()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_ruta)!);
            File.WriteAllText(_ruta, JsonSerializer.Serialize(_rutas, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Tampoco debe impedir trabajar si el perfil no es escribible.
        }
    }
}
