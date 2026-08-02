using System.IO;
using System.Reflection;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Qué versión del programa se está ejecutando y cuál da por buena el laboratorio.
/// <para>
/// Sirve para dos cosas: que nadie trabaje meses con una versión vieja sin saberlo, y
/// poder responder <b>con qué versión se registró cada ensayo</b>, que es parte de lo
/// que pide la ISO 17025 sobre validación de software.
/// </para>
/// </summary>
public static class ServicioDeVersion
{
    /// <summary>La versión de este ejecutable.</summary>
    public static string EnEjecucion { get; } =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? "desconocida";

    private static VersionPublicada? _publicada;
    private static bool _leida;

    /// <summary>La que el laboratorio ha publicado, si hay carpeta compartida.</summary>
    public static VersionPublicada? Publicada
    {
        get
        {
            if (_leida) return _publicada;

            _leida = true;
            _publicada = ServicioDeCarpetas.Compartida() is { } carpeta ? ControlDeVersion.Leer(carpeta) : null;
            return _publicada;
        }
    }

    public static bool HayMasNueva => ControlDeVersion.HayMasNueva(EnEjecucion, Publicada);

    /// <summary>Marca esta versión como la del laboratorio. Devuelve si se pudo.</summary>
    public static bool PublicarEsta(string? notas)
    {
        if (ServicioDeCarpetas.Compartida() is not { } carpeta) return false;

        ControlDeVersion.Publicar(carpeta, EnEjecucion, notas, Environment.UserName);
        _leida = false;
        return true;
    }

    public static bool HayCarpetaCompartida => ServicioDeCarpetas.HayCompartida;

    /// <summary>Al cambiar de carpeta, la versión publicada es otra.</summary>
    public static void Olvidar() => _leida = false;
}
