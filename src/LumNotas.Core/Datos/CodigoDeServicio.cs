namespace LumNotas.Core.Datos;

/// <summary>
/// De dónde sale el código de servicio: de las primeras del código de la toma de notas.
/// <para>
/// En el laboratorio un servicio puede llevar varias familias de luminarias, cada una con
/// su toma de notas. Todas comparten el código de servicio y se distinguen por lo que va
/// detrás: <c>TECNO2602</c> es el servicio, y <c>TECNO260201-00</c> y
/// <c>TECNO260202-00</c> son dos de sus tomas de notas.
/// </para>
/// <para>
/// Está aquí y no en la ventana porque la relación entre los dos códigos es del negocio,
/// y porque la usan tres caminos —el alta rápida, la cabecera de la toma de notas y el
/// nombre del fichero— que tienen que dar exactamente lo mismo.
/// </para>
/// </summary>
public static class CodigoDeServicio
{
    /// <summary>
    /// Cuántas del código de la toma de notas forman el del servicio. Es del laboratorio:
    /// cinco letras del cliente y cuatro cifras de año y mes.
    /// </summary>
    public const int Longitud = 9;

    /// <summary>
    /// Cuánto ocupa el servicio más el número de familia: <c>TECNO260201</c>. Es lo que
    /// identifica <b>una familia concreta</b> de un trabajo, sin la edición del documento.
    /// </summary>
    public const int LongitudConFamilia = 11;

    /// <summary>
    /// Lo que ocupa un código entero: <c>TECNO260201-00</c>. Los once de arriba, el guion y
    /// dos de edición del documento.
    /// </summary>
    public const int LongitudCompleta = 14;

    /// <summary>
    /// Si el código está entero. <b>Vacío no lo está, y a medias tampoco.</b>
    /// <para>
    /// De este código salen otros tres —el de servicio, el de familia y el nombre del
    /// fichero—, y los tres se recortan de él: uno corto los deja a los tres mal, y
    /// corregirlo después obliga a renombrar. Por eso se exige exacto y no «al menos».
    /// </para>
    /// </summary>
    public static bool EstaCompleto(string? codigoTomaDeNotas)
        => (codigoTomaDeNotas ?? "").Trim().Length == LongitudCompleta;

    /// <summary>
    /// El código de servicio que le corresponde a esa toma de notas. Si todavía es más
    /// corto que <see cref="Longitud"/> devuelve lo que haya, para que la cabecera se
    /// vaya rellenando mientras se teclea en vez de quedarse en blanco hasta el final.
    /// </summary>
    public static string Derivar(string? codigoTomaDeNotas)
        => Recortar(codigoTomaDeNotas, Longitud);

    /// <summary>
    /// El servicio con su número de familia, sin la edición: de <c>TECNO260201-00</c>
    /// queda <c>TECNO260201</c>.
    /// <para>
    /// Es lo que encabeza cada tarjeta del tablero y cada barra del calendario. El de
    /// servicio a secas no valía —las cuatro familias de un trabajo se llamaban igual y no
    /// se distinguían—, y el completo tampoco: el <c>-00</c> es la edición del documento,
    /// que se corrige por una errata del técnico y no dice nada de qué hay que ensayar.
    /// </para>
    /// </summary>
    public static string ConFamilia(string? codigoTomaDeNotas)
        => Recortar(codigoTomaDeNotas, LongitudConFamilia);

    private static string Recortar(string? codigo, int cuantas)
    {
        var texto = (codigo ?? "").Trim();
        return texto.Length <= cuantas ? texto : texto[..cuantas];
    }

    /// <summary>
    /// Qué código de servicio dejar cuando cambia el de la toma de notas.
    /// <para>
    /// <b>No se pisa lo que haya escrito una persona.</b> Se rellena solo si estaba vacío
    /// o si lo que hay es exactamente lo que se dedujo del código anterior —es decir, si
    /// lo puso el programa y nadie lo ha tocado—. Un servicio cuyo código no son las nueve
    /// primeras existe, y corregirlo a mano tiene que aguantar que se siga escribiendo
    /// arriba.
    /// </para>
    /// </summary>
    /// <param name="anterior">Cómo era el código de la toma de notas antes del cambio.</param>
    /// <param name="nuevo">Cómo es ahora.</param>
    /// <param name="servicioActual">Lo que hay puesto en el código de servicio.</param>
    public static string Sugerir(string? anterior, string? nuevo, string? servicioActual)
    {
        var actual = (servicioActual ?? "").Trim();

        var loPusoElPrograma = actual.Length == 0
                               || actual == RequisitosDelProyecto.CodigoSinAsignar
                               || actual == Derivar(anterior);

        return loPusoElPrograma ? Derivar(nuevo) : actual;
    }
}
