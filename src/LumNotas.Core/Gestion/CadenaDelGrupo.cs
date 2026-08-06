namespace LumNotas.Core.Gestion;

/// <summary>
/// Coloca en fila las tomas de notas de un mismo trabajo: <b>una detrás de otra, sin
/// solaparse y sin huecos</b>.
/// <para>
/// Nace de que la cadena antes <b>solo se dibujaba</b>: cada fichero conservaba sus fechas
/// y el calendario las encadenaba al pintar, así que el diálogo de planificación decía una
/// cosa y la línea de tiempo otra. Ahora las fechas se <b>escriben</b>, y el calendario
/// dibuja lo que pone en el fichero — una sola verdad.
/// </para>
/// <para>
/// <b>Solo toca la planificación</b>, que es dato de gestión y se mueve constantemente. Las
/// fechas que rellena el técnico dentro de cada ensayo son otra cosa y viven en otro sitio
/// del fichero; la exportación no mira la planificación (DD‑53).
/// </para>
/// </summary>
public static class CadenaDelGrupo
{
    /// <summary>Lo que dura una toma de notas de la que nadie ha dicho cuánto dura.</summary>
    public const int DiasPorDefecto = 7;

    /// <summary>
    /// El orden de la cadena, que lo dan <b>las fechas de inicio</b> y nada más.
    /// <para>
    /// No hay número de orden guardado a propósito: sería un segundo dato que decir lo
    /// mismo, y en cuanto se desincronizara habría dos verdades otra vez. Para adelantar
    /// una familia se le pone una fecha de inicio anterior, que es el gesto natural.
    /// </para>
    /// </summary>
    /// <param name="recienEditada">
    /// La que se acaba de guardar, si la hay. <b>Gana los empates</b>: es lo que hace que
    /// ponerle a la segunda el mismo día que la primera las invierta, en vez de dejarlo
    /// todo igual porque el desempate lo decidiera el código.
    /// </param>
    public static IReadOnlyList<ResumenDeProyecto> EnOrden(
        IEnumerable<ResumenDeProyecto> miembros, ResumenDeProyecto? recienEditada = null)
        => [.. miembros
              // Sin fecha de inicio, al final: es donde cae una recién adjuntada.
              .OrderBy(m => m.Planificacion.Inicio ?? DateTime.MaxValue)
              .ThenBy(m => EsLaMisma(m, recienEditada) ? 0 : 1)
              // Y a igualdad de todo, el código: así no bailan de sitio porque sí.
              .ThenBy(m => m.Rotulo, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Las planificaciones que hay que guardar para que el trabajo quede en fila. Vacío si
    /// ya lo estaba: recolocar dos veces seguidas no escribe nada la segunda.
    /// </summary>
    /// <param name="hoy">
    /// Para el único valor que no se puede deducir de nada: una toma de notas sin ninguna
    /// fecha encabezando el trabajo empieza <b>mañana</b>.
    /// </param>
    public static IReadOnlyList<(ResumenDeProyecto Proyecto, Planificacion Plan)> Recolocar(
        IReadOnlyList<ResumenDeProyecto> miembros, ResumenDeProyecto? recienEditada, DateTime hoy)
    {
        var cambios = new List<(ResumenDeProyecto, Planificacion)>();
        DateTime? finAnterior = null;

        foreach (var miembro in EnOrden(miembros, recienEditada))
        {
            var plan = miembro.Planificacion;

            // Una familia con las fechas bloqueadas no se mueve, y las de detrás siguen a
            // partir de donde ella acaba. Sin esto, el bloqueo se rompería por el único
            // camino que nadie ve venir: el de la cadena, que escribe sola.
            if (plan.FechasBloqueadas)
            {
                finAnterior = plan.FinEfectivo ?? plan.Inicio ?? finAnterior;
                continue;
            }

            // La primera se queda donde está; las demás arrancan al día siguiente de que
            // acabe la anterior. Ese «+1» es lo que evita que el día de la frontera se
            // cuente en las dos.
            var inicio = finAnterior is { } anterior
                ? anterior.AddDays(1)
                : plan.Inicio ?? hoy.Date.AddDays(1);

            var fin = inicio + DuracionDe(plan);

            if (plan.Inicio != inicio || plan.Fin != fin)
            {
                var nueva = plan.Copia();
                nueva.Inicio = inicio;
                nueva.Fin = fin;
                cambios.Add((miembro, nueva));
            }

            finAnterior = fin;
        }

        return cambios;
    }

    /// <summary>
    /// Lo que ocupa una toma de notas, que es lo que se conserva al recolocarla: si estaba
    /// planificada para diez días, sigue durando diez días aunque cambie de sitio.
    /// <para>
    /// Sin fecha de fin se le da una semana. Es una suposición, pero deja el trabajo
    /// dibujable y visible, que es lo que hace que alguien se acuerde de corregirla; con
    /// duración cero no se vería.
    /// </para>
    /// </summary>
    private static TimeSpan DuracionDe(Planificacion plan)
        => plan.Inicio is { } inicio && plan.FinEfectivo is { } fin && fin > inicio
            ? fin - inicio
            : TimeSpan.FromDays(DiasPorDefecto);

    private static bool EsLaMisma(ResumenDeProyecto miembro, ResumenDeProyecto? otra)
        => otra is not null
           && string.Equals(miembro.Ruta, otra.Ruta, StringComparison.OrdinalIgnoreCase);
}
