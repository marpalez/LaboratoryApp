using System.Windows;
using LumNotas.Core.Gestion;

namespace LumNotas.App;

/// <summary>
/// Fechas, estado y recepción de muestras de un servicio. Es lo único que escribe el
/// calendario, y no toca ningún dato de ensayo.
/// <para>
/// Las fechas se eligen con calendario y no se escriben a mano: en el laboratorio se
/// planifica por semanas y teclear «08/07» cuando se quería «07/08» pasa constantemente.
/// </para>
/// </summary>
public partial class DialogoPlanificacion : Window
{
    private Planificacion? _resultado;

    private DialogoPlanificacion() => InitializeComponent();

    /// <summary>Devuelve la planificación editada, o <c>null</c> si se canceló.</summary>
    public static Planificacion? Preguntar(Window propietario, string titulo, Planificacion actual)
    {
        var dialogo = new DialogoPlanificacion { Owner = propietario };
        dialogo.Rellenar(titulo, actual);
        dialogo.ShowDialog();
        return dialogo._resultado;
    }

    private void Rellenar(string titulo, Planificacion actual)
    {
        Titulo.Text = titulo;

        Inicio.SelectedDate = actual.Inicio;
        Fin.SelectedDate = actual.Fin;

        Estado.ItemsSource = Planificacion.Estados
            .Select(e => new { Valor = e, Etiqueta = Planificacion.EtiquetaDe(e) })
            .ToList();
        Estado.SelectedValue = actual.Estado;

        Recibidas.IsChecked = actual.MuestrasRecibidas;
        Recepcion.SelectedDate = actual.RecepcionMuestras;
        Archivar.IsChecked = actual.Archivado;

        ActualizarZonaRecepcion();
        ActualizarSemanas();
    }

    private void AlCambiarFecha(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => ActualizarSemanas();

    private void AlMarcarRecibidas(object sender, RoutedEventArgs e)
    {
        // Marcar la casilla sin poner fecha dejaría el dato a medias; se propone hoy.
        if (Recibidas.IsChecked == true && Recepcion.SelectedDate is null)
            Recepcion.SelectedDate = DateTime.Today;

        ActualizarZonaRecepcion();
    }

    private void ActualizarZonaRecepcion()
        => ZonaRecepcion.Visibility = Recibidas.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Enseña las semanas ISO de las fechas elegidas: el laboratorio planifica por
    /// semana («entra en la S32»), así que conviene verlo sin tener que contarlo.
    /// </summary>
    private void ActualizarSemanas()
    {
        Semana.Text = (Inicio.SelectedDate, Fin.SelectedDate) switch
        {
            ({ } a, { } b) when Semanas(a) == Semanas(b) => $"Semana {Semanas(a)}",
            ({ } a, { } b) => $"De la semana {Semanas(a)} a la {Semanas(b)}",
            ({ } a, null) => $"Empieza en la semana {Semanas(a)}",
            (null, { } b) => $"Termina en la semana {Semanas(b)}",
            _ => ""
        };

        Aviso.Visibility = Visibility.Collapsed;
    }

    private static int Semanas(DateTime fecha) => System.Globalization.ISOWeek.GetWeekOfYear(fecha);

    private void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (Inicio.SelectedDate is { } inicio && Fin.SelectedDate is { } fin && fin < inicio)
        {
            Aviso.Text = "La fecha de fin es anterior a la de inicio.";
            Aviso.Visibility = Visibility.Visible;
            return;
        }

        _resultado = new Planificacion
        {
            Inicio = Inicio.SelectedDate,
            Fin = Fin.SelectedDate,
            Estado = Estado.SelectedValue is EstadoDeProyecto estado ? estado : EstadoDeProyecto.PorHacer,
            RecepcionMuestras = Recibidas.IsChecked == true ? Recepcion.SelectedDate : null,
            Archivado = Archivar.IsChecked == true
        };

        Close();
    }

    private void AlCancelar(object sender, RoutedEventArgs e) => Close();
}
