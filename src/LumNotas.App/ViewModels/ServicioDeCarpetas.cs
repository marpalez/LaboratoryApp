using System.IO;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Las dos carpetas de OneDrive con las que trabaja el laboratorio.
/// <para>
/// <b>No son la misma.</b> Los proyectos cuelgan de la carpeta de clientes, cada uno en
/// su rama —<c>clientes/antares/antar2504/01/tomadenotas/</c>—, mientras que las normas,
/// los técnicos, la tarifa y la versión viven aparte.
/// </para>
/// <para>
/// Si la compartida se deja en blanco se usa la de proyectos, para no romper una
/// instalación que las tenga juntas.
/// </para>
/// </summary>
public static class ServicioDeCarpetas
{
    /// <summary>Dónde están los proyectos. Se escanea entera, con sus subcarpetas.</summary>
    public static string Proyectos => Ajustes.Cargar().CarpetaDeProyectos;

    /// <summary>Lo que el técnico ha elegido como carpeta compartida, esté o no accesible.</summary>
    public static string CompartidaElegida => Ajustes.Cargar().CarpetaCompartida;

    /// <summary>
    /// De dónde salen normas, técnicos, tarifa y versión, o <c>null</c> si no hay ninguna
    /// accesible: ni la compartida ni, como respaldo, la de proyectos.
    /// </summary>
    public static string? Compartida()
    {
        var ajustes = Ajustes.Cargar();

        foreach (var candidata in new[] { ajustes.CarpetaCompartida, ajustes.CarpetaDeProyectos })
            if (!string.IsNullOrWhiteSpace(candidata) && Directory.Exists(candidata)) return candidata;

        return null;
    }

    public static bool HayCompartida => Compartida() is not null;
}
