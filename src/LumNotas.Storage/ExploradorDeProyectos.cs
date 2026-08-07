using System.Collections.Concurrent;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;

namespace LumNotas.Storage;

/// <summary>Cómo va el escaneo, para poder contarlo mientras dura.</summary>
public sealed record AvanceDeExploracion(int Leidos, int Total, string Texto);

/// <summary>
/// Busca proyectos en una carpeta y calcula su estado.
/// <para>
/// Se escanea la carpeta en vez de mantener un índice: el fichero es la única verdad.
/// Con OneDrive y varios técnicos, un índice se desincronizaría y mentiría.
/// </para>
/// <para>
/// En el laboratorio los proyectos cuelgan de una matrioska de carpetas de clientes
/// —<c>clientes/antares/antar2504/01/tomadenotas/</c>— con años de historia, así que el
/// escaneo está preparado para árboles grandes: <b>caché entre sesiones</b>, lectura en
/// paralelo, aviso de por dónde va y posibilidad de cancelarlo.
/// </para>
/// </summary>
/// <param name="rutaDeCache">
/// Dónde se guarda lo ya analizado. Se puede fijar para no tocar la caché real del
/// usuario desde los tests.
/// </param>
public sealed class ExploradorDeProyectos(RepositorioDeProyectos repositorio, string? rutaDeCache = null)
{
    /// <summary>
    /// Cuántos proyectos se leen a la vez. Sobre OneDrive el tiempo se va esperando al
    /// disco, no calculando, así que unos cuantos en paralelo compensan de sobra; pasarse
    /// solo añade contención.
    /// </summary>
    private const int ALaVez = 8;

    private readonly CacheDeResumenes _cache = CacheDeResumenes.Cargar(rutaDeCache);

    /// <summary>Atajo para cuando solo hay una norma en juego, como en los tests.</summary>
    public IReadOnlyList<ResumenDeProyecto> Explorar(
        string carpeta, PlantillaEnsayos plantilla, bool incluirSubcarpetas = true,
        IProgress<AvanceDeExploracion>? avance = null, CancellationToken cancelar = default)
        => Explorar(carpeta, new Dictionary<string, PlantillaEnsayos> { [plantilla.Meta.Id] = plantilla },
                    plantilla, incluirSubcarpetas, avance, cancelar);

    /// <param name="normasInstaladas">
    /// Las plantillas del laboratorio, por id. Cada proyecto se mide contra <b>las suyas</b>:
    /// un servicio de luminarias con módulos LED lleva dos, y medirlo solo contra una diría
    /// que está terminado con media toma de notas sin rellenar.
    /// </param>
    /// <param name="porDefecto">
    /// Con qué medir un proyecto que no diga qué normas lleva, como los guardados antes de
    /// que los proyectos apuntaran sus normas.
    /// </param>
    public IReadOnlyList<ResumenDeProyecto> Explorar(
        string carpeta, IReadOnlyDictionary<string, PlantillaEnsayos> normasInstaladas,
        PlantillaEnsayos porDefecto, bool incluirSubcarpetas = true,
        IProgress<AvanceDeExploracion>? avance = null, CancellationToken cancelar = default)
    {
        if (!Directory.Exists(carpeta)) return [];

        avance?.Report(new AvanceDeExploracion(0, 0, "Buscando tomas de notas"));

        var ficheros = Buscar(carpeta, incluirSubcarpetas, cancelar);
        if (ficheros.Count == 0) return [];

        // La caché guarda con qué normas se midió: al publicar una versión nueva de
        // cualquiera de ellas, lo guardado deja de describir lo mismo.
        var clave = ClaveDe(normasInstaladas.Values.Append(porDefecto));
        var resultados = new ConcurrentBag<ResumenDeProyecto>();
        var leidos = 0;
        var deCache = 0;

        Parallel.ForEach(
            ficheros,
            new ParallelOptions { MaxDegreeOfParallelism = ALaVez, CancellationToken = cancelar },
            fichero =>
            {
                ResumenDeProyecto resumen;

                // Lo ya analizado no se vuelve a leer: es lo que hace que el segundo
                // arranque sea inmediato aunque haya cientos de proyectos.
                if (_cache.Recuperar(fichero, clave) is { } guardado)
                {
                    resumen = guardado;
                    Interlocked.Increment(ref deCache);
                }
                else
                {
                    resumen = Analizar(fichero, normasInstaladas, porDefecto);
                    lock (_cache) _cache.Anotar(fichero, clave, resumen);
                }

                resultados.Add(resumen);

                var vistos = Interlocked.Increment(ref leidos);
                if (vistos % 10 == 0 || vistos == ficheros.Count)
                    avance?.Report(new AvanceDeExploracion(vistos, ficheros.Count,
                        $"Leyendo tomas de notas {vistos} de {ficheros.Count}"));
            });

        _cache.ConservarSolo(ficheros.Select(f => f.FullName));
        _cache.Guardar();

        return [.. resultados.OrderByDescending(r => r.Modificado)];
    }

