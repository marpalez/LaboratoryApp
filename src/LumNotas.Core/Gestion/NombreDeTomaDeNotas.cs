using LumNotas.Core.Datos;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Cómo se llama el fichero de una toma de notas.
/// <para>
/// El nombre lo fija el laboratorio y se lee de un vistazo en el explorador, sin abrir
/// nada: <c>TdN_60598_LEDC42502xx-00.lumproj</c> dice que es una toma de notas, de qué
/// norma, de qué servicio y en qué edición va.
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
    /// Número de toma de notas y edición, tal y como se crea el fichero.
    /// <para>
    /// Las dos partes <b>las lleva el técnico a mano</b>, renombrando el fichero: el
    /// <c>xx</c> es un hueco que se sustituye por el número que le toque —un servicio
    /// puede llevar varias familias— y el <c>00</c> es la edición, que sube a <c>01</c>
    /// cuando hay que corregir algo ya emitido. El programa no las gestiona a propósito:
    /// numerar y reeditar son decisiones del laboratorio, no del software.
    /// </para>
    /// </summary>
    public const string SufijoPorDefecto = "xx-00";

    /// <summary>
    /// El nombre completo, sin extensión.
    /// <para>
    /// Ejemplo: norma <c>60598</c> y servicio <c>LEDC42502</c> dan
    /// <c>TdN_60598_LEDC42502xx-00</c>.
    /// </para>
    /// </summary>
    public static string Componer(string? normaId, string? codigoServicio,
                                  string sufijo = SufijoPorDefecto)
    {
        var norma = Limpiar(normaId);
        var cuerpo = Limpiar(CodigoUtil(codigoServicio)) + sufijo;

        // Sin norma no se deja un «TdN__…» con el hueco a la vista, pero el «TdN_» se
        // queda: es lo que dice qué es el fichero sin abrirlo.
        return Encabezado + (norma.Length > 0 ? norma + "_" : "") + cuerpo;
    }

    /// <summary>Lo mismo, con la extensión puesta.</summary>
    public static string ConExtension(string? normaId, string? codigoServicio,
                                      string extension, string sufijo = SufijoPorDefecto)
        => Componer(normaId, codigoServicio, sufijo) + extension;

    /// <summary>El código, salvo cuando todavía no hay: el marcador no es un código.</summary>
    private static string CodigoUtil(string? codigo)
        => string.IsNullOrWhiteSpace(codigo) || codigo == RequisitosDelProyecto.CodigoSinAsignar
            ? ""
            : codigo.Trim();

    private static string Limpiar(string? texto)
        => string.IsNullOrWhiteSpace(texto)
            ? ""
            : new string([.. texto.Trim().Where(c => !Path.GetInvalidFileNameChars().Contains(c))]);
}
