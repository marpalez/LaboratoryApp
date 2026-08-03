using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;
using Microsoft.Win32;

namespace LumNotas.App;

/// <summary>
/// Las dos carpetas de OneDrive del laboratorio.
/// <para>
/// Son dos y no una: los proyectos cuelgan de la carpeta de clientes, cada uno en su
/// rama —<c>clientes/antares/antar2504/01/tomadenotas/</c>—, y lo compartido (normas,
/// técnicos, tarifa, versión) vive en otro sitio.
/// </para>
/// <para>
/// Vive en «Configuración» y no solo en el tablero porque un técnico que no abra nunca
/// el tablero no las elegiría jamás, y trabajaría aislado sin enterarse.
/// </para>
/// </summary>
public partial class DialogoCarpeta : Window
{
    private Action<string>? _aplicarProyectos;
    private Action? _aplicarCompartida;

    private DialogoCarpeta() => InitializeComponent();

    /// <param name="aplicarProyectos">Guarda la carpeta de proyectos y reexplora.</param>
    /// <param name="aplicarCompartida">Relee técnicos, tarifa y versión.</param>
    public static void Mostrar(Window propietario, Action<string> aplicarProyectos, Action aplicarCompartida)
    {
        var dialogo = new DialogoCarpeta
        {
            Owner = propietario,
            _aplicarProyectos = aplicarProyectos,
            _aplicarCompartida = aplicarCompartida
        };

        dialogo.Refrescar();
        dialogo.ShowDialog();
    }

    private void Refrescar()
    {
        RefrescarProyectos();
        RefrescarCompartida();
    }

    private void RefrescarProyectos()
    {
        var carpeta = ServicioDeCarpetas.Proyectos;

        if (Directory.Exists(carpeta))
        {
            Pintar(EstadoProyectos, Verde);
            TituloProyectos.Text = "Carpeta elegida";

            var cuantos = Directory
                .EnumerateFiles(carpeta, "*" + RepositorioDeProyectos.Extension, SearchOption.AllDirectories)
                .Count();

            RutaProyectos.Text = $"{carpeta}\n{cuantos} proyecto{(cuantos == 1 ? "" : "s")} encontrado"
                                 + (cuantos == 1 ? "" : "s");
        }
        else if (!string.IsNullOrWhiteSpace(carpeta))
        {
            Pintar(EstadoProyectos, Rojo);
            TituloProyectos.Text = "La carpeta elegida no está accesible";
            RutaProyectos.Text = carpeta + "\n¿OneDrive sin conexión, o la carpeta se ha movido?";
        }
        else
        {
            Pintar(EstadoProyectos, Ambar);
            TituloProyectos.Text = "Todavía no hay carpeta de proyectos";
            RutaProyectos.Text = "El tablero y el calendario estarán vacíos hasta que la elijas.";
        }
    }

    private void RefrescarCompartida()
    {
        var elegida = ServicioDeCarpetas.CompartidaElegida;
        var enUso = ServicioDeCarpetas.Compartida();

        BotonUsarProyectos.IsEnabled = !string.IsNullOrWhiteSpace(elegida);

        if (!string.IsNullOrWhiteSpace(elegida) && !Directory.Exists(elegida))
        {
            Pintar(EstadoCompartida, Rojo);
            TituloCompartida.Text = "La carpeta compartida no está accesible";
            RutaCompartida.Text = elegida + "\n¿OneDrive sin conexión, o la carpeta se ha movido?";
        }
        else if (!string.IsNullOrWhiteSpace(elegida))
        {
            Pintar(EstadoCompartida, Verde);
            TituloCompartida.Text = "Carpeta elegida";
            RutaCompartida.Text = elegida;
        }
        else if (enUso is not null)
        {
            Pintar(EstadoCompartida, Ambar);
            TituloCompartida.Text = "Sin elegir: se está usando la de proyectos";
            RutaCompartida.Text = enUso;
        }
        else
        {
            Pintar(EstadoCompartida, Ambar);
            TituloCompartida.Text = "Todavía no hay carpeta compartida";
            RutaCompartida.Text = "El programa está trabajando solo con lo que hay en este equipo: "
                                  + "sus normas, su lista de técnicos y sin aviso de versiones nuevas.";
        }

        Contenido.Children.Clear();
        if (enUso is null) return;

        var normas = Directory.Exists(Path.Combine(enUso, PlantillasCompartidas.NombreDeCarpeta))
            ? CatalogoDeNormas.Disponibles(Path.Combine(enUso, PlantillasCompartidas.NombreDeCarpeta)).Count
            : 0;

        Linea(normas > 0
            ? $"Normas publicadas: {normas}"
            : "Normas publicadas: ninguna — se usan las de este equipo", normas > 0 ? "#15803D" : "#B45309");

        Linea(File.Exists(Path.Combine(enUso, CatalogoDeTecnicos.NombreDeFichero))
            ? $"Técnicos: {CatalogoDeTecnicos.Cargar(enUso).Tecnicos.Count}"
            : "Técnicos: sin fichero — se usa la lista de partida", "#4B5563");

        Linea(File.Exists(Path.Combine(enUso, CapacidadMensual.NombreDeFichero))
            ? $"Tarifa: {CapacidadMensual.Cargar(enUso).EurosPorHoraEfectivos:0.##} € por hora de trabajo"
            : $"Tarifa: sin fichero — se usan {new CapacidadMensual().EurosPorHoraEfectivos:0.##} € por hora", "#4B5563");

        Linea(ControlDeVersion.Leer(enUso) is { } version
            ? $"Versión publicada: {version.Version}"
            : "Versión publicada: ninguna", "#4B5563");
    }

