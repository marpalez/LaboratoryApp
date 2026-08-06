namespace LumNotas.Core.Gestion;

/// <summary>
/// Lo que el responsable ha decidido mirar. <b>Un solo juego para las cuatro vistas</b>:
/// tablero, calendario, carga y BBDD hablan de los mismos proyectos, así que elegir dos
/// veces lo mismo en dos sitios distintos solo servía para que discreparan.
/// <para>
/// Vive en el núcleo y no en la ventana porque decidir qué proyecto se ve es una regla
/// del laboratorio, no una cuestión de interfaz — y porque en la ventana no había forma
/// de probarla.
/// </para>
/// </summary>
public sealed record FiltrosDeGestion
{
    /// <summary>Lo que se elige para no filtrar por eso.</summary>
    public const string Cualquiera = ResumenDeFiltros.Cualquiera;

    public string Estado { get; init; } = FiltroDeEstado.EnDesarrollo;
    public string Tecnico { get; init; } = Cualquiera;
    public string Norma { get; init; } = Cualquiera;
    public string Ip { get; init; } = Cualquiera;
    public string Ik { get; init; } = Cualquiera;
    public string Acreditacion { get; init; } = Cualquiera;

    /// <summary>La caja de buscar. Mira en todo lo que se lee del proyecto.</summary>
    public string Texto { get; init; } = "";

    /// <summary>
    /// Periodo de ensayo. Se compara contra las fechas que dejó el trabajo
    /// —<see cref="Planificacion.EnsayoDesde"/> y <see cref="Planificacion.EnsayoHasta"/>—,
    /// que se rellenan solas al terminar un servicio.
    /// <para>
    /// Entra lo que <b>se solapa</b> con el periodo, no solo lo que cabe entero dentro: un
    /// ensayo de enero a marzo tiene que salir al preguntar por febrero. Y un servicio sin
    /// esas fechas —porque no está terminado— no sale: la pregunta es «qué se hizo».
    /// </para>
    /// </summary>
    public DateTime? Desde { get; init; }

    public DateTime? Hasta { get; init; }

    /// <summary>Si un proyecto pasa todos los filtros y la búsqueda.</summary>
    public bool Pasa(ResumenDeProyecto proyecto)
    {
        if (!FiltroDeEstado.Pasa(proyecto.Planificacion, Estado)) return false;
        if (EsUnFiltro(Tecnico) && !EsSuyo(proyecto)) return false;
        if (EsUnFiltro(Norma) && !proyecto.Normas.Contains(Norma)) return false;
        if (!EntraEnElPeriodo(proyecto.Planificacion)) return false;

        return BusquedaDeProyectos.Pasa(proyecto, Texto, Ip, Ik, Acreditacion);
    }

    private bool EntraEnElPeriodo(Planificacion plan)
    {
        if (Desde is null && Hasta is null) return true;

        var desde = plan.EnsayoDesde;
        var hasta = plan.EnsayoHasta ?? plan.EnsayoDesde;
        if (desde is null || hasta is null) return false;

        if (Hasta is { } tope && desde > tope.Date) return false;
        if (Desde is { } suelo && hasta < suelo.Date) return false;

        return true;
    }

    /// <summary>
    /// Si el servicio lo lleva el técnico elegido. <b>«(sin técnico)» es una opción más</b>:
    /// pedirlo enseña justo los que están sin asignar, que es lo que hay que repartir.
    /// </summary>
    private bool EsSuyo(ResumenDeProyecto proyecto)
        => Tecnico == CargaPorTecnico.SinTecnico
            ? string.IsNullOrWhiteSpace(proyecto.Tecnico)
            : string.Equals(proyecto.Tecnico, Tecnico, StringComparison.CurrentCultureIgnoreCase);

    internal static bool EsUnFiltro(string? valor)
        => !string.IsNullOrWhiteSpace(valor) && valor != Cualquiera;
}
