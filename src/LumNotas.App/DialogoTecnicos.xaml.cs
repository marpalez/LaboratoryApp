using System.IO;
using System.Windows;
using System.Windows.Input;
using LumNotas.App.ViewModels;
using LumNotas.Storage;

namespace LumNotas.App;

/// <summary>
/// Editor de la lista de técnicos del laboratorio.
/// <para>
/// La diferencia importante entre las dos operaciones destructivas: <b>quitar</b> a un
/// técnico no toca ningún proyecto —el ensayo lo hizo esa persona—, mientras que
/// <b>corregir</b> su nombre sí se propaga, porque una errata no es una persona distinta
/// y si no se propagase, el filtro por técnico dejaría de encontrar sus proyectos.
/// </para>
/// </summary>
public partial class DialogoTecnicos : Window
{
    private readonly RepositorioDeProyectos _repositorio = new();

    private DialogoTecnicos() => InitializeComponent();

    /// <summary>
    /// Abre el editor. Devuelve las correcciones de nombre hechas —para aplicarlas también
    /// a las pestañas abiertas— o <c>null</c> si no se tocó nada.
    /// </summary>
    public static IReadOnlyList<(string Viejo, string Nuevo)>? Editar(Window propietario)
    {
        var dialogo = new DialogoTecnicos { Owner = propietario };
        dialogo.Preparar();
        dialogo.ShowDialog();
        return dialogo._huboCambios ? dialogo._renombrados : null;
    }

    private bool _huboCambios;
    private readonly List<(string Viejo, string Nuevo)> _renombrados = [];

    private void Preparar()
    {
        ServicioDeTecnicos.Recargar();

        Explicacion.Text = DondeSeGuarda();
        Explicacion.ToolTip = ServicioDeTecnicos.Carpeta();

        Refrescar();
    }

    /// <summary>
    /// Dónde va a parar la lista, dicho con el nombre de la carpeta que es.
    /// <para>
    /// Decía «la carpeta de proyectos» y eso <b>era mentira desde que las dos carpetas se
    /// separaron</b>: la lista se guarda en la <b>compartida</b>, y solo cae a la de
    /// proyectos como respaldo. Es justo el texto que lee quien mantiene la lista para
    /// saber si lo que acaba de escribir lo va a ver alguien más, así que equivocarse aquí
    /// es peor que no decir nada.
    /// </para>
    /// <para>
    /// Son <b>tres</b> casos y antes se contaban dos: la compartida elegida, el respaldo en
    /// la de proyectos —que también comparte el laboratorio, pero conviene saber que se
    /// está usando— y ninguna de las dos, que es cuando el cambio se queda en este equipo.
    /// </para>
    /// </summary>
    private static string DondeSeGuarda()
    {
        if (!ServicioDeTecnicos.EsCompartida)
            return "No hay ninguna carpeta del laboratorio elegida, así que la lista se guarda solo en "
                 + "este equipo y no la verá nadie más. Elígela en "
                 + "«Configuración | Carpetas: tomas de notas y compartida».";

        var compartida = ServicioDeCarpetas.CompartidaElegida;

        if (!string.IsNullOrWhiteSpace(compartida)
            && string.Equals(compartida, ServicioDeTecnicos.Carpeta(), StringComparison.OrdinalIgnoreCase))
            return "La lista se guarda en la carpeta compartida, así que el cambio lo verá todo el laboratorio.";

        return "Todavía no hay carpeta compartida elegida: la lista se guarda en la de tomas de notas, que "
             + "también ve todo el laboratorio.";
    }

    private void Refrescar(string? seleccionar = null)
    {
        Lista.ItemsSource = ServicioDeTecnicos.Catalogo.Tecnicos.ToList();
        if (seleccionar is not null) Lista.SelectedItem = seleccionar;
    }

    private void AlCambiarSeleccion(object remitente, System.Windows.Controls.SelectionChangedEventArgs args)
    {
        var hay = Lista.SelectedItem is string;
        BotonRenombrar.IsEnabled = hay;
        BotonQuitar.IsEnabled = hay;

        if (Lista.SelectedItem is string elegido) Nombre.Text = elegido;
    }

    private void AlTeclearNombre(object remitente, KeyEventArgs args)
    {
        if (args.Key == Key.Enter) AlAnadir(remitente, args);
    }

    private void AlAnadir(object remitente, RoutedEventArgs args)
    {
        var nombre = Nombre.Text.Trim();

        if (nombre.Length == 0) { Avisar("Escribe el nombre del técnico."); return; }
        if (!ServicioDeTecnicos.Catalogo.Anadir(nombre)) { Avisar($"«{nombre}» ya está en la lista."); return; }

        Guardar();
        Nombre.Clear();
        Refrescar(nombre);
        Avisar(null);
    }

    private void AlQuitar(object remitente, RoutedEventArgs args)
    {
        if (Lista.SelectedItem is not string elegido) return;

        var respuesta = MessageBox.Show(
            $"¿Quitar a {elegido} de la lista?\n\nLas tomas de notas que ya lo tengan no cambian.",
            "Quitar técnico", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        ServicioDeTecnicos.Catalogo.Quitar(elegido);
        Guardar();
        Nombre.Clear();
        Refrescar();
        Avisar($"«{elegido}» ya no aparece en los desplegables. Sus tomas de notas siguen a su nombre.");
    }

    private void AlRenombrar(object remitente, RoutedEventArgs args)
    {
        if (Lista.SelectedItem is not string viejo) return;

        var nuevo = Nombre.Text.Trim();

        if (nuevo.Length == 0) { Avisar("Escribe el nombre corregido."); return; }
        if (nuevo == viejo) { Avisar("El nombre no ha cambiado."); return; }

        var respuesta = MessageBox.Show(
            $"¿Corregir «{viejo}» por «{nuevo}»?\n\n" +
            "Se cambiará también en las tomas de notas que lo lleven.",
            "Corregir nombre", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        if (!ServicioDeTecnicos.Catalogo.Renombrar(viejo, nuevo))
        {
            Avisar($"No se pudo corregir: puede que «{nuevo}» ya esté en la lista.");
            return;
        }

        Guardar();
        _renombrados.Add((viejo, nuevo));

        // Los proyectos van después de la lista: si esto fallara, la lista ya está bien y
        // se puede volver a intentar corrigiendo el nombre otra vez.
        var carpeta = Ajustes.Cargar().CarpetaDeProyectos;
        var cambiados = 0;

        if (!string.IsNullOrWhiteSpace(carpeta) && Directory.Exists(carpeta))
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                cambiados = _repositorio.RenombrarTecnicoEnLaCarpeta(carpeta, viejo, nuevo);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        Nombre.Text = nuevo;
        Refrescar(nuevo);
        Avisar(cambiados == 0
            ? "Corregido en la lista. Ninguna toma de notas lo llevaba."
            : $"Corregido en la lista y en {cambiados} toma{(cambiados == 1 ? "" : "s")} de notas.");
    }

    private void Guardar()
    {
        try
        {
            ServicioDeTecnicos.Guardar();
            _huboCambios = true;
        }
        catch (Exception ex)
        {
            Avisar("No se pudo guardar la lista: " + ex.Message);
        }
    }

    private void Avisar(string? texto)
    {
        Aviso.Text = texto ?? "";
        Aviso.Visibility = texto is null ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
