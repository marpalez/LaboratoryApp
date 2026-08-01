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

        Explicacion.Text = ServicioDeTecnicos.EsCompartida
            ? "La lista se guarda en la carpeta de proyectos, así que el cambio lo verá todo el laboratorio."
            : "Todavía no hay carpeta de proyectos elegida, así que la lista se guarda solo en este equipo. "
              + "Elige la carpeta en «Gestión de proyectos» para compartirla.";

        Refrescar();
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
            $"¿Quitar a {elegido} de la lista?\n\nLos proyectos que ya lo tengan no cambian.",
            "Quitar técnico", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (respuesta != MessageBoxResult.OK) return;

        ServicioDeTecnicos.Catalogo.Quitar(elegido);
        Guardar();
        Nombre.Clear();
        Refrescar();
        Avisar($"«{elegido}» ya no aparece en los desplegables. Sus proyectos siguen a su nombre.");
    }

    private void AlRenombrar(object remitente, RoutedEventArgs args)
    {
        if (Lista.SelectedItem is not string viejo) return;

        var nuevo = Nombre.Text.Trim();

        if (nuevo.Length == 0) { Avisar("Escribe el nombre corregido."); return; }
        if (nuevo == viejo) { Avisar("El nombre no ha cambiado."); return; }

        var respuesta = MessageBox.Show(
            $"¿Corregir «{viejo}» por «{nuevo}»?\n\n" +
            "Se cambiará también en los proyectos que lo lleven.",
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
            ? "Corregido en la lista. Ningún proyecto lo llevaba."
            : $"Corregido en la lista y en {cambiados} proyecto{(cambiados == 1 ? "" : "s")}.");
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
