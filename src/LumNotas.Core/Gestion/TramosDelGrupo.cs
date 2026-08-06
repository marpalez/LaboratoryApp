namespace LumNotas.Core.Gestion;

/// <summary>
/// Un trozo de la barra: qué familia es y qué parte del ancho ocupa.
/// </summary>
/// <param name="Fraccion">De 0 a 1, la parte de la barra que le toca.</param>
public sealed record TramoDelGrupo(ResumenDeProyecto Miembro, DateTime Desde, DateTime Hasta, double Fraccion);

/// <summary>
/// Cómo se reparte la barra de un trabajo entre las familias enlazadas.
/// <para>
/// <b>Cada familia dura lo suyo.</b> Lo que manda es su <b>duración</b> —lo que va de su
/// inicio a su fin—, no la fecha en que acaba: si una lleva cinco días y la siguiente
/// quince, se dibujan de cinco y de quince, vayan donde vayan en la cadena. Antes se
/// razonaba con fechas de corte, y en cuanto la fecha de una familia no servía de corte
/// —caía por detrás de donde iba el reparto, o justo en el final del trabajo— sus fechas
/// se tiraban a la basura y esa familia acababa <b>repartiéndose el hueco a partes
/// iguales</b> con las de al lado. De ahí que dos familias planificadas de distinta
/// duración salieran del mismo tamaño.
/// </para>
/// <para>
/// <b>Se encadenan.</b> Cada familia empieza donde acabó la anterior, aunque tenga otra
/// fecha de inicio escrita: en un trabajo partido en cuatro, lo que interesa ver es la
/// secuencia, no cuatro tramos solapados. La primera arranca donde arranca el trabajo.
/// </para>
/// <para>
/// El orden lo da el <b>código</b> y no las fechas: es el orden natural del laboratorio y
/// es estable, así que el técnico ve siempre las familias en el mismo sitio aunque las
/// fechas cambien.
/// </para>
/// <para>
/// <b>La que no dice cuánto dura, dura como las demás</b>: se le da la media de las que sí
/// lo dicen. Y si no lo dice ninguna, la barra se parte en trozos iguales — que es lo único
/// que los datos permiten decir cuando nadie ha planificado cada familia por separado.
/// </para>
/// </summary>
public static class TramosDelGrupo
{
    /// <summary>
    /// Los tramos de una entrada del calendario. Una toma de notas suelta da un solo
    /// tramo que ocupa la barra entera, así que quien dibuje no necesita distinguir casos.
    /// </summary>
    public static IReadOnlyList<TramoDelGrupo> Calcular(EntradaDeCalendario entrada)
        => Calcular(entrada.EnOrden, entrada.Inicio, entrada.Fin);

    /// <summary>
    /// Lo mismo, pero diciendo <b>de cuándo a cuándo</b> va el trabajo en vez de deducirlo
    /// de las familias.
    /// <para>
    /// Hace falta para el arrastre: mientras se arrastra, el trabajo enseña unas fechas que
    /// todavía no están guardadas y las tarjetas tienen que seguirlas. Las duraciones salen
    /// siempre de lo <b>guardado</b>, así que la diferencia entre lo que se pide y lo que
    /// suman dice qué gesto es y quién tiene que absorberla:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Mover</b>: no hay diferencia. Todas conservan su duración y se desplazan.</item>
    /// <item><b>Estirar por la izquierda</b>: cambia el arranque, así que crece la primera.</item>
    /// <item><b>Estirar por la derecha</b>: crece la última.</item>
    /// </list>
    /// <para>
    /// Es justo lo que luego escribe <see cref="RepartoDelArrastre"/>, y por eso al soltar
    /// las tarjetas se quedan donde se estaban viendo en vez de dar un salto.
    /// </para>
    /// <para>Siempre devuelve <b>un tramo por familia</b>, en el orden recibido.</para>
    /// </summary>
    public static IReadOnlyList<TramoDelGrupo> Calcular(
        IReadOnlyList<ResumenDeProyecto> miembros, DateTime? desde, DateTime? hasta)
    {
        if (miembros.Count == 0) return [];

        if (desde is not { } inicio || hasta is not { } fin || fin <= inicio)
            return Iguales(miembros, desde, hasta);

        var dias = DiasDeCadaUna(miembros);

        // Nadie ha dicho cuánto dura nada: trozos iguales, que es lo único que se puede decir.
        if (dias is null) return Iguales(miembros, inicio, fin);

        var total = (fin - inicio).TotalDays + 1;
        var diferencia = total - dias.Sum();

        if (diferencia != 0)
        {
            var quien = SeHaMovidoElArranque(miembros, inicio) ? 0 : dias.Length - 1;
            dias[quien] = Math.Max(1, dias[quien] + diferencia);
        }

        var tramos = new List<TramoDelGrupo>(miembros.Count);
        var cursor = inicio;

        for (var i = 0; i < miembros.Count; i++)
        {
            // La última cierra el trabajo. Así las tarjetas llenan la barra exactamente, sin
            // hueco al final ni un pico que se salga, pase lo que pase con las duraciones.
            var ultimoDia = i == miembros.Count - 1
                ? fin
                : Acotar(cursor.AddDays(dias[i] - 1), cursor, fin);

            tramos.Add(Tramo(miembros[i], cursor, ultimoDia, total));

            // Al día siguiente, no el mismo día: si no, la frontera se cuenta dos veces y
            // cada tarjeta sale corrida un día respecto de lo que pone en su fichero.
            cursor = ultimoDia.AddDays(1);
        }

        return tramos;
    }

