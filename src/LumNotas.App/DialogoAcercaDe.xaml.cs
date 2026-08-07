using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Core.Plantilla;

namespace LumNotas.App;

/// <summary>
/// Qué versión se está ejecutando, con qué normas, y si el laboratorio ha publicado una
/// más nueva. Además de evitar que alguien trabaje meses con una versión vieja, es lo
/// que permite responder <b>con qué versión se registró cada ensayo</b>.
/// </summary>
public partial class DialogoAcercaDe : Window
{
    private DialogoAcercaDe() => InitializeComponent();

    public static void Mostrar(Window propietario)
    {
        var dialogo = new DialogoAcercaDe { Owner = propietario };
        dialogo.Refrescar();
        dialogo.ShowDialog();
    }

    private void Refrescar()
    {
        Nombre.Text = ServicioDeVersion.Nombre;
        Version.Text = "Versión " + ServicioDeVersion.EnEjecucion;

        var publicada = ServicioDeVersion.Publicada;

        if (ServicioDeVersion.HayMasNueva)
        {
            Pintar(0xFE, 0xF3, 0xC7, 0xFD, 0xE6, 0x8A);
            Titulo.Text = $"Hay una versión más nueva: {publicada!.Version}";
            Detalle.Text = $"Publicada el {publicada.PublicadoEl:dd/MM/yyyy}"
                           + (string.IsNullOrWhiteSpace(publicada.PublicadoPor) ? "" : $" por {publicada.PublicadoPor}")
                           + (string.IsNullOrWhiteSpace(publicada.Notas) ? "" : $".\n{publicada.Notas}");
        }
        else if (publicada is not null)
        {
            Pintar(0xDC, 0xFC, 0xE7, 0xBB, 0xF7, 0xD0);
            Titulo.Text = "Estás al día";
            Detalle.Text = $"El laboratorio tiene publicada la {publicada.Version}, del "
                           + $"{publicada.PublicadoEl:dd/MM/yyyy}.";
        }
        else
        {
            Pintar(0xF3, 0xF4, 0xF6, 0xE5, 0xE7, 0xEB);
            Titulo.Text = $"Todavía no hay ninguna versión publicada de {ServicioDeVersion.Nombre}";
            Detalle.Text = ServicioDeVersion.HayCarpetaCompartida
                ? "Publica esta versión si es la que deben utilizar el resto de equipos."
                : "Elige antes la carpeta de tomas de notas en «Planificación de TdN y servicios».";
        }

        var origen = ServicioDePlantillas.Origen;

        // Una por línea y con su designación completa. Con el nombre corto y todas
        // seguidas se leía «Luminarias v1.0.0 · Luminarias v1.0.0»: las dos plantillas de
        // la 60598 eran indistinguibles justo en la ventana que existe para saber qué hay
        // instalado. La versión sale del catálogo, que ya la trae — cargar las cinco
        // plantillas enteras solo para leer un número era trabajo de más.
        Normas.Text = string.Join("\n", CatalogoDeNormas.Disponibles(origen.Carpeta)
            .Select(n => $"· {n.ComoSeLlama}   (plantilla v{n.Version})"));

        OrigenNormas.Text = (origen.EsCompartida ? "De la carpeta del laboratorio: " : "De este equipo: ")
                            + origen.Carpeta;

        BotonPublicar.IsEnabled = ServicioDeVersion.HayCarpetaCompartida;
    }

    private void Pintar(byte fr, byte fg, byte fb, byte br, byte bg, byte bb)
    {
        Estado.Background = new SolidColorBrush(Color.FromRgb(fr, fg, fb));
        Estado.BorderBrush = new SolidColorBrush(Color.FromRgb(br, bg, bb));
    }

    private void AlPublicar(object remitente, RoutedEventArgs args)
    {
        var respuesta = MessageBox.Show(
            $"¿Publicar la versión {ServicioDeVersion.EnEjecucion} como la del laboratorio?\n\n"
            + "Se copia el programa entero a la carpeta compartida. Los demás equipos se "
            + "pondrán al día solos la próxima vez que lo abran.",
            "Publicar versión", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        try
        {
            // Copiar el programa entero tarda lo suyo: sin el reloj de espera parece
            // colgado, y publicar es de las cosas que no se repiten «por si acaso».
            Mouse.OverrideCursor = Cursors.Wait;

            if (ServicioDeVersion.PublicarEsta(Notas.Text) is not { } ficheros)
            {
                Avisar("No hay carpeta compartida donde publicarla.", error: true);
                return;
            }

            Refrescar();
            Avisar($"Publicada la {ServicioDeVersion.EnEjecucion}: {ficheros} ficheros copiados.", error: false);
        }
        catch (Exception ex)
        {
            Avisar("No se pudo publicar: " + ex.Message, error: true);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void Avisar(string texto, bool error)
    {
        Resultado.Foreground = new SolidColorBrush(error
            ? Color.FromRgb(0xB9, 0x1C, 0x1C)
            : Color.FromRgb(0x15, 0x80, 0x3D));
        Resultado.Text = texto;
        Resultado.Visibility = Visibility.Visible;
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
