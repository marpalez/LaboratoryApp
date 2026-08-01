using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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

    /// <summary>Franja del borde en la que el calendario empieza a desplazarse solo.</summary>
    private const double FranjaDeArrastre = 30;

    /// <summary>Píxeles que se desplaza el calendario en cada latido del arrastre.</summary>
    private const double Salto = 16;

    private static FrameworkElement? _barra;
    private static TarjetaPlanViewModel? _tarjeta;
    private static Point _origen;
    private static bool _arrastrando;

    /// <summary>El panel desplazable de las barras, para poder seguir la barra al arrastrarla.</summary>
    private static ScrollViewer? _pista;

    /// <summary>
    /// Late mientras dura el arrastre. Hace falta un temporizador y no basta con el
    /// movimiento del ratón: al llegar al borde el técnico se queda quieto esperando que
    /// el calendario avance, y sin latido no avanzaría nunca.
    /// </summary>
    private static readonly DispatcherTimer Latido = new() { Interval = TimeSpan.FromMilliseconds(40) };

    static ArrastreDeBarra() => Latido.Tick += (_, _) => Seguir();

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
        _pista = Ancestro<ScrollViewer>(elemento);

        // El origen se mide contra el padre, que no se mueve: la barra sí lo hace.
        _origen = args.GetPosition(Padre(elemento));

        tarjeta.EmpezarArrastre(ModoDe(elemento, args.GetPosition(elemento).X));
        elemento.CaptureMouse();
        Latido.Start();
    }

    /// <summary>
    /// Desplaza el calendario cuando el ratón llega al borde y vuelve a colocar la barra.
    /// Sin esto, con la ventana pequeña la barra se sale de la vista al arrastrarla y ya
    /// no se ve dónde se está soltando.
    /// </summary>
    private static void Seguir()
    {
        if (_barra is null || _tarjeta is null || _pista is null || !_arrastrando) return;

        var enPista = Mouse.GetPosition(_pista);

        var paso = enPista.X < FranjaDeArrastre ? -Salto
                 : enPista.X > _pista.ViewportWidth - FranjaDeArrastre ? Salto
                 : 0;

        if (paso == 0) return;

        _pista.ScrollToHorizontalOffset(_pista.HorizontalOffset + paso);

        // El padre se desplaza con el contenido, así que el recorrido crece solo.
        _tarjeta.Arrastrar(Mouse.GetPosition(Padre(_barra)).X - _origen.X);
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

        // El botón ya no puede disparar su Click: al soltar la captura arriba, WPF da el
        // clic por cancelado. Así que aquí se decide qué era el gesto y se actúa.
        args.Handled = true;

        if (huboArrastre) tarjeta.SoltarArrastre();
        else tarjeta.Planificar.Execute(null);   // fue un clic: se abre su configuración
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

    /// <summary>
    /// Cierra el gesto. <b>El orden importa:</b> soltar la captura levanta
    /// <c>LostMouseCapture</c> en el acto, así que hay que borrar el estado antes. Si no,
    /// el manejador de ese evento cree que seguimos a media faena, cancela el arrastre y
    /// la barra vuelve a su sitio justo antes de guardarla.
    /// </summary>
    private static void Terminar(FrameworkElement elemento)
    {
        Latido.Stop();
        _barra = null;
        _tarjeta = null;
        _pista = null;
        _arrastrando = false;

        if (elemento.IsMouseCaptured) elemento.ReleaseMouseCapture();
    }

    private static T? Ancestro<T>(DependencyObject nodo) where T : DependencyObject
    {
        for (var actual = VisualTreeHelper.GetParent(nodo); actual is not null;
             actual = VisualTreeHelper.GetParent(actual))
            if (actual is T encontrado) return encontrado;

        return null;
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
