using LumNotas.Core.Gestion;
using LumNotas.Report;

namespace LumNotas.Core.Tests;

/// <summary>
/// El listado de la BBDD exportado a HTML (DD‑140).
/// <para>
/// Lo que se vigila aquí es que el papel diga <b>lo mismo que la pantalla</b>: las mismas
/// columnas, las mismas filas —todas las que quedan tras el filtro, no las que caben en el
/// monitor— y, escrito encima, qué filtros estaban puestos.
/// </para>
/// </summary>
public class ListadoExportadoTests
{
    private static ResumenDeProyecto Proyecto(
        string codigo, string tecnico = "M. Madrigal", bool archivado = false)
        => new()
        {
            Ruta = $"C:/x/{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            Tecnico = tecnico,
            NormaPrincipal = "EN IEC 60598-1:2024 + A11:2024",
            NumeroMuestras = 3,
            GradoIp = "IP54",
            GradoIk = "IK08",
            Acreditaciones = ["ENAC"],
            Modificado = new DateTime(2026, 8, 1),
            Planificacion = new Planificacion
            {
                Archivado = archivado,
                Estado = EstadoDeProyecto.EnCurso,
                EnsayoDesde = new DateTime(2026, 7, 1),
                EnsayoHasta = new DateTime(2026, 7, 9)
            }
        };

    private static IReadOnlyList<FilaDeBbdd> Filas(params ResumenDeProyecto[] proyectos)
        => [.. proyectos.Select(p => new FilaDeBbdd(p))];

    private static string Html(IReadOnlyList<FilaDeBbdd> filas)
        => new ExportadorDeListado().GenerarHtml(filas);

    // ---- se exporta el listado entero, no lo que se ve ----------------------

    /// <summary>
    /// <b>Lo pidió el laboratorio con estas palabras</b>: si el filtro deja cuatro, cuatro;
    /// si no hay filtro y hay cien, cien. Es un cuidado real y no una obviedad — la tabla
    /// está virtualizada (DD‑131), así que en pantalla <b>solo existen las filas que caben</b>.
    /// Exportar recorriendo lo dibujado daría quince y parecería correcto.
    /// </summary>
    [Fact]
    public void SalenTodasLasFilasDelListadoAunqueNoQuepanEnLaPantalla()
    {
        var muchas = Filas([.. Enumerable.Range(1, 100).Select(i => Proyecto($"TECNO2602{i:00}-00"))]);

        var html = Html(muchas);

        Assert.Contains("100 tomas de notas", html);
        foreach (var i in new[] { 1, 15, 16, 50, 100 })
            Assert.Contains($"TECNO2602{i:00}-00", html);

        // Y una fila por proyecto, ni una de más.
        Assert.Equal(100, ContarFilas(html));
    }

    [Fact]
    public void ConCuatroFilasSalenCuatro()
    {
        var html = Html(Filas([.. Enumerable.Range(1, 4).Select(i => Proyecto($"LEDCA26010{i}-00"))]));

        Assert.Contains("4 tomas de notas", html);
        Assert.Equal(4, ContarFilas(html));
    }

    /// <summary>Una sola no se anuncia en plural.</summary>
    [Fact]
    public void ConUnaSolaSeDiceEnSingular()
    {
        Assert.Contains("1 toma de notas", Html(Filas(Proyecto("ANTAR260101-00"))));
    }

    // ---- las mismas columnas que la pantalla -------------------------------

    /// <summary>
    /// Las columnas salen de <see cref="FilaDeBbdd.Columnas"/>, que es la misma lista que
    /// dibuja la tabla. Si alguien añade una columna a la pantalla y no al papel, o al revés,
    /// es que ha dejado de usar la lista — y esto lo caza.
    /// </summary>
    [Fact]
    public void ElPapelLlevaLasMismasColumnasQueLaPantalla()
    {
        var html = Html(Filas(Proyecto("TECNO260201-00")));

        foreach (var columna in FilaDeBbdd.Columnas)
            Assert.Contains($"<th>{columna.Titulo}</th>", html);

        Assert.Equal(FilaDeBbdd.Columnas.Count, ContarCabeceras(html));
    }

