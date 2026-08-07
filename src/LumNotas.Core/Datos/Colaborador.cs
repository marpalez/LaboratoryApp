namespace LumNotas.Core.Datos;

/// <summary>
/// Un laboratorio de fuera que ha hecho parte del ensayo.
/// <para>
/// Va en la toma de notas y no en un objeto aparte porque aquí no hay objeto proyecto
/// (DD‑89): lo que existe es la toma de notas de cada familia, y es en ella donde consta
/// quién ensayó qué.
/// </para>
/// </summary>
public sealed class Colaborador
{
    /// <summary>Cómo se llama. Texto libre: «IMQ Italia», «CandelTEC», «Asselum».</summary>
    public string Laboratorio { get; set; } = "";

    /// <summary>Qué ensayo hizo y por qué se le encargó.</summary>
    public string EnsayoYMotivo { get; set; } = "";

    /// <summary>
    /// Lo que traiga el fichero de una versión posterior y aquí no se entienda. Ver
    /// <see cref="FormatoDeFichero"/>: sin esto, un equipo con la versión de antes borra
    /// los campos nuevos <b>de dentro de cada colaborador</b> al guardar.
    /// <para>
    /// Hace falta uno por cada objeto anidado, no basta con el de la raíz. Se descubrió
    /// midiéndolo: la primera versión de la protección cubría el documento y la
    /// planificación, y lo de dentro de las listas se seguía perdiendo.
    /// </para>
    /// </summary>
    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? Desconocido { get; set; }

    /// <summary>
    /// Si tiene algo escrito. Una fila recién añadida y sin rellenar no se guarda: el
    /// técnico pulsa el botón, se lo piensa y se va — y eso no es un colaborador.
    /// </summary>
    public bool TieneAlgo
        => !string.IsNullOrWhiteSpace(Laboratorio) || !string.IsNullOrWhiteSpace(EnsayoYMotivo);

    /// <summary>Lo que se lee de un vistazo en el listado.</summary>
    public override string ToString() => Laboratorio.Trim();
}
