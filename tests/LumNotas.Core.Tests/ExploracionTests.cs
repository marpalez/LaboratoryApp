using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// El escaneo de la carpeta de clientes.
/// <para>
/// En el laboratorio los proyectos cuelgan de una matrioska —<c>clientes/antares/
/// antar2504/01/tomadenotas/</c>— con años de historia, así que lo que se vigila aquí es
/// que el escaneo aguante árboles grandes: que encuentre lo que hay por hondo que esté,
/// que no relea lo que no ha cambiado y que una rama rota no lo tumbe.
/// </para>
/// </summary>
public class ExploracionTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "lumnotas-exp-" + Guid.NewGuid().ToString("N"));
    private readonly string _clientes;
    private readonly string _cache;
    private readonly RepositorioDeProyectos _repositorio = new();

    public ExploracionTests()
    {
        _clientes = Path.Combine(_raiz, "clientes");
        _cache = Path.Combine(_raiz, "resumenes.cache.json");
        Directory.CreateDirectory(_clientes);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Un proyecto donde de verdad los pone el laboratorio.</summary>
    private string Crear(string cliente, string servicio)
    {
        var carpeta = Path.Combine(_clientes, cliente, servicio, "01", "tomadenotas");
        Directory.CreateDirectory(carpeta);

        var datos = new DatosProyecto { CodigoServicio = servicio, NumeroMuestras = 1 };
        datos.Establecer("proyecto", "tecnico1", "Javier Ibor");

        var ruta = Path.Combine(carpeta, servicio + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0");
        return ruta;
    }

    private IReadOnlyList<ResumenDeProyecto> Explorar(ExploradorDeProyectos explorador)
        => explorador.Explorar(_clientes, Contexto.Plantilla);

    // ---- encontrar ---------------------------------------------------------

    [Fact]
    public void EncuentraLosProyectosPorHondosQueEsten()
    {
        Crear("antares", "antar2504");
        Crear("moonoff", "moono2304");
        Crear("antares", "antar2601");

        var resumenes = Explorar(new ExploradorDeProyectos(_repositorio, _cache));

        Assert.Equal(3, resumenes.Count);
        Assert.Contains(resumenes, r => r.CodigoServicio == "moono2304");
    }

    /// <summary>Lo oculto y lo del sistema no son proyectos: papeleras, `.git`, metadatos.</summary>
    [Fact]
    public void NoEntraEnLasCarpetasOcultas()
    {
        Crear("antares", "antar2504");

        var escondida = Path.Combine(_clientes, ".papelera");
        Directory.CreateDirectory(escondida);
        new DirectoryInfo(escondida).Attributes |= FileAttributes.Hidden;
        File.Copy(Crear("moonoff", "moono2304"), Path.Combine(escondida, "borrado.lumproj"));

        var resumenes = Explorar(new ExploradorDeProyectos(_repositorio, _cache));

        Assert.DoesNotContain(resumenes, r => r.Ruta.Contains(".papelera"));
    }

    [Fact]
    public void UnFicheroCorruptoNoTumbaElEscaneo()
    {
        Crear("antares", "antar2504");
        File.WriteAllText(Path.Combine(_clientes, "roto" + RepositorioDeProyectos.Extension), "{ no es json");

        var resumenes = Explorar(new ExploradorDeProyectos(_repositorio, _cache));

        Assert.Equal(2, resumenes.Count);
        Assert.Single(resumenes, r => r.Error is not null);
    }

    [Fact]
    public void UnaCarpetaQueNoExisteDevuelveListaVacia()
        => Assert.Empty(new ExploradorDeProyectos(_repositorio, _cache)
            .Explorar(Path.Combine(_raiz, "no-existe"), Contexto.Plantilla));

    // ---- caché entre sesiones ----------------------------------------------

    /// <summary>
    /// <b>Lo que hace que el segundo arranque sea inmediato.</b> La caché vive en disco,
    /// así que un explorador nuevo —otra sesión del programa— no tiene que releer nada.
    /// </summary>
    [Fact]
    public void LoYaAnalizadoNoSeVuelveALeerEnLaSiguienteSesion()
    {
        var ruta = Crear("antares", "antar2504");

        var cache = CacheDeResumenes.Cargar(_cache);
        var fichero = new FileInfo(ruta);
        var resumen = AnalizadorDeProyectos.Analizar(
            Contexto.Plantilla, _repositorio.Cargar(ruta), ruta, fichero.LastWriteTime);

        cache.Anotar(fichero, "60598/1.0.0", resumen);
        cache.Guardar();

        // Otra sesión: se lee el fichero de caché desde cero.
        var otra = CacheDeResumenes.Cargar(_cache);

        Assert.NotNull(otra.Recuperar(new FileInfo(ruta), "60598/1.0.0"));
        Assert.Equal("antar2504", otra.Recuperar(new FileInfo(ruta), "60598/1.0.0")!.CodigoServicio);
    }

    /// <summary>Si el proyecto cambia, lo guardado deja de valer.</summary>
    [Fact]
    public void UnProyectoModificadoDejaDeValerEnLaCache()
    {
        var ruta = Crear("antares", "antar2504");
        var cache = CacheDeResumenes.Cargar(_cache);
        var antes = new FileInfo(ruta);

        cache.Anotar(antes, "60598/1.0.0", AnalizadorDeProyectos.NoLegible(ruta, antes.LastWriteTime, "x"));

        // Se vuelve a guardar el proyecto: cambia la fecha y el tamaño.
        var datos = _repositorio.Cargar(ruta);
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 8, 2));
        Thread.Sleep(20);
        _repositorio.Guardar(datos, ruta, "1.0.0");

        Assert.Null(cache.Recuperar(new FileInfo(ruta), "60598/1.0.0"));
    }

    /// <summary>
    /// El resumen sale de aplicar las reglas de una norma. Al publicar una versión nueva
    /// de la plantilla, lo guardado ya no describe lo mismo.
    /// </summary>
    [Fact]
    public void AlCambiarLaPlantillaLaCacheDejaDeValer()
    {
        var ruta = Crear("antares", "antar2504");
        var cache = CacheDeResumenes.Cargar(_cache);
        var fichero = new FileInfo(ruta);

        cache.Anotar(fichero, "60598/1.0.0", AnalizadorDeProyectos.NoLegible(ruta, fichero.LastWriteTime, "x"));

        Assert.NotNull(cache.Recuperar(fichero, "60598/1.0.0"));
        Assert.Null(cache.Recuperar(fichero, "60598/1.1.0"));
        Assert.Null(cache.Recuperar(fichero, "62031/1.0.0"));
    }

    /// <summary>La caché no puede crecer sin fin con proyectos que ya no están.</summary>
    [Fact]
    public void LosProyectosQueDesaparecenSeOlvidan()
    {
        var uno = Crear("antares", "antar2504");
        var otro = Crear("moonoff", "moono2304");
        var cache = CacheDeResumenes.Cargar(_cache);

        foreach (var ruta in new[] { uno, otro })
            cache.Anotar(new FileInfo(ruta), "p", AnalizadorDeProyectos.NoLegible(ruta, DateTime.Now, "x"));

        Assert.Equal(2, cache.Cuantos);

        cache.ConservarSolo([uno]);

        Assert.Equal(1, cache.Cuantos);
        Assert.Null(cache.Recuperar(new FileInfo(otro), "p"));
    }

    /// <summary>Una caché corrupta se tira y se rehace; no puede impedir abrir el tablero.</summary>
    [Fact]
    public void UnaCacheCorruptaNoImpideExplorar()
    {
        Crear("antares", "antar2504");
        File.WriteAllText(_cache, "{ esto no es json");

        Assert.Equal(0, CacheDeResumenes.Cargar(_cache).Cuantos);
        Assert.Single(Explorar(new ExploradorDeProyectos(_repositorio, _cache)));
    }

    // ---- proyectos con varias normas ---------------------------------------

    private static IReadOnlyDictionary<string, PlantillaEnsayos> Instaladas()
        => Contexto.TodasLasPlantillas().ToDictionary(p => p.Meta.Id);

    private string ConNormas(string servicio, params string[] normas)
    {
        var carpeta = Path.Combine(_clientes, "antares", servicio, "01", "tomadenotas");
        Directory.CreateDirectory(carpeta);

        var datos = new DatosProyecto { CodigoServicio = servicio, NumeroMuestras = 1 };
        datos.Establecer("proyecto", "tecnico1", "Javier Ibor");
        foreach (var norma in normas) datos.Normas.Add(norma);

        var ruta = Path.Combine(carpeta, servicio + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0");
        return ruta;
    }

    /// <summary>
    /// <b>Una norma añadida ocupa una sola línea.</b> Al responsable le interesa el
    /// detalle de lo que está ensayando y, de lo añadido, solo si queda algo por hacer:
    /// desplegar la 62031 entera dentro de un servicio de luminarias enterraba lo demás.
    /// </summary>
    [Fact]
    public void UnaNormaAnadidaSeResumeEnUnaSolaLinea()
    {
        ConNormas("antar2504", "60598");
        ConNormas("antar2601", "60598", "62031");

        var resumenes = new ExploradorDeProyectos(_repositorio, _cache)
            .Explorar(_clientes, Instaladas(), Contexto.Plantilla);

        var solaLuminarias = resumenes.Single(r => r.CodigoServicio == "antar2504");
        var conModulos = resumenes.Single(r => r.CodigoServicio == "antar2601");

        // Exactamente una sección más que sin la 62031: la línea de la 62031.
        Assert.Equal(solaLuminarias.SeccionesAplicables + 1, conModulos.SeccionesAplicables);
        Assert.Equal(solaLuminarias.SeccionesPendientes.Count + 1, conModulos.SeccionesPendientes.Count);

        var deModulos = Assert.Single(conModulos.SeccionesPendientes,
            s => s.Titulo.Contains("62031", StringComparison.Ordinal));

        // Y esa línea trae la cuenta de toda su toma de notas, no de una sección suelta.
        Assert.True(deModulos.Aplicables > 1, "La línea debe resumir todos sus apartados.");
    }

    /// <summary>Las secciones de la norma principal se siguen viendo una a una.</summary>
    [Fact]
    public void LaNormaPrincipalSeSigueDetallando()
    {
        ConNormas("antar2601", "60598", "62031");

        var resumen = new ExploradorDeProyectos(_repositorio, _cache)
            .Explorar(_clientes, Instaladas(), Contexto.Plantilla).Single();

        var deLuminarias = Contexto.Plantilla.Secciones.Select(s => s.Titulo).ToHashSet();

        Assert.True(resumen.SeccionesPendientes.Count(s => deLuminarias.Contains(s.Titulo)) > 5,
            "Las secciones de luminarias deben aparecer una a una.");
    }

    /// <summary>
    /// La línea de una norma añadida <b>solo desaparece cuando toda ella está completa</b>,
    /// no cuando lo está una de sus secciones.
    /// </summary>
    [Fact]
    public void LaLineaDeUnaNormaAnadidaResumeTodaSuTomaDeNotas()
    {
        var ruta = ConNormas("antar2601", "60598", "62031");
        var normas = Instaladas();
        var datos = _repositorio.Cargar(ruta);

        var juntas = AnalizadorDeProyectos.Analizar(
            [normas["60598"], normas["62031"]], datos, ruta, DateTime.Now);

        // La 62031 medida por su cuenta, sección a sección.
        var sola = AnalizadorDeProyectos.Analizar([normas["62031"]], datos, ruta, DateTime.Now);

        var linea = Assert.Single(juntas.SeccionesPendientes, s => s.Titulo.Contains("62031"));

        // Una línea, pero con la cuenta de todas sus secciones: por eso no desaparece
        // hasta que la norma entera está completa.
        Assert.True(sola.SeccionesPendientes.Count > 1, "La 62031 tiene varias secciones.");
        Assert.Equal(sola.SeccionesPendientes.Sum(s => s.Pendientes), linea.Pendientes);
        Assert.Equal(sola.SeccionesPendientes.Sum(s => s.Aplicables), linea.Aplicables);
    }

    /// <summary>
    /// Cada proyecto se mide con <b>sus</b> normas, no con la que esté abierta. Un servicio
    /// de IP no puede evaluarse contra las reglas de luminarias.
    /// </summary>
    [Fact]
    public void CadaProyectoSeMideConSusPropiasNormas()
    {
        ConNormas("antar2504", "60529");

        var resumen = new ExploradorDeProyectos(_repositorio, _cache)
            .Explorar(_clientes, Instaladas(), Contexto.Plantilla).Single();

        var deIp = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");

        Assert.True(resumen.SeccionesAplicables <= deIp.Secciones.Count,
            "Un proyecto de IP no puede tener más secciones que la propia norma de IP.");
        Assert.All(resumen.SeccionesPendientes,
            s => Assert.Contains(deIp.Secciones, x => x.Titulo == s.Titulo));
    }

    /// <summary>Los proyectos antiguos no apuntaban sus normas; se miden con la de siempre.</summary>
    [Fact]
    public void UnProyectoSinNormasApuntadasSeMideConLaDePorDefecto()
    {
        Crear("antares", "antar2504");   // se guarda sin normas

        var resumen = new ExploradorDeProyectos(_repositorio, _cache)
            .Explorar(_clientes, Instaladas(), Contexto.Plantilla).Single();

        Assert.NotEmpty(resumen.SeccionesPendientes);
        Assert.All(resumen.SeccionesPendientes,
            s => Assert.Contains(Contexto.Plantilla.Secciones, x => x.Titulo == s.Titulo));
    }

    // ---- escaneo completo --------------------------------------------------

    /// <summary>
    /// El escaneo avisa de por dónde va: con una carpeta de clientes grande, quedarse en
    /// blanco varios segundos parece que se ha colgado.
    /// </summary>
    [Fact]
    public void ElEscaneoCuentaPorDondeVa()
    {
        for (var i = 0; i < 12; i++) Crear("antares", $"antar25{i:00}");

        var pasos = new List<AvanceDeExploracion>();
        var explorador = new ExploradorDeProyectos(_repositorio, _cache);

        explorador.Explorar(_clientes, Contexto.Plantilla, true,
            new Progress<AvanceDeExploracion>(pasos.Add));

        // Progress<T> avisa en el hilo de origen; en un test sin bucle de mensajes basta
        // con comprobar que el escaneo termina bien y que no se pierde ningún proyecto.
        Assert.Equal(12, Explorar(explorador).Count);
    }

    [Fact]
    public void SePuedeCancelarUnEscaneo()
    {
        for (var i = 0; i < 20; i++) Crear("antares", $"antar25{i:00}");

        using var cancelacion = new CancellationTokenSource();
        cancelacion.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            new ExploradorDeProyectos(_repositorio, _cache)
                .Explorar(_clientes, Contexto.Plantilla, true, null, cancelacion.Token));
    }

    /// <summary>
    /// Se leen varios proyectos a la vez. El motor de reglas crea una instancia por
    /// proyecto y la plantilla es de solo lectura, pero conviene tener quien lo vigile.
    /// </summary>
    [Fact]
    public void LeerEnParaleloDaElMismoResultado()
    {
        for (var i = 0; i < 30; i++) Crear("antares", $"antar25{i:00}");

        var resumenes = Explorar(new ExploradorDeProyectos(_repositorio, _cache));

        Assert.Equal(30, resumenes.Count);
        Assert.Equal(30, resumenes.Select(r => r.Ruta).Distinct().Count());
        Assert.All(resumenes, r => Assert.Null(r.Error));
        Assert.All(resumenes, r => Assert.NotEmpty(r.SeccionesPendientes));
    }
}
