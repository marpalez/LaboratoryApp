using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using LumNotas.Core.Despliegue;

namespace LumNotas.Lanzador;

/// <summary>
/// Lo que hay detrás del acceso directo. Mira si el laboratorio ha publicado una versión
/// más nueva, se la trae, y arranca el programa.
///
/// <para>
/// <b>Existe para que actualizar el laboratorio sea publicar en un sitio.</b> Antes había
/// que ir equipo por equipo: el programa avisaba de que había una versión nueva y no
/// tenía de dónde sacarla.
/// </para>
///
/// <para>
/// <b>Es la única pieza que se instala a mano</b>, y por eso hace lo mínimo. Todo lo que
/// pueda vivir dentro del programa vive dentro del programa, que se actualiza solo;
/// cambiar el lanzador obliga a dar la vuelta por los seis equipos otra vez.
/// </para>
///
/// <para>
/// <b>No tiene ventana.</b> Si todo va bien no se ve: arranca y se va. Solo aparece un
/// aviso cuando no hay nada que arrancar, que es el único caso en el que el técnico tiene
/// algo que hacer. Un aviso por cada arranque correcto se aprendería a cerrar sin leerlo.
/// </para>
/// </summary>
public static class Programa
{
    private const string Nombre = "LumenLab";

    [STAThread]
    public static int Main(string[] argumentos)
    {
        try
        {
            var puesta = RepartoDelPrograma.PonerAlDia(CarpetaCompartida(), CarpetaLocal());

            if (puesta.RutaDelExe is null || !File.Exists(puesta.RutaDelExe))
            {
                Avisar(
                    "No hay ninguna versión de LumenLab instalada en este equipo, y no se ha "
                    + "podido traer del laboratorio."
                    + Environment.NewLine + Environment.NewLine
                    + (puesta.Aviso ?? "")
                    + Environment.NewLine + Environment.NewLine
                    + "Avisa a quien lleve el programa.");
                return 1;
            }

            // Lo que no impide trabajar no interrumpe: queda anotado para poder mirarlo
            // cuando alguien pregunte por qué su equipo sigue con la versión de antes.
            if (puesta.Aviso is not null) Anotar(puesta.Aviso);
            if (puesta.SeHaActualizado) Anotar($"Actualizado a la {puesta.Version}.");

            // Se le pasan los argumentos tal cual: el doble clic sobre un .lmnlab llega
            // aquí primero, y si no se reenvían el fichero no se abre.
            var arranque = new ProcessStartInfo(puesta.RutaDelExe)
            {
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(puesta.RutaDelExe)!
            };

            foreach (var argumento in argumentos) arranque.ArgumentList.Add(argumento);

            Process.Start(arranque);
            return 0;
        }
        catch (Exception ex)
        {
            // Que falle el lanzador no puede dejar sin programa a nadie: se intenta abrir
            // lo que haya instalado, aunque sea viejo, antes de darse por vencido.
            Anotar(ex.ToString());

            if (RepartoDelPrograma.UltimaInstalada(CarpetaLocal()) is { } respaldo)
            {
                Process.Start(new ProcessStartInfo(
                    Path.Combine(respaldo, RepartoDelPrograma.NombreDelExe)) { UseShellExecute = false });
                return 0;
            }

            Avisar("No se ha podido abrir LumenLab." + Environment.NewLine + Environment.NewLine + ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Dónde guarda este equipo sus versiones. En <c>%LOCALAPPDATA%</c> y no en Archivos
    /// de programa <b>a propósito</b>: escribir ahí pide administrador, y entonces la
    /// actualización dejaría de ser automática — que es justo lo que se está montando.
    /// </summary>
    private static string CarpetaLocal() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Nombre, "versiones");

    /// <summary>
    /// La carpeta compartida sale de los ajustes del propio programa, que es donde el
    /// técnico ya la eligió. El lanzador <b>no pregunta nada</b>: en un equipo recién
    /// instalado no hay ajustes todavía, y ahí lo que corresponde es abrir el programa
    /// —que sí sabe preguntar— con lo que trajera el instalador.
    /// </summary>
    private static string? CarpetaCompartida()
    {
        try
        {
            var ruta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LumNotas", "ajustes.json");

            if (!File.Exists(ruta)) return null;

            using var documento = JsonDocument.Parse(File.ReadAllText(ruta));

            // Las dos, en el mismo orden que el programa: si no hay compartida elegida se
            // usa la de proyectos, para no romper a quien las tenga juntas.
            foreach (var clave in new[] { "CarpetaCompartida", "CarpetaDeProyectos" })
                if (documento.RootElement.TryGetProperty(clave, out var valor)
                    && valor.GetString() is { Length: > 0 } carpeta
                    && Directory.Exists(carpeta))
                    return carpeta;

            return null;
        }
        catch
        {
            // Unos ajustes ilegibles no son motivo para no abrir el programa.
            return null;
        }
    }

    private static void Anotar(string texto)
    {
        try
        {
            var registro = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Nombre, "lanzador.log");

            Directory.CreateDirectory(Path.GetDirectoryName(registro)!);
            File.AppendAllText(registro, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {texto}{Environment.NewLine}");
        }
        catch
        {
            // Sin registro se sigue: es para diagnosticar, no para funcionar.
        }
    }

    private static void Avisar(string texto)
        => MessageBox.Show(texto, Nombre, MessageBoxButton.OK, MessageBoxImage.Warning);
}
