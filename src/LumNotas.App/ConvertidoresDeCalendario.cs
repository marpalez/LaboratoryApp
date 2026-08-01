using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LumNotas.App;

/// <summary>
/// Coloca un elemento a tantos píxeles del borde izquierdo. En el calendario la
/// posición de cada barra se calcula en el modelo, no en la vista, y aquí solo se
/// traduce ese número a un margen.
/// </summary>
public sealed class DesplazamientoAMargen : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => new Thickness(value is double x && !double.IsNaN(x) ? x : 0, 0, 0, 0);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
