namespace LumNotas.Core.Gestion;

/// <summary>
/// Qué hay que escribir cuando se suelta la barra de un trabajo con varias familias.
/// <para>
/// La barra abarca de la primera familia a la última, así que al arrastrarla ya no basta
/// con escribir las fechas de una: <b>mover el trabajo mueve a todas</b>, y estirar un
/// borde solo toca la del extremo que se ha estirado. Sin esto, la barra volvería a su
/// sitio al soltarla —se dibuja de las fechas de todas y solo se guardaban las de una— y
/// parecería que el arrastre no funciona.
/// </para>
/// <para>
/// Vive en el núcleo por lo mismo que el resto del gesto: en este equipo no se pueden
/// automatizar los eventos de ratón, así que la única forma de comprobarlo es que la
/// decisión esté fuera de la interfaz.
/// </para>
/// </summary>
public static class RepartoDelArrastre
{
    /// <summary>
    /// Las planificaciones a guardar, con su proyecto. Vacío si no hay nada que cambiar.
    /// </summary>
    /// <param name="inicio">El inicio nuevo del trabajo entero, tras el arrastre.</param>
    /// <param name="fin">Y el fin nuevo.</param>
    public static IReadOnlyList<(ResumenDeProyecto Proyecto, Planificacion Plan)> Aplicar(
        EntradaDeCalendario entrada, DateTime? inicio, DateTime? fin)
    {
        if (inicio is not { } nuevoInicio || fin is not { } nuevoFin) return [];
        if (entrada.Inicio is not { } viejoInicio || entrada.Fin is not { } viejoFin) return [];

        var deltaInicio = nuevoInicio - viejoInicio;
        var deltaFin = nuevoFin - viejoFin;

        if (deltaInicio == TimeSpan.Zero && deltaFin == TimeSpan.Zero) return [];

        // Los dos bordes se han movido lo mismo: se ha arrastrado el trabajo entero, así
        // que todas las familias se desplazan con él y las distancias entre ellas se
        // mantienen. Es lo que espera quien mueve un trabajo de semana.
        if (deltaInicio == deltaFin) return Desplazar(entrada, deltaInicio);

        // Un solo borde: se ha estirado. Solo cambia la familia de ese extremo.
        return deltaInicio == TimeSpan.Zero
            ? Estirar(entrada.EnOrden[^1], p => p.Fin = nuevoFin)
            : Estirar(entrada.EnOrden[0], p => p.Inicio = nuevoInicio);
    }

    private static IReadOnlyList<(ResumenDeProyecto, Planificacion)> Desplazar(
        EntradaDeCalendario entrada, TimeSpan delta)
    {
        var cambios = new List<(ResumenDeProyecto, Planificacion)>();

        foreach (var miembro in entrada.EnOrden)
        {
            var plan = miembro.Planificacion;
            if (plan.Inicio is null && plan.Fin is null) continue;   // sin fechas, nada que mover

            var nueva = plan.Copia();
            if (plan.Inicio is { } i) nueva.Inicio = i + delta;
            if (plan.Fin is { } f) nueva.Fin = f + delta;

            cambios.Add((miembro, nueva));
        }

        return cambios;
    }

    private static IReadOnlyList<(ResumenDeProyecto, Planificacion)> Estirar(
        ResumenDeProyecto miembro, Action<Planificacion> ajustar)
    {
        var nueva = miembro.Planificacion.Copia();
        ajustar(nueva);
        return [(miembro, nueva)];
    }
}
