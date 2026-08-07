namespace LumNotas.Core.Gestion;

/// <summary>
/// Una línea del listado de la BBDD. Solo texto: aquí no se edita nada.
/// <para>
/// <b>Vive en el núcleo y no en la interfaz</b> desde que el listado se puede exportar. La
/// exportación tiene que enseñar <b>exactamente</b> lo que se está viendo en pantalla, y
/// con dos definiciones de qué va en cada columna —una para la tabla y otra para el HTML—
/// eso duraría hasta el primer cambio. Aquí hay una sola, y las dos la usan.
/// </para>
/// </summary>
public sealed class FilaDeBbdd(ResumenDeProyecto proyecto)
{
    public string Ruta { get; } = proyecto.Ruta;

    public string Codigo { get; } = string.IsNullOrWhiteSpace(proyecto.CodigoTomaDeNotas)
        ? proyecto.Nombre
        : proyecto.CodigoTomaDeNotas;

    public string Acreditacion { get; } = string.Join(" | ", proyecto.Acreditaciones);
    public string Tecnico1 { get; } = proyecto.Tecnico;
    public string Tecnico2 { get; } = proyecto.Tecnico2;

    /// <summary>La norma con la que nació. Las añadidas no se enseñan: hoy como mucho hay una.</summary>
    public string Norma { get; } = proyecto.NormaPrincipal;

    public string Muestras { get; } = proyecto.NumeroMuestras.ToString();
    public string Ip { get; } = proyecto.GradoIp;
    public string Ik { get; } = proyecto.GradoIk;
    public string Estado { get; } = EstadoDe(proyecto);
    public string Colaboradores { get; } = string.Join(" | ", proyecto.Colaboradores);

    /// <summary>
    /// Cuándo se ensayó de verdad. Se rellena sola al dar el servicio por terminado, así
    /// que en lo que sigue en marcha está en blanco — y eso mismo se lee de un vistazo.
    /// </summary>
    public string Ensayado { get; } = Periodo(proyecto.Planificacion);

    /// <remarks>
    /// Con la cultura del laboratorio, no con la del equipo. La barra de <c>dd/MM/yyyy</c>
    /// no es una barra: es «el separador de fechas que toque», así que en un ordenador
    /// configurado en otra región las mismas fechas salían <c>01-07-2026</c>. Da igual para
    /// leerlo, pero el listado se exporta y se archiva, y dos copias del mismo listado hechas
    /// en dos equipos no pueden escribir distinto. Es el mismo motivo por el que el eje del
    /// calendario fija <see cref="EjeDeSemanas.CulturaDelLaboratorio"/>.
    /// </remarks>
    private static string Periodo(Planificacion plan)
    {
        if (plan.EnsayoDesde is not { } desde) return "";
        var hasta = plan.EnsayoHasta ?? desde;
        var cultura = EjeDeSemanas.CulturaDelLaboratorio;

        return desde.Date == hasta.Date
            ? desde.ToString("dd/MM/yyyy", cultura)
            : $"{desde.ToString("dd/MM/yyyy", cultura)} – {hasta.ToString("dd/MM/yyyy", cultura)}";
    }

    /// <summary>
    /// Lo archivado se dice como tal: es lo que explica que un servicio no aparezca en el
    /// tablero, y sin ello el listado y el tablero parecerían contradecirse.
    /// </summary>
    private static string EstadoDe(ResumenDeProyecto proyecto)
        => proyecto.Planificacion.Archivado
            ? "Archivado"
            : Planificacion.EtiquetaDe(proyecto.Planificacion.Estado);

    /// <summary>
    /// Las columnas del listado, en el orden en que se enseñan, y cómo se saca cada una de
    /// una fila.
    /// <para>
    /// Está aquí para que <b>la tabla de la pantalla y la del papel no puedan discrepar</b>:
    /// añadir una columna es tocar esta lista, y sale en los dos sitios. Los anchos son los
    /// de la pantalla en píxeles; el HTML los reparte en proporción, que es lo que necesita
    /// para caber en un A4 apaisado sea cual sea el número de columnas.
    /// </para>
    /// </summary>
    public static IReadOnlyList<ColumnaDeBbdd> Columnas =>
    [
        new("CÓDIGO", 170, f => f.Codigo),
        new("ACREDITACIÓN", 130, f => f.Acreditacion),
        new("TÉCNICO 1", 150, f => f.Tecnico1),
        new("TÉCNICO 2", 150, f => f.Tecnico2),
        new("NORMA", 270, f => f.Norma),
        new("MUESTRAS", 70, f => f.Muestras),
        new("IP", 70, f => f.Ip),
        new("IK", 70, f => f.Ik),
        new("ESTADO", 110, f => f.Estado),
        new("ENSAYADO", 180, f => f.Ensayado),
        new("LAB. EXTERNO", 200, f => f.Colaboradores)
    ];
}

/// <summary>Una columna del listado: cómo se titula, cuánto ocupa y de dónde sale.</summary>
public sealed record ColumnaDeBbdd(string Titulo, double Ancho, Func<FilaDeBbdd, string> Valor);
