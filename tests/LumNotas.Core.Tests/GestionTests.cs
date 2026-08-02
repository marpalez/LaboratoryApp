using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Tablero de gestión: una columna por proyecto y una tarjeta por sección pendiente.
/// Los proyectos se detectan escaneando una carpeta, sin índice que pueda mentir.
/// </summary>
public class GestionTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-pm-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    public GestionTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Guardar(DatosProyecto datos, string nombre)
    {
        var carpeta = Path.Combine(_carpeta, nombre);
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, nombre + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0-mvp");
        return ruta;
    }

    private static DatosProyecto Proyecto(string codigo)
    {
        var datos = new DatosProyecto { CodigoServicio = codigo, NumeroMuestras = 2 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        return datos;
    }

    [Fact]
    public void UnProyectoVacio_TieneTodasLasSeccionesPendientes()
    {
        var resumen = AnalizadorDeProyectos.Analizar(
            Contexto.Plantilla, Proyecto("111112026"), "x.lumproj", DateTime.Now);

        Assert.Null(resumen.Error);
        Assert.NotEmpty(resumen.SeccionesPendientes);
        Assert.Equal(0, resumen.SeccionesCompletadas);
        Assert.False(resumen.Terminado);
    }

    /// <summary>
    /// El avance del tablero se cuenta por secciones: la sección 7 vale 1 aunque tenga
    /// trece apartados dentro. Es lo que el PM quiere ver.
    /// </summary>
    [Fact]
    public void ElAvanceCuentaSeccionesNoApartados()
    {
        var resumen = AnalizadorDeProyectos.Analizar(
            Contexto.Plantilla, Proyecto("111112026"), "x", DateTime.Now);

        var seccionesConApartadosAplicables = Contexto.Plantilla.Secciones.Count(s => s.Bloques.Count > 0);

        Assert.True(resumen.SeccionesAplicables <= seccionesConApartadosAplicables);
        Assert.True(resumen.SeccionesAplicables < Contexto.Plantilla.Bloques().Count(),
            "Contar por secciones debe dar bastante menos que contar por apartados.");

        // La sección 7 tiene 13 apartados y solo puede aportar una tarjeta.
        Assert.Single(resumen.SeccionesPendientes, s => s.Titulo == "Sección 7 - Construcción");
    }

    [Fact]
    public void AlCompletarUnApartado_SuSeccionBajaDePendientes()
    {
        var datos = Proyecto("111112026");
        var antes = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x", DateTime.Now);
        var marcadoAntes = antes.SeccionesPendientes.Single(s => s.Titulo == "Sección 6 - Marcado");

        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));
        var despues = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x", DateTime.Now);

        Assert.Equal(1, marcadoAntes.Pendientes);
        Assert.DoesNotContain(despues.SeccionesPendientes, s => s.Titulo == "Sección 6 - Marcado");
        Assert.Equal(antes.SeccionesCompletadas + 1, despues.SeccionesCompletadas);
    }

    [Fact]
    public void LasSeccionesQueNoAplican_NoSalenEnElTablero()
    {
        // Sin ninguna parte -2 marcada, esa sección entera desaparece.
        var resumen = AnalizadorDeProyectos.Analizar(
            Contexto.Plantilla, Proyecto("111112026"), "x", DateTime.Now);

        Assert.DoesNotContain(resumen.SeccionesPendientes,
            s => s.Titulo.Contains("partes -2"));
    }

    [Fact]
    public void ExplorarEncuentraLosProyectosDeLasSubcarpetas()
    {
        Guardar(Proyecto("111112026"), "111112026");
        Guardar(Proyecto("222222026"), "222222026");
        Guardar(Proyecto("333332026"), "333332026");

        var resumenes = new ExploradorDeProyectos(_repositorio, Path.Combine(_carpeta, "cache.json")).Explorar(_carpeta, Contexto.Plantilla);

        Assert.Equal(3, resumenes.Count);
        Assert.All(resumenes, r => Assert.Null(r.Error));
        Assert.Contains(resumenes, r => r.CodigoServicio == "222222026");
    }

    [Fact]
    public void UnFicheroCorrupto_NoTumbaElTablero()
    {
        Guardar(Proyecto("111112026"), "bueno");
        File.WriteAllText(Path.Combine(_carpeta, "roto" + RepositorioDeProyectos.Extension), "{ esto no es json");

        var resumenes = new ExploradorDeProyectos(_repositorio, Path.Combine(_carpeta, "cache.json")).Explorar(_carpeta, Contexto.Plantilla);

        Assert.Equal(2, resumenes.Count);
        Assert.Single(resumenes, r => r.Error is not null);
        Assert.Single(resumenes, r => r.Error is null);
    }

    [Fact]
    public void CarpetaInexistente_DevuelveListaVacia()
    {
        var resumenes = new ExploradorDeProyectos(_repositorio, Path.Combine(_carpeta, "cache.json"))
            .Explorar(Path.Combine(_carpeta, "no-existe"), Contexto.Plantilla);

        Assert.Empty(resumenes);
    }
}
