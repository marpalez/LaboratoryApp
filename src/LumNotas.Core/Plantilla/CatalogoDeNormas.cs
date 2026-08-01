namespace LumNotas.Core.Plantilla;

/// <summary>Una norma disponible en la carpeta de plantillas, sin cargarla entera.</summary>
public sealed record NormaDisponible(string Id, string Titulo, string Ruta)
{
    /// <summary>Nombre corto para las tarjetas de la portada. Si falta, se usa el largo.</summary>
    public string TituloCorto { get; init; } = "";

    public override string ToString() => Titulo;
}

/// <summary>
/// Las normas que el laboratorio tiene instaladas. Cada una es un fichero
/// <c>plantilla-*.json</c> en la carpeta de plantillas: añadir una norma es dejar caer
/// un fichero ahí, sin tocar ni recompilar la aplicación.
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
                        : plantilla.Meta.TituloCorto!
                });
            }
            catch (Exception)
            {
                // se ignora: la norma simplemente no aparece en la lista
            }
        }

        return [.. normas.OrderBy(n => n.Id == "60598" ? 0 : 1).ThenBy(n => n.Titulo, StringComparer.CurrentCulture)];
    }

    private static string TituloDe(PlantillaEnsayos plantilla)
        => string.IsNullOrWhiteSpace(plantilla.Meta.Titulo)
            ? plantilla.Meta.Id
            : plantilla.Meta.Titulo!;
}
