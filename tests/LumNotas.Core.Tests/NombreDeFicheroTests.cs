using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Cómo se llama el fichero de una toma de notas. El nombre lo fija el laboratorio y se
/// lee de un vistazo en el explorador, sin abrir nada.
/// </summary>
public class NombreDeFicheroTests
{
    /// <summary>El ejemplo que dio el laboratorio, tal cual.</summary>
    [Fact]
    public void ElEjemploDelLaboratorio()
        => Assert.Equal(
            "TdN_60598_TECNO260201-00",
            NombreDeTomaDeNotas.Componer("60598", "TECNO260201-00"));

    /// <summary>
    /// El número de familia y la edición <b>van dentro del código de la toma de notas</b>
    /// y entran tal cual. El programa ya no pega ningún <c>xx-00</c>: antes lo hacía y el
    /// técnico tenía que sustituirlo renombrando el fichero, y ahora se teclea una vez en
    /// la cabecera y no hay que renombrar nada.
    /// </summary>
    [Fact]
    public void ElCodigoEntraTalCualSinAnadirleNada()
    {
        Assert.Equal("TdN_60598_TECNO260203-01",
            NombreDeTomaDeNotas.Componer("60598", "TECNO260203-01"));

        Assert.DoesNotContain("xx", NombreDeTomaDeNotas.Componer("60598", "TECNO260201-00"));
    }

    /// <summary>Las normas instaladas dan nombre, sin que ninguna sea un caso aparte.</summary>
    [Fact]
    public void LasNormasInstaladasDanNombre()
    {
        foreach (var plantilla in Contexto.TodasLasPlantillas())
        {
            var nombre = NombreDeTomaDeNotas.Componer(plantilla.Meta.Id, "TECNO260201-00");

            Assert.Equal($"TdN_{plantilla.Meta.Id}_TECNO260201-00", nombre);
        }
    }

    [Fact]
    public void LlevaLaExtensionCuandoSePide()
        => Assert.Equal(
            "TdN_60598_TECNO260201-00.lmnlab",
            NombreDeTomaDeNotas.ConExtension("60598", "TECNO260201-00", ".lmnlab"));

    /// <summary>
    /// <b>El fichero de trabajo y la exportación se llaman igual</b>, solo cambia la
    /// extensión: son el mismo ensayo. Con nombres distintos —«Toma de notas ANTAR2504»
    /// frente a «TdN_60598_ANTAR250401-00»— no se emparejaban ni ordenados en la carpeta
    /// ni de un vistazo, y el HTML es lo que el director técnico revisa y firma.
    /// </summary>
    [Fact]
    public void LaExportacionSeLlamaIgualQueElFicheroDeTrabajo()
    {
        var trabajo = NombreDeTomaDeNotas.ConExtension("60598", "TECNO260201-00", ".lmnlab");
        var exportado = NombreDeTomaDeNotas.ConExtension("60598", "TECNO260201-00", ".html");

        Assert.Equal(Path.GetFileNameWithoutExtension(trabajo),
                     Path.GetFileNameWithoutExtension(exportado));

        Assert.Equal("TdN_60598_TECNO260201-00.html", exportado);
    }

    /// <summary>
    /// Sin norma no se deja un «TdN__» con el hueco a la vista, y sin código todavía no se
    /// deja el «_» colgando. El «TdN_» no se pierde en ningún caso: es lo que dice qué es
    /// el fichero sin abrirlo.
    /// </summary>
    [Fact]
    public void SinNormaOSinCodigoElNombreSigueSiendoDecente()
    {
        Assert.Equal("TdN_TECNO260201-00", NombreDeTomaDeNotas.Componer(null, "TECNO260201-00"));
        Assert.Equal("TdN_60598", NombreDeTomaDeNotas.Componer("60598", ""));
        Assert.Equal("TdN_", NombreDeTomaDeNotas.Componer(null, null));
    }

    /// <summary>
    /// El código lo teclea una persona y «TECNO/2602» es una forma natural de escribirlo;
    /// no puede reventar al componer la ruta.
    /// </summary>
    [Fact]
    public void UnCodigoConBarrasNoRompeLaRuta()
    {
        var nombre = NombreDeTomaDeNotas.Componer("60598", "TECNO/2602:01-00");

        Assert.DoesNotContain('/', nombre);
        Assert.DoesNotContain(':', nombre);
        Assert.Equal(-1, nombre.IndexOfAny(Path.GetInvalidFileNameChars()));
    }
}
