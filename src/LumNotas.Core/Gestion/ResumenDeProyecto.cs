using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Gestion;

/// <summary>Una sección con trabajo pendiente, para el tablero de gestión.</summary>
public sealed record SeccionPendiente(string Titulo, int Pendientes, int Aplicables);

/// <summary>
/// Estado de un proyecto visto desde fuera, sin abrirlo. Es lo que alimenta el tablero:
/// una columna por proyecto y una tarjeta por sección pendiente.
/// </summary>
public sealed record ResumenDeProyecto
{
    public required string Ruta { get; init; }
    public required string Nombre { get; init; }
    public string CodigoServicio { get; init; } = "";
    public string Tecnico { get; init; } = "";
    public int NumeroMuestras { get; init; }
    public DateTime Modificado { get; init; }

    /// <summary>Normas que lleva el servicio, para poder filtrar el calendario por ellas.</summary>
    public IReadOnlyList<string> Normas { get; init; } = [];

    /// <summary>Fechas y estado del servicio. Vacía en los proyectos aún sin planificar.</summary>
    public Planificacion Planificacion { get; init; } = new();

    public IReadOnlyList<SeccionPendiente> SeccionesPendientes { get; init; } = [];

    /// <summary>
    /// El avance se cuenta <b>por secciones</b>, no por apartados: la sección 7 cuenta
    /// como una aunque tenga trece apartados dentro. Es la vista que necesita el PM.
    /// </summary>
    public int SeccionesCompletadas { get; init; }
    public int SeccionesAplicables { get; init; }

    /// <summary>Motivo por el que no se pudo leer. Si tiene valor, el resto no es fiable.</summary>
    public string? Error { get; init; }

    public bool Terminado => Error is null && SeccionesAplicables > 0
                             && SeccionesCompletadas == SeccionesAplicables;

    public string Avance => Error is not null
        ? "no se pudo leer"
        : $"{SeccionesCompletadas}/{SeccionesAplicables} secciones";
}

/// <summary>Calcula el resumen de un proyecto reutilizando el motor de reglas.</summary>
public static class AnalizadorDeProyectos
{
    public static ResumenDeProyecto Analizar(
        PlantillaEnsayos plantilla, DatosProyecto datos, string ruta, DateTime modificado,
        Planificacion? planificacion = null)
        => Analizar([plantilla], datos, ruta, modificado, planificacion);

    /// <summary>
    /// Analiza el proyecto contra <b>todas las normas que lleva</b>.
    /// <para>
    /// La <b>principal</b> se detalla sección a sección, como siempre. Cada norma
    /// <b>añadida</b> —módulos LED 62031, grados IK— se resume en <b>una sola línea</b>,
    /// que desaparece cuando todas sus secciones están completas.
    /// </para>
    /// <para>
    /// Es lo que pidió el laboratorio: al responsable le interesa el detalle de lo que
    /// está ensayando y, de lo añadido, solo si queda algo por hacer. Desplegar la 62031
    /// entera dentro de un servicio de luminarias enterraba lo importante.
    /// </para>
    /// </summary>
    public static ResumenDeProyecto Analizar(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos, string ruta, DateTime modificado,
        Planificacion? planificacion = null)
    {
        var ordenadas = Ordenar(normas, datos);
        var pendientes = new List<SeccionPendiente>();
        var completadas = 0;
        var aplicables = 0;

        // La principal, sección a sección.
        foreach (var seccion in Contar(ordenadas[0], datos))
        {
            aplicables++;
            if (seccion.Pendientes == 0) completadas++;
            else pendientes.Add(seccion);
        }

        // Cada añadida, en una línea.
        foreach (var añadida in ordenadas.Skip(1))
        {
            var suyas = Contar(añadida, datos).ToList();
            if (suyas.Count == 0) continue;

            var pendientesEnLaNorma = suyas.Sum(s => s.Pendientes);

            aplicables++;

            if (pendientesEnLaNorma == 0) completadas++;
            else pendientes.Add(new SeccionPendiente(
                TituloDe(añadida), pendientesEnLaNorma, suyas.Sum(s => s.Aplicables)));
        }

        return new ResumenDeProyecto
        {
            Ruta = ruta,
            Nombre = Path.GetFileNameWithoutExtension(ruta),
            CodigoServicio = datos.CodigoServicio,
            Tecnico = datos.Tecnico1 ?? "",
            NumeroMuestras = datos.NumeroMuestras,
            Modificado = modificado,
            Normas = [.. datos.Normas.OrderBy(n => n)],
            Planificacion = planificacion ?? new Planificacion(),
            SeccionesPendientes = pendientes,
            SeccionesCompletadas = completadas,
            SeccionesAplicables = aplicables
        };
    }

