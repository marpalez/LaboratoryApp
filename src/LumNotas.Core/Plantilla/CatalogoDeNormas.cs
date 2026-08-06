namespace LumNotas.Core.Plantilla;

/// <summary>Una norma disponible en la carpeta de plantillas, sin cargarla entera.</summary>
public sealed record NormaDisponible(string Id, string Titulo, string Ruta)
{
    /// <summary>Nombre corto para las tarjetas de la portada. Si falta, se usa el largo.</summary>
    public string TituloCorto { get; init; } = "";

    /// <summary>Ids con los que se conoció antes, para reconocer proyectos ya guardados.</summary>
    public IReadOnlyList<string> IdsAnteriores { get; init; } = [];

    /// <summary>Lo que sale en el nombre del fichero, que es más corto que el id.</summary>
    public string CodigoDeFichero { get; init; } = "";

    /// <summary>Versión de esta plantilla. Con varias del mismo id, manda la más alta.</summary>
    public string Version { get; init; } = "";

    /// <summary>Año de publicación, para distinguir dos versiones de la misma norma.</summary>
    public string AnioDePublicacion { get; init; } = "";

    /// <summary>
    /// La designación con sus enmiendas: <c>EN IEC 60598-1:2024 + A11:2024</c>. Es lo que
    /// hay que enseñar donde se listan las normas instaladas — con el nombre corto, las
    /// dos plantillas de la 60598 aparecían como «Luminarias» y «Luminarias».
    /// </summary>
    public string Designacion { get; init; } = "";

    /// <summary>Cómo se llama, con lo más preciso que tenga.</summary>
    public string ComoSeLlama => string.IsNullOrWhiteSpace(Designacion) ? Titulo : Designacion;

    /// <summary>
    /// Lo que se enseña en la tarjeta de la portada: el número de norma a secas. El id
    /// lleva además el año y no cabe — y al técnico le dice menos que «60598».
    /// </summary>
    public string Rotulo => string.IsNullOrWhiteSpace(CodigoDeFichero) ? Id : CodigoDeFichero;

    /// <summary>«Luminarias | 2024», para cuando conviven dos años de la misma norma.</summary>
    public string TituloConAnio => string.IsNullOrWhiteSpace(AnioDePublicacion)
        ? TituloCorto
        : $"{TituloCorto} | {AnioDePublicacion}";

    /// <summary>Edición de la norma, si la plantilla la declara. Ver <c>Meta.Edicion</c>.</summary>
    public string Edicion { get; init; } = "";

    /// <summary>
    /// El subtexto de la tarjeta de la portada: «Luminarias Ed.9». Ahí arriba va la
    /// designación completa, que es lo que distingue dos años de la misma norma; esto dice
    /// de qué trata, en una línea.
    /// <para>
    /// <b>Sin edición declarada se queda solo el nombre.</b> Un «Ed.» sin número detrás
    /// parece un dato a medio escribir, y las tres normas que aún no la tienen no deberían
    /// enseñarlo hasta que el laboratorio lo diga.
    /// </para>
    /// </summary>
    public string DescripcionConEdicion => string.IsNullOrWhiteSpace(Edicion)
        ? TituloCorto
        : $"{TituloCorto}  |  Ed. {Edicion}";

    /// <summary>Si es la norma que un proyecto pide, con el id de ahora o con uno viejo.</summary>
    public bool Responde(string? id)
        => !string.IsNullOrWhiteSpace(id)
           && (string.Equals(Id, id, StringComparison.OrdinalIgnoreCase)
               || IdsAnteriores.Any(a => string.Equals(a, id, StringComparison.OrdinalIgnoreCase)));

    public override string ToString() => Titulo;
}

/// <summary>
/// Las normas que el laboratorio tiene instaladas. Cada una es un fichero
/// <c>plantilla-&lt;id&gt;_&lt;version&gt;.json</c> en la carpeta de plantillas —por ejemplo
/// <c>plantilla-60598-1_2024_1.0.0.json</c>—: añadir una norma es dejar caer un fichero
/// ahí, sin tocar ni recompilar la aplicación.
/// <para>
/// El nombre lleva el <b>año de publicación</b> para que dos versiones de la misma norma puedan
/// convivir en la carpeta, y la <b>versión</b> para saber qué hay instalado sin abrir
/// nada. De la versión se ocupa <see cref="Disponibles"/>: si de un mismo id hay varias,
/// <b>solo cuenta la más alta</b> — las anteriores se quedan como respaldo y no aparecen
/// duplicadas en la portada.
/// </para>
/// </summary>
public static class CatalogoDeNormas
{
    private const string Patron = "plantilla-*.json";

