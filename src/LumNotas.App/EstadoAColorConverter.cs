using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LumNotas.Core.Motor;

namespace LumNotas.App;

/// <summary>
/// Colores del índice de apartados. Sin parámetro da el color del punto de una fila;
/// con <c>Fondo</c> o <c>Texto</c> da los de la píldora de la sección, que lleva un
/// número dentro y por eso necesita más contraste que un punto.
/// <para>
/// Son tres: gris sin empezar, ámbar a medias, verde terminado. El ámbar es el que hace
/// falta de verdad —dice dónde se dejó el trabajo—, y para tenerlo hubo que enseñarle al
/// motor a mirar si hay algo escrito; ver <see cref="LumNotas.Core.Motor.EstadoApartado"/>.
/// </para>
/// <para>
/// Lo que falta por rellenar va en <b>gris, no en rojo</b>. Al abrir una toma de notas
/// nueva está todo por hacer, así que el rojo salía a la vez en todas las filas y en
/// todas las secciones —doce seguidas en la sección 4—: cuando algo grita siempre, deja
/// de avisar. El rojo se queda para dentro del formulario, en
/// <see cref="FaltaAColorConverter"/>, donde señala un campo concreto que falta.
/// </para>
/// </summary>
public sealed class EstadoAColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Hecho = Brocha("#16A34A");
    private static readonly SolidColorBrush AMedias = Brocha("#F59E0B");
    private static readonly SolidColorBrush Pendiente = Brocha("#CBD5E1");
    private static readonly SolidColorBrush Apagado = Brocha("#E5E7EB");

    private static readonly SolidColorBrush FondoHecho = Brocha("#DCFCE7");
    private static readonly SolidColorBrush FondoAMedias = Brocha("#FEF3C7");
    private static readonly SolidColorBrush FondoPendiente = Brocha("#F1F5F9");
    private static readonly SolidColorBrush TextoHecho = Brocha("#15803D");
    private static readonly SolidColorBrush TextoAMedias = Brocha("#B45309");
    private static readonly SolidColorBrush TextoPendiente = Brocha("#64748B");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value as EstadoApartado? ?? EstadoApartado.SinReglas;

        return $"{parameter}" switch
        {
            "Fondo" => estado switch
            {
                EstadoApartado.Completo => FondoHecho,
                EstadoApartado.Empezado => FondoAMedias,
                _ => FondoPendiente
            },
            "Texto" => estado switch
            {
                EstadoApartado.Completo => TextoHecho,
                EstadoApartado.Empezado => TextoAMedias,
                _ => TextoPendiente
            },
            _ => estado switch
            {
                EstadoApartado.Completo => Hecho,
                EstadoApartado.Empezado => AMedias,
                EstadoApartado.FaltanDatos => Pendiente,
                _ => Apagado
            }
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static SolidColorBrush Brocha(string color)
        => new((Color)ColorConverter.ConvertFromString(color));
}
