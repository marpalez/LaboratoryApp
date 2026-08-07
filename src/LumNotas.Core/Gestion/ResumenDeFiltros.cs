namespace LumNotas.Core.Gestion;

/// <summary>
/// Qué está filtrando ahora mismo la vista de gestión, en una línea.
/// <para>
/// Existe porque los filtros pasaron a vivir dentro de un botón. Escondidos, el riesgo
/// deja de ser estético: alguien mira el tablero, no ve su servicio y **concluye que se
/// ha perdido**, cuando lo que pasa es que hay un técnico o una norma elegidos de la
/// semana pasada. El botón tiene que delatar que está filtrando <b>sin abrirlo</b>.
/// </para>
/// </summary>
public static class ResumenDeFiltros
{
    /// <summary>Lo que se elige para no filtrar por esa columna.</summary>
    public const string Cualquiera = "(todos)";

    /// <summary>
    /// Cuántos filtros están apartando algo. <b>«En desarrollo» no cuenta</b>: es lo que
    /// se mira a diario y lo que hay puesto al abrir, así que señalarlo como filtro activo
    /// dejaría el aviso encendido siempre y nadie volvería a mirarlo. <b>«Cualquier estado»
    /// tampoco</b>, por lo contrario: no aparta nada, lo enseña todo.
    /// </summary>
    public static int Cuantos(FiltrosDeGestion filtros)
    {
        var cuantos = 0;

        if (!string.IsNullOrWhiteSpace(filtros.Estado)
            && filtros.Estado != FiltroDeEstado.EnDesarrollo
            && filtros.Estado != FiltroDeEstado.Todos
            && filtros.Estado != FiltroDeEstado.Cualquiera) cuantos++;

        foreach (var valor in new[] { filtros.Tecnico, filtros.Norma, filtros.Ip, filtros.Ik, filtros.Acreditacion })
            if (FiltrosDeGestion.EsUnFiltro(valor)) cuantos++;

        // El periodo cuenta como uno solo aunque se pongan las dos fechas: es una sola
        // pregunta —«qué se ensayó entre estas dos»— partida en dos casillas.
        if (filtros.Desde is not null || filtros.Hasta is not null) cuantos++;

        return cuantos;
    }

    /// <summary>Lo que se lee en el botón: «Filtros», o «Filtros (2)» si aparta algo.</summary>
    public static string Rotulo(FiltrosDeGestion filtros)
    {
        var cuantos = Cuantos(filtros);
        return cuantos == 0 ? "Filtros" : $"Filtros ({cuantos})";
    }

    /// <summary>
    /// Qué se está viendo, para el consejo emergente del botón. Se nombra siempre el
    /// estado —aunque sea el de por defecto—, porque «En desarrollo» tampoco lo enseña
    /// todo: <b>deja fuera lo archivado</b>, y quien no lo sepa echará algo en falta. Los
    /// demás solo salen cuando están puestos, para que la línea diga en un vistazo lo que
    /// aparta y no una lista de «(todos)».
    /// </summary>
    /// <param name="nombreDeNorma">
    /// Cómo se llama una norma, dado su id. El filtro guarda el <b>id</b> —identidad estable
    /// (DD‑134)— y aquí hay que escribir la <b>designación</b>, que es lo que entiende quien
    /// lee: «EN IEC 60598‑1:2024 + A11:2024» y no <c>60598-1_2024</c>. Se recibe de fuera
    /// porque el núcleo no sabe qué normas hay instaladas. Sin ella se escribe el id, que es
    /// lo que se hacía antes y sigue siendo mejor que no decir nada.
    /// </param>
    public static string Detalle(FiltrosDeGestion filtros, Func<string, string>? nombreDeNorma = null)
    {
        var partes = new List<string>
        {
            "Estado: " + (string.IsNullOrWhiteSpace(filtros.Estado)
                ? FiltroDeEstado.EnDesarrollo
                : filtros.Estado)
        };

        Anadir("Técnico", filtros.Tecnico);
        Anadir("Norma", FiltrosDeGestion.EsUnFiltro(filtros.Norma) && nombreDeNorma is not null
                            ? nombreDeNorma(filtros.Norma)
                            : filtros.Norma);
        Anadir("IP", filtros.Ip);
        Anadir("IK", filtros.Ik);
        Anadir("Acreditación", filtros.Acreditacion);

        if (filtros.Desde is { } desde || filtros.Hasta is not null)
            partes.Add("Ensayado " + (filtros.Desde is { } d ? $"desde {d:dd/MM/yyyy} " : "")
                                   + (filtros.Hasta is { } h ? $"hasta {h:dd/MM/yyyy}" : "").Trim());

        if (FiltrosDeGestion.EsUnFiltro(filtros.Texto)) partes.Add($"Buscando «{filtros.Texto.Trim()}»");

        return "Mostrando | " + string.Join("   |   ", partes);

        void Anadir(string rotulo, string? valor)
        {
            if (FiltrosDeGestion.EsUnFiltro(valor)) partes.Add($"{rotulo}: {valor!.Trim()}");
        }
    }
}
