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
    {
        var motor = new MotorDeReglas(plantilla, datos);
        var pendientes = new List<SeccionPendiente>();
        var completadas = 0;
        var aplicables = 0;

        foreach (var seccion in plantilla.Secciones)
        {
            var visibles = seccion.Bloques.Where(b => EstadoDeApartado.EsVisible(motor, b)).ToList();
            var estados = visibles.Select(b => EstadoDeApartado.De(motor, datos, b)).ToList();

            var aplicablesEnSeccion = estados.Count(e => e != EstadoApartado.NoAplica);
            var pendientesEnSeccion = estados.Count(e => e == EstadoApartado.FaltanDatos);

            // Una sección entera sin nada aplicable no cuenta para el avance.
            if (aplicablesEnSeccion == 0) continue;

            aplicables++;
            if (pendientesEnSeccion == 0) completadas++;
            else pendientes.Add(new SeccionPendiente(seccion.Titulo, pendientesEnSeccion, aplicablesEnSeccion));
        }

        return new ResumenDeProyecto
        {
            Ruta = ruta,
            Nombre = Path.GetFileNameWithoutExtension(ruta),
            CodigoServicio = datos.CodigoServicio,
            Tecnico = datos.Obtener("proyecto", "tecnico1") as string ?? "",
            NumeroMuestras = datos.NumeroMuestras,
            Modificado = modificado,
            Normas = [.. datos.Normas.OrderBy(n => n)],
            Planificacion = planificacion ?? new Planificacion(),
            SeccionesPendientes = pendientes,
            SeccionesCompletadas = completadas,
            SeccionesAplicables = aplicables
        };
    }

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
