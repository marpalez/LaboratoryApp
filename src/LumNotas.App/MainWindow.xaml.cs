using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LumNotas.App.ViewModels;
using LumNotas.Storage;
using Microsoft.Win32;

namespace LumNotas.App;

public partial class MainWindow : Window
{
    private const string Filtro = "Proyecto de toma de notas (*.lumproj)|*.lumproj|Todos los ficheros (*.*)|*.*";

    public MainWindow(VentanaPrincipalViewModel modelo)
    {
        InitializeComponent();
        DataContext = modelo;

        // Los diálogos son de WPF, así que se los da la ventana al modelo.
        modelo.Servicios.PedirFicheroParaAbrir = () =>
        {
            var dialogo = new OpenFileDialog { Filter = Filtro, Title = "Abrir proyecto" };
            return dialogo.ShowDialog(this) == true ? dialogo.FileName : null;
        };

        modelo.Servicios.PedirFicheroParaGuardar = sugerido =>
        {
            var dialogo = new SaveFileDialog
            {
                Filter = Filtro,
                Title = "Guardar proyecto",
                FileName = sugerido,
                DefaultExt = RepositorioDeProyectos.Extension
            };
            return dialogo.ShowDialog(this) == true ? dialogo.FileName : null;
        };

        modelo.Servicios.PedirFicheroParaInforme = sugerido =>
        {
            var dialogo = new SaveFileDialog
            {
                Filter = "Informe de toma de notas (*.html)|*.html",
                Title = "Exportar la toma de notas",
                FileName = sugerido,
                DefaultExt = LumNotas.Report.ExportadorDeInforme.Extension
            };
            return dialogo.ShowDialog(this) == true ? dialogo.FileName : null;
        };

        // Abrir el PDF recién generado en el visor del sistema: el técnico lo ve y lo imprime.
        modelo.Servicios.AbrirEnElVisor = ruta =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch
            {
                // Si no hay visor de PDF instalado, el fichero está generado igualmente.
            }
        };

        modelo.Servicios.ConfirmarDescartarCambios = () => DialogoCambiosSinGuardar.Preguntar(this);

        // Cerrar la ventana es la forma más fácil de perder una toma de notas entera,
        // así que pregunta por cada pestaña con cambios.
        Closing += (_, args) => args.Cancel = !modelo.PuedeSalir();

        modelo.Gestion.PedirCarpeta = actual =>
        {
            var dialogo = new OpenFolderDialog
            {
                Title = "Carpeta donde el laboratorio guarda los proyectos",
                InitialDirectory = Directory.Exists(actual) ? actual : ""
            };
            return dialogo.ShowDialog(this) == true ? dialogo.FolderName : null;
        };

        modelo.Gestion.PedirPlanificacion = (titulo, actual)
            => DialogoPlanificacion.Preguntar(this, titulo, actual);

        InputBindings.Add(new KeyBinding(modelo.NuevaPestana, Key.T, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(modelo.Abrir, Key.O, ModifierKeys.Control));

        // Guardar y exportar van a la pestaña de delante, que cambia sola.
        InputBindings.Add(new KeyBinding(new Comando(
            () => modelo.ActivoDocumento?.Guardar.Execute(null)), Key.S, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new Comando(
            () => modelo.ActivoDocumento?.ExportarInforme.Execute(null)), Key.P, ModifierKeys.Control));
    }

    /// <summary>
    /// Al pinchar una sección se despliega o se pliega; al pinchar un apartado se muestra
    /// su formulario. Se hace aquí porque <c>TreeView.SelectedItem</c> es de solo lectura.
    /// </summary>
    private void AlSeleccionarNodo(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // El árbol vive dentro de la pestaña, así que el documento se saca de él y no
        // del DataContext de la ventana.
        if (sender is not TreeView arbol || arbol.DataContext is not DocumentoViewModel documento) return;

        switch (e.NewValue)
        {
            case SeccionViewModel seccion:
                if (arbol.ItemContainerGenerator.ContainerFromItem(seccion) is TreeViewItem nodo)
                    nodo.IsExpanded = !nodo.IsExpanded;
                break;

            case BloqueViewModel or ProyectoViewModel:
                documento.PanelActual = e.NewValue;
                break;
        }
    }

    private bool _corrigiendoVista;

    /// <summary>
    /// Al enfocar un control, WPF pide llevarlo a la vista y el panel se desplaza también
    /// en horizontal. En la tabla de muestras eso significaba que pulsar una casilla movía
    /// la vista a otra columna. Aquí se rehace la petición con un rectángulo sin ancho:
    /// el panel sigue subiendo o bajando para enseñar el control —que es lo que hace falta
    /// al tabular entre campos— pero deja de moverse de lado.
    /// </summary>
    private void AlPedirLlevarALaVista(object sender, RequestBringIntoViewEventArgs e)
    {
        // La llamada de abajo vuelve a levantar el evento: sin esto sería infinito.
        if (_corrigiendoVista || e.TargetObject is not FrameworkElement destino) return;

        e.Handled = true;
        _corrigiendoVista = true;
        try
        {
            destino.BringIntoView(new Rect(0, 0, 0, destino.ActualHeight));
        }
        finally
        {
            _corrigiendoVista = false;
        }
    }

}
