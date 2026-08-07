namespace LumNotas.App;

/// <summary>
/// Lo que explican los dos diálogos que planifican un servicio: «Nueva toma de notas» y
/// «Planificación del servicio».
/// <para>
/// <b>Están escritos una sola vez a propósito</b> (2026‑08‑07). Los dos diálogos hacen lo
/// mismo y hasta ahora lo contaban con palabras distintas, cada uno en cuatro sitios —el
/// XAML y el código de cada uno—. Con el texto copiado, corregir uno y olvidar el otro no
/// es un descuido posible sino lo que pasa siempre: es exactamente como el diálogo de
/// filtros se quedó dos días diciendo algo que había dejado de ser cierto.
/// </para>
/// <para>
/// Se leen también desde el XAML, con <c>{x:Static}</c>, para que no haya una copia del
/// rótulo inicial esperando a separarse de la que pone el código al vaciar la casilla.
/// </para>
/// </summary>
public static class TextosDePlanificacion
{
    /// <summary>
    /// Qué hace escribir un nombre de grupo.
    /// <para>
    /// <b>Dice que cada familia lleve sus fechas y su importe</b>, que es lo contrario de lo
    /// que decía antes. Lo de antes —«las fechas y el importe se ponen solo en una de
    /// ellas»— describía el diseño original, cuando el grupo tenía una cabecera que llevaba
    /// los datos por todas. Hoy no es así: el calendario encadena las familias <b>cada una
    /// durando lo suyo</b> (DD‑123) y la carga cuenta <b>cada familia con su importe</b>,
    /// porque cuatro familias son cuatro ensayos que ocupan cuatro veces. Una familia sin
    /// fechas propias sale dibujada de prestado y no suma nada en la carga.
    /// </para>
    /// </summary>
    public const string Grupo =
        "Escribe el mismo nombre en las tomas de notas que sean del mismo servicio para "
        + "tratarlas como una sola. En el calendario se verán juntas, pero para que el "
        + "programa funcione correctamente, cada una de ellas deberá tener sus fechas y su "
        + "importe.";

    /// <summary>Con qué se enlaza lo que se está escribiendo.</summary>
    public static string GrupoEscrito(string nombre)
        => $"Se enlazará con las que lleven «{nombre}». No hace falta escribirlo igual: "
           + "no se distinguen mayúsculas, espacios ni guiones.";

    /// <summary>
    /// Qué hace archivar. <b>Dice cómo volver a verlo</b>, que es la pregunta que deja
    /// colgada: antes remitía a un botón «Ver archivados» que ya no existe — lo sustituyeron
    /// los filtros compartidos de las cuatro vistas.
    /// </summary>
    public const string Archivar =
        "Archiva la toma de notas para que no aparezca en la planificación. Para que "
        + "salga utiliza los filtros «Archivados» o «Cualquier estado», o desarchívala.";
}
