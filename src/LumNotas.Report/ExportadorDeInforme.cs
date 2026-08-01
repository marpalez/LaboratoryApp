using System.Text;
using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Report;

/// <summary>
/// Exporta la toma de notas a un documento HTML con estilos de impresión A4 vertical.
/// <para>
/// Se eligió HTML deliberadamente en lugar de una librería de PDF: <b>no hay ninguna
/// dependencia externa</b>, es solo texto, y desde el fichero generado se llega a los dos
/// formatos que el laboratorio necesita en un clic — Word lo abre como documento y permite
/// «Guardar como PDF», y cualquier navegador lo imprime a PDF con Ctrl+P.
/// </para>
/// <para>
/// El CSS declara <c>@page A4 portrait</c> y un salto de página antes de cada apartado,
/// así que impreso sale exactamente como pedía DD-06 y DD-15.
/// </para>
/// </summary>
public sealed class ExportadorDeInforme(PlantillaEnsayos plantilla, CatalogoDeEquipos? catalogo = null)
{
    public const string Extension = ".html";

    private readonly CatalogoDeEquipos _catalogo = catalogo ?? CatalogoDeEquipos.Vacio;

    /// <summary>Una norma añadida al proyecto, con su propio catálogo de equipos.</summary>
    public sealed record NormaAdicional(PlantillaEnsayos Plantilla, CatalogoDeEquipos Catalogo);

    /// <summary>
    /// Normas añadidas a la principal. Un servicio de luminarias puede llevar también la
    /// 62031 o el IK 62262, y el informe tiene que traerlas todas: es el registro
    /// primario del ensayo completo, no de una norma suelta.
    /// </summary>
    public IReadOnlyList<NormaAdicional> Adicionales { get; init; } = [];

    public void Exportar(DatosProyecto datos, string ruta)
        => File.WriteAllText(ruta, GenerarHtml(datos), new UTF8Encoding(false));

    public string GenerarHtml(DatosProyecto datos)
    {
        var normas = new List<NormaAdicional> { new(plantilla, _catalogo) };
        normas.AddRange(Adicionales);

        var motores = normas.ToDictionary(n => n, n => new MotorDeReglas(n.Plantilla, datos));
        var avance = IndicadorDeAvance.Resultado.Sumar(
            motores.Values.Select(m => new IndicadorDeAvance(m).Calcular()));

        var h = new StringBuilder();

        h.AppendLine("<!DOCTYPE html>");
        h.AppendLine("<html lang=\"es\"><head><meta charset=\"utf-8\">");
        h.AppendLine($"<title>Toma de notas {E(datos.CodigoServicio)}</title>");
        h.AppendLine($"<style>{Estilos}</style>");
        h.AppendLine("</head><body>");

        Portada(h, datos, avance, normas);

        foreach (var norma in normas)
        {
            // Con una sola norma no se pone separador: el informe queda como siempre.
            if (normas.Count > 1) SeparadorDeNorma(h, norma.Plantilla);

            foreach (var seccion in norma.Plantilla.Secciones)
                foreach (var bloque in seccion.Bloques)
                    Apartado(h, motores[norma], norma.Catalogo, datos, seccion, bloque);
        }

        h.AppendLine("</body></html>");
        return h.ToString();
    }

    /// <summary>Portadilla de cada norma, en su propia página, cuando el proyecto lleva varias.</summary>
    private static void SeparadorDeNorma(StringBuilder h, PlantillaEnsayos norma)
    {
        h.AppendLine("<section class=\"apartado norma\">");
        h.AppendLine($"<h1>{E(TituloDe(norma))}</h1>");
        h.AppendLine($"<p class=\"sub\">Versión de plantilla {E(norma.Meta.Version)}</p>");
        h.AppendLine("</section>");
    }

    private static string TituloDe(PlantillaEnsayos norma)
        => string.IsNullOrWhiteSpace(norma.Meta.Titulo) ? norma.Meta.Id : norma.Meta.Titulo!;

