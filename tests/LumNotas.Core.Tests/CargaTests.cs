using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Carga de trabajo por técnico y mes.
/// <para>
/// El laboratorio mide el trabajo de un servicio <b>en horas</b>: divide el importe de
/// su oferta entre 105 y multiplica por 1,3. Una oferta de 2 000 € son unas 25 horas,
/// algo más de tres jornadas de ocho. Esas jornadas se reparten entre los meses que
/// abarca el servicio, en proporción a sus días entre semana, y se comparan con lo que
/// cabe en cada mes —22 días, salvo agosto (10) y diciembre (15)—.
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
    public void AgostoYDiciembreSonMesesCortos()
    {
        var capacidad = new CapacidadMensual();

        Assert.Equal(22, capacidad.Dias(3));
        Assert.Equal(10, capacidad.Dias(8));
        Assert.Equal(15, capacidad.Dias(12));
    }

    /// <summary>
    /// La cuenta del laboratorio: <b>importe ÷ 105 × 1,3 = horas</b>. Una oferta de
    /// 2 000 € son unas 25 horas, algo más de tres jornadas.
    /// </summary>
    [Fact]
    public void ElTrabajoSeMideEnHoras()
    {
        var capacidad = new CapacidadMensual();

        Assert.Equal(2000d / 105 * 1.3, capacidad.HorasDeTrabajo(2000), 6);
        Assert.Equal(24.8, capacidad.HorasDeTrabajo(2000), 1);

        // Y en jornadas, que es la unidad de la tabla de carga.
        Assert.Equal(capacidad.HorasDeTrabajo(2000) / 8, capacidad.DiasDeTrabajo(2000), 6);
        Assert.Equal(3.1, capacidad.DiasDeTrabajo(2000), 1);
    }

    /// <summary>
    /// La cuenta tiene que cuadrar con la tarifa que factura el laboratorio, 80 €/hora.
    /// No da 80 exactos —105 ÷ 1,3 son 80,77— y por eso se comprueba con margen: si
    /// alguien cambia el divisor o el factor y se aleja de la tarifa, se ve aquí.
    /// </summary>
    [Fact]
    public void LaCuentaCuadraConLaTarifaDelLaboratorio()
        => Assert.InRange(new CapacidadMensual().EurosPorHoraEfectivos, 79, 82);

    [Fact]
    public void LaTarifaYLaCapacidadSobrevivenAlGuardado()
    {
        var capacidad = new CapacidadMensual { EurosPorHora = 95, Factor = 1.2, HorasPorDia = 7.5 };
        capacidad.DiasPorMes[7] = 8;
        capacidad.Guardar(_carpeta);

        var leida = CapacidadMensual.Cargar(_carpeta);

        Assert.Equal(95, leida.EurosPorHora);
        Assert.Equal(1.2, leida.Factor);
        Assert.Equal(7.5, leida.HorasPorDia);
        Assert.Equal(8, leida.Dias(8));
    }

    /// <summary>Un fichero a medias no puede dejar el cálculo sin base.</summary>
    [Fact]
    public void UnFicheroCorruptoDevuelveLosValoresDePartida()
    {
        File.WriteAllText(Path.Combine(_carpeta, CapacidadMensual.NombreDeFichero), "{ roto");

        var leida = CapacidadMensual.Cargar(_carpeta);

        Assert.Equal(CapacidadMensual.EurosPorHoraPorDefecto, leida.EurosPorHora);
        Assert.Equal(CapacidadMensual.FactorPorDefecto, leida.Factor);
        Assert.Equal(12, leida.DiasPorMes.Count);
    }

    /// <summary>
    /// Un <c>capacidad.json</c> escrito antes de pasar a horas no trae ninguno de los
    /// campos nuevos. No puede dejar la carga en cero: se rellenan con los de partida.
    /// </summary>
    [Fact]
    public void UnFicheroAnteriorAlCambioAHorasSeCompleta()
    {
        File.WriteAllText(Path.Combine(_carpeta, CapacidadMensual.NombreDeFichero),
            """{ "EurosPorDia": 80, "DiasPorMes": [22,22,22,22,22,22,22,10,22,22,22,15] }""");

        var leida = CapacidadMensual.Cargar(_carpeta);

        Assert.Equal(CapacidadMensual.EurosPorHoraPorDefecto, leida.EurosPorHora);
        Assert.Equal(CapacidadMensual.FactorPorDefecto, leida.Factor);
        Assert.Equal(CapacidadMensual.HorasPorDiaPorDefecto, leida.HorasPorDia);
        Assert.True(leida.DiasDeTrabajo(2000) > 0);
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

    /// <remarks>
    /// Por defecto «En curso», que es un servicio que ocupa. Lo terminado se pide a
    /// propósito, porque es el caso raro y el que cambia el resultado.
    /// </remarks>
    private static ServicioPlanificado Servicio(string tecnico, string inicio, string fin, double? importe,
                                                EstadoDeProyecto estado = EstadoDeProyecto.EnCurso)
        => new(tecnico, DateTime.Parse(inicio), DateTime.Parse(fin), importe, estado);

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

    // ---- lo terminado no es carga ------------------------------------------

    /// <summary>
    /// <b>El caso que lo destapó</b>: un técnico con diciembre entero ya cerrado salía al
    /// 122 % (laboratorio, 2026‑08‑07). La tabla contesta «¿cabe lo que le queda?», y un
    /// ensayo hecho no ocupa a nadie — avisaba de una sobrecarga que no existía.
    /// </summary>
    [Fact]
    public void UnServicioTerminadoNoOcupaAlTecnico()
    {
        var (meses, filas) = CargaPorTecnico.Calcular(
            [Servicio("Raúl", "2026-12-01", "2026-12-23", 10000, EstadoDeProyecto.Terminado)],
            new CapacidadMensual());

        // Ni carga, ni fila, ni columna: si fuera lo único que hay, la tabla está vacía.
        Assert.Empty(filas);
        Assert.Empty(meses);
    }

    /// <summary>
    /// Y no se lleva por delante lo que sí ocupa: en el mismo mes, lo terminado desaparece
    /// y lo que sigue en marcha se queda con su carga entera.
    /// </summary>
    [Fact]
    public void EnElMismoMesSoloCuentaLoQueSigueEnMarcha()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("Raúl", "2026-12-01", "2026-12-23", 10000, EstadoDeProyecto.Terminado),
            Servicio("Raúl", "2026-12-01", "2026-12-23", 800, EstadoDeProyecto.EnCurso)
        ], new CapacidadMensual());

        var soloElVivo = CargaPorTecnico.Calcular(
            [Servicio("Raúl", "2026-12-01", "2026-12-23", 800)], new CapacidadMensual());

        var conAmbos = filas[0].Meses.Single(c => c.Mes == 12);
        var esperado = soloElVivo.Filas[0].Meses.Single(c => c.Mes == 12);

        Assert.Equal(esperado.Dias, conAmbos.Dias, 3);
    }

    /// <summary>
    /// Los demás estados sí ocupan, incluido «Pendiente cliente»: manda el estado que puso
    /// la persona (DD‑74), y esperar a que el cliente confirme algo no es haber acabado.
    /// </summary>
    [Theory]
    [InlineData(EstadoDeProyecto.PorHacer)]
    [InlineData(EstadoDeProyecto.Planificado)]
    [InlineData(EstadoDeProyecto.EnCurso)]
    [InlineData(EstadoDeProyecto.PendienteCliente)]
    public void LosDemasEstadosSiguenOcupando(EstadoDeProyecto estado)
    {
        var (_, filas) = CargaPorTecnico.Calcular(
            [Servicio("Raúl", "2026-12-01", "2026-12-23", 800, estado)], new CapacidadMensual());

        Assert.True(filas[0].Meses.Single(c => c.Mes == 12).Dias > 0);
    }

    /// <summary>
    /// <b>El aviso que justifica la vista.</b> 25 días de trabajo en un agosto donde solo
    /// caben 10 es un 250 %: el mes no da de sí y hay que verlo antes, no en septiembre.
    /// </summary>
    [Fact]
    public void UnAgostoSobrevendidoPasaDelCienPorCien()
    {
        // 10 000 € son 124 horas: 15,5 jornadas en un agosto que solo tiene 10.
        var (_, filas) = CargaPorTecnico.Calcular(
            [Servicio("Daniel Martínez", "2026-08-03", "2026-08-28", 10000)], new CapacidadMensual());

        var agosto = filas[0].Meses.Single(c => c.Mes == 8);

        Assert.Equal(15.5, agosto.Dias, 1);
        Assert.Equal(10, agosto.Capacidad);
        Assert.InRange(agosto.Porcentaje, 154, 156);
    }

    [Fact]
    public void UnMesNormalConUnServicioModestoVaHolgado()
    {
        var (_, filas) = CargaPorTecnico.Calcular(
            [Servicio("Javier Ibor", "2026-09-07", "2026-09-25", 800)], new CapacidadMensual());

        var septiembre = filas[0].Meses.Single(c => c.Mes == 9);

        Assert.Equal(1.24, septiembre.Dias, 2);     // 800 ÷ 105 × 1,3 = 9,9 h
        Assert.Equal(22, septiembre.Capacidad);
        Assert.InRange(septiembre.Porcentaje, 5, 6);
    }

    [Fact]
    public void LosServiciosDelMismoTecnicoSeSuman()
    {
        var (_, filas) = CargaPorTecnico.Calcular([
            Servicio("Daniel Martínez", "2026-09-07", "2026-09-11", 800),
            Servicio("Daniel Martínez", "2026-09-14", "2026-09-18", 800)
        ], new CapacidadMensual());

        Assert.Single(filas);
        Assert.Equal(2.48, filas[0].Meses.Single(c => c.Mes == 9).Dias, 2);
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
        Assert.Equal(1.24, filas[0].Meses.Single(c => c.Mes == 9).Dias, 2);
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
