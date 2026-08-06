namespace LumNotas.Core.Gestion;

/// <summary>
/// Buscar una toma de notas entre todas las que hay, incluidas las terminadas y las
/// archivadas.
/// <para>
/// <b>Es lo que responde a «¿te acuerdas de aquel proyecto de Antares con IP65?».</b> Por
/// eso ignora el filtro compartido de las otras tres vistas: ese arranca en «En
/// desarrollo», y lo que se busca aquí casi siempre está terminado. Un buscador que
/// esconde lo viejo no busca nada.
/// </para>
/// </summary>
public static class BusquedaDeProyectos
{
    /// <summary>Lo que se elige para no filtrar por esa columna.</summary>
    public const string Cualquiera = ResumenDeFiltros.Cualquiera;

    /// <summary>
    /// Los proyectos que encajan. El texto se busca <b>en todo lo que se lee</b> —código,
    /// técnicos, norma, acreditación, colaboradores— porque quien recuerda un proyecto no
    /// sabe por qué columna lo recuerda.
    /// </summary>
    public static IReadOnlyList<ResumenDeProyecto> Filtrar(
        IEnumerable<ResumenDeProyecto> proyectos,
        string? texto = null, string? ip = null, string? ik = null, string? acreditacion = null)
        => [.. proyectos.Where(p => Pasa(p, texto, ip, ik, acreditacion))];

    public static bool Pasa(ResumenDeProyecto proyecto,
                            string? texto, string? ip, string? ik, string? acreditacion)
    {
        if (EsUnFiltro(ip) && !Igual(proyecto.GradoIp, ip)) return false;
        if (EsUnFiltro(ik) && !Igual(proyecto.GradoIk, ik)) return false;

        if (EsUnFiltro(acreditacion)
            && !proyecto.Acreditaciones.Any(a => Igual(a, acreditacion))) return false;

        return CoincideElTexto(proyecto, texto);
    }

    /// <summary>
    /// Solo la parte de texto de la búsqueda. La usan también el tablero, el calendario y
    /// la carga, que buscan sobre lo que ya tienen filtrado en vez de sobre todo.
    /// <para>
    /// Vive aquí y no en cada vista para que <b>buscar signifique lo mismo en las cuatro</b>:
    /// si el listado mira los colaboradores y el tablero no, escribir «IMQ» en un sitio y
    /// en otro daría resultados distintos sin que nada lo explique.
    /// </para>
    /// </summary>
    public static bool CoincideElTexto(ResumenDeProyecto proyecto, string? texto)
        => !EsUnFiltro(texto) || Contiene(proyecto, texto!);

    /// <summary>
    /// Si algo de lo que se lee del proyecto contiene ese texto. Se compara sin distinguir
    /// mayúsculas: el técnico escribe «antar» buscando «ANTAR2504».
    /// </summary>
    private static bool Contiene(ResumenDeProyecto proyecto, string texto)
    {
        var buscado = texto.Trim();

        foreach (var campo in Legibles(proyecto))
            if (!string.IsNullOrEmpty(campo)
                && campo.Contains(buscado, StringComparison.CurrentCultureIgnoreCase)) return true;

        return false;
    }

    private static IEnumerable<string> Legibles(ResumenDeProyecto proyecto)
    {
        yield return proyecto.CodigoTomaDeNotas;
        yield return proyecto.CodigoServicio;
        yield return proyecto.Nombre;
        yield return proyecto.Tecnico;
        yield return proyecto.Tecnico2;
        yield return proyecto.GradoIp;
        yield return proyecto.GradoIk;

        foreach (var norma in proyecto.Normas) yield return norma;
        foreach (var acreditacion in proyecto.Acreditaciones) yield return acreditacion;
        foreach (var colaborador in proyecto.Colaboradores) yield return colaborador;
    }

    /// <summary>
    /// Los valores que ofrecer en cada desplegable: los que de verdad hay en los proyectos
    /// leídos, con «(todos)» delante. Una lista fija ofrecería grados que nadie ha
    /// ensayado nunca y escondería los que sí.
    /// </summary>
    public static IReadOnlyList<string> Opciones(IEnumerable<string?> valores)
        => [Cualquiera, .. valores.Where(v => !string.IsNullOrWhiteSpace(v))
                                  .Select(v => v!.Trim())
                                  .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                  .OrderBy(v => v, StringComparer.CurrentCulture)];

    private static bool EsUnFiltro(string? valor)
        => !string.IsNullOrWhiteSpace(valor) && valor != Cualquiera;

    private static bool Igual(string? uno, string? otro)
        => string.Equals((uno ?? "").Trim(), (otro ?? "").Trim(), StringComparison.CurrentCultureIgnoreCase);
}
