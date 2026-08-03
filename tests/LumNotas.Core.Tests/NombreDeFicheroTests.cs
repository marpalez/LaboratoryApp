using LumNotas.Core.Datos;
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
            "TdN_60598_LEDC42502xx-00",
            NombreDeTomaDeNotas.Componer("60598", "LEDC42502"));

    /// <summary>
    /// El <c>xx</c> y el <c>00</c> se crean así y <b>los lleva el técnico a mano</b>: el
    /// hueco se sustituye por el número que le toque a esa toma de notas y la revisión
    /// sube cuando hay que corregir algo ya emitido. Numerar y reeditar son decisiones
    /// del laboratorio, no del programa.
    /// </summary>
    [Fact]
    public void ElHuecoYLaRevisionSeCreanParaQueLosPongaElTecnico()
    {
        Assert.EndsWith("xx-00", NombreDeTomaDeNotas.Componer("60598", "LEDC42502"));

        // Y se puede componer con lo que el técnico haya decidido.
        Assert.Equal("TdN_60598_LEDC4250203-01",
            NombreDeTomaDeNotas.Componer("60598", "LEDC42502", "03-01"));
    }

    /// <summary>Las cuatro normas instaladas dan nombre, sin que ninguna sea un caso aparte.</summary>
    [Fact]
    public void LasCuatroNormasInstaladasDanNombre()
    {
        foreach (var plantilla in Contexto.TodasLasPlantillas())
        {
            var nombre = NombreDeTomaDeNotas.Componer(plantilla.Meta.Id, "LEDC42502");

            Assert.Equal($"TdN_{plantilla.Meta.Id}_LEDC42502xx-00", nombre);
        }
    }

    [Fact]
    public void LlevaLaExtensionCuandoSePide()
        => Assert.Equal(
            "TdN_60598_LEDC42502xx-00.lumproj",
            NombreDeTomaDeNotas.ConExtension("60598", "LEDC42502", ".lumproj"));

    /// <summary>
    /// Sin norma elegida —el alta lo permite— no se deja un «TdN__» con el hueco a la
    /// vista, pero el «TdN_» no se pierde: es lo que dice qué es el fichero. Y sin código
    /// todavía tampoco se cuela el marcador «NUEVO».
    /// </summary>
    [Fact]
    public void SinNormaOSinCodigoElNombreSigueSiendoDecente()
    {
        Assert.Equal("TdN_LEDC42502xx-00", NombreDeTomaDeNotas.Componer(null, "LEDC42502"));
        Assert.Equal("TdN_60598_xx-00", NombreDeTomaDeNotas.Componer("60598", ""));

        Assert.Equal("TdN_60598_xx-00",
            NombreDeTomaDeNotas.Componer("60598", RequisitosDelProyecto.CodigoSinAsignar));
    }

    /// <summary>
    /// El código lo teclea una persona y «LEDC/42502» es una forma natural de escribirlo;
    /// no puede reventar al componer la ruta.
    /// </summary>
    [Fact]
    public void UnCodigoConBarrasNoRompeLaRuta()
    {
        var nombre = NombreDeTomaDeNotas.Componer("60598", "LEDC/425:02");

        Assert.DoesNotContain('/', nombre);
        Assert.DoesNotContain(':', nombre);
        Assert.Equal(-1, nombre.IndexOfAny(Path.GetInvalidFileNameChars()));
    }
}
