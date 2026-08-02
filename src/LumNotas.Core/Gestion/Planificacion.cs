using System.Globalization;
using System.Text.Json.Serialization;

namespace LumNotas.Core.Gestion;

/// <summary>
/// En qué punto está un servicio. Lo decide el técnico a mano, y es distinto del
/// avance que calcula el motor: un proyecto puede tener todas las secciones
/// rellenas y seguir «pendiente cliente» porque falta que confirmen algo.
/// </summary>
public enum EstadoDeProyecto
{
    PorHacer,
    EnCurso,
    PendienteCliente,
    Terminado
}

/// <summary>
/// Planificación de un servicio: cuándo empieza, cuándo termina, en qué estado está
/// y si las muestras han llegado. Es lo que alimenta la línea de tiempo.
/// <para>
/// <b>Vive en el <c>.lumproj</c> pero no la gestiona la toma de notas.</b> Solo la
/// escribe el calendario; al guardar un proyecto desde su pestaña, la planificación
/// se conserva releyéndola del disco. Así, mover una fecha desde el calendario no lo
/// pisa el técnico que tenía ese proyecto abierto desde hace media hora.
/// </para>
/// </summary>
public sealed class Planificacion
{
    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public EstadoDeProyecto Estado { get; set; } = EstadoDeProyecto.PorHacer;

    /// <summary>
    /// Cuándo llegaron las muestras. Es una fecha y no un sí/no a propósito: con la
    /// fecha se ve «llevan tres semanas aquí y sigue sin empezar»; con un booleano, no.
    /// </summary>
    public DateTime? RecepcionMuestras { get; set; }

    /// <summary>
    /// Retirado de la línea de tiempo. <b>No borra nada</b>: el proyecto sigue en su
    /// carpeta y vuelve a verse con «Ver archivados». Se guarda en el fichero, no en
    /// los ajustes de cada usuario, para que todos vean el mismo calendario.
    /// </summary>
    public bool Archivado { get; set; }

    /// <summary>
    /// Importe de la oferta, en euros. De aquí sale el trabajo que supone el servicio
    /// —el laboratorio lo mide dividiendo entre 80 €/día—, que es lo que alimenta la
    /// vista de carga por técnico.
    /// <para>
    /// Es dato comercial, no de ensayo: se guarda con la planificación y <b>no aparece
    /// en el informe</b> que se firma.
    /// </para>
    /// </summary>
    public double? Importe { get; set; }

    /// <summary>
    /// Trabajo del que forma parte esta toma de notas. Es lo que enlaza las varias
    /// familias de luminarias de un mismo servicio para que el calendario las enseñe
    /// como <b>una sola barra</b>.
    /// <para>
    /// Es un nombre, no un identificador: se teclea en el diálogo de planificación y se
    /// compara sin mayúsculas ni espacios. No hay fichero de grupo — el enlace vive
    /// dentro de cada toma de notas y viaja con ella si se mueve de carpeta (DD‑89).
    /// </para>
    /// <para>
    /// De las enlazadas, <b>una hace de cabecera</b>: la que lleva las fechas y el
    /// importe. Las demás solo dicen a qué grupo pertenecen. Así el importe de la oferta
    /// está en un único sitio y la carga no lo cuenta cuatro veces.
    /// </para>
    /// </summary>
    public string? Grupo { get; set; }

    // Lo que sigue se calcula, no se guarda: sin [JsonIgnore] el .lumproj acababa con
    // campos como «hayFechas» o «esVacia», que además mentirían al releerlos.

    [JsonIgnore]
    public bool HayFechas => Inicio is not null && Fin is not null;

    /// <summary>Sin planificar todavía. Los proyectos anteriores al calendario están así.</summary>
    [JsonIgnore]
    public bool EsVacia => Inicio is null && Fin is null && RecepcionMuestras is null
                           && Estado == EstadoDeProyecto.PorHacer && !Archivado && Importe is null
                           && string.IsNullOrWhiteSpace(Grupo);

    /// <summary>Fin corregido: una fecha de fin anterior al inicio dibujaría al revés.</summary>
    [JsonIgnore]
    public DateTime? FinEfectivo => Fin is { } fin && Inicio is { } inicio && fin < inicio ? inicio : Fin;

    [JsonIgnore]
    public bool MuestrasRecibidas => RecepcionMuestras is not null;

