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

    [Fact]
    public void LosMesesDeLaCabeceraCubrenTodoElEje()
    {
        var eje = EjeDeSemanas.Para([(new DateTime(2026, 8, 3), new DateTime(2026, 11, 30))],
                                    new DateTime(2026, 8, 15), 46);

        Assert.Equal(eje.Ancho, eje.Meses.Sum(m => m.Ancho), 3);
        Assert.Equal(eje.Meses.Count, eje.Meses.Select(m => m.Nombre).Distinct().Count());
    }
}
