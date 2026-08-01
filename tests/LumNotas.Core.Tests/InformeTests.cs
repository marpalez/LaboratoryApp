using LumNotas.Core.Datos;
using LumNotas.Report;

namespace LumNotas.Core.Tests;

/// <summary>
/// El informe exportable (DD-06). Se genera como HTML con estilos de impresión A4,
/// y desde ahí Word o el navegador producen el PDF que se firma en papel (DD-07).
/// Al ser texto, los tests pueden comprobar el contenido y no solo que el fichero exista.
/// </summary>
public class InformeTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-inf-" + Guid.NewGuid().ToString("N"));

    public InformeTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    private static string Html(DatosProyecto datos)
        => new ExportadorDeInforme(Contexto.Plantilla).GenerarHtml(datos);

    private static DatosProyecto ProyectoRelleno()
    {
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 3, Clase = Clase.I };
        datos.IpSegundaCifra.Add("IPX3");
        datos.IpPrimeraCifra.Add("IP5X");
        datos.Partes2.Add("-2-3");

        datos.Establecer("proyecto", "tecnico1", "A. Pérez");
        datos.Establecer("proyecto", "ta", 25.0);

        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));
        datos.Establecer("6", "ambiente.temperatura", 23.4);
        datos.Establecer("6", "ambiente.humedad", 48.0);

        datos.Establecer("generales", "tamano[0].alto", 120.0, 1);
        datos.Establecer("generales", "tamano[0].ancho", 60.0, 1);
        datos.Establecer("generales", "tamano[0].profundo", 80.0, 1);

        datos.Establecer("7.12.1", "tornillos[0].parte", "Carcasa", 1);
        datos.Establecer("7.12.1", "tornillos[0].diametro", 4.0, 1);
        datos.Marcar("7.12.portalamparas", "portalamparas", "e40");

        datos.Marcar("15.2.1", "origenMuestra", "corteEbp");
        datos.Establecer("15.2.1", "espesor", 3.2, 1);

        return datos;
    }

    [Fact]
    public void SeEscribeElFicheroYEsUnDocumentoHtml()
    {
        var ruta = Path.Combine(_carpeta, "toma-de-notas" + ExportadorDeInforme.Extension);
        new ExportadorDeInforme(Contexto.Plantilla).Exportar(ProyectoRelleno(), ruta);

        Assert.True(File.Exists(ruta));
        var contenido = File.ReadAllText(ruta);
        Assert.StartsWith("<!DOCTYPE html>", contenido);
        Assert.Contains("</html>", contenido);
    }

    [Fact]
    public void LlevaLosEstilosDeImpresionA4VerticalYUnSaltoPorApartado()
    {
        var html = Html(ProyectoRelleno());

        Assert.Contains("size: A4 portrait", html);
        Assert.Contains("page-break-before: always", html);
    }

    [Fact]
    public void LaPortadaIdentificaElProyectoYLaVersionDePlantilla()
    {
        var html = Html(ProyectoRelleno());

        Assert.Contains("123452026", html);
        Assert.Contains("EBP_SAFE123452026", html);
        Assert.Contains("A. Pérez", html);
        Assert.Contains(Contexto.Plantilla.Meta.Version, html);
    }

    [Fact]
    public void SaleUnApartadoPorCadaBloqueDeLaPlantilla()
    {
        var html = Html(ProyectoRelleno());

        var apartados = html.Split("class=\"apartado\"").Length - 1;
        Assert.Equal(Contexto.Plantilla.Bloques().Count(), apartados);

        foreach (var bloque in Contexto.Plantilla.Bloques())
            Assert.Contains(bloque.Titulo, html);
    }

    [Fact]
    public void LosDatosIntroducidosAparecenEnElInforme()
    {
        var html = Html(ProyectoRelleno());

        Assert.Contains("Carcasa", html);        // parte del tornillo 1
        Assert.Contains("20/07/2026", html);     // fecha del marcado
        Assert.Contains("23,4", html);           // temperatura ambiente
        Assert.Contains("E40", html);            // checklist de portalámparas marcado
    }

    [Fact]
    public void UnaColumnaPorMuestra()
    {
        var html = Html(ProyectoRelleno());   // 3 muestras

        Assert.Contains("<th>01</th>", html);
        Assert.Contains("<th>03</th>", html);
        Assert.DoesNotContain("<th>04</th>", html);
    }

    [Fact]
    public void ConOchoMuestrasSalenLasOchoColumnas()
    {
        var datos = ProyectoRelleno();
        datos.NumeroMuestras = 8;

        var html = Html(datos);
        Assert.Contains("<th>08</th>", html);
    }

    /// <summary>
    /// La numeración puede no empezar en 1: si la primera muestra del servicio es la 03,
    /// las columnas y los identificadores deben seguir esa numeración, no la posición.
    /// </summary>
    [Fact]
    public void LaNumeracionPersonalizadaSaleEnLasColumnasYEnLaPortada()
    {
        var datos = ProyectoRelleno();        // 3 muestras
        datos.EstablecerNumeroDeMuestra(1, 3);
        datos.EstablecerNumeroDeMuestra(2, 4);
        datos.EstablecerNumeroDeMuestra(3, 5);

        var html = Html(datos);

        Assert.Contains("<th>03</th>", html);
        Assert.Contains("<th>05</th>", html);
        Assert.DoesNotContain("<th>01</th>", html);
        Assert.Contains("EBP_SAFE12345202603", html);
        Assert.Contains("EBP_SAFE12345202605", html);
    }

    [Fact]
    public void UnProyectoVacioTambienSeExportaYAvisaDeLoQueFalta()
    {
        // Imprimir a mitad de ensayo es habitual: no puede fallar por faltar datos.
        var html = Html(new DatosProyecto { CodigoServicio = "000002026", NumeroMuestras = 1 });

        Assert.Contains("quedan apartados con datos pendientes", html);
        Assert.Contains("FALTAN DATOS EN ESTE APARTADO", html);
    }

    [Fact]
    public void UnApartadoMarcadoComoNaSaleIdentificadoYSinTablas()
    {
        var datos = ProyectoRelleno();
        datos.EstablecerNa("6/na", true);

        var html = Html(datos);
        Assert.Contains("APARTADO NO APLICABLE", html);
    }

    [Fact]
    public void ElTextoDelProyectoSeEscapaParaNoRomperElHtml()
    {
        var datos = ProyectoRelleno();
        datos.Establecer("7.12.1", "tornillos[0].parte", "Tornillo <M4> & tuerca", 1);

        var html = Html(datos);
        Assert.Contains("Tornillo &lt;M4&gt; &amp; tuerca", html);
        Assert.DoesNotContain("<M4>", html);
    }
}
