using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;

namespace LumNotas.Storage;

/// <summary>
/// Busca proyectos en una carpeta y calcula su estado.
/// <para>
/// Se escanea la carpeta en vez de mantener un índice: el fichero es la única verdad.
/// Con OneDrive y varios técnicos, un índice se desincronizaría y mentiría.
/// </para>
/// </summary>
public sealed class ExploradorDeProyectos(RepositorioDeProyectos repositorio)
{
    /// <summary>Ficheros ya analizados, para no releer lo que no ha cambiado.</summary>
    private readonly Dictionary<string, (DateTime Modificado, ResumenDeProyecto Resumen)> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ResumenDeProyecto> Explorar(string carpeta, PlantillaEnsayos plantilla,
                                                    bool incluirSubcarpetas = true)
    {
        if (!Directory.Exists(carpeta)) return [];

        var busqueda = incluirSubcarpetas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var resumenes = new List<ResumenDeProyecto>();

        foreach (var ruta in Directory.EnumerateFiles(carpeta, "*" + RepositorioDeProyectos.Extension, busqueda))
        {
            var modificado = File.GetLastWriteTime(ruta);

            // Releer 50 proyectos en cada refresco sería lento sobre OneDrive.
            if (_cache.TryGetValue(ruta, out var guardado) && guardado.Modificado == modificado)
            {
                resumenes.Add(guardado.Resumen);
                continue;
            }

            ResumenDeProyecto resumen;
            try
            {
                var (datos, planificacion) = repositorio.CargarCompleto(ruta);
                resumen = AnalizadorDeProyectos.Analizar(plantilla, datos, ruta, modificado, planificacion);
            }
            catch (Exception ex)
            {
                // Un fichero corrupto o ajeno no puede tumbar el tablero.
                resumen = AnalizadorDeProyectos.NoLegible(ruta, modificado, ex.Message);
            }

            _cache[ruta] = (modificado, resumen);
            resumenes.Add(resumen);
        }

        return [.. resumenes.OrderByDescending(r => r.Modificado)];
    }

    public void OlvidarCache() => _cache.Clear();
}
