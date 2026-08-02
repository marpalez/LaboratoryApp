using System.IO;
using System.Windows;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Core.Plantilla;

namespace LumNotas.App;

/// <summary>
/// Qué normas hay instaladas y de dónde salen. Sirve para lo que más puede doler en un
/// laboratorio acreditado: que dos equipos rellenen versiones distintas de la misma
/// norma sin que nadie se dé cuenta.
/// </summary>
public partial class DialogoPlantillas : Window
{
    private DialogoPlantillas() => InitializeComponent();

    public static void Mostrar(Window propietario)
    {
        var dialogo = new DialogoPlantillas { Owner = propietario };
        dialogo.Refrescar();
        dialogo.ShowDialog();
    }

    private void Refrescar()
    {
        var origen = ServicioDePlantillas.Origen;

        if (origen.EsCompartida)
        {
            Titulo.Text = "Se están usando las normas del laboratorio";
            Estado.Background = new SolidColorBrush(Color.FromRgb(0xDC, 0xFC, 0xE7));
            Estado.BorderBrush = new SolidColorBrush(Color.FromRgb(0xBB, 0xF7, 0xD0));
        }
        else
        {
            Titulo.Text = "Se están usando las normas de este equipo";
            Estado.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7));
            Estado.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFD, 0xE6, 0x8A));
        }

        Ruta.Text = origen.Carpeta;

        Aviso.Text = origen.Aviso ?? "";
        Aviso.Visibility = origen.HayAviso ? Visibility.Visible : Visibility.Collapsed;

        Normas.ItemsSource = CatalogoDeNormas.Disponibles(origen.Carpeta)
            .Select(n => new
            {
                n.Titulo,
                Version = "v" + PlantillaEnsayos.Cargar(n.Ruta).Meta.Version
            })
            .ToList();

        // Publicar solo tiene sentido si hay copia local y carpeta compartida a la que ir.
        BotonPublicar.IsEnabled = PlantillasCompartidas.LocalSiExiste() is not null
                                  && ServicioDeCarpetas.HayCompartida;
    }

    private void AlPublicar(object remitente, RoutedEventArgs args)
    {
        if (PlantillasCompartidas.LocalSiExiste() is not { } local) return;
        if (ServicioDeCarpetas.Compartida() is not { } proyectos) return;

        var respuesta = MessageBox.Show(
            $"Se copiarán las normas de este equipo a:\n\n{Path.Combine(proyectos, PlantillasCompartidas.NombreDeCarpeta)}\n\n"
            + "Pasarán a ser las que use todo el laboratorio, sustituyendo a las que hubiera allí.",
            "Publicar las normas", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        try
        {
            var copiados = PlantillasCompartidas.Publicar(local, proyectos);
            ServicioDePlantillas.Reconsiderar();
            Refrescar();

            Resultado.Text = $"Publicadas: {copiados} ficheros copiados. "
                             + "Los demás equipos las usarán la próxima vez que abran el programa.";
            Resultado.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            Resultado.Foreground = new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C));
            Resultado.Text = "No se pudieron publicar: " + ex.Message;
            Resultado.Visibility = Visibility.Visible;
        }
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