    /// <summary>
    /// Fuera de plazo: la fecha de fin ya pasó y el servicio no está terminado. Es el
    /// motivo por el que existe la línea de tiempo, así que se mira en la tarjeta.
    /// </summary>
    public bool Retrasado(DateTime hoy)
        => Estado != EstadoDeProyecto.Terminado && FinEfectivo is { } fin && fin.Date < hoy.Date;

    public Planificacion Copia() => new()
    {
        Inicio = Inicio,
        Fin = Fin,
        Estado = Estado,
        RecepcionMuestras = RecepcionMuestras,
        Archivado = Archivado,
        Importe = Importe,
        Grupo = Grupo
    };

    public static string EtiquetaDe(EstadoDeProyecto estado) => estado switch
    {
        EstadoDeProyecto.PorHacer => "Por hacer",
        EstadoDeProyecto.EnCurso => "En curso",
        EstadoDeProyecto.PendienteCliente => "Pendiente cliente",
        EstadoDeProyecto.Terminado => "Terminado",
        _ => estado.ToString()
    };

    /// <summary>Color de la tarjeta según el estado, en el formato que entiende WPF.</summary>
    public static string ColorDe(EstadoDeProyecto estado) => estado switch
    {
        EstadoDeProyecto.PorHacer => "#94A3B8",
        EstadoDeProyecto.EnCurso => "#2563EB",
        EstadoDeProyecto.PendienteCliente => "#D97706",
        EstadoDeProyecto.Terminado => "#16A34A",
        _ => "#94A3B8"
    };

    public static IReadOnlyList<EstadoDeProyecto> Estados =>
    [
        EstadoDeProyecto.PorHacer,
        EstadoDeProyecto.EnCurso,
        EstadoDeProyecto.PendienteCliente,
        EstadoDeProyecto.Terminado
    ];
}

/// <summary>Qué se está cambiando al arrastrar una barra del calendario con el ratón.</summary>
public enum ModoArrastre
{
    /// <summary>La barra entera: cambian el inicio y el fin a la vez.</summary>
    Mover,

    /// <summary>El borde izquierdo: solo el inicio.</summary>
    Inicio,

    /// <summary>El borde derecho: solo el fin.</summary>
    Fin
}

/// <summary>
/// Dónde acaban las fechas de un servicio al arrastrar su barra tantos días.
/// <para>
/// Vive en el núcleo y no en la interfaz porque es lo único del arrastre que puede
/// estar mal: mover conserva la duración, y los bordes topan el uno con el otro
/// porque un fin anterior al inicio no existe.
/// </para>
/// </summary>
public static class ArrastreDeFechas
{
    public static (DateTime Inicio, DateTime Fin) Aplicar(
        DateTime inicio, DateTime fin, ModoArrastre modo, int dias) => modo switch
    {
        ModoArrastre.Mover => (inicio.AddDays(dias), fin.AddDays(dias)),
        ModoArrastre.Inicio => (Menor(inicio.AddDays(dias), fin), fin),
        ModoArrastre.Fin => (inicio, Mayor(fin.AddDays(dias), inicio)),
        _ => (inicio, fin)
    };

    private static DateTime Menor(DateTime a, DateTime b) => a < b ? a : b;
    private static DateTime Mayor(DateTime a, DateTime b) => a > b ? a : b;
}

/// <summary>Una semana de la cabecera del calendario.</summary>
/// <param name="Numero">Número de semana ISO, que es como planifica el laboratorio.</param>
public sealed record CeldaDeSemana(int Numero, DateTime Lunes, double Izquierda, double Ancho, bool EsActual)
{
    public string Rotulo => "S" + Numero.ToString("00");

    /// <summary>El lunes, que es como se cita una semana en el laboratorio.</summary>
    public string Desde => Lunes.ToString("dd/MM");
}

/// <summary>Un mes de la cabecera, que abarca las semanas que caen dentro.</summary>
public sealed record CeldaDeMes(string Nombre, double Izquierda, double Ancho);

/// <summary>
/// El eje horizontal del calendario, medido en <b>semanas</b> porque es la unidad con
/// la que planifica el laboratorio (el jefe habla de «la S32», no del 4 de agosto).
/// <para>
/// Vive en el núcleo y no en la interfaz para poder probarlo: la aritmética de
/// semanas ISO, con años que tienen 53 y semanas que cruzan de diciembre a enero, es
/// justo lo que se rompe en silencio.
/// </para>
/// </summary>
public sealed class EjeDeSemanas
{
    /// <summary>Semanas de margen a cada lado, para que nada quede pegado al borde.</summary>
    private const int Margen = 2;

