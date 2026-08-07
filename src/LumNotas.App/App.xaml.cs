using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;

namespace LumNotas.App;

public partial class App : Application
{
    private static VentanaPrincipalViewModel? _modelo;

    protected override void OnStartup(StartupEventArgs e)
    {
        FijarElIdioma();

        // Si ya hay una ventana abierta, se le pasa el fichero y esta se va.
        if (!UnaSolaInstancia.Reclamar(e.Args))
        {
            Shutdown();
            return;
        }

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
            _modelo = modelo;
            ventana.Show();

            // A partir de aquí, los dobles clics posteriores sobre un .lmnlab llegan por
            // aquí en vez de abrir un segundo programa.
            UnaSolaInstancia.Atender(Abrir);

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
    /// Abre un fichero que ha mandado otro arranque, y trae la ventana al frente. Lo llama
    /// el hilo que escucha, así que hay que saltar al de la interfaz.
    /// </summary>
    private static void Abrir(string ruta) => Current.Dispatcher.Invoke(() =>
    {
        _modelo?.AbrirEnPestana(ruta);

        if (Current.MainWindow is { } ventana)
        {
            if (ventana.WindowState == WindowState.Minimized) ventana.WindowState = WindowState.Normal;
            ventana.Activate();
        }
    });

    /// <summary>
    /// El programa es en español, lo pida el equipo o no.
    /// <para>
    /// No es cosmética: de la cultura salen <b>cómo se leen y se escriben las fechas y los
    /// números</b>, y esto guarda registros de ensayo. En un equipo configurado en inglés,
    /// un 08/07 se leería como el 7 de agosto en vez del 8 de julio.
    /// </para>
    /// <para>
    /// Son <b>dos culturas y hacen falta las dos</b>: la de formato decide cómo se escribe
    /// una fecha, y la de <b>interfaz</b> decide de qué idioma saca WPF sus propios textos.
    /// De la segunda venía el «Select a date» del selector de fechas — comprobado el
    /// 2026‑08‑07: el <c>Language</c> del elemento no lo cambia, la cultura de interfaz sí.
    /// </para>
    /// <para>
    /// Y el <c>Language</c> se pone igualmente, porque WPF lo trae cableado a
    /// <c>en-US</c> y de él dependen los nombres de los meses del calendario desplegable.
    /// <b>Tiene que ir antes de crear el primer elemento</b>: llamarlo después no da error,
    /// simplemente no hace nada.
    /// </para>
    /// </summary>
    private static void FijarElIdioma()
    {
        var cultura = EjeDeSemanas.CulturaDelLaboratorio;

        CultureInfo.CurrentCulture = cultura;
        CultureInfo.CurrentUICulture = cultura;
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultura.IetfLanguageTag)));
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
            Rotar(registro);
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
    /// Un fallo que se repite —uno dentro de un manejador que salta a cada tecla— escribe
    /// sin parar. Al pasar de medio mega, lo de antes se aparta a <c>.1</c> y se empieza de
    /// cero, así que quedan siempre los dos últimos tramos y nunca más de un mega en total.
    /// <para>
    /// Se guarda uno anterior y no solo el de ahora: un fallo suele traer detrás la
    /// avalancha que lo tapa, y sin el tramo anterior se pierde justo la primera vez, que
    /// es la que dice qué pasó.
    /// </para>
    /// </summary>
    private static void Rotar(string registro)
    {
        const long Maximo = 512 * 1024;

        var fichero = new FileInfo(registro);
        if (!fichero.Exists || fichero.Length < Maximo) return;

        var anterior = registro + ".1";
        if (File.Exists(anterior)) File.Delete(anterior);
        File.Move(registro, anterior);
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
