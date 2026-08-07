namespace LumNotas.Core.Datos;

/// <summary>
/// Se ha intentado escribir una toma de notas que nació con una versión posterior del
/// programa. No se escribe, para no borrarle lo que este programa no sabe interpretar.
/// </summary>
public sealed class TomaDeNotasMasNuevaException(string mensaje) : Exception(mensaje);

/// <summary>
/// La marca de formato del <c>.lmnlab</c> —<c>lmnlab/1</c>— y qué hacer cuando el fichero
/// viene de una versión posterior.
/// <para>
/// <b>Por qué existe.</b> El laboratorio tiene seis equipos y se actualizan de uno en uno,
/// así que <b>habrá días con dos versiones conviviendo</b>: eso no es una avería, es el
/// estado normal de un despliegue. Lo que no puede pasar es que la versión de antes
/// destruya en silencio el trabajo de la de después.
/// </para>
/// <para>
/// Hay dos defensas y hacen falta las dos. La primera es conservar lo desconocido
/// (<c>[JsonExtensionData]</c>), que resuelve el caso corriente: campos nuevos sueltos que
/// van y vienen intactos. La segunda es esta, y cubre lo que la primera no puede: un
/// cambio de forma —un campo que pasa a significar otra cosa, una lista que se parte en
/// dos— donde conservar el texto no basta porque el programa viejo <b>sí</b> entiende el
/// nombre y lo interpreta mal. Ahí lo único correcto es no tocar el fichero.
/// </para>
/// <para>
/// <b>Solo frena al escribir.</b> Leer y mirar un fichero más nuevo se permite: dejar a un
/// técnico sin poder consultar un ensayo porque su equipo va una versión por detrás sería
/// peor que el problema, y leyendo no se rompe nada.
/// </para>
/// </summary>
public static class FormatoDeFichero
{
    /// <summary>La marca que escribe esta versión.</summary>
    public const string Actual = "lmnlab/1";

    /// <summary>
    /// Hasta qué número se sabe escribir. <b>Se sube a la vez que se cambia la forma del
    /// fichero</b>, no antes ni después: subirlo sin cambiar nada deja a los demás equipos
    /// sin poder guardar sin motivo, y cambiar la forma sin subirlo devuelve el problema
    /// que esto viene a resolver.
    /// </summary>
    public const int VersionQueSeEntiende = 1;

    /// <summary>
    /// El número de una marca —<c>lmnlab/2</c> → 2—, o 1 si no se entiende.
    /// <para>
    /// Ante una marca rara devuelve 1 y no un número alto: un fichero con la marca
    /// estropeada es un fichero viejo o tocado a mano, y bloquear el guardado por eso
    /// dejaría al técnico sin poder trabajar sobre un ensayo perfectamente legible.
    /// </para>
    /// </summary>
    public static int NumeroDe(string? marca)
    {
        if (string.IsNullOrWhiteSpace(marca)) return 1;

        var barra = marca.LastIndexOf('/');
        return barra >= 0 && int.TryParse(marca[(barra + 1)..].Trim(), out var numero) && numero > 0
            ? numero
            : 1;
    }

    /// <summary>Si el fichero viene de una versión que este programa no sabe escribir.</summary>
    public static bool EsMasNuevo(string? marca) => NumeroDe(marca) > VersionQueSeEntiende;

    /// <summary>
    /// Corta el guardado si el fichero es más nuevo. El mensaje va dirigido al técnico
    /// —dice qué hacer, no qué ha fallado—, porque es el que lo va a leer en un aviso.
    /// </summary>
    public static void ExigirQueSePuedaEscribir(string? marca, string ruta)
    {
        if (!EsMasNuevo(marca)) return;

        throw new TomaDeNotasMasNuevaException(
            $"Esta toma de notas se guardó con una versión más nueva del programa "
            + $"(formato {marca}). No se ha escrito nada, para no estropearla. "
            + $"Actualiza este equipo y vuelve a intentarlo.{Environment.NewLine}{ruta}");
    }
}
