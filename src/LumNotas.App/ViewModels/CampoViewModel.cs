using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Una celda editable: un campo de la plantilla para una muestra concreta.
/// Escribe directamente en el almacén y avisa al padre para reevaluar las reglas.
/// </summary>
public sealed class CeldaViewModel(
    DatosProyecto datos, string ambito, string campo, int muestra, string tipo, Action alCambiar)
    : ObservableObject
{
    public int Muestra => muestra;
    public string Tipo => tipo;

    /// <summary>
    /// Cabecera de la columna. Muestra el número real de la muestra, no su posición:
    /// si el servicio empieza en la 03, la primera columna pone «03» y no «1».
    /// </summary>
    public string EtiquetaMuestra => muestra == DatosProyecto.SinMuestra
        ? ""
        : datos.NumeroDeMuestra(muestra).ToString("00");

    /// <summary>Los campos booleanos se pintan como casilla, no como caja de texto.</summary>
    public bool EsBooleano => tipo == "booleano";
    public bool EsTexto => !EsBooleano;

    public bool Marcado
    {
        get => datos.Obtener(ambito, campo, muestra) is true;
        set
        {
            datos.Establecer(ambito, campo, value, muestra);
            Notificar();
            alCambiar();
        }
    }

    public string Texto
    {
        get => Formatear(datos.Obtener(ambito, campo, muestra));
        set
        {
            datos.Establecer(ambito, campo, Interpretar(value), muestra);
            Notificar();
            alCambiar();
        }
    }

    private static string Formatear(object? valor) => valor switch
    {
        null => "",
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("dd/MM/yyyy") : d.ToString("dd/MM/yyyy HH:mm"),
        double n => n.ToString("0.###"),
        _ => valor.ToString() ?? ""
    };

    private object? Interpretar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        texto = texto.Trim();

        if (tipo is "instante" or "fecha" && DateTime.TryParse(texto, out var fecha)) return fecha;
        if (tipo is "numero" or "entero" && double.TryParse(texto, out var numero)) return numero;
        if (tipo is "numero" or "entero") return null;   // no se guarda basura en un campo numérico
        return texto;
    }
}

/// <summary>Una fila del formulario: la etiqueta y una celda por muestra.</summary>
public sealed class CampoViewModel : ObservableObject
{
    /// <param name="porMuestra">
    /// Sobrescribe el <c>porMuestra</c> del campo. Hace falta para los grupos repetidos:
    /// la marca está en el grupo padre (tornillos, tamaño, ratings…) y no en cada hijo.
    /// </param>
    public CampoViewModel(DatosProyecto datos, string ambito, Campo campo, string rutaCampo,
                          int numeroMuestras, Action alCambiar, string? etiquetaPrefijo = null,
                          bool? porMuestra = null)
    {
        Etiqueta = etiquetaPrefijo is null ? campo.Etiqueta : $"{etiquetaPrefijo} · {campo.Etiqueta}";
        Unidad = campo.Unidad;
        Nota = campo.Nota;
        PorMuestra = porMuestra ?? campo.PorMuestra;

        var muestras = PorMuestra ? Enumerable.Range(1, numeroMuestras) : [DatosProyecto.SinMuestra];
        Celdas = [.. muestras.Select(m => new CeldaViewModel(datos, ambito, rutaCampo, m, campo.Tipo, alCambiar))];
    }

    public string Etiqueta { get; }
    public string? Unidad { get; }
    public string? Nota { get; }
    public bool PorMuestra { get; }
    public IReadOnlyList<CeldaViewModel> Celdas { get; }

    public string EtiquetaConUnidad => Unidad is null ? Etiqueta : $"{Etiqueta} ({Unidad})";
}

/// <summary>Una casilla de un checklist.</summary>
public sealed class OpcionViewModel(
    DatosProyecto datos, string ambito, string checklist, OpcionChecklist opcion, Action alCambiar)
    : ObservableObject
{
    public string Etiqueta => opcion.Etiqueta;

    public bool Marcada
    {
        get => datos.Marcada(ambito, checklist, opcion.Id);
        set
        {
            datos.Marcar(ambito, checklist, opcion.Id, value);
            Notificar();
            alCambiar();
        }
    }
}

/// <summary>Un equipo del catálogo, marcable como utilizado en el apartado.</summary>
public sealed class EquipoViewModel(
    DatosProyecto datos, string ambito, EquipoDelCatalogo equipo, Action alCambiar) : ObservableObject
{
    public const string Checklist = "equipos";

    public string Codigo => equipo.CodigoLiteral;
    public string Descripcion => equipo.Descripcion;

    public bool Utilizado
    {
        get => datos.Marcada(ambito, Checklist, equipo.Id);
        set
        {
            datos.Marcar(ambito, Checklist, equipo.Id, value);
            Notificar();
            alCambiar();
        }
    }
}

public sealed class ChecklistViewModel(
    string? etiqueta, string? nota, IReadOnlyList<OpcionViewModel> opciones,
    bool esNa = false, bool esExencion = false)
{
    public string? Etiqueta { get; } = etiqueta;
    public string? Nota { get; } = nota;
    public IReadOnlyList<OpcionViewModel> Opciones { get; } = opciones;
    public bool TieneEtiqueta => !string.IsNullOrWhiteSpace(Etiqueta);
    public bool TieneNota => !string.IsNullOrWhiteSpace(Nota);

    /// <summary>Marca el «no aplica» del apartado, que se pinta antes que el resto.</summary>
    public bool EsNa { get; } = esNa;

    /// <summary>
    /// Exime al subapartado por una característica de la luminaria («NO tiene tornillos»).
    /// Va tras las notas y antes de los campos: condiciona si hay que rellenarlos.
    /// </summary>
    public bool EsExencion { get; } = esExencion;
}
