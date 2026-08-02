using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Carga de trabajo por técnico y mes.
/// <para>
/// El laboratorio mide el trabajo de un servicio dividiendo el importe de su oferta
/// entre 80 €: 2 000 € son 25 días. Esos días se reparten entre los meses que abarca el
/// servicio, en proporción a sus días entre semana, y se comparan con lo que cabe en
/// cada mes —22 días, salvo agosto (10) y diciembre (15)—.
/// </para>
/// </summary>
public class CargaTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-carga-" + Guid.NewGuid().ToString("N"));

    public CargaTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    // ---- la tarifa y el calendario laboral ---------------------------------

    [Fact]
    public void DosMilEurosSonVeinticincoDiasDeTrabajo()
        => Assert.Equal(25, new CapacidadMensual().DiasDeTrabajo(2000));

    [Fact]
    public void AgostoYDiciembreSonMesesCortos()
    {
        var capacidad = new CapacidadMensual();

        Assert.Equal(22, capacidad.Dias(3));
        Assert.Equal(10, capacidad.Dias(8));
        Assert.Equal(15, capacidad.Dias(12));
    }

    [Fact]
    public void LaTarifaYLaCapacidadSobrevivenAlGuardado()
    {
        var capacidad = new CapacidadMensual { EurosPorDia = 95 };
        capacidad.DiasPorMes[7] = 8;
        capacidad.Guardar(_carpeta);

        var leida = CapacidadMensual.Cargar(_carpeta);

        Assert.Equal(95, leida.EurosPorDia);
        Assert.Equal(8, leida.Dias(8));
    }

    /// <summary>Un fichero a medias no puede dejar el cálculo sin base.</summary>
    [Fact]
    public void UnFicheroCorruptoDevuelveLosValoresDePartida()
    {
        File.WriteAllText(Path.Combine(_carpeta, CapacidadMensual.NombreDeFichero), "{ roto");

        var leida = CapacidadMensual.Cargar(_carpeta);

        Assert.Equal(CapacidadMensual.EurosPorDiaPorDefecto, leida.EurosPorDia);
        Assert.Equal(12, leida.DiasPorMes.Count);
    }

    // ---- días entre semana -------------------------------------------------

    [Fact]
    public void LosFinesDeSemanaNoCuentan()
    {
        // Del lunes 3 al domingo 9 de agosto de 2026: cinco laborables.
        Assert.Equal(5, RepartoDeTrabajo.DiasEntreSemana(new DateTime(2026, 8, 3), new DateTime(2026, 8, 9)));

        // Sábado y domingo solos: ninguno.
        Assert.Equal(0, RepartoDeTrabajo.DiasEntreSemana(new DateTime(2026, 8, 8), new DateTime(2026, 8, 9)));

        // Un solo lunes: uno.
        Assert.Equal(1, RepartoDeTrabajo.DiasEntreSemana(new DateTime(2026, 8, 3), new DateTime(2026, 8, 3)));
    }

    // ---- el reparto entre meses --------------------------------------------

    [Fact]
    public void UnServicioDentroDeUnMesNoSeReparte()
    {
        var reparto = RepartoDeTrabajo.Repartir(new DateTime(2026, 9, 7), new DateTime(2026, 9, 25), 12);

        var unico = Assert.Single(reparto);
        Assert.Equal((2026, 9), (unico.Ano, unico.Mes));
        Assert.Equal(12, unico.Dias, 3);
    }

    /// <summary>
    /// El caso que dio pie a todo esto: un servicio a caballo de dos meses reparte su
    /// trabajo en proporción a los días entre semana que tiene en cada uno.
    /// </summary>
    [Fact]
    public void UnServicioACaballoDeDosMesesRepartePorDiasEntreSemana()
    {
        // Del 10 de agosto al 4 de septiembre de 2026: 16 laborables en agosto, 4 en
        // septiembre. Un servicio de 2 000 € son 25 días de trabajo.
        var reparto = RepartoDeTrabajo.Repartir(new DateTime(2026, 8, 10), new DateTime(2026, 9, 4), 25);

        Assert.Equal(2, reparto.Count);
        Assert.Equal(20, reparto.Single(t => t.Mes == 8).Dias, 3);
        Assert.Equal(5, reparto.Single(t => t.Mes == 9).Dias, 3);

        // Y no se pierde ni se inventa trabajo por el camino.
        Assert.Equal(25, reparto.Sum(t => t.Dias), 6);
    }

    [Fact]
    public void ElRepartoNuncaPierdeTrabajo()
    {
        var reparto = RepartoDeTrabajo.Repartir(new DateTime(2026, 1, 5), new DateTime(2026, 12, 18), 100);

        Assert.Equal(100, reparto.Sum(t => t.Dias), 6);
        Assert.Equal(12, reparto.Count);
    }

    /// <summary>Un servicio que cae entero en fin de semana no puede desaparecer del recuento.</summary>
    [Fact]
    public void UnServicioSoloDeFinDeSemanaSeImputaASuMes()
    {
        var reparto = RepartoDeTrabajo.Repartir(new DateTime(2026, 8, 8), new DateTime(2026, 8, 9), 3);

        var unico = Assert.Single(reparto);
        Assert.Equal(8, unico.Mes);
        Assert.Equal(3, unico.Dias, 3);
    }

    // ---- la tabla ----------------------------------------------------------

    private static ServicioPlanificado Servicio(string tecnico, string inicio, string fin, double? importe)
        => new(tecnico, DateTime.Parse(inicio), DateTime.Parse(fin), importe);

    [Fact]
    public void CadaTecnicoTieneSuFilaYLosMesesSonLosQueSeAbarcan()
    {
        var (meses, filas) = CargaPorTecnico.Calcular([
            Servicio("Daniel Martínez", "2026-08-03", "2026-08-28", 2000),
            Servicio("Javier Ibor", "2026-09-07", "2026-09-25", 1600)
        ], new CapacidadMensual());

        Assert.Equal(2, meses.Count);
        Assert.Equal((2026, 8), meses[0]);
        Assert.Equal((2026, 9), meses[1]);

        Assert.Equal(2, filas.Count);
        Assert.Equal("Daniel Martínez", filas[0].Tecnico);
        Assert.Equal("Javier Ibor", filas[1].Tecnico);
    }

    /// <summary>
    /// <b>El aviso que justifica la vista.</b> 25 días de trabajo en un agosto donde solo
    /// caben 10 es un 250 %: el mes no da de sí y hay que verlo antes, no en septiembre.
    /// </summary>
    [Fact]
    public void UnAgostoSobrevendidoPasaDelCienPorCien()
    {
        var (_, filas) = CargaPorTecnico.Calcular(
            [Servicio("Daniel Martínez", "2026-08-03", "2026-08-28", 2000)], new CapacidadMensual());

        var agosto = filas[0].Meses.Single(c => c.Mes == 8);

        Assert.Equal(25, agosto.Dias, 3);
        Assert.Equal(10, agosto.Capacidad);
        Assert.Equal(250, agosto.Porcentaje, 1);
    }

    [Fact]
    public void UnMesNormalConUnServicioModestoVaHolgado()
    {
        var (_, filas) = CargaPorTecnico.Calcular(
            [Servicio("Javier Ibor", "2026-09-07", "2026-09-25", 800)], new CapacidadMensual());

        var septiembre = filas[0].Meses.Single(c => c.Mes == 9);

        Assert.Equal(10, septiembre.Dias, 3);       // 800 / 80
        Assert.Equal(22, septiembre.Capacidad);
        Assert.InRange(septiembre.Porcentaje, 45, 46);
    }

    [Fact]
    public void LosServiciosDelMismoTecnicoSeSuman()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("Daniel Martínez", "2026-09-07", "2026-09-11", 800),
            Servicio("Daniel Martínez", "2026-09-14", "2026-09-18", 800)
        ], new CapacidadMensual());

        Assert.Single(filas);
        Assert.Equal(20, filas[0].Meses.Single(c => c.Mes == 9).Dias, 3);
    }

    /// <summary>
    /// Sin importe no hay trabajo que repartir. Se cuenta aparte para que se vea que
    /// falta el dato, en vez de rebajar la carga del técnico en silencio.
    /// </summary>
    [Fact]
    public void LosServiciosSinImporteSeCuentanAparteYNoBajanLaCarga()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("Daniel Martínez", "2026-09-07", "2026-09-25", 800),
            Servicio("Daniel Martínez", "2026-09-07", "2026-09-25", null),
            Servicio("Daniel Martínez", "2026-09-07", "2026-09-25", 0)
        ], new CapacidadMensual());

        Assert.Equal(2, filas[0].SinImporte);
        Assert.Equal(10, filas[0].Meses.Single(c => c.Mes == 9).Dias, 3);
    }

    [Fact]
    public void LosServiciosSinTecnicoVanAlFinalYSeVen()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("", "2026-09-07", "2026-09-25", 800),
            Servicio("Javier Ibor", "2026-09-07", "2026-09-25", 800)
        ], new CapacidadMensual());

        Assert.Equal("Javier Ibor", filas[0].Tecnico);
        Assert.Equal(CargaPorTecnico.SinTecnico, filas[^1].Tecnico);
    }

    [Fact]
    public void SinServiciosNoHayTabla()
    {
        var (meses, filas) = CargaPorTecnico.Calcular([], new CapacidadMensual());

        Assert.Empty(meses);
        Assert.Empty(filas);
    }

    /// <summary>Un mes en el que un técnico no tiene nada sale vacío, no a cero.</summary>
    [Fact]
    public void UnMesSinTrabajoSaleVacio()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("Daniel Martínez", "2026-08-03", "2026-08-28", 800),
            Servicio("Javier Ibor", "2026-10-05", "2026-10-23", 800)
        ], new CapacidadMensual());

        var daniel = filas.Single(f => f.Tecnico == "Daniel Martínez");

        Assert.True(daniel.Meses.Single(c => c.Mes == 10).Vacia);
        Assert.False(daniel.Meses.Single(c => c.Mes == 8).Vacia);
    }

    /// <summary>Una fecha disparatada no puede generar una tabla de mil columnas.</summary>
    [Fact]
    public void LaTablaTieneTope()
    {
        var (meses, _) = CargaPorTecnico.Calcular(
            [Servicio("Daniel Martínez", "2026-01-05", "2050-12-18", 800)], new CapacidadMensual());

        Assert.True(meses.Count <= 36, $"La tabla tiene {meses.Count} columnas.");
    }
}
