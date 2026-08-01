using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>
/// Indicador de avance: pesos del Excel + contador de apartados (DD-11 / D-18).
/// Apartados con peso volcados hasta ahora: Partes activas (5), Marcado (3), 7.6 (3),
/// 7.12 (3), 7.13 (3), 7.14.1 (3), 7.24.2 (5), IP 2ª cifra (5) y Bola (5) = 35 puntos.
/// </summary>
public class AvanceTests
{
    private static IndicadorDeAvance.Resultado Calcular(LumNotas.Core.Datos.DatosProyecto datos)
        => new IndicadorDeAvance(Contexto.Motor(datos)).Calcular();

    /// <summary>Los totales se leen de la plantilla: crecen en cada tramo que se vuelca.</summary>
    private static (int Peso, int Apartados) Referencia()
    {
        var r = Calcular(Contexto.ProyectoVacio());
        return (r.PesoTotal, r.ApartadosAplicables);
    }

    [Fact]
    public void ProyectoVacio_TodoAplicaYNadaEstaTerminado()
    {
        var r = Calcular(Contexto.ProyectoVacio());

        Assert.True(r.PesoTotal > 0, "La plantilla debe declarar pesos de avance.");
        Assert.Equal(0, r.PesoEjecutado);
        Assert.Equal(r.PesoTotal, r.PesoPendiente);
        Assert.Equal(0, r.ApartadosCompletados);
        Assert.Equal($"0/{r.ApartadosAplicables}", r.Contador);
        Assert.Equal(0, r.PorcentajePonderado);
    }

    [Fact]
    public void ApartadoTerminado_SubeElPorcentajeYElContador()
    {
        var (peso, apartados) = Referencia();

        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));
        var r = Calcular(datos);

        Assert.Equal(3, r.PesoEjecutado);                  // Marcado pesa 3
        Assert.Equal($"1/{apartados}", r.Contador);
        Assert.Equal(3d / peso * 100, r.PorcentajePonderado, 2);
    }

    [Fact]
    public void ApartadoNA_SaleDelDenominador()
    {
        var (peso, apartados) = Referencia();

        var datos = Contexto.ProyectoVacio();
        datos.EstablecerNa("6/na", true);
        var r = Calcular(datos);

        Assert.Equal(peso - 3, r.PesoTotal);
        Assert.Equal(apartados - 1, r.ApartadosAplicables);
        Assert.Equal($"0/{apartados - 1}", r.Contador);
    }

    [Fact]
    public void LosDosIndicadoresPuedenDiscrepar()
    {
        // Terminar solo la bola aporta 5 puntos de peso pero 1 apartado de 9:
        // es justo la discrepancia que D-18 pedía hacer visible.
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("15.2.1", "ambiente.fecha", new DateTime(2026, 7, 20));
        datos.Marcar("15.2.1", "origenMuestra", "corteEbp");
        datos.Marcar("15.2.1", "acondicionamiento", "verificado24h");
        foreach (var opcion in new[] { "introduccion30s", "recuperacion5min", "inmersionAgua", "inmersion4a8min" })
            datos.Marcar("15.2.1", "procedimiento", opcion);
        datos.Establecer("15.2.1", "espesor", 3.0, 1);
        datos.Establecer("15.2.1", "largo", 15.0, 1);
        datos.Establecer("15.2.1", "ancho", 12.0, 1);
        datos.Establecer("15.2.1", "tempTermopar", 850.0, 1);
        datos.Establecer("15.2.1", "tempAgua", 20.0, 1);
        datos.Establecer("15.2.1", "tiempoAgua", 6.0, 1);
        datos.Establecer("15.2.1", "acondInicio", new DateTime(2026, 7, 18, 9, 0, 0), 1);
        datos.Establecer("15.2.1", "acondFin", new DateTime(2026, 7, 19, 9, 0, 0), 1);
        datos.Establecer("15.2.1", "hornoAcondInicio", new DateTime(2026, 7, 20, 8, 0, 0), 1);
        datos.Establecer("15.2.1", "hornoAcondFin", new DateTime(2026, 7, 20, 11, 30, 0), 1);
        datos.Establecer("15.2.1", "ensayoInicio", new DateTime(2026, 7, 20, 12, 0, 0), 1);
        datos.Establecer("15.2.1", "ensayoFin", new DateTime(2026, 7, 20, 13, 0, 0), 1);

        var (peso, apartados) = Referencia();
        var r = Calcular(datos);

        Assert.Equal(5, r.PesoEjecutado);                  // la bola pesa 5
        Assert.Equal($"1/{apartados}", r.Contador);
        Assert.Equal(5d / peso * 100, r.PorcentajePonderado, 2);
    }
}
