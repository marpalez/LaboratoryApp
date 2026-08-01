using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;

namespace LumNotas.Storage;

/// <summary>
/// Lee y escribe proyectos en un único fichero JSON por proyecto (DD-02).
/// <para>
/// La escritura es <b>atómica</b> —fichero temporal y reemplazo— porque los proyectos
/// viven en una carpeta sincronizada con OneDrive: una base de datos con bloqueo de
/// fichero se corrompería, mientras que el reemplazo completo se sincroniza bien y
/// además deja historial de versiones gratis.
/// </para>
/// </summary>
public sealed class RepositorioDeProyectos
{
    /// <summary>
    /// Extensión propia para que el proyecto se vea como un documento del laboratorio
    /// y no como un fichero técnico. Por dentro es JSON, igual que un .xlsx es un zip.
    /// </summary>
    public const string Extension = ".lumproj";

    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // El estado del proyecto se guarda por su nombre («pendienteCliente»), no por su
        // número: un fichero de laboratorio tiene que poder leerse sin la aplicación.
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public void Guardar(DatosProyecto datos, string ruta, string versionPlantilla)
    {
        var documento = new DocumentoProyecto
        {
            // La planificación no la escribe la toma de notas: se conserva la que haya
            // en el disco. Si no, un técnico con el proyecto abierto desde hace media
            // hora borraría al guardar las fechas que otro acaba de mover en el calendario.
            Planificacion = SoloSiTieneAlgo(LeerPlanificacion(ruta)),
            VersionPlantilla = versionPlantilla,
            GuardadoEl = DateTime.Now,
            CodigoServicio = datos.CodigoServicio,
            NumeroMuestras = datos.NumeroMuestras,
            Normas = [.. datos.Normas],
            PatronIdentificador = datos.PatronIdentificador,
            Selecciones = datos.VolcarSelecciones().ToDictionary(s => s.Campo, s => s.Valores.ToList()),
            Valores = [.. datos.Volcar().Select(v => new ValorGuardado
            {
                Ambito = v.Ambito,
                Campo = v.Campo,
                Muestra = v.Muestra,
                Tipo = TipoDe(v.Valor),
                Valor = ATexto(v.Valor)
            })],
            Na = datos.VolcarNa().Where(n => n.Valor).Select(n => n.Ambito).ToList(),
            Checklists = datos.VolcarChecklists().Where(c => c.Valor).Select(c => c.Ruta).ToList()
        };

        EscribirDeFormaAtomica(ruta, JsonSerializer.Serialize(documento, Opciones));
    }

    public DatosProyecto Cargar(string ruta) => CargarCompleto(ruta).Datos;

    /// <summary>
    /// Datos y planificación en una sola lectura. Lo usa el tablero, que recorre todos
    /// los proyectos de la carpeta y no puede permitirse abrir cada fichero dos veces.
    /// </summary>
    public (DatosProyecto Datos, Planificacion Planificacion) CargarCompleto(string ruta)
    {
        var documento = JsonSerializer.Deserialize<DocumentoProyecto>(File.ReadAllText(ruta), Opciones)
                        ?? throw new InvalidOperationException($"El proyecto '{ruta}' no se pudo leer.");

        var datos = new DatosProyecto
        {
            CodigoServicio = documento.CodigoServicio,
            NumeroMuestras = documento.NumeroMuestras,
            PatronIdentificador = documento.PatronIdentificador ?? DatosProyecto.PatronPorDefecto
        };

        foreach (var norma in documento.Normas) datos.Normas.Add(norma);

        foreach (var (campo, valores) in documento.Selecciones)
            foreach (var valor in valores) datos.Seleccion(campo).Add(valor);

        // Formato antiguo: la cabecera de luminarias iba en campos propios del documento.
        // Se sigue leyendo para que los proyectos ya guardados abran sin perder nada.
        foreach (var p in documento.Partes2) datos.Partes2.Add(p);
        foreach (var g in documento.IpSegundaCifra) datos.IpSegundaCifra.Add(g);
        foreach (var g in documento.IpPrimeraCifra) datos.IpPrimeraCifra.Add(g);
        if (documento.Clase is { } clase) datos.Clase = (Clase)clase;

        foreach (var v in documento.Valores)
            datos.Establecer(v.Ambito, v.Campo, DeTexto(v.Tipo, v.Valor), v.Muestra);

        foreach (var ambito in documento.Na) datos.CargarNa(ambito, true);
        foreach (var ruta2 in documento.Checklists) datos.CargarChecklist(ruta2, true);

        return (datos, documento.Planificacion ?? new Planificacion());
    }

    /// <summary>Una planificación en blanco no se escribe: solo añadiría ruido al fichero.</summary>
    private static Planificacion? SoloSiTieneAlgo(Planificacion planificacion)
        => planificacion.EsVacia ? null : planificacion;

