using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LumNotas.App.ViewModels;

/// <summary>MVVM mínimo, sin dependencias externas.</summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Notificar([CallerMemberName] string? propiedad = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));

    protected bool Establecer<T>(ref T campo, T valor, [CallerMemberName] string? propiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor)) return false;
        campo = valor;
        Notificar(propiedad);
        return true;
    }
}

/// <summary>
/// Algo que se puede señalar en el índice de secciones.
/// <para>
/// Existe porque <c>TreeView.SelectedItem</c> es de <b>solo lectura</b>: no se le puede
/// decir al árbol qué marcar. La única forma de mandar sobre lo señalado es que cada nodo
/// lleve su propio interruptor y que <c>TreeViewItem.IsSelected</c> se ate a él en los dos
/// sentidos.
/// </para>
/// <para>
/// Sin esto, el panel y el índice se separaban en cuanto se salía por un botón de la barra
/// —«Planificación»—: el árbol seguía señalando «Datos del proyecto» mientras la pantalla
/// enseñaba otra cosa, y volver a pulsar el nodo ya señalado <b>no hacía nada</b>, porque
/// el árbol solo avisa cuando la selección <b>cambia</b>.
/// </para>
/// </summary>
public interface INodoDelIndice
{
    bool Seleccionado { get; set; }

    /// <summary>
    /// Cómo se llama el nodo para quien no ve la pantalla: lectores, control por voz y los
    /// guiones que manejan el programa para comprobarlo.
    /// <para>
    /// Hace falta porque <b>WPF solo deduce el nombre cuando la cabecera del nodo es una
    /// cadena</b>. Aquí son plantillas con rejilla, píldora y punto de color, así que no
    /// sabe cuál de las piezas es el nombre y se rinde: llama a <c>ToString()</c> del modelo
    /// de vista. El árbol entero se anunciaba como <c>LumNotas.App.ViewModels.SeccionViewModel</c>.
    /// </para>
    /// <para>
    /// <b>Va aquí y no en la plantilla</b>: el elemento que existe para la automatización es
    /// el <c>TreeViewItem</c>, no la rejilla de dentro. Poner el nombre dentro de la
    /// plantilla se lo pone a algo que nadie mira — se probó, y el árbol siguió diciendo el
    /// nombre de la clase.
    /// </para>
    /// <para>
    /// <b>No lleva cuentas ni estados que cambien.</b> «Sección 7 ‑ Construcción», no
    /// «Sección 7 ‑ Construcción 3/13»: un guion que busque el nodo por su nombre dejaría de
    /// encontrarlo en cuanto alguien rellenara un apartado. Lo que cambia va en el ToolTip.
    /// </para>
    /// </summary>
    string Rotulo { get; }
}

public sealed class Comando(Action ejecutar, Func<bool>? puedeEjecutar = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => puedeEjecutar?.Invoke() ?? true;

    public void Execute(object? parameter) => ejecutar();

    public void Revisar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>Comando que recibe un parámetro desde la vista (por ejemplo, una ruta).</summary>
public sealed class ComandoCon<T>(Action<T> ejecutar) : ICommand
{
    // Se delega en WPF, que ya reevalúa los comandos cuando cambia el foco o la entrada.
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => parameter is T;

    public void Execute(object? parameter)
    {
        if (parameter is T valor) ejecutar(valor);
    }
}
