using System.IO;
using System.Windows;
using LumNotas.App.ViewModels;
using LumNotas.Core.Plantilla;

namespace LumNotas.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Sin esto, cualquier excepción no capturada cierra la ventana sin decir nada.
        DispatcherUnhandledException += (_, args) =>
        {
            MostrarFallo(args.Exception);
            args.Handled = true;   // la aplicación sigue viva: no se pierde el trabajo
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) MostrarFallo(ex);
        };

        try
        {
            // Comprueba que haya al menos una norma instalada antes de abrir la ventana.
            BuscarPlantilla();

            var modelo = new VentanaPrincipalViewModel();
            var ventana = new MainWindow(modelo);
            ventana.Show();
            PedirCarpetaLaPrimeraVez(modelo);

            // Si se ha arrancado con un fichero como argumento (doble clic sobre un
            // .lmnlab en la carpeta del servicio), se abre ese proyecto directamente.
            var fichero = e.Args.FirstOrDefault(a => File.Exists(a));
            if (fichero is not null) modelo.AbrirEnPestana(fichero);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "No se pudo arrancar", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// En un equipo recién instalado, pide las carpetas del laboratorio.
    /// <para>
    /// Se pregunta <b>una sola vez</b> y no se vuelve a insistir. Sin ellas, el técnico
    /// trabajaría con las normas de su equipo, su propia lista de técnicos y sin
    /// enterarse de las versiones nuevas — y todo eso en silencio, que es lo que se
    /// quiere evitar. Quien las deje sin elegir lo hará a sabiendas, y siempre puede
    /// ponerlas desde «Configuración».
    /// </para>
    /// </summary>
    private static void PedirCarpetaLaPrimeraVez(VentanaPrincipalViewModel modelo)
    {
        var ajustes = Ajustes.Cargar();
        if (ajustes.CarpetaYaPreguntada || !string.IsNullOrWhiteSpace(ajustes.CarpetaDeProyectos)) return;

        ajustes.CarpetaYaPreguntada = true;
        ajustes.Guardar();

        modelo.ElegirCarpetaDelLaboratorio.Execute(null);
    }

    /// <summary>
    /// Muestra el fallo y lo deja también en un fichero de texto, para poder pegarlo
    /// tal cual cuando haya que diagnosticar algo.
    /// </summary>
    private static void MostrarFallo(Exception ex)
    {
        var detalle = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{ex}";

        var registro = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LumNotas", "errores.log");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(registro)!);
            File.AppendAllText(registro, detalle + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            registro = "(no se pudo escribir el registro)";
        }

        MessageBox.Show(
            $"{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{Environment.NewLine}" +
            $"Detalle guardado en:{Environment.NewLine}{registro}{Environment.NewLine}{Environment.NewLine}" +
            $"{ex.StackTrace}",
            "Se ha producido un error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>
    /// Norma con la que arranca la aplicación: luminarias, que es la de uso más
    /// frecuente. Desde la ventana se puede cambiar a cualquier otra instalada.
    /// </summary>
    private static string BuscarPlantilla()
    {
        var normas = ServicioDePlantillas.Normas();
        if (normas.Count == 0)
            throw new FileNotFoundException("No hay ninguna toma de notas instalada en la carpeta 'plantilla'.");

        return (normas.FirstOrDefault(n => n.Id == "60598") ?? normas[0]).Ruta;
    }
}
