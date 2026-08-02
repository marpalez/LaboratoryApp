using System.Text.Json;

namespace LumNotas.Core.Plantilla;

/// <summary>La versión del programa que el laboratorio da por buena.</summary>
public sealed class VersionPublicada
{
    public string Version { get; set; } = "";
    public DateTime PublicadoEl { get; set; }
    public string? Notas { get; set; }
    public string? PublicadoPor { get; set; }
}

/// <summary>
/// Avisa cuando un equipo se ha quedado con una versión vieja del programa.
/// <para>
/// Con la aplicación copiada en varios ordenadores, lo que no puede pasar es que alguien
/// trabaje meses con una versión antigua <b>sin saberlo</b>. Quien instala una versión
/// nueva la publica en la carpeta compartida, y los demás lo ven al arrancar.
/// </para>
/// <para>
/// Es un aviso, no un candado: el programa sigue funcionando. Bloquear el trabajo de un
/// laboratorio porque un fichero de OneDrive dice otra cosa sería peor que el problema.
/// </para>
/// </summary>
public static class ControlDeVersion
{
    public const string NombreDeFichero = "version.json";

    public static VersionPublicada? Leer(string carpeta)
    {
        try
        {
            var ruta = Path.Combine(carpeta, NombreDeFichero);
            if (!File.Exists(ruta)) return null;

            var leida = JsonSerializer.Deserialize<VersionPublicada>(File.ReadAllText(ruta));
            return string.IsNullOrWhiteSpace(leida?.Version) ? null : leida;
        }
        catch
        {
            // Un fichero roto no puede impedir arrancar: simplemente no hay aviso.
            return null;
        }
    }

    public static void Publicar(string carpeta, string version, string? notas, string? publicadoPor)
    {
        Directory.CreateDirectory(carpeta);

        var texto = JsonSerializer.Serialize(
            new VersionPublicada
            {
                Version = version,
                PublicadoEl = DateTime.Now,
                Notas = string.IsNullOrWhiteSpace(notas) ? null : notas.Trim(),
                PublicadoPor = publicadoPor
            },
            new JsonSerializerOptions { WriteIndented = true });

        var temporal = Path.Combine(carpeta, Path.GetRandomFileName());
        File.WriteAllText(temporal, texto, new System.Text.UTF8Encoding(false));

        var ruta = Path.Combine(carpeta, NombreDeFichero);
        if (File.Exists(ruta)) File.Replace(temporal, ruta, destinationBackupFileName: null);
        else File.Move(temporal, ruta);
    }

    /// <summary>
    /// Si la versión publicada es más nueva que la que se está ejecutando. Ante cualquier
    /// duda —un número que no se entiende— devuelve falso: es preferible no avisar que
    /// avisar en falso todos los días.
    /// </summary>
    public static bool HayMasNueva(string enEjecucion, VersionPublicada? publicada)
        => publicada is not null
           && Version.TryParse(Limpiar(enEjecucion), out var actual)
           && Version.TryParse(Limpiar(publicada.Version), out var disponible)
           && disponible > actual;

    /// <summary>Quita lo que cuelgue detrás del número, como «1.2.0+abc123» o «1.2.0-beta».</summary>
    private static string Limpiar(string version)
    {
        var texto = (version ?? "").Trim();
        var corte = texto.IndexOfAny(['-', '+', ' ']);
        return corte > 0 ? texto[..corte] : texto;
    }
}
