using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using LumNotas.App.ViewModels;
using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;
using Microsoft.Win32;

namespace LumNotas.App;

/// <summary>
/// Dar de alta una toma de notas para poder planificarla.
/// <para>
/// La ventana está partida en dos: <b>lo obligatorio</b> —código, técnico 1, norma y
/// carpeta— y <b>lo opcional</b>, que es la planificación entera. Lo opcional va plegado
/// porque el alta es corta a propósito (DD‑83, DD‑85): el responsable crea la tarjeta en
/// cuatro segundos y el técnico rellena el resto cuando empiece. Desplegado de serie, esos
/// cuatro segundos se convertirían en un formulario.
/// </para>
/// <para>
/// Quién decide qué bloquea es <see cref="AltaDeProyecto"/>, en el núcleo y con tests:
/// esta ventana solo lo pregunta.
/// </para>
/// </summary>
public partial class DialogoNuevoProyecto : Window
{
    private readonly RepositorioDeProyectos _repositorio;
    private Func<string, RespuestaRepetido?>? _yaExiste;
    private string? _creado;

    /// <summary>
    /// Dónde abrir el examinador. No se escribe en la casilla: esa empieza vacía para que
    /// se vea que hay que elegir carpeta, pero buscarla desde cero en la matrioska de
    /// clientes serían varios clics de más.
    /// </summary>
    private string? _carpetaSugerida;

    private DialogoNuevoProyecto(RepositorioDeProyectos repositorio)
    {
        _repositorio = repositorio;
        InitializeComponent();
    }

    /// <summary>
    /// Devuelve la ruta de la toma de notas creada, o <c>null</c> si se canceló.
    /// </summary>
    /// <param name="carpetaSugerida">
    /// Dónde se abre el examinador. Es la carpeta de proyectos del laboratorio: dentro
    /// de ella cuelga la matrioska de clientes donde va a acabar el fichero.
    /// </param>
    /// <param name="yaExiste">
    /// Consulta si ese código ya está en la carpeta del laboratorio. Se pregunta también
    /// aquí, y no solo al guardar desde la toma de notas, porque **dar de alta dos veces
    /// la misma familia** es igual de fácil: dos responsables, o el mismo dos semanas
    /// después.
    /// </param>
    public static string? Preguntar(Window propietario, RepositorioDeProyectos repositorio,
                                    string? carpetaSugerida,
                                    Func<string, RespuestaRepetido?>? yaExiste = null)
    {
        var dialogo = new DialogoNuevoProyecto(repositorio) { Owner = propietario, _yaExiste = yaExiste };
        dialogo.Rellenar(carpetaSugerida);
        dialogo.ShowDialog();
        return dialogo._creado;
    }

    private void Rellenar(string? carpetaSugerida)
    {
        // Los desplegables empiezan en blanco: el responsable es una decisión, no un
        // valor por defecto que se cuele por no mirarlo.
        var tecnicos = ServicioDeTecnicos.Catalogo.Tecnicos;
        Tecnico1.ItemsSource = tecnicos;
        Tecnico2.ItemsSource = tecnicos;

        // Sin normas instaladas no se puede dar de alta nada, pero eso ya lo dice el
        // arranque: aquí basta con no reventar y dejar «Crear» apagado.
        try { Norma.ItemsSource = ServicioDePlantillas.Normas(); }
        catch (Exception) { }

        if (!string.IsNullOrWhiteSpace(carpetaSugerida) && Directory.Exists(carpetaSugerida))
            _carpetaSugerida = carpetaSugerida;

        Estado.ItemsSource = Planificacion.Estados
            .Select(e => new { Valor = e, Etiqueta = Planificacion.EtiquetaDe(e) })
            .ToList();
        Estado.SelectedValue = EstadoDeProyecto.PorHacer;

        ActualizarZonaRecepcion();
        ActualizarSemanas();
        ActualizarTrabajo();
        Revisar();
        Nombre.Focus();
    }

    // ---- que la ventana no se salga de la pantalla --------------------------
    //
    // Con lo opcional desplegado el contenido pasa de los 900 px, y en una pantalla a
    // 200 % eso ya no cabe: la ventana crecía por debajo del borde y los botones de
    // «Crear» y «Cancelar» quedaban fuera, sin forma de llegar a ellos porque además no
    // se puede redimensionar.

