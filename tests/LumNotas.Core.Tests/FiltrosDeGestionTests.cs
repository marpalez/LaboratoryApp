using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Qué proyectos se ven. Es un solo juego de filtros para las cuatro vistas, así que un
/// error aquí no se equivoca en una pantalla: se equivoca en todas a la vez.
/// </summary>
public class FiltrosDeGestionTests
{
    private static ResumenDeProyecto Proyecto(
        string codigo, string tecnico = "Javier Ibor", string ip = "", string ik = "",
        string[]? acreditaciones = null, string[]? normas = null, bool archivado = false)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            CodigoServicio = codigo[..Math.Min(9, codigo.Length)],
            Tecnico = tecnico,
            GradoIp = ip,
            GradoIk = ik,
            Acreditaciones = acreditaciones ?? [],
            Normas = normas ?? ["60598-1_2024"],
            Planificacion = new Planificacion { Archivado = archivado }
        };

    private static readonly ResumenDeProyecto Antares =
        Proyecto("ANTAR250401-00", "Daniel Martínez", ip: "IP65", ik: "IK08", acreditaciones: ["ENAC"]);

    private static readonly ResumenDeProyecto SinAsignar =
        Proyecto("MOONO230401-00", tecnico: "", ip: "IP20");

    private static readonly ResumenDeProyecto Archivado =
        Proyecto("VIEJO200101-00", "Javier Ibor", ip: "IP65", archivado: true);

    private static readonly ResumenDeProyecto[] Todos = [Antares, SinAsignar, Archivado];

    private static IReadOnlyList<string> Ven(FiltrosDeGestion filtros)
        => [.. Todos.Where(filtros.Pasa).Select(p => p.CodigoTomaDeNotas)];

    /// <summary>
    /// Recién abierto se ve lo que está en marcha. Lo archivado no: es lo que hace que el
    /// tablero quepa en una pantalla después de años de servicios.
    /// </summary>
    [Fact]
    public void ReciénAbiertoSeVeLoQueEstaEnMarcha()
    {
        var visibles = Ven(new FiltrosDeGestion());

        Assert.Contains("ANTAR250401-00", visibles);
        Assert.Contains("MOONO230401-00", visibles);
        Assert.DoesNotContain("VIEJO200101-00", visibles);
    }

    /// <summary>
    /// <b>Y con «Cualquier estado» aparece lo archivado.</b> Es lo que hace buscable la
    /// BBDD desde que obedece al mismo estado que las otras tres vistas: sin esta opción,
    /// encontrar un servicio de hace tres años obligaba a ir probando estados de uno en
    /// uno hasta acertar.
    /// </summary>
    [Fact]
    public void ConCualquierEstadoApareceLoArchivado()
        => Assert.Contains("VIEJO200101-00", Ven(new FiltrosDeGestion { Estado = FiltroDeEstado.Cualquiera }));

    [Fact]
    public void ElGradoIpApartaLoQueNoLoTiene()
    {
        var visibles = Ven(new FiltrosDeGestion { Ip = "IP65" });

        Assert.Contains("ANTAR250401-00", visibles);
        Assert.DoesNotContain("MOONO230401-00", visibles);
    }

    [Fact]
    public void ElGradoIkYLaAcreditacionTambienFiltran()
    {
        Assert.Equal(["ANTAR250401-00"], Ven(new FiltrosDeGestion { Ik = "IK08" }));
        Assert.Equal(["ANTAR250401-00"], Ven(new FiltrosDeGestion { Acreditacion = "ENAC" }));
    }

    /// <summary>
    /// Los filtros se suman, no se sustituyen: un IP65 con el estado por defecto sigue sin
    /// enseñar el archivado, aunque también sea IP65.
    /// </summary>
    [Fact]
    public void LosFiltrosSeSuman()
        => Assert.Equal(["ANTAR250401-00"], Ven(new FiltrosDeGestion { Ip = "IP65" }));

    /// <summary>
    /// «(sin técnico)» enseña justo lo que falta por repartir, que es para lo que se mira
    /// el tablero un lunes.
    /// </summary>
    [Fact]
    public void SinTecnicoEnseñaLoQueFaltaPorRepartir()
        => Assert.Equal(["MOONO230401-00"],
            Ven(new FiltrosDeGestion { Tecnico = CargaPorTecnico.SinTecnico }));

    [Fact]
    public void ElTecnicoNoDistingueMayusculas()
        => Assert.Equal(["ANTAR250401-00"], Ven(new FiltrosDeGestion { Tecnico = "daniel martínez" }));

    [Fact]
    public void LaBusquedaSeSumaALosFiltros()
    {
        Assert.Equal(["ANTAR250401-00"], Ven(new FiltrosDeGestion { Texto = "antar" }));

        // Existe, pero está archivado: con el estado por defecto no sale.
        Assert.Empty(Ven(new FiltrosDeGestion { Texto = "viejo" }));
        Assert.Equal(["VIEJO200101-00"],
            Ven(new FiltrosDeGestion { Texto = "viejo", Estado = FiltroDeEstado.Cualquiera }));
    }

    [Fact]
    public void LaNormaFiltra()
    {
        Assert.Equal(3, Ven(new FiltrosDeGestion
        {
            Norma = "60598-1_2024",
            Estado = FiltroDeEstado.Cualquiera
        }).Count);

        Assert.Empty(Ven(new FiltrosDeGestion { Norma = "62031_2020_A11" }));
    }

    /// <summary>
    /// «Cualquier estado» no aparta nada, así que el botón no debe señalarlo como filtro
    /// activo: el número existe para avisar de lo que esconde trabajo, y esto lo enseña
    /// todo.
    /// </summary>
    [Fact]
    public void CualquierEstadoNoCuentaComoFiltroActivo()
        => Assert.Equal(0, ResumenDeFiltros.Cuantos(
            new FiltrosDeGestion { Estado = FiltroDeEstado.Cualquiera }));
}
