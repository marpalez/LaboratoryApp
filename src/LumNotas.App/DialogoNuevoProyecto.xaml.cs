using System.IO;
using System.Windows;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;
using Microsoft.Win32;

namespace LumNotas.App;

/// <summary>
/// Dar de alta un proyecto para poder planificarlo. <b>Solo el nombre y el técnico 1 son
/// obligatorios</b> (DD‑85): el resto está en el diálogo por comodidad, no como peaje.
/// <para>
/// Es el camino del responsable, que planifica antes de que exista un solo dato de
/// ensayo. Antes había que elegir norma, escribir en la cabecera de la toma de notas y
/// pasar por «Guardar como»; ahora se rellenan dos casillas y el <c>.lumproj</c> está en
/// disco. Quién decide qué bloquea es <see cref="AltaDeProyecto"/>, en el núcleo y con
/// tests: esta ventana solo lo pregunta.
/// </para>
/// </summary>
public partial class DialogoNuevoProyecto : Window
{
    private readonly RepositorioDeProyectos _repositorio;
    private Func<string, RespuestaRepetido?>? _yaExiste;
    private string? _creado;

    private DialogoNuevoProyecto(RepositorioDeProyectos repositorio)
    {
        _repositorio = repositorio;
        InitializeComponent();
    }

    /// <summary>
    /// Devuelve la ruta del proyecto creado, o <c>null</c> si se canceló.
    /// </summary>
    /// <param name="carpetaSugerida">
    /// Dónde se abre el examinador. Es la carpeta de proyectos del laboratorio: dentro
    /// de ella cuelga la matrioska de clientes donde va a acabar el fichero.
    /// </param>
    /// <param name="yaExiste">
    /// Consulta si ese código ya está en la carpeta del laboratorio. Se pregunta también
    /// aquí, y no solo al guardar desde la toma de notas, porque **dar de alta dos veces
    /// el mismo servicio** es igual de fácil: dos responsables, o el mismo dos semanas
    /// después.
    /// </param>
    public static string? Preguntar(Window propietario, RepositorioDeProyectos repositorio,
                                    string? carpetaSugerida,
                                    Func<string, RespuestaRepetido?>? yaExiste = null)
    {
        var dialogo = new DialogoNuevoProyecto(repositorio) { Owner = propietario, _yaExiste = yaExiste };
        dialogo.Rellenar(carpetaSugerida);
        dialogo.ShowDialog();
        return dialogo._creado;
    }

    private void Rellenar(string? carpetaSugerida)
    {
        // Los desplegables empiezan en blanco: el responsable es una decisión, no un
        // valor por defecto que se cuele por no mirarlo.
        var tecnicos = ServicioDeTecnicos.Catalogo.Tecnicos;
        Tecnico1.ItemsSource = tecnicos;
        Tecnico2.ItemsSource = tecnicos;

        // Sin normas instaladas no se puede dar de alta nada, pero eso ya lo dice el
        // arranque: aquí basta con no reventar y dejar «Crear» apagado.
        try { Norma.ItemsSource = ServicioDePlantillas.Normas(); }
        catch (Exception) { }

        if (!string.IsNullOrWhiteSpace(carpetaSugerida) && Directory.Exists(carpetaSugerida))
            Carpeta.Text = carpetaSugerida;

        Revisar();
        Nombre.Focus();
    }

    private void AlEscribir(object remitente, RoutedEventArgs args) => Revisar();

    private void AlElegirTecnico(object remitente, RoutedEventArgs args) => Revisar();

    private void AlElegirNorma(object remitente, RoutedEventArgs args) => Revisar();

