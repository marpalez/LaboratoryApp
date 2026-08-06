using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Lo mínimo para que un <c>.lmnlab</c> llegue al disco: código de la toma de notas y
/// técnico 1. Sin ellos el fichero no se puede ni nombrar ni atribuir.
/// </summary>
public class RequisitosParaGuardarTests
{
    private static DatosProyecto Con(string codigo, string? tecnico)
    {
        var datos = new DatosProyecto { CodigoTomaDeNotas = codigo, NumeroMuestras = 1 };
        if (tecnico is not null) datos.Tecnico1 = tecnico;
        return datos;
    }

    [Fact]
    public void ConLosDosSePuedeGuardar()
        => Assert.True(RequisitosParaGuardar.SePuede(Con("TECNO260201-00", "Javier Ibor")));

    /// <summary>
    /// Sin código no hay con qué nombrar el fichero, y todas las familias de un trabajo
    /// acabarían peleándose por el mismo nombre.
    /// </summary>
    [Fact]
    public void SinCodigoNoSeGuarda()
    {
        Assert.False(RequisitosParaGuardar.SePuede(Con("", "Javier Ibor")));
        Assert.False(RequisitosParaGuardar.SePuede(Con("   ", "Javier Ibor")));
    }

    /// <summary>Sin técnico 1 no se sabe de quién es el ensayo.</summary>
    [Fact]
    public void SinTecnicoNoSeGuarda()
    {
        Assert.False(RequisitosParaGuardar.SePuede(Con("TECNO260201-00", null)));
        Assert.False(RequisitosParaGuardar.SePuede(Con("TECNO260201-00", "  ")));
    }

    [Fact]
    public void SeDiceQueFaltaYNoSeAdivina()
    {
        Assert.Equal([AltaDeProyecto.CampoNombre, AltaDeProyecto.CampoTecnico],
                     RequisitosParaGuardar.Faltan(Con("", null)));

        Assert.Empty(RequisitosParaGuardar.Faltan(Con("TECNO260201-00", "Javier Ibor")));
    }

    /// <summary>El aviso nombra lo que falta y dice dónde está, no solo que falta.</summary>
    [Fact]
    public void ElAvisoDiceQueFaltaYDonde()
    {
        var aviso = RequisitosParaGuardar.Aviso(Con("", null));

        Assert.Contains("código de la toma de notas", aviso);
        Assert.Contains("técnico 1", aviso);
        Assert.Contains("Datos del proyecto", aviso);

        Assert.Equal("", RequisitosParaGuardar.Aviso(Con("TECNO260201-00", "Javier Ibor")));
    }

    /// <summary>
    /// <b>Guardar exige menos que ensayar, y es a propósito.</b> Un servicio a medias —sin
    /// clase, sin Ta, sin acreditación— es el estado normal durante semanas y tiene que
    /// poder guardarse; lo que no puede es existir un fichero sin nombre ni dueño.
    /// </summary>
    [Fact]
    public void GuardarExigeMenosQueEmpezarAEnsayar()
    {
        var aMedias = Con("TECNO260201-00", "Javier Ibor");

        Assert.True(RequisitosParaGuardar.SePuede(aMedias));
        Assert.NotEmpty(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, aMedias));
    }

    /// <summary>
    /// <b>Con el código a medias no se guarda</b> (2026‑08‑06, decisión del laboratorio).
    /// <para>
    /// Se sopesó dejarlo pasar, porque los proyectos anteriores a la regla tienen códigos
    /// de nueve y bloquearlos obliga a arreglarlos antes de poder escribir. Se decidió que
    /// <b>eso es justamente lo que se quiere</b>: con la excepción puesta, esos proyectos se
    /// quedaban a medias para siempre —nada obligaba nunca a completarlos— y de ese código
    /// salen el de servicio, el de familia, el identificador de las muestras y el nombre del
    /// fichero.
    /// </para>
    /// <para>
    /// Nada se pierde por el camino: si al cerrar se elige «Guardar» y el guardado se
    /// rechaza, <c>ConfirmarSiHayCambios</c> ve que los cambios siguen ahí y <b>cancela el
    /// cierre</b> en vez de tirarlos.
    /// </para>
    /// </summary>
    [Fact]
    public void ConElCodigoAMediasNoSeGuarda()
    {
        var corto = Con("ANTAR2504", "Javier Ibor");

        Assert.False(RequisitosParaGuardar.SePuede(corto));
        Assert.Equal([AltaDeProyecto.CampoNombre], RequisitosParaGuardar.Faltan(corto));

        // Y completándolo se guarda, sin tocar nada más.
        var entero = Con("ANTAR250401-00", "Javier Ibor");

        Assert.True(RequisitosParaGuardar.SePuede(entero));
        Assert.DoesNotContain(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, entero),
            f => f.Contains("toma de notas"));
    }

    /// <summary>
    /// Guardar y ensayar exigen <b>lo mismo</b> sobre el código: entero. Lo que las separa
    /// son los demás datos —clase, Ta, acreditación—, que solo hacen falta para ensayar.
    /// </summary>
    [Fact]
    public void ElCodigoSeExigeIgualParaGuardarQueParaEnsayar()
    {
        var corto = Con("ANTAR2504", "Javier Ibor");

        Assert.False(RequisitosParaGuardar.SePuede(corto));
        Assert.Contains(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, corto),
            f => f.Contains("toma de notas"));
    }

    /// <summary>
    /// Los mismos dos que pide el alta rápida: lo que nace por un camino y lo que nace por
    /// el otro tiene que ser igual de identificable.
    /// </summary>
    [Fact]
    public void SonLosMismosDosQuePideElAlta()
    {
        var delAlta = AltaDeProyecto.Faltan(nombre: "", tecnico1: "", norma: "60598");

        Assert.Equal([AltaDeProyecto.CampoNombre, AltaDeProyecto.CampoTecnico], delAlta);
        Assert.Equal(delAlta, RequisitosParaGuardar.Faltan(Con("", null)));
    }
}
