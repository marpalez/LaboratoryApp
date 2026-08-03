using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Tests;

/// <summary>
/// De dónde salen las normas.
/// <para>
/// Lo que se vigila aquí es que <b>mande la carpeta compartida</b>. Si cada equipo lleva
/// su copia, dos técnicos pueden rellenar versiones distintas de la misma norma sin
/// enterarse, y en un laboratorio acreditado eso es un problema, no una molestia.
/// </para>
/// </summary>
public class PlantillasCompartidasTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "lumnotas-plan-" + Guid.NewGuid().ToString("N"));
    private readonly string _proyectos;

    public PlantillasCompartidasTests()
    {
        _proyectos = Path.Combine(_raiz, "proyectos");
        Directory.CreateDirectory(_proyectos);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string CarpetaCompartida() => Path.Combine(_proyectos, PlantillasCompartidas.NombreDeCarpeta);

    /// <summary>
    /// Cuántos ficheros hay instalados en este equipo. Se cuenta, no se escribe: añadir
    /// una norma —o un año nuevo de una— es dejar caer un fichero, y estos tests no deben tener
    /// que enterarse.
    /// </summary>
    private static int Instaladas(string patron)
        => Directory.GetFiles(Contexto.CarpetaDePlantillas(), patron).Length;

    private void PublicarUnaNorma(string version = "1.0.0")
    {
        var destino = CarpetaCompartida();
        Directory.CreateDirectory(destino);
        File.WriteAllText(Path.Combine(destino, "plantilla-99999.v1.json"),
            $$"""{ "meta": { "id": "99999", "titulo": "Norma de prueba", "version": "{{version}}" } }""");
    }

    // ---- de dónde se lee ---------------------------------------------------

    [Fact]
    public void SiLaCarpetaCompartidaTieneNormasSonLasQueMandan()
    {
        PublicarUnaNorma();

        var origen = PlantillasCompartidas.Resolver(_proyectos);

        Assert.True(origen.EsCompartida);
        Assert.Equal(CarpetaCompartida(), origen.Carpeta);
        Assert.False(origen.HayAviso);
    }

    /// <summary>
    /// Sin normas publicadas todavía se sigue trabajando con las de este equipo, pero
    /// <b>diciéndolo</b>: trabajar con una versión distinta a la del compañero no puede
    /// pasar inadvertido.
    /// </summary>
    [Fact]
    public void SiNoEstanPublicadasSeAvisaDeQueSeUsanLasDeEsteEquipo()
    {
        var origen = PlantillasCompartidas.Resolver(_proyectos);

        Assert.False(origen.EsCompartida);
        Assert.True(origen.HayAviso);
        Assert.Contains("este equipo", origen.Aviso!, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Con OneDrive sin conexión el laboratorio tiene que poder seguir trabajando.</summary>
    [Fact]
    public void SiNoSeLlegaALaCarpetaCompartidaSeUsaElRespaldoLocal()
    {
        var origen = PlantillasCompartidas.Resolver(Path.Combine(_raiz, "no-existe"));

        Assert.False(origen.EsCompartida);
        Assert.True(origen.HayAviso);
        Assert.Equal(PlantillasCompartidas.LocalSiExiste(), origen.Carpeta);
    }

    [Fact]
    public void SinCarpetaDeProyectosSeUsaLaDeEsteEquipo()
    {
        foreach (var vacia in new[] { null, "", "   " })
        {
            var origen = PlantillasCompartidas.Resolver(vacia);

            Assert.False(origen.EsCompartida);
            Assert.Equal(PlantillasCompartidas.LocalSiExiste(), origen.Carpeta);
        }
    }

    /// <summary>Una carpeta compartida vacía no vale: no basta con que exista.</summary>
    [Fact]
    public void UnaCarpetaCompartidaSinNormasNoCuenta()
    {
        Directory.CreateDirectory(CarpetaCompartida());

        Assert.False(PlantillasCompartidas.Resolver(_proyectos).EsCompartida);
    }

    // ---- publicar ----------------------------------------------------------

    /// <summary>
    /// Las plantillas y sus catálogos de equipos viajan juntos: el catálogo se busca al
    /// lado de su plantilla, así que publicar solo una parte dejaría apartados sin equipos.
    /// </summary>
    [Fact]
    public void PublicarLlevaLasNormasYSusCatalogosDeEquipos()
    {
        var local = Contexto.CarpetaDePlantillas();

        var copiados = PlantillasCompartidas.Publicar(local, _proyectos);

        var publicadas = Directory.GetFiles(CarpetaCompartida(), "plantilla-*.json");
        var equipos = Directory.GetFiles(CarpetaCompartida(), "equipos-*.json");

        Assert.Equal(Instaladas("plantilla-*.json"), publicadas.Length);
        Assert.Equal(Instaladas("equipos-*.json"), equipos.Length);
        Assert.Equal(publicadas.Length + equipos.Length, copiados);
    }

    [Fact]
    public void TrasPublicarSeLeeDeLaCarpetaCompartida()
    {
        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);

        var origen = PlantillasCompartidas.Resolver(_proyectos);

        Assert.True(origen.EsCompartida);
        Assert.Equal(CatalogoDeNormas.Disponibles(Contexto.CarpetaDePlantillas()).Count,
                     CatalogoDeNormas.Disponibles(origen.Carpeta).Count);
    }

    // ---- lo que este equipo tiene y el laboratorio no ----------------------

    /// <summary>
    /// Desde que se publica la primera tanda, el programa lee de la carpeta compartida y
    /// <b>deja de mirar la local</b>. Añadir una norma aquí no producía ninguna señal: el
    /// fichero estaba, no aparecía en el programa y nada explicaba por qué.
    /// </summary>
    [Fact]
    public void UnaNormaNuevaEnEsteEquipoSeAvisaComoSinPublicar()
    {
        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);

        // Lo publicado es exactamente lo que hay aquí: nada pendiente.
        Assert.False(PlantillasCompartidas
            .Comparar(Contexto.CarpetaDePlantillas(), _proyectos).HayAlgo);

        // Llega una norma a este equipo y todavía no está publicada.
        File.Delete(Path.Combine(CarpetaCompartida(), "plantilla-60529_2018_1.0.0.json"));

        var pendientes = PlantillasCompartidas.Comparar(Contexto.CarpetaDePlantillas(), _proyectos);

        Assert.Equal(1, pendientes.Cuantas);
        Assert.Contains(pendientes.Nuevas, t => t.Contains("60529"));
        Assert.Empty(pendientes.MasNuevas);
    }

    /// <summary>
    /// Y si la de aquí es una corrección de una que ya está publicada, también hay que
    /// avisar: si no, este equipo trabajaría con una versión que los demás no tienen.
    /// </summary>
    [Fact]
    public void UnaVersionMasNuevaEnEsteEquipoTambienSeAvisa()
    {
        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);

        // Se rebaja la publicada, como si aquí se hubiera corregido la plantilla.
        var publicada = Path.Combine(CarpetaCompartida(), "plantilla-60529_2018_1.0.0.json");
        File.WriteAllText(publicada,
            File.ReadAllText(publicada).Replace("\"version\": \"1.0.0\"", "\"version\": \"0.9.0\""));

        var pendientes = PlantillasCompartidas.Comparar(Contexto.CarpetaDePlantillas(), _proyectos);

        Assert.Empty(pendientes.Nuevas);
        Assert.Contains(pendientes.MasNuevas, t => t.Contains("60529"));
    }

    /// <summary>Sin carpeta compartida no hay nada que comparar: se sigue en local.</summary>
    [Fact]
    public void SinCarpetaCompartidaNoSeAvisaDeNada()
    {
        Assert.False(PlantillasCompartidas.Comparar(Contexto.CarpetaDePlantillas(), null).HayAlgo);
        Assert.False(PlantillasCompartidas.Comparar(Contexto.CarpetaDePlantillas(), _proyectos).HayAlgo);
    }

    /// <summary>Publicar dos veces actualiza, no duplica ni falla.</summary>
    [Fact]
    public void PublicarSobreLoYaPublicadoLoSustituye()
    {
        PublicarUnaNorma("0.0.1");
        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);
        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);

        // Las del laboratorio más la de prueba, que no se toca por no venir de local.
        Assert.Equal(Instaladas("plantilla-*.json") + 1,
                     Directory.GetFiles(CarpetaCompartida(), "plantilla-*.json").Length);
    }

    /// <summary>
    /// Publicar es lo que hace que la carpeta compartida pase a mandar: es la migración
    /// entera, de «cada equipo con su copia» a «una sola versión para todos».
    /// </summary>
    [Fact]
    public void LaMigracionCompletaCambiaDeOrigen()
    {
        Assert.False(PlantillasCompartidas.Resolver(_proyectos).EsCompartida);

        PlantillasCompartidas.Publicar(Contexto.CarpetaDePlantillas(), _proyectos);

        Assert.True(PlantillasCompartidas.Resolver(_proyectos).EsCompartida);
    }

    // ---- aviso de versión del programa -------------------------------------

    [Fact]
    public void SinVersionPublicadaNoHayAviso()
    {
        Assert.Null(ControlDeVersion.Leer(_proyectos));
        Assert.False(ControlDeVersion.HayMasNueva("1.0.0", null));
    }

    [Fact]
    public void LaVersionPublicadaSobreviveAlGuardado()
    {
        ControlDeVersion.Publicar(_proyectos, "1.4.0", "Vista de carga", "dmartinez");

        var leida = ControlDeVersion.Leer(_proyectos);

        Assert.Equal("1.4.0", leida!.Version);
        Assert.Equal("Vista de carga", leida.Notas);
        Assert.Equal("dmartinez", leida.PublicadoPor);
    }

    /// <summary>Solo avisa cuando la publicada es realmente posterior.</summary>
    [Theory]
    [InlineData("1.0.0", "1.1.0", true)]
    [InlineData("1.0.0", "2.0.0", true)]
    [InlineData("1.10.0", "1.9.0", false)]     // 10 va después de 9, no antes
    [InlineData("1.2.0", "1.2.0", false)]
    [InlineData("1.3.0", "1.2.0", false)]
    public void ElAvisoSoloSaltaConUnaVersionPosterior(string enEjecucion, string publicada, bool avisa)
    {
        ControlDeVersion.Publicar(_proyectos, publicada, null, null);

        Assert.Equal(avisa, ControlDeVersion.HayMasNueva(enEjecucion, ControlDeVersion.Leer(_proyectos)));
    }

    /// <summary>
    /// Las versiones de compilación llevan cosas detrás («1.2.0+abc123»). Deben compararse
    /// igual, y ante cualquier duda es preferible no avisar a avisar en falso cada día.
    /// </summary>
    [Theory]
    [InlineData("1.2.0+abc123", "1.3.0", true)]
    [InlineData("1.2.0", "1.3.0-beta", true)]
    [InlineData("1.2.0", "esto no es una versión", false)]
    [InlineData("tampoco", "1.3.0", false)]
    public void LasVersionesConSufijoSeComparanIgual(string enEjecucion, string publicada, bool avisa)
        => Assert.Equal(avisa, ControlDeVersion.HayMasNueva(
            enEjecucion, new VersionPublicada { Version = publicada }));

    [Fact]
    public void UnFicheroDeVersionCorruptoNoImpideArrancar()
    {
        File.WriteAllText(Path.Combine(_proyectos, ControlDeVersion.NombreDeFichero), "{ roto");

        Assert.Null(ControlDeVersion.Leer(_proyectos));
    }

    [Fact]
    public void PublicarDosVecesDejaLaUltima()
    {
        ControlDeVersion.Publicar(_proyectos, "1.4.0", null, null);
        ControlDeVersion.Publicar(_proyectos, "1.5.0", null, null);

        Assert.Equal("1.5.0", ControlDeVersion.Leer(_proyectos)!.Version);
    }
}
