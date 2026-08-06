using System.Globalization;
using System.Windows.Data;

namespace LumNotas.App;

/// <summary>
/// Cuánto ocupa el índice de secciones según lo ancha que esté la ventana.
/// <para>
/// Llevaba 360 fijos, y eso en la ventana más pequeña era más de la mitad de la pantalla:
/// el formulario se quedaba sin sitio y sus casillas se salían por la derecha. Ahora se
/// lleva una parte del ancho, con tope arriba —no crece indefinidamente en un monitor
/// grande, que dejaría el formulario perdido a la derecha— y suelo abajo, porque un índice
/// de 120 no deja leer «Sección 11 - Líneas de fuga».
/// </para>
/// <para>
/// Se ata al ancho de la <b>ventana</b> y no al del panel que lo contiene: el índice vive
/// dentro de ese panel, y medirse contra su propio contenedor es la clase de bucle que WPF
/// resuelve dejando el elemento en cero.
/// </para>
/// </summary>
public sealed class AnchoDelIndice : IValueConverter
{
    public const double Minimo = 200;
    public const double Maximo = 360;

    /// <summary>Parte del ancho de la ventana que se lleva el índice cuando cabe holgado.</summary>
    public const double Proporcion = 0.30;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double ancho || double.IsNaN(ancho) || ancho <= 0) return Maximo;

        return Math.Clamp(ancho * Proporcion, Minimo, Maximo);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
