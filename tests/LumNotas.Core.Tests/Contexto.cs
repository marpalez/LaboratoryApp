using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Tests;

/// <summary>
/// Carga la plantilla real del MVP (no una de juguete): si la plantilla y el motor
/// dejan de encajar, los tests lo detectan.
/// </summary>
public static class Contexto
{
    private static readonly Lazy<PlantillaEnsayos> Cargada = new(() =>
        PlantillaEnsayos.Cargar(RutaPlantilla()));

    public static PlantillaEnsayos Plantilla => Cargada.Value;

    /// <remarks>
    /// Se busca por patrón y no por nombre exacto: el fichero lleva el año y la
    /// versión —<c>plantilla-60598-1_2024_1.0.0.json</c>— y atarlo al nombre completo
    /// obligaría a tocar esto en cada publicación.
    /// </remarks>
    public static string RutaPlantilla()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var carpeta = Path.Combine(dir.FullName, "plantilla");
            if (Directory.Exists(carpeta)
                && Directory.GetFiles(carpeta, "plantilla-60598*.json").OrderBy(r => r).LastOrDefault() is { } ruta)
                return ruta;

            dir = dir.Parent;
        }
        throw new FileNotFoundException(
            "No se encuentra plantilla/plantilla-60598*.json subiendo desde " + AppContext.BaseDirectory);
    }

    public static string RutaEquipos()
        => Path.Combine(Path.GetDirectoryName(RutaPlantilla())!,
                        PlantillaEnsayos.Cargar(RutaPlantilla()).Meta.CatalogoEquipos!);

    public static string CarpetaDePlantillas() => Path.GetDirectoryName(RutaPlantilla())!;

    /// <summary>
    /// Todas las plantillas del laboratorio: 60598-1, 62031, 60529 e IK 62262. Los tests
    /// de integridad se pasan sobre todas, no solo sobre la de luminarias.
    /// </summary>
    public static IEnumerable<PlantillaEnsayos> TodasLasPlantillas()
        => Directory.GetFiles(CarpetaDePlantillas(), "plantilla-*.json")
                    .OrderBy(r => r)
                    .Select(PlantillaEnsayos.Cargar);

    /// <summary>
    /// Una norma instalada, por su <b>código corto</b> —el que sale en el nombre del
    /// fichero— y no por su id. El id lleva el año (<c>60598-1_2024</c>), así que
    /// buscar por él obligaría a repasar los tests cada vez que salga un año nuevo;
    /// el código corto identifica la norma y no cambia.
    /// </summary>
    /// <remarks>
    /// Si hay varias versiones de la misma norma, la de id más alto — que es la más
    /// reciente. Los tests hablan de la norma vigente salvo que digan otra cosa.
    /// </remarks>
    public static PlantillaEnsayos Norma(string codigo)
        => TodasLasPlantillas()
            .Where(p => p.Meta.CodigoParaFichero == codigo)
            .OrderBy(p => p.Meta.Id, StringComparer.Ordinal)
            .Last();

    /// <summary>Proyecto mínimo válido: 1 muestra, clase I, sin nada relleno.</summary>
    public static DatosProyecto ProyectoVacio(int muestras = 1)
        => new() { CodigoTomaDeNotas = "12345202601-00", CodigoServicio = "123452026", NumeroMuestras = muestras, Clase = Clase.I };

    public static MotorDeReglas Motor(DatosProyecto datos) => new(Plantilla, datos);
}
