using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Un apartado de ensayo tal como se ve en pantalla. Se construye a partir de la
/// plantilla, no de código escrito a mano: añadir un apartado nuevo al JSON lo hace
/// aparecer aquí sin tocar la aplicación.
/// </summary>
public sealed class BloqueViewModel : ObservableObject
{
    private readonly MotorDeReglas _motor;
    private readonly DatosProyecto _datos;
    private readonly Bloque _bloque;
    private readonly Action _alCambiar;

    public BloqueViewModel(MotorDeReglas motor, DatosProyecto datos, Seccion seccion, Bloque bloque,
                           Action alCambiar, CatalogoDeEquipos? catalogo = null)
    {
        _motor = motor;
        _datos = datos;
        _bloque = bloque;
        _alCambiar = alCambiar;

        Seccion = seccion.Titulo;
        Codigo = bloque.Codigo;
        Titulo = bloque.Titulo;
        Notas = bloque.Notas;
        TieneAmbiente = bloque.Ambiente is not null;
        PuedeSerNa = bloque.Na is not null;
        Peso = bloque.PesoAvance;

        Grupos = [.. ConstruirGrupos(bloque, datos, alCambiar)];
        Ambiente = ConstruirAmbiente(bloque.Id, bloque.Ambiente, datos, alCambiar);

        // Comentarios del apartado: en el Excel hay uno por apartado (54 en total).
        TieneComentarios = bloque.Comentarios;
        Comentarios = new CeldaViewModel(datos, bloque.Id, "comentarios",
            DatosProyecto.SinMuestra, "textoLargo", alCambiar);

        // Equipos utilizados, tomados del catálogo del laboratorio.
        var grupo = (catalogo ?? CatalogoDeEquipos.Vacio).Grupo(bloque.Equipos);
        TituloEquipos = grupo?.Titulo ?? "";
        NotasEquipos = grupo?.Notas ?? [];
        PermiteOtrosEquipos = grupo?.PermiteOtros ?? false;
        Equipos = grupo is null
            ? []
            : [.. grupo.Equipos.Select(e => new EquipoViewModel(datos, bloque.Id, e, alCambiar))];
        OtrosEquipos = new CeldaViewModel(datos, bloque.Id, "equipos.otros",
            DatosProyecto.SinMuestra, "texto", alCambiar);
    }

    public string Seccion { get; }
    public string Codigo { get; }
    public string Titulo { get; }
    public IReadOnlyList<string> Notas { get; }
    public bool TieneAmbiente { get; }
    public bool PuedeSerNa { get; }
    public int? Peso { get; }
    public IReadOnlyList<GrupoViewModel> Grupos { get; }
    public IReadOnlyList<CampoViewModel> Ambiente { get; }

    public bool TieneComentarios { get; }
    public CeldaViewModel Comentarios { get; }

    public string TituloEquipos { get; }
    public IReadOnlyList<string> NotasEquipos { get; }
    public bool PermiteOtrosEquipos { get; }
    public IReadOnlyList<EquipoViewModel> Equipos { get; }
    public CeldaViewModel OtrosEquipos { get; }
    public bool TieneEquipos => Equipos.Count > 0;

    public string Encabezado => string.IsNullOrWhiteSpace(Codigo) || Codigo == Titulo
        ? Titulo
        : $"{Codigo} · {Titulo}";

    public bool NoAplica
    {
        get => _datos.Na($"{_bloque.Id}/{_bloque.Na?.Id ?? "na"}");
        set
        {
            _datos.EstablecerNa($"{_bloque.Id}/{_bloque.Na?.Id ?? "na"}", value);
            Notificar();
            Notificar(nameof(Aplica));
            _alCambiar();
        }
    }

    /// <summary>
    /// Mientras el apartado no aplique, sus campos se desactivan: marcar N/A y poder
    /// seguir escribiendo dejaba proyectos que decían a la vez que no aplica y traían
    /// datos de ensayo.
    /// </summary>
    public bool Aplica => !NoAplica;

    /// <summary>
    /// Si el apartado se muestra. Los ensayos de las partes -2 solo aparecen cuando su
    /// parte está marcada en los datos del proyecto; el resto se muestra siempre.
    /// </summary>
    public bool Visible => _bloque.VisibleSi is null || Regla(_bloque.VisibleSi, siFalla: true);

    /// <summary>Estado del apartado. La lógica vive en Core: la comparte el tablero de gestión.</summary>
    public EstadoApartado Estado
    {
        get
        {
            try { return EstadoDeApartado.De(_motor, _datos, _bloque); }
            catch (Exception ex)
            {
                FalloDeRegla ??= $"{Codigo}: {ex.Message}";
                return EstadoApartado.FaltanDatos;
            }
        }
    }

