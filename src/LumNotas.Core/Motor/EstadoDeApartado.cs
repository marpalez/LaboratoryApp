using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Motor;

/// <summary>
/// En qué punto está un apartado. <c>Empezado</c> va el último a propósito: añadirlo en
/// medio cambiaría el número de los que ya existían.
/// <para>
/// No hay estado de «fallo», y no es un olvido: en este laboratorio una toma de notas
/// rellena es siempre satisfactoria, así que lo único que hay que saber de un apartado
/// es cuánto le queda.
/// </para>
/// </summary>
public enum EstadoApartado { SinReglas, FaltanDatos, Completo, NoAplica, Empezado }

/// <summary>
/// Decide si un apartado se muestra y en qué estado está. Vive en el núcleo porque lo
/// necesitan dos sitios: el índice de la ventana de ensayos y el tablero de gestión.
/// </summary>
public static class EstadoDeApartado
{
    /// <summary>
    /// Un apartado se muestra salvo que su <c>visibleSi</c> diga lo contrario: los
    /// ensayos de partes -2 no marcadas, la tierra en Clase II o el doble aislamiento
    /// en Clase I no aparecen.
    /// </summary>
    public static bool EsVisible(MotorDeReglas motor, Bloque bloque)
        => bloque.VisibleSi is null || motor.EsVerdadera(bloque.VisibleSi);

    public static EstadoApartado De(MotorDeReglas motor, DatosProyecto datos, Bloque bloque)
    {
        if (datos.Na($"{bloque.Id}/{bloque.Na?.Id ?? "na"}")) return EstadoApartado.NoAplica;

        var regla = ReglaDeCierre(bloque);
        if (regla is null) return EstadoApartado.SinReglas;

        if (!motor.EsVerdadera(regla)) return EstadoApartado.Completo;

        // A la regla de cierre solo le consta que faltan datos. Que falten habiendo ya
        // algo escrito es otra cosa —hay un ensayo a medias— y el que tiene que
        // terminarlo necesita verlo.
        return HayAlgoEscrito(datos, bloque) ? EstadoApartado.Empezado : EstadoApartado.FaltanDatos;
    }

    /// <summary>
    /// Un apartado que aplica y todavía no está resuelto, esté vacío o a medias. Lo
    /// preguntan el índice y el tablero de gestión, y tiene que querer decir lo mismo en
    /// los dos: un apartado empezado sigue siendo trabajo pendiente.
    /// </summary>
    public static bool EstaPendiente(EstadoApartado estado)
        => estado is EstadoApartado.FaltanDatos or EstadoApartado.Empezado;

    // Los subapartados guardan lo suyo bajo su propio id —la sección 12 tiene tres, cada
    // uno con su fecha—, así que mirar solo el del apartado dejaría fuera casi todo.
    private static bool HayAlgoEscrito(DatosProyecto datos, Bloque bloque)
        => datos.HayAlgoEn(bloque.Id) || bloque.SubBloques.Any(sub => datos.HayAlgoEn(sub.Id));

    /// <summary>
    /// Regla que determina el estado. La plantilla puede fijarla con <c>reglaDeCierre</c>;
    /// si no, se toma la última del bloque que concluya algo: <c>faltanDatos</c> o el
    /// condicional <c>si</c> que lo envuelve cuando el apartado puede no aplicar.
    /// </summary>
    public static string? ReglaDeCierre(Bloque bloque)
    {
        if (bloque.ReglaDeCierre is not null) return bloque.ReglaDeCierre;

        static bool Concluye(Regla r) => r.Tipo is "faltanDatos" or "si";

        return bloque.Reglas.LastOrDefault(Concluye)?.Id
               ?? bloque.SubBloques.SelectMany(s => s.Reglas).LastOrDefault(Concluye)?.Id;
    }
}
