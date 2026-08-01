using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Motor;

public enum EstadoApartado { SinReglas, FaltanDatos, Completo, NoAplica }

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

        return motor.EsVerdadera(regla) ? EstadoApartado.FaltanDatos : EstadoApartado.Completo;
    }

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