    /// <summary>
    /// Busca la carpeta <c>plantilla</c> subiendo desde el ejecutable. En la versión
    /// instalada irá junto al binario; durante el desarrollo está en la raíz del proyecto.
    /// </summary>
    public static string Carpeta()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidata = Path.Combine(dir.FullName, "plantilla");
            if (Directory.Exists(candidata) && Directory.EnumerateFiles(candidata, Patron).Any())
                return candidata;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            "No se encuentra la carpeta 'plantilla' con las tomas de notas de las normas.");
    }

    /// <summary>
    /// Normas instaladas, con luminarias primero por ser la de uso más frecuente y el
    /// resto por título.
    /// <para>
    /// <b>De cada id, solo la versión más alta.</b> Con la versión en el nombre del
    /// fichero, publicar una corrección deja las dos en la carpeta; sin esta regla, la
    /// portada enseñaría dos tarjetas de la misma norma y el técnico tendría que adivinar
    /// cuál. La anterior se queda de respaldo y deja de contar.
    /// </para>
    /// </summary>
    public static IReadOnlyList<NormaDisponible> Disponibles(string? carpeta = null)
    {
        carpeta ??= Carpeta();

        var normas = new List<NormaDisponible>();
        foreach (var ruta in Directory.GetFiles(carpeta, Patron))
        {
            // Una plantilla rota no puede impedir que se abran las demás.
            try
            {
                var plantilla = PlantillaEnsayos.Cargar(ruta);
                if (string.IsNullOrWhiteSpace(plantilla.Meta.Id)) continue;

                var titulo = TituloDe(plantilla);
                normas.Add(new NormaDisponible(plantilla.Meta.Id, titulo, ruta)
                {
                    TituloCorto = string.IsNullOrWhiteSpace(plantilla.Meta.TituloCorto)
                        ? titulo
                        : plantilla.Meta.TituloCorto!,
                    IdsAnteriores = [.. plantilla.Meta.IdsAnteriores ?? []],
                    CodigoDeFichero = plantilla.Meta.CodigoParaFichero,
                    Version = plantilla.Meta.Version,
                    AnioDePublicacion = plantilla.Meta.AnioDePublicacion ?? "",
                    Edicion = plantilla.Meta.Edicion ?? "",
                    Designacion = plantilla.Meta.ComoSeLlamaLaNorma
                });
            }
            catch (Exception)
            {
                // se ignora: la norma simplemente no aparece en la lista
            }
        }

        // Luminarias primero. Se mira el código de fichero y no el id, que ahora lleva el
        // año: si mañana entra la de 2027, sigue saliendo la primera sin tocar nada.
        return [.. normas.GroupBy(n => n.Id, StringComparer.OrdinalIgnoreCase)
                         .Select(LaMasNueva)
                         .OrderBy(n => n.CodigoDeFichero == "60598" ? 0 : 1)
                         .ThenBy(n => n.Titulo, StringComparer.CurrentCulture)];
    }

    /// <summary>
    /// De varios ficheros del mismo id, el de versión más alta. Una versión que no se
    /// entiende cuenta como la más baja: entre un número raro y uno legible, se instala
    /// el legible.
    /// </summary>
    private static NormaDisponible LaMasNueva(IEnumerable<NormaDisponible> mismasNormas)
        => mismasNormas
            .OrderByDescending(n => Version.TryParse(Numero(n.Version), out var v) ? v : new Version(0, 0))
            .ThenByDescending(n => n.Version, StringComparer.OrdinalIgnoreCase)
            .First();

    /// <summary>Quita lo que cuelgue detrás del número, como «1.2.0-beta».</summary>
    private static string Numero(string version)
    {
        var texto = (version ?? "").Trim();
        var corte = texto.IndexOfAny(['-', '+', ' ']);
        return corte > 0 ? texto[..corte] : texto;
    }

    private static string TituloDe(PlantillaEnsayos plantilla)
        => string.IsNullOrWhiteSpace(plantilla.Meta.Titulo)
            ? plantilla.Meta.Id
            : plantilla.Meta.Titulo!;
}
