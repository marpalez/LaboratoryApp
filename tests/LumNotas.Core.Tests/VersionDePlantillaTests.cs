using LumNotas.Core.Datos;
using LumNotas.Report;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Con qué versión de la plantilla se registró cada ensayo.
/// <para>
/// Importa por la trazabilidad: si el laboratorio publica una corrección de la plantilla,
/// el informe de un ensayo hecho antes tiene que seguir diciendo <b>con cuál se tomaron
/// las notas</b>, no con cuál se imprime. Decir la de hoy sería atribuirle al ensayo una
/// plantilla que no se usó.
/// </para>
/// </summary>
public class VersionDePlantillaTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-ver-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    private string Ruta => Path.Combine(_carpeta, "toma" + RepositorioDeProyectos.Extension);

    public VersionDePlantillaTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Se escribía desde el principio y <b>no se leía nunca</b>: estaba en el fichero para
    /// quien lo abriera con un editor. Ahora vuelve al proyecto al abrirlo.
    /// </summary>
    [Fact]
    public void LaVersionConLaQueSeGuardoSeRecuperaAlAbrir()
    {
        _repositorio.Guardar(Contexto.ProyectoVacio(), Ruta, "1.0.0");

        Assert.Equal("1.0.0", _repositorio.Cargar(Ruta).VersionDePlantillaGuardada);
    }

    /// <summary>Guardar la actualiza: exportar justo después no puede decir la anterior.</summary>
    [Fact]
    public void GuardarDejaElProyectoConLaVersionQueAcabaDeEscribir()
    {
        var datos = Contexto.ProyectoVacio();
        Assert.Null(datos.VersionDePlantillaGuardada);

        _repositorio.Guardar(datos, Ruta, "1.1.0");

        Assert.Equal("1.1.0", datos.VersionDePlantillaGuardada);
    }

    /// <summary>
    /// <b>El informe dice con qué se registró, no con qué se imprime.</b> Cuando no
    /// coinciden se dicen las dos, que es lo que necesita quien audite.
    /// </summary>
    [Fact]
    public void ElInformeDeclaraLaVersionConLaQueSeRegistro()
    {
        // Una anterior a la instalada: es el caso que se quiere ver en el informe.
        var datos = Contexto.ProyectoVacio();
        datos.VersionDePlantillaGuardada = "0.9.0";

        var html = new ExportadorDeInforme(Contexto.Plantilla).GenerarHtml(datos);

        Assert.Contains("0.9.0 (registrado)", html);
        Assert.Contains(Contexto.Plantilla.Meta.Version, html);
    }

    /// <summary>Y cuando coinciden, no se marea a nadie con dos números iguales.</summary>
    [Fact]
    public void SiCoincidenElInformeDiceUnaSolaVersion()
    {
        var datos = Contexto.ProyectoVacio();
        datos.VersionDePlantillaGuardada = Contexto.Plantilla.Meta.Version;

        Assert.DoesNotContain("(registrado)", new ExportadorDeInforme(Contexto.Plantilla).GenerarHtml(datos));
    }

    /// <summary>
    /// <b>El informe dice contra qué año de la norma se ensayó.</b> Antes el título
    /// era «Luminarias — EN/IEC 60598-1» a secas: con dos años instalados, dos
    /// informes idénticos podían venir de normas distintas y nada lo distinguía.
    /// </summary>
    [Fact]
    public void ElInformeDiceElAnioDeLaNorma()
    {
        var html = new ExportadorDeInforme(Contexto.Plantilla).GenerarHtml(Contexto.ProyectoVacio());

        Assert.Contains(Contexto.Plantilla.Meta.AnioDePublicacion!, html);
        Assert.Contains(Contexto.Plantilla.Meta.Titulo!, html);
    }

    /// <summary>Y ninguna plantilla se queda con un título que no diga su año.</summary>
    [Fact]
    public void TodasLasNormasDicenSuAnioEnElTitulo()
        => Assert.All(Contexto.TodasLasPlantillas(),
            p => Assert.Contains(p.Meta.AnioDePublicacion!, p.Meta.Titulo!));

    /// <summary>Un proyecto que aún no se ha guardado nunca tampoco enseña dos números.</summary>
    [Fact]
    public void UnProyectoSinGuardarNoInventaVersiones()
        => Assert.DoesNotContain("(registrado)",
            new ExportadorDeInforme(Contexto.Plantilla).GenerarHtml(Contexto.ProyectoVacio()));
}
