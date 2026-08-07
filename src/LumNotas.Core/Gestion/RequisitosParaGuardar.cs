using LumNotas.Core.Datos;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Lo mínimo para que un <c>.lmnlab</c> pueda escribirse en disco: <b>el código de la toma
/// de notas —entero— y el técnico 1</b>.
/// <para>
/// No es lo mismo que <see cref="RequisitosDelProyecto"/>, que dice cuándo se puede
/// empezar a ensayar: un servicio a medias es el estado normal durante semanas y tiene que
/// poder guardarse. Estos dos son otra cosa — <b>sin ellos el fichero no se puede ni
/// nombrar ni atribuir</b>: el código es lo que le da nombre y lo que lo distingue de las
/// otras familias del trabajo, y el técnico 1 es de quién es.
/// </para>
/// <para>
/// <b>El código se exige entero, no solo escrito</b> (2026‑08‑06, decisión del
/// laboratorio). Antes bastaba con que tuviera algo, para no dejar sin guardar a los
/// proyectos anteriores a la regla de los 14 caracteres; con la excepción puesta, esos
/// proyectos se quedaban con el código a medias para siempre, porque nada obligaba nunca a
/// completarlo. Ahora hay que arreglarlo <b>antes de poder escribir</b>: el aviso dice qué
/// falta y la vista salta a la cabecera, donde está en rojo.
/// </para>
/// <para>
/// Son los mismos dos que ya exigía el alta rápida (<see cref="AltaDeProyecto"/>), y es a
/// propósito: lo que nace por un camino y lo que nace por el otro tiene que ser igual de
/// identificable.
/// </para>
/// <para>
/// <b>No afecta a la cadena del grupo</b> (DD‑123): recolocar las fechas de un trabajo
/// enlazado escribe solo la planificación por otro camino, así que una familia con el
/// código a medias sigue pudiendo recibirla.
/// </para>
/// </summary>
public static class RequisitosParaGuardar
{
    /// <summary>Qué falta para poder guardar. Vacío si se puede.</summary>
    public static IReadOnlyList<string> Faltan(DatosProyecto datos)
    {
        var faltan = new List<string>();

        if (!CodigoDeServicio.EstaCompleto(datos.CodigoTomaDeNotas)) faltan.Add(AltaDeProyecto.CampoNombre);
        if (string.IsNullOrWhiteSpace(datos.Tecnico1)) faltan.Add(AltaDeProyecto.CampoTecnico);

        return faltan;
    }

    public static bool SePuede(DatosProyecto datos) => Faltan(datos).Count == 0;

    /// <summary>Lo que se le dice al técnico cuando no se puede guardar todavía.</summary>
    public static string Aviso(DatosProyecto datos)
    {
        var faltan = Faltan(datos);
        if (faltan.Count == 0) return "";

        // «Completar» y no «sin»: el código puede estar escrito y aun así estar a medias,
        // y «no se puede guardar sin código» delante de un campo con texto no se entiende.
        // El nombre del apartado no se escribe aquí: lo pone quien también rotula el
        // nodo del índice, o el aviso acaba mandando a una pantalla que ya no se llama así.
        var donde = faltan.Count == 1
            ? $"Está marcado en rojo en «{AltaDeProyecto.SeccionDeDatos}»."
            : $"Están marcados en rojo en «{AltaDeProyecto.SeccionDeDatos}».";

        return "No se puede guardar hasta completar: "
               + string.Join(" y ", faltan).ToLowerInvariant() + ". " + donde;
    }
}
