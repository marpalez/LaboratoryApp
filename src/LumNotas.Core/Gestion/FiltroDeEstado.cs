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
    /// <summary>Ni terminado ni archivado: en lo que se está trabajando.</summary>
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
    /// Lo que se ofrece, con «En desarrollo» primero por ser lo que se mira a diario.
    /// Lo terminado y lo archivado se piden a propósito, cada uno por su nombre.
    /// </summary>
    public static IReadOnlyList<string> Opciones =>
        [EnDesarrollo, .. Planificacion.Estados.Select(Planificacion.EtiquetaDe), Archivados];

    /// <summary>
    /// Si un proyecto entra en lo que se está mirando.
    /// <para>
    /// <b>Ninguna opción general trae lo terminado ni lo archivado.</b> Para verlos hay
    /// que pedirlos por su nombre —«Terminado», «Archivados»—, igual que quien busca
    /// «En curso» no quiere lo que se apartó de en medio.
    /// </para>
    /// </summary>
    public static bool Pasa(Planificacion plan, string? filtro) => filtro switch
    {
        null or "" or EnDesarrollo or Todos =>
            !plan.Archivado && plan.Estado != EstadoDeProyecto.Terminado,
        Archivados => plan.Archivado,
        _ => !plan.Archivado && Planificacion.EtiquetaDe(plan.Estado) == filtro
    };
}
