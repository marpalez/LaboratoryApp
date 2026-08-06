using System.Text.Json;
using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Motor;

/// <summary>
/// Evalúa las reglas de la plantilla contra los datos de un proyecto.
/// Implementa los ocho patrones P1-P8 documentados en docs/REGLAS-NEGOCIO.md;
/// lo que no es declarativo vive en <see cref="Predicados"/> y <see cref="Calculos"/>.
/// </summary>
/// <remarks>
/// <b>Ámbito de datos:</b> una regla lee los campos del elemento que los declara.
/// Si la regla está en un subbloque, su ámbito es el id del subbloque; si está en el
/// bloque, el id del bloque. Así los tres subbloques de IP pueden tener cada uno su
/// propia fecha sin pisarse.
/// </remarks>
public sealed class MotorDeReglas(PlantillaEnsayos plantilla, DatosProyecto datos)
{
    private readonly Dictionary<string, object?> _cache = [];
    private readonly HashSet<string> _enCurso = [];

    public PlantillaEnsayos Plantilla { get; } = plantilla;
    public DatosProyecto Datos { get; } = datos;

    private Dictionary<string, Entrada>? _indice;
    private Dictionary<Ruta, Ruta>? _derivados;

    /// <summary>
    /// Una regla con su contexto. <paramref name="Ambiente"/> es el del elemento que
    /// declara la regla —subbloque si lo hay—, no el del bloque padre: cada subapartado
    /// de calentamiento o de la sección 12 tiene su propia fecha.
    /// </summary>
    private readonly record struct Entrada(Regla Regla, Bloque Bloque, string Ambito, Ambiente? Ambiente);
    internal readonly record struct Ruta(string Ambito, string Campo);

    private Dictionary<string, Entrada> Indice
    {
        get
        {
            if (_indice is not null) return _indice;
            _indice = [];
            foreach (var regla in Plantilla.Proyecto.Reglas)
                _indice[regla.Id] = new Entrada(regla, new Bloque { Id = "proyecto" }, "proyecto", null);

            foreach (var bloque in Plantilla.Bloques())
            {
                foreach (var sub in bloque.SubBloques)
                    foreach (var regla in sub.Reglas)
                        _indice[regla.Id] = new Entrada(regla, bloque, sub.Id, sub.Ambiente ?? bloque.Ambiente);

                foreach (var regla in bloque.Reglas)
                    _indice[regla.Id] = new Entrada(regla, bloque, bloque.Id, bloque.Ambiente);
            }
            return _indice;
        }
    }

    /// <summary>Redirecciones de los campos declarados como derivados en la plantilla.</summary>
    private Dictionary<Ruta, Ruta> Derivados
    {
        get
        {
            if (_derivados is not null) return _derivados;
            _derivados = [];
            foreach (var bloque in Plantilla.Bloques())
            {
                Registrar(bloque.Id, bloque.Campos);
                foreach (var sub in bloque.SubBloques) Registrar(sub.Id, sub.Campos);
            }
            return _derivados;

            void Registrar(string ambito, IEnumerable<Campo> campos)
            {
                foreach (var campo in campos)
                {
                    if (campo.Tipo != "derivado" || campo.De is null) continue;
                    var partes = campo.De.Split('.', 2);
                    if (partes.Length == 2)
                        _derivados![new Ruta(ambito, campo.Id)] = new Ruta(partes[0], partes[1]);
                }
            }
        }
    }

    /// <summary>Aplica las redirecciones de campos derivados, conservando el sufijo del grupo repetido.</summary>
    internal Ruta Resolver(string ambito, string campo)
    {
        var corte = campo.IndexOf('[');
        var raiz = corte < 0 ? campo : campo[..corte];
        var sufijo = corte < 0 ? "" : campo[corte..];

        return Derivados.TryGetValue(new Ruta(ambito, raiz), out var destino)
            ? new Ruta(destino.Ambito, destino.Campo + sufijo)
            : new Ruta(ambito, campo);
    }