    /// <summary>Fuerza a releerlo todo: lo usa el botón de actualizar.</summary>
    public void OlvidarCache()
    {
        _cache.Vaciar();
        _cache.Guardar();
    }

    private ResumenDeProyecto Analizar(FileInfo fichero,
                                       IReadOnlyDictionary<string, PlantillaEnsayos> normasInstaladas,
                                       PlantillaEnsayos porDefecto)
    {
        try
        {
            var (datos, planificacion) = repositorio.CargarCompleto(fichero.FullName);

            return AnalizadorDeProyectos.Analizar(
                NormasDe(datos, normasInstaladas, porDefecto),
                datos, fichero.FullName, fichero.LastWriteTime, planificacion);
        }
        catch (Exception ex)
        {
            // Un fichero corrupto o ajeno no puede tumbar el tablero.
            return AnalizadorDeProyectos.NoLegible(fichero.FullName, fichero.LastWriteTime, ex.Message);
        }
    }

    /// <summary>
    /// Las plantillas con las que medir un proyecto: las que él dice llevar. Cuál es la
    /// principal lo decide el analizador. Si no dice ninguna —o ninguna está instalada—
    /// se usa la de por defecto, para no dejarlo sin medir.
    /// </summary>
    private static IReadOnlyList<PlantillaEnsayos> NormasDe(
        Core.Datos.DatosProyecto datos, IReadOnlyDictionary<string, PlantillaEnsayos> instaladas,
        PlantillaEnsayos porDefecto)
    {
        // Por id exacto y, si no, por los que la plantilla dice haber tenido antes: los
        // proyectos guardados llevan el id que existía el día que se guardaron (DD‑95).
        var suyas = datos.Normas
            .Select(id => instaladas.GetValueOrDefault(id)
                          ?? instaladas.Values.FirstOrDefault(p => p.Meta.Responde(id)))
            .OfType<PlantillaEnsayos>()
            .Distinct()
            .ToList();

        return suyas.Count > 0 ? suyas : [porDefecto];
    }

    /// <summary>
    /// Recorre el árbol. <c>IgnoreInaccessible</c> es imprescindible: en una carpeta de
    /// clientes con años de historia siempre hay alguna rama sin permisos, y sin esto
    /// una sola de ellas abortaba el escaneo entero.
    /// </summary>
    private static List<FileInfo> Buscar(string carpeta, bool incluirSubcarpetas, CancellationToken cancelar)
    {
        var opciones = new EnumerationOptions
        {
            RecurseSubdirectories = incluirSubcarpetas,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            // Lo oculto y lo del sistema no son proyectos: papeleras, .git, metadatos.
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
        };

        var encontrados = new List<FileInfo>();

        var raiz = new DirectoryInfo(carpeta);

        foreach (var patron in RepositorioDeProyectos.Patrones)
            foreach (var fichero in raiz.EnumerateFiles(patron, opciones))
            {
                cancelar.ThrowIfCancellationRequested();
                encontrados.Add(fichero);
            }

        return encontrados;
    }

    /// <summary>
    /// Identidad de las normas con las que se midió. El resumen sale de aplicar sus
    /// reglas, así que al publicar una versión nueva de cualquiera deja de valer.
    /// </summary>
    private static string ClaveDe(IEnumerable<PlantillaEnsayos> normas)
        => string.Join(",", normas.Select(p => $"{p.Meta.Id}/{p.Meta.Version}")
                                  .Distinct()
                                  .OrderBy(t => t, StringComparer.Ordinal));
}