    /// <summary>
    /// Las secciones de una norma que aportan algo: las que tienen al menos un apartado
    /// aplicable. Una sección entera que no aplica no cuenta para el avance.
    /// </summary>
    private static IEnumerable<SeccionPendiente> Contar(PlantillaEnsayos plantilla, DatosProyecto datos)
    {
        var motor = new MotorDeReglas(plantilla, datos);

        foreach (var seccion in plantilla.Secciones)
        {
            var visibles = seccion.Bloques.Where(b => EstadoDeApartado.EsVisible(motor, b)).ToList();
            var estados = visibles.Select(b => EstadoDeApartado.De(motor, datos, b)).ToList();

            var aplicables = estados.Count(e => e != EstadoApartado.NoAplica);
            if (aplicables == 0) continue;

            yield return new SeccionPendiente(
                seccion.Titulo, estados.Count(e => e == EstadoApartado.FaltanDatos), aplicables);
        }
    }

    /// <summary>
    /// La norma principal primero y las añadidas detrás.
    /// <para>
    /// <b>Lo dice el proyecto</b>: se apunta al elegirla y aquí solo se lee. Es un dato
    /// suyo, igual que el responsable, y no algo que haya que reconstruir cada vez.
    /// </para>
    /// </summary>
    private static IReadOnlyList<PlantillaEnsayos> Ordenar(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos)
    {
        if (normas.Count <= 1) return normas;

        // Si la que dice el proyecto ya no está entre las suyas —se quitó desde la toma de
        // notas— no vale de nada: se vuelve a deducir en lugar de detallar una norma que
        // el servicio ya no lleva.
        var principal = normas.FirstOrDefault(p => p.Meta.Id == datos.NormaPrincipal)
                        ?? Deducir(normas, datos);

        return [principal, .. normas.Where(p => !ReferenceEquals(p, principal))
                                    .OrderBy(p => p.Meta.Id, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Cuál era la principal en un proyecto guardado antes de que se apuntara.
    /// <para>
    /// Lo delata <b>cómo se nombran las muestras</b>: las de seguridad son
    /// <c>EBP_SAFE…</c> y las de IK <c>EBP_CLIM…</c>, y ese patrón lo fijó la norma con la
    /// que nació el proyecto. Cuando eso no lo aclara —varias normas comparten patrón—
    /// manda luminarias, que es la de uso más frecuente; y si tampoco está, el orden
    /// alfabético, que al menos es estable.
    /// </para>
    /// </summary>
    private static PlantillaEnsayos Deducir(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos)
    {
        var porPatron = normas
            .Where(p => p.Muestras.Identificador?.Patron == datos.PatronIdentificador)
            .ToList();

        return normas.FirstOrDefault(p => p.Meta.Id == "60598")
               ?? (porPatron.Count == 1 ? porPatron[0] : null)
               ?? normas.OrderBy(p => p.Meta.Id, StringComparer.Ordinal).First();
    }

    /// <summary>Cómo se llama la línea de una norma añadida.</summary>
    private static string TituloDe(PlantillaEnsayos plantilla)
        => string.IsNullOrWhiteSpace(plantilla.Meta.Titulo) ? plantilla.Meta.Id : plantilla.Meta.Titulo!;

    /// <summary>Resumen de un proyecto que no se pudo leer: el tablero lo muestra igualmente.</summary>
    public static ResumenDeProyecto NoLegible(string ruta, DateTime modificado, string motivo)
        => new()
        {
            Ruta = ruta,
            Nombre = Path.GetFileNameWithoutExtension(ruta),
            Modificado = modificado,
            Error = motivo
        };
}
