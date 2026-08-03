using LumNotas.Core.Datos;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// El fichero de proyecto vive en OneDrive (DD-02), así que lo que importa es que
/// el ciclo guardar/cargar no pierda nada y que la escritura sea atómica.
/// </summary>
public class PersistenciaTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    private string Ruta => Path.Combine(_carpeta, "123452026" + RepositorioDeProyectos.Extension);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    private static DatosProyecto ProyectoDeEjemplo()
    {
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 3, Clase = Clase.II };
        datos.IpSegundaCifra.Add("IPX4");
        datos.IpPrimeraCifra.Add("IP6X");
        datos.Partes2.Add("-2-3");

        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20, 9, 30, 0));
        datos.Establecer("generales", "tamano[0].alto", 42.5, 1);
        datos.Establecer("7.12.1", "tornillos[0].parte", "Carcasa superior", 2);
        datos.Establecer("15.2.1", "ensayoInicio", new DateTime(2026, 7, 20, 23, 40, 0), 1);
        datos.EstablecerNa("11.3/na", true);
        datos.Marcar("15.2.1", "origenMuestra", "corteEbp");
        return datos;
    }

    /// <summary>
    /// Con qué norma nació el proyecto es un dato suyo y se guarda. Antes había que
    /// reconstruirlo del patrón de muestras cada vez que se leía el fichero.
    /// </summary>
    [Fact]
    public void LaNormaPrincipalSeGuardaConElProyecto()
    {
        var datos = ProyectoDeEjemplo();
        Contexto.Norma("62031").AplicarA(datos);
        Contexto.Plantilla.AplicarA(datos, principal: false);

        _repositorio.Guardar(datos, Ruta, "1.0.0-mvp");
        var leido = _repositorio.Cargar(Ruta);

        Assert.Equal(Contexto.Norma("62031").Meta.Id, leido.NormaPrincipal);
        Assert.Contains(Contexto.Plantilla.Meta.Id, leido.Normas);
    }

    /// <summary>
    /// Un proyecto guardado antes de que existiera el campo se abre igual: se queda sin
    /// principal apuntada y el tablero la deduce, como hacía siempre. <b>No hay
    /// migración</b>: el fichero se cura cuando se guarde.
    /// </summary>
    [Fact]
    public void UnProyectoSinNormaPrincipalSeAbreIgual()
    {
        _repositorio.Guardar(ProyectoDeEjemplo(), Ruta, "1.0.0-mvp");

        // Se le quita el campo, dejando el fichero como lo escribía la versión anterior.
        var json = File.ReadAllText(Ruta);
        Assert.DoesNotContain("\"normaPrincipal\"", json);

        var leido = _repositorio.Cargar(Ruta);
        Assert.Null(leido.NormaPrincipal);
        Assert.Equal("123452026", leido.CodigoServicio);
    }

    [Fact]
    public void GuardarYCargar_ConservaTodosLosDatos()
    {
        var original = ProyectoDeEjemplo();
        _repositorio.Guardar(original, Ruta, "1.0.0-mvp");

        var leido = _repositorio.Cargar(Ruta);

        Assert.Equal("123452026", leido.CodigoServicio);
        Assert.Equal(3, leido.NumeroMuestras);
        Assert.Equal(Clase.II, leido.Clase);
        Assert.Contains("IPX4", leido.IpSegundaCifra);
        Assert.Contains("IP6X", leido.IpPrimeraCifra);
        Assert.Contains("-2-3", leido.Partes2);

        Assert.Equal(new DateTime(2026, 7, 20, 9, 30, 0), leido.Instante("6", "ambiente.fecha"));
        Assert.Equal(42.5, leido.Numero("generales", "tamano[0].alto", 1));
        Assert.Equal("Carcasa superior", leido.Obtener("7.12.1", "tornillos[0].parte", 2));
        Assert.Equal(new DateTime(2026, 7, 20, 23, 40, 0), leido.Instante("15.2.1", "ensayoInicio", 1));
        Assert.True(leido.Na("11.3/na"));
        Assert.True(leido.Marcada("15.2.1", "origenMuestra", "corteEbp"));
    }

    [Fact]
    public void ElProyectoLeidoProduceLasMismasReglasQueElOriginal()
    {
        var original = ProyectoDeEjemplo();
        _repositorio.Guardar(original, Ruta, "1.0.0-mvp");
        var leido = _repositorio.Cargar(Ruta);

        var antes = new LumNotas.Core.Motor.IndicadorDeAvance(Contexto.Motor(original)).Calcular();
        var despues = new LumNotas.Core.Motor.IndicadorDeAvance(Contexto.Motor(leido)).Calcular();

        Assert.Equal(antes.PesoTotal, despues.PesoTotal);
        Assert.Equal(antes.PesoEjecutado, despues.PesoEjecutado);
        Assert.Equal(antes.Contador, despues.Contador);
    }

    [Fact]
    public void GuardarDosVeces_ReemplazaSinDejarTemporales()
    {
        var datos = ProyectoDeEjemplo();
        _repositorio.Guardar(datos, Ruta, "1.0.0-mvp");

        datos.Establecer("generales", "tamano[0].alto", 99.0, 1);
        _repositorio.Guardar(datos, Ruta, "1.0.0-mvp");

        Assert.Equal(99.0, _repositorio.Cargar(Ruta).Numero("generales", "tamano[0].alto", 1));
        Assert.Single(Directory.GetFiles(_carpeta));   // sin restos de la escritura atómica
    }

    [Fact]
    public void ElFicheroEsLegibleSinLaAplicacion()
    {
        _repositorio.Guardar(ProyectoDeEjemplo(), Ruta, "1.0.0-mvp");
        var texto = File.ReadAllText(Ruta);

        // JSON con sangrado y sin escapes ilegibles: recuperable a mano si algo va mal.
        Assert.Contains("\"codigoServicio\": \"123452026\"", texto);
        Assert.Contains("Carcasa superior", texto);
        Assert.Contains("\"versionPlantilla\": \"1.0.0-mvp\"", texto);
    }
}
