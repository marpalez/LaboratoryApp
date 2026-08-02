using System.Globalization;
using System.Windows;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;

namespace LumNotas.App;

/// <summary>
/// Tarifa y calendario laboral: los dos números con los que se calcula la carga. La
/// tarifa convierte el importe de una oferta en días de trabajo; la capacidad dice
/// cuántos caben en cada mes, que no es lo mismo en agosto que en marzo.
/// </summary>
public partial class DialogoCapacidad : Window
{
    private List<MesEditable> _meses = [];

    private DialogoCapacidad() => InitializeComponent();

    /// <summary>Devuelve si se ha guardado algo, para volver a calcular la tabla.</summary>
    public static bool Editar(Window propietario)
    {
        var dialogo = new DialogoCapacidad { Owner = propietario };
        dialogo.Rellenar();
        dialogo.ShowDialog();
        return dialogo._guardado;
    }

    private bool _guardado;

    private void Rellenar(CapacidadMensual? capacidad = null)
    {
        capacidad ??= ServicioDeCapacidad.Capacidad;

        Tarifa.Text = capacidad.EurosPorDia.ToString("0.##", CultureInfo.CurrentCulture);

        var cultura = EjeDeSemanas.CulturaDelLaboratorio;
        _meses = [.. Enumerable.Range(1, 12).Select(m => new MesEditable
        {
            Numero = m,
            Nombre = cultura.TextInfo.ToTitleCase(new DateTime(2026, m, 1).ToString("MMMM", cultura)),
            Dias = capacidad.Dias(m).ToString(CultureInfo.CurrentCulture)
        })];

        Meses.ItemsSource = _meses;
    }

    private void AlRestaurar(object remitente, RoutedEventArgs args)
    {
        Rellenar(new CapacidadMensual());
        Avisar(null);
    }

    private void AlGuardar(object remitente, RoutedEventArgs args)
    {
        if (!double.TryParse(Tarifa.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var tarifa)
            || tarifa <= 0)
        {
            Avisar("La tarifa tiene que ser un número mayor que cero.");
            return;
        }

        var dias = new List<int>();

        foreach (var mes in _meses)
        {
            if (!int.TryParse(mes.Dias.Trim(), out var valor) || valor < 1 || valor > 31)
            {
                Avisar($"Los días de {mes.Nombre} tienen que ser un número entre 1 y 31.");
                return;
            }

            dias.Add(valor);
        }

        ServicioDeCapacidad.Capacidad.EurosPorDia = tarifa;
        ServicioDeCapacidad.Capacidad.DiasPorMes = dias;

        try
        {
            ServicioDeCapacidad.Guardar();
            _guardado = true;
            Close();
        }
        catch (Exception ex)
        {
            Avisar("No se pudo guardar: " + ex.Message);
        }
    }

    private void AlCancelar(object remitente, RoutedEventArgs args) => Close();

    private void Avisar(string? texto)
    {
        Aviso.Text = texto ?? "";
        Aviso.Visibility = texto is null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>Un mes mientras se edita, con los días como texto para poder validarlos.</summary>
    private sealed class MesEditable
    {
        public int Numero { get; init; }
        public string Nombre { get; init; } = "";
        public string Dias { get; set; } = "";
    }
}