    public bool EsVerdadera(string idRegla) => Evaluar(idRegla) is true;

    /// <summary>
    /// Evalúa una regla por id. Admite el prefijo '!' para negar, tal y como
    /// documenta el contrato de la plantilla.
    /// </summary>
    public object? Evaluar(string referencia)
    {
        if (referencia.StartsWith('!'))
            return Evaluar(referencia[1..]) is not true;

        if (_cache.TryGetValue(referencia, out var cacheado)) return cacheado;

        if (!_enCurso.Add(referencia))
            throw new InvalidOperationException($"Ciclo de dependencias al evaluar la regla '{referencia}'.");

        try
        {
            if (!Indice.TryGetValue(referencia, out var entrada))
                throw new KeyNotFoundException($"No existe la regla '{referencia}' en la plantilla.");

            var valor = Calcular(entrada);
            _cache[referencia] = valor;
            return valor;
        }
        finally
        {
            _enCurso.Remove(referencia);
        }
    }

    /// <summary>Descarta los resultados memorizados. Se llama al cambiar cualquier dato.</summary>
    public void Invalidar() => _cache.Clear();

    private object? Calcular(Entrada e)
    {
        var r = e.Regla;
        return r.Tipo switch
        {
            "avisoFecha"      => AvisoFecha(r, e),
            "faltanDatos"     => FaltanDatos(r, e),
            "alMenosUna"      => Checklist(r, e, modo: "alMenosUna"),
            "exactamenteUna"  => Checklist(r, e, modo: "exactamenteUna"),
            "todas"           => Checklist(r, e, modo: "todas"),
            "opcion"          => Datos.Marcada(e.Ambito, r.Checklist!, r.Opcion!),
            "recuento"        => Recuento(r, e),
            "recuentoDatos"   => RecuentoDatos(r, e),
            "duracionMinima"  => DuracionMinima(r, e),
            "duracionEnRango" => DuracionEnRango(r, e),
            "rango"           => Rango(r, e),
            "noVacio"         => NoVacio(r, e),
            "y"               => r.De.All(d => Evaluar(d) is true),
            "o"               => r.De.Any(d => Evaluar(d) is true),
            "si"              => Condicional(r),
            "predicado"       => Predicados.Evaluar(r.Nombre!, this, e.Ambito, r.Parametro),
            "calculo"         => Calculos.Evaluar(r.Nombre!, this, e.Ambito),
            "aviso"           => r.CuandoTodas.Count > 0 && r.CuandoTodas.All(c => Evaluar(c) is true),
            "peso"            => Peso(r, e),
            _ => throw new NotSupportedException($"Tipo de regla no soportado: '{r.Tipo}' (regla {r.Id}).")
        };
    }

    // ---- P2 ---------------------------------------------------------------

    private bool AvisoFecha(Regla r, Entrada e)
    {
        var campo = r.Campo ?? "ambiente.fecha";
        if (Datos.Instante(e.Ambito, campo) is not null) return false;

        // Variante con muestras: basta con que alguna muestra tenga fecha.
        if (e.Ambiente?.FechaPorMuestra == true
            && Datos.Muestras.Any(m => Datos.Instante(e.Ambito, campo, m) is not null))
            return false;

        return true;
    }

    // ---- P3 ---------------------------------------------------------------

    private bool FaltanDatos(Regla r, Entrada e)
    {
        if (NoAplica(r.SiNoAplica, e)) return false;
        return r.FaltanSi.Any(f => Evaluar(f) is true);
    }

    private bool NoAplica(string? referencia, Entrada e)
    {
        if (referencia is null) return false;
        // El N/A puede venir del propio ámbito, de la sección, o de un checklist del ámbito.
        if (Datos.Na($"{e.Ambito}/{referencia}")) return true;
        if (Datos.Na($"{e.Bloque.Id}/{referencia}")) return true;
        if (Datos.Na(referencia)) return true;
        return Datos.Marcada(e.Ambito, referencia, referencia);
    }

