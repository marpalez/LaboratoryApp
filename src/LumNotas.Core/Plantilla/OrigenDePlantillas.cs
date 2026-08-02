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
    /// <summary>Subcarpeta que se busca dentro de la carpeta de proyectos.</summary>
    public const string NombreDeCarpeta = "plantilla";

    private const string Patron = "plantilla-*.json";
    private const string Equipos = "equipos-*.json";

    public static OrigenDePlantillas Resolver(string? carpetaDeProyectos)
    {
        var local = LocalSiExiste();

        if (string.IsNullOrWhiteSpace(carpetaDeProyectos))
            return Devolver(local, "No hay carpeta de proyectos elegida, así que se usan las normas de este equipo.");

        var compartida = Path.Combine(carpetaDeProyectos, NombreDeCarpeta);

        if (Tiene(compartida)) return new OrigenDePlantillas(compartida, EsCompartida: true);

        if (!Directory.Exists(carpetaDeProyectos))
            return Devolver(local, "No se pudo llegar a la carpeta compartida; se usan las normas de este equipo.");

        return Devolver(local, "Las normas todavía no están publicadas en la carpeta compartida; "
                               + "se usan las de este equipo.");
    }

    private static OrigenDePlantillas Devolver(string? local, string aviso)
        => local is null
            ? throw new DirectoryNotFoundException(
                "No se encuentra la carpeta 'plantilla' con las tomas de notas de las normas.")
            : new OrigenDePlantillas(local, EsCompartida: false, aviso);

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
    public static int Publicar(string carpetaLocal, string carpetaDeProyectos)
    {
        var destino = Path.Combine(carpetaDeProyectos, NombreDeCarpeta);
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
