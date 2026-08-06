using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// El buscador del listado. Responde a «¿te acuerdas de aquel proyecto de Antares con
/// IP65?», que es justo lo que hoy se pregunta a un compañero de viva voz.
/// </summary>
public class BusquedaDeProyectosTests
{
    private static ResumenDeProyecto Proyecto(
        string codigo, string tecnico = "Javier Ibor", string ip = "", string ik = "",
        string[]? acreditaciones = null, string[]? colaboradores = null, string tecnico2 = "")
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            CodigoServicio = codigo.Length > 9 ? codigo[..9] : codigo,
            Tecnico = tecnico,
            Tecnico2 = tecnico2,
            GradoIp = ip,
            GradoIk = ik,
            Acreditaciones = acreditaciones ?? [],
            Colaboradores = colaboradores ?? []
        };

    private static readonly ResumenDeProyecto[] Todos =
    [
        Proyecto("ANTAR250401-00", "Daniel Martínez", ip: "IP65", ik: "IK08", acreditaciones: ["ENAC"]),
        Proyecto("MOONO230401-00", "Javier Ibor", ip: "IP20", acreditaciones: ["Sin acreditar"]),
        Proyecto("TECNO260201-00", "Javier Ibor", ip: "IP65", ik: "IK10",
                 acreditaciones: ["ENEC", "CB"], colaboradores: ["IMQ Italia"])
    ];

    // ---- la caja de búsqueda -----------------------------------------------

    [Fact]
    public void SinNadaPuestoSalenTodos()
        => Assert.Equal(3, BusquedaDeProyectos.Filtrar(Todos).Count);

    /// <summary>
    /// La parte de texto se usa suelta desde el tablero, el calendario y la carga, que
    /// buscan sobre lo que ya tienen filtrado. Tiene que mirar <b>lo mismo</b> que el
    /// listado: si allí se encuentra un proyecto por su colaborador y aquí no, escribir lo
    /// mismo en dos sitios daría resultados distintos sin que nada lo explique.
    /// </summary>
    [Theory]
    [InlineData("antar", "ANTAR250401-00")]
    [InlineData("Daniel", "ANTAR250401-00")]
    [InlineData("IMQ", "TECNO260201-00")]
    [InlineData("ENEC", "TECNO260201-00")]
    public void LaBusquedaSueltaMiraLoMismoQueElListado(string texto, string esperado)
    {
        var encontrados = Todos.Where(p => BusquedaDeProyectos.CoincideElTexto(p, texto)).ToList();

        Assert.Equal(esperado, Assert.Single(encontrados).CodigoTomaDeNotas);
    }

    /// <summary>
    /// Una caja vacía no aparta nada. Es el estado normal del tablero, así que si esto
    /// fallara la vista nacería vacía y parecería que se han perdido los proyectos.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UnaCajaVaciaNoApartaNada(string? texto)
        => Assert.All(Todos, p => Assert.True(BusquedaDeProyectos.CoincideElTexto(p, texto)));

    [Fact]
    public void LoQueNoEstaEnNingunProyectoNoDaNada()
        => Assert.DoesNotContain(Todos, p => BusquedaDeProyectos.CoincideElTexto(p, "zzzz"));

    /// <summary>
    /// Se busca por trozo y sin distinguir mayúsculas: nadie teclea el código entero ni se
    /// acuerda de cómo iba escrito.
    /// </summary>
    [Fact]
    public void SeBuscaPorTrozoYSinMayusculas()
    {
        var encontrado = Assert.Single(BusquedaDeProyectos.Filtrar(Todos, texto: "antar"));

        Assert.Equal("ANTAR250401-00", encontrado.CodigoTomaDeNotas);
    }

    /// <summary>
    /// <b>Se busca en todas las columnas.</b> Quien recuerda un proyecto no sabe por cuál
    /// lo recuerda: puede ser el técnico, la acreditación o el laboratorio de fuera.
    /// </summary>
    [Theory]
    [InlineData("Daniel", "ANTAR250401-00")]
    [InlineData("IMQ", "TECNO260201-00")]
    [InlineData("ENAC", "ANTAR250401-00")]
    [InlineData("IK10", "TECNO260201-00")]
    public void SeBuscaEnTodasLasColumnas(string texto, string esperado)
        => Assert.Equal(esperado,
            Assert.Single(BusquedaDeProyectos.Filtrar(Todos, texto: texto)).CodigoTomaDeNotas);

    [Fact]
    public void LoQueNoEstaNoSale()
        => Assert.Empty(BusquedaDeProyectos.Filtrar(Todos, texto: "no existe esto"));

    // ---- los desplegables ---------------------------------------------------

    [Fact]
    public void ElFiltroDeIpDejaSoloLosDeEseGrado()
    {
        var encontrados = BusquedaDeProyectos.Filtrar(Todos, ip: "IP65");

        Assert.Equal(2, encontrados.Count);
        Assert.All(encontrados, p => Assert.Equal("IP65", p.GradoIp));
    }

    /// <summary>Un proyecto con varias acreditaciones sale al pedir cualquiera de ellas.</summary>
    [Fact]
    public void ElFiltroDeAcreditacionMiraTodasLasQueTenga()
    {
        Assert.Single(BusquedaDeProyectos.Filtrar(Todos, acreditacion: "ENEC"));
        Assert.Single(BusquedaDeProyectos.Filtrar(Todos, acreditacion: "CB"));
    }

    [Fact]
    public void LosFiltrosSeSuman()
    {
        Assert.Single(BusquedaDeProyectos.Filtrar(Todos, texto: "Javier", ip: "IP65"));
        Assert.Empty(BusquedaDeProyectos.Filtrar(Todos, texto: "Daniel", ip: "IP20"));
    }

    [Fact]
    public void TodosNoFiltraNada()
        => Assert.Equal(3, BusquedaDeProyectos
            .Filtrar(Todos, BusquedaDeProyectos.Cualquiera, BusquedaDeProyectos.Cualquiera,
                     BusquedaDeProyectos.Cualquiera, BusquedaDeProyectos.Cualquiera).Count);

    // ---- qué se ofrece en cada desplegable ---------------------------------

    /// <summary>
    /// Se ofrecen los valores que de verdad hay. Una lista fija ofrecería grados que nadie
    /// ha ensayado nunca y, lo que es peor, escondería los que sí.
    /// </summary>
    [Fact]
    public void SoloSeOfreceLoQueHayEnLosProyectos()
    {
        var opciones = BusquedaDeProyectos.Opciones(Todos.Select(p => p.GradoIp));

        Assert.Equal([BusquedaDeProyectos.Cualquiera, "IP20", "IP65"], opciones);
    }

    /// <summary>Lo vacío no es una opción: un servicio sin IK no ofrece un IK en blanco.</summary>
    [Fact]
    public void LoVacioNoSeOfrece()
    {
        var opciones = BusquedaDeProyectos.Opciones(Todos.Select(p => p.GradoIk));

        Assert.Equal([BusquedaDeProyectos.Cualquiera, "IK08", "IK10"], opciones);
    }
}
