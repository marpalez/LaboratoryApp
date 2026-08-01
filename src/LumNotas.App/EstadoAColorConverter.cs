using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LumNotas.Core.Motor;

namespace LumNotas.App;

/// <summary>Semáforo del índice de apartados.</summary>
public sealed class EstadoAColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Falta = new((Color)ColorConverter.ConvertFromString("#DC2626"));
    private static readonly SolidColorBrush Completo = new((Color)ColorConverter.ConvertFromString("#16A34A"));
    private static readonly SolidColorBrush NoAplica = new((Color)ColorConverter.ConvertFromString("#9CA3AF"));
    private static readonly SolidColorBrush Neutro = new((Color)ColorConverter.ConvertFromString("#D1D5DB"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        EstadoApartado.FaltanDatos => Falta,
        EstadoApartado.Completo => Completo,
        EstadoApartado.NoAplica => NoAplica,
        _ => Neutro
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
