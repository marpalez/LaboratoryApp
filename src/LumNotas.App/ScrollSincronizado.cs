using System.Windows;
using System.Windows.Controls;

namespace LumNotas.App;

/// <summary>
/// Hace que un <see cref="ScrollViewer"/> siga el desplazamiento horizontal de otro.
/// Lo usa la cabecera del calendario para mantenerse alineada con las barras.
/// <para>
/// <b>Por qué un ScrollViewer y no una transformación.</b> Antes la cabecera era un panel
/// desplazado con <c>TranslateTransform</c>. Funcionaba con la ventana maximizada y
/// fallaba con la ventana pequeña: cuando un elemento pide más ancho del que le dan,
/// WPF le aplica un recorte de maquetación, así que las semanas que no cabían en el
/// ancho disponible <b>no llegaban a dibujarse</b> y la cabecera se quedaba en blanco al
/// desplazarse. Dentro de un ScrollViewer el contenido se mide sin límite y eso no pasa.
/// </para>
/// </summary>
public static class ScrollSincronizado
{
    public static readonly DependencyProperty ConProperty = DependencyProperty.RegisterAttached(
        "Con", typeof(ScrollViewer), typeof(ScrollSincronizado), new PropertyMetadata(null, AlCambiar));

    public static void SetCon(DependencyObject destino, ScrollViewer? valor) => destino.SetValue(ConProperty, valor);

    public static ScrollViewer? GetCon(DependencyObject destino) => (ScrollViewer?)destino.GetValue(ConProperty);

    /// <summary>El manejador suscrito, guardado para poder quitarlo si cambia el maestro.</summary>
    private static readonly DependencyProperty ManejadorProperty = DependencyProperty.RegisterAttached(
        "Manejador", typeof(ScrollChangedEventHandler), typeof(ScrollSincronizado));

    private static void AlCambiar(DependencyObject destino, DependencyPropertyChangedEventArgs args)
    {
        if (destino is not ScrollViewer seguidor) return;

        if (args.OldValue is ScrollViewer anterior &&
            destino.GetValue(ManejadorProperty) is ScrollChangedEventHandler viejo)
            anterior.ScrollChanged -= viejo;

        if (args.NewValue is not ScrollViewer maestro)
        {
            destino.SetValue(ManejadorProperty, null);
            return;
        }

        void Seguir(object remitente, ScrollChangedEventArgs e)
            => seguidor.ScrollToHorizontalOffset(e.HorizontalOffset);

        maestro.ScrollChanged += Seguir;
        destino.SetValue(ManejadorProperty, (ScrollChangedEventHandler)Seguir);

        // Por si el maestro ya estaba desplazado cuando se enganchó.
        seguidor.ScrollToHorizontalOffset(maestro.HorizontalOffset);
    }
}
