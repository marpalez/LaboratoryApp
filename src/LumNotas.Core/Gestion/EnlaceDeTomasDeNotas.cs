namespace LumNotas.Core.Gestion;

/// <summary>
/// Una línea del calendario: una toma de notas suelta, o varias enlazadas entre sí.
/// </summary>
/// <param name="Cabecera">
/// La que lleva las fechas y el importe, y por tanto la que se dibuja y se arrastra.
/// </param>
/// <param name="Miembros">
/// Todas las del grupo, cabecera incluida. En una toma de notas suelta es ella sola.
/// </param>
public sealed record EntradaDeCalendario(
    ResumenDeProyecto Cabecera, IReadOnlyList<ResumenDeProyecto> Miembros)
{
    public bool EsGrupo => Miembros.Count > 1;

    public string? Grupo => Cabecera.Planificacion.Grupo;

    /// <summary>
    /// Las familias en el orden en que se dibujan, que lo dan <b>las fechas de inicio</b>
    /// (DD‑123). Sigue siendo estable —la cadena las deja en fila y solo se mueven si
    /// alguien las mueve queriendo—, y a igualdad de fecha desempata el código.
    /// </summary>
    public IReadOnlyList<ResumenDeProyecto> EnOrden => CadenaDelGrupo.EnOrden(Miembros);

    /// <summary>
    /// Cuándo empieza el trabajo entero: el inicio de la <b>primera</b> familia por código.
    /// <para>
    /// No es el más temprano de todas, y es a propósito: las familias se encadenan, así
    /// que la segunda empieza donde acaba la primera aunque tenga otra fecha escrita. Si
    /// la primera no lleva inicio, manda el de la primera que lo tenga.
    /// </para>
    /// </summary>
    public DateTime? Inicio
        => EnOrden.Select(m => m.Planificacion.Inicio).OfType<DateTime>().FirstOrDefault() is
           { } valor && valor != default ? valor : null;

    /// <summary>
    /// Y cuándo acaba: <b>donde acaba la cadena</b> de sus familias, cada una durando lo
    /// suyo. No es sin más la fecha más tardía que haya escrita: si la primera dura tres
    /// semanas y la segunda va detrás, el trabajo acaba tres semanas después de la primera,
    /// no cuando ponga la segunda —que es una fecha pensada sin saber dónde iba a caer.
    /// <b>Por eso la barra de un grupo es más larga que la de su cabecera</b> en cuanto las
    /// familias llevan fechas propias.
    /// </summary>
    public DateTime? Fin => TramosDelGrupo.FinDelTrabajo(EnOrden, Inicio, FinMasTardio);

    /// <summary>Lo más tarde que acabe cualquiera. Es el suelo del fin del trabajo.</summary>
    private DateTime? FinMasTardio
        => Miembros.Select(m => m.Planificacion.FinEfectivo).OfType<DateTime>().DefaultIfEmpty().Max() is
           { } valor && valor != default ? valor : null;

    /// <summary>En qué se parte la barra. Una suelta da un solo tramo, el ancho entero.</summary>
    public IReadOnlyList<TramoDelGrupo> Tramos => TramosDelGrupo.Calcular(this);

    /// <summary>El avance del trabajo entero, sumando lo de todas las enlazadas.</summary>
    public int SeccionesCompletadas => Miembros.Sum(m => m.SeccionesCompletadas);

    public int SeccionesAplicables => Miembros.Sum(m => m.SeccionesAplicables);

    /// <summary>
    /// Lo que enseña la barra: el avance del grupo entero, no el de su cabecera. Un
    /// trabajo con cuatro familias no está hecho porque lo esté la primera.
    /// </summary>
    public string Avance => Cabecera.Error is not null
        ? "no se pudo leer"
        : $"{SeccionesCompletadas}/{SeccionesAplicables} secciones";

    /// <summary>
    /// Suma de los importes de las enlazadas. Con la cabecera llevando el único importe
    /// —que es como debe hacerse— coincide con el de la oferta. Si aparece <b>el cuádruple
    /// de lo que costó el trabajo</b>, es que se ha repetido en las cuatro.
    /// </summary>
    public double? Importe
    {
        get
        {
            var importes = Miembros.Select(m => m.Planificacion.Importe).OfType<double>().ToList();
            return importes.Count == 0 ? null : importes.Sum();
        }
    }
}

/// <summary>
/// Enlaza varias tomas de notas como un solo trabajo.
/// <para>
/// Nace de que un servicio del laboratorio puede llevar <b>cuatro familias de
/// luminarias</b>, cada una con su toma de notas: el jefe quiere planificar una cosa y el
/// técnico seguir viendo las cuatro. Se resuelve enlazándolas —un nombre de grupo dentro
/// de cada fichero— y <b>no</b> creando un fichero de proyecto por encima, que rompería
/// la trazabilidad (DD‑89).
/// </para>
/// </summary>
public static class EnlaceDeTomasDeNotas
{
    /// <summary>
    /// El nombre del grupo lo teclea una persona en cada una de las cuatro, así que se
    /// compara con la misma manga ancha que los códigos de servicio.
    /// </summary>
    public static bool EsElMismoGrupo(string? uno, string? otro)
    {
        var a = Normalizar(uno);
        var b = Normalizar(otro);
        return a.Length > 0 && a == b;
    }

    /// <summary>
    /// Reparte los proyectos en líneas de calendario: las enlazadas se juntan en una y
    /// las sueltas van cada una por su lado.
    /// </summary>
    public static IReadOnlyList<EntradaDeCalendario> Agrupar(IEnumerable<ResumenDeProyecto> proyectos)
    {
        var entradas = new List<EntradaDeCalendario>();
        var grupos = new Dictionary<string, List<ResumenDeProyecto>>(StringComparer.Ordinal);

        foreach (var proyecto in proyectos)
        {
            var clave = Normalizar(proyecto.Planificacion.Grupo);

            if (clave.Length == 0)
            {
                entradas.Add(new EntradaDeCalendario(proyecto, [proyecto]));
                continue;
            }

            if (!grupos.TryGetValue(clave, out var miembros)) grupos[clave] = miembros = [];
            miembros.Add(proyecto);
        }

        foreach (var miembros in grupos.Values)
            entradas.Add(new EntradaDeCalendario(Cabecera(miembros), miembros));

        return entradas;
    }

    /// <summary>
    /// Cuál de las enlazadas manda: <b>la que lleva las fechas</b>. Si las llevan varias
    /// —no debería, pero nadie lo impide— manda la que empieza antes, que es la que
    /// describe cuándo arranca el trabajo. Y si ninguna las tiene, la primera por código,
    /// para que el grupo salga siempre en el mismo sitio de la banda de sin planificar.
    /// </summary>
    private static ResumenDeProyecto Cabecera(List<ResumenDeProyecto> miembros)
        => miembros.Where(m => m.Planificacion.HayFechas)
                   .OrderBy(m => m.Planificacion.Inicio)
                   .FirstOrDefault()
           ?? miembros.OrderBy(m => m.CodigoServicio, StringComparer.CurrentCultureIgnoreCase).First();

    private static string Normalizar(string? grupo)
        => string.IsNullOrWhiteSpace(grupo)
            ? ""
            : new string([.. grupo.Where(c => c is not (' ' or '-' or '_' or '.' or '/'))]).ToUpperInvariant();
}
