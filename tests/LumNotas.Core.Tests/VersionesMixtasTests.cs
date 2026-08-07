using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Qué le pasa a una toma de notas escrita por una versión <b>posterior</b> del programa
/// cuando la toca una anterior.
/// <para>
/// No es un caso rebuscado: el laboratorio tiene seis equipos y se actualizan de uno en
/// uno, así que habrá días con dos versiones conviviendo. Y el camino peligroso no es
/// abrir el fichero, sino <b>arrastrar una barra en el calendario</b>, que reescribe el
/// documento entero de ficheros que nadie tiene abiertos.
/// </para>
/// <para>
/// Antes del 2026‑08‑07 esto borraba los campos nuevos <b>sin error y sin aviso</b>.
/// </para>
/// </summary>
public class VersionesMixtasTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "mixtas-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    public VersionesMixtasTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Un fichero como el que escribiría una versión posterior: misma forma, con campos
    /// que aquí no se conocen, arriba y dentro de la planificación.
    /// </summary>
    private string EscribirComoUnaVersionPosterior(string formato = FormatoDeFichero.Actual)
    {
        var ruta = Path.Combine(_carpeta, formato.Replace('/', '-') + RepositorioDeProyectos.Extension);

        File.WriteAllText(ruta, $$"""
        {
          "formato": "{{formato}}",
          "versionPlantilla": "1.0.0",
          "codigoTomaDeNotas": "TECNO260201-00",
          "codigoServicio": "TECNO2602",
          "numeroMuestras": 3,
          "normas": ["60598-1_2024"],
          "firmadoPor": "Javier Ibor",
          "planificacion": {
            "inicio": "2026-09-01T00:00:00",
            "fin": "2026-09-15T00:00:00",
            "estado": "enCurso",
            "fechasBloqueadas": true,
            "aprobadoPorCliente": true
          },
          "valores": [
            { "ambito": "generales", "campo": "clase", "muestra": 0, "tipo": "texto", "valor": "I" }
          ]
        }
        """);

        return ruta;
    }

    /// <summary>
    /// El caso corriente: una entrega añade campos sin cambiar la forma del fichero. El
    /// equipo que va por detrás <b>tiene que poder trabajar</b>, y lo que no entiende
    /// vuelve al fichero tal cual.
    /// </summary>
    [Fact]
    public void MoverLaBarraNoBorraLosCamposDeUnaVersionPosterior()
    {
        var ruta = EscribirComoUnaVersionPosterior();

        var plan = _repositorio.LeerPlanificacion(ruta);
        plan.Inicio = new DateTime(2026, 9, 8);
        plan.Fin = new DateTime(2026, 9, 22);
        _repositorio.ActualizarPlanificacion(ruta, plan);

        var despues = File.ReadAllText(ruta);

        Assert.Contains("firmadoPor", despues);            // campo suelto del documento
        Assert.Contains("aprobadoPorCliente", despues);    // campo dentro de la planificación
        Assert.Contains("Javier Ibor", despues);           // y con su valor, no vacío

        // Y el trabajo se ha hecho: esto no va de bloquear, va de no destruir.
        Assert.Equal(new DateTime(2026, 9, 8), _repositorio.LeerPlanificacion(ruta).Inicio);
    }

    /// <summary>
    /// <b>La protección tiene que llegar a los objetos de dentro, no solo a la raíz.</b>
    /// La primera versión cubría el documento y la planificación, y lo de dentro de las
    /// listas se seguía perdiendo — se descubrió midiéndolo, no leyéndolo.
    /// <para>
    /// Importa porque es justo donde crecerá esto: a un valor de ensayo se le añade una
    /// incertidumbre o con qué equipo se midió, y a un colaborador su acreditación.
    /// </para>
    /// </summary>
    [Fact]
    public void TampocoSePierdeLoDeDentroDeLasListas()
    {
        var ruta = Path.Combine(_carpeta, "anidado" + RepositorioDeProyectos.Extension);

        File.WriteAllText(ruta, """
        {
          "formato": "lmnlab/1",
          "codigoTomaDeNotas": "TECNO260201-00",
          "codigoServicio": "TECNO2602",
          "numeroMuestras": 2,
          "normas": ["60598-1_2024"],
          "colaboradores": [
            { "laboratorio": "Ensayos SL", "ensayoYMotivo": "fotometría", "acreditadoPorEllos": true }
          ],
          "valores": [
            { "ambito": "generales", "campo": "clase", "muestra": 0, "tipo": "texto", "valor": "I",
              "medidoPor": "Raúl", "incertidumbre": 0.3 }
          ]
        }
        """);

        // Los dos caminos que escriben, uno detrás de otro.
        var plan = _repositorio.LeerPlanificacion(ruta);
        plan.Inicio = new DateTime(2026, 9, 8);
        _repositorio.ActualizarPlanificacion(ruta, plan);
        _repositorio.Guardar(_repositorio.Cargar(ruta), ruta, "1.0.0");

        var despues = File.ReadAllText(ruta);

        Assert.Contains("acreditadoPorEllos", despues);   // dentro de un colaborador
        Assert.Contains("medidoPor", despues);            // dentro de un valor de ensayo
        Assert.Contains("incertidumbre", despues);
        Assert.Contains("Raúl", despues);                 // con su valor, no solo la clave
    }

    /// <summary>Lo mismo por el otro camino que escribe: guardar desde la toma de notas.</summary>
    [Fact]
    public void GuardarLaTomaDeNotasTampocoLosBorra()
    {
        var ruta = EscribirComoUnaVersionPosterior();

        var datos = _repositorio.Cargar(ruta);
        datos.NumeroMuestras = 4;
        _repositorio.Guardar(datos, ruta, "1.0.0");

        var despues = File.ReadAllText(ruta);

        Assert.Contains("firmadoPor", despues);
        Assert.Contains("aprobadoPorCliente", despues);
        Assert.Contains("fechasBloqueadas", despues);   // la planificación se conserva entera
    }

    /// <summary>
    /// Cuando cambia la <b>forma</b> del fichero, conservar el texto no basta: el programa
    /// viejo entiende los nombres y los interpreta mal. Lo único correcto es no tocarlo.
    /// </summary>
    [Fact]
    public void UnFormatoPosteriorNoSeEscribe()
    {
        var ruta = EscribirComoUnaVersionPosterior("lmnlab/2");
        var antes = File.ReadAllText(ruta);

        var plan = _repositorio.LeerPlanificacion(ruta);
        plan.Inicio = new DateTime(2026, 9, 8);

        Assert.Throws<TomaDeNotasMasNuevaException>(() => _repositorio.ActualizarPlanificacion(ruta, plan));
        Assert.Throws<TomaDeNotasMasNuevaException>(() => _repositorio.Guardar(_repositorio.Cargar(ruta), ruta, "1.0.0"));

        // Ni un byte movido.
        Assert.Equal(antes, File.ReadAllText(ruta));
    }

    /// <summary>Pero leerlo y mirarlo sí se puede: dejar sin consultar un ensayo sería peor.</summary>
    [Fact]
    public void UnFormatoPosteriorSiSePuedeLeer()
    {
        var ruta = EscribirComoUnaVersionPosterior("lmnlab/2");

        Assert.Equal("TECNO260201-00", _repositorio.Cargar(ruta).CodigoTomaDeNotas);
        Assert.Equal(new DateTime(2026, 9, 1), _repositorio.LeerPlanificacion(ruta).Inicio);
    }

    /// <summary>
    /// Una marca estropeada o de las antiguas cuenta como la de ahora. Bloquear el
    /// guardado por una marca rara dejaría al técnico sin trabajar sobre un ensayo
    /// perfectamente legible.
    /// </summary>
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("lumproj/1", 1)]
    [InlineData("lmnlab/1", 1)]
    [InlineData("cualquier cosa", 1)]
    [InlineData("lmnlab/0", 1)]
    [InlineData("lmnlab/2", 2)]
    [InlineData("lmnlab/17", 17)]
    public void LaMarcaSeEntiendeOSeDaPorLaDeAhora(string? marca, int esperado)
        => Assert.Equal(esperado, FormatoDeFichero.NumeroDe(marca));

    /// <summary>
    /// Una planificación que solo trae campos de una versión posterior <b>no está vacía</b>.
    /// Si lo estuviera, quien escribe se la saltaría entera y sería otra forma de borrarla.
    /// </summary>
    [Fact]
    public void LoDesconocidoCuentaComoContenido()
    {
        Assert.True(new Planificacion().EsVacia);

        var conCamposNuevos = new Planificacion
        {
            Desconocido = new() { ["aprobadoPorCliente"] = System.Text.Json.JsonDocument.Parse("true").RootElement }
        };

        Assert.False(conCamposNuevos.EsVacia);
    }
}
