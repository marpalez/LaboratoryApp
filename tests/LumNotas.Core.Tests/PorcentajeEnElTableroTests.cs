using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>
/// El porcentaje que enseñan el tablero y el calendario.
/// <para>
/// Lo que se vigila aquí es <b>que sea el mismo número que estampa el informe</b>. El
/// programa sabe contar el avance de tres maneras —secciones, apartados y peso— y con
/// tres sitios donde enseñarlo es fácil acabar con un tablero que dice 6 % y un PDF
/// firmado que dice 45 %. Esa es la regresión que estos tests tienen que cazar.
/// </para>
/// </summary>
public class PorcentajeEnElTableroTests
{
    private static ResumenDeProyecto Resumir(LumNotas.Core.Datos.DatosProyecto datos)
        => AnalizadorDeProyectos.Analizar(
            Contexto.Plantilla, datos, "C:/x/servicio.lmnlab", new DateTime(2026, 8, 6));

    private static IndicadorDeAvance.Resultado ElDelInforme(LumNotas.Core.Datos.DatosProyecto datos)
        => new IndicadorDeAvance(Contexto.Motor(datos)).Calcular();

    /// <summary>El motivo por el que se eligió el ponderado y no otro: un solo número.</summary>
    [Fact]
    public void ElTableroDiceElMismoPorcentajeQueElInforme()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));   // Marcado, pesa 3

        var informe = ElDelInforme(datos);
        Assert.True(informe.PesoEjecutado > 0, "El apartado tenía que contar para el peso.");

        Assert.Equal((int)Math.Floor(informe.PorcentajePonderado), Resumir(datos).PorcentajePonderado);
    }

    [Fact]
    public void UnProyectoVacioEstaAlCeroPorCiento()
    {
        Assert.Equal(0, Resumir(Contexto.ProyectoVacio()).PorcentajePonderado);
    }

    /// <summary>
    /// Nunca 100 % por redondeo. Un 99,6 % redondeado al alza pondría el cartel de acabado
    /// en un servicio al que le falta un ensayo, y eso se firma.
    /// </summary>
    [Fact]
    public void SoloDiceCienCuandoNoQuedaPeso()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));

        var resumen = Resumir(datos);
        var informe = ElDelInforme(datos);

        Assert.True(informe.PesoPendiente > 0, "Quedaba peso por hacer.");
        Assert.NotEqual(100, resumen.PorcentajePonderado);

        // Y por debajo nunca redondea hacia arriba: se trunca.
        Assert.True(resumen.PorcentajePonderado <= informe.PorcentajePonderado);
    }

    /// <summary>
    /// Las cinco normas instaladas declaran pesos, así que el número existe en todas. Si
    /// alguien publica una sin ellos, el resumen tiene que decir «no sé» —nulo— y no «0 %»,
    /// que sería mentira fija hasta en un servicio terminado.
    /// </summary>
    [Fact]
    public void TodasLasNormasInstaladasDeclaranPesos()
    {
        foreach (var plantilla in Contexto.TodasLasPlantillas())
        {
            var motor = new MotorDeReglas(plantilla, Contexto.ProyectoVacio());
            Assert.True(new IndicadorDeAvance(motor).Calcular().PesoTotal > 0,
                        $"«{plantilla.Meta.Id}» no declara ningún peso de avance: el tablero se quedaría sin %.");
        }
    }

    // ---- el renglón --------------------------------------------------------

    /// <summary>Lo que no hay se cae, en vez de dejar un hueco entre barras.</summary>
    [Fact]
    public void SinFechasElRenglonNoLlevaSemanas()
    {
        var linea = Resumir(Contexto.ProyectoVacio()).LineaDeAvance;

        Assert.DoesNotContain("W", linea);
        Assert.Contains("0 %", linea);
        Assert.Contains("secciones", linea);
        Assert.DoesNotContain("|  |", linea);
    }

    [Fact]
    public void ConFechasElRenglonEmpiezaPorLasSemanas()
    {
        var resumen = Resumir(Contexto.ProyectoVacio()) with
        {
            Planificacion = new Planificacion
            {
                Inicio = new DateTime(2026, 9, 1),
                Fin = new DateTime(2026, 9, 21)
            }
        };

        Assert.StartsWith("3W  |  ", resumen.LineaDeAvance);
    }

    /// <summary>
    /// Un proyecto que no se pudo leer no inventa un cero: no hay número, y el renglón dice
    /// lo único que se sabe de él.
    /// </summary>
    [Fact]
    public void UnProyectoIlegibleNoTienePorcentaje()
    {
        var roto = AnalizadorDeProyectos.NoLegible("C:/x/roto.lmnlab", new DateTime(2026, 8, 6), "json inválido");

        Assert.Null(roto.PorcentajePonderado);
        Assert.Equal("no se pudo leer", roto.LineaDeAvance);
    }
}
