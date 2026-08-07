using System.Text.Json;

namespace LumNotas.Core.Despliegue;

/// <summary>Un fichero del reparto y lo que tiene que ocupar.</summary>
public sealed record FicheroRepartido(string Nombre, long Bytes);

/// <summary>Qué lleva una versión publicada. Se escribe junto a ella, dentro de su carpeta.</summary>
public sealed class Manifiesto
{
    public string Version { get; set; } = "";
    public DateTime CreadoEl { get; set; }
    public List<FicheroRepartido> Ficheros { get; set; } = [];
}

/// <summary>Cómo ha quedado el equipo después de mirar si hay versión nueva.</summary>
/// <param name="RutaDelExe">Lo que hay que arrancar. <c>null</c> si no hay nada que arrancar.</param>
/// <param name="Version">La versión que se va a arrancar.</param>
/// <param name="SeHaActualizado">Si se ha traído una versión nueva en esta pasada.</param>
/// <param name="Aviso">
/// Por qué no se ha podido actualizar, cuando corresponda. <b>No impide arrancar</b>: se
/// arranca con lo que ya había.
/// </param>
public sealed record Puesta(string? RutaDelExe, string? Version, bool SeHaActualizado, string? Aviso);

/// <summary>
/// Reparte el programa por la carpeta compartida: se instala en un equipo, se publica, y
/// los demás se ponen al día solos al abrir.
///
/// <para>
/// <b>La compartida es el almacén, no el sitio desde donde se ejecuta.</b> Arrancar el
/// <c>.exe</c> directamente de OneDrive bloquea el fichero —y entonces no se puede
/// publicar encima—, falla sin conexión, y con Archivos a Petición el ejecutable puede
/// estar solo en la nube. Por eso cada equipo se queda su copia local.
/// </para>
///
/// <para>
/// <b>OneDrive no sincroniza en orden</b>, así que no basta con escribir el marcador el
/// último: puede llegar <c>version.json</c> diciendo «1.0.1» antes que la carpeta de la
/// 1.0.1 entera. De ahí el manifiesto: antes de cambiar nada se comprueba que estén
/// todos los ficheros y que ocupen lo que decían. Si no cuadra, <b>no se toca nada y se
/// arranca con lo de siempre</b>; mañana volverá a intentarlo.
/// </para>
///
/// <para>
/// Se comprueba el <b>tamaño y no una huella</b>. Es lo que caza el fallo real —un
/// fichero a medio sincronizar o que no ha llegado—, y una huella daría a entender una
/// garantía contra manipulación que aquí no la da el programa sino los permisos de la
/// carpeta.
/// </para>
///
/// <para>
/// Se conservan las <b>dos últimas</b> versiones a los dos lados. Volver atrás es
/// reescribir <c>version.json</c> con el número anterior: sin guardar la anterior, no
/// habría a dónde volver.
/// </para>
/// </summary>
public static class RepartoDelPrograma
{
    /// <summary>Dentro de la carpeta compartida, dónde van las versiones.</summary>
    public const string CarpetaDeProgramas = "programa";

    public const string NombreDelManifiesto = "manifiesto.json";

    /// <summary>Lo que se arranca.</summary>
    public const string NombreDelExe = "LumenLab.exe";

    /// <summary>Cuántas versiones se guardan, contando la de ahora.</summary>
    public const int VersionesQueSeConservan = 2;

    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // Lo que no se reparte. Los símbolos de depuración no los usa el técnico y multiplican
    // por dos lo que hay que sincronizar; el icono ya va dentro del ejecutable.
    private static readonly string[] NoSeReparte = [".pdb"];

    // ------------------------------------------------------------ publicar

    /// <summary>
    /// Copia esta instalación a la carpeta compartida y la marca como la buena.
    /// <para>
    /// <b>El orden importa y es el único orden correcto</b>: primero los ficheros, luego
    /// el manifiesto de esa versión, y <b>al final</b> <c>version.json</c>. Cada paso deja
    /// el reparto en un estado en el que nadie se lleva nada a medias.
    /// </para>
    /// </summary>
    /// <param name="carpetaDeOrigen">De dónde se copia: la instalación que se está usando.</param>
    /// <param name="compartida">La carpeta compartida del laboratorio.</param>
    public static Manifiesto Publicar(
        string carpetaDeOrigen, string compartida, string version, string? notas, string? quien)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Una versión sin número no se puede publicar.", nameof(version));

        var destino = CarpetaDeLaVersion(compartida, version);

        // Republicar el mismo número tiene que dejar la carpeta como la de aquí y no una
        // mezcla de las dos: si sobra un fichero de un intento anterior, el manifiesto no
        // lo nombraría y los demás equipos se lo llevarían igual.
        if (Directory.Exists(destino)) Directory.Delete(destino, recursive: true);
        Directory.CreateDirectory(destino);

