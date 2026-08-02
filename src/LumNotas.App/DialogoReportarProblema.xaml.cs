using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using LumNotas.App.ViewModels;

namespace LumNotas.App;

/// <summary>
/// A quién avisar cuando algo falla, y con qué datos.
/// <para>
/// No manda nada por su cuenta: enviar correo desde el programa obligaría a llevar una
/// contraseña de servidor dentro del ejecutable, que es exactamente lo que se descartó
/// al hablar de recuperar contraseñas (DD‑67). Aquí solo se enseña la dirección y se
/// abre el programa de correo del equipo, que es quien tiene las credenciales.
/// </para>
/// </summary>
public partial class DialogoReportarProblema : Window
{
    /// <summary>
    /// Quien mantiene el programa. Está aquí y no en un fichero de configuración a
    /// propósito: si el correo dependiera de la carpeta compartida, un equipo mal
    /// configurado se quedaría sin saber a quién avisar justo cuando algo va mal.
    /// </summary>
    private const string CorreoDeContacto = "davidmarpalez@gmail.com";

    private DialogoReportarProblema() => InitializeComponent();

    public static void Mostrar(Window propietario)
    {
        var dialogo = new DialogoReportarProblema { Owner = propietario };
        dialogo.Rellenar();
        dialogo.ShowDialog();
    }

    /// <summary>Dónde deja <c>App</c> los fallos que no se pueden capturar.</summary>
    private static string RutaDelRegistro => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LumNotas", "errores.log");

    private void Rellenar()
    {
        Correo.Text = CorreoDeContacto;
        Datos.Text = Resumen();

        // Sin registro todavía no hay nada que abrir, y un botón que no hace nada
        // confunde más que uno que no está.
        BotonRegistro.IsEnabled = File.Exists(RutaDelRegistro);
    }

    /// <summary>
    /// Lo mínimo para poder reproducir un fallo. La versión es lo primero que hay que
    /// mirar: la mitad de los «a mí no me pasa» son dos equipos con versiones distintas.
    /// </summary>
    private static string Resumen() =>
        $"Programa:  {ServicioDeVersion.Nombre} {ServicioDeVersion.EnEjecucion}\n"
        + $"Equipo:    {Environment.MachineName}  ({Environment.UserName})\n"
        + $"Windows:   {Environment.OSVersion.VersionString}\n"
        + $"Fecha:     {DateTime.Now:yyyy-MM-dd HH:mm}\n"
        + $"Registro:  {RutaDelRegistro}";

    private void AlEscribir(object remitente, RoutedEventArgs args)
    {
        var asunto = Uri.EscapeDataString($"{ServicioDeVersion.Nombre} {ServicioDeVersion.EnEjecucion} — problema");
        var cuerpo = Uri.EscapeDataString(
            "Qué estaba haciendo:\n\n\nQué esperaba que pasara:\n\n\nQué pasó:\n\n\n"
            + "---- datos del equipo ----\n" + Resumen());

        try
        {
            Process.Start(new ProcessStartInfo($"mailto:{CorreoDeContacto}?subject={asunto}&body={cuerpo}")
            {
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Un equipo sin programa de correo asociado es normal en un laboratorio:
            // se copia la dirección y se escribe desde el correo del navegador.
            Avisar("No se pudo abrir el programa de correo. Copia la dirección y escribe desde donde uses el correo.",
                   error: true);
        }
    }

    private void AlCopiar(object remitente, RoutedEventArgs args)
        => Copiar(CorreoDeContacto, "Dirección copiada.");

    private void AlCopiarDatos(object remitente, RoutedEventArgs args)
        => Copiar(Resumen(), "Datos copiados: pégalos en el correo.");

    private void Copiar(string texto, string hecho)
    {
        try
        {
            Clipboard.SetText(texto);
            Avisar(hecho, error: false);
        }
        catch (Exception)
        {
            // El portapapeles lo puede tener tomado otro programa durante un instante.
            Avisar("No se pudo copiar. Selecciona el texto y usa Ctrl+C.", error: true);
        }
    }

    private void AlAbrirRegistro(object remitente, RoutedEventArgs args)
    {
        try
        {
            // Se abre el explorador con el fichero ya seleccionado, en vez de el fichero
            // mismo: así se puede arrastrar al correo como adjunto.
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{RutaDelRegistro}\"")
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Avisar("No se pudo abrir el registro: " + ex.Message, error: true);
        }
    }

    private void Avisar(string texto, bool error)
    {
        Aviso.Text = texto;
        Aviso.Foreground = new SolidColorBrush(error
            ? Color.FromRgb(0xB4, 0x53, 0x09)
            : Color.FromRgb(0x15, 0x80, 0x3D));
        Aviso.Visibility = Visibility.Visible;
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
