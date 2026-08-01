using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LumNotas.App;

/// <summary>
/// Pinta en rojo lo que falta por rellenar. Se usa tanto para el borde de las cajas
/// obligatorias como para sus etiquetas; el color «cuando ya está» se pasa como
/// parámetro, porque no es el mismo en un borde que en un texto.
/// </summary>
public sealed class FaltaAColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Falta = new((Color)ColorConverter.ConvertFromString("#DC2626"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true) return Falta;

        var normal = parameter as string ?? "#C9CDD4";
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(normal));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
