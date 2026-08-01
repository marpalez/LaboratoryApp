namespace LumNotas.Core.Datos;

/// <summary>
/// Datos de un proyecto de toma de notas. Es un almacén plano: las claves llevan
/// el bloque, la ruta del campo y la muestra, de modo que añadir campos a la
/// plantilla no obliga a tocar el modelo de datos.
/// </summary>
public sealed class DatosProyecto
{
    /// <summary>Muestra que representa "sin muestra": datos generales del bloque.</summary>
    public const int SinMuestra = 0;

    private readonly Dictionary<Clave, object?> _valores = [];
    private readonly Dictionary<string, bool> _na = [];
    private readonly Dictionary<string, bool> _checklists = [];
    private readonly Dictionary<string, HashSet<string>> _selecciones = [];

    public string CodigoServicio { get; set; } = "";
    public int NumeroMuestras { get; set; } = 1;

    /// <summary>
    /// Normas que lleva este proyecto (los <c>meta.id</c> de las plantillas). Un servicio
    /// puede ensayarse contra varias a la vez: 60598-1, 62031, 60529 e IK 62262.
    /// </summary>
    public HashSet<string> Normas { get; } = [];

    public IEnumerable<int> Muestras => Enumerable.Range(1, NumeroMuestras);

    // ---- selecciones múltiples de la cabecera ------------------------------

    /// <summary>
    /// Opciones marcadas en un campo de selección múltiple de la cabecera, por id de
    /// campo de la plantilla. Es genérico a propósito: cada norma tiene los suyos
    /// —partes ‑2 en luminarias, clasificación del módulo en 62031, grados IP e IK—
    /// y ninguno debe estar escrito en el modelo de datos.
    /// </summary>
    public HashSet<string> Seleccion(string campo)
    {
        if (!_selecciones.TryGetValue(campo, out var valores))
            _selecciones[campo] = valores = [];
        return valores;
    }

    /// <summary>
    /// Grados marcados en un campo de cabecera, vengan del proyecto o de cada muestra.
    /// Hay normas que declaran el grado IP objetivo por muestra —la 60529, porque un
    /// mismo servicio puede traer productos con grados distintos— y las reglas tienen
    /// que verlo igual que si fuera del proyecto.
    /// </summary>
    public IEnumerable<string> GradosDe(string campo)
        => Seleccion(campo).Concat(
            Muestras.Select(m => Obtener("proyecto", campo, m) as string)
                    .OfType<string>()
                    .Where(v => !string.IsNullOrWhiteSpace(v)));

    /// <summary>
    /// Grados que aplican a una muestra concreta: el suyo si la norma los declara por
    /// muestra y, si no lo tiene, los del proyecto. Lo usan los cálculos que dependen
    /// del objetivo de esa muestra, como la altura efectiva del arco de lluvia.
    /// </summary>
    public IEnumerable<string> GradosDe(string campo, int muestra)
        => Obtener("proyecto", campo, muestra) is string valor && !string.IsNullOrWhiteSpace(valor)
            ? [valor]
            : Seleccion(campo);

    // Atajos de luminarias: son el mismo almacén genérico con el nombre que usa la
    // norma 60598, para que los predicados y el informe se lean con naturalidad.
    public HashSet<string> Partes2 => Seleccion("partes2");
    public HashSet<string> IpSegundaCifra => Seleccion("ipSegundaCifra");
    public HashSet<string> IpPrimeraCifra => Seleccion("ipPrimeraCifra");

    /// <summary>Clase de aislamiento (60598). Vive en el almacén general, no aparte.</summary>
    public Clase Clase
    {
        get => (Obtener("proyecto", "clase") as string) switch
        {
            "II" => Clase.II,
            "III" => Clase.III,
            _ => Clase.I
        };
        set => Establecer("proyecto", "clase", value switch
        {
            Clase.II => "II",
            Clase.III => "III",
            _ => "I"
        });
    }

    // ---- identificación de las muestras -----------------------------------

    /// <summary>
    /// Patrón del identificador de muestra, tomado de <c>muestras.identificador</c> de la
    /// plantilla. En luminarias es <c>EBP_SAFE{codigoServicio}</c>; en IK 62262 el
    /// laboratorio usa <c>EBP_CLIM{codigoServicio}</c>.
    /// </summary>
    public string PatronIdentificador { get; set; } = PatronPorDefecto;

    public const string PatronPorDefecto = "EBP_SAFE{codigoServicio}";

    /// <summary>
    /// Número asignado a una muestra. Por defecto coincide con su posición, pero el
    /// técnico puede empezar en otro —la primera muestra del servicio puede ser la 03—
    /// y entonces las siguientes van consecutivas.
    /// </summary>
    public int NumeroDeMuestra(int posicion)
        => Numero("proyecto", "muestra.numero", posicion) is { } n ? (int)n : posicion;