    /// <summary>
    /// Cuándo acaba el trabajo: donde acaba la cadena de sus familias. <b>Por eso la barra
    /// de un grupo es más larga que la de su cabecera</b> en cuanto las familias llevan
    /// fechas propias.
    /// </summary>
    /// <param name="finMasTardio">
    /// Lo más tarde que acabe cualquiera de ellas. Sirve de suelo: si alguien ha escrito una
    /// fecha más allá de donde llega la cadena, el trabajo no puede acabar antes que ella.
    /// </param>
    public static DateTime? FinDelTrabajo(
        IReadOnlyList<ResumenDeProyecto> miembros, DateTime? inicio, DateTime? finMasTardio)
    {
        if (inicio is not { } arranque || miembros.Count == 0) return finMasTardio;
        if (DiasDeCadaUna(miembros) is not { } dias) return finMasTardio;

        // −1 porque el primer día ya lo ocupa el arranque: un trabajo de un día empieza y
        // acaba el mismo.
        var cadena = arranque.AddDays(dias.Sum() - 1);

        return finMasTardio is { } tope && tope > cadena ? tope : cadena;
    }

    /// <summary>
    /// Los días que ocupa cada familia según lo que tenga escrito, en el orden recibido.
    /// Devuelve <c>null</c> si no lo dice ninguna.
    /// <para>
    /// Van <b>los dos extremos incluidos</b>: del 1 al 6 son seis días, no cinco. Es lo que
    /// hace que la siguiente arranque el 7 —al día siguiente— y que lo dibujado coincida
    /// con lo que <see cref="CadenaDelGrupo"/> deja escrito en los ficheros.
    /// </para>
    /// <para>
    /// Se recorren <b>desde el arranque guardado</b>, no desde el que se esté enseñando: lo
    /// que dura una familia es un dato suyo y no puede cambiar por estar arrastrando la
    /// barra. Si lo hiciera, al estirar por la izquierda la primera se encogería en vez de
    /// crecer.
    /// </para>
    /// </summary>
    private static double[]? DiasDeCadaUna(IReadOnlyList<ResumenDeProyecto> miembros)
    {
        var arranque = miembros.Select(m => m.Planificacion.Inicio).OfType<DateTime>().FirstOrDefault();
        if (arranque == default) return null;   // nadie dice cuándo empieza nada

        var dichos = new double?[miembros.Count];
        var cursor = arranque;

        for (var i = 0; i < miembros.Count; i++)
        {
            dichos[i] = DiasDe(miembros[i].Planificacion, cursor);
            if (dichos[i] is { } suyos) cursor = cursor.AddDays(suyos);
        }

        var conocidos = dichos.OfType<double>().ToList();
        if (conocidos.Count == 0) return null;

        // La que no lo dice dura como las demás. Es mejor suposición que partirlo todo por
        // igual: al menos las que sí lo dicen conservan su tamaño.
        var porDefecto = conocidos.Average();

        return [.. dichos.Select(d => d ?? porDefecto)];
    }

    /// <summary>
    /// Los días que ocupa una familia, <b>los dos extremos incluidos</b>. Con sus dos fechas,
    /// lo que va de una a otra, que es lo que hay que respetar la ponga la cadena donde la
    /// ponga. Con solo la de fin, lo que va desde donde arrancaba — que es como se ha escrito
    /// siempre: «yo acabo tal día».
    /// </summary>
    private static double? DiasDe(Planificacion plan, DateTime cursor)
    {
        if (plan.Inicio is { } suyoInicio && plan.FinEfectivo is { } suyoFin && suyoFin >= suyoInicio)
            return (suyoFin - suyoInicio).TotalDays + 1;

        if (plan.FinEfectivo is { } fin && fin >= cursor) return (fin - cursor).TotalDays + 1;

        return null;
    }

    /// <summary>
    /// Si el arranque que se está enseñando no es el guardado. Distingue un estirón por la
    /// izquierda —que alarga la primera— de uno por la derecha, que alarga la última. Al
    /// mover no se llega a preguntar: ahí no sobra ni falta nada.
    /// </summary>
    private static bool SeHaMovidoElArranque(IReadOnlyList<ResumenDeProyecto> miembros, DateTime inicio)
        => miembros.Select(m => m.Planificacion.Inicio).OfType<DateTime>().FirstOrDefault() is { } arranque
           && arranque != default && arranque != inicio;

    private static DateTime Acotar(DateTime fecha, DateTime minimo, DateTime maximo)
        => fecha < minimo ? minimo : fecha > maximo ? maximo : fecha;

    private static TramoDelGrupo Tramo(ResumenDeProyecto miembro, DateTime desde, DateTime hasta, double total)
        => new(miembro, desde, hasta, total <= 0 ? 0 : ((hasta - desde).TotalDays + 1) / total);

    /// <summary>
    /// Sin nada que encadenar, partes iguales. Es lo que sale cuando el trabajo está sin
    /// planificar o cuando nadie ha puesto fechas familia a familia.
    /// </summary>
    private static IReadOnlyList<TramoDelGrupo> Iguales(
        IReadOnlyList<ResumenDeProyecto> miembros, DateTime? inicio, DateTime? fin)
    {
        if (miembros.Count == 0) return [];

        var desde = inicio ?? DateTime.Today;
        var hasta = fin ?? desde;
        var trozo = (hasta - desde).TotalDays / miembros.Count;

        return [.. miembros.Select((m, i) => new TramoDelGrupo(
            m, desde.AddDays(trozo * i), desde.AddDays(trozo * (i + 1)), 1d / miembros.Count))];
    }
}