    [Fact]
    public void CadaFilaTraeLoQueEnsenaSuColumna()
    {
        var html = Html(Filas(Proyecto("TECNO260201-00", "J. Salvador")));

        foreach (var esperado in new[] { "TECNO260201-00", "J. Salvador", "ENAC", "IP54", "IK08",
                                         "En curso", "01/07/2026", "3" })
            Assert.Contains(esperado, html);
    }

    /// <summary>Lo archivado se dice, igual que en la pantalla.</summary>
    [Fact]
    public void LoArchivadoSaleComoArchivado()
    {
        Assert.Contains("Archivado", Html(Filas(Proyecto("VIEJO260101-00", archivado: true))));
    }

    // ---- la cabecera: título y cuenta, y nada más --------------------------

    /// <summary>
    /// El laboratorio revisó el listado el 2026‑08‑07 y quitó las tres cosas que llevaba
    /// además del título: la fecha y hora de generación, la línea de filtros y el aviso de
    /// que no es un informe. <b>Este test las mantiene fuera</b>, porque las tres son de las
    /// que se vuelven a colar «por si acaso» al tocar la cabecera.
    /// </summary>
    [Fact]
    public void LaCabeceraLlevaSoloElTituloYLaCuenta()
    {
        var html = Html(Filas(Proyecto("TECNO260201-00")));

        Assert.Contains("LISTADO DE TOMAS DE NOTAS", html);
        Assert.Contains("1 toma de notas", html);

        foreach (var fuera in new[] { "generado", "Mostrando", "Documento interno",
                                      "informe de ensayo", "class=\"filtros\"", "class=\"aviso\"" })
            Assert.DoesNotContain(fuera, html, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Y el título tampoco lleva fecha: el navegador imprime el <c>&lt;title&gt;</c> en la
    /// cabecera de cada hoja, así que dejarla ahí devolvería al papel lo que se acaba de
    /// quitar del documento.
    /// </summary>
    [Fact]
    public void ElTituloDeLaPaginaNoLlevaFecha()
    {
        Assert.Contains("<title>Listado de tomas de notas</title>",
                        Html(Filas(Proyecto("TECNO260201-00"))));
    }

    [Fact]
    public void SinFilasElPapelLoDiceEnVezDeSalirEnBlanco()
    {
        var html = Html([]);

        Assert.Contains("No hay ninguna toma de notas", html);
        Assert.DoesNotContain("<table>", html);
    }

    // ---- el formato --------------------------------------------------------

    /// <summary>Once columnas no caben en A4 vertical: el CSS tiene que pedir apaisado.</summary>
    [Fact]
    public void SeImprimeEnA4Apaisado()
    {
        Assert.Contains("size: A4 landscape", Html(Filas(Proyecto("TECNO260201-00"))));
    }

    /// <summary>
    /// Con doscientas filas la tabla ocupa varias hojas, y a partir de la segunda no habría
    /// forma de saber qué columna es cuál sin repetir la cabecera.
    /// </summary>
    [Fact]
    public void LaCabeceraSeRepiteEnCadaHoja()
    {
        Assert.Contains("thead { display: table-header-group; }",
                        Html(Filas(Proyecto("TECNO260201-00"))));
    }

    /// <summary>Lo que rompería el HTML se escapa; los acentos no, que el fichero es UTF‑8.</summary>
    [Fact]
    public void LoQueRomperiaElHtmlSeEscapa()
    {
        var html = Html(Filas(Proyecto("<script>", "Muñoz & Pérez")));

        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("Muñoz &amp; Pérez", html);
        Assert.DoesNotContain("<script>", html);
    }

    private static int ContarFilas(string html)
        => html.Split("<tr>").Length - 1 - ContarCabecerasDeFila(html);

    private static int ContarCabecerasDeFila(string html)
        => html.Contains("<thead>") ? 1 : 0;

    private static int ContarCabeceras(string html) => html.Split("<th>").Length - 1;
}
