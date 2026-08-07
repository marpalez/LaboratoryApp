using LumNotas.Core.Despliegue;

namespace LumNotas.Core.Tests;

/// <summary>
/// Cómo llega una versión nueva a los otros cinco equipos.
/// <para>
/// Se prueba entero porque es la pieza que <b>nadie va a mirar hasta que falle</b>: si el
/// reparto se lleva media versión, lo que se rompe no es una pantalla sino el programa
/// que abre el laboratorio por la mañana.
/// </para>
/// <para>
/// Lo que más se comprueba aquí es lo que pasa <b>cuando algo va mal</b>: OneDrive a
/// medias, la compartida sin llegar, una copia interrumpida. En todos esos casos la
/// respuesta correcta es la misma —arrancar con lo que ya había— y es la que hay que
/// vigilar, porque la de «todo bien» se ve a simple vista.
/// </para>
/// </summary>
public class RepartoDelProgramaTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "reparto-" + Guid.NewGuid().ToString("N"));

    private string Compartida => Path.Combine(_raiz, "compartida");
    private string Local => Path.Combine(_raiz, "local");

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Una instalación de mentira, con su subcarpeta de normas.</summary>
    private string Instalacion(string version, string contenido = "programa")
    {
        var carpeta = Path.Combine(_raiz, "origen-" + version);
        Directory.CreateDirectory(Path.Combine(carpeta, "plantilla"));

        File.WriteAllText(Path.Combine(carpeta, RepartoDelPrograma.NombreDelExe), contenido);
        File.WriteAllText(Path.Combine(carpeta, "LumNotas.Core.dll"), "nucleo");
        File.WriteAllText(Path.Combine(carpeta, "plantilla", "plantilla-60598.json"), "{}");
        File.WriteAllText(Path.Combine(carpeta, "LumenLab.pdb"), "simbolos");

        return carpeta;
    }

    private void Publicar(string version, string contenido = "programa")
        => RepartoDelPrograma.Publicar(Instalacion(version, contenido), Compartida, version, null, "yo");

    // ------------------------------------------------------------ publicar

    /// <summary>
    /// Lo que se publica es la instalación entera, con sus subcarpetas. Los símbolos de
    /// depuración no: duplican lo que hay que sincronizar y no los usa nadie.
    /// </summary>
    [Fact]
    public void SePublicaElProgramaEnteroMenosLosSimbolos()
    {
        Publicar("1.0.0");

        var carpeta = RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.0");

        Assert.True(File.Exists(Path.Combine(carpeta, RepartoDelPrograma.NombreDelExe)));
        Assert.True(File.Exists(Path.Combine(carpeta, "plantilla", "plantilla-60598.json")));
        Assert.False(File.Exists(Path.Combine(carpeta, "LumenLab.pdb")));

        var manifiesto = RepartoDelPrograma.LeerManifiesto(carpeta);
        Assert.NotNull(manifiesto);
        Assert.Equal("1.0.0", manifiesto.Version);
        Assert.Contains(manifiesto.Ficheros, f => f.Nombre == "plantilla/plantilla-60598.json");
        Assert.DoesNotContain(manifiesto.Ficheros, f => f.Nombre.EndsWith(".pdb"));
    }

    /// <summary>
    /// Publicar deja además el marcador, que es lo que hace que los demás se muevan. Y lo
    /// deja <b>al final</b>: antes de él, la carpeta ya está entera.
    /// </summary>
    [Fact]
    public void PublicarDejaElMarcadorYLaCarpetaCompleta()
    {
        Publicar("1.0.0");

        Assert.Equal("1.0.0", ControlDeVersion.Leer(Compartida)?.Version);
        Assert.True(RepartoDelPrograma.EstaCompleta(RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.0")));
    }

    /// <summary>
    /// Republicar el mismo número deja la carpeta como la de aquí, no una mezcla. Si
    /// sobrara un fichero de un intento anterior, el manifiesto no lo nombraría y los
    /// demás equipos se lo llevarían igual.
    /// </summary>
    [Fact]
    public void RepublicarNoDejaRestosDelIntentoAnterior()
    {
        Publicar("1.0.0");

        var carpeta = RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.0");
        File.WriteAllText(Path.Combine(carpeta, "sobra.dll"), "de un intento anterior");

        Publicar("1.0.0");

        Assert.False(File.Exists(Path.Combine(carpeta, "sobra.dll")));
    }

    // ------------------------------------------------------------ ponerse al día

    [Fact]
    public void UnEquipoSinNadaSeTraeLoPublicado()
    {
        Publicar("1.0.0");

        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.True(puesta.SeHaActualizado);
        Assert.Equal("1.0.0", puesta.Version);
        Assert.Null(puesta.Aviso);
        Assert.True(File.Exists(puesta.RutaDelExe));
        Assert.True(File.Exists(Path.Combine(Local, "1.0.0", "plantilla", "plantilla-60598.json")));
    }

    /// <summary>Al día no se copia nada: abrir el programa no puede costar una copia diaria.</summary>
    [Fact]
    public void SiYaEstaAlDiaNoSeCopiaNada()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        var segunda = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.False(segunda.SeHaActualizado);
        Assert.Equal("1.0.0", segunda.Version);
    }

    [Fact]
    public void UnaVersionNuevaSustituyeALaQueHabia()
    {
        Publicar("1.0.0", "viejo");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Publicar("1.0.1", "nuevo");
        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.True(puesta.SeHaActualizado);
        Assert.Equal("nuevo", File.ReadAllText(puesta.RutaDelExe!));
    }

    // ------------------------------------------------------------ cuando algo va mal

    /// <summary>
    /// <b>El caso que motivó el manifiesto.</b> OneDrive no sincroniza en orden, así que
    /// puede llegar el marcador diciendo «1.0.1» antes que la carpeta de la 1.0.1 entera.
    /// Ni se copia a medias ni se deja al técnico sin programa: se abre el de siempre.
    /// </summary>
    [Fact]
    public void UnRepartoAMediasNoSeInstalaYSeAbreElDeSiempre()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Publicar("1.0.1");

        // Como si OneDrive hubiera traído el marcador y el manifiesto, pero no el programa.
        File.Delete(Path.Combine(RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.1"),
                                 RepartoDelPrograma.NombreDelExe));

        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.False(puesta.SeHaActualizado);
        Assert.Equal("1.0.0", puesta.Version);
        Assert.Contains("sincroniz", puesta.Aviso);
        Assert.False(Directory.Exists(Path.Combine(Local, "1.0.1")));
    }

    /// <summary>Lo mismo con un fichero a medio bajar: existe, pero no ocupa lo que decía.</summary>
    [Fact]
    public void UnFicheroAMedioBajarTampocoPasa()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Publicar("1.0.1", "un programa entero y largo");
        File.WriteAllText(Path.Combine(RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.1"),
                                       RepartoDelPrograma.NombreDelExe), "un prog");

        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.False(puesta.SeHaActualizado);
        Assert.Equal("1.0.0", puesta.Version);
    }

    /// <summary>Sin manifiesto no hay lista de la compra, así que no hay forma de comprobar nada.</summary>
    [Fact]
    public void SinManifiestoNoSeInstala()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Publicar("1.0.1");
        File.Delete(Path.Combine(RepartoDelPrograma.CarpetaDeLaVersion(Compartida, "1.0.1"),
                                 RepartoDelPrograma.NombreDelManifiesto));

        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.False(puesta.SeHaActualizado);
        Assert.Equal("1.0.0", puesta.Version);
    }

    /// <summary>OneDrive sin conexión, o el portátil fuera del laboratorio.</summary>
    [Fact]
    public void SinCarpetaCompartidaSeAbreLoQueHaya()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        var puesta = RepartoDelPrograma.PonerAlDia(null, Local);

        Assert.Equal("1.0.0", puesta.Version);
        Assert.Contains("compartida", puesta.Aviso);
    }

    /// <summary>Un equipo recién instalado y sin compartida: no hay nada que hacer, y se dice.</summary>
    [Fact]
    public void SinNadaEnNingunLadoSeDiceQueNoHayNadaQueAbrir()
    {
        var puesta = RepartoDelPrograma.PonerAlDia(null, Local);

        Assert.Null(puesta.RutaDelExe);
        Assert.NotNull(puesta.Aviso);
    }

    /// <summary>
    /// Una copia local interrumpida —un apagón a mitad— no puede darse por buena la
    /// próxima vez. Por eso se copia a un nombre temporal y solo al final se renombra.
    /// </summary>
    [Fact]
    public void UnaCopiaLocalAMediasNoCuentaComoInstalada()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        File.Delete(Path.Combine(Local, "1.0.0", "LumNotas.Core.dll"));

        Assert.Null(RepartoDelPrograma.UltimaInstalada(Local));
    }

    // ------------------------------------------------------------ volver atrás

    /// <summary>
    /// Se guardan las dos últimas a los dos lados. Sin la anterior, volver atrás no
    /// tendría a dónde volver.
    /// </summary>
    [Fact]
    public void SeConservanLasDosUltimasYSeVaLaTercera()
    {
        Publicar("1.0.0");
        Publicar("1.0.1");
        Publicar("1.0.2");

        var programas = Path.Combine(Compartida, RepartoDelPrograma.CarpetaDeProgramas);

        Assert.True(Directory.Exists(Path.Combine(programas, "1.0.2")));
        Assert.True(Directory.Exists(Path.Combine(programas, "1.0.1")));
        Assert.False(Directory.Exists(Path.Combine(programas, "1.0.0")));
    }

    /// <summary>
    /// Volver atrás es reescribir el marcador con el número anterior. El equipo se lleva
    /// la vieja sin que nadie toque nada más — que es lo que se quiere el día que una
    /// versión sale mala.
    /// </summary>
    [Fact]
    public void VolverAtrasEsReescribirElMarcador()
    {
        Publicar("1.0.0", "buena");
        Publicar("1.0.1", "mala");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        ControlDeVersion.Publicar(Compartida, "1.0.0", "la 1.0.1 rompía el informe", "yo");
        var puesta = RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.Equal("1.0.0", puesta.Version);
        Assert.Equal("buena", File.ReadAllText(puesta.RutaDelExe!));
    }

    /// <summary>
    /// Y la copia local también conserva dos, para que volver atrás no dependa de que
    /// OneDrive esté disponible ese día.
    /// </summary>
    [Fact]
    public void ElEquipoTambienConservaLaAnterior()
    {
        Publicar("1.0.0");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);
        Publicar("1.0.1");
        RepartoDelPrograma.PonerAlDia(Compartida, Local);

        Assert.True(Directory.Exists(Path.Combine(Local, "1.0.0")));
        Assert.True(Directory.Exists(Path.Combine(Local, "1.0.1")));
    }
}
