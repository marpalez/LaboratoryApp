using System.Globalization;
using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Planificación de servicios y línea de tiempo del tablero.
/// <para>
/// Lo que se vigila aquí es que la planificación conviva con la toma de notas sin
/// pisarla ni ser pisada, y que la aritmética de semanas ISO —que es la que usa el
/// laboratorio para planificar— no se rompa en los saltos de año.
/// </para>
/// </summary>
public class PlanificacionTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-plan-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    public PlanificacionTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Guardar(DatosProyecto datos, string nombre = "servicio")
    {
        var ruta = Path.Combine(_carpeta, nombre + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0");
        return ruta;
    }

    private static DatosProyecto Proyecto(string codigo = "111112026")
    {
        var datos = new DatosProyecto { CodigoServicio = codigo, NumeroMuestras = 2 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        return datos;
    }

    private static Planificacion Plan(int diaInicio = 10, int diaFin = 20) => new()
    {
        Inicio = new DateTime(2026, 8, diaInicio),
        Fin = new DateTime(2026, 8, diaFin),
        Estado = EstadoDeProyecto.EnCurso,
        RecepcionMuestras = new DateTime(2026, 8, 3)
    };

    // ---- persistencia ------------------------------------------------------

    [Fact]
    public void LaPlanificacionSobreviveAlGuardadoYALaLectura()
    {
        var ruta = Guardar(Proyecto());
        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var leida = _repositorio.LeerPlanificacion(ruta);

        Assert.Equal(new DateTime(2026, 8, 10), leida.Inicio);
        Assert.Equal(new DateTime(2026, 8, 20), leida.Fin);
        Assert.Equal(EstadoDeProyecto.EnCurso, leida.Estado);
        Assert.Equal(new DateTime(2026, 8, 3), leida.RecepcionMuestras);
        Assert.False(leida.Archivado);
    }

    /// <summary>
    /// En el fichero solo se guarda lo que se decide, no lo que se deduce. Sin esto el
    /// <c>.lmnlab</c> acababa con campos como «hayFechas» o «esVacia», que además
    /// mentirían en cuanto alguien editara las fechas a mano.
    /// </summary>
    [Fact]
    public void ElFicheroNoGuardaLoQueSeCalcula()
    {
        var ruta = Guardar(Proyecto());
        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var texto = File.ReadAllText(ruta);

        foreach (var calculado in new[] { "hayFechas", "esVacia", "finEfectivo", "muestrasRecibidas",
                                          "semanas", "rotuloSemanas" })
            Assert.DoesNotContain(calculado, texto, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("inicio", texto, StringComparison.OrdinalIgnoreCase);
    }

    // ---- duración en semanas ----------------------------------------------

    /// <summary>
    /// Las semanas se redondean <b>hacia arriba</b>: un servicio de un día ocupa una
    /// semana de agenda igual que uno de cinco, y decir «0W» no tendría sentido.
    /// </summary>
    [Theory]
    [InlineData(10, 10, 1)]     // un solo día
    [InlineData(10, 16, 1)]     // siete días justos, la semana entera
    [InlineData(10, 17, 2)]     // uno más y ya se va a la segunda
    [InlineData(10, 30, 3)]     // veintiún días
    public void LasSemanasSeRedondeanHaciaArriba(int diaInicio, int diaFin, int esperadas)
    {
        var plan = new Planificacion
        {
            Inicio = new DateTime(2026, 8, diaInicio),
            Fin = new DateTime(2026, 8, diaFin)
        };

        Assert.Equal(esperadas, plan.Semanas);
        Assert.Equal($"{esperadas}W", plan.RotuloSemanas);
    }

    /// <summary>Sin fechas no hay duración que enseñar, y el rótulo se cae del renglón.</summary>
    [Fact]
    public void SinFechasNoHaySemanas()
    {
        Assert.Null(new Planificacion().Semanas);
        Assert.Equal("", new Planificacion().RotuloSemanas);

        // Con solo una de las dos tampoco: media fecha no es un tramo.
        Assert.Null(new Planificacion { Inicio = new DateTime(2026, 8, 10) }.Semanas);
        Assert.Null(new Planificacion { Fin = new DateTime(2026, 8, 10) }.Semanas);
    }

    /// <summary>
    /// Un fin anterior al inicio es una errata, no una duración negativa: se mide contra
    /// <see cref="Planificacion.FinEfectivo"/>, que ya la corrige, y sale un día.
    /// </summary>
    [Fact]
    public void UnFinAnteriorAlInicioNoDaSemanasNegativas()
    {
        var plan = new Planificacion
        {
            Inicio = new DateTime(2026, 8, 20),
            Fin = new DateTime(2026, 8, 10)
        };

        Assert.Equal(1, plan.Semanas);
    }

    /// <summary>
    /// La hora no cuenta. Dos servicios del mismo día que se teclearon a horas distintas
    /// tienen que decir lo mismo, o el tablero enseñaría «1W» y «2W» sin motivo.
    /// </summary>
    [Fact]
    public void LaHoraNoCambiaLasSemanas()
    {
        var plan = new Planificacion
        {
            Inicio = new DateTime(2026, 8, 10, 23, 50, 0),
            Fin = new DateTime(2026, 8, 16, 0, 10, 0)
        };

        Assert.Equal(1, plan.Semanas);
    }

    /// <summary>
    /// Quitar las fechas devuelve el servicio a «pendiente de planificar» sin perder lo
    /// demás: el estado y la recepción de muestras siguen ahí.
    /// </summary>
    [Fact]
    public void QuitarLasFechasDejaElServicioPendienteDePlanificar()
    {
        var ruta = Guardar(Proyecto());
        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var sinFechas = _repositorio.LeerPlanificacion(ruta).Copia();
        sinFechas.Inicio = null;
        sinFechas.Fin = null;
        _repositorio.ActualizarPlanificacion(ruta, sinFechas);

        var leida = _repositorio.LeerPlanificacion(ruta);
        Assert.False(leida.HayFechas);
        Assert.False(leida.EsVacia);                                  // sigue teniendo estado
        Assert.Equal(EstadoDeProyecto.EnCurso, leida.Estado);
        Assert.Equal(new DateTime(2026, 8, 3), leida.RecepcionMuestras);
    }

    /// <summary>
    /// Un proyecto que nunca se ha planificado no arrastra un nodo vacío en el fichero,
    /// y leerlo devuelve una planificación en blanco en vez de fallar.
    /// </summary>
    [Fact]
    public void UnProyectoSinPlanificar_DevuelvePlanificacionVacia()
    {
        var ruta = Guardar(Proyecto());

        var leida = _repositorio.LeerPlanificacion(ruta);

        Assert.True(leida.EsVacia);
        Assert.DoesNotContain("planificacion", File.ReadAllText(ruta), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <b>La regla que evita perder trabajo.</b> El técnico abre el proyecto, el jefe le
    /// mueve las fechas desde el calendario, y el técnico guarda media hora después: las
    /// fechas tienen que seguir ahí. Por eso la toma de notas nunca escribe planificación,
    /// la conserva releyéndola del disco.
    /// </summary>
    [Fact]
    public void GuardarLaTomaDeNotas_NoPisaLaPlanificacion()
    {
        var datos = Proyecto();
        var ruta = Guardar(datos);              // el técnico tenía el proyecto abierto

        _repositorio.ActualizarPlanificacion(ruta, Plan());   // otro mueve las fechas

        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 8, 12));
        _repositorio.Guardar(datos, ruta, "1.0.0");           // el técnico guarda lo suyo

        var leida = _repositorio.LeerPlanificacion(ruta);
        Assert.Equal(new DateTime(2026, 8, 10), leida.Inicio);
        Assert.Equal(EstadoDeProyecto.EnCurso, leida.Estado);
    }

    /// <summary>Y al revés: planificar no puede tocar ni un dato de ensayo.</summary>
    [Fact]
    public void PlanificarNoTocaLosDatosDeEnsayo()
    {
        var datos = Proyecto();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));
        datos.Marcar("7", "sujecion", "tornillo");
        datos.CargarNa("11.2", true);
        var ruta = Guardar(datos);

        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var releido = _repositorio.Cargar(ruta);
        Assert.Equal(new DateTime(2026, 7, 20), releido.Obtener("6", "ambiente.fecha"));
        Assert.True(releido.Marcada("7", "sujecion", "tornillo"));
        Assert.True(releido.Na("11.2"));
        Assert.Equal("111112026", releido.CodigoServicio);
    }

    [Fact]
    public void ElTableroVeLaPlanificacionSinAbrirElProyecto()
    {
        var ruta = Guardar(Proyecto(), "111112026");
        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var resumenes = new ExploradorDeProyectos(_repositorio, Path.Combine(_carpeta, "cache.json")).Explorar(_carpeta, Contexto.Plantilla);

        var resumen = Assert.Single(resumenes);
        Assert.Equal(new DateTime(2026, 8, 10), resumen.Planificacion.Inicio);
        Assert.True(resumen.Planificacion.MuestrasRecibidas);
    }

    // ---- estado ------------------------------------------------------------

    [Fact]
    public void UnServicioSeRetrasaCuandoPasaSuFinSinTerminar()
    {
        var plan = Plan();
        var despues = new DateTime(2026, 8, 25);

        Assert.True(plan.Retrasado(despues));
        Assert.False(plan.Retrasado(new DateTime(2026, 8, 15)));

        plan.Estado = EstadoDeProyecto.Terminado;
        Assert.False(plan.Retrasado(despues));
    }

    /// <summary>Una fecha de fin anterior al inicio dibujaría la barra del revés.</summary>
    [Fact]
    public void UnFinAnteriorAlInicioSeCorrigeAlDibujar()
    {
        var plan = new Planificacion { Inicio = new DateTime(2026, 8, 20), Fin = new DateTime(2026, 8, 10) };

        Assert.Equal(new DateTime(2026, 8, 20), plan.FinEfectivo);
    }

    // ---- el eje de semanas -------------------------------------------------

    [Fact]
    public void ElLunesDeCualquierDiaEsElDeSuSemana()
    {
        foreach (var dia in Enumerable.Range(0, 400).Select(i => new DateTime(2026, 1, 1).AddDays(i)))
        {
            var lunes = EjeDeSemanas.LunesDe(dia);

            Assert.Equal(DayOfWeek.Monday, lunes.DayOfWeek);
            Assert.InRange((dia - lunes).TotalDays, 0, 6);
        }
    }

    [Fact]
    public void CadaCeldaLlevaElNumeroDeSemanaIsoDeSuLunes()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 10), new DateTime(2026, 9, 30))],
                                    new DateTime(2026, 8, 15), 46);

        Assert.All(eje.Celdas, c => Assert.Equal(ISOWeek.GetWeekOfYear(c.Lunes), c.Numero));
        Assert.All(eje.Celdas, c => Assert.Equal(DayOfWeek.Monday, c.Lunes.DayOfWeek));
    }

    /// <summary>
    /// El salto de año es donde se rompen las cuentas de semanas: hay años de 53 y la
    /// semana 1 puede empezar en diciembre. Las celdas deben seguir siendo consecutivas.
    /// </summary>
    [Fact]
    public void ElEjeCruzaElCambioDeAnoSinSaltarseSemanas()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 12, 7), new DateTime(2027, 1, 25))],
                                    new DateTime(2026, 12, 20), 46);

        for (var i = 1; i < eje.Celdas.Count; i++)
            Assert.Equal(7, (eje.Celdas[i].Lunes - eje.Celdas[i - 1].Lunes).TotalDays);

        Assert.Contains(eje.Celdas, c => c.Numero == 1);
        Assert.Contains(eje.Celdas, c => c.Numero >= 52);
    }

    /// <summary>
    /// Aunque no haya ningún proyecto cerca, el calendario tiene que abrir enseñando la
    /// semana en curso: si no, arranca en un sitio que no dice nada.
    /// </summary>
    [Fact]
    public void ElEjeSiempreIncluyeLaSemanaActual()
    {
        var hoy = new DateTime(2026, 8, 15);

        foreach (var eje in new[]
                 {
                     EjeDeSemanas.Para([], hoy, 46),
                     EjeDeSemanas.Para([(new DateTime(2025, 2, 3), new DateTime(2025, 3, 3))], hoy, 46),
                     EjeDeSemanas.Para([(new DateTime(2027, 2, 3), new DateTime(2027, 3, 3))], hoy, 46)
                 })
        {
            Assert.True(eje.HoyEstaDentro);
            Assert.Contains(eje.Celdas, c => c.EsActual);
            Assert.InRange(eje.PosicionDeHoy, 0, eje.Ancho);
        }
    }

    [Fact]
    public void UnEjeVacioNoSaleRidiculamenteCorto()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 46);

        Assert.True(eje.Semanas >= 12, $"El eje solo tiene {eje.Semanas} semanas.");
    }

    /// <summary>
    /// <b>Siempre se dibuja medio año por detrás del último trabajo.</b> Con dos semanas de
    /// margen, arrastrar un trabajo hasta el borde lo dejaba sin calendario debajo donde
    /// soltarlo y había que parar a pedir sitio con «▶».
    /// </summary>
    [Fact]
    public void ElEjeLlegaSiempreMedioAnoMasAllaDelUltimoTrabajo()
    {
        var fin = new DateTime(2027, 8, 1);
        var eje = EjeDeSemanas.Para([(new DateTime(2027, 7, 1), fin)], new DateTime(2026, 8, 15), 46);

        Assert.True(eje.Hasta >= new DateTime(2028, 1, 1),
                    $"El eje solo llega hasta {eje.Hasta:dd/MM/yyyy}.");
    }

    /// <summary>
    /// Y el salto de año se dibuja solo: era donde más se notaba, porque el año siguiente
    /// ni aparecía en la cabecera.
    /// </summary>
    [Fact]
    public void ElEjeCruzaElAnoSinPedirSitio()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 11, 2), new DateTime(2026, 11, 30))],
                                    new DateTime(2026, 11, 10), 46);

        Assert.Contains(eje.Meses, m => m.Nombre.Contains("2027"));
    }

    /// <summary>Sin nada planificado, el sitio para planificar tiene que estar ya puesto.</summary>
    [Fact]
    public void UnEjeVacioTambienLlegaMedioAnoMasAlla()
    {
        var hoy = new DateTime(2026, 8, 15);
        var eje = EjeDeSemanas.Para([], hoy, 46);

        Assert.True(eje.Hasta >= hoy.AddMonths(6), $"El eje solo llega hasta {eje.Hasta:dd/MM/yyyy}.");
    }

    [Fact]
    public void LaBarraEmpiezaDondeLaFechaDeInicio()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16))],
                                    new DateTime(2026, 8, 12), 70);

        // El 10 de agosto de 2026 es lunes: la barra cae justo en el borde de su semana.
        var celda = eje.Celdas.Single(c => c.Lunes == new DateTime(2026, 8, 10));

        Assert.Equal(celda.Izquierda, eje.PosicionDe(new DateTime(2026, 8, 10)), 3);
        Assert.Equal(70, eje.AnchoEntre(new DateTime(2026, 8, 10), new DateTime(2026, 8, 16)), 3);
    }

    /// <summary>Un servicio de un solo día tiene que verse; cero píxeles no se ve.</summary>
    [Fact]
    public void UnServicioDeUnSoloDiaSigueDibujandose()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 46);

        Assert.True(eje.AnchoEntre(new DateTime(2026, 8, 12), new DateTime(2026, 8, 12)) > 0);
    }

    // ---- arrastrar las barras con el ratón ---------------------------------

    /// <summary>Píxeles y días tienen que ser la misma cosa vista de dos maneras.</summary>
    [Fact]
    public void ArrastrarUnaSemanaEnPixelesSonSieteDias()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 70);

        Assert.Equal(7, eje.DiasEn(70));
        Assert.Equal(-7, eje.DiasEn(-70));
        Assert.Equal(1, eje.DiasEn(10));

        // Y al revés: donde dibuja una fecha es donde se lee esa fecha.
        var fecha = new DateTime(2026, 8, 12);
        Assert.Equal(fecha, eje.FechaEn(eje.PosicionDe(fecha)));
    }

    /// <summary>
    /// A zoom mínimo un día son menos de cuatro píxeles: sin ajustar a días enteros el
    /// técnico no podría colocar nada donde quiere.
    /// </summary>
    [Fact]
    public void ElArrastreSeAjustaADiasEnteros()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 26);

        Assert.Equal(0, eje.DiasEn(1));
        Assert.Equal(1, eje.DiasEn(4));
        Assert.Equal(7, eje.DiasEn(26));
    }

    [Fact]
    public void MoverLaBarraConservaLaDuracion()
    {
        var (inicio, fin) = ArrastreDeFechas.Aplicar(
            new DateTime(2026, 8, 10), new DateTime(2026, 8, 20), ModoArrastre.Mover, 7);

        Assert.Equal(new DateTime(2026, 8, 17), inicio);
        Assert.Equal(new DateTime(2026, 8, 27), fin);
    }

    [Fact]
    public void CadaBordeCambiaSoloSuFecha()
    {
        var inicial = (Inicio: new DateTime(2026, 8, 10), Fin: new DateTime(2026, 8, 20));

        var izquierdo = ArrastreDeFechas.Aplicar(inicial.Inicio, inicial.Fin, ModoArrastre.Inicio, -3);
        Assert.Equal(new DateTime(2026, 8, 7), izquierdo.Inicio);
        Assert.Equal(inicial.Fin, izquierdo.Fin);

        var derecho = ArrastreDeFechas.Aplicar(inicial.Inicio, inicial.Fin, ModoArrastre.Fin, 5);
        Assert.Equal(inicial.Inicio, derecho.Inicio);
        Assert.Equal(new DateTime(2026, 8, 25), derecho.Fin);
    }

    /// <summary>Los bordes topan: un fin anterior al inicio no existe.</summary>
    [Fact]
    public void UnBordeNoPuedeAdelantarAlOtro()
    {
        var inicio = new DateTime(2026, 8, 10);
        var fin = new DateTime(2026, 8, 20);

        var estirado = ArrastreDeFechas.Aplicar(inicio, fin, ModoArrastre.Inicio, 60);
        Assert.Equal(fin, estirado.Inicio);
        Assert.Equal(fin, estirado.Fin);

        var encogido = ArrastreDeFechas.Aplicar(inicio, fin, ModoArrastre.Fin, -60);
        Assert.Equal(inicio, encogido.Inicio);
        Assert.Equal(inicio, encogido.Fin);
    }

    /// <summary>
    /// Al soltar una barra el eje se rehace, y si creciera el calendario se desplazaría
    /// bajo el ratón. Mientras las fechas nuevas sigan cayendo dentro, se conserva el
    /// que había. Arrastrar el servicio que marca el extremo sí lo reencuadra, porque
    /// el eje siempre deja dos semanas de margen alrededor de lo que hay.
    /// </summary>
    [Fact]
    public void ElEjeSeConservaMientrasLasFechasNuevasQuepan()
    {
        var hoy = new DateTime(2026, 8, 15);
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 7, 6), new DateTime(2026, 9, 25))], hoy, 46);

        var dentro = EjeDeSemanas.Para([(new DateTime(2026, 7, 20), new DateTime(2026, 9, 11))], hoy, 46);
        Assert.True(eje.Cubre(dentro));

        var lejos = EjeDeSemanas.Para([(new DateTime(2027, 1, 4), new DateTime(2027, 2, 5))], hoy, 46);
        Assert.False(eje.Cubre(lejos));

        // Cambiar el zoom siempre obliga a rehacerlo: las medidas ya no valen.
        Assert.False(eje.Cubre(EjeDeSemanas.Para([(new DateTime(2026, 8, 3), new DateTime(2026, 8, 10))], hoy, 78)));
    }

    // ---- el gesto entero, sin ratón ----------------------------------------

    private static (BarraDePlanificacion Barra, Planificacion Plan) Barra(double anchoSemana = 70)
    {
        var plan = Plan();   // 10 → 20 de agosto de 2026
        var eje = EjeDeSemanas.Para([(plan.Inicio!.Value, plan.Fin!.Value)], new DateTime(2026, 8, 15), anchoSemana);
        return (new BarraDePlanificacion(plan, eje), plan);
    }

    [Fact]
    public void ArrastrarLaBarraMueveLasDosFechasYLoQueSeDibuja()
    {
        var (barra, _) = Barra();
        var izquierdaAntes = barra.Izquierda;
        var anchoAntes = barra.Ancho;

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(70);   // una semana

        Assert.Equal(new DateTime(2026, 8, 17), barra.Inicio);
        Assert.Equal(new DateTime(2026, 8, 27), barra.Fin);
        Assert.Equal(izquierdaAntes + 70, barra.Izquierda, 3);
        Assert.Equal(anchoAntes, barra.Ancho, 3);      // mover no cambia la duración
        Assert.True(barra.HayCambio);
    }

    [Fact]
    public void ArrastrarElBordeDerechoSoloAlargaLaBarra()
    {
        var (barra, _) = Barra();
        var izquierdaAntes = barra.Izquierda;
        var anchoAntes = barra.Ancho;

        barra.Empezar(ModoArrastre.Fin);
        barra.Arrastrar(70);

        Assert.Equal(new DateTime(2026, 8, 10), barra.Inicio);
        Assert.Equal(new DateTime(2026, 8, 27), barra.Fin);
        Assert.Equal(izquierdaAntes, barra.Izquierda, 3);
        Assert.Equal(anchoAntes + 70, barra.Ancho, 3);
    }

    /// <summary>
    /// El gesto se calcula siempre desde el punto de partida, no acumulando: ir y volver
    /// tiene que dejar la barra exactamente donde estaba, o el redondeo a días se
    /// arrastraría y el servicio acabaría movido sin que nadie lo pidiera.
    /// </summary>
    [Fact]
    public void IrYVolverNoDejaRastro()
    {
        var (barra, _) = Barra();

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(23);
        barra.Arrastrar(51);
        barra.Arrastrar(9);
        barra.Arrastrar(0);

        Assert.False(barra.HayCambio);
        Assert.Equal(new DateTime(2026, 8, 10), barra.Inicio);
        Assert.Equal(new DateTime(2026, 8, 20), barra.Fin);
    }

    [Fact]
    public void CancelarDevuelveLaBarraASuSitio()
    {
        var (barra, _) = Barra();

        barra.Empezar(ModoArrastre.Inicio);
        barra.Arrastrar(-140);
        Assert.True(barra.HayCambio);

        barra.Cancelar();

        Assert.False(barra.HayCambio);
        Assert.Equal(new DateTime(2026, 8, 10), barra.Inicio);
    }

    /// <summary>Soltar guarda las fechas nuevas y no toca nada más de la planificación.</summary>
    [Fact]
    public void ElResultadoConservaEstadoYRecepcionDeMuestras()
    {
        var (barra, plan) = Barra();

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(70);
        var resultado = barra.Resultado();

        Assert.Equal(new DateTime(2026, 8, 17), resultado.Inicio);
        Assert.Equal(plan.Estado, resultado.Estado);
        Assert.Equal(plan.RecepcionMuestras, resultado.RecepcionMuestras);
        Assert.Equal(plan.Archivado, resultado.Archivado);

        // Y el original no se ha tocado hasta que alguien guarde.
        Assert.Equal(new DateTime(2026, 8, 10), plan.Inicio);
    }

    /// <summary>
    /// <b>No se puede arrastrar una barra fuera del calendario dibujado.</b> Se podía, y
    /// la barra quedaba flotando en blanco, sin semanas debajo y sin saber en qué fecha
    /// se estaba soltando. Para ir más lejos se pide sitio con «▶».
    /// </summary>
    [Fact]
    public void LaBarraNoSeSaleDelCalendarioDibujado()
    {
        var (barra, _) = Barra();
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 10), new DateTime(2026, 8, 20))],
                                    new DateTime(2026, 8, 15), 70);

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(5000);

        Assert.True(barra.Fin!.Value < eje.Hasta, $"El fin {barra.Fin} se ha salido de {eje.Hasta}.");
        Assert.InRange(barra.Izquierda, 0, eje.Ancho);
        Assert.InRange(barra.Izquierda + barra.Ancho, 0, eje.Ancho);

        // Y al topar conserva la duración: mover no puede encoger el servicio.
        Assert.Equal(10, (barra.Fin.Value - barra.Inicio!.Value).Days);
    }

    [Fact]
    public void TampocoSeSaleArrastrandoHaciaAtras()
    {
        var (barra, _) = Barra();
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 10), new DateTime(2026, 8, 20))],
                                    new DateTime(2026, 8, 15), 70);

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(-5000);

        Assert.True(barra.Inicio!.Value >= eje.Desde);
        Assert.InRange(barra.Izquierda, 0, eje.Ancho);
        Assert.Equal(10, (barra.Fin!.Value - barra.Inicio.Value).Days);
    }

    [Fact]
    public void LosBordesTampocoSeSalen()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 10), new DateTime(2026, 8, 20))],
                                    new DateTime(2026, 8, 15), 70);

        var (izquierdo, _) = Barra();
        izquierdo.Empezar(ModoArrastre.Inicio);
        izquierdo.Arrastrar(-5000);
        Assert.Equal(eje.Desde, izquierdo.Inicio);

        var (derecho, _) = Barra();
        derecho.Empezar(ModoArrastre.Fin);
        derecho.Arrastrar(5000);
        Assert.True(derecho.Fin!.Value < eje.Hasta);
        Assert.InRange(derecho.Izquierda + derecho.Ancho, 0, eje.Ancho);
    }

    [Fact]
    public void UnServicioSinFechasNoSeArrastra()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 70);
        var barra = new BarraDePlanificacion(new Planificacion(), eje);

        barra.Empezar(ModoArrastre.Mover);
        barra.Arrastrar(200);

        Assert.False(barra.SePuedeArrastrar);
        Assert.False(barra.HayCambio);
        Assert.Null(barra.Inicio);
    }

    // ---- varios años -------------------------------------------------------

    /// <summary>
    /// El calendario no está atado a ningún año: se calcula, no se almacena. Un servicio
    /// planificado en 2027, en 2030 o en 2044 se dibuja igual que uno de esta semana.
    /// </summary>
    [Theory]
    [InlineData(2027)]
    [InlineData(2029)]
    [InlineData(2031)]
    public void UnServicioDeCualquierAnoSeDibuja(int ano)
    {
        var hoy = new DateTime(2026, 8, 15);
        var inicio = new DateTime(ano, 3, 8);
        var fin = new DateTime(ano, 4, 20);

        var eje = EjeDeSemanas.Para([(inicio, fin)], hoy, 46);

        Assert.True(eje.Contiene(inicio, fin));
        Assert.InRange(eje.PosicionDe(inicio), 0, eje.Ancho);
        Assert.Contains(eje.Celdas, c => c.Lunes.Year == ano);
        Assert.Equal(ISOWeek.GetWeekOfYear(inicio),
                     eje.Celdas.First(c => c.Lunes == EjeDeSemanas.LunesDe(inicio)).Numero);

        // Y el eje sigue empezando donde se trabaja, no en el año lejano.
        Assert.True(eje.HoyEstaDentro);
    }

    /// <summary>
    /// Se puede caminar hacia delante todo lo que haga falta para planificar en años
    /// futuros, y el eje crece justo lo pedido.
    /// </summary>
    [Fact]
    public void PedirSitioHaciaDelanteAlargaElEje()
    {
        var hoy = new DateTime(2026, 8, 15);
        var eje = EjeDeSemanas.Para([], hoy, 46);
        var conSitio = EjeDeSemanas.Para([], hoy, 46, extraDespues: 8);

        Assert.Equal(eje.Semanas + 8, conSitio.Semanas);
        Assert.Equal(eje.Desde, conSitio.Desde);
        Assert.True(conSitio.HoyEstaDentro);
    }

    /// <summary>
    /// <b>La protección que evita colgar la aplicación.</b> Un año tecleado mal —3026 en
    /// vez de 2026— generaría cien mil semanas. Ni siquiera encuadra el calendario: el eje
    /// sale igual que si ese proyecto no estuviera, y el disparate queda fuera para que se
    /// vea que hay que corregirlo.
    /// </summary>
    [Theory]
    [InlineData(3026)]
    [InlineData(1026)]
    public void UnAnoDisparatadoNiEncuadraNiAgrandaElEje(int ano)
    {
        var hoy = new DateTime(2026, 8, 15);
        var disparate = (Inicio: new DateTime(ano, 5, 4), Fin: new DateTime(ano, 6, 8));
        var real = (Inicio: new DateTime(2026, 8, 3), Fin: new DateTime(2026, 9, 4));

        var conDisparate = EjeDeSemanas.Para([disparate, real], hoy, 46);
        var sinDisparate = EjeDeSemanas.Para([real], hoy, 46);

        Assert.Equal(sinDisparate.Semanas, conDisparate.Semanas);
        Assert.Equal(sinDisparate.Desde, conDisparate.Desde);
        Assert.True(conDisparate.Semanas <= EjeDeSemanas.MaximoSemanas);
        Assert.True(conDisparate.HoyEstaDentro);
        Assert.True(conDisparate.Contiene(real.Inicio, real.Fin));
        Assert.False(conDisparate.Contiene(disparate.Inicio, disparate.Fin));
    }

    /// <summary>Ni siquiera pidiendo sitio a lo bestia se pasa del tope.</summary>
    [Fact]
    public void PedirSitioTampocoSaltaElTope()
    {
        var eje = EjeDeSemanas.Para([], new DateTime(2026, 8, 15), 46,
                                    extraAntes: 100_000, extraDespues: 100_000);

        Assert.True(eje.Semanas <= EjeDeSemanas.MaximoSemanas);
        Assert.True(eje.HoyEstaDentro);
        Assert.Equal(eje.Semanas, eje.Celdas.Count);
    }

    // ---- qué proyectos se miran --------------------------------------------

    private static Planificacion Con(EstadoDeProyecto estado, bool archivado = false)
        => new() { Estado = estado, Archivado = archivado };

    /// <summary>
    /// <b>Lo único que esconde es archivar</b> (2026‑08‑05). Antes «En desarrollo» dejaba
    /// fuera lo terminado, y eso escondía trabajo que sigue vivo: un servicio terminado la
    /// semana pasada se sigue mirando —hay que facturarlo, el cliente pregunta—.
    /// </summary>
    [Fact]
    public void EnDesarrolloSoloDejaFueraLoArchivado()
    {
        foreach (var estado in Planificacion.Estados)
            Assert.True(FiltroDeEstado.Pasa(Con(estado), FiltroDeEstado.EnDesarrollo),
                        $"{Planificacion.EtiquetaDe(estado)} debería entrar");

        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), FiltroDeEstado.EnDesarrollo));
    }

    /// <summary>Es lo que se mira a diario, así que es lo que sale sin elegir nada.</summary>
    [Fact]
    public void SinElegirFiltroSeMiraLoQueEstaEnDesarrollo()
    {
        Assert.Equal(FiltroDeEstado.EnDesarrollo, FiltroDeEstado.Opciones[0]);

        foreach (var vacio in new string?[] { null, "" })
        {
            Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso), vacio));
            Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.Terminado), vacio));
            Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), vacio));
        }
    }

    /// <summary>
    /// Archivar es el único gesto que retira algo de las vistas generales. Es deliberado y
    /// quiere decir «quítamelo de en medio»; terminar, no.
    /// </summary>
    [Fact]
    public void NingunFiltroGeneralTraeLoArchivado()
    {
        foreach (var general in new string?[] { null, "", FiltroDeEstado.EnDesarrollo, FiltroDeEstado.Todos })
        {
            Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), general));
            Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.Terminado), general));
            Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso), general));
        }
    }

    /// <summary>
    /// Los estados y sus rótulos. <b>«Por hacer» se lee «Por planificar»</b>, y el nombre
    /// interno se queda como estaba: es lo que llevan escrito los ficheros ya guardados.
    /// </summary>
    [Fact]
    public void LosEstadosVanEnElOrdenEnQueAvanzaUnTrabajo()
    {
        Assert.Equal(
            ["Por planificar", "Planificado", "En curso", "Pendiente cliente", "Terminado"],
            Planificacion.Estados.Select(Planificacion.EtiquetaDe));

        Assert.Equal("Por planificar", Planificacion.EtiquetaDe(EstadoDeProyecto.PorHacer));
    }

    /// <summary>
    /// «Todos» ya no se ofrece porque significaba lo mismo que «En desarrollo» en cuanto
    /// dejó de traer lo cerrado; dos entradas idénticas en el desplegable solo confunden.
    /// El valor se sigue aceptando, para que nadie que lo pase se quede sin tablero.
    /// </summary>
    [Fact]
    public void ElDesplegableNoOfreceDosVecesLoMismo()
    {
        Assert.DoesNotContain(FiltroDeEstado.Todos, FiltroDeEstado.Opciones);
        Assert.Contains("Terminado", FiltroDeEstado.Opciones);
        Assert.Contains(FiltroDeEstado.Archivados, FiltroDeEstado.Opciones);
    }

    [Fact]
    public void ArchivadosEnsenaSoloLoApartado()
    {
        Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), FiltroDeEstado.Archivados));
        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso), FiltroDeEstado.Archivados));
    }

    /// <summary>
    /// Los servicios sin responsable se agrupan bajo <b>un solo rótulo</b>, y es el mismo
    /// en el calendario, en la carga y en el filtro del tablero. Si cada vista se
    /// inventara el suyo, filtrar por «(sin técnico)» dejaría de casar con lo que enseña
    /// el calendario, y nadie entendería por qué.
    /// </summary>
    [Fact]
    public void LoQueNoTieneTecnicoSeLlamaIgualEnTodasPartes()
    {
        Assert.Equal("(sin técnico)", CargaPorTecnico.SinTecnico);

        // Y lleva paréntesis a propósito: nadie se llama así, de modo que no puede
        // chocar con una persona de la lista del laboratorio.
        //
        // **Sí está en el catálogo, y desde el 2026‑08‑06 es lo único que trae** (DD‑132):
        // una instalación nueva no viene con técnicos. Antes este test comprobaba lo
        // contrario —que el rótulo NO estuviera— para que el cajón y las personas no se
        // mezclaran; ahora el cajón es una opción elegible, y por eso importa todavía más
        // que sea **este mismo texto** y no uno parecido: con «Sin técnico» en la lista y
        // «(sin técnico)» en las vistas, lo elegido a mano y lo que está sin asignar
        // saldrían en dos filas distintas queriendo decir lo mismo.
        Assert.Equal([CargaPorTecnico.SinTecnico], CatalogoDeTecnicos.DePartida().Tecnicos);
    }

    /// <summary>Quien busca «En curso» no quiere lo que se apartó de en medio.</summary>
    [Fact]
    public void UnEstadoConcretoNoTraeLoArchivado()
    {
        Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso), "En curso"));
        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), "En curso"));
        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.PorHacer), "En curso"));

        // Y «Terminado» sí se puede pedir a propósito.
        Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.Terminado), "Terminado"));
    }

    /// <summary>
    /// <b>Manda el estado que puso la persona, no el que deduce el programa.</b> Un
    /// servicio con todo relleno pero esperando al cliente no está terminado.
    /// </summary>
    [Fact]
    public void ElFiltroNoMiraElAvanceSinoLoQueDijoLaPersona()
    {
        // Pedir «En curso» trae ese estado y solo ese, lo diga el avance o no.
        Assert.True(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso), "En curso"));
        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.PendienteCliente), "En curso"));

        // Y un archivado no sale ni pidiendo su propio estado.
        Assert.False(FiltroDeEstado.Pasa(Con(EstadoDeProyecto.EnCurso, archivado: true), "En curso"));
    }

    // ---- carga de un técnico -----------------------------------------------

    [Fact]
    public void LaOcupacionCuentaLosDiasDeCadaServicio()
    {
        var dias = Ocupacion.Dias([
            (new DateTime(2026, 8, 3), new DateTime(2026, 8, 7)),      // 5 días
            (new DateTime(2026, 9, 1), new DateTime(2026, 9, 3))       // 3 días
        ]);

        Assert.Equal(8, dias);
    }

    /// <summary>
    /// <b>Dos servicios a la vez no ocupan el doble.</b> Sumar duraciones exageraría la
    /// carga justo de quien lleva varios a la vez, que es a quien el responsable busca.
    /// </summary>
    [Fact]
    public void LosServiciosQueSeSolapanNoCuentanDosVeces()
    {
        var solapados = Ocupacion.Dias([
            (new DateTime(2026, 8, 3), new DateTime(2026, 8, 14)),     // 12 días
            (new DateTime(2026, 8, 10), new DateTime(2026, 8, 21))     // 12 días, 5 en común
        ]);

        Assert.Equal(19, solapados);   // del 3 al 21, no 24
    }

    /// <summary>Un servicio que acaba el lunes y otro que empieza el martes son un tramo.</summary>
    [Fact]
    public void LosTramosPegadosSeUnen()
    {
        var dias = Ocupacion.Dias([
            (new DateTime(2026, 8, 3), new DateTime(2026, 8, 7)),
            (new DateTime(2026, 8, 8), new DateTime(2026, 8, 12))
        ]);

        Assert.Equal(10, dias);
    }

    [Fact]
    public void UnServicioContenidoEnOtroNoAnadeNada()
    {
        var dias = Ocupacion.Dias([
            (new DateTime(2026, 8, 3), new DateTime(2026, 8, 28)),
            (new DateTime(2026, 8, 10), new DateTime(2026, 8, 14))
        ]);

        Assert.Equal(26, dias);
    }

    [Fact]
    public void SinServiciosNoHayOcupacion() => Assert.Equal(0, Ocupacion.Dias([]));

    [Fact]
    public void LaCargaSeCuentaEnDiasSiEsCortaYEnSemanasSiEsLarga()
    {
        Assert.Equal("1 proyecto", Ocupacion.Resumir(1, 0));
        Assert.Equal("1 proyecto | 3 días", Ocupacion.Resumir(1, 3));
        Assert.Equal("2 proyectos | 1 semana", Ocupacion.Resumir(2, 7));
        Assert.Equal("3 proyectos | 4 semanas", Ocupacion.Resumir(3, 22));
    }

    [Fact]
    public void LosMesesDeLaCabeceraCubrenTodoElEje()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 3), new DateTime(2026, 11, 30))],
                                    new DateTime(2026, 8, 15), 46);

        Assert.Equal(eje.Ancho, eje.Meses.Sum(m => m.Ancho), 3);
        Assert.Equal(eje.Meses.Count, eje.Meses.Select(m => m.Nombre).Distinct().Count());
    }
}
