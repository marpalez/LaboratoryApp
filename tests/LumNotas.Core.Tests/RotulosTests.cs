using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Cómo se identifica una toma de notas abierta: la lengüeta de su pestaña y el título
/// que la encabeza.
/// </summary>
public class RotulosTests
{
    // ---- la pestaña --------------------------------------------------------

    /// <summary>
    /// <b>Lo que arregla este rótulo.</b> Un trabajo puede llevar cuatro familias, y con
    /// las cuatro abiertas las pestañas decían todas «TECNO2602 | Luminarias»: no había
    /// forma de saber en cuál se estaba. El código de la toma de notas es justo lo que las
    /// diferencia.
    /// </summary>
    [Fact]
    public void CadaFamiliaDelMismoTrabajoTieneSuPropiaPestana()
    {
        var una = RotulosDeTomaDeNotas.Pestana("TECNO260201-00", cambiosSinGuardar: false);
        var otra = RotulosDeTomaDeNotas.Pestana("TECNO260202-00", cambiosSinGuardar: false);

        Assert.Equal("TECNO260201-00", una);
        Assert.NotEqual(una, otra);
    }

    [Fact]
    public void SinCodigoLaPestanaLoDice()
        => Assert.Equal("Sin código", RotulosDeTomaDeNotas.Pestana("", cambiosSinGuardar: false));

    /// <summary>El punto va el último, también cuando no hay código.</summary>
    [Fact]
    public void LaMarcaDeSinGuardarVaAlFinal()
    {
        Assert.Equal("TECNO260201-00 •", RotulosDeTomaDeNotas.Pestana("TECNO260201-00", true));
        Assert.Equal("Sin código •", RotulosDeTomaDeNotas.Pestana(null, true));
    }

    // ---- el título ---------------------------------------------------------

    /// <summary>
    /// <b>La norma va entera y con su año.</b> El laboratorio tiene dos años de la 60598
    /// instalados a la vez; con «Luminarias» en el título, anotar contra el año que no era
    /// no se vería hasta emitir el informe.
    /// </summary>
    [Fact]
    public void ElTituloDiceLaNormaConSuAnio()
    {
        var titulo = RotulosDeTomaDeNotas.Titulo(
            "EN IEC 60598-1:2024 + A11:2024", "TECNO260201-00", cambiosSinGuardar: false);

        Assert.Equal("EN IEC 60598-1:2024 + A11:2024 | TECNO260201-00", titulo);
    }

    [Fact]
    public void LosDosAnosDeLaMismaNormaNoSeConfunden()
        => Assert.NotEqual(
            RotulosDeTomaDeNotas.Titulo("EN IEC 60598-1:2024 + A11:2024", "TECNO260201-00", false),
            RotulosDeTomaDeNotas.Titulo("EN IEC 60598-1:2021 + A11:2022", "TECNO260201-00", false));

    [Fact]
    public void SinCodigoElTituloLoDice()
        => Assert.Equal("EN 60529:2018 | sin código",
            RotulosDeTomaDeNotas.Titulo("EN 60529:2018", "", cambiosSinGuardar: false));

    [Fact]
    public void ElTituloTambienLlevaLaMarcaAlFinal()
        => Assert.Equal("EN 60529:2018 | sin código •",
            RotulosDeTomaDeNotas.Titulo("EN 60529:2018", null, cambiosSinGuardar: true));

    /// <summary>Sin norma no se deja un «| » colgando delante.</summary>
    [Fact]
    public void SinNormaElTituloNoDejaSeparadorSuelto()
    {
        var titulo = RotulosDeTomaDeNotas.Titulo("", "TECNO260201-00", cambiosSinGuardar: false);

        Assert.Equal("TECNO260201-00", titulo);
        Assert.DoesNotContain(RotulosDeTomaDeNotas.Separador, titulo);
    }

    // ---- de dónde sale la designación --------------------------------------

    /// <summary>
    /// Las cinco plantillas instaladas declaran su designación, y en todas se puede leer
    /// el año: es lo único que distingue dos plantillas de la misma norma.
    /// </summary>
    [Fact]
    public void TodasLasNormasDicenSuDesignacionConElAnio()
    {
        foreach (var plantilla in Contexto.TodasLasPlantillas())
        {
            var norma = plantilla.Meta.ComoSeLlamaLaNorma;
            var anio = plantilla.Meta.AnioDePublicacion;

            Assert.False(string.IsNullOrWhiteSpace(norma), $"{plantilla.Meta.Id} no dice cómo se llama.");
            Assert.Contains(anio!, norma);

            // Y es la designación, no el nombre comercial: no se cuela el título corto.
            Assert.DoesNotContain("—", norma);
        }
    }

    /// <summary>
    /// <b>Las normas instaladas se distinguen entre sí.</b> «Acerca de» las lista para
    /// responder qué hay instalado, y con el nombre corto las dos plantillas de la 60598
    /// salían las dos como «Luminarias v1.0.0»: la ventana que existe para saberlo era
    /// justo la que no lo decía.
    /// </summary>
    [Fact]
    public void LaListaDeNormasInstaladasNoTieneDosIguales()
    {
        var instaladas = Plantilla.CatalogoDeNormas.Disponibles(Contexto.CarpetaDePlantillas())
            .Select(n => $"{n.ComoSeLlama} v{n.Version}")
            .ToList();

        Assert.Equal(instaladas.Count, instaladas.Distinct().Count());
        Assert.Contains(instaladas, t => t.Contains("60598-1:2024"));
        Assert.Contains(instaladas, t => t.Contains("60598-1:2021"));
    }

    /// <summary>
    /// Una plantilla que no declare designación sigue funcionando: se usa lo que tenga.
    /// Añadir una norma no puede exigir rellenar todos los campos nuevos.
    /// </summary>
    [Fact]
    public void UnaPlantillaSinDesignacionUsaLoQueTenga()
    {
        var meta = new Plantilla.Meta { Id = "99999_2030", Titulo = "Algo — EN 99999:2030" };

        Assert.Equal("Algo — EN 99999:2030", meta.ComoSeLlamaLaNorma);
        Assert.Equal("99999_2030", new Plantilla.Meta { Id = "99999_2030" }.ComoSeLlamaLaNorma);
    }
}