        var manifiesto = new Manifiesto { Version = version, CreadoEl = DateTime.Now };

        foreach (var origen in FicherosQueSeReparten(carpetaDeOrigen))
        {
            var relativa = Path.GetRelativePath(carpetaDeOrigen, origen);
            var copia = Path.Combine(destino, relativa);

            Directory.CreateDirectory(Path.GetDirectoryName(copia)!);
            File.Copy(origen, copia, overwrite: true);

            manifiesto.Ficheros.Add(new FicheroRepartido(Normalizar(relativa), new FileInfo(copia).Length));
        }

        File.WriteAllText(
            Path.Combine(destino, NombreDelManifiesto),
            JsonSerializer.Serialize(manifiesto, Opciones),
            new System.Text.UTF8Encoding(false));

        // Y ahora sí: el marcador que hace que los demás se muevan.
        ControlDeVersion.Publicar(compartida, version, notas, quien);

        Limpiar(Path.Combine(compartida, CarpetaDeProgramas), version);

        return manifiesto;
    }

    /// <summary>
    /// Qué se copia. Todo lo que hay en la instalación menos los símbolos de depuración,
    /// con sus subcarpetas — <c>plantilla/</c> viaja con el programa.
    /// </summary>
    public static IEnumerable<string> FicherosQueSeReparten(string carpeta)
        => Directory.EnumerateFiles(carpeta, "*", SearchOption.AllDirectories)
                    .Where(f => !NoSeReparte.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

    // ------------------------------------------------------------ comprobar

    public static string CarpetaDeLaVersion(string raiz, string version)
        => Path.Combine(raiz, CarpetaDeProgramas, version);

    public static Manifiesto? LeerManifiesto(string carpetaDeLaVersion)
    {
        try
        {
            var ruta = Path.Combine(carpetaDeLaVersion, NombreDelManifiesto);
            if (!File.Exists(ruta)) return null;

            var leido = JsonSerializer.Deserialize<Manifiesto>(File.ReadAllText(ruta), Opciones);
            return leido is { Ficheros.Count: > 0 } ? leido : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Si esa carpeta tiene <b>todo</b> lo que dice su manifiesto, con el tamaño que decía.
    /// Sin manifiesto se responde que no: media copia sin lista de la compra no se
    /// distingue de una copia entera.
    /// </summary>
    public static bool EstaCompleta(string carpetaDeLaVersion)
        => LeerManifiesto(carpetaDeLaVersion) is { } manifiesto && EstaCompleta(carpetaDeLaVersion, manifiesto);

    public static bool EstaCompleta(string carpeta, Manifiesto manifiesto)
        => manifiesto.Ficheros.All(f =>
        {
            var fichero = new FileInfo(Path.Combine(carpeta, f.Nombre));
            return fichero.Exists && fichero.Length == f.Bytes;
        });

    // ------------------------------------------------------------ ponerse al día

    /// <summary>
    /// Lo que hace el lanzador al arrancar: mira qué versión da por buena el laboratorio,
    /// se la trae si hace falta y dice qué hay que ejecutar.
    /// <para>
    /// <b>Nunca deja al técnico sin programa.</b> Si la compartida no está, si el reparto
    /// llegó a medias o si la copia falla, se arranca con lo que ya hubiera instalado y el
    /// motivo viaja en <see cref="Puesta.Aviso"/>.
    /// </para>
    /// </summary>
    /// <param name="compartida">La carpeta compartida, o <c>null</c> si no hay o no se llega.</param>
    /// <param name="carpetaLocal">Dónde guarda este equipo sus versiones.</param>
    public static Puesta PonerAlDia(string? compartida, string carpetaLocal)
    {
        var instalada = UltimaInstalada(carpetaLocal);

        if (string.IsNullOrWhiteSpace(compartida) || !Directory.Exists(compartida))
            return Arrancar(instalada, "No se llega a la carpeta compartida; se abre la versión de este equipo.");

        if (ControlDeVersion.Leer(compartida) is not { } publicada || string.IsNullOrWhiteSpace(publicada.Version))
            return Arrancar(instalada, "El laboratorio no ha publicado ninguna versión todavía.");

        if (instalada is not null && MismaVersion(instalada, publicada.Version))
            return Arrancar(instalada, null);

        var origen = CarpetaDeLaVersion(compartida, publicada.Version);

        if (LeerManifiesto(origen) is not { } manifiesto)
            return Arrancar(instalada,
                $"El laboratorio anuncia la {publicada.Version}, pero todavía no ha llegado entera. "
                + "Se abre la versión de este equipo y se volverá a intentar.");

        if (!EstaCompleta(origen, manifiesto))
            return Arrancar(instalada,
                $"La {publicada.Version} está a medio sincronizar. Se abre la versión de este equipo "
                + "y se volverá a intentar cuando OneDrive termine.");

        try
        {
            var destino = Copiar(origen, carpetaLocal, publicada.Version, manifiesto);
            Limpiar(carpetaLocal, publicada.Version);

            return new Puesta(Path.Combine(destino, NombreDelExe), publicada.Version, SeHaActualizado: true, null);
        }
        catch (Exception ex)
        {
            return Arrancar(instalada, $"No se pudo copiar la {publicada.Version}: {ex.Message}");
        }
    }

    /// <summary>
    /// Copia a una carpeta temporal y solo al final la deja con su nombre. Así una copia
    /// interrumpida —un corte de red, un apagón— no deja una versión a medias que la
    /// próxima vez se daría por buena.
    /// </summary>
    private static string Copiar(string origen, string carpetaLocal, string version, Manifiesto manifiesto)
    {
        var destino = Path.Combine(carpetaLocal, version);
        var temporal = destino + ".copiando";

        if (Directory.Exists(temporal)) Directory.Delete(temporal, recursive: true);
        Directory.CreateDirectory(temporal);

        foreach (var fichero in manifiesto.Ficheros)
        {
            var copia = Path.Combine(temporal, fichero.Nombre);
            Directory.CreateDirectory(Path.GetDirectoryName(copia)!);
            File.Copy(Path.Combine(origen, fichero.Nombre), copia, overwrite: true);
        }

        File.Copy(Path.Combine(origen, NombreDelManifiesto),
                  Path.Combine(temporal, NombreDelManifiesto), overwrite: true);

        // Comprobar la copia y no solo el original: lo que se va a ejecutar es esto.
        if (!EstaCompleta(temporal, manifiesto))
        {
            Directory.Delete(temporal, recursive: true);
            throw new IOException("La copia no ha quedado completa.");
        }

        if (Directory.Exists(destino)) Directory.Delete(destino, recursive: true);
        Directory.Move(temporal, destino);

        return destino;
    }

    /// <summary>
    /// La versión instalada más alta que esté entera. Una a medias no cuenta: arrancarla
    /// sería peor que no tener ninguna.
    /// </summary>
    public static string? UltimaInstalada(string carpetaLocal)
        => VersionesDe(carpetaLocal)
            .Where(v => File.Exists(Path.Combine(v.Carpeta, NombreDelExe)) && EstaCompleta(v.Carpeta))
            .OrderByDescending(v => v.Numero)
            .Select(v => v.Carpeta)
            .FirstOrDefault();

    /// <summary>Borra lo antiguo y deja las dos últimas, contando la recién puesta.</summary>
    private static void Limpiar(string raiz, string versionQueSeQueda)
    {
        if (!Directory.Exists(raiz)) return;

        var sobran = VersionesDe(raiz)
            .Where(v => !string.Equals(v.Texto, versionQueSeQueda, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.Numero)
            .Skip(VersionesQueSeConservan - 1);

        foreach (var vieja in sobran)
            try { Directory.Delete(vieja.Carpeta, recursive: true); }
            catch { /* la tendrá abierta alguien: se irá la próxima vez */ }
    }

    private static IEnumerable<(string Carpeta, string Texto, Version Numero)> VersionesDe(string raiz)
    {
        if (!Directory.Exists(raiz)) yield break;

        foreach (var carpeta in Directory.EnumerateDirectories(raiz))
        {
            var nombre = Path.GetFileName(carpeta);
            if (Version.TryParse(nombre, out var numero)) yield return (carpeta, nombre, numero);
        }
    }

    private static Puesta Arrancar(string? carpetaInstalada, string? aviso)
        => carpetaInstalada is null
            ? new Puesta(null, null, false, aviso ?? "No hay ninguna versión instalada en este equipo.")
            : new Puesta(Path.Combine(carpetaInstalada, NombreDelExe),
                         Path.GetFileName(carpetaInstalada), SeHaActualizado: false, aviso);

    private static bool MismaVersion(string carpetaInstalada, string publicada)
        => string.Equals(Path.GetFileName(carpetaInstalada), publicada, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Las rutas del manifiesto van con barra normal. Se escribe en Windows y se lee en
    /// Windows, pero un separador dependiente del sistema en un fichero que viaja es de
    /// las cosas que se descubren tarde y mal.
    /// </summary>
    private static string Normalizar(string relativa) => relativa.Replace('\\', '/');
}
