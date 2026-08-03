namespace LumNotas.Core.Plantilla;

/// <summary>De dónde se han leído las normas y por qué.</summary>
/// <param name="Carpeta">Carpeta que se está usando.</param>
/// <param name="EsCompartida">Si es la del laboratorio o la de este equipo.</param>
/// <param name="Aviso">
/// Por qué no se está usando la compartida, cuando corresponda. Es lo que hay que
/// enseñar al técnico: trabajar con una versión distinta de la norma que el compañero
/// no puede pasar inadvertido.
/// </param>
public sealed record OrigenDePlantillas(string Carpeta, bool EsCompartida, string? Aviso = null)
{
    public bool HayAviso => !string.IsNullOrWhiteSpace(Aviso);
}

/// <summary>
/// Decide de dónde salen las normas.
/// <para>
/// <b>Manda la carpeta compartida.</b> Si cada equipo lleva su copia de las plantillas,
/// dos técnicos pueden estar rellenando versiones distintas de la misma norma sin
/// enterarse, y eso en un laboratorio acreditado es un problema, no una molestia.
/// </para>
/// <para>
/// La copia local queda como respaldo: si OneDrive está sin conexión, el laboratorio
/// tiene que poder seguir trabajando, pero <b>sabiendo</b> que está con su copia.
/// </para>
/// </summary>
public static class PlantillasCompartidas
{
    /// <summary>Subcarpeta que se busca dentro de la carpeta compartida.</summary>
    public const string NombreDeCarpeta = "plantilla";

    private const string Patron = "plantilla-*.json";
    private const string Equipos = "equipos-*.json";

    /// <param name="carpetaCompartida">
    /// La carpeta compartida del laboratorio, dentro de la cual se busca la subcarpeta
    /// <c>plantilla</c>. <b>No es la de proyectos</b> —aunque se use como respaldo cuando
    /// no hay compartida elegida—, y confundirlas hace buscar las normas donde no están.
    /// </param>
    public static OrigenDePlantillas Resolver(string? carpetaCompartida)
    {
        var local = LocalSiExiste();

        if (string.IsNullOrWhiteSpace(carpetaCompartida))
            return Devolver(local, "No hay carpeta compartida elegida, así que se usan las normas de este equipo.");

        var conNormas = Path.Combine(carpetaCompartida, NombreDeCarpeta);

        if (Tiene(conNormas)) return new OrigenDePlantillas(conNormas, EsCompartida: true);

        if (!Directory.Exists(carpetaCompartida))
            return Devolver(local, "No se pudo llegar a la carpeta compartida; se usan las normas de este equipo.");

        return Devolver(local, "Las normas todavía no están publicadas en la carpeta compartida; "
                               + "se usan las de este equipo.");
    }

    private static OrigenDePlantillas Devolver(string? local, string aviso)
        => local is null
            ? throw new DirectoryNotFoundException(
                "No se encuentra la carpeta 'plantilla' con las tomas de notas de las normas.")
            : new OrigenDePlantillas(local, EsCompartida: false, aviso);

    /// <summary>
    /// Normas de este equipo que el laboratorio todavía no tiene.
    /// <para>
    /// Una vez publicada la primera tanda, el programa lee de la carpeta compartida y
    /// <b>deja de mirar la local</b>. Sin esta comparación, añadir una norma —o un año
    /// nuevo de una— aquí no producía ninguna señal: el fichero estaba en el equipo, no
    /// aparecía en el programa y nada explicaba por qué.
    /// </para>
    /// </summary>
    /// <param name="Nuevas">Ids que no están publicados.</param>
    /// <param name="MasNuevas">Ids publicados con una versión anterior a la de aquí.</param>
    public sealed record SinPublicar(IReadOnlyList<string> Nuevas, IReadOnlyList<string> MasNuevas)
    {
        public int Cuantas => Nuevas.Count + MasNuevas.Count;

        public bool HayAlgo => Cuantas > 0;

        public static SinPublicar Nada { get; } = new([], []);
    }

    /// <summary>
    /// Compara lo que hay en este equipo con lo publicado. Si la carpeta compartida no
    /// está elegida no hay nada que comparar: aún se está trabajando en local.
    /// </summary>
    public static SinPublicar Comparar(string? carpetaLocal, string? carpetaCompartida)
    {
        if (string.IsNullOrWhiteSpace(carpetaLocal) || string.IsNullOrWhiteSpace(carpetaCompartida))
            return SinPublicar.Nada;

        var conNormas = Path.Combine(carpetaCompartida, NombreDeCarpeta);
        if (!Tiene(conNormas)) return SinPublicar.Nada;

        var locales = CatalogoDeNormas.Disponibles(carpetaLocal);
        var publicadas = CatalogoDeNormas.Disponibles(conNormas)
            .ToDictionary(n => n.Id, n => n.Version, StringComparer.OrdinalIgnoreCase);

        var nuevas = new List<string>();
        var masNuevas = new List<string>();

        foreach (var local in locales)
        {
            if (!publicadas.TryGetValue(local.Id, out var publicada)) nuevas.Add(local.Titulo);
            else if (EsPosterior(local.Version, publicada)) masNuevas.Add(local.Titulo);
        }

        return new SinPublicar(nuevas, masNuevas);
    }

    /// <summary>
    /// Si la de aquí es posterior a la publicada. Ante un número que no se entiende
    /// devuelve falso: es preferible no avisar que avisar en falso todos los días.
    /// </summary>
    private static bool EsPosterior(string aqui, string publicada)
        => Version.TryParse(Numero(aqui), out var a)
           && Version.TryParse(Numero(publicada), out var b)
           && a > b;

    private static string Numero(string version)
    {
        var texto = (version ?? "").Trim();
        var corte = texto.IndexOfAny(['-', '+', ' ']);
        return corte > 0 ? texto[..corte] : texto;
    }

    /// <summary>La carpeta de este equipo, o <c>null</c> si tampoco está.</summary>
    public static string? LocalSiExiste()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidata = Path.Combine(dir.FullName, NombreDeCarpeta);
            if (Tiene(candidata)) return candidata;
            dir = dir.Parent;
        }

        return null;
    }

    /// <summary>
    /// Copia las normas de este equipo a la carpeta compartida, para que pasen a ser las
    /// de todos. Devuelve cuántos ficheros se han copiado.
    /// </summary>
    public static int Publicar(string carpetaLocal, string carpetaCompartida)
    {
        var destino = Path.Combine(carpetaCompartida, NombreDeCarpeta);
        Directory.CreateDirectory(destino);

        var copiados = 0;

        // Las plantillas y sus catálogos de equipos viajan juntos: el catálogo se busca
        // al lado de su plantilla, así que separarlos dejaría apartados sin equipos.
        foreach (var patron in new[] { Patron, Equipos })
            foreach (var origen in Directory.GetFiles(carpetaLocal, patron))
            {
                File.Copy(origen, Path.Combine(destino, Path.GetFileName(origen)), overwrite: true);
                copiados++;
            }

        return copiados;
    }

    private static bool Tiene(string carpeta)
        => Directory.Exists(carpeta) && Directory.EnumerateFiles(carpeta, Patron).Any();
}