    public void EstablecerNumeroDeMuestra(int posicion, int numero)
        => Establecer("proyecto", "muestra.numero", (double)numero, posicion);

    /// <summary>Identificador completo, p. ej. <c>EBP_SAFE12345202603</c>.</summary>
    public string IdentificadorDeMuestra(int posicion)
        => PatronIdentificador.Replace("{codigoServicio}", CodigoServicio)
           + NumeroDeMuestra(posicion).ToString("00");

    // ---- valores de campo -------------------------------------------------

    public void Establecer(string bloque, string campo, object? valor, int muestra = SinMuestra)
        => _valores[new Clave(bloque, campo, muestra)] = valor;

    public object? Obtener(string bloque, string campo, int muestra = SinMuestra)
        => _valores.TryGetValue(new Clave(bloque, campo, muestra), out var v) ? v : null;

    public double? Numero(string bloque, string campo, int muestra = SinMuestra)
        => Obtener(bloque, campo, muestra) switch
        {
            null => null,
            double d => d,
            int i => i,
            long l => l,
            decimal m => (double)m,
            string s when double.TryParse(s, out var p) => p,
            _ => null
        };

    public DateTime? Instante(string bloque, string campo, int muestra = SinMuestra)
        => Obtener(bloque, campo, muestra) as DateTime?;

    /// <summary>
    /// Valores de un campo (o de un grupo repetido, por prefijo) en una muestra.
    /// Para un grupo repetido "tornillos" devuelve todo lo guardado bajo "tornillos[...]".
    /// </summary>
    public IEnumerable<object?> ValoresDe(string bloque, string campo, int muestra)
    {
        var prefijo = campo + "[";
        foreach (var (clave, valor) in _valores)
        {
            if (clave.Bloque != bloque || clave.Muestra != muestra) continue;
            if (clave.Campo == campo || clave.Campo.StartsWith(prefijo, StringComparison.Ordinal))
                yield return valor;
        }
    }

    /// <summary>Igual que <see cref="ValoresDe"/> pero para todas las muestras del proyecto.</summary>
    public IEnumerable<object?> ValoresDeTodasLasMuestras(string bloque, string campo)
        => Muestras.SelectMany(m => ValoresDe(bloque, campo, m));

    /// <summary>
    /// Mayor índice con datos de un grupo repetido, o -1 si no hay ninguno. Sirve para
    /// que un sexto tornillo añadido a mano siga apareciendo al reabrir el proyecto.
    /// </summary>
    public int MaximoIndiceDe(string bloque, string campo)
    {
        var prefijo = campo + "[";
        var maximo = -1;

        foreach (var (clave, valor) in _valores)
        {
            if (clave.Bloque != bloque || valor is null) continue;
            if (!clave.Campo.StartsWith(prefijo, StringComparison.Ordinal)) continue;

            var cierre = clave.Campo.IndexOf(']', prefijo.Length);
            if (cierre < 0) continue;

            if (int.TryParse(clave.Campo[prefijo.Length..cierre], out var indice) && indice > maximo)
                maximo = indice;
        }

        return maximo;
    }

    // ---- N/A --------------------------------------------------------------

    public void EstablecerNa(string ambito, bool valor) => _na[ambito] = valor;

    public bool Na(string ambito) => _na.TryGetValue(ambito, out var v) && v;

    // ---- checklists -------------------------------------------------------

    public void Marcar(string bloque, string checklist, string opcion, bool valor = true)
        => _checklists[$"{bloque}/{checklist}/{opcion}"] = valor;

    public bool Marcada(string bloque, string checklist, string opcion)
        => _checklists.TryGetValue($"{bloque}/{checklist}/{opcion}", out var v) && v;

    public int Marcadas(string bloque, string checklist, IEnumerable<string> opciones)
        => opciones.Count(o => Marcada(bloque, checklist, o));

    // ---- volcado y carga (lo usa LumNotas.Storage) ------------------------

    public IEnumerable<(string Ambito, string Campo, int Muestra, object? Valor)> Volcar()
        => _valores.Select(p => (p.Key.Bloque, p.Key.Campo, p.Key.Muestra, p.Value));

    public IEnumerable<(string Ambito, bool Valor)> VolcarNa()
        => _na.Select(p => (p.Key, p.Value));

    public IEnumerable<(string Ruta, bool Valor)> VolcarChecklists()
        => _checklists.Select(p => (p.Key, p.Value));

    public IEnumerable<(string Campo, IReadOnlyCollection<string> Valores)> VolcarSelecciones()
        => _selecciones.Where(p => p.Value.Count > 0)
                       .Select(p => (p.Key, (IReadOnlyCollection<string>)p.Value));

    public void CargarNa(string ambito, bool valor) => _na[ambito] = valor;

    public void CargarChecklist(string ruta, bool valor) => _checklists[ruta] = valor;

    private readonly record struct Clave(string Bloque, string Campo, int Muestra);
}

public enum Clase { I = 1, II = 2, III = 3 }
