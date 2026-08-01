using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>Los ocho patrones P1-P8 de docs/REGLAS-NEGOCIO.md, sobre la plantilla real.</summary>
public class PatronesTests
{
    // ---- P2 · aviso de fecha ----------------------------------------------

    [Fact]
    public void P2_SinFecha_AvisaDeQueFalta()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.True(motor.EsVerdadera("R-06-01"));
    }

    [Fact]
    public void P2_ConFecha_NoAvisa()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-06-01"));
    }

    // ---- P3 · faltan datos ------------------------------------------------

    [Fact]
    public void P3_MarcadoSinFecha_FaltanDatos()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.True(motor.EsVerdadera("R-06-02"));
    }

    [Fact]
    public void P3_ApartadoMarcadoComoNA_NuncaFaltanDatos()
    {
        var datos = Contexto.ProyectoVacio();
        datos.EstablecerNa("6/na", true);   // N/A del bloque, sin rellenar la fecha

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-06-01"));   // la fecha sigue faltando
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-06-02"));  // pero el apartado no reclama
    }

    // ---- P4 · al menos una ------------------------------------------------

    [Fact]
    public void P4_SinNingunaOpcionMarcada_NoSeCumple()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.False(motor.EsVerdadera("R-07.12-04"));
        Assert.True(motor.EsVerdadera("R-07.12-05"));   // ⇒ faltan datos de portalámparas
    }

    [Fact]
    public void P4_ConUnaOpcionMarcada_SeCumple()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar("7.12.portalamparas", "portalamparas", "e40");

        var motor = Contexto.Motor(datos);
        Assert.True(motor.EsVerdadera("R-07.12-04"));
        Assert.False(motor.EsVerdadera("R-07.12-05"));
    }

    // ---- P5 · exactamente una ---------------------------------------------

    [Fact]
    public void P5_DosOrigenesDeMuestraMarcados_NoSeCumple()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar("15.2.1", "origenMuestra", "corteEbp");
        datos.Marcar("15.2.1", "origenMuestra", "productoAcabado");

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-15.2-01"));
    }

    [Fact]
    public void P5_UnSoloOrigenDeMuestraMarcado_SeCumple()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar("15.2.1", "origenMuestra", "corteEbp");

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-15.2-01"));
    }

    // ---- P6 · recuento de datos -------------------------------------------

    [Fact]
    public void P6_SinDatosDeTornillos_FaltanDatos()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.True(motor.EsVerdadera("R-07.12-02"));
    }

    [Fact]
    public void P6_ConUnTornilloAnotado_YaNoFaltanDatos()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("7.12.1", "tornillos[0].diametro", 4.0, muestra: 1);

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-07.12-01"));
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-07.12-02"));
    }

    [Fact]
    public void P6_LuminariaSinTornillos_NoReclamaDatos()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar("7.12.1", "sinTornillos", "sinTornillos");

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-07.12-02"));
    }

    [Fact]
    public void P6_UmbralPorMuestra_EscalaConElNumeroDeMuestras()
    {
        // Tres datos de tamaño por muestra; con dos muestras hacen falta seis.
        var datos = Contexto.ProyectoVacio(muestras: 2);
        datos.IpSegundaCifra.Add("IPX3");   // así el tamaño es obligatorio

        foreach (var m in new[] { 1, 2 })
        {
            datos.Establecer("generales", "tamano[0].alto", 30.0, m);
            datos.Establecer("generales", "tamano[0].ancho", 20.0, m);
        }
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-GEN-02"));   // 4 de 6

        foreach (var m in new[] { 1, 2 })
            datos.Establecer("generales", "tamano[0].profundo", 10.0, m);

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-GEN-02"));    // 6 de 6
    }

    // ---- P7 · duración mínima ---------------------------------------------

    [Fact]
    public void P7_CuarentaYSieteHoras_NoLlegaALas48()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("11.3", "inicio", new DateTime(2026, 7, 20, 8, 0, 0), 1);
        datos.Establecer("11.3", "fin", new DateTime(2026, 7, 22, 7, 0, 0), 1);

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-11-22"));
    }

    [Fact]
    public void P7_CuarentaYOchoHorasExactas_SeAceptan()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("11.3", "inicio", new DateTime(2026, 7, 20, 8, 0, 0), 1);
        datos.Establecer("11.3", "fin", new DateTime(2026, 7, 22, 8, 0, 0), 1);

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-11-22"));
    }

    [Fact]
    public void P7_SinFechas_NoSeDaPorVerificado()
    {
        Assert.False(Contexto.Motor(Contexto.ProyectoVacio()).EsVerdadera("R-11-22"));
    }

    [Theory]
    [InlineData("48h", 48)]
    [InlineData("24h", 24)]
    [InlineData("180min", 3)]
    public void P7_FormatosDeDuracionDelContrato(string texto, double horas)
        => Assert.Equal(TimeSpan.FromHours(horas), MotorDeReglas.LeerDuracion(texto));

    // ---- P8 · peso de avance ----------------------------------------------

    [Fact]
    public void P8_ApartadoSinTerminar_AportaPesoAlTotalPeroNoAlEjecutado()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        var aportacion = (Aportacion)motor.Evaluar("R-06-P8")!;

        Assert.True(aportacion.Aplica);
        Assert.False(aportacion.Terminado);
        Assert.Equal(3, aportacion.PesoEnProyecto);
        Assert.Equal(0, aportacion.PesoFinalizado);
    }

    [Fact]
    public void P8_ApartadoNA_NoAportaNiAlTotal()
    {
        var datos = Contexto.ProyectoVacio();
        datos.EstablecerNa("6/na", true);

        var aportacion = (Aportacion)Contexto.Motor(datos).Evaluar("R-06-P8")!;

        Assert.False(aportacion.Aplica);
        Assert.Equal(0, aportacion.PesoEnProyecto);
    }

    [Fact]
    public void P8_ApartadoTerminado_AportaAlEjecutado()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));

        var aportacion = (Aportacion)Contexto.Motor(datos).Evaluar("R-06-P8")!;

        Assert.True(aportacion.Terminado);
        Assert.Equal(3, aportacion.PesoFinalizado);
    }
}
