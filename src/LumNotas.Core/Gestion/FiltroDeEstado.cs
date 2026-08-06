namespace LumNotas.Core.Gestion;

/// <summary>
/// Qué proyectos se están mirando. Es el mismo filtro para las tres vistas: el
/// responsable decide una vez qué le interesa y el tablero, el calendario y la carga
/// hablan de lo mismo.
/// <para>
/// <b>Manda el estado que puso la persona</b>, no el que deduce el programa. Un servicio
/// con todas las secciones rellenas pero esperando confirmación del cliente no está
/// terminado, y uno al que le falta una casilla puede estarlo. Lo que calcula el motor
/// —cuántas secciones van— se queda como lo que es: el avance.
/// </para>
/// </summary>
public static class FiltroDeEstado
{
    /// <summary>
    /// Todo lo que no está archivado, terminados incluidos (2026‑08‑05).
    /// <para>
    /// Antes dejaba fuera lo terminado, y eso escondía trabajo que sigue vivo: un servicio
    /// terminado la semana pasada se sigue mirando —hay que facturarlo, el cliente
    /// pregunta— y desaparecer del tablero no ayudaba a nadie. <b>Lo único excluyente es
    /// archivar</b>, que es un gesto deliberado y quiere decir «quítamelo de en medio».
    /// </para>
    /// </summary>
    public const string EnDesarrollo = "En desarrollo";

    /// <summary>
    /// Nombre antiguo de <see cref="EnDesarrollo"/>, cuando «Todos» quería decir
    /// literalmente todo. <b>Ya no se ofrece</b>: lo terminado y lo archivado no se
    /// miran a diario, y con ellos dentro la carga mensual salía inflada por trabajo
    /// que ya no existe. Se sigue reconociendo —significando lo mismo que
    /// «En desarrollo»— para que quien lo pase no se encuentre el tablero en blanco.
    /// </summary>
    public const string Todos = "Todos";

    public const string Archivados = "Archivados";

    /// <summary>
    /// Todo a la vez, archivado incluido. <b>Es lo que hace buscable la BBDD</b>: desde que
    /// los filtros son un solo juego para las cuatro vistas, sin esta opción no había forma
    /// de ver a la vez lo terminado y lo archivado, y encontrar un servicio de hace tres
    /// años obligaba a ir probando estados de uno en uno hasta acertar.
    /// <para>
    /// No se pone por defecto y sigue sin ser lo normal: con lo terminado dentro, la carga
    /// mensual sale inflada por trabajo que ya no existe. Se pide a sabiendas.
    /// </para>
    /// </summary>
    public const string Cualquiera = "Cualquier estado";

    /// <summary>
    /// Lo que se ofrece, con «En desarrollo» primero por ser lo que se mira a diario y
    /// «Cualquier estado» al final, que es el cajón de buscar cosas viejas.
    /// </summary>
    public static IReadOnlyList<string> Opciones =>
        [EnDesarrollo, .. Planificacion.Estados.Select(Planificacion.EtiquetaDe), Archivados, Cualquiera];

    /// <summary>
    /// Si un proyecto entra en lo que se está mirando.
    /// <para>
    /// <b>Lo único que esconde es archivar.</b> «En desarrollo» trae todo lo demás,
    /// terminados incluidos; pedir un estado concreto —«En curso», «Terminado»— trae ese y
    /// solo ese, y sigue dejando fuera lo archivado, porque quien busca «En curso» no
    /// quiere lo que se apartó de en medio.
    /// </para>
    /// </summary>
    public static bool Pasa(Planificacion plan, string? filtro) => filtro switch
    {
        null or "" or EnDesarrollo or Todos => !plan.Archivado,
        Cualquiera => true,
        Archivados => plan.Archivado,
        _ => !plan.Archivado && Planificacion.EtiquetaDe(plan.Estado) == filtro
    };
}