    // ---- planificación (la línea de tiempo del tablero) --------------------

    /// <summary>
    /// Planificación guardada en un proyecto, o una vacía si no la tiene todavía o el
    /// fichero no se puede leer. Nunca lanza: la usa <see cref="Guardar"/> y un fallo
    /// aquí no puede impedir guardar el trabajo del técnico.
    /// </summary>
    public Planificacion LeerPlanificacion(string ruta)
    {
        try
        {
            if (!File.Exists(ruta)) return new Planificacion();
            var documento = JsonSerializer.Deserialize<DocumentoProyecto>(File.ReadAllText(ruta), Opciones);
            return documento?.Planificacion ?? new Planificacion();
        }
        catch
        {
            return new Planificacion();
        }
    }

    /// <summary>
    /// Cambia <b>solo</b> la planificación de un proyecto: relee el fichero, sustituye
    /// ese trozo y lo vuelve a escribir entero. Así el calendario puede mover fechas de
    /// un proyecto que no tiene abierto sin tocar ni un dato de ensayo.
    /// </summary>
    public void ActualizarPlanificacion(string ruta, Planificacion planificacion)
    {
        var documento = JsonSerializer.Deserialize<DocumentoProyecto>(File.ReadAllText(ruta), Opciones)
                        ?? throw new InvalidOperationException($"El proyecto '{ruta}' no se pudo leer.");

        documento.Planificacion = SoloSiTieneAlgo(planificacion);
        EscribirDeFormaAtomica(ruta, JsonSerializer.Serialize(documento, Opciones));
    }

    /// <summary>
    /// Escribe a un temporal en la misma carpeta y reemplaza. Si el proceso muere a
    /// mitad, el fichero original sigue intacto.
    /// </summary>
    private static void EscribirDeFormaAtomica(string ruta, string contenido)
    {
        var carpeta = Path.GetDirectoryName(Path.GetFullPath(ruta))!;
        Directory.CreateDirectory(carpeta);
        var temporal = Path.Combine(carpeta, Path.GetRandomFileName());

        File.WriteAllText(temporal, contenido, new System.Text.UTF8Encoding(false));

        if (File.Exists(ruta)) File.Replace(temporal, ruta, destinationBackupFileName: null);
        else File.Move(temporal, ruta);
    }

    private static string TipoDe(object? valor) => valor switch
    {
        null => "nulo",
        DateTime => "instante",
        bool => "bool",
        double or int or long or decimal => "numero",
        _ => "texto"
    };

    private static string? ATexto(object? valor) => valor switch
    {
        null => null,
        DateTime d => d.ToString("O", CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false",
        double d => d.ToString("R", CultureInfo.InvariantCulture),
        int i => i.ToString(CultureInfo.InvariantCulture),
        long l => l.ToString(CultureInfo.InvariantCulture),
        decimal m => m.ToString(CultureInfo.InvariantCulture),
        _ => valor.ToString()
    };

    private static object? DeTexto(string tipo, string? texto) => texto is null ? null : tipo switch
    {
        "instante" => DateTime.Parse(texto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        "bool" => bool.Parse(texto),
        "numero" => double.Parse(texto, CultureInfo.InvariantCulture),
        "nulo" => null,
        _ => texto
    };

    private sealed class DocumentoProyecto
    {
        public string Formato { get; init; } = "lumproj/1";
        public string VersionPlantilla { get; init; } = "";
        public DateTime GuardadoEl { get; init; }
        public string CodigoServicio { get; init; } = "";
        public int NumeroMuestras { get; init; } = 1;

        /// <summary>Normas que lleva el proyecto. Un servicio puede ensayarse contra varias.</summary>
        public List<string> Normas { get; init; } = [];

        public string? PatronIdentificador { get; init; }

        /// <summary>
        /// Fechas, estado y recepción de muestras. Es el único trozo del documento que
        /// se escribe por separado, desde el calendario del tablero.
        /// </summary>
        public Planificacion? Planificacion { get; set; }

        /// <summary>Selecciones múltiples de la cabecera, por id de campo de la plantilla.</summary>
        public Dictionary<string, List<string>> Selecciones { get; init; } = [];

        // --- formato antiguo: solo se leen, ya no se escriben ---
        public int? Clase { get; init; }
        public List<string> Partes2 { get; init; } = [];
        public List<string> IpSegundaCifra { get; init; } = [];
        public List<string> IpPrimeraCifra { get; init; } = [];

        public List<ValorGuardado> Valores { get; init; } = [];
        public List<string> Na { get; init; } = [];
        public List<string> Checklists { get; init; } = [];
    }

    private sealed class ValorGuardado
    {
        public string Ambito { get; init; } = "";
        public string Campo { get; init; } = "";
        public int Muestra { get; init; }
        public string Tipo { get; init; } = "texto";
        public string? Valor { get; init; }
    }
}
