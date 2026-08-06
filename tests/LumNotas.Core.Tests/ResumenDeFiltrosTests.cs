using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Qué delata el botón de filtros. Desde que viven dentro de un diálogo, esto es lo único
/// que hay en la barra para saber que se está filtrando.
/// </summary>
public class ResumenDeFiltrosTests
{
    private const string Todos = ResumenDeFiltros.Cualquiera;

    /// <summary>Lo que hay puesto al abrir el programa.</summary>
    private static readonly FiltrosDeGestion ReciénAbierto = new();

    /// <summary>
    /// Al abrir el programa no hay nada que señalar: el botón dice «Filtros» a secas. Si
    /// contase «En desarrollo» —que es lo puesto por defecto— el aviso estaría encendido
    /// siempre y dejaría de mirarse, que es como no tenerlo.
    /// </summary>
    [Fact]
    public void ReciénAbiertoElBotonNoSeñalaNada()
    {
        Assert.Equal(0, ResumenDeFiltros.Cuantos(ReciénAbierto));
        Assert.Equal("Filtros", ResumenDeFiltros.Rotulo(ReciénAbierto));
    }

    /// <summary>
    /// <b>Lo que este rótulo existe para evitar.</b> Con un técnico elegido la semana
    /// pasada y los filtros escondidos, quien no vea su servicio en el tablero pensará que
    /// se ha perdido. El botón tiene que decir que está apartando trabajo sin abrirlo.
    /// </summary>
    [Fact]
    public void UnTecnicoElegidoSeVeSinAbrirLosFiltros()
    {
        var filtros = ReciénAbierto with { Tecnico = "Daniel Martínez" };

        Assert.Equal("Filtros (1)", ResumenDeFiltros.Rotulo(filtros));
        Assert.True(ResumenDeFiltros.Cuantos(filtros) > 0);
    }

    [Fact]
    public void SeCuentanLosSeis()
        => Assert.Equal("Filtros (6)", ResumenDeFiltros.Rotulo(new FiltrosDeGestion
        {
            Estado = "Archivados",
            Tecnico = "Javier Ibor",
            Norma = "60598-1_2024",
            Ip = "IP65",
            Ik = "IK08",
            Acreditacion = "ENAC"
        }));

    /// <summary>
    /// Los tres que vinieron de la BBDD cuentan igual que los otros: si el botón no los
    /// delatara, un grado IP elegido ayer dejaría el tablero medio vacío sin explicación.
    /// </summary>
    [Theory]
    [InlineData("IP65", null, null)]
    [InlineData(null, "IK08", null)]
    [InlineData(null, null, "ENAC")]
    public void LosTresQueVinieronDeLaBbddTambienSeSeñalan(string? ip, string? ik, string? acreditacion)
    {
        var filtros = ReciénAbierto with
        {
            Ip = ip ?? Todos,
            Ik = ik ?? Todos,
            Acreditacion = acreditacion ?? Todos
        };

        Assert.Equal("Filtros (1)", ResumenDeFiltros.Rotulo(filtros));
    }

    /// <summary>El nombre antiguo de «En desarrollo» significa lo mismo y tampoco cuenta.</summary>
    [Fact]
    public void ElNombreAntiguoDelEstadoTampocoCuenta()
        => Assert.Equal(0, ResumenDeFiltros.Cuantos(ReciénAbierto with { Estado = FiltroDeEstado.Todos }));

    // ---- el detalle --------------------------------------------------------

    /// <summary>
    /// El estado se nombra siempre, aunque sea el de por defecto: «En desarrollo» tampoco
    /// lo enseña todo —deja fuera lo terminado y lo archivado— y quien no lo sepa echará
    /// algo en falta sin saber por qué. Ahora manda también en la BBDD, así que es la
    /// explicación de por qué un servicio antiguo no aparece al buscarlo.
    /// </summary>
    [Fact]
    public void ElDetalleNombraSiempreElEstado()
        => Assert.Contains("En desarrollo", ResumenDeFiltros.Detalle(ReciénAbierto));

    [Fact]
    public void ElDetalleNoNombraLoQueNoFiltra()
    {
        var detalle = ResumenDeFiltros.Detalle(ReciénAbierto);

        Assert.DoesNotContain("Técnico", detalle);
        Assert.DoesNotContain("Norma", detalle);
        Assert.DoesNotContain("IP", detalle);
        Assert.DoesNotContain(Todos, detalle);
    }

    [Fact]
    public void ElDetalleNombraLoQueSiFiltra()
    {
        var detalle = ResumenDeFiltros.Detalle(new FiltrosDeGestion
        {
            Estado = "Archivados",
            Tecnico = "Javier Ibor",
            Norma = "60598-1_2024",
            Ip = "IP65",
            Acreditacion = "ENAC",
            Texto = "antar"
        });

        Assert.Contains("Archivados", detalle);
        Assert.Contains("Javier Ibor", detalle);
        Assert.Contains("60598-1_2024", detalle);
        Assert.Contains("IP65", detalle);
        Assert.Contains("ENAC", detalle);
        Assert.Contains("antar", detalle);
    }

    /// <summary>
    /// La búsqueda sale en el detalle pero <b>no cuenta</b> en el rótulo: la caja está a la
    /// vista en la barra, y el número avisa de lo que aparta trabajo sin verse.
    /// </summary>
    [Fact]
    public void LaBusquedaSeCuentaEnElDetallePeroNoEnElNumero()
    {
        var filtros = ReciénAbierto with { Texto = "antar" };

        Assert.Equal("Filtros", ResumenDeFiltros.Rotulo(filtros));
        Assert.Contains("antar", ResumenDeFiltros.Detalle(filtros));
    }

    /// <summary>Sin nada puesto no revienta: se comporta como recién abierto.</summary>
    [Fact]
    public void SinValoresSeComportaComoReciénAbierto()
    {
        var vacios = new FiltrosDeGestion
        {
            Estado = "", Tecnico = "", Norma = "", Ip = "", Ik = "", Acreditacion = "", Texto = ""
        };

        Assert.Equal(0, ResumenDeFiltros.Cuantos(vacios));
        Assert.Contains(FiltroDeEstado.EnDesarrollo, ResumenDeFiltros.Detalle(vacios));
    }
}