    private EjeDeSemanas(DateTime desde, int semanas, double anchoSemana, DateTime hoy)
    {
        Desde = desde;
        Semanas = semanas;
        AnchoSemana = anchoSemana;
        Hoy = hoy;

        var celdas = new List<CeldaDeSemana>(semanas);
        for (var i = 0; i < semanas; i++)
        {
            var lunes = desde.AddDays(7 * i);
            celdas.Add(new CeldaDeSemana(
                ISOWeek.GetWeekOfYear(lunes), lunes, i * anchoSemana, anchoSemana,
                lunes == LunesDe(hoy)));
        }

        Celdas = celdas;
        Meses = AgruparPorMes(celdas);
    }

    public DateTime Desde { get; }
    public int Semanas { get; }
    public double AnchoSemana { get; }
    public DateTime Hoy { get; }

    public double Ancho => Semanas * AnchoSemana;

    /// <summary>Primer día que ya queda fuera del eje.</summary>
    public DateTime Hasta => Desde.AddDays(7 * Semanas);

    public IReadOnlyList<CeldaDeSemana> Celdas { get; }
    public IReadOnlyList<CeldaDeMes> Meses { get; }

    /// <summary>Dónde cae «hoy» en píxeles. Es la línea vertical del calendario.</summary>
    public double PosicionDeHoy => PosicionDe(Hoy);

    public bool HoyEstaDentro => Hoy.Date >= Desde && Hoy.Date < Desde.AddDays(7 * Semanas);

    /// <summary>Píxeles desde el borde izquierdo hasta esa fecha.</summary>
    public double PosicionDe(DateTime fecha)
        => (fecha.Date - Desde).TotalDays / 7.0 * AnchoSemana;

    /// <summary>
    /// Ancho de una barra entre dos fechas, ambas incluidas: un servicio que empieza y
    /// termina el mismo día ocupa un día, no cero.
    /// </summary>
    public double AnchoEntre(DateTime inicio, DateTime fin)
        => Math.Max(((fin.Date - inicio.Date).TotalDays + 1) / 7.0 * AnchoSemana, AnchoSemana / 7.0);

    /// <summary>
    /// Días que representan tantos píxeles, redondeados. Es la operación inversa del
    /// dibujo y la que necesita el arrastre de una barra con el ratón.
    /// </summary>
    public int DiasEn(double pixeles) => (int)Math.Round(pixeles / AnchoSemana * 7);

    /// <summary>Fecha que cae en esa posición del eje, ajustada al día.</summary>
    public DateTime FechaEn(double x) => Desde.AddDays(DiasEn(x));

    /// <summary>
    /// Si este eje abarca todo lo que abarca el otro. Sirve para no rehacer el eje al
    /// soltar una barra: si las fechas nuevas siguen cabiendo, el calendario no se mueve
    /// bajo el ratón.
    /// </summary>
    public bool Cubre(EjeDeSemanas otro)
        => AnchoSemana == otro.AnchoSemana && Desde <= otro.Desde && Hasta >= otro.Hasta;

    /// <summary>El lunes de la semana ISO en la que cae una fecha.</summary>
    public static DateTime LunesDe(DateTime fecha)
        => fecha.Date.AddDays(-(((int)fecha.DayOfWeek + 6) % 7));

    /// <summary>
    /// Tope de semanas que se dibujan de una vez: diez años.
    /// <para>
    /// El calendario <b>no está atado a ningún año</b> —se calcula, no se almacena— y
    /// funciona igual en 2027 o en 2040. Lo que sí hay que acotar es el tamaño de una
    /// sola pantalla: un año tecleado mal (3026 en vez de 2026) generaría cien mil
    /// semanas y colgaría la aplicación. Con el tope, ese proyecto no se dibuja y
    /// aparece en la banda de abajo, que es donde se ve que hay que corregirlo.
    /// </para>
    /// </summary>
    public const int MaximoSemanas = 520;