    /// <summary>Rojo mientras falte; el gris de siempre en cuanto esté.</summary>
    private static readonly Brush Falta = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
    private static readonly Brush Puesto = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51));

    /// <summary>
    /// Enciende «Crear» cuando están los datos que hacen falta, y pone en rojo el rótulo
    /// de los que faltan. Es el mismo criterio que las casillas obligatorias de la toma de
    /// notas: <b>rojo mientras esté vacío</b>, no rojo para siempre — así el rojo señala
    /// trabajo pendiente y no decora.
    /// </summary>
    private void Revisar()
    {
        if (BotonCrear is null) return;   // durante InitializeComponent aún no existe

        var norma = Norma.SelectedItem as NormaDisponible;

        var faltan = AltaDeProyecto
            .Faltan(Nombre.Text, Tecnico1.SelectedItem as string, norma?.Id)
            .ToList();

        // La carpeta no la exige el negocio pero sí la física: sin ella no hay dónde
        // escribir el fichero.
        var sinCarpeta = string.IsNullOrWhiteSpace(Carpeta.Text);
        if (sinCarpeta) faltan.Add("Carpeta");

        EtiquetaNombre.Foreground = Marca(AltaDeProyecto.CampoNombre, faltan);
        EtiquetaTecnico.Foreground = Marca(AltaDeProyecto.CampoTecnico, faltan);
        EtiquetaNorma.Foreground = Marca(AltaDeProyecto.CampoNorma, faltan);
        EtiquetaCarpeta.Foreground = sinCarpeta ? Falta : Puesto;

        BotonCrear.IsEnabled = faltan.Count == 0;

        if (faltan.Count == 0) Aviso.Visibility = Visibility.Collapsed;
        else Avisar("Falta " + string.Join(", ", faltan).ToLowerInvariant() + ".", error: false);
    }

    private static Brush Marca(string campo, List<string> faltan)
        => faltan.Contains(campo) ? Falta : Puesto;

    private void AlElegirCarpeta(object remitente, RoutedEventArgs args)
    {
        var dialogo = new OpenFolderDialog
        {
            Title = "Carpeta del proyecto",
            InitialDirectory = Directory.Exists(Carpeta.Text) ? Carpeta.Text : ""
        };

        if (dialogo.ShowDialog(this) == true) Carpeta.Text = dialogo.FolderName;
        Revisar();
    }

    private void AlCrear(object remitente, RoutedEventArgs args)
    {
        var nombre = Nombre.Text.Trim();
        var tecnico1 = Tecnico1.SelectedItem as string;
        var norma = Norma.SelectedItem as NormaDisponible;

        if (!AltaDeProyecto.SePuedeCrear(nombre, tecnico1, norma?.Id)) return;

        var plantilla = PlantillaEnsayos.Cargar(norma!.Ruta);

        // El nombre lo fija el laboratorio y sale de la norma y del código.
        var ruta = Path.Combine(Carpeta.Text, NombreDeTomaDeNotas.ConExtension(
            plantilla.Meta.Id, nombre, RepositorioDeProyectos.Extension));

        // Crear un proyecto no puede pisar otro: dos servicios del mismo cliente con el
        // mismo nombre es un descuido frecuente, y aquí se perdería el trabajo del otro.
        if (File.Exists(ruta))
        {
            Avisar($"Ya hay un proyecto llamado «{Path.GetFileName(ruta)}» en esa carpeta.", error: true);
            return;
        }

        // Y en cualquier otra carpeta del laboratorio: dar de alta dos veces el mismo
        // servicio es igual de fácil que empezarlo dos veces.
        if (_yaExiste?.Invoke(nombre) is { } respuesta && respuesta != RespuestaRepetido.CrearIgualmente)
        {
            // Si ha elegido abrir el que ya había, la ventana lo abre y aquí no hay
            // nada más que hacer.
            if (respuesta == RespuestaRepetido.Abrir) Close();
            return;
        }

        try
        {
            var datos = AltaDeProyecto.Crear(
                nombre, tecnico1!, Tecnico2.SelectedItem as string, plantilla);

            _repositorio.Guardar(datos, ruta, plantilla.Meta.Version);
            _creado = ruta;
            Close();
        }
        catch (Exception ex)
        {
            Avisar("No se pudo crear: " + ex.Message, error: true);
        }
    }

    private void Avisar(string texto, bool error)
    {
        Aviso.Text = texto;
        Aviso.Foreground = new SolidColorBrush(error
            ? Color.FromRgb(0xB4, 0x53, 0x09)
            : Color.FromRgb(0x6B, 0x72, 0x80));
        Aviso.Visibility = Visibility.Visible;
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
