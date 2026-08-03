using System.Text.Json;
using System.Text.Json.Serialization;

namespace LumNotas.Core.Plantilla;

/// <summary>
/// Definición de plantilla de ensayos. Se carga de un JSON versionado y no cambia
/// durante la vida de un proyecto: cada proyecto se queda con la versión con la que nació (DD-09).
/// </summary>
public sealed class PlantillaEnsayos
{
    public Meta Meta { get; init; } = new();
    public DefinicionProyecto Proyecto { get; init; } = new();
    public DefinicionMuestras Muestras { get; init; } = new();
    public List<Seccion> Secciones { get; init; } = [];
    public Avance? Avance { get; init; }

    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static PlantillaEnsayos Cargar(string rutaJson)
    {
        var json = File.ReadAllText(rutaJson);
        return Deserializar(json);
    }

    public static PlantillaEnsayos Deserializar(string json)
        => JsonSerializer.Deserialize<PlantillaEnsayos>(json, Opciones)
           ?? throw new InvalidOperationException("La plantilla no se pudo deserializar.");

    /// <summary>
    /// Marca el proyecto con lo que aporta esta norma: su id y cómo se nombran las
    /// muestras. Un proyecto puede llevar varias normas, así que la norma se añade a
    /// las que ya tenga en vez de sustituirlas.
    /// </summary>
    /// <param name="principal">
    /// Si es la norma con la que nació el proyecto. <b>Solo esa decide cómo se nombran
    /// las muestras</b>: añadir el IP 62262 a un servicio de luminarias no puede
    /// renombrarlas a <c>EBP_CLIM</c>, ni al revés.
    /// </param>
    public void AplicarA(Datos.DatosProyecto datos, bool principal = true)
    {
        if (!string.IsNullOrWhiteSpace(Meta.Id)) datos.Normas.Add(Meta.Id);

        if (!principal) return;

        // Quién es la principal se apunta, no se deja para deducirlo después. El patrón
        // de muestras es una consecuencia de haberla elegido, no la elección.
        if (!string.IsNullOrWhiteSpace(Meta.Id)) datos.NormaPrincipal = Meta.Id;

        if (Muestras.Identificador is { Patron.Length: > 0 } identificador)
            datos.PatronIdentificador = identificador.Patron;
    }

    /// <summary>Todos los bloques de la plantilla, incluidos los de las secciones anidadas.</summary>
    public IEnumerable<Bloque> Bloques() => Secciones.SelectMany(s => s.Bloques);

    public Bloque Bloque(string id)
        => Bloques().FirstOrDefault(b => b.Id == id)
           ?? throw new KeyNotFoundException($"No existe el bloque '{id}' en la plantilla.");

    /// <summary>Reglas del bloque y de sus subbloques, en orden de declaración.</summary>
    public static IEnumerable<Regla> ReglasDe(Bloque bloque)
    {
        foreach (var sub in bloque.SubBloques)
            foreach (var r in sub.Reglas)
                yield return r;
        foreach (var r in bloque.Reglas)
            yield return r;
    }
}

public sealed class Meta
{
    /// <summary>
    /// Qué norma, qué parte y <b>de qué año</b>: <c>60598-1_2024</c>.
    /// <para>
    /// El <b>año de publicación</b> forma parte de la identidad porque un ensayo hecho
    /// contra la norma de un año tiene que seguir midiéndose contra esa. Cuando el id no
    /// lo llevaba, publicar la norma nueva <b>remedía en silencio</b> todos los ensayos
    /// anteriores.
    /// </para>
    /// <para>
    /// <b><see cref="Version"/> se queda fuera del id</b>: se sube por corregir una errata
    /// nuestra, y meterla dentro dejaría huérfano cada proyecto en cada corrección.
    /// </para>
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Ids con los que se conoció antes esta misma norma y año. Es lo que permite cambiar
    /// el esquema de identificación sin romper los proyectos ya guardados: cada uno lleva
    /// escrito el id que existía el día que se guardó, y aquí se dice que sigue siendo
    /// esta.
    /// <para>
    /// La migración vive en el JSON, como todo lo demás — no en un <c>switch</c> de C#
    /// que haya que ampliar cada vez.
    /// </para>
    /// </summary>
    public List<string>? IdsAnteriores { get; init; }

    /// <summary>
    /// Lo que sale en el nombre del fichero: <c>TdN_<b>60598</b>_LEDC42502xx-00</c>.
    /// <para>
    /// Va aparte del id a propósito. El id creció para llevar la parte y el año, y el
    /// laboratorio quiere que el nombre del fichero siga siendo corto. Si falta, se usa
    /// el id entero.
    /// </para>
    /// </summary>
    public string? CodigoDeFichero { get; init; }

