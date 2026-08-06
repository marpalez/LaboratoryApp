using LumNotas.Core.Datos;

namespace LumNotas.Core.Motor;

/// <summary>
/// Cuándo se ensayó de verdad: la primera y la última fecha que hay escritas en la toma
/// de notas.
/// <para>
/// <b>No son las fechas de la planificación.</b> Aquellas son las previstas, y se mueven
/// por el calendario mientras el trabajo no empieza; estas son las que dejó el ensayo,
/// apartado a apartado —la fecha de las condiciones, el inicio y el fin de una estancia en
/// estufa—. Para responder «¿qué se ensayó en el primer trimestre?» sirven estas.
/// </para>
/// <para>
/// Se calculan solas al dar el servicio por terminado. El técnico no escribe ni un dato
/// más: si tuviera que rellenar dos fechas a mano al cerrar, no las rellenaría, y la
/// consulta de la BBDD nacería vacía.
/// </para>
/// </summary>
public static class FechasDelEnsayo
{
    /// <summary>
    /// La más temprana y la más tardía de todo lo escrito. Devuelve <c>null</c> en las dos
    /// cuando la toma de notas no tiene ni una fecha, que es el caso de una recién abierta.
    /// </summary>
    public static (DateTime? Desde, DateTime? Hasta) De(DatosProyecto datos)
    {
        DateTime? desde = null;
        DateTime? hasta = null;

        foreach (var (_, _, _, valor) in datos.Volcar())
        {
            if (Fecha(valor) is not { } fecha) continue;

            if (desde is null || fecha < desde) desde = fecha;
            if (hasta is null || fecha > hasta) hasta = fecha;
        }

        return (desde?.Date, hasta?.Date);
    }

    /// <summary>
    /// Una fecha de verdad. <b>Los textos no cuentan</b>: en la toma de notas hay campos
    /// libres donde cabe cualquier cosa, y admitir lo que se parezca a una fecha metería
    /// en la cuenta números de serie y códigos de muestra.
    /// </summary>
    private static DateTime? Fecha(object? valor) => valor switch
    {
        DateTime fecha => fecha,
        DateTimeOffset fecha => fecha.DateTime,
        _ => null
    };
}
