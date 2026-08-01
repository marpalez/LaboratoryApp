using System.Windows;
using System.Windows.Input;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;

namespace LumNotas.App;

/// <summary>
/// Arrastrar una barra del calendario con el ratón: el centro la mueve entera, los
/// bordes cambian solo el inicio o solo el fin.
/// <para>
/// Aquí solo se traducen gestos a llamadas; <b>la aritmética está en
/// <see cref="TarjetaPlanViewModel"/></b>, que sí se puede probar. Se activa desde el
/// XAML con <c>local:ArrastreDeBarra.Activo="True"</c>.
/// </para>
/// </summary>
public static class ArrastreDeBarra
{
    /// <summary>Franja de cada extremo que cambia una sola fecha, en píxeles.</summary>
    private const double Borde = 7;

    /// <summary>
    /// Por debajo de este ancho no hay bordes: la barra entera es zona de mover. Si no,
    /// un servicio de dos días sería imposible de arrastrar sin estirarlo.
    /// </summary>
    private const double AnchoMinimoConBordes = 30;

    /// <summary>
    /// Cuánto hay que mover el ratón para que deje de ser un clic. Sin este margen,
    /// cualquier intento de abrir el diálogo movería el servicio un día.
    /// </summary>
    private const double Umbral = 4;

    private static FrameworkElement? _barra;
    private static TarjetaPlanViewModel? _tarjeta;
    private static Point _origen;
    private static bool _arrastrando;

    public static readonly DependencyProperty ActivoProperty = DependencyProperty.RegisterAttached(
        "Activo", typeof(bool), typeof(ArrastreDeBarra), new PropertyMetadata(false, AlActivar));

    public static void SetActivo(DependencyObject destino, bool valor) => destino.SetValue(ActivoProperty, valor);

    public static bool GetActivo(DependencyObject destino) => (bool)destino.GetValue(ActivoProperty);

    private static void AlActivar(DependencyObject destino, DependencyPropertyChangedEventArgs args)
    {
        if (destino is not FrameworkElement elemento) return;

        if ((bool)args.NewValue)
        {
            elemento.PreviewMouseLeftButtonDown += AlPulsar;
            elemento.PreviewMouseMove += AlMover;
            elemento.PreviewMouseLeftButtonUp += AlSoltar;
            elemento.LostMouseCapture += AlPerderElRaton;
        }
        else
        {
            elemento.PreviewMouseLeftButtonDown -= AlPulsar;
            elemento.PreviewMouseMove -= AlMover;
            elemento.PreviewMouseLeftButtonUp -= AlSoltar;
            elemento.LostMouseCapture -= AlPerderElRaton;
        }
    }

    private static void AlPulsar(object remitente, MouseButtonEventArgs args)
    {
        if (remitente is not FrameworkElement elemento ||
            elemento.DataContext is not TarjetaPlanViewModel tarjeta ||
            !tarjeta.SePuedeArrastrar) return;

        _barra = elemento;
        _tarjeta = tarjeta;
        _arrastrando = false;

        // El origen se mide contra el padre, que no se mueve: la barra sí lo hace.
        _origen = args.GetPosition(Padre(elemento));

        tarjeta.EmpezarArrastre(ModoDe(elemento, args.GetPosition(elemento).X));
        elemento.CaptureMouse();
    }

    private static void AlMover(object remitente, MouseEventArgs args)
    {
        if (remitente is not FrameworkElement elemento) return;

        if (_barra != elemento || _tarjeta is null)
        {
            // Sin arrastre en curso, el cursor anuncia qué haría cada zona.
            if (elemento.DataContext is TarjetaPlanViewModel t && t.SePuedeArrastrar)
                elemento.Cursor = ModoDe(elemento, args.GetPosition(elemento).X) == ModoArrastre.Mover
                    ? Cursors.SizeAll
                    : Cursors.SizeWE;
            return;
        }

        var recorrido = args.GetPosition(Padre(elemento)).X - _origen.X;

        if (!_arrastrando && Math.Abs(recorrido) < Umbral) return;

        _arrastrando = true;
        _tarjeta.Arrastrar(recorrido);
    }

    private static void AlSoltar(object remitente, MouseButtonEventArgs args)
    {
        if (remitente is not FrameworkElement elemento || _barra != elemento || _tarjeta is null) return;

        var tarjeta = _tarjeta;
        var huboArrastre = _arrastrando;

        Terminar(elemento);

        if (!huboArrastre) return;   // fue un clic: que siga su camino y abra el diálogo

        // Con arrastre, el clic del botón no debe llegar: abriría el diálogo al soltar.
        args.Handled = true;
        tarjeta.SoltarArrastre();
    }

    /// <summary>
    /// Si algo nos quita el ratón a media faena —otra ventana, un diálogo— la barra
    /// vuelve a donde estaba en vez de quedarse en un sitio que nadie eligió.
    /// </summary>
    private static void AlPerderElRaton(object remitente, MouseEventArgs args)
    {
        if (remitente is not FrameworkElement elemento || _barra != elemento) return;

        _tarjeta?.CancelarArrastre();
        Terminar(elemento);
    }

    private static void Terminar(FrameworkElement elemento)
    {
        if (elemento.IsMouseCaptured) elemento.ReleaseMouseCapture();
        _barra = null;
        _tarjeta = null;
        _arrastrando = false;
    }

    private static ModoArrastre ModoDe(FrameworkElement elemento, double x)
    {
        if (elemento.ActualWidth < AnchoMinimoConBordes) return ModoArrastre.Mover;
        if (x <= Borde) return ModoArrastre.Inicio;
        if (x >= elemento.ActualWidth - Borde) return ModoArrastre.Fin;
        return ModoArrastre.Mover;
    }

    private static IInputElement Padre(FrameworkElement elemento)
        => elemento.Parent as IInputElement ?? elemento;
}