    /// <summary>
    /// El <b>año de publicación</b> de la norma, para poder enseñarlo sin descomponer el id.
    /// <para>
    /// Es el año y solo el año: <c>2021</c>, no <c>2021+A11:2022</c>. La designación
    /// completa —con sus enmiendas— va en <see cref="Titulo"/>, que es lo que se lee y lo
    /// que sale en el informe.
    /// </para>
    /// <para>
    /// <b>No es la «edición» de la norma.</b> Una norma tiene edición —la 8, la 9— y año
    /// de publicación, y no son lo mismo; lo que el laboratorio usa para distinguirlas, y
    /// lo que lleva el id, es el año.
    /// </para>
    /// </summary>
    public string? AnioDePublicacion { get; init; }
    /// <summary>Nombre con el que la norma se ofrece al técnico. Si falta, se usa el id.</summary>
    public string? Titulo { get; init; }

    /// <summary>
    /// Nombre corto para donde no cabe el largo, como las tarjetas de la portada. El
    /// informe sigue usando el título completo, que es lo que espera un documento.
    /// </summary>
    public string? TituloCorto { get; init; }

    /// <summary>
    /// Normas que se pueden añadir a esta en un mismo servicio. Luminarias no admite la
    /// 60529 porque ya lleva el IP dentro; el IP y el IK solo se admiten entre sí.
    /// <para>Si no se declara, se admiten todas: una norma nueva funciona sin tocar nada.</para>
    /// </summary>
    public List<string>? NormasCompatibles { get; init; }
    public string Version { get; init; } = "";
    public string? Origen { get; init; }
    public string? Numeracion { get; init; }
    public Alcance? Alcance { get; init; }
    public string? CatalogoEquipos { get; init; }

    /// <summary>Para el nombre del fichero: el código corto si lo hay, y si no el id.</summary>
    public string CodigoParaFichero => string.IsNullOrWhiteSpace(CodigoDeFichero) ? Id : CodigoDeFichero;

    /// <summary>
    /// Si esta plantilla es la que corresponde a un id guardado en un proyecto, sea el
    /// suyo de ahora o uno de los que tuvo antes. Se compara sin distinguir mayúsculas
    /// porque el id lo escribe una persona en el JSON.
    /// </summary>
    public bool Responde(string? id)
        => !string.IsNullOrWhiteSpace(id)
           && (string.Equals(Id, id, StringComparison.OrdinalIgnoreCase)
               || (IdsAnteriores?.Any(a => string.Equals(a, id, StringComparison.OrdinalIgnoreCase)) ?? false));
}

public sealed class Alcance
{
    public List<string> Incluye { get; init; } = [];
    public List<string> Excluye { get; init; } = [];
}

public sealed class DefinicionProyecto
{
    public List<Campo> Campos { get; init; } = [];
    public List<Regla> Reglas { get; init; } = [];
}

public sealed class DefinicionMuestras
{
    public int Min { get; init; } = 1;
    public int Max { get; init; } = 8;
    public IdentificadorDeMuestra? Identificador { get; init; }
    public List<Campo> Campos { get; init; } = [];
}

/// <summary>
/// Cómo se nombran las muestras del servicio. El patrón admite <c>{codigoServicio}</c>;
/// el número de dos cifras se añade al final. Cambia por norma: las de seguridad usan
/// <c>EBP_SAFE…</c> y las de IK 62262, <c>EBP_CLIM…</c>.
/// </summary>
public sealed class IdentificadorDeMuestra
{
    public string Patron { get; init; } = Datos.DatosProyecto.PatronPorDefecto;
    public string? OrigenExcel { get; init; }
}

public sealed class Seccion
{
    public string Codigo { get; init; } = "";
    public string? CodigoAntiguo { get; init; }
    public string Titulo { get; init; } = "";
    public OpcionNa? Na { get; init; }
    public List<Bloque> Bloques { get; init; } = [];
}

public sealed class Bloque
{
    public string Id { get; init; } = "";
    public string Codigo { get; init; } = "";
    public string? CodigoAntiguo { get; init; }
    public string Titulo { get; init; } = "";
    public string? OrigenExcel { get; init; }
    public int? PesoAvance { get; init; }
    /// <summary>
    /// Id de regla que decide si el apartado se muestra. Sirve para los ensayos de las
    /// partes -2: si la parte no está marcada en el proyecto, el apartado no aparece.
    /// Si se omite, el apartado siempre se muestra.
    /// </summary>
    public string? VisibleSi { get; init; }
    /// <summary>
    /// Id de la regla que determina el estado del apartado. Si se omite, se deduce de
    /// las reglas del bloque. Conviene fijarla cuando la deducción no sea evidente.
    /// </summary>
    public string? ReglaDeCierre { get; init; }
    public OpcionNa? Na { get; init; }
    public Ambiente? Ambiente { get; init; }
    public List<string> Notas { get; init; } = [];
    public string? Equipos { get; init; }
    public bool Comentarios { get; init; }
    public List<Campo> Campos { get; init; } = [];
    public List<Checklist> Checklists { get; init; } = [];
    public List<SubBloque> SubBloques { get; init; } = [];
    public List<Regla> Reglas { get; init; } = [];
}