    /// <summary>Lo que se deja libre alrededor, para que no quede pegada al borde.</summary>
    private const double Margen = 60;

    private void AlAbrirse(object remitente, RoutedEventArgs args) => EncajarEnLaPantalla();

    private void AlPlegarODesplegar(object remitente, RoutedEventArgs args) => EncajarEnLaPantalla();

    /// <summary>
    /// Topa la ventana al alto útil de la pantalla —el de verdad, no un número fijo— y la
    /// sube si al crecer se ha salido por abajo. A partir de ahí el contenido se desplaza
    /// por dentro, que es lo que hace que se pueda llegar al final siempre.
    /// </summary>
    private void EncajarEnLaPantalla()
    {
        var util = SystemParameters.WorkArea;
        MaxHeight = Math.Max(300, util.Height - Margen);

        // El alto todavía no está recalculado cuando salta el evento: se ajusta la
        // posición cuando WPF haya terminado de medir.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Top + ActualHeight > util.Bottom)
                Top = Math.Max(util.Top, util.Bottom - ActualHeight);
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void AlEscribir(object remitente, RoutedEventArgs args) => Revisar();

    private void AlElegirTecnico(object remitente, RoutedEventArgs args) => Revisar();

    private void AlElegirNorma(object remitente, RoutedEventArgs args) => Revisar();

    /// <summary>Rojo mientras falte; el gris de siempre en cuanto esté.</summary>
    private static readonly Brush Falta = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
    private static readonly Brush Puesto = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51));

    /// <summary>
    /// Enciende «Crear» cuando están los datos que hacen falta, y pone en rojo el rótulo
    /// de los que faltan. Es el mismo criterio que las casillas obligatorias de la toma de
    /// notas: <b>rojo mientras esté vacío</b>, no rojo para siempre — así el rojo señala
    /// trabajo pendiente y no decora.
    /// </summary>
    private void Revisar()
    {
        if (BotonCrear is null) return;   // durante InitializeComponent aún no existe

        var norma = Norma.SelectedItem as NormaDisponible;

        var faltan = AltaDeProyecto
            .Faltan(Nombre.Text, Tecnico1.SelectedItem as string, norma?.Id)
            .ToList();

        // La carpeta no la exige el negocio pero sí la física: sin ella no hay dónde
        // escribir el fichero.
        var sinCarpeta = string.IsNullOrWhiteSpace(Carpeta.Text);
        if (sinCarpeta) faltan.Add("Carpeta");

        EtiquetaNombre.Foreground = Marca(AltaDeProyecto.CampoNombre, faltan);
        EtiquetaTecnico.Foreground = Marca(AltaDeProyecto.CampoTecnico, faltan);
        EtiquetaNorma.Foreground = Marca(AltaDeProyecto.CampoNorma, faltan);
        EtiquetaCarpeta.Foreground = sinCarpeta ? Falta : Puesto;

        // Qué código de servicio va a quedar y cómo se va a llamar el fichero, a la vista
        // antes de crear nada: los dos salen de lo que se está tecleando arriba, y verlos
        // es lo que evita darse cuenta del error cuando el fichero ya está en la carpeta.
        // Debajo de la caja: mientras el código esté a medias, cuánto le falta; cuando esté
        // completo, qué código de servicio queda y cómo se va a llamar el fichero. El
        // ejemplo del formato va aparte y fijo, encima de la caja.
        var codigo = Nombre.Text.Trim();
        Derivado.Text = codigo.Length switch
        {
            0 => "",
            var n when n != AltaDeProyecto.LongitudDelCodigo
                => $"El código son {AltaDeProyecto.LongitudDelCodigo} caracteres; llevas {n}.",
            _ => $"Código de servicio: {CodigoDeServicio.Derivar(codigo)}"
                 + (norma is null
                     ? ""
                     : $"   |   Fichero: {NombreDeTomaDeNotas.ConExtension(
                         norma.CodigoDeFichero, codigo, RepositorioDeProyectos.Extension)}")
        };

        BotonCrear.IsEnabled = faltan.Count == 0;

        // Aquí iba una frase enumerando lo que falta. Sobraba: los rótulos de esos mismos
        // campos ya están en rojo y «Crear» está apagado, así que repetirlo por escrito
        // gastaba una línea fija de la ventana para decir lo que ya se ve. El aviso se
        // queda para lo que no se ve: un fallo al crear el fichero.
    }

    private static Brush Marca(string campo, List<string> faltan)
        => faltan.Contains(campo) ? Falta : Puesto;

    private void AlElegirCarpeta(object remitente, RoutedEventArgs args)
    {
        var dialogo = new OpenFolderDialog
        {
            Title = "Carpeta del proyecto",
            InitialDirectory = Directory.Exists(Carpeta.Text) ? Carpeta.Text : _carpetaSugerida ?? ""
        };

        if (dialogo.ShowDialog(this) == true) Carpeta.Text = dialogo.FolderName;
        Revisar();
    }

    // ---- la parte opcional: la planificación --------------------------------

    /// <summary>
    /// Enseña en qué se traduce el importe, que es el número con el que se planifica.
    /// Verlo mientras se teclea evita descubrir en la tabla de carga que había un cero de más.
    /// </summary>
    private void ActualizarTrabajo()
    {
        if (LeerImporte() is { } importe && importe > 0)
        {
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

    private void AlCambiarImporte(object remitente, System.Windows.Controls.TextChangedEventArgs args)
        => ActualizarTrabajo();

    /// <summary>
    /// El grupo se escribe a mano en cada toma de notas, así que una errata las deja sin
    /// enlazar. Aquí se recuerda cómo va a quedar guardado.
    /// </summary>
    private void AlCambiarGrupo(object remitente, System.Windows.Controls.TextChangedEventArgs args)
    {
        var escrito = Grupo.Text.Trim();

        AvisoGrupo.Text = escrito.Length == 0
            ? "Escribe el mismo nombre en las tomas de notas que sean del mismo trabajo —las familias de un servicio— y el calendario las enseñará en una sola barra. Las fechas y el importe se ponen solo en una de ellas."
            : $"Se enlazará con las que lleven «{escrito}». No hace falta escribirlo igual: "
              + "no se distinguen mayúsculas, espacios ni guiones.";
    }

    private void AlCambiarFecha(object remitente, System.Windows.Controls.SelectionChangedEventArgs args)
        => ActualizarSemanas();

    private void AlMarcarRecibidas(object remitente, RoutedEventArgs args)
    {
        // Marcar la casilla sin poner fecha dejaría el dato a medias; se propone hoy.
        if (Recibidas.IsChecked == true && Recepcion.SelectedDate is null)
            Recepcion.SelectedDate = DateTime.Today;

        ActualizarZonaRecepcion();
    }

    private void ActualizarZonaRecepcion()
        => ZonaRecepcion.Visibility = Recibidas.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

    private void AlBloquear(object remitente, RoutedEventArgs args) => ActualizarBloqueo();

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
    }

    private void AlQuitarFechas(object remitente, RoutedEventArgs args)
    {
        Inicio.SelectedDate = null;
        Fin.SelectedDate = null;
        ActualizarSemanas();
    }

    private static int Semanas(DateTime fecha) => ISOWeek.GetWeekOfYear(fecha);

    /// <summary>
    /// La planificación que se ha rellenado, o <c>null</c> si algo no cuadra —en cuyo caso
    /// ya se ha avisado y no hay que crear nada.
    /// </summary>
    private Planificacion? LeerPlanificacion()
    {
        if (Inicio.SelectedDate is { } inicio && Fin.SelectedDate is { } fin && fin < inicio)
        {
            Avisar("La fecha de fin es anterior a la de inicio.");
            return null;
        }

        // Con una sola fecha el servicio no se puede dibujar y se quedaría en un limbo:
        // ni planificado ni pendiente. O las dos, o ninguna.
        if (Inicio.SelectedDate is null ^ Fin.SelectedDate is null)
        {
            Avisar("Pon las dos fechas, o quítalas las dos con «Quitar fechas» "
                   + "para dejarlo pendiente de planificar.");
            return null;
        }

        var importe = LeerImporte();

        if (Importe.Text.Trim().Length > 0 && importe is null)
        {
            Avisar("El importe no se entiende. Escribe solo el número, por ejemplo 2000.");
            return null;
        }

        if (importe < 0)
        {
            Avisar("El importe no puede ser negativo.");
            return null;
        }

        return new Planificacion
        {
            Inicio = Inicio.SelectedDate,
            Fin = Fin.SelectedDate,
            Estado = Estado.SelectedValue is EstadoDeProyecto estado ? estado : EstadoDeProyecto.PorHacer,
            RecepcionMuestras = Recibidas.IsChecked == true ? Recepcion.SelectedDate : null,
            // Archivar no se ofrece aquí: nace archivado lo que nadie va a mirar.
            Archivado = false,
            Importe = importe,
            // Vacío es sin grupo, no un grupo llamado "".
            Grupo = Grupo.Text.Trim() is { Length: > 0 } grupo ? grupo : null,
            FechasBloqueadas = Bloquear.IsChecked == true
        };
    }

    // ---- crear ---------------------------------------------------------------

    private void AlCrear(object remitente, RoutedEventArgs args)
    {
        var nombre = Nombre.Text.Trim();
        var tecnico1 = Tecnico1.SelectedItem as string;
        var norma = Norma.SelectedItem as NormaDisponible;

        if (!AltaDeProyecto.SePuedeCrear(nombre, tecnico1, norma?.Id)) return;

        // Lo opcional se comprueba antes de tocar el disco: si el importe no se entiende,
        // no se crea un fichero a medias que luego haya que corregir a mano.
        if (LeerPlanificacion() is not { } planificacion)
        {
            Opcionales.IsExpanded = true;   // el fallo está ahí dentro; sin abrirlo no se ve
            return;
        }

        var plantilla = PlantillaEnsayos.Cargar(norma!.Ruta);

        // El nombre lo fija el laboratorio y sale de la norma y del código de la toma de notas.
        var ruta = Path.Combine(Carpeta.Text, NombreDeTomaDeNotas.ConExtension(
            plantilla.Meta.CodigoParaFichero, nombre, RepositorioDeProyectos.Extension));

        // Crear una toma de notas no puede pisar otra: dos servicios del mismo cliente con
        // el mismo nombre es un descuido frecuente, y aquí se perdería el trabajo del otro.
        if (File.Exists(ruta))
        {
            Avisar($"Ya hay una toma de notas llamada «{Path.GetFileName(ruta)}» en esa carpeta.");
            return;
        }

        // Se pregunta por el código de la toma de notas: por el de servicio saltaría en
        // cada familia nueva de un trabajo, que es lo normal y no un descuido.
        if (_yaExiste?.Invoke(nombre) is { } respuesta && respuesta != RespuestaRepetido.CrearIgualmente)
        {
            // Si ha elegido abrir el que ya había, la ventana lo abre y aquí no hay
            // nada más que hacer.
            if (respuesta == RespuestaRepetido.Abrir) Close();
            return;
        }

        try
        {
            var datos = AltaDeProyecto.Crear(
                nombre, tecnico1!, Tecnico2.SelectedItem as string, plantilla);

            _repositorio.Guardar(datos, ruta, plantilla.Meta.Version);

            // La planificación va en un segundo paso porque es lo único que la escribe
            // (DD‑53): «Guardar» nunca la toca, la relee del disco. Se aplica solo si se
            // ha puesto algo, para no marcar como tocado lo que nadie rellenó.
            if (!planificacion.EsVacia) _repositorio.ActualizarPlanificacion(ruta, planificacion);

            _creado = ruta;
            Close();
        }
        catch (Exception ex)
        {
            Avisar("No se pudo crear: " + ex.Message);
        }
    }

    /// <summary>
    /// Lo que ha ido mal. <b>Solo se usa para eso</b>: desde que la enumeración de campos
    /// que faltan se fue —los rótulos ya salen en rojo—, aquí no llega nada que no sea un
    /// problema, así que el aviso ya no necesita dos colores.
    /// </summary>
    private void Avisar(string texto)
    {
        Aviso.Text = texto;
        Aviso.Visibility = Visibility.Visible;
    }

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
