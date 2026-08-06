using LumNotas.Core.Datos;

namespace LumNotas.Core.Tests;

/// <summary>
/// La acreditación del servicio: varias a la vez, salvo «Sin acreditar», que no admite
/// compañía. Quién excluye a quién lo declara la plantilla, no este código.
/// </summary>
public class AcreditacionTests
{
    private const string SinAcreditar = "Sin acreditar";

    private static ISet<string> Conjunto(params string[] valores)
        => new HashSet<string>(valores, StringComparer.OrdinalIgnoreCase);

    /// <summary>Un servicio puede ser ENAC y ENEC a la vez: eso es lo normal.</summary>
    [Fact]
    public void VariasAcreditacionesConvivenSinProblema()
    {
        var marcadas = Conjunto();

        SeleccionExcluyente.Aplicar(marcadas, "ENAC", true, SinAcreditar);
        SeleccionExcluyente.Aplicar(marcadas, "ENEC", true, SinAcreditar);
        SeleccionExcluyente.Aplicar(marcadas, "CB", true, SinAcreditar);

        Assert.Equal(3, marcadas.Count);
    }

    /// <summary>
    /// <b>Marcar «Sin acreditar» borra el resto.</b> Sin esto se podría guardar un servicio
    /// declarado a la vez como acreditado y sin acreditar, y en el listado saldrían las dos
    /// sin que nadie supiera cuál vale.
    /// </summary>
    [Fact]
    public void SinAcreditarSeQuedaSola()
    {
        var marcadas = Conjunto("ENAC", "ENEC");

        SeleccionExcluyente.Aplicar(marcadas, SinAcreditar, true, SinAcreditar);

        Assert.Equal([SinAcreditar], marcadas);
    }

    /// <summary>Y al revés: marcar una acreditación quita el «Sin acreditar».</summary>
    [Fact]
    public void MarcarUnaAcreditacionQuitaElSinAcreditar()
    {
        var marcadas = Conjunto(SinAcreditar);

        SeleccionExcluyente.Aplicar(marcadas, "ENAC", true, SinAcreditar);

        Assert.Equal(["ENAC"], marcadas);
    }

    [Fact]
    public void DesmarcarQuitaSoloEsa()
    {
        var marcadas = Conjunto("ENAC", "ENEC");

        SeleccionExcluyente.Aplicar(marcadas, "ENAC", false, SinAcreditar);

        Assert.Equal(["ENEC"], marcadas);
    }

    /// <summary>Sin opción excluyente declarada es un marcar y desmarcar corriente.</summary>
    [Fact]
    public void SinExcluyenteSeComportaComoSiempre()
    {
        var marcadas = Conjunto("-2-1");

        SeleccionExcluyente.Aplicar(marcadas, "-2-3", true, excluyente: null);

        Assert.Equal(2, marcadas.Count);
    }

    // ---- cómo la exige la cabecera -----------------------------------------

    /// <summary>
    /// Es obligatoria: sin marcar nada, la cabecera no está completa y los apartados de
    /// ensayo no aparecen. Es lo que impide que un servicio llegue al final sin que nadie
    /// haya dicho contra qué acreditación se ensayó.
    /// </summary>
    [Fact]
    public void SinAcreditacionLaCabeceraNoEstaCompleta()
    {
        var datos = Contexto.ProyectoVacio();

        Assert.Contains(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos),
            f => f.Contains("Acreditación"));
    }

    /// <summary>Con una cualquiera marcada, deja de faltar.</summary>
    [Fact]
    public void ConUnaMarcadaYaNoFalta()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Seleccion("acreditacion").Add("ENAC");

        Assert.DoesNotContain(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos),
            f => f.Contains("Acreditación"));
    }

    /// <summary>Las cinco normas instaladas la piden, no solo luminarias.</summary>
    [Fact]
    public void TodasLasNormasPidenAcreditacion()
    {
        foreach (var plantilla in Contexto.TodasLasPlantillas())
        {
            var campo = plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == "acreditacion");

            Assert.True(campo is not null, $"{plantilla.Meta.Id} no pide acreditación.");
            Assert.True(campo!.Obligatorio, $"{plantilla.Meta.Id} no la exige.");
            Assert.True(campo.Multiple, $"{plantilla.Meta.Id} no admite varias.");
            Assert.Equal(SinAcreditar, campo.OpcionExcluyente);
        }
    }
}
