namespace LumNotas.Core.Datos;

/// <summary>
/// Marcar y desmarcar en un campo de varias opciones donde <b>una de ellas excluye a las
/// demás</b>. En la acreditación esa opción es «Sin acreditar»: un servicio puede ser ENAC
/// y ENEC a la vez, pero no puede ser ENAC y estar sin acreditar.
/// <para>
/// La regla vive aquí y no en la ventana porque quién excluye a quién <b>lo declara la
/// plantilla</b> (<c>opcionExcluyente</c>): un campo nuevo con la misma forma no tiene que
/// tocar código. Y porque marcar una casilla que borra otras es justo lo que conviene
/// tener probado.
/// </para>
/// </summary>
public static class SeleccionExcluyente
{
    /// <summary>
    /// Aplica al conjunto lo que acaba de hacer el técnico. Si marca la excluyente, se
    /// queda ella sola; si marca cualquier otra, la excluyente se cae.
    /// </summary>
    /// <param name="excluyente">
    /// La opción que no admite compañía, o <c>null</c> si en ese campo ninguna lo es —y
    /// entonces esto es un marcar y desmarcar corriente.
    /// </param>
    public static void Aplicar(ISet<string> conjunto, string opcion, bool marcada, string? excluyente)
    {
        if (!marcada)
        {
            conjunto.Remove(opcion);
            return;
        }

        var esLaExcluyente = excluyente is not null
                             && string.Equals(opcion, excluyente, StringComparison.OrdinalIgnoreCase);

        if (esLaExcluyente) conjunto.Clear();
        else if (excluyente is not null) conjunto.Remove(excluyente);

        conjunto.Add(opcion);
    }
}
