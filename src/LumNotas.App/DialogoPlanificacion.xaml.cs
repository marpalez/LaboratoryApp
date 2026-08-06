using System.Globalization;
using System.Windows;
using LumNotas.App.ViewModels;
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

        Importe.Text = actual.Importe?.ToString("0.##", CultureInfo.CurrentCulture) ?? "";
        Recibidas.IsChecked = actual.MuestrasRecibidas;
        Recepcion.SelectedDate = actual.RecepcionMuestras;
        Archivar.IsChecked = actual.Archivado;
        Grupo.Text = actual.Grupo ?? "";
        Bloquear.IsChecked = actual.FechasBloqueadas;

        ActualizarBloqueo();
        ActualizarZonaRecepcion();
        ActualizarSemanas();
        ActualizarTrabajo();
    }

    /// <summary>
    /// Enseña en qué se traduce el importe, que es el número con el que se planifica.
    /// Verlo mientras se teclea evita descubrir en la tabla de carga que había un cero de más.
    /// </summary>
    private void ActualizarTrabajo()
    {
        if (LeerImporte() is { } importe && importe > 0)
        {
            // En horas, que es como lo mide el laboratorio, y en jornadas, que es la
            // unidad de la tabla de carga.
            var capacidad = ServicioDeCapacidad.Capacidad;
            Trabajo.Text = $"≈ {capacidad.HorasDeTrabajo(importe):0.#} h "
                           + $"({capacidad.DiasDeTrabajo(importe):0.#} jornadas)";
            return;
        }

        Trabajo.Text = Importe.Text.Trim().Length == 0
            ? "Sin importe, este servicio no cuenta en la carga"
            : "No se entiende ese importe";
    }

    private double? LeerImporte()
    {
        var texto = Importe.Text.Trim().Replace("€", "").Trim();
        if (texto.Length == 0) return null;

        return double.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
               || double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valor)
            ? valor
            : null;
    }

    private void AlCambiarImporte(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => ActualizarTrabajo();

    /// <summary>
    /// El grupo se escribe a mano en cada toma de notas, así que una errata las deja sin
    /// enlazar. Aquí se recuerda cómo va a quedar guardado —sin mayúsculas ni espacios de
    /// más— para que se vea que «ANTAR 2504» y «antar2504» son el mismo trabajo.
    /// </summary>
    private void AlCambiarGrupo(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var escrito = Grupo.Text.Trim();

        AvisoGrupo.Text = escrito.Length == 0
            ? "Escribe el mismo nombre en las tomas de notas que sean del mismo trabajo —las familias de un servicio— y el calendario las enseñará en una sola barra. Las fechas y el importe se ponen solo en una de ellas."
            : $"Se enlazará con las que lleven «{escrito}». No hace falta escribirlo igual: "
              + "no se distinguen mayúsculas, espacios ni guiones.";
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
            _ => "Sin planificar"
        };

        BotonQuitarFechas.IsEnabled = Inicio.SelectedDate is not null || Fin.SelectedDate is not null;
        Aviso.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Devuelve el servicio a la banda de pendientes de planificar. Es lo contrario de
    /// arrastrar: quitar las fechas a mano en dos casillas de calendario es incómodo.
    /// </summary>
    private void AlQuitarFechas(object sender, RoutedEventArgs e)
    {
        Inicio.SelectedDate = null;
        Fin.SelectedDate = null;
        ActualizarSemanas();
    }

    private void AlBloquear(object sender, RoutedEventArgs e) => ActualizarBloqueo();

    /// <summary>
    /// Con el candado puesto, las casillas de fecha se apagan. <b>La casilla de bloquear
    /// no</b>: si se apagara con ella misma, no habría forma de volver a abrirlo.
    /// </summary>
    private void ActualizarBloqueo()
    {
        var abierto = Bloquear.IsChecked != true;

        Inicio.IsEnabled = abierto;
        Fin.IsEnabled = abierto;
        BotonQuitarFechas.IsEnabled = abierto;
    }

    private static int Semanas(DateTime fecha) => System.Globalization.ISOWeek.GetWeekOfYear(fecha);

    private void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (Inicio.SelectedDate is { } inicio && Fin.SelectedDate is { } fin && fin < inicio)
        {
            Avisar("La fecha de fin es anterior a la de inicio.");
            return;
        }

        // Con una sola fecha el servicio no se puede dibujar y se quedaría en un limbo:
        // ni planificado ni pendiente. O las dos, o ninguna.
        if (Inicio.SelectedDate is null ^ Fin.SelectedDate is null)
        {
            Avisar("Pon las dos fechas, o quítalas las dos con «Quitar fechas» "
                   + "para dejarlo pendiente de planificar.");
            return;
        }

        var importe = LeerImporte();

        if (Importe.Text.Trim().Length > 0 && importe is null)
        {
            Avisar("El importe no se entiende. Escribe solo el número, por ejemplo 2000.");
            return;
        }

        if (importe < 0)
        {
            Avisar("El importe no puede ser negativo.");
            return;
        }

        _resultado = new Planificacion
        {
            Inicio = Inicio.SelectedDate,
            Fin = Fin.SelectedDate,
            Estado = Estado.SelectedValue is EstadoDeProyecto estado ? estado : EstadoDeProyecto.PorHacer,
            RecepcionMuestras = Recibidas.IsChecked == true ? Recepcion.SelectedDate : null,
            Archivado = Archivar.IsChecked == true,
            Importe = importe,
            // Vacío es sin grupo, no un grupo llamado "".
            Grupo = Grupo.Text.Trim() is { Length: > 0 } grupo ? grupo : null,
            FechasBloqueadas = Bloquear.IsChecked == true
        };

        Close();
    }

    private void AlCancelar(object sender, RoutedEventArgs e) => Close();

    private void Avisar(string texto)
    {
        Aviso.Text = texto;
        Aviso.Visibility = Visibility.Visible;
    }
}