    /// <summary>
    /// Construye el eje que hace falta para dibujar unos rangos: los abarca todos, deja
    /// margen a los lados y siempre incluye la semana actual, aunque no haya ningún
    /// proyecto cerca —si no, el calendario abre en un sitio que no dice nada.
    /// </summary>
    /// <param name="extraAntes">Semanas vacías añadidas por delante, para poder mirar atrás.</param>
    /// <param name="extraDespues">
    /// Semanas vacías añadidas por detrás. Es lo que permite planificar en años futuros:
    /// se pide sitio y luego se arrastra o se planifica allí.
    /// </param>
    public static EjeDeSemanas Para(IEnumerable<(DateTime Inicio, DateTime Fin)> rangos, DateTime hoy,
                                    double anchoSemana, int semanasMinimas = 12,
                                    int extraAntes = 0, int extraDespues = 0)
    {
        var lunesDeHoy = LunesDe(hoy);
        var primero = lunesDeHoy;
        var ultimo = lunesDeHoy;

        foreach (var (inicio, fin) in rangos)
        {
            // Los disparates no encuadran el calendario. Un año tecleado mal estiraría el
            // eje a diez años y dejaría el trabajo real reducido a una franja diminuta;
            // se ignora aquí y el proyecto sale en la banda de abajo, que es donde se ve
            // que hay que corregirlo.
            if (!EstaEnHorizonte(inicio, fin, lunesDeHoy)) continue;

            var a = LunesDe(inicio);
            var b = LunesDe(fin);
            if (a < primero) primero = a;
            if (b > ultimo) ultimo = b;
        }

        var desde = primero.AddDays(-7 * Margen);
        var hasta = ultimo.AddDays(7 * Margen);
        var semanas = (int)Math.Round((hasta - desde).TotalDays / 7.0) + 1;

        // Con pocos proyectos el eje saldría muy corto y el calendario parecería vacío.
        if (semanas < semanasMinimas)
        {
            desde = desde.AddDays(-7 * ((semanasMinimas - semanas) / 2));
            semanas = semanasMinimas;
        }

        // El sitio pedido a mano se añade encima del encuadre natural, y no antes, para
        // que pulsar «ver más adelante» mueva siempre lo mismo en vez de quedar absorbido
        // por el mínimo cuando hay pocos proyectos.
        desde = desde.AddDays(-7 * Math.Max(extraAntes, 0));
        semanas += Math.Max(extraAntes, 0) + Math.Max(extraDespues, 0);

        // La semana actual tiene que seguir dentro pase lo que pase, así que el recorte
        // se hace alrededor de hoy y no del extremo de los datos.
        var principio = lunesDeHoy.AddDays(-7 * (MaximoSemanas / 2));
        if (desde < principio)
        {
            semanas -= (int)Math.Round((principio - desde).TotalDays / 7.0);
            desde = principio;
        }

        return new EjeDeSemanas(desde, Math.Clamp(semanas, semanasMinimas, MaximoSemanas), anchoSemana, hoy);
    }

    /// <summary>Si un servicio cae, aunque sea en parte, dentro de lo que se está dibujando.</summary>
    public bool Contiene(DateTime inicio, DateTime fin) => fin.Date >= Desde && inicio.Date < Hasta;

    /// <summary>
    /// Si unas fechas son planificables: cinco años a cada lado de hoy. No es un límite
    /// del programa —la ventana se mueve con el calendario y nunca caduca—, sino la
    /// frontera a partir de la cual una fecha es casi con seguridad una errata.
    /// </summary>
    public static bool EstaEnHorizonte(DateTime inicio, DateTime fin, DateTime hoy)
    {
        var mitad = 7 * (MaximoSemanas / 2);
        return fin.Date >= LunesDe(hoy).AddDays(-mitad) && inicio.Date <= LunesDe(hoy).AddDays(mitad);
    }

    private static IReadOnlyList<CeldaDeMes> AgruparPorMes(List<CeldaDeSemana> celdas)
    {
        var cultura = CulturaDelLaboratorio;
        var meses = new List<CeldaDeMes>();

        foreach (var celda in celdas)
        {
            // Una semana se atribuye al mes de su jueves, que es el criterio ISO y evita
            // que la semana de cambio de mes aparezca dos veces.
            var jueves = celda.Lunes.AddDays(3);
            var nombre = cultura.TextInfo.ToTitleCase(jueves.ToString("MMMM yyyy", cultura));

            if (meses.Count > 0 && meses[^1].Nombre == nombre)
                meses[^1] = meses[^1] with { Ancho = meses[^1].Ancho + celda.Ancho };
            else
                meses.Add(new CeldaDeMes(nombre, celda.Izquierda, celda.Ancho));
        }

        return meses;
    }

    /// <summary>
    /// El laboratorio trabaja en español; fijarlo evita que el calendario salga en
    /// inglés en un equipo con otra configuración regional.
    /// </summary>
    public static readonly CultureInfo CulturaDelLaboratorio = CultureInfo.GetCultureInfo("es-ES");
}
