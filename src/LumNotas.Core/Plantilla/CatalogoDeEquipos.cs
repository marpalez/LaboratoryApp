using System.Text.Json;

namespace LumNotas.Core.Plantilla;

/// <summary>
/// Catálogo de equipos del laboratorio, importado literalmente del Excel (DD-10).
/// Cada apartado de la plantilla apunta a un grupo por su id.
/// </summary>
public sealed class CatalogoDeEquipos
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    public List<string> NotasDeUso { get; init; } = [];
    public List<GrupoDeEquipos> Grupos { get; init; } = [];

    public static CatalogoDeEquipos Cargar(string rutaJson)
        => JsonSerializer.Deserialize<CatalogoDeEquipos>(File.ReadAllText(rutaJson), Opciones)
           ?? throw new InvalidOperationException($"El catálogo de equipos '{rutaJson}' no se pudo leer.");

    /// <summary>Vacío, para cuando no hay catálogo disponible: la aplicación sigue funcionando.</summary>
    public static CatalogoDeEquipos Vacio { get; } = new();

    /// <summary>
    /// El catálogo que declara una plantilla, buscado junto a ella. Si falta, se
    /// devuelve el vacío: la toma de notas funciona igual, solo que sin poder marcar
    /// equipos.
    /// </summary>
    public static CatalogoDeEquipos Junto(string rutaPlantilla, PlantillaEnsayos plantilla)
    {
        var nombre = plantilla.Meta.CatalogoEquipos;
        if (string.IsNullOrWhiteSpace(nombre)) return Vacio;

        var ruta = Path.Combine(Path.GetDirectoryName(rutaPlantilla)!, nombre);
        return File.Exists(ruta) ? Cargar(ruta) : Vacio;
    }

    public GrupoDeEquipos? Grupo(string? id)
        => id is null ? null : Grupos.FirstOrDefault(g => g.Id == id);
}

public sealed class GrupoDeEquipos
{
    public string Id { get; init; } = "";
    public string Seccion { get; init; } = "";
    public string Titulo { get; init; } = "";
    public string? OrigenExcel { get; init; }
    public bool PermiteOtros { get; init; }
    public bool FueraDeAlcance { get; init; }
    public List<string> Notas { get; init; } = [];
    public List<EquipoDelCatalogo> Equipos { get; init; } = [];
}

public sealed class EquipoDelCatalogo
{
    public string CodigoLiteral { get; init; } = "";
    public List<string> Codigos { get; init; } = [];
    public string Descripcion { get; init; } = "";
    public string? OrigenExcel { get; init; }

    /// <summary>
    /// Identificador estable para guardar la selección. Se compone de los códigos y no
    /// de la posición, para que reordenar el catálogo no invalide proyectos guardados.
    /// </summary>
    public string Id => Codigos.Count > 0
        ? string.Join("+", Codigos)
        : new string([.. CodigoLiteral.Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '-')]);

    /// <summary>Etiqueta para la interfaz y el informe.</summary>
    public string Etiqueta => string.IsNullOrWhiteSpace(Descripcion)
        ? CodigoLiteral
        : $"{CodigoLiteral} · {Descripcion}";
}
