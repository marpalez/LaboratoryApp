using System.Globalization;
using System.Text;
using LumNotas.Core.Gestion;

namespace LumNotas.Report;

/// <summary>
/// Exporta el listado de la BBDD a un HTML con estilos de impresión <b>A4 apaisado</b>.
/// <para>
/// Mismo formato que el informe de ensayo y por el mismo motivo (DD‑06): es solo texto, sin
/// ninguna dependencia externa, y desde el fichero se llega al PDF en un clic —Ctrl+P en
/// cualquier navegador— o a Excel abriéndolo desde ahí.
/// </para>
/// <para>
/// <b>Apaisado y no vertical</b>: son once columnas. En A4 vertical quedan 180 mm útiles y
/// la tabla habría que partirla o encoger la letra hasta no leerse; apaisado hay 267 mm.
/// </para>
/// <para>
/// <b>La cabecera lleva el título y la cuenta, y nada más</b> (revisado con el laboratorio
/// el 2026‑08‑07). Se probó con tres cosas más —la fecha y hora de generación, la línea de
/// filtros y un aviso de que no es un informe— y las tres se quitaron: el listado se mira
/// en el momento y junto a la pantalla de la que sale, así que repetir ahí lo que ya se
/// sabe solo robaba sitio a la tabla.
/// </para>
/// <para>
/// <b>Por eso el título tampoco lleva fecha.</b> Los navegadores imprimen el
/// <c>&lt;title&gt;</c> en la cabecera de cada hoja, así que dejarlo ahí habría devuelto a
/// la hoja la fecha que se acababa de quitar del documento.
/// </para>
/// </summary>
public sealed class ExportadorDeListado
{
    public const string Extension = ".html";

    public void Exportar(IReadOnlyList<FilaDeBbdd> filas, string ruta)
        => File.WriteAllText(ruta, GenerarHtml(filas), new UTF8Encoding(false));

    public string GenerarHtml(IReadOnlyList<FilaDeBbdd> filas)
    {
        var columnas = FilaDeBbdd.Columnas;
        var total = columnas.Sum(c => c.Ancho);

        var h = new StringBuilder();

        h.AppendLine("<!DOCTYPE html>");
        h.AppendLine("<html lang=\"es\"><head><meta charset=\"utf-8\">");
        h.AppendLine("<title>Listado de tomas de notas</title>");
        h.AppendLine($"<style>{Estilos}</style>");
        h.AppendLine("</head><body>");

        h.AppendLine("<header>");
        h.AppendLine("<h1>LISTADO DE TOMAS DE NOTAS</h1>");
        h.AppendLine($"<p class=\"sub\">{E(Recuento(filas.Count))}</p>");
        h.AppendLine("</header>");

        if (filas.Count == 0)
        {
            h.AppendLine("<p class=\"vacio\">No hay ninguna toma de notas que encaje con lo que se estaba buscando</p>");
            h.AppendLine("</body></html>");
            return h.ToString();
        }

        h.AppendLine("<table>");

        // Los anchos van en porcentaje del total: así la tabla ocupa el ancho de la página
        // sea cual sea el número de columnas, y añadir una no obliga a recalcular nada.
        h.AppendLine("<colgroup>");
        foreach (var columna in columnas)
            h.AppendLine($"<col style=\"width:{(columna.Ancho / total * 100).ToString("0.##", CultureInfo.InvariantCulture)}%\">");
        h.AppendLine("</colgroup>");

        // La cabecera se repite en cada página impresa: con doscientas filas, a partir de
        // la segunda hoja no habría forma de saber qué columna es cuál.
        h.AppendLine("<thead><tr>");
        foreach (var columna in columnas) h.AppendLine($"<th>{E(columna.Titulo)}</th>");
        h.AppendLine("</tr></thead>");

        h.AppendLine("<tbody>");
        foreach (var fila in filas)
        {
            h.AppendLine("<tr>");
            foreach (var columna in columnas) h.AppendLine($"<td>{E(columna.Valor(fila))}</td>");
            h.AppendLine("</tr>");
        }
        h.AppendLine("</tbody>");

        h.AppendLine("</table>");
        h.AppendLine("</body></html>");
        return h.ToString();
    }

    private static string Recuento(int cuantas)
        => cuantas == 1 ? "1 toma de notas" : $"{cuantas} tomas de notas";

    /// <summary>
    /// Escapa solo lo que rompe el HTML, igual que el informe: el documento declara UTF‑8,
    /// así que convertir los acentos a entidades solo lo haría ilegible en un editor.
    /// </summary>
    private static string E(string? texto) => (texto ?? "")
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");

    private const string Estilos = """
        @page { size: A4 landscape; margin: 12mm 10mm; }

        body { font-family: "Segoe UI", Arial, sans-serif; font-size: 8pt; color: #111827;
               line-height: 1.35; margin: 0; padding: 10mm; background: #fff; }

        header { border-bottom: 3px solid #111827; padding-bottom: 8px; margin-bottom: 14px; }
        h1 { font-size: 15pt; margin: 0 0 4px; letter-spacing: 0.5px; }
        .sub { margin: 0; color: #4B5563; font-size: 9pt; }

        .vacio { color: #6B7280; font-style: italic; }

        table { width: 100%; border-collapse: collapse; table-layout: fixed; }

        th { text-align: left; font-size: 7.5pt; letter-spacing: 0.3px; color: #374151;
             border-bottom: 2px solid #9CA3AF; padding: 4px 5px; }

        td { border-bottom: 1px solid #E5E7EB; padding: 3px 5px; vertical-align: top;
             word-wrap: break-word; overflow-wrap: anywhere; }

        /* Una fila no se parte entre dos hojas, y la cabecera vuelve a salir en cada una */
        tr { page-break-inside: avoid; }
        thead { display: table-header-group; }

        tbody tr:nth-child(even) { background: #F8FAFC; }
        """;
}