    /// <summary>
    /// Evalúa una regla sin dejar que una excepción tumbe la ventana. Si algo falla,
    /// se anota el apartado y la regla culpables y se sigue: es mucho más útil ver
    /// «Error en la regla X» en el índice que perder el trabajo por un cierre.
    /// </summary>
    private bool Regla(string id, bool siFalla)
    {
        try
        {
            return _motor.EsVerdadera(id);
        }
        catch (Exception ex)
        {
            FalloDeRegla ??= $"{Codigo} · regla {id}: {ex.Message}";
            return siFalla;
        }
    }

    /// <summary>Primer fallo de evaluación detectado en este apartado, si hubo alguno.</summary>
    public string? FalloDeRegla { get; private set; }

    public bool HayFalloDeRegla => FalloDeRegla is not null;

    public string EstadoTexto => Estado switch
    {
        EstadoApartado.NoAplica => "No aplica",
        EstadoApartado.Completo => "Completo",
        EstadoApartado.FaltanDatos => "Faltan datos",
        _ => ""
    };

    /// <summary>Avisos activos del apartado, con el texto que veía el técnico en el Excel.</summary>
    public IReadOnlyList<string> Avisos =>
    [
        .. PlantillaEnsayos.ReglasDe(_bloque)
            .Where(r => r.Tipo == "aviso" && r.Texto is not null && Regla(r.Id, siFalla: false))
            .Select(r => r.Texto!)
    ];

    public bool HayAvisos => Avisos.Count > 0;

    public void Refrescar()
    {
        FalloDeRegla = null;
        Notificar(nameof(Visible));
        Notificar(nameof(FalloDeRegla));
        Notificar(nameof(HayFalloDeRegla));
        Notificar(nameof(Estado));
        Notificar(nameof(EstadoTexto));
        Notificar(nameof(Avisos));
        Notificar(nameof(HayAvisos));
        Notificar(nameof(NoAplica));
        Notificar(nameof(Aplica));

        foreach (var grupo in Grupos) grupo.Refrescar();
    }

    private static IEnumerable<GrupoViewModel> ConstruirGrupos(Bloque bloque, DatosProyecto datos, Action alCambiar)
    {
        // Un checklist que alguna regla usa como «siNoAplica» es una exención del
        // subapartado («La luminaria NO tiene tornillos»), no un dato más.
        var exenciones = PlantillaEnsayos.ReglasDe(bloque)
            .Select(r => r.SiNoAplica)
            .Where(id => id is not null)
            .ToHashSet(StringComparer.Ordinal)!;

        var principal = Construir(bloque.Id, bloque.Titulo, bloque.Campos, bloque.Checklists, bloque.Notas,
                                  comentarios: false, ambiente: null, exenciones, datos, alCambiar);
        if (principal.TieneContenido) yield return principal;

        // Cada subapartado lleva sus propias condiciones de ensayo: los tres de la
        // sección 12 o los cuatro de calentamiento tienen fecha propia, no la del padre.
        foreach (var sub in bloque.SubBloques)
        {
            var grupo = Construir(sub.Id, sub.Titulo, sub.Campos, sub.Checklists, sub.Notas,
                                  sub.Comentarios, sub.Ambiente, exenciones, datos, alCambiar);
            if (grupo.TieneContenido) yield return grupo;
        }
    }

    private static GrupoViewModel Construir(
        string ambito, string titulo, IReadOnlyList<Campo> campos, IReadOnlyList<Checklist> checklists,
        IReadOnlyList<string> notas, bool comentarios, Ambiente? ambiente,
        IReadOnlySet<string?> exenciones, DatosProyecto datos, Action alCambiar)
    {
        var filas = new List<CampoViewModel>();
        var repetidos = new List<GrupoRepetidoViewModel>();

        foreach (var campo in campos)
        {
            if (campo.Tipo == "derivado") continue;   // se muestra donde se declara

            if (campo.Tipo == "grupoRepetido")
                repetidos.Add(new GrupoRepetidoViewModel(datos, ambito, campo, datos.NumeroMuestras, alCambiar));
            else
                filas.Add(new CampoViewModel(datos, ambito, campo, campo.Id, datos.NumeroMuestras, alCambiar));
        }

        // Un checklist con una sola opción llamada «na» es el «no aplica» del apartado.
        var listas = checklists
            .Select(c =>
            {
                var esNa = c.Opciones.Count == 1 && c.Opciones[0].Id == "na";
                return new ChecklistViewModel(c.Etiqueta, c.Nota,
                    [.. c.Opciones.Select(o => new OpcionViewModel(datos, ambito, c.Id, o, alCambiar))],
                    esNa: esNa,
                    esExencion: !esNa && exenciones.Contains(c.Id));
            })
            .ToList();

        var caja = comentarios
            ? new CeldaViewModel(datos, ambito, "comentarios", DatosProyecto.SinMuestra, "textoLargo", alCambiar)
            : null;

        var condiciones = ConstruirAmbiente(ambito, ambiente, datos, alCambiar);

        return new GrupoViewModel(titulo, filas, listas, notas, caja, condiciones, repetidos);
    }