    // ---- P4 / P5 ----------------------------------------------------------

    private bool Checklist(Regla r, Entrada e, string modo)
    {
        var lista = BuscarChecklist(e.Bloque, r.Checklist!)
            ?? throw new KeyNotFoundException($"No existe el checklist '{r.Checklist}' en el bloque '{e.Bloque.Id}'.");
        var ids = lista.Opciones.Select(o => o.Id).ToList();
        var marcadas = Datos.Marcadas(e.Ambito, lista.Id, ids);

        return modo switch
        {
            "alMenosUna" => marcadas >= 1,
            "exactamenteUna" => marcadas == 1,
            "todas" => marcadas == ids.Count,
            _ => throw new NotSupportedException(modo)
        };
    }

    private static Checklist? BuscarChecklist(Bloque b, string id)
        => b.Checklists.FirstOrDefault(c => c.Id == id)
           ?? b.SubBloques.SelectMany(s => s.Checklists).FirstOrDefault(c => c.Id == id);

    // ---- P6 ---------------------------------------------------------------

    private int Recuento(Regla r, Entrada e)
        => ValoresSegunAmbito(r, e).Count(v => TieneDato(v, r.CuentaCeros));

    /// <summary>
    /// Si están los datos que pide la regla.
    /// <para>
    /// <c>umbralPorMuestra</c> se comprueba <b>muestra a muestra</b>, y no sumando: la
    /// regla dice «alto, ancho y profundo <i>de cada muestra</i>», así que una muestra
    /// medida de sobra no puede tapar a otra sin medir. Sumando, un servicio de dos
    /// muestras con las cuatro medidas de la primera y dos de la segunda llegaba a seis
    /// —el total pedido— y el apartado se daba por terminado con media muestra sin medir.
    /// </para>
    /// <para>
    /// <c>umbral</c>, en cambio, sí es un total: ese no habla de muestras.
    /// </para>
    /// </summary>
    private bool RecuentoDatos(Regla r, Entrada e)
    {
        if (r.SoloSi is not null && Evaluar(r.SoloSi) is not true) return true;

        if (r.UmbralPorMuestra is { } porMuestra)
            return Datos.Muestras.All(m => RecuentoDe(r, e, m) >= porMuestra);

        return Recuento(r, e) >= (r.Umbral ?? 1);
    }

    private int RecuentoDe(Regla r, Entrada e, int muestra)
    {
        var ruta = Resolver(e.Ambito, r.Campo!);
        return Datos.ValoresDe(ruta.Ambito, ruta.Campo, muestra)
                    .Count(v => TieneDato(v, r.CuentaCeros));
    }

    private IEnumerable<object?> ValoresSegunAmbito(Regla r, Entrada e)
    {
        var ruta = Resolver(e.Ambito, r.Campo!);
        return (r.Ambito ?? "primeraMuestra") switch
        {
            "todasLasMuestras" => Datos.ValoresDeTodasLasMuestras(ruta.Ambito, ruta.Campo),
            _ => Datos.ValoresDe(ruta.Ambito, ruta.Campo, 1)
        };
    }

    private static bool TieneDato(object? valor, bool cuentaCeros) => valor switch
    {
        null => false,
        string s => !string.IsNullOrWhiteSpace(s),
        double d => cuentaCeros || d != 0,
        int i => cuentaCeros || i != 0,
        _ => true
    };

    // ---- P7 ---------------------------------------------------------------

    private bool DuracionMinima(Regla r, Entrada e)
    {
        var duracion = Duracion(r, e);
        return duracion is not null && duracion >= LeerDuracion(r.Minimo!);
    }

    private bool DuracionEnRango(Regla r, Entrada e)
    {
        var duracion = Duracion(r, e);
        if (duracion is null) return false;
        var min = LeerDuracion(r.Minimo ?? throw new InvalidOperationException($"{r.Id}: falta 'minimo'."));
        var max = LeerDuracion(r.Maximo ?? throw new InvalidOperationException($"{r.Id}: falta 'maximo'."));
        return duracion >= min && duracion <= max;
    }

