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

    public static string RutaPlantilla()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidata = Path.Combine(dir.FullName, "plantilla", "plantilla-60598.v1.json");
            if (File.Exists(candidata)) return candidata;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("No se encuentra plantilla/plantilla-60598.v1.json subiendo desde " + AppContext.BaseDirectory);
    }

    public static string RutaEquipos()
        => Path.Combine(Path.GetDirectoryName(RutaPlantilla())!, "equipos-60598.v1.json");

    public static string CarpetaDePlantillas() => Path.GetDirectoryName(RutaPlantilla())!;

    /// <summary>
    /// Todas las plantillas del laboratorio: 60598-1, 62031, 60529 e IK 62262. Los tests
    /// de integridad se pasan sobre todas, no solo sobre la de luminarias.
    /// </summary>
    public static IEnumerable<PlantillaEnsayos> TodasLasPlantillas()
        => Directory.GetFiles(CarpetaDePlantillas(), "plantilla-*.json")
                    .OrderBy(r => r)
                    .Select(PlantillaEnsayos.Cargar);

    /// <summary>Proyecto mínimo válido: 1 muestra, clase I, sin nada relleno.</summary>
    public static DatosProyecto ProyectoVacio(int muestras = 1)
        => new() { CodigoServicio = "123452026", NumeroMuestras = muestras, Clase = Clase.I };

    public static MotorDeReglas Motor(DatosProyecto datos) => new(Plantilla, datos);
}
