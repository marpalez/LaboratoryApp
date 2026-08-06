using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Storage;
using Microsoft.Win32;

namespace LumNotas.App;

public partial class MainWindow : Window
{
    // Al abrir se enseñan también los de la extensión anterior —si no, un fichero que
    // está ahí no aparece en el diálogo y parece perdido—; al guardar no, porque no se
    // escribe ninguno nuevo con la vieja.
    private const string FiltroAbrir =
        "Toma de notas (*" + RepositorioDeProyectos.Extension + ";*" + RepositorioDeProyectos.ExtensionAnterior + ")"
        + "|*" + RepositorioDeProyectos.Extension + ";*" + RepositorioDeProyectos.ExtensionAnterior
        + "|Todos los ficheros (*.*)|*.*";

    private const string FiltroGuardar =
        "Toma de notas (*" + RepositorioDeProyectos.Extension + ")|*" + RepositorioDeProyectos.Extension
        + "|Todos los ficheros (*.*)|*.*";

    public MainWindow(VentanaPrincipalViewModel modelo)
    {
        InitializeComponent();
        DataContext = modelo;

        // Los diálogos son de WPF, así que se los da la ventana al modelo.
        modelo.Servicios.PedirFicheroParaAbrir = () =>
        {
            var dialogo = new OpenFileDialog { Filter = FiltroAbrir, Title = "Abrir toma de notas" };
            return dialogo.ShowDialog(this) == true ? dialogo.FileName : null;
        };

        modelo.Servicios.PedirFicheroParaGuardar = sugerido =>
        {
            var dialogo = new SaveFileDialog
            {
                Filter = FiltroGuardar,
                Title = "Guardar la toma de notas",
                FileName = sugerido,
                DefaultExt = RepositorioDeProyectos.Extension
            };
            return dialogo.ShowDialog(this) == true ? dialogo.FileName : null;
        };

        modelo.Servicios.PedirFicheroParaInforme = sugerido =>
        {
            var dialogo = new SaveFileDialog
            {
                Filter = "Toma de notas en HTML (*.html)|*.html",
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
        modelo.Servicios.PreguntarSiSeCierra = motivo => DialogoCerrarProyecto.Preguntar(this, motivo);

        // El mismo diálogo que abre el tablero: la planificación se edita en un solo sitio,
        // se entre por donde se entre.
        modelo.Servicios.PedirPlanificacion = (titulo, actual)
            => DialogoPlanificacion.Preguntar(this, titulo, actual);
        modelo.Servicios.EditarTecnicos = () => DialogoTecnicos.Editar(this);
        modelo.Servicios.EditarCapacidad = () => DialogoCapacidad.Editar(this);
        modelo.Servicios.VerPlantillas = () => DialogoPlantillas.Mostrar(this);
        modelo.Servicios.VerAcercaDe = () => DialogoAcercaDe.Mostrar(this);
        modelo.Servicios.ReportarProblema = () => DialogoReportarProblema.Mostrar(this);
        modelo.Servicios.CrearProyecto = carpeta => DialogoNuevoProyecto.Preguntar(
            this, new RepositorioDeProyectos(), carpeta,
            codigo => modelo.Servicios.ComprobarSiYaExiste?.Invoke(codigo, null));

        modelo.Servicios.ComprobarSiYaExiste = (codigo, rutaPropia) =>
        {
            var existentes = modelo.Gestion.BuscarPorCodigo(codigo, rutaPropia);
            if (existentes.Count == 0) return null;

            var respuesta = DialogoProyectoRepetido.Preguntar(this, codigo, existentes);

            // Abrirlo es cosa de la ventana, que es quien lleva las pestañas.
            if (respuesta == RespuestaRepetido.Abrir) modelo.AbrirEnPestana(existentes[0].Ruta);

            return respuesta;
        };

        // Lo que no ha salido bien. Interrumpe a propósito: sustituye a la franja del pie
        // de ventana, que se quitó (DD‑133), y por aquí pasan los cuatro únicos avisos de
        // que algo ha fallado. Uno que se pueda pasar por alto no serviría.
        modelo.Servicios.Avisar = texto => MessageBox.Show(
            this, texto, VentanaPrincipalViewModel.NombreDelPrograma,
            MessageBoxButton.OK, MessageBoxImage.Warning);

        // La carpeta se aplica a través del tablero, que es quien la guarda y avisa al
        // resto de la ventana de que hay que releerlo todo.
        modelo.Servicios.ElegirCarpetaDelLaboratorio = () => DialogoCarpeta.Mostrar(
            this,
            carpeta => modelo.Gestion.Carpeta = carpeta,
            modelo.AdoptarCarpetaCompartida);

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
    /// Pulsar el código de una fila del listado abre esa toma de notas en una pestaña.
    /// La ruta viaja en la propia fila, así que no hace falta buscarla otra vez.
    /// </summary>
    private void AlAbrirDeLaBbdd(object remitente, RoutedEventArgs args)
    {
        if (remitente is FrameworkElement origen
            && origen.DataContext is FilaDeBbdd fila
            && DataContext is VentanaPrincipalViewModel modelo)
            modelo.AbrirEnPestana(fila.Ruta);
    }

    /// <summary>
    /// Abre los filtros de gestión. Va en el código de la ventana y no como comando del
    /// modelo porque abrir una ventana es cosa de la interfaz; el diálogo trabaja contra
    /// el mismo <see cref="GestionViewModel"/> del botón, así que lo que se elija ahí
    /// filtra las tres vistas en el acto.
    /// </summary>
    private void AlAbrirFiltros(object remitente, RoutedEventArgs args)
    {
        if (remitente is FrameworkElement origen && origen.DataContext is GestionViewModel gestion)
            DialogoFiltros.Mostrar(this, gestion);
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

            case BloqueViewModel or ProyectoViewModel or PlanificacionViewModel:
                documento.PanelActual = e.NewValue;
                break;
        }
    }

    /// <summary>
    /// Al enfocar un control, WPF pide llevarlo a la vista y el panel se desplaza también
    /// en horizontal. En la tabla de muestras eso significaba que pulsar una casilla movía
    /// la vista a otra columna; en la cabecera, que marcar una norma añadida daba un salto
    /// de lado —su casilla ocupa todo el ancho del contenido, así que enseñarla entera
    /// obliga a recorrerlo—.
    /// <para>
    /// Se cancela la petición y <b>el desplazamiento vertical se hace a mano</b>. Antes se
    /// rehacía la petición con un rectángulo sin ancho, y eso quitaba media enfermedad pero
    /// no la otra mitad: ese rectángulo pide <b>el punto x=0 del control</b>, así que si el
    /// panel estaba desplazado a la derecha volvía de un salto a la izquierda. Tocando solo
    /// <see cref="ScrollViewer.VerticalOffset"/> no hay manera de que se mueva de lado.
    /// </para>
    /// <para>
    /// El vertical sí hace falta: es lo que enseña el campo al que se llega tabulando.
    /// </para>
    /// </summary>
    private void AlPedirLlevarALaVista(object sender, RequestBringIntoViewEventArgs e)
    {
        if (e.TargetObject is not FrameworkElement destino) return;

        // Se atiende desde dentro del ScrollViewer, no desde él: el que desplaza es el
        // ScrollContentPresenter de su plantilla, que queda por debajo en el árbol. Puesto
        // en el ScrollViewer, el evento ya venía atendido y la vista movida.
        e.Handled = true;

        if (sender is not DependencyObject nodo ||
            PanelQueLoContiene(nodo) is not { } panel ||
            !destino.IsDescendantOf(panel) || destino.ActualHeight <= 0) return;

        var caja = destino.TransformToAncestor(panel)
                          .TransformBounds(new Rect(0, 0, destino.ActualWidth, destino.ActualHeight));

        // Las coordenadas van contra el ScrollViewer entero, así que lo que se ve empieza
        // después de su relleno de arriba.
        var arriba = panel.Padding.Top;
        var abajo = arriba + panel.ViewportHeight;

        // Se corrige lo justo para que asome, y solo hacia arriba o hacia abajo. Si el
        // control es más alto que lo que se ve, manda su borde de arriba.
        if (caja.Top < arriba)
            panel.ScrollToVerticalOffset(panel.VerticalOffset + (caja.Top - arriba));
        else if (caja.Bottom > abajo)
            panel.ScrollToVerticalOffset(panel.VerticalOffset + (caja.Bottom - abajo));
    }

    private static ScrollViewer? PanelQueLoContiene(DependencyObject nodo)
    {
        for (var actual = VisualTreeHelper.GetParent(nodo); actual is not null;
             actual = VisualTreeHelper.GetParent(actual))
            if (actual is ScrollViewer panel) return panel;

        return null;
    }

}
