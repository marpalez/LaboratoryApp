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
        var datos = new DatosProyecto { CodigoTomaDeNotas = "12345202601-00", CodigoServicio = "123452026", NumeroMuestras = 3, Clase = Clase.I };
        datos.IpSegundaCifra.Add("IPX3");
        datos.IpPrimeraCifra.Add("IP5X");
        datos.Partes2.Add("-2-3");

        datos.Seleccion("acreditacion").Add("ENAC");
        datos.Seleccion("acreditacion").Add("CB");

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

    /// <summary>
    /// <b>La acreditación sale en la exportación.</b> Este documento no es un certificado:
    /// es la toma de notas puesta en limpio para que el director técnico la verifique
    /// antes de firmarla, y para eso tiene que ver contra qué acreditación se ensayó.
    /// Salen todas las marcadas, que pueden ser varias.
    /// </summary>
    [Fact]
    public void LaExportacionDiceLaAcreditacion()
    {
        var html = Html(ProyectoRelleno());

        Assert.Contains("Acreditación", html);
        Assert.Contains("ENAC", html);
        Assert.Contains("CB", html);
    }

    /// <summary>Sin marcar nada no se inventa una acreditación: se deja el guion.</summary>
    [Fact]
    public void SinAcreditacionNoSeInventaNinguna()
    {
        var datos = ProyectoRelleno();
        datos.Seleccion("acreditacion").Clear();

        var html = Html(datos);

        Assert.Contains("Acreditación", html);
        Assert.DoesNotContain("ENAC", html);
    }

    /// <summary>
    /// <b>Una fila por muestra, con sus grados.</b> Antes iban todas juntas en una línea
    /// —«IP2X, IPX0»—, que engaña dos veces: mezcla las dos cifras de un mismo grado y
    /// junta los de muestras distintas. Un servicio puede traer una IP65 y otra IP20, y
    /// quien firma necesita saber cuál es cuál.
    /// </summary>
    [Fact]
    public void HayUnaTablaConUnaFilaPorMuestra()
    {
        var datos = ProyectoRelleno();
        datos.Establecer("proyecto", "ipPrimeraCifra", "IP6X", 1);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX5", 1);
        datos.Establecer("proyecto", "gradoIk", "IK08", 1);
        datos.Establecer("proyecto", "ipPrimeraCifra", "IP2X", 2);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX0", 2);

        var html = Html(datos);

        Assert.Contains("<h3>Muestras</h3>", html);
        Assert.Contains("EBP_SAFE12345202601", html);
        Assert.Contains("IP65", html);   // la primera
        Assert.Contains("IP20", html);   // la segunda, distinta
        Assert.Contains("IK08", html);
    }

    /// <summary>
    /// Que el ensayo saliera de casa consta en lo que se firma, con qué se subcontrató y
    /// por qué. Es lo primero que pregunta una auditoría sobre subcontratación.
    /// </summary>
    [Fact]
    public void LaExportacionDiceLosLaboratoriosExternos()
    {
        var datos = ProyectoRelleno();
        datos.Colaboradores.Add(new Colaborador
        {
            Laboratorio = "IMQ Italia",
            EnsayoYMotivo = "Fotobiología — no tenemos cámara"
        });

        var html = Html(datos);

        Assert.Contains("Laboratorios externos", html);
        Assert.Contains("IMQ Italia", html);
        Assert.Contains("no tenemos cámara", html);
    }

    /// <summary>
    /// Sin ninguno se dice que no hay, con un guion. Un hueco en blanco no distingue
    /// «no se subcontrató nada» de «nadie lo rellenó».
    /// </summary>
    [Fact]
    public void SinLaboratoriosExternosSeDiceQueNoHay()
    {
        var html = Html(ProyectoRelleno());

        Assert.Contains("Laboratorios externos", html);
        Assert.DoesNotContain("IMQ", html);
    }

    /// <summary>
    /// <b>Con qué se generó el documento.</b> La versión de plantilla dice contra qué
    /// reglas se midió; la del programa, con qué software se produjo. Las dos son parte
    /// del rastro que pide la ISO 17025 sobre validación de software, y sin la segunda el
    /// documento no dice de dónde salió.
    /// </summary>
    [Fact]
    public void LaExportacionDiceLaVersionDelProgramaYLaDeLaPlantilla()
    {
        var html = new ExportadorDeInforme(Contexto.Plantilla) { VersionDelPrograma = "9.9.9" }
            .GenerarHtml(ProyectoRelleno());

        Assert.Contains("Versión del programa", html);
        Assert.Contains("9.9.9", html);

        // Y la norma con su nombre completo al lado de su versión de plantilla.
        Assert.Contains("Normas y plantillas", html);
        Assert.Contains(Contexto.Plantilla.Meta.Titulo!, html);
        Assert.Contains("· plantilla " + Contexto.Plantilla.Meta.Version, html);
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
        var html = Html(new DatosProyecto { CodigoTomaDeNotas = "00000202601-00", CodigoServicio = "000002026", NumeroMuestras = 1 });

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
