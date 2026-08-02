using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// De dónde lee la aplicación las normas. Manda la carpeta compartida del laboratorio;
/// la copia de este equipo es el respaldo para cuando OneDrive no esté accesible.
/// <para>
/// Se resuelve <b>una vez por sesión</b>: cambiar de carpeta de normas a media faena,
/// con proyectos abiertos que ya se han cargado con una versión, sería peor que
/// esperar al siguiente arranque.
/// </para>
/// </summary>
public static class ServicioDePlantillas
{
    private static OrigenDePlantillas? _origen;

    public static OrigenDePlantillas Origen =>
        _origen ??= PlantillasCompartidas.Resolver(ServicioDeCarpetas.Compartida());

    public static IReadOnlyList<NormaDisponible> Normas() => CatalogoDeNormas.Disponibles(Origen.Carpeta);

    /// <summary>Tras publicar o cambiar de carpeta, vuelve a decidir de dónde se lee.</summary>
    public static void Reconsiderar() => _origen = null;
}
