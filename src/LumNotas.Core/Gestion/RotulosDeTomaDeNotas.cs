namespace LumNotas.Core.Gestion;

/// <summary>
/// Cómo se identifica una toma de notas abierta: la lengüeta de su pestaña y el título
/// que la encabeza.
/// <para>
/// Vive en el núcleo y no en la ventana por el mismo motivo que
/// <see cref="NombreDeTomaDeNotas"/>: son dos cadenas con reglas —qué se enseña cuando
/// falta el código, dónde va la marca de sin guardar— y ninguna prueba toca la interfaz.
/// Aquí se pueden comprobar.
/// </para>
/// </summary>
public static class RotulosDeTomaDeNotas
{
    /// <summary>Las partes se separan con esto y nunca con un punto: el punto ya significa «sin guardar».</summary>
    public const string Separador = " | ";

    /// <summary>La marca de que hay cambios sin guardar. Va siempre la última.</summary>
    public const string Marca = " •";

    /// <summary>Lo que se lee en la lengüeta cuando todavía no hay nada abierto.</summary>
    public const string PestanaVacia = "Nueva pestaña";

    private const string SinCodigoEnPestana = "Sin código";
    private const string SinCodigoEnTitulo = "sin código";

    /// <summary>
    /// La lengüeta de la pestaña: <b>solo el código de la toma de notas</b>.
    /// <para>
    /// Antes decía el código de <i>servicio</i> y la norma. Con un trabajo de cuatro
    /// familias abiertas, las cuatro pestañas ponían lo mismo y no se distinguían entre
    /// sí; el código de la toma de notas es justo lo que las diferencia.
    /// </para>
    /// </summary>
    public static string Pestana(string? codigoTomaDeNotas, bool cambiosSinGuardar)
        => Texto(codigoTomaDeNotas, SinCodigoEnPestana) + Si(cambiosSinGuardar);

    /// <summary>
    /// El título que encabeza la toma de notas abierta: <b>la designación de la norma</b>
    /// y el código.
    /// <para>
    /// La norma va entera —<c>EN IEC 60598-1:2024 + A11:2024</c>— y no como «Luminarias»:
    /// el laboratorio tiene dos años de la misma norma instalados a la vez, y anotar
    /// contra el año que no era es el error que esto evita.
    /// </para>
    /// </summary>
    public static string Titulo(string? norma, string? codigoTomaDeNotas, bool cambiosSinGuardar)
    {
        var codigo = Texto(codigoTomaDeNotas, SinCodigoEnTitulo);

        return string.IsNullOrWhiteSpace(norma)
            ? codigo + Si(cambiosSinGuardar)
            : norma.Trim() + Separador + codigo + Si(cambiosSinGuardar);
    }

    private static string Texto(string? codigo, string cuandoFalta)
        => string.IsNullOrWhiteSpace(codigo) ? cuandoFalta : codigo.Trim();

    private static string Si(bool cambiosSinGuardar) => cambiosSinGuardar ? Marca : "";
}
