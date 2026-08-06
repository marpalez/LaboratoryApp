using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Cómo se colocan en fila las tomas de notas de un mismo trabajo. Antes la cadena solo se
/// dibujaba, así que el diálogo de planificación decía una cosa y el calendario otra; ahora
/// las fechas se escriben y hay una sola verdad.
/// </summary>
public class CadenaDelGrupoTests
{
    /// <summary>
    /// <b>Una familia con las fechas bloqueadas no la mueve ni la cadena.</b> Es la vía por
    /// la que el bloqueo se rompería sin que nadie lo viera venir: el técnico bloquea unas
    /// fechas comprometidas con el cliente, alguien guarda otra familia del mismo trabajo,
    /// y la cadena se las lleva por delante escribiendo en un fichero que nadie tenía
    /// abierto.
    /// </summary>
    [Fact]
    public void UnaFamiliaConLasFechasBloqueadasNoSeMueve()
    {
        var bloqueada = ConPlan("ANTAR250402-00", new Planificacion
        {
            Inicio = new DateTime(2026, 9, 1),
            Fin = new DateTime(2026, 9, 10),
            FechasBloqueadas = true
        });

        var otra = ConPlan("ANTAR250401-00", new Planificacion
        {
            Inicio = new DateTime(2026, 8, 3),
            Fin = new DateTime(2026, 8, 7)
        });

        var cambios = CadenaDelGrupo.Recolocar([otra, bloqueada], null, new DateTime(2026, 8, 1));

        Assert.DoesNotContain(cambios, c => c.Proyecto.Ruta == bloqueada.Ruta);
    }

    /// <summary>Y la de detrás arranca a partir de donde acaba la bloqueada.</summary>
    [Fact]
    public void LaSiguienteArrancaTrasLaBloqueada()
    {
        var bloqueada = ConPlan("ANTAR250401-00", new Planificacion
        {
            Inicio = new DateTime(2026, 9, 1),
            Fin = new DateTime(2026, 9, 10),
            FechasBloqueadas = true
        });

        var detras = ConPlan("ANTAR250402-00", new Planificacion
        {
            Inicio = new DateTime(2026, 9, 20),
            Fin = new DateTime(2026, 9, 24)
        });

        var cambios = CadenaDelGrupo.Recolocar([bloqueada, detras], null, new DateTime(2026, 8, 1));
        var movida = Assert.Single(cambios);

        Assert.Equal(new DateTime(2026, 9, 11), movida.Plan.Inicio);
    }