    /// <summary>Duración más corta entre las muestras del ámbito; null si falta algún instante.</summary>
    private TimeSpan? Duracion(Regla r, Entrada e)
    {
        var muestras = (r.Ambito ?? "primeraMuestra") == "todasLasMuestras" ? Datos.Muestras : [1];
        var rutaInicio = Resolver(e.Ambito, r.Inicio!);
        var rutaFin = Resolver(e.Ambito, r.Fin!);

        TimeSpan? peor = null;
        foreach (var m in muestras)
        {
            var inicio = Datos.Instante(rutaInicio.Ambito, rutaInicio.Campo, m);
            var fin = Datos.Instante(rutaFin.Ambito, rutaFin.Campo, m);
            if (inicio is null || fin is null) return null;

            var d = fin.Value - inicio.Value;
            if (peor is null || d < peor) peor = d;
        }
        return peor;
    }

    /// <summary>Lee duraciones del contrato de la plantilla: "48h", "180min", "10d".</summary>
    public static TimeSpan LeerDuracion(string texto)
    {
        texto = texto.Trim().ToLowerInvariant();
        var cultura = System.Globalization.CultureInfo.InvariantCulture;
        if (texto.EndsWith("min")) return TimeSpan.FromMinutes(double.Parse(texto[..^3], cultura));
        if (texto.EndsWith('h')) return TimeSpan.FromHours(double.Parse(texto[..^1], cultura));
        if (texto.EndsWith('d')) return TimeSpan.FromDays(double.Parse(texto[..^1], cultura));
        throw new FormatException($"Duración no reconocida: '{texto}'. Formatos admitidos: 30min, 48h, 10d.");
    }

    // ---- rango / noVacio --------------------------------------------------

    private bool Rango(Regla r, Entrada e)
    {
        var ruta = Resolver(e.Ambito, r.Campo!);
        var muestras = (r.Ambito ?? "primeraMuestra") == "todasLasMuestras" ? Datos.Muestras : [1];

        foreach (var m in muestras)
        {
            var v = Datos.Numero(ruta.Ambito, ruta.Campo, m);
            if (v is null) return false;
            if (r.Min is { } min && v < min) return false;
            if (r.Max is { } max && v > max) return false;
        }
        return true;
    }

    private bool NoVacio(Regla r, Entrada e)
    {
        var ruta = Resolver(e.Ambito, r.Campo!);
        return TieneDato(Datos.Obtener(ruta.Ambito, ruta.Campo, 1), cuentaCeros: false);
    }

    // ---- si ---------------------------------------------------------------

    private object? Condicional(Regla r)
    {
        if (Evaluar(r.Condicion!) is true)
            return r.Entonces is null ? true : Evaluar(r.Entonces);

        if (r.SiNo is not { } siNo) return false;
        return siNo.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => Evaluar(siNo.GetString()!),
            _ => null
        };
    }

    // ---- P8 ---------------------------------------------------------------

    private Aportacion Peso(Regla r, Entrada e)
    {
        var valor = r.Valor ?? e.Bloque.PesoAvance ?? 0;
        var aplica = r.AplicaSiNo is null || !NoAplica(r.AplicaSiNo, e);
        var terminado = aplica && (r.FinSiNo is null || Evaluar(r.FinSiNo) is not true);
        return new Aportacion(e.Ambito, valor, aplica, terminado);
    }
}

/// <summary>Aportación de un apartado al indicador de avance (P8).</summary>
public readonly record struct Aportacion(string Apartado, int Peso, bool Aplica, bool Terminado)
{
    public int PesoEnProyecto => Aplica ? Peso : 0;
    public int PesoFinalizado => Terminado ? PesoEnProyecto : 0;
}