    private void Portada(StringBuilder h, DatosProyecto datos, IndicadorDeAvance.Resultado avance,
                         IReadOnlyList<NormaAdicional> normas)
    {
        h.AppendLine("<header>");
        h.AppendLine("<h1>TOMA DE NOTAS DE ENSAYOS</h1>");
        h.AppendLine($"<p class=\"sub\">{E(string.Join(" · ", normas.Select(n => TituloDe(n.Plantilla))))}</p>");
        h.AppendLine("</header>");

        h.AppendLine("<table class=\"ficha\">");
        Ficha(h, "Código de servicio", datos.CodigoServicio);
        Ficha(h, "Muestras",
            datos.NumeroMuestras == 0
                ? "—"
                : string.Join(", ", datos.Muestras.Select(datos.IdentificadorDeMuestra)));
        Ficha(h, "Técnico 1", V(datos, "proyecto", "tecnico1"));
        Ficha(h, "Técnico 2", V(datos, "proyecto", "tecnico2"));
        Ficha(h, "Nº de muestras", datos.NumeroMuestras.ToString());
        Ficha(h, "Clase", datos.Clase.ToString());
        Ficha(h, "Ta", V(datos, "proyecto", "ta"));
        // Hay normas que declaran el grado por muestra, así que se listan todos los que
        // haya en el proyecto, vengan de donde vengan.
        Ficha(h, "Grado IP objetivo",
            Lista(datos.GradosDe("ipPrimeraCifra").Concat(datos.GradosDe("ipSegundaCifra")).Distinct()));
        Ficha(h, "Partes -2 aplicables", Lista(datos.Partes2));
        if (normas.Count > 1)
            Ficha(h, "Normas incluidas", string.Join(" · ", normas.Select(n => TituloDe(n.Plantilla))));
        Ficha(h, "Versión de plantilla", plantilla.Meta.Version);
        Ficha(h, "Documento generado el", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        Ficha(h, "Estado", $"{avance.PorcentajePonderado:0}% ponderado · {avance.Contador} apartados completados");
        h.AppendLine("</table>");

        if (avance.ApartadosCompletados < avance.ApartadosAplicables)
            h.AppendLine("<p class=\"aviso\">ATENCIÓN: quedan apartados con datos pendientes. " +
                         "El detalle está en cada apartado.</p>");

        h.AppendLine("<p class=\"sub\">Este documento es el registro primario del ensayo. " +
                     "Se firma en papel una vez impreso.</p>");
    }

    private void Apartado(StringBuilder h, MotorDeReglas motor, CatalogoDeEquipos catalogoDeLaNorma,
                          DatosProyecto datos, Seccion seccion, Bloque bloque)
    {
        h.AppendLine("<section class=\"apartado\">");

        var titulo = bloque.Codigo == bloque.Titulo ? bloque.Titulo : $"{bloque.Codigo} · {bloque.Titulo}";
        h.AppendLine($"<h2>{E(titulo)}</h2>");
        h.AppendLine($"<p class=\"sub\">{E(seccion.Titulo)}</p>");

        if (datos.Na($"{bloque.Id}/{bloque.Na?.Id ?? "na"}"))
        {
            h.AppendLine("<p class=\"na\">APARTADO NO APLICABLE (N/A)</p>");
            h.AppendLine("</section>");
            return;
        }

        foreach (var regla in PlantillaEnsayos.ReglasDe(bloque))
            if (regla.Tipo == "aviso" && regla.Texto is not null && motor.EsVerdadera(regla.Id))
                h.AppendLine($"<p class=\"aviso\">{E(regla.Texto)}</p>");

        if (bloque.Ambiente is not null)
            h.AppendLine("<p class=\"condiciones\"><b>Condiciones:</b> " +
                         $"T {E(V(datos, bloque.Id, "ambiente.temperatura"))} ºC · " +
                         $"H {E(V(datos, bloque.Id, "ambiente.humedad"))} % · " +
                         $"Fecha {E(V(datos, bloque.Id, "ambiente.fecha"))}</p>");

        foreach (var nota in bloque.Notas) h.AppendLine($"<p class=\"nota\">{E(nota)}</p>");

        Grupo(h, datos, bloque.Id, bloque.Titulo, bloque.Campos, bloque.Checklists, []);
        foreach (var sub in bloque.SubBloques)
        {
            Grupo(h, datos, sub.Id, sub.Titulo, sub.Campos, sub.Checklists, sub.Notas);
            if (sub.Comentarios) Comentarios(h, datos, sub.Id);
        }

        Equipos(h, catalogoDeLaNorma, datos, bloque);
        if (bloque.Comentarios) Comentarios(h, datos, bloque.Id);

        var cierre = bloque.Reglas.LastOrDefault(r => r.Tipo == "faltanDatos")
                     ?? bloque.SubBloques.SelectMany(s => s.Reglas).LastOrDefault(r => r.Tipo == "faltanDatos");
        if (cierre is not null && motor.EsVerdadera(cierre.Id))
            h.AppendLine("<p class=\"falta\">FALTAN DATOS EN ESTE APARTADO</p>");

        h.AppendLine("</section>");
    }

    private static void Grupo(
        StringBuilder h, DatosProyecto datos, string ambito, string titulo,
        IReadOnlyList<Campo> campos, IReadOnlyList<Checklist> checklists, IReadOnlyList<string> notas)
    {
        var filas = Aplanar(campos).ToList();
        var marcadas = checklists
            .Select(c => (Lista: c, Opciones: c.Opciones.Where(o => datos.Marcada(ambito, c.Id, o.Id)).ToList()))
            .Where(x => x.Opciones.Count > 0)
            .ToList();

        if (filas.Count == 0 && marcadas.Count == 0) return;

        h.AppendLine($"<h3>{E(titulo)}</h3>");
        foreach (var nota in notas) h.AppendLine($"<p class=\"nota\">{E(nota)}</p>");

        if (filas.Count > 0)
        {
            h.AppendLine("<table class=\"datos\"><thead><tr><th>Dato</th>");
            foreach (var m in datos.Muestras)
                h.AppendLine($"<th>{E(datos.NumeroDeMuestra(m).ToString("00"))}</th>");
            h.AppendLine("</tr></thead><tbody>");

            foreach (var (etiqueta, ruta, porMuestra) in filas)
            {
                h.Append($"<tr><td class=\"etq\">{E(etiqueta)}</td>");
                if (porMuestra)
                    foreach (var m in datos.Muestras)
                        h.Append($"<td>{E(V(datos, ambito, ruta, m))}</td>");
                else
                    h.Append($"<td colspan=\"{datos.NumeroMuestras}\">{E(V(datos, ambito, ruta))}</td>");
                h.AppendLine("</tr>");
            }
            h.AppendLine("</tbody></table>");
        }

        foreach (var (lista, opciones) in marcadas)
        {
            h.Append("<p class=\"check\">");
            if (!string.IsNullOrWhiteSpace(lista.Etiqueta)) h.Append($"<b>{E(lista.Etiqueta)}:</b> ");
            h.Append(string.Join(" &nbsp;·&nbsp; ", opciones.Select(o => "☑ " + E(o.Etiqueta))));
            h.AppendLine("</p>");
        }
    }

    private static IEnumerable<(string Etiqueta, string Ruta, bool PorMuestra)> Aplanar(IReadOnlyList<Campo> campos)
    {
        foreach (var campo in campos)
        {
            if (campo.Tipo == "derivado") continue;

            if (campo.Tipo == "grupoRepetido")
            {
                var elementos = campo.Elementos ?? 1;
                for (var i = 0; i < elementos; i++)
                    foreach (var hijo in campo.Campos)
                    {
                        var prefijo = elementos > 1 ? $"{campo.Etiqueta} {i + 1}" : campo.Etiqueta;
                        yield return ($"{prefijo} · {Con(hijo)}", $"{campo.Id}[{i}].{hijo.Id}", campo.PorMuestra);
                    }
            }
            else
            {
                yield return (Con(campo), campo.Id, campo.PorMuestra);
            }
        }

        static string Con(Campo c) => c.Unidad is null ? c.Etiqueta : $"{c.Etiqueta} ({c.Unidad})";
    }

    /// <summary>Equipos marcados como utilizados, más los anotados en «otros».</summary>
    private static void Equipos(StringBuilder h, CatalogoDeEquipos catalogo, DatosProyecto datos, Bloque bloque)
    {
        var grupo = catalogo.Grupo(bloque.Equipos);
        if (grupo is null) return;

        var utilizados = grupo.Equipos
            .Where(e => datos.Marcada(bloque.Id, "equipos", e.Id))
            .ToList();

        var otros = datos.Obtener(bloque.Id, "equipos.otros") as string;
        var hayOtros = !string.IsNullOrWhiteSpace(otros);

        if (utilizados.Count == 0 && !hayOtros) return;

        h.AppendLine("<h3>Equipos utilizados</h3>");
        h.AppendLine("<table><thead><tr><th>Código</th><th>Descripción</th></tr></thead><tbody>");
        foreach (var equipo in utilizados)
            h.AppendLine($"<tr><td>{E(equipo.CodigoLiteral)}</td><td>{E(equipo.Descripcion)}</td></tr>");
        if (hayOtros)
            h.AppendLine($"<tr><td>Otros</td><td>{E(otros)}</td></tr>");
        h.AppendLine("</tbody></table>");
    }

    /// <summary>Comentarios del apartado o del subapartado, si se han escrito.</summary>
    private static void Comentarios(StringBuilder h, DatosProyecto datos, string ambito)
    {
        if (datos.Obtener(ambito, "comentarios") is not string texto || string.IsNullOrWhiteSpace(texto)) return;

        h.AppendLine("<h3>Comentarios</h3>");
        h.AppendLine($"<p class=\"comentarios\">{E(texto)}</p>");
    }

    private static void Ficha(StringBuilder h, string etiqueta, string valor)
        => h.AppendLine($"<tr><th>{E(etiqueta)}</th><td>{E(valor)}</td></tr>");

    /// <summary>
    /// El informe se formatea siempre en español, no con la configuración regional del
    /// PC: un registro de ensayo no puede cambiar de formato según la máquina que lo imprima.
    /// </summary>
    private static readonly System.Globalization.CultureInfo Formato = new("es-ES");

    private static string V(DatosProyecto datos, string ambito, string campo, int muestra = DatosProyecto.SinMuestra)
        => datos.Obtener(ambito, campo, muestra) switch
        {
            null => "—",
            DateTime d => d.TimeOfDay == TimeSpan.Zero
                ? d.ToString("dd/MM/yyyy", Formato)
                : d.ToString("dd/MM/yyyy HH:mm", Formato),
            double n => n.ToString("0.###", Formato),
            var v => v.ToString() ?? "—"
        };

    private static string Lista(IEnumerable<string> valores)
    {
        var ordenados = valores.OrderBy(v => v).ToList();
        return ordenados.Count == 0 ? "—" : string.Join(", ", ordenados);
    }

    /// <summary>
    /// Escapa solo lo que rompe el HTML. No se usa <c>WebUtility.HtmlEncode</c> a propósito:
    /// convierte los acentos en entidades numéricas («P&#233;rez») y el documento ya
    /// declara UTF-8, así que solo haría el fichero ilegible al abrirlo en un editor.
    /// </summary>
    private static string E(string? texto) => (texto ?? "")
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");

    private const string Estilos = """
        @page { size: A4 portrait; margin: 16mm 14mm; }

        body { font-family: "Segoe UI", Arial, sans-serif; font-size: 10pt; color: #111827;
               line-height: 1.45; margin: 0 auto; max-width: 195mm; padding: 14mm 12mm;
               background: #fff; }

        header { border-bottom: 3px solid #111827; padding-bottom: 10px; margin-bottom: 22px; }
        h1 { font-size: 18pt; margin: 0 0 4px; letter-spacing: .3px; }
        h2 { font-size: 14pt; margin: 0 0 3px; }
        h3 { font-size: 11pt; margin: 22px 0 6px; padding-left: 9px;
             border-left: 4px solid #9ca3af; color: #1f2937; }
        p { margin: 5px 0; }

        .sub { color: #6b7280; font-size: 9pt; margin-top: 0; }
        .nota { color: #6b7280; font-size: 8.5pt; font-style: italic; margin: 3px 0; }
        .condiciones { margin: 12px 0; padding: 7px 10px; background: #f9fafb;
                       border-left: 3px solid #d1d5db; }
        .check { margin: 7px 0; }

        table { border-collapse: collapse; width: 100%; margin: 10px 0 16px; }
        th, td { border: 1px solid #d1d5db; padding: 6px 8px; font-size: 9pt; text-align: left;
                 vertical-align: top; }
        thead th { background: #f3f4f6; }
        tbody tr:nth-child(even) { background: #fcfcfd; }
        .ficha { margin-bottom: 20px; }
        .ficha th { background: #f3f4f6; width: 34%; }
        .datos .etq { width: 38%; }

        .aviso { background: #fef3c7; border: 1px solid #f59e0b; color: #92400e;
                 padding: 10px 12px; font-weight: bold; margin: 14px 0; }
        .falta { color: #92400e; font-weight: bold; margin-top: 18px;
                 padding: 8px 12px; background: #fef2f2; border-left: 4px solid #dc2626; }
        .na { font-weight: bold; margin-top: 16px; padding: 8px 12px;
              background: #f3f4f6; border-left: 4px solid #9ca3af; }

        .comentarios { margin: 12px 0 4px; padding: 9px 12px; background: #f9fafb;
                       border: 1px solid #e5e7eb; white-space: pre-wrap; }

        /* Separación clara entre apartados en pantalla; en papel manda el salto de página. */
        .apartado { margin-top: 34px; padding-top: 22px; border-top: 2px solid #e5e7eb; }
        .apartado h2 { margin-top: 0; }

        @media print {
            body { max-width: none; padding: 0; }
            /* Cada apartado empieza en página nueva al imprimir (DD-15). */
            .apartado { page-break-before: always; break-before: page;
                        margin-top: 0; padding-top: 0; border-top: none; }
            tr, table { page-break-inside: avoid; break-inside: avoid; }
            h3 { page-break-after: avoid; break-after: avoid; }
        }
        """;
}