    // ---- acciones ----------------------------------------------------------

    private void AlElegirProyectos(object remitente, RoutedEventArgs args)
    {
        if (Elegir("Carpeta que contiene todos los proyectos", ServicioDeCarpetas.Proyectos) is not { } carpeta)
            return;

        _aplicarProyectos?.Invoke(carpeta);
        Refrescar();
        AvisarSiCambianLasNormas();
    }

    private void AlElegirCompartida(object remitente, RoutedEventArgs args)
    {
        if (Elegir("Carpeta compartida del laboratorio", ServicioDeCarpetas.CompartidaElegida) is not { } carpeta)
            return;

        Ajustes.Actualizar(a => a.CarpetaCompartida = carpeta);
        _aplicarCompartida?.Invoke();
        Refrescar();
        AvisarSiCambianLasNormas();
    }

    private void AlUsarProyectos(object remitente, RoutedEventArgs args)
    {
        Ajustes.Actualizar(a => a.CarpetaCompartida = "");
        _aplicarCompartida?.Invoke();
        Refrescar();
        AvisarSiCambianLasNormas();
    }

    private string? Elegir(string titulo, string actual)
    {
        var dialogo = new OpenFolderDialog
        {
            Title = titulo,
            InitialDirectory = Directory.Exists(actual) ? actual : ""
        };

        return dialogo.ShowDialog(this) == true ? dialogo.FolderName : null;
    }

    /// <summary>
    /// Las normas se resuelven una vez por sesión: cambiarlas en caliente dejaría unas
    /// pestañas con una versión de la norma y otras con otra.
    /// </summary>
    private void AvisarSiCambianLasNormas()
    {
        var ahora = PlantillasCompartidas.Resolver(ServicioDeCarpetas.Compartida());
        if (ahora.Carpeta == ServicioDePlantillas.Origen.Carpeta) return;

        Aviso.Text = "Reinicia el programa para que las normas se lean de la carpeta nueva. "
                     + "Lo demás ya está actualizado.";
        Aviso.Visibility = Visibility.Visible;
    }

    // ---- pintura -----------------------------------------------------------

    private static readonly (byte, byte, byte, byte, byte, byte) Verde = (0xDC, 0xFC, 0xE7, 0xBB, 0xF7, 0xD0);
    private static readonly (byte, byte, byte, byte, byte, byte) Ambar = (0xFE, 0xF3, 0xC7, 0xFD, 0xE6, 0x8A);
    private static readonly (byte, byte, byte, byte, byte, byte) Rojo = (0xFE, 0xE2, 0xE2, 0xFE, 0xCA, 0xCA);

    private static void Pintar(Border ficha, (byte fr, byte fg, byte fb, byte br, byte bg, byte bb) color)
    {
        ficha.Background = new SolidColorBrush(Color.FromRgb(color.fr, color.fg, color.fb));
        ficha.BorderBrush = new SolidColorBrush(Color.FromRgb(color.br, color.bg, color.bb));
    }

    private void Linea(string texto, string color)
        => Contenido.Children.Add(new TextBlock
        {
            Text = texto,
            FontSize = 12,
            Margin = new Thickness(0, 2, 0, 2),
            TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)new BrushConverter().ConvertFromString(color)!
        });

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
