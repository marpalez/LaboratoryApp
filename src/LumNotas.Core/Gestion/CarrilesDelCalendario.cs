namespace LumNotas.Core.Gestion;

/// <summary>
/// Reparte los trabajos del calendario en <b>carriles</b>: filas compartidas por todos los
/// que no se pisan entre sí.
/// <para>
/// Antes cada trabajo se llevaba su propia fila. Un técnico con veinte proyectos daba
/// veinte renglones aunque fueran uno detrás de otro y no coincidieran nunca, así que el
/// calendario se leía en vertical —bajando— cuando lo que se quiere leer es el tiempo, que
/// va en horizontal. Colocados por carriles, esos mismos veinte caben en una sola fila y
/// <b>lo que se cuenta hacia abajo pasa a ser cuántos trabajos coinciden a la vez</b>, que
/// es justo lo que el responsable necesita ver.
/// </para>
/// <para>
/// La regla es la de siempre en estos repartos: se recorren por fecha de inicio y cada uno
/// cae en <b>el primer carril donde quepa</b>. Tomando el más alto disponible en cada paso
/// no hace falta probar combinaciones: salen tantos carriles como trabajos coincidan en el
/// día más cargado, que es el mínimo posible.
/// </para>
/// </summary>
public static class CarrilesDelCalendario
{
    /// <summary>
    /// Coloca los trabajos y devuelve los carriles de arriba abajo, cada uno con los suyos
    /// en el orden en que se dibujan.
    /// </summary>
    /// <param name="tramo">
    /// De dónde a dónde ocupa cada trabajo. Las dos fechas van <b>incluidas</b>: un trabajo
    /// de un solo día tiene el mismo inicio que fin.
    /// </param>
    public static IReadOnlyList<IReadOnlyList<T>> Repartir<T>(
        IEnumerable<T> trabajos, Func<T, (DateTime Desde, DateTime Hasta)> tramo)
    {
        var carriles = new List<List<T>>();

        // El día en que se queda libre cada carril. Se lleva aparte para no tener que
        // repasar lo que ya hay puesto en cada trabajo nuevo.
        var ocupadoHasta = new List<DateTime>();

        var enOrden = trabajos
            .Select(t => (Trabajo: t, Tramo: Normalizar(tramo(t))))
            .OrderBy(x => x.Tramo.Desde)
            .ThenBy(x => x.Tramo.Hasta);

        foreach (var (trabajo, (desde, hasta)) in enOrden)
        {
            var carril = ocupadoHasta.FindIndex(fin => fin < desde);

            if (carril < 0)
            {
                carril = carriles.Count;
                carriles.Add([]);
                ocupadoHasta.Add(hasta);
            }
            else
            {
                ocupadoHasta[carril] = hasta;
            }

            carriles[carril].Add(trabajo);
        }

        return carriles;
    }

    /// <summary>
    /// <b>Compartir un solo día ya es pisarse.</b> Dos barras pegadas sin un hueco entre
    /// ellas se leen como una sola barra larga, así que el día de fin cuenta como ocupado y
    /// el siguiente trabajo del carril tiene que empezar al menos un día después.
    /// <para>
    /// La hora se descarta: las fechas del calendario son días, y una guardada con hora
    /// mandaría a un carril nuevo a un trabajo que empieza cuando el anterior ya ha
    /// terminado.
    /// </para>
    /// </summary>
    private static (DateTime Desde, DateTime Hasta) Normalizar((DateTime Desde, DateTime Hasta) tramo)
    {
        var desde = tramo.Desde.Date;
        var hasta = tramo.Hasta.Date;

        // Un fin anterior al inicio es un dato mal guardado; se trata como un solo día en
        // vez de dejar el carril ocupado hacia atrás, que descolocaría a todos los demás.
        return (desde, hasta < desde ? desde : hasta);
    }
}
