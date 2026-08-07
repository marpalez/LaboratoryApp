using System.IO;
using System.Reflection;
using LumNotas.Core.Despliegue;

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
    /// <summary>
    /// Cómo se llama el programa. Sale de <c>&lt;Product&gt;</c> en el <c>.csproj</c>, que es
    /// el único sitio donde está escrito: la portada, la ventana «Acerca de» y las
    /// propiedades del ejecutable dicen lo mismo sin tener que acordarse de tres sitios.
    /// </summary>
    public static string Nombre { get; } =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>()?.Product
        ?? "LumNotas";

    /// <summary>La versión de este ejecutable.</summary>
    public static string EnEjecucion { get; } =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? "desconocida";

    private static VersionPublicada? _publicada;
    private static DateTime _leidaA = DateTime.MinValue;

    /// <summary>
    /// Cada cuánto se vuelve a mirar la carpeta compartida. No se guardaba y se leía una
    /// sola vez por sesión: en el laboratorio hay quien no cierra el programa en toda la
    /// semana, y a ese «Acerca de» le seguía diciendo el lunes lo del lunes anterior.
    /// <para>
    /// Media hora y no cada vez, porque esto lo pide la portada al pintarse y la
    /// compartida está en OneDrive: preguntar por un fichero de red a cada repintado se
    /// nota.
    /// </para>
    /// </summary>
    private static readonly TimeSpan CadaCuanto = TimeSpan.FromMinutes(30);

    /// <summary>La que el laboratorio ha publicado, si hay carpeta compartida.</summary>
    public static VersionPublicada? Publicada
    {
        get
        {
            if (DateTime.Now - _leidaA < CadaCuanto) return _publicada;

            _leidaA = DateTime.Now;
            _publicada = ServicioDeCarpetas.Compartida() is { } carpeta ? ControlDeVersion.Leer(carpeta) : null;
            return _publicada;
        }
    }

    public static bool HayMasNueva => ControlDeVersion.HayMasNueva(EnEjecucion, Publicada);

    /// <summary>
    /// Sube <b>esta instalación</b> a la carpeta compartida y la marca como la buena.
    /// Devuelve cuántos ficheros se han copiado, o <c>null</c> si no hay compartida.
    /// <para>
    /// Se copia la carpeta desde la que se está ejecutando, que es exactamente lo que hace
    /// «Publicar las normas»: no hace falta ninguna máquina de compilación ni volver a
    /// generar nada — se reparte lo que este equipo ya está usando y funciona.
    /// </para>
    /// </summary>
    public static int? PublicarEsta(string? notas)
    {
        if (ServicioDeCarpetas.Compartida() is not { } carpeta) return null;

        var manifiesto = RepartoDelPrograma.Publicar(
            AppContext.BaseDirectory, carpeta, EnEjecucion, notas, Environment.UserName);

        _leidaA = DateTime.MinValue;
        return manifiesto.Ficheros.Count;
    }

    public static bool HayCarpetaCompartida => ServicioDeCarpetas.HayCompartida;

    /// <summary>Al cambiar de carpeta, la versión publicada es otra.</summary>
    public static void Olvidar() => _leidaA = DateTime.MinValue;
}
