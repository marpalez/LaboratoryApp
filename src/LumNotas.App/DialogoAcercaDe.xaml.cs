using System.Windows;
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
            Titulo.Text = "Todavía no hay ninguna versión publicada";
            Detalle.Text = ServicioDeVersion.HayCarpetaCompartida
                ? "Publica esta si es la que debe usar el laboratorio."
                : "Elige antes la carpeta de proyectos en «Gestión de proyectos».";
        }

        var origen = ServicioDePlantillas.Origen;
        Normas.Text = string.Join("   ·   ", CatalogoDeNormas.Disponibles(origen.Carpeta)
            .Select(n => $"{n.TituloCorto} v{PlantillaEnsayos.Cargar(n.Ruta).Meta.Version}"));

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
            + "Los equipos que sigan con una anterior verán un aviso al arrancar.",
            "Publicar versión", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        try
        {
            if (!ServicioDeVersion.PublicarEsta(Notas.Text))
            {
                Avisar("No hay carpeta de proyectos donde publicarla.", error: true);
                return;
            }

            Refrescar();
            Avisar($"Publicada la {ServicioDeVersion.EnEjecucion}.", error: false);
        }
        catch (Exception ex)
        {
            Avisar("No se pudo publicar: " + ex.Message, error: true);
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
