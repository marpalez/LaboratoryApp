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
    /// <c>.lumproj</c> acababa con campos como «hayFechas» o «esVacia», que además
    /// mentirían en cuanto alguien editara las fechas a mano.
    /// </summary>
    [Fact]
    public void ElFicheroNoGuardaLoQueSeCalcula()
    {
        var ruta = Guardar(Proyecto());
        _repositorio.ActualizarPlanificacion(ruta, Plan());

        var texto = File.ReadAllText(ruta);

        foreach (var calculado in new[] { "hayFechas", "esVacia", "finEfectivo", "muestrasRecibidas" })
            Assert.DoesNotContain(calculado, texto, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("inicio", texto, StringComparison.OrdinalIgnoreCase);
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

        var resumenes = new ExploradorDeProyectos(_repositorio).Explorar(_carpeta, Contexto.Plantilla);

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

    [Fact]
    public void LosMesesDeLaCabeceraCubrenTodoElEje()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 3), new DateTime(2026, 11, 30))],
                                    new DateTime(2026, 8, 15), 46);

        Assert.Equal(eje.Ancho, eje.Meses.Sum(m => m.Ancho), 3);
        Assert.Equal(eje.Meses.Count, eje.Meses.Select(m => m.Nombre).Distinct().Count());
    }
}