    /// <summary>
    /// Condiciones de ensayo: T, H y fecha. Cuando el apartado declara
    /// <c>fechaPorMuestra</c> se añade además una fila con una fecha por muestra, tal
    /// como en el Excel («si hay más de una muestra marcar las fechas a la derecha»).
    /// </summary>
    private static IReadOnlyList<CampoViewModel> ConstruirAmbiente(
        string ambito, Ambiente? ambiente, DatosProyecto datos, Action alCambiar)
    {
        if (ambiente is not { } a) return [];

        var campos = new List<CampoViewModel>();
        if (a.Temperatura) campos.Add(Uno("ambiente.temperatura", "T", "ºC", "numero", porMuestra: false));
        if (a.Humedad) campos.Add(Uno("ambiente.humedad", "H", "%", "numero", porMuestra: false));
        if (a.Fecha) campos.Add(Uno("ambiente.fecha", "Fecha", null, "fecha", porMuestra: false));
        if (a.FechaPorMuestra) campos.Add(Uno("ambiente.fecha", "Fecha por muestra", null, "fecha", porMuestra: true));
        return campos;

        CampoViewModel Uno(string id, string etiqueta, string? unidad, string tipo, bool porMuestra)
            => new(datos, ambito,
                new Campo { Id = id, Etiqueta = etiqueta, Unidad = unidad, Tipo = tipo, PorMuestra = porMuestra },
                id, datos.NumeroMuestras, alCambiar);
    }
}

public sealed class GrupoViewModel(
    string titulo,
    IReadOnlyList<CampoViewModel> campos,
    IReadOnlyList<ChecklistViewModel> checklists,
    IReadOnlyList<string> notas,
    CeldaViewModel? comentarios = null,
    IReadOnlyList<CampoViewModel>? ambiente = null,
    IReadOnlyList<GrupoRepetidoViewModel>? repetidos = null) : ObservableObject
{
    public string Titulo { get; } = titulo;
    public IReadOnlyList<CampoViewModel> Campos { get; } = campos;

    /// <summary>
    /// El N/A del subapartado va justo bajo su título: es lo primero que decide el
    /// técnico, así que no tiene sentido pintarlo al final, después de los campos.
    /// </summary>
    public IReadOnlyList<ChecklistViewModel> ChecklistsNa { get; } = [.. checklists.Where(c => c.EsNa)];

    /// <summary>
    /// Exenciones por características de la luminaria. Van tras las notas y antes de los
    /// campos, porque deciden si hay algo que rellenar.
    /// </summary>
    public IReadOnlyList<ChecklistViewModel> ChecklistsExencion { get; } = [.. checklists.Where(c => c.EsExencion)];

    public IReadOnlyList<ChecklistViewModel> Checklists { get; } =
        [.. checklists.Where(c => !c.EsNa && !c.EsExencion)];

    public bool TieneNa => ChecklistsNa.Count > 0;
    public bool TieneExencion => ChecklistsExencion.Count > 0;
    public IReadOnlyList<string> Notas { get; } = notas;
    public CeldaViewModel? Comentarios { get; } = comentarios;
    public IReadOnlyList<CampoViewModel> Ambiente { get; } = ambiente ?? [];

    /// <summary>Grupos repetidos (tornillos, uniones…), cada uno con sus propios bloques.</summary>
    public IReadOnlyList<GrupoRepetidoViewModel> Repetidos { get; } = repetidos ?? [];

    /// <summary>Cabeceras M1…Mn para saber a qué muestra corresponde cada columna.</summary>
    public IReadOnlyList<string> Muestras { get; } =
        campos.Concat(ambiente ?? []).FirstOrDefault(c => c.PorMuestra) is { } conMuestras
            ? [.. conMuestras.Celdas.Select(c => c.EtiquetaMuestra)]
            : [];

    public bool TieneMuestras => Muestras.Count > 0;

    public bool TieneContenido =>
        Campos.Count > 0 || Checklists.Count > 0 || Comentarios is not null
        || Ambiente.Count > 0 || Repetidos.Count > 0;

    public bool TieneNotas => Notas.Count > 0;
    public bool TieneComentarios => Comentarios is not null;
    public bool TieneAmbiente => Ambiente.Count > 0;

    /// <summary>
    /// El subapartado aplica mientras no se marque su «no aplica» ni ninguna exención
    /// («La luminaria NO tiene tornillos»). Cuando no aplica, sus campos se desactivan:
    /// las casillas que lo deciden siguen activas para poder volver atrás.
    /// </summary>
    public bool Aplica => !ChecklistsNa.Concat(ChecklistsExencion)
                              .SelectMany(c => c.Opciones)
                              .Any(o => o.Marcada);

    public void Refrescar() => Notificar(nameof(Aplica));
}

