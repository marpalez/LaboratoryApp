using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Qué se escribe al soltar la barra de un trabajo con varias familias. La barra abarca de
/// la primera a la última, así que ya no basta con guardar las fechas de una.
/// </summary>
public class RepartoDelArrastreTests
{
    private static ResumenDeProyecto Familia(string codigo, DateTime? inicio = null, DateTime? fin = null)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            Planificacion = new Planificacion { Inicio = inicio, Fin = fin, Grupo = "ANTAR2504" }
        };

    private static EntradaDeCalendario Grupo(params ResumenDeProyecto[] familias)
        => Assert.Single(EnlaceDeTomasDeNotas.Agrupar(familias));

    private static readonly DateTime Sep1 = new(2026, 9, 1);
    private static readonly DateTime Sep15 = new(2026, 9, 15);
    private static readonly DateTime Sep30 = new(2026, 9, 30);

    /// <summary>
    /// <b>Mover el trabajo mueve todas las familias.</b> Sin esto, la barra volvería a su
    /// sitio al soltarla —se dibuja de las fechas de todas y solo se guardaban las de
    /// una— y parecería que el arrastre no funciona.
    /// </summary>
    [Fact]
    public void MoverElTrabajoDesplazaTodasLasFamilias()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30));

        // Una semana más tarde: los dos bordes se mueven lo mismo.
        var cambios = RepartoDelArrastre.Aplicar(grupo, Sep1.AddDays(7), Sep30.AddDays(7));

        Assert.Equal(2, cambios.Count);
        Assert.Equal(Sep1.AddDays(7), cambios[0].Plan.Inicio);
        Assert.Equal(Sep15.AddDays(7), cambios[0].Plan.Fin);
        Assert.Equal(Sep30.AddDays(7), cambios[1].Plan.Fin);
    }

    /// <summary>Las distancias entre familias se mantienen: se mueve el trabajo, no se deforma.</summary>
    [Fact]
    public void AlMoverSeMantienenLasDistancias()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30));

        var cambios = RepartoDelArrastre.Aplicar(grupo, Sep1.AddDays(7), Sep30.AddDays(7));

        Assert.Equal(Sep30 - Sep15, cambios[1].Plan.Fin!.Value - cambios[0].Plan.Fin!.Value);
    }

    /// <summary>Estirar por la derecha alarga solo la última: las demás no se enteran.</summary>
    [Fact]
    public void EstirarPorLaDerechaSoloTocaLaUltima()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30));

        var cambio = Assert.Single(RepartoDelArrastre.Aplicar(grupo, Sep1, Sep30.AddDays(7)));

        Assert.Equal("ANTAR250402", cambio.Proyecto.Rotulo);
        Assert.Equal(Sep30.AddDays(7), cambio.Plan.Fin);
    }

    /// <summary>Y por la izquierda, solo la primera.</summary>
    [Fact]
    public void EstirarPorLaIzquierdaSoloTocaLaPrimera()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30));

        var cambio = Assert.Single(RepartoDelArrastre.Aplicar(grupo, Sep1.AddDays(-7), Sep30));

        Assert.Equal("ANTAR250401", cambio.Proyecto.Rotulo);
        Assert.Equal(Sep1.AddDays(-7), cambio.Plan.Inicio);
    }

    /// <summary>Arrastrar y volver al sitio no escribe nada: no se toca el fichero por nada.</summary>
    [Fact]
    public void SinCambioNoSeEscribeNada()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30));

        Assert.Empty(RepartoDelArrastre.Aplicar(grupo, Sep1, Sep30));
    }

    /// <summary>Las familias sin fechas no se inventan ninguna al mover el trabajo.</summary>
    [Fact]
    public void LasFamiliasSinFechasSeQuedanSinFechas()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00"));

        var cambio = Assert.Single(RepartoDelArrastre.Aplicar(grupo, Sep1.AddDays(7), Sep15.AddDays(7)));

        Assert.Equal("ANTAR250401", cambio.Proyecto.Rotulo);
    }

    /// <summary>
    /// Una toma de notas suelta se comporta como siempre: un solo cambio, el suyo. Es el
    /// caso más frecuente con diferencia y no puede haberse alterado.
    /// </summary>
    [Fact]
    public void UnaSueltaSeComportaComoSiempre()
    {
        var suelta = new ResumenDeProyecto
        {
            Ruta = @"C:\x.lmnlab",
            Nombre = "MOONO230401-00",
            CodigoTomaDeNotas = "MOONO230401-00",
            Planificacion = new Planificacion { Inicio = Sep1, Fin = Sep15 }
        };

        var entrada = Assert.Single(EnlaceDeTomasDeNotas.Agrupar([suelta]));
        var cambio = Assert.Single(RepartoDelArrastre.Aplicar(entrada, Sep1.AddDays(3), Sep15.AddDays(3)));

        Assert.Equal(Sep1.AddDays(3), cambio.Plan.Inicio);
        Assert.Equal(Sep15.AddDays(3), cambio.Plan.Fin);
    }

    /// <summary>Lo que no son fechas —importe, grupo, estado— viaja intacto.</summary>
    [Fact]
    public void ElRestoDeLaPlanificacionNoSeToca()
    {
        var familia = Familia("ANTAR250401-00", Sep1, Sep15);
        familia.Planificacion.Importe = 2400;
        familia.Planificacion.Estado = EstadoDeProyecto.EnCurso;

        var cambio = Assert.Single(
            RepartoDelArrastre.Aplicar(Grupo(familia), Sep1.AddDays(7), Sep15.AddDays(7)));

        Assert.Equal(2400, cambio.Plan.Importe);
        Assert.Equal(EstadoDeProyecto.EnCurso, cambio.Plan.Estado);
        Assert.Equal("ANTAR2504", cambio.Plan.Grupo);
    }
}
