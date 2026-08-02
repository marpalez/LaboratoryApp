using System.IO;
using LumNotas.Core.Gestion;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// De dónde salen los técnicos que ofrecen las fichas de proyecto.
/// <para>
/// El fichero vive en la <b>carpeta de proyectos</b> —la compartida—, para que añadir o
/// corregir un técnico se haga una vez y lo vea todo el laboratorio. Mientras esa
/// carpeta no esté elegida se usa la de plantillas, que siempre existe, de modo que un
/// equipo recién instalado ya ofrece la lista de partida.
/// </para>
/// </summary>
public static class ServicioDeTecnicos
{
    private static CatalogoDeTecnicos? _catalogo;

    public static CatalogoDeTecnicos Catalogo => _catalogo ??= CatalogoDeTecnicos.Cargar(Carpeta());

    /// <summary>Vuelve a leer el fichero: otro técnico puede haberlo cambiado.</summary>
    public static void Recargar() => _catalogo = CatalogoDeTecnicos.Cargar(Carpeta());

    public static void Guardar() => Catalogo.Guardar(Carpeta());

    /// <summary>La carpeta compartida si está elegida; si no, la de plantillas.</summary>
    public static string Carpeta()
        => ServicioDeCarpetas.Compartida()
           ?? PlantillasCompartidas.LocalSiExiste()
           ?? AppContext.BaseDirectory;

    /// <summary>Si el fichero se guardará donde lo ven los demás, para poder avisarlo.</summary>
    public static bool EsCompartida => ServicioDeCarpetas.HayCompartida;
}
