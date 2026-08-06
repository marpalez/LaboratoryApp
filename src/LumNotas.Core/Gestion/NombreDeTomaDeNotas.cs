namespace LumNotas.Core.Gestion;

/// <summary>
/// Cómo se llama el fichero de una toma de notas.
/// <para>
/// El nombre lo fija el laboratorio y se lee de un vistazo en el explorador, sin abrir
/// nada: <c>TdN_60598_TECNO260201-00.lmnlab</c> dice que es una toma de notas, de qué
/// norma y de cuál de las familias del servicio.
/// </para>
/// <para>
/// Se compone aquí y no en la ventana porque lo usan dos caminos —el alta rápida y el
/// «Guardar como» de la toma de notas— y tienen que producir exactamente lo mismo.
/// </para>
/// </summary>
public static class NombreDeTomaDeNotas
{
    /// <summary>Delata qué es el fichero antes de abrirlo.</summary>
    public const string Encabezado = "TdN_";

    /// <summary>
    /// El nombre completo, sin extensión.
    /// <para>
    /// Ejemplo: norma <c>60598</c> y toma de notas <c>TECNO260201-00</c> dan
    /// <c>TdN_60598_TECNO260201-00</c>.
    /// </para>
    /// <para>
    /// <b>El código entra tal cual.</b> Antes el programa le pegaba un <c>xx-00</c>
    /// —número de familia y edición— que el técnico tenía que sustituir renombrando el
    /// fichero; ahora esas dos parejas van dentro del código de la toma de notas, que se
    /// teclea una vez en la cabecera y ya no hay que renombrar nada.
    /// </para>
    /// </summary>
    public static string Componer(string? normaId, string? codigoTomaDeNotas)
    {
        var norma = Limpiar(normaId);
        var codigo = Limpiar(codigoTomaDeNotas);

        // Sin norma no se deja un «TdN__…» con el hueco a la vista, y sin código tampoco
        // se deja el «_» colgando. El «TdN_» se queda siempre: es lo que dice qué es el
        // fichero sin abrirlo.
        if (norma.Length == 0) return Encabezado + codigo;

        return Encabezado + norma + (codigo.Length > 0 ? "_" + codigo : "");
    }

    /// <summary>Lo mismo, con la extensión puesta.</summary>
    public static string ConExtension(string? normaId, string? codigoTomaDeNotas, string extension)
        => Componer(normaId, codigoTomaDeNotas) + extension;

    private static string Limpiar(string? texto)
        => string.IsNullOrWhiteSpace(texto)
            ? ""
            : new string([.. texto.Trim().Where(c => !Path.GetInvalidFileNameChars().Contains(c))]);
}
