using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>
/// Cuándo se ensayó de verdad, y el filtro por periodo que sale de ahí. Es lo que hace
/// contestable «¿qué se hizo en el primer trimestre?» sin que el técnico teclee nada.
/// </summary>
public class FechasDelEnsayoTests
{
    [Fact]
    public void UnaTomaDeNotasSinFechasNoTieneNinguna()
    {
        var (desde, hasta) = FechasDelEnsayo.De(Contexto.ProyectoVacio());

        Assert.Null(desde);
        Assert.Null(hasta);
    }

    /// <summary>
    /// La primera y la última de todo lo escrito, esté donde esté: las condiciones de un
    /// apartado, el inicio de una estufa o el fin de un ensayo de humedad.
    /// </summary>
    [Fact]
    public void CogeLaMasTempranaYLaMasTardia()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 3, 10));
        datos.Establecer("7.9", "humedadInicio", new DateTime(2026, 1, 8, 9, 30, 0), 1);
        datos.Establecer("7.9", "humedadFin", new DateTime(2026, 5, 22), 1);

        var (desde, hasta) = FechasDelEnsayo.De(datos);

        Assert.Equal(new DateTime(2026, 1, 8), desde);
        Assert.Equal(new DateTime(2026, 5, 22), hasta);
    }

    /// <summary>
    /// <b>Los textos no cuentan.</b> En la toma de notas hay campos libres, y dar por buena
    /// cualquier cosa con pinta de fecha metería códigos de muestra en la cuenta.
    /// </summary>
    [Fact]
    public void LoQueNoEsUnaFechaNoCuenta()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("6", "comentarios", "05/08/2026");
        datos.Establecer("6", "muestra", 20260805d);

        var (desde, _) = FechasDelEnsayo.De(datos);

        Assert.Null(desde);
    }

    // ---- el filtro por periodo ---------------------------------------------

    private static ResumenDeProyecto Ensayado(string codigo, DateTime? desde, DateTime? hasta)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            CodigoServicio = codigo[..Math.Min(9, codigo.Length)],
            Tecnico = "Javier Ibor",
            Normas = ["60598-1_2024"],
            Planificacion = new Planificacion
            {
                Estado = EstadoDeProyecto.Terminado,
                EnsayoDesde = desde,
                EnsayoHasta = hasta
            }
        };

    private static bool Pasa(ResumenDeProyecto proyecto, DateTime? desde, DateTime? hasta)
        => new FiltrosDeGestion
        {
            Estado = FiltroDeEstado.Cualquiera,
            Desde = desde,
            Hasta = hasta
        }.Pasa(proyecto);

    /// <summary>
    /// Entra lo que <b>se solapa</b> con el periodo, no solo lo que cabe entero dentro: un
    /// ensayo de enero a marzo tiene que salir al preguntar por febrero.
    /// </summary>
    [Fact]
    public void EntraLoQueSeSolapaConElPeriodo()
    {
        var largo = Ensayado("ANTAR250401-00", new DateTime(2026, 1, 10), new DateTime(2026, 3, 20));

        Assert.True(Pasa(largo, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28)));
        Assert.True(Pasa(largo, new DateTime(2026, 3, 20), new DateTime(2026, 6, 1)));
        Assert.False(Pasa(largo, new DateTime(2026, 4, 1), new DateTime(2026, 6, 1)));
        Assert.False(Pasa(largo, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31)));
    }

    [Fact]
    public void SoloUnaDeLasDosFechasTambienFiltra()
    {
        var enMarzo = Ensayado("ANTAR250401-00", new DateTime(2026, 3, 10), new DateTime(2026, 3, 12));

        Assert.True(Pasa(enMarzo, new DateTime(2026, 1, 1), null));
        Assert.False(Pasa(enMarzo, new DateTime(2026, 6, 1), null));
        Assert.True(Pasa(enMarzo, null, new DateTime(2026, 12, 31)));
        Assert.False(Pasa(enMarzo, null, new DateTime(2026, 1, 31)));
    }

    /// <summary>
    /// Un servicio sin esas fechas no sale al preguntar por un periodo: no está terminado,
    /// así que todavía no hay un «cuándo se hizo» que comparar.
    /// </summary>
    [Fact]
    public void LoQueNoEstaTerminadoNoSaleEnUnaConsultaPorPeriodo()
    {
        var enCurso = Ensayado("MOONO230401-00", null, null);

        Assert.False(Pasa(enCurso, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));
        Assert.True(Pasa(enCurso, null, null));
    }

    /// <summary>El periodo cuenta como un filtro solo, aunque se pongan las dos fechas.</summary>
    [Fact]
    public void ElPeriodoCuentaComoUnSoloFiltro()
    {
        var filtros = new FiltrosDeGestion
        {
            Desde = new DateTime(2026, 1, 1),
            Hasta = new DateTime(2026, 3, 31)
        };

        Assert.Equal(1, ResumenDeFiltros.Cuantos(filtros));
        Assert.Contains("Ensayado", ResumenDeFiltros.Detalle(filtros));
    }
}
