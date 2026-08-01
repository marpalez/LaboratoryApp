using System.Text.Json;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Tests;

/// <summary>
/// Comprueba que la plantilla del MVP y el motor siguen encajando: todos los tipos
/// de regla existen, todos los predicados están registrados y no hay referencias rotas.
/// </summary>
public class PlantillaTests
{
    private static readonly string[] TiposSoportados =
    [
        "avisoFecha", "faltanDatos", "alMenosUna", "exactamenteUna", "todas", "opcion",
        "recuento", "recuentoDatos", "duracionMinima", "duracionEnRango", "rango",
        "noVacio", "y", "o", "si", "predicado", "calculo", "aviso", "peso",
        "seleccionAutomaticaEquipos"
    ];

    private static IEnumerable<Regla> TodasLasReglas()
    {
        var p = Contexto.Plantilla;
        foreach (var r in p.Proyecto.Reglas) yield return r;
        foreach (var b in p.Bloques())
            foreach (var r in PlantillaEnsayos.ReglasDe(b))
                yield return r;
    }

    [Fact]
    public void LaPlantillaSeCarga()
    {
        var p = Contexto.Plantilla;
        Assert.Equal("60598", p.Meta.Id);
        Assert.Equal("2024", p.Meta.Numeracion);

        // El IK dejó de estar excluido: se elige por muestra y tiene su propia sección.
        Assert.Contains(p.Meta.Alcance!.Incluye, i => i.Contains("IK"));
        Assert.Contains(p.Meta.Alcance!.Excluye, e => e.Contains("62031"));
    }

    [Fact]
    public void ContieneTodosLosApartadosDeLaNorma()
    {
        var ids = Contexto.Plantilla.Bloques().Select(b => b.Id).ToList();
        Assert.Equal(
        [
            "generales", "anexoA", "6",
            "7.4", "7.6", "7.9", "7.10", "7.12", "7.13", "7.14.1", "7.18.1", "7.18.2", "7.24.2", "7.28", "7.33", "7.35",
            "2-3.3.6.3.1", "2-3.3.6.5.1", "2-3.3.6.5.2", "2-3.3.6.8", "2-4.4.7.3",
            "2-5.5.6.5", "2-5.5.6.8.1", "2-5.5.6.8.2", "2-10.10.6.2", "2-10.10.6.3",
            "2-13.13.6.1", "2-18.18.7.2", "2-18.18.7.3", "2-22.22.17.3", "2-22.22.19",
            "13.2", "9", "8", "10", "14", "11", "12",
            "15.2.1", "15.3.1", "15.3.2", "15.4",
            "16", "17",
            "60598.ik"
        ], ids);
    }

    /// <summary>
    /// El orden y los títulos de las secciones son los de la hoja «Toma de notas 60598»,
    /// no el orden numérico de la norma: el técnico busca los apartados donde los tiene hoy.
    /// </summary>
    [Fact]
    public void LasSeccionesSiguenElOrdenYLosTitulosDelExcel()
    {
        var titulos = Contexto.Plantilla.Secciones.Select(s => s.Titulo).ToList();
        Assert.Equal(
        [
            "Ratings y otros datos generales",
            "Verificación partes activas",
            "Sección 6 - Marcado",
            "Sección 7 - Construcción",
            "Sección 7 - Construcción y especificaciones partes -2",
            "Sección 13 - Líneas de fuga y distancias en el aire",
            "Sección 9 - Tierra",
            "Sección 8 - Cableado",
            "Sección 10 - Protección contra choque eléctrico",
            "Sección 14 - Calentamiento y endurancia",
            "Sección 11 - IP y humedad",
            "Sección 12 - Resistencia de aislamiento, rigidez dieléctrica y corrientes de fuga",
            "Sección 15 - Resistencia al fuego",
            // El Excel las juntaba en una hoja; el laboratorio pidió separarlas (2026-08-01).
            "Sección 16 - Bornes con tornillos",
            "Sección 17 - Bornes sin tornillo",
            // El IK dejó de ser toma de notas aparte para luminarias (2026-08-01):
            // se elige por muestra y esta sección aparece si alguna lo lleva.
            "Ensayo de IK - EN/IEC 62262"
        ], titulos);
    }

    [Fact]
    public void TodosLosTiposDeReglaEstanSoportadosPorElMotor()
    {
        var desconocidos = TodasLasReglas()
            .Select(r => r.Tipo)
            .Distinct()
            .Where(t => !TiposSoportados.Contains(t))
            .ToList();

        Assert.Empty(desconocidos);
    }

    [Fact]
    public void TodosLosPredicadosDeLaPlantillaEstanRegistrados()
    {
        var usados = TodasLasReglas()
            .Where(r => r.Tipo == "predicado")
            .Select(r => r.Nombre!)
            .Distinct();

        Assert.All(usados, n => Assert.Contains(n, Predicados.Registrados));
    }

    [Fact]
    public void NoHayIdsDeReglaRepetidos()
    {
        var repetidos = TodasLasReglas()
            .GroupBy(r => r.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(repetidos);
    }

    [Fact]
    public void TodasLasReferenciasEntreReglasApuntanAReglasExistentes()
    {
        var existentes = TodasLasReglas().Select(r => r.Id).ToHashSet();
        var rotas = new List<string>();

        foreach (var r in TodasLasReglas())
        {
            var referencias = r.FaltanSi
                .Concat(r.De)
                .Concat(r.CuandoTodas)
                .Concat(new[] { r.Condicion, r.Entonces, r.SoloSi, r.FinSiNo }.Where(x => x is not null)!)
                .Select(x => x!.TrimStart('!'));

            foreach (var referencia in referencias)
                if (!existentes.Contains(referencia))
                    rotas.Add($"{r.Id} → {referencia}");

            if (r.SiNo is { ValueKind: JsonValueKind.String } s && !existentes.Contains(s.GetString()!))
                rotas.Add($"{r.Id} → {s.GetString()}");
        }

        Assert.Empty(rotas);
    }

    [Fact]
    public void TodasLasReglasSeEvaluanSinExcepcionSobreUnProyectoVacio()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio(muestras: 2));
        var fallos = new List<string>();

        foreach (var r in TodasLasReglas())
        {
            if (r.Tipo == "seleccionAutomaticaEquipos") continue;   // agrupador, no se evalúa
            try { motor.Evaluar(r.Id); }
            catch (Exception ex) { fallos.Add($"{r.Id} ({r.Tipo}): {ex.Message}"); }
        }

        Assert.Empty(fallos);
    }

    [Fact]
    public void ElCatalogoDeEquiposReferenciadoExiste()
    {
        Assert.True(File.Exists(Contexto.RutaEquipos()));

        var grupos = Contexto.Plantilla.Bloques()
            .Select(b => b.Equipos)
            .Where(e => e is not null)
            .ToList();

        var json = JsonDocument.Parse(File.ReadAllText(Contexto.RutaEquipos()));
        var ids = json.RootElement.GetProperty("grupos")
            .EnumerateArray()
            .Select(g => g.GetProperty("id").GetString())
            .ToHashSet();

        Assert.All(grupos, g => Assert.Contains(g, ids));
    }
}