    private static ResumenDeProyecto ConPlan(string codigo, Planificacion plan)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            CodigoServicio = codigo[..9],
            Planificacion = plan
        };

    private static readonly DateTime Hoy = new(2026, 8, 5);

    private static ResumenDeProyecto Familia(string codigo, DateTime? inicio = null, DateTime? fin = null)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            Planificacion = new Planificacion { Inicio = inicio, Fin = fin, Grupo = "ANTAR2504" }
        };

    private static readonly DateTime Sep1 = new(2026, 9, 1);

    /// <summary>Lo que quedaría guardado, ya recolocado, para poder leerlo de un vistazo.</summary>
    private static List<(string Codigo, DateTime Inicio, DateTime Fin)> Cadena(
        IReadOnlyList<ResumenDeProyecto> miembros, ResumenDeProyecto? editada = null)
    {
        var cambios = CadenaDelGrupo.Recolocar(miembros, editada, Hoy);

        return [.. CadenaDelGrupo.EnOrden(miembros, editada).Select(m =>
        {
            var plan = cambios.FirstOrDefault(c => c.Proyecto == m).Plan ?? m.Planificacion;
            return (m.Rotulo, plan.Inicio!.Value, plan.Fin!.Value);
        })];
    }

    // ---- la fila -----------------------------------------------------------

    /// <summary>
    /// <b>La que empieza antes encabeza y conserva su fecha.</b> Es lo único que no se
    /// recoloca: a partir de ahí todo cuelga de ella.
    /// </summary>
    [Fact]
    public void LaQueEmpiezaAntesEncabezaYNoSeMueve()
    {
        var cadena = Cadena([
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(30)),
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5))
        ]);

        Assert.Equal("ANTAR250401", cadena[0].Codigo);
        Assert.Equal(Sep1, cadena[0].Inicio);
    }

    /// <summary>
    /// <b>Cada una empieza al día siguiente de que acabe la anterior.</b> Antes empezaba el
    /// mismo día, así que la frontera se contaba dos veces.
    /// </summary>
    [Fact]
    public void CadaUnaEmpiezaAlDiaSiguienteDeQueAcabeLaAnterior()
    {
        var cadena = Cadena([
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(30))
        ]);

        Assert.Equal(cadena[0].Fin.AddDays(1), cadena[1].Inicio);
    }

    /// <summary>Y al recolocarse conserva su duración: cambia de sitio, no de tamaño.</summary>
    [Fact]
    public void AlRecolocarseConservaSuDuracion()
    {
        var cadena = Cadena([
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(30))
        ]);

        Assert.Equal(5, (cadena[0].Fin - cadena[0].Inicio).TotalDays);
        Assert.Equal(10, (cadena[1].Fin - cadena[1].Inicio).TotalDays);
    }

    /// <summary>Con tres o más sigue igual: cada una detrás de la anterior, sin huecos.</summary>
    [Fact]
    public void ConVariasLaFilaNoDejaHuecosNiSolapes()
    {
        var cadena = Cadena([
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(10), Sep1.AddDays(25)),
            Familia("ANTAR250403-00", Sep1.AddDays(30), Sep1.AddDays(38))
        ]);

        for (var i = 1; i < cadena.Count; i++)
            Assert.Equal(cadena[i - 1].Fin.AddDays(1), cadena[i].Inicio);

        Assert.Equal([5d, 15d, 8d], [.. cadena.Select(c => (c.Fin - c.Inicio).TotalDays)]);
    }

    // ---- lo que falta ------------------------------------------------------

    /// <summary>Sin fecha de fin, una semana. Con duración cero no se vería en el calendario.</summary>
    [Fact]
    public void SinFechaDeFinSeLeDaUnaSemana()
    {
        var cadena = Cadena([
            Familia("ANTAR250401-00", Sep1),
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(25))
        ]);

        Assert.Equal(Sep1, cadena[0].Inicio);
        Assert.Equal(Sep1.AddDays(CadenaDelGrupo.DiasPorDefecto), cadena[0].Fin);
        Assert.Equal(Sep1.AddDays(CadenaDelGrupo.DiasPorDefecto + 1), cadena[1].Inicio);
    }

    /// <summary>Y sin ninguna fecha, mañana: es lo único que no se puede deducir de nada.</summary>
    [Fact]
    public void SinNingunaFechaEmpiezaManana()
    {
        var cadena = Cadena([Familia("ANTAR250401-00"), Familia("ANTAR250402-00")]);

        Assert.Equal(Hoy.AddDays(1), cadena[0].Inicio);
        Assert.Equal(Hoy.AddDays(1 + CadenaDelGrupo.DiasPorDefecto), cadena[0].Fin);
        Assert.Equal(cadena[0].Fin.AddDays(1), cadena[1].Inicio);
    }

    /// <summary>La recién adjuntada, que no trae fechas, se va al final de la fila.</summary>
    [Fact]
    public void LaRecienAdjuntadaSinFechasVaAlFinal()
    {
        var nueva = Familia("ANTAR250401-00");

        var cadena = Cadena([Familia("ANTAR250409-00", Sep1, Sep1.AddDays(5)), nueva], nueva);

        Assert.Equal("ANTAR250409", cadena[0].Codigo);
        Assert.Equal("ANTAR250401", cadena[1].Codigo);
    }

    // ---- reordenar con las fechas ------------------------------------------

    /// <summary>
    /// <b>Adelantar una familia es ponerle una fecha de inicio anterior.</b> No hay botones
    /// ni número de orden guardado: manda la fecha, que es el gesto natural y no puede
    /// desincronizarse de lo que se ve.
    /// </summary>
    [Fact]
    public void PonerUnaFechaAnteriorLaAdelanta()
    {
        var segunda = Familia("ANTAR250402-00", Sep1.AddDays(-3), Sep1.AddDays(12));

        var cadena = Cadena([Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)), segunda], segunda);

        Assert.Equal("ANTAR250402", cadena[0].Codigo);
        Assert.Equal(Sep1.AddDays(-3), cadena[0].Inicio);
        Assert.Equal("ANTAR250401", cadena[1].Codigo);
    }

    /// <summary>
    /// <b>Con el mismo día también se invierte.</b> El empate lo gana la que se acaba de
    /// tocar: si lo decidiera el código, teclear el mismo día no haría nada y parecería que
    /// el programa no obedece.
    /// </summary>
    [Fact]
    public void ConElMismoDiaGanaLaQueSeAcabaDeTocar()
    {
        var segunda = Familia("ANTAR250402-00", Sep1, Sep1.AddDays(12));

        var cadena = Cadena([Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)), segunda], segunda);

        Assert.Equal("ANTAR250402", cadena[0].Codigo);
    }

    /// <summary>Y sin tocar nada, el empate lo desempata el código, que es estable.</summary>
    [Fact]
    public void SinTocarNadaElEmpateLoDeshaceElCodigo()
    {
        var cadena = Cadena([
            Familia("ANTAR250402-00", Sep1, Sep1.AddDays(12)),
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5))
        ]);

        Assert.Equal("ANTAR250401", cadena[0].Codigo);
    }

    /// <summary>Meterse por en medio también vale: se ordena por la fecha tecleada.</summary>
    [Fact]
    public void UnaFechaEnMedioLaColocaEnMedio()
    {
        var tercera = Familia("ANTAR250403-00", Sep1.AddDays(2), Sep1.AddDays(6));

        var cadena = Cadena([
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(25)),
            tercera
        ], tercera);

        Assert.Equal(["ANTAR250401", "ANTAR250403", "ANTAR250402"], [.. cadena.Select(c => c.Codigo)]);
    }

    // ---- no escribir por escribir ------------------------------------------

    /// <summary>
    /// Una cadena ya colocada <b>no se vuelve a escribir</b>. Sin esto, cada refresco
    /// marcaría como tocados ficheros que están en OneDrive y no ha cambiado nadie.
    /// </summary>
    [Fact]
    public void UnaCadenaYaColocadaNoSeReescribe()
    {
        var miembros = new[]
        {
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(6), Sep1.AddDays(20))
        };

        Assert.Empty(CadenaDelGrupo.Recolocar(miembros, null, Hoy));
    }

    /// <summary>Y solo se escribe lo que de verdad se mueve, no el grupo entero.</summary>
    [Fact]
    public void SoloSeEscribeLoQueSeMueve()
    {
        var miembros = new[]
        {
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(30), Sep1.AddDays(40))
        };

        var cambio = Assert.Single(CadenaDelGrupo.Recolocar(miembros, null, Hoy));

        Assert.Equal("ANTAR250402", cambio.Proyecto.Rotulo);
    }

    /// <summary>Lo que no son fechas —importe, grupo, estado— viaja intacto.</summary>
    [Fact]
    public void ElRestoDeLaPlanificacionNoSeToca()
    {
        var segunda = Familia("ANTAR250402-00", Sep1.AddDays(30), Sep1.AddDays(40));
        segunda.Planificacion.Importe = 2400;
        segunda.Planificacion.Estado = EstadoDeProyecto.EnCurso;
        segunda.Planificacion.RecepcionMuestras = Sep1;

        var cambio = Assert.Single(CadenaDelGrupo.Recolocar(
            [Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)), segunda], null, Hoy));

        Assert.Equal(2400, cambio.Plan.Importe);
        Assert.Equal(EstadoDeProyecto.EnCurso, cambio.Plan.Estado);
        Assert.Equal("ANTAR2504", cambio.Plan.Grupo);
        Assert.Equal(Sep1, cambio.Plan.RecepcionMuestras);
    }

    /// <summary>
    /// Recolocar es <b>estable</b>: aplicar el resultado y volver a recolocar no cambia
    /// nada. Si no lo fuera, cada guardado movería el trabajo un poco más lejos.
    /// </summary>
    [Fact]
    public void RecolocarDosVecesDaLoMismo()
    {
        var miembros = new[]
        {
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(20), Sep1.AddDays(30)),
            Familia("ANTAR250403-00")
        };

        foreach (var (proyecto, plan) in CadenaDelGrupo.Recolocar(miembros, null, Hoy))
        {
            proyecto.Planificacion.Inicio = plan.Inicio;
            proyecto.Planificacion.Fin = plan.Fin;
        }

        Assert.Empty(CadenaDelGrupo.Recolocar(miembros, null, Hoy));
    }

    /// <summary>Una toma de notas suelta no es una cadena: se queda como está.</summary>
    [Fact]
    public void UnaSolaNoSeToca()
    {
        var suelta = Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5));

        Assert.Empty(CadenaDelGrupo.Recolocar([suelta], null, Hoy));
    }

    // ---- lo escrito y lo dibujado ------------------------------------------

    /// <summary>
    /// <b>El calendario dibuja exactamente lo que quedó escrito.</b> Es el motivo de ser de
    /// toda esta clase: antes la cadena solo se dibujaba y el diálogo decía otra cosa.
    /// <para>
    /// Se le escapó una vez: al escribir las fechas ya encadenadas, el dibujo seguía
    /// colocando cada familia <b>en</b> el fin de la anterior en vez de al día siguiente, y
    /// se desviaba un día por familia. Este test lo habría cazado.
    /// </para>
    /// </summary>
    [Fact]
    public void LoQueSeEscribeEsLoQueSeDibuja()
    {
        var miembros = new[]
        {
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5)),
            Familia("ANTAR250402-00", Sep1.AddDays(19), Sep1.AddDays(29)),
            Familia("ANTAR250403-00")
        };

        foreach (var (proyecto, plan) in CadenaDelGrupo.Recolocar(miembros, null, Hoy))
        {
            proyecto.Planificacion.Inicio = plan.Inicio;
            proyecto.Planificacion.Fin = plan.Fin;
        }

        var entrada = Assert.Single(EnlaceDeTomasDeNotas.Agrupar(miembros));

        Assert.Equal(miembros.Length, entrada.Tramos.Count);

        foreach (var tramo in entrada.Tramos)
        {
            Assert.Equal(tramo.Miembro.Planificacion.Inicio, tramo.Desde);
            Assert.Equal(tramo.Miembro.Planificacion.Fin, tramo.Hasta);
        }
    }
}
