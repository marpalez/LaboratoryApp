using LumNotas.Core.Datos;

namespace LumNotas.Core.Tests;

/// <summary>
/// Los tres puntos en que la aplicación se aparta del Excel a propósito.
/// Si alguno de estos tests falla, hemos vuelto a replicar el defecto original.
/// </summary>
public class DefectosCorregidosTests
{
    // ---- D-21 · agregación de 7.12 ----------------------------------------

    [Fact]
    public void D21_FaltaSoloUnSubapartado_ElApartadoAvisaIgualmente()
    {
        var datos = ProyectoCon712CasiCompleto();

        // Todo resuelto menos los prensaestopas.
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-07.12-02"));    // tornillos OK
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-07.12-03b"));   // uniones OK
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-07.12-05"));    // portalámparas OK
        Assert.True(Contexto.Motor(datos).EsVerdadera("R-07.12-09"));     // prensaestopas NO

        // El Excel devolvía "no faltan datos" aquí, porque solo avisaba
        // cuando fallaban los cuatro subapartados a la vez.
        Assert.True(Contexto.Motor(datos).EsVerdadera("R-07.12-10"));
        Assert.True(Contexto.Motor(datos).EsVerdadera("R-07.12-11"));
    }

    [Fact]
    public void D21_ConLosCuatroSubapartadosResueltos_NoAvisa()
    {
        var datos = ProyectoCon712CasiCompleto();
        datos.Marcar("7.12.5", "sinPrensaestopas", "sinPrensaestopas");
        datos.Establecer("7.12", "ambiente.fecha", new DateTime(2026, 7, 20));

        var motor = Contexto.Motor(datos);
        Assert.False(motor.EsVerdadera("R-07.12-10"));
        Assert.False(motor.EsVerdadera("R-07.12-11"));
    }

    [Fact]
    public void D21_ConLosCuatroSubapartadosVacios_TambienAvisa()
    {
        // Único caso en que el Excel acertaba; debe seguir avisando.
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.True(motor.EsVerdadera("R-07.12-10"));
    }

    private static DatosProyecto ProyectoCon712CasiCompleto()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("7.12.1", "tornillos[0].diametro", 4.0, 1);
        datos.Establecer("7.12.4", "uniones[0].m", 6.0, 1);
        datos.Marcar("7.12.portalamparas", "portalamparas", "e40");
        return datos;
    }

    // ---- D-05 · instantes completos en el ensayo de bola -------------------

    [Fact]
    public void D05_EnsayoQueCruzaLaMedianoche_SeValidaCorrectamente()
    {
        var datos = Contexto.ProyectoVacio();
        // 60 minutos exactos, de 23:40 a 00:40 del día siguiente.
        datos.Establecer("15.2.1", "ensayoInicio", new DateTime(2026, 7, 20, 23, 40, 0), 1);
        datos.Establecer("15.2.1", "ensayoFin", new DateTime(2026, 7, 21, 0, 40, 0), 1);

        // Con la fórmula del Excel (solo hh:mm) la resta salía negativa y esto fallaba.
        Assert.True(Contexto.Motor(datos).EsVerdadera("R-15.2-08"));
    }

    [Fact]
    public void D05_AcondicionamientoQueCruzaLaMedianoche_SeValidaCorrectamente()
    {
        var datos = Contexto.ProyectoVacio();
        // 4 horas, de 22:00 a 02:00.
        datos.Establecer("15.2.1", "hornoAcondInicio", new DateTime(2026, 7, 20, 22, 0, 0), 1);
        datos.Establecer("15.2.1", "hornoAcondFin", new DateTime(2026, 7, 21, 2, 0, 0), 1);

        Assert.True(Contexto.Motor(datos).EsVerdadera("R-15.2-07"));
    }

    [Fact]
    public void D05_AcondicionamientoDemasiadoCorto_SigueRechazandose()
    {
        var datos = Contexto.ProyectoVacio();
        // 2 horas: por debajo de los 180 minutos exigidos.
        datos.Establecer("15.2.1", "hornoAcondInicio", new DateTime(2026, 7, 20, 22, 0, 0), 1);
        datos.Establecer("15.2.1", "hornoAcondFin", new DateTime(2026, 7, 21, 0, 0, 0), 1);

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-15.2-07"));
    }

    [Fact]
    public void D05_EnsayoDeCincuentaMinutos_QuedaFueraDelRango()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("15.2.1", "ensayoInicio", new DateTime(2026, 7, 20, 10, 0, 0), 1);
        datos.Establecer("15.2.1", "ensayoFin", new DateTime(2026, 7, 20, 10, 50, 0), 1);

        Assert.False(Contexto.Motor(datos).EsVerdadera("R-15.2-08"));
    }

    // ---- D-03 · peso de la fila IPX_ --------------------------------------

    [Fact]
    public void D03_ElPesoDeAguaUsaElNaDeAguaYNoElDePolvo()
    {
        var datos = Contexto.ProyectoVacio();
        datos.IpSegundaCifra.Add("IPX5");
        // N/A del apartado de polvo (1ª cifra), no del de agua.
        datos.EstablecerNa("11.2.2-11.2.3/na", true);

        var aportacion = (LumNotas.Core.Motor.Aportacion)Contexto.Motor(datos).Evaluar("R-11-P8a")!;

        // El Excel marcaba esta fila como "no aplica" por leer el N/A equivocado.
        Assert.True(aportacion.Aplica);
        Assert.Equal(5, aportacion.PesoEnProyecto);
    }
}