public sealed class SubBloque
{
    public string Id { get; init; } = "";
    public string Titulo { get; init; } = "";
    public OpcionNa? Na { get; init; }
    public Ambiente? Ambiente { get; init; }
    public int? PesoAvance { get; init; }
    public bool Comentarios { get; init; }
    public List<string> Notas { get; init; } = [];
    public List<Campo> Campos { get; init; } = [];
    public List<Checklist> Checklists { get; init; } = [];
    public List<Regla> Reglas { get; init; } = [];
}

public sealed class OpcionNa
{
    public string Id { get; init; } = "na";
    public string Etiqueta { get; init; } = "N/A";
    public string? OrigenExcel { get; init; }
}

public sealed class Ambiente
{
    public bool Temperatura { get; init; }
    public bool Humedad { get; init; }
    public bool Fecha { get; init; }
    public bool FechaPorMuestra { get; init; }
    public string? OrigenExcel { get; init; }
}

public sealed class Campo
{
    public string Id { get; init; } = "";
    public string Etiqueta { get; init; } = "";
    public string Tipo { get; init; } = "texto";
    public string? Unidad { get; init; }
    public bool PorMuestra { get; init; }
    public bool Obligatorio { get; init; }
    public bool Opcional { get; init; }
    public int? Elementos { get; init; }
    public bool Ampliable { get; init; }
    public List<Campo> Campos { get; init; } = [];
    public List<string> Opciones { get; init; } = [];
    public bool Multiple { get; init; }
    /// <summary>Para campos derivados: ruta "ambito.campo" de la que se toma el valor.</summary>
    public string? De { get; init; }
    public string? Nota { get; init; }

    /// <summary>Rótulo breve para donde no cabe la etiqueta entera, como la fila de una muestra.</summary>
    public string? EtiquetaCorta { get; init; }

    public string? OrigenExcel { get; init; }
    public string? VisibleSi { get; init; }
    public string? Cambio { get; init; }

    /// <summary>
    /// Valor con el que se rellena el campo si está vacío. Se guarda tal cual viene del
    /// JSON porque unas plantillas lo declaran como número y otras como texto.
    /// </summary>
    public JsonElement? PorDefecto { get; init; }

    /// <summary>El valor por defecto como número, si lo es.</summary>
    public double? NumeroPorDefecto
        => PorDefecto is { ValueKind: JsonValueKind.Number } v ? v.GetDouble() : null;

    /// <summary>El valor por defecto como texto, si lo es. Lo usan los desplegables.</summary>
    public string? TextoPorDefecto
        => PorDefecto is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;
}

public sealed class Checklist
{
    public string Id { get; init; } = "";
    public string? Etiqueta { get; init; }
    /// <summary>libre | unica | alMenosUna | exactamenteUna | todas</summary>
    public string Modo { get; init; } = "libre";
    public string? Nota { get; init; }
    public List<OpcionChecklist> Opciones { get; init; } = [];
}

public sealed class OpcionChecklist
{
    public string Id { get; init; } = "";
    public string Etiqueta { get; init; } = "";
    public bool PorDefecto { get; init; }
    public string? OrigenExcel { get; init; }
}

public sealed class Regla
{
    public string Id { get; init; } = "";
    public string Tipo { get; init; } = "";
    public string? Etiqueta { get; init; }
    public string? Descripcion { get; init; }
    public string? OrigenExcel { get; init; }

    // avisoFecha / rango / noVacio / recuento / recuentoDatos
    public string? Campo { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
    public int? Umbral { get; init; }
    public int? UmbralPorMuestra { get; init; }
    public bool CuentaCeros { get; init; }
    public string? SoloSi { get; init; }
    /// <summary>primeraMuestra (por defecto) | todasLasMuestras | algunaMuestra</summary>
    public string? Ambito { get; init; }

    // checklist
    public string? Checklist { get; init; }
    public string? Opcion { get; init; }

    // faltanDatos
    public string? SiNoAplica { get; init; }
    public List<string> FaltanSi { get; init; } = [];

    // y / o
    public List<string> De { get; init; } = [];

    // si
    public string? Condicion { get; init; }
    public string? Entonces { get; init; }
    public JsonElement? SiNo { get; init; }

    // duraciones (los umbrales van en texto: "48h", "180min")
    public string? Inicio { get; init; }
    public string? Fin { get; init; }
    public string? Minimo { get; init; }
    public string? Maximo { get; init; }

    // predicado / calculo
    public string? Nombre { get; init; }
    /// <summary>Argumento del predicado, cuando el mismo predicado sirve para varios casos.</summary>
    public string? Parametro { get; init; }

    // aviso
    public string? Texto { get; init; }
    public string? Nivel { get; init; }
    public List<string> CuandoTodas { get; init; } = [];

    // peso
    public int? Valor { get; init; }
    public string? AplicaSiNo { get; init; }
    public string? FinSiNo { get; init; }

    // trazabilidad de defectos corregidos respecto al Excel
    public string? Defecto { get; init; }
    public string? NotaDefecto { get; init; }
    public string? Cambio { get; init; }
}

public sealed class Avance
{
    public string Modo { get; init; } = "ponderado";
    public string? IndicadorSecundario { get; init; }
    public int? PesoTotalProyectoCompleto { get; init; }
}
