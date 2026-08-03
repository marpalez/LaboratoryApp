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

        AvisarDeLoQueFaltaPorPublicar();
    }

    /// <summary>
    /// Dice si en este equipo hay normas que el laboratorio todavía no tiene. Desde que
    /// se publica la primera tanda, el programa lee de la carpeta compartida y deja de
    /// mirar la local: añadir una norma aquí no producía <b>ninguna</b> señal — el fichero
    /// estaba, no aparecía, y nada explicaba por qué.
    /// </summary>
    private void AvisarDeLoQueFaltaPorPublicar()
    {
        var pendientes = PlantillasCompartidas.Comparar(
            PlantillasCompartidas.LocalSiExiste(), ServicioDeCarpetas.Compartida());

        if (!pendientes.HayAlgo)
        {
            SinPublicar.Visibility = Visibility.Collapsed;
            return;
        }

        var lineas = new List<string>();

        if (pendientes.Nuevas.Count > 0)
            lineas.Add("Sin publicar: " + string.Join(", ", pendientes.Nuevas));

        if (pendientes.MasNuevas.Count > 0)
            lineas.Add("Más nuevas aquí que en el laboratorio: " + string.Join(", ", pendientes.MasNuevas));

        lineas.Add(pendientes.Cuantas == 1
            ? "Hasta que se publique, la usa solo este equipo."
            : "Hasta que se publiquen, las usa solo este equipo.");

        SinPublicar.Text = string.Join("\n", lineas);
        SinPublicar.Visibility = Visibility.Visible;
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
