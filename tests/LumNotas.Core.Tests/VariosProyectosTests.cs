using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Varios proyectos en carpetas distintas, que es como trabaja el laboratorio:
/// cada servicio tiene su carpeta y dentro va su toma de notas.
/// </summary>
public class VariosProyectosTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "lumnotas-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Carpeta(string servicio)
    {
        var carpeta = Path.Combine(_raiz, servicio);
        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    private string RutaDe(string servicio)
        => Path.Combine(Carpeta(servicio), servicio + RepositorioDeProyectos.Extension);

    [Fact]
    public void TresProyectosEnTresCarpetas_SonIndependientes()
    {
        // Proyecto 1: marcado terminado.
        var uno = new DatosProyecto { CodigoServicio = "111112026", NumeroMuestras = 1 };
        uno.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));

        // Proyecto 2: marcado sin empezar, con 4 muestras.
        var dos = new DatosProyecto { CodigoServicio = "222222026", NumeroMuestras = 4 };

        // Proyecto 3: marcado marcado como no aplica.
        var tres = new DatosProyecto { CodigoServicio = "333332026", NumeroMuestras = 2 };
        tres.EstablecerNa("6/na", true);

        _repositorio.Guardar(uno, RutaDe("111112026"), "1.0.0-mvp");
        _repositorio.Guardar(dos, RutaDe("222222026"), "1.0.0-mvp");
        _repositorio.Guardar(tres, RutaDe("333332026"), "1.0.0-mvp");

        // Se releen por separado y cada uno conserva su estado.
        var leidoUno = _repositorio.Cargar(RutaDe("111112026"));
        var leidoDos = _repositorio.Cargar(RutaDe("222222026"));
        var leidoTres = _repositorio.Cargar(RutaDe("333332026"));

        Assert.Equal(1, leidoUno.NumeroMuestras);
        Assert.Equal(4, leidoDos.NumeroMuestras);
        Assert.Equal(2, leidoTres.NumeroMuestras);

        Assert.False(Contexto.Motor(leidoUno).EsVerdadera("R-06-02"));   // completo
        Assert.True(Contexto.Motor(leidoDos).EsVerdadera("R-06-02"));    // faltan datos
        Assert.False(Contexto.Motor(leidoTres).EsVerdadera("R-06-02"));  // no aplica

        Assert.Equal(3, new IndicadorDeAvance(Contexto.Motor(leidoUno)).Calcular().PesoEjecutado);
        Assert.Equal(0, new IndicadorDeAvance(Contexto.Motor(leidoDos)).Calcular().PesoEjecutado);
        // Con el Marcado en N/A, su peso (3) sale del total de la plantilla.
        var total = new IndicadorDeAvance(Contexto.Motor(leidoUno)).Calcular().PesoTotal;
        Assert.Equal(total - 3, new IndicadorDeAvance(Contexto.Motor(leidoTres)).Calcular().PesoTotal);
    }

    [Fact]
    public void GuardarUnProyectoNoTocaLosDemas()
    {
        var uno = new DatosProyecto { CodigoServicio = "111112026", NumeroMuestras = 1 };
        var dos = new DatosProyecto { CodigoServicio = "222222026", NumeroMuestras = 1 };
        _repositorio.Guardar(uno, RutaDe("111112026"), "1.0.0-mvp");
        _repositorio.Guardar(dos, RutaDe("222222026"), "1.0.0-mvp");

        var selloAntes = File.GetLastWriteTimeUtc(RutaDe("222222026"));

        uno.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 21));
        _repositorio.Guardar(uno, RutaDe("111112026"), "1.0.0-mvp");

        Assert.Equal(selloAntes, File.GetLastWriteTimeUtc(RutaDe("222222026")));
        Assert.Null(_repositorio.Cargar(RutaDe("222222026")).Instante("6", "ambiente.fecha"));
    }

    [Fact]
    public void CadaProyectoRecuerdaLaVersionDePlantillaConLaQueNacio()
    {
        var datos = new DatosProyecto { CodigoServicio = "111112026", NumeroMuestras = 1 };
        _repositorio.Guardar(datos, RutaDe("111112026"), "1.0.0-mvp");

        Assert.Contains("\"versionPlantilla\": \"1.0.0-mvp\"", File.ReadAllText(RutaDe("111112026")));
    }
}
