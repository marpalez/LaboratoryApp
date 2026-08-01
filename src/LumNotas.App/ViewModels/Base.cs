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
