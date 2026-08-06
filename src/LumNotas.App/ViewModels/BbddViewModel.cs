using System.Collections.ObjectModel;
using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// El listado de todas las tomas de notas del laboratorio, para encontrar una de hace
/// meses sin ir preguntando a los compañeros.
/// <para>
/// <b>Solo lee.</b> No es una base de datos aparte: es una lente sobre los mismos
/// <c>.lmnlab</c> que ya escanea el tablero. Un fichero índice que hubiera que mantener
/// sería una segunda verdad que se desincroniza, y eso ya se descartó dos veces (DD‑27,
/// DD‑89).
/// </para>
/// <para>
/// <b>Ya no filtra por su cuenta.</b> Tuvo su propia caja de buscar y sus desplegables de
/// IP, IK y acreditación; ahora los filtros son un solo juego para las cuatro vistas y
/// viven en el botón «Filtros». Elegir dos veces lo mismo en dos sitios distintos solo
/// servía para que discreparan.
/// </para>
/// <para>
/// A cambio, aquí manda también el estado: para encontrar algo archivado hay que poner
/// «Estado» en «(todos)». El botón lo dice sin abrirlo.
/// </para>
/// </summary>
public sealed class BbddViewModel : ObservableObject
{
    /// <summary>Las filas que se ven ahora mismo. Se sustituyen en bloque (ver
    /// <see cref="ColeccionEnBloque{T}"/>): son cientos y se rehacen enteras.</summary>
    public ColeccionEnBloque<FilaDeBbdd> Filas { get; } = [];

    /// <summary>Cuántas tomas de notas se están enseñando.</summary>
    public string Recuento => $"{Filas.Count} toma{(Filas.Count == 1 ? "" : "s")} de notas";

    public bool NoHayNada => Filas.Count == 0;

    /// <summary>Lo contrario, porque el convertidor de WPF a visibilidad no sabe invertir.</summary>
    public bool HayAlgo => Filas.Count > 0;

    /// <summary>Abrir la toma de notas de una fila; lo resuelve la ventana.</summary>
    public Action<string>? Abrir { get; set; }

    /// <summary>
    /// Recibe lo que ya ha pasado el filtro. Las ilegibles se quedan fuera: una fila sin
    /// datos que leer no es un resultado de búsqueda, y la portada ya avisa de cuántas hay.
    /// </summary>
    public void Cargar(IReadOnlyList<ResumenDeProyecto> proyectos)
    {
        Filas.Reemplazar(proyectos.Where(p => p.Error is null)
                                  .OrderByDescending(p => p.Modificado)
                                  .Select(p => new FilaDeBbdd(p)));

        Notificar(nameof(Recuento));
        Notificar(nameof(NoHayNada));
        Notificar(nameof(HayAlgo));
    }
}

/// <summary>Una línea del listado. Solo texto: aquí no se edita nada.</summary>
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

    private static string Periodo(Planificacion plan)
    {
        if (plan.EnsayoDesde is not { } desde) return "";
        var hasta = plan.EnsayoHasta ?? desde;

        return desde.Date == hasta.Date
            ? desde.ToString("dd/MM/yyyy")
            : $"{desde:dd/MM/yyyy} – {hasta:dd/MM/yyyy}";
    }

    /// <summary>
    /// Lo archivado se dice como tal: es lo que explica que un servicio no aparezca en el
    /// tablero, y sin ello el listado y el tablero parecerían contradecirse.
    /// </summary>
    private static string EstadoDe(ResumenDeProyecto proyecto)
        => proyecto.Planificacion.Archivado
            ? "Archivado"
            : Planificacion.EtiquetaDe(proyecto.Planificacion.Estado);
}
