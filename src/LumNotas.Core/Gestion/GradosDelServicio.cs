using LumNotas.Core.Datos;

namespace LumNotas.Core.Gestion;

/// <summary>
/// El grado IP e IK <b>del servicio entero</b>, a partir de los de sus muestras.
/// <para>
/// Los grados van por muestra: un mismo servicio puede traer luminarias con objetivos
/// distintos. Para el listado hace falta un solo valor por toma de notas, y el laboratorio
/// pide <b>el mayor</b>.
/// </para>
/// </summary>
public static class GradosDelServicio
{
    // Cómo lo declara la plantilla. Están aquí y no repetidos en la ventana porque la
    // regla de qué es «el mayor» es del laboratorio y tiene tests.
    public const string CampoPrimeraCifra = "ipPrimeraCifra";
    public const string CampoSegundaCifra = "ipSegundaCifra";
    public const string CampoIk = "gradoIk";
    public const string CampoOrdinaria = "luminariaOrdinaria";

    /// <summary>Lo que se elige cuando la muestra no lleva ensayo de impacto.</summary>
    public const string SinIk = "No IK";

    /// <summary>Una luminaria ordinaria es IP20: el atajo del laboratorio.</summary>
    private const int OrdinariaSolidos = 2;
    private const int OrdinariaAgua = 0;

    /// <summary>
    /// El IP mayor del servicio, como <c>IP54</c>, o vacío si ninguna muestra lo lleva.
    /// <para>
    /// <b>Manda la segunda cifra</b> —la del agua—, y la primera solo desempata: para el
    /// laboratorio <c>IP28</c> es mayor que <c>IP54</c>. No es un orden físico (proteger
    /// del polvo y proteger del agua no se comparan), es el criterio con el que aquí se
    /// ordenan los servicios, y por eso se decide en un solo sitio.
    /// </para>
    /// </summary>
    public static string IpMaximo(DatosProyecto datos)
    {
        (int Agua, int Solidos)? mayor = null;

        foreach (var muestra in datos.Muestras)
        {
            var grado = IpDe(datos, muestra);
            if (grado is null) continue;

            // Se compara primero el agua y después los sólidos: es justo el orden en que
            // se escribe la tupla, así que la comparación por defecto ya sirve.
            if (mayor is null || grado.Value.CompareTo(mayor.Value) > 0) mayor = grado;
        }

        return mayor is null ? "" : $"IP{mayor.Value.Solidos}{mayor.Value.Agua}";
    }

    /// <summary>
    /// El IK mayor del servicio, como <c>IK08</c>, o vacío si ninguna muestra lleva
    /// ensayo de impacto. <c>No IK</c> no es un grado bajo: es no haberlo.
    /// </summary>
    public static string IkMaximo(DatosProyecto datos)
    {
        var mayor = -1;

        foreach (var muestra in datos.Muestras)
        {
            var numero = NumeroDeIk(datos.Obtener(DatosProyecto.Cabecera, CampoIk, muestra) as string);
            if (numero > mayor) mayor = numero;
        }

        return mayor < 0 ? "" : $"IK{mayor:00}";
    }

    /// <summary>
    /// El IP de <b>una</b> muestra, como <c>IP65</c>, o vacío si no lo lleva. Lo usa la
    /// tabla de muestras de la exportación, donde cada una va en su fila: enseñar ahí el
    /// máximo del servicio sería atribuir a una muestra el grado de otra.
    /// </summary>
    public static string IpDeLaMuestra(DatosProyecto datos, int muestra)
        => IpDe(datos, muestra) is { } grado ? $"IP{grado.Solidos}{grado.Agua}" : "";

    /// <summary>El IK de <b>una</b> muestra, como <c>IK08</c>. Vacío si no lleva ensayo.</summary>
    public static string IkDeLaMuestra(DatosProyecto datos, int muestra)
    {
        var numero = NumeroDeIk(datos.Obtener(DatosProyecto.Cabecera, CampoIk, muestra) as string);
        return numero < 0 ? "" : $"IK{numero:00}";
    }

    /// <summary>
    /// El IP de una muestra, en cifras. <c>null</c> si no tiene ninguna de las dos: una
    /// muestra a medio rellenar no puede inventarse un grado.
    /// </summary>
    private static (int Agua, int Solidos)? IpDe(DatosProyecto datos, int muestra)
    {
        // El atajo manda: al marcar «Luminaria ordinaria» la plantilla rellena IP2X e
        // IPX0, pero si alguien lo marcó antes de que existiera ese automatismo, el
        // grado sigue siendo IP20.
        if (datos.Obtener(DatosProyecto.Cabecera, CampoOrdinaria, muestra) is true)
            return (OrdinariaAgua, OrdinariaSolidos);

        var solidos = Cifra(datos.Obtener(DatosProyecto.Cabecera, CampoPrimeraCifra, muestra) as string);
        var agua = Cifra(datos.Obtener(DatosProyecto.Cabecera, CampoSegundaCifra, muestra) as string);

        if (solidos is null && agua is null) return null;

        // La «X» es «no se declara», y para ordenar cuenta como 0 — que es lo que
        // significa en la norma: sin protección declarada por ese lado.
        return (agua ?? 0, solidos ?? 0);
    }

    /// <summary>La cifra de un <c>IP4X</c> o un <c>IPX7</c>. La «X» no es una cifra.</summary>
    private static int? Cifra(string? grado)
    {
        if (string.IsNullOrWhiteSpace(grado)) return null;

        foreach (var c in grado)
            if (char.IsDigit(c)) return c - '0';

        return null;
    }

    /// <summary>El número de un <c>IK08</c>. Devuelve ‑1 cuando no hay grado.</summary>
    private static int NumeroDeIk(string? grado)
    {
        if (string.IsNullOrWhiteSpace(grado) || grado.Trim() == SinIk) return -1;

        var digitos = new string([.. grado.Where(char.IsDigit)]);
        return int.TryParse(digitos, out var numero) ? numero : -1;
    }
}
