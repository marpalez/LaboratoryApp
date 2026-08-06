using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Los carriles del calendario: cuántas filas hacen falta para dibujar unos trabajos y cuál
/// le toca a cada uno.
/// <para>
/// Lo que se vigila aquí es que <b>una fila por trabajo deje de ser la norma</b>. Un técnico
/// con veinte proyectos seguidos tiene que caber en un renglón; lo que obliga a bajar es que
/// dos coincidan, no que existan.
/// </para>
/// </summary>
public class CarrilesDelCalendarioTests
{
    private sealed record Trabajo(string Nombre, DateTime Desde, DateTime Hasta);

    /// <summary>Días de enero de 2026, que es todo lo que hace falta para estas pruebas.</summary>
    private static Trabajo En(string nombre, int desde, int hasta)
        => new(nombre, new DateTime(2026, 1, desde), new DateTime(2026, 1, hasta));

    private static IReadOnlyList<IReadOnlyList<Trabajo>> Repartir(params Trabajo[] trabajos)
        => CarrilesDelCalendario.Repartir(trabajos, t => (t.Desde, t.Hasta));

    private static string[] Nombres(IReadOnlyList<Trabajo> carril) => [.. carril.Select(t => t.Nombre)];

    /// <summary>
    /// <b>Lo que motivó todo esto.</b> Trabajos uno detrás de otro, sin coincidir nunca:
    /// antes eran cinco filas y ahora son una.
    /// </summary>
    [Fact]
    public void LosQueNoSePisanCabenEnUnaSolaFila()
    {
        var carriles = Repartir(
            En("A", 1, 3), En("B", 5, 7), En("C", 9, 11), En("D", 13, 15), En("E", 17, 19));

        Assert.Single(carriles);
        Assert.Equal(["A", "B", "C", "D", "E"], Nombres(carriles[0]));
    }

    /// <summary>Solaparse es lo único que abre fila nueva.</summary>
    [Fact]
    public void ElQueSePisaBajaAlCarrilSiguiente()
    {
        var carriles = Repartir(En("A", 1, 10), En("B", 5, 15));

        Assert.Equal(2, carriles.Count);
        Assert.Equal(["A"], Nombres(carriles[0]));
        Assert.Equal(["B"], Nombres(carriles[1]));
    }

    /// <summary>
    /// <b>Compartir un solo día ya cuenta como pisarse.</b> Pegadas sin hueco, dos barras se
    /// leen como una sola barra larga y el calendario mentiría sobre cuándo acaba cada una.
    /// </summary>
    [Fact]
    public void TocarseEnUnDiaBastaParaSepararlos()
    {
        var pegados = Repartir(En("A", 1, 10), En("B", 10, 15));
        Assert.Equal(2, pegados.Count);

        // Con un día de por medio sí comparten carril.
        var separados = Repartir(En("A", 1, 10), En("B", 11, 15));
        Assert.Single(separados);
        Assert.Equal(["A", "B"], Nombres(separados[0]));
    }

    /// <summary>
    /// Cada uno sube todo lo que puede: si en un carril de arriba quedó un hueco, se
    /// aprovecha en vez de estrenar fila.
    /// </summary>
    [Fact]
    public void SeSubeAlHuecoQueQuedoLibreArriba()
    {
        // «Largo» ocupa el mes entero; los otros dos caben antes y después, en el mismo carril.
        var carriles = Repartir(En("Largo", 1, 31), En("Corto", 1, 5), En("Tardio", 10, 20));

        Assert.Equal(2, carriles.Count);
        Assert.Equal(["Corto", "Tardio"], Nombres(carriles[0]));
        Assert.Equal(["Largo"], Nombres(carriles[1]));
    }

    /// <summary>
    /// Salen tantas filas como trabajos coincidan <b>el día más cargado</b>, ni una más.
    /// Cuatro que se solapan de dos en dos caben en dos carriles.
    /// </summary>
    [Fact]
    public void NoSeGastanMasFilasDeLasQueHacenFalta()
    {
        var carriles = Repartir(
            En("A", 1, 10), En("B", 5, 12),   // se pisan entre ellos
            En("C", 14, 20), En("D", 16, 22)); // y estos dos, pero no con los primeros

        Assert.Equal(2, carriles.Count);
        Assert.Equal(["A", "C"], Nombres(carriles[0]));
        Assert.Equal(["B", "D"], Nombres(carriles[1]));
    }

    /// <summary>Tres a la vez son tres filas: no hay forma de dibujarlos con menos.</summary>
    [Fact]
    public void TresALaVezSonTresFilas()
    {
        var carriles = Repartir(En("A", 1, 20), En("B", 2, 21), En("C", 3, 22));

        Assert.Equal(3, carriles.Count);
        Assert.All(carriles, c => Assert.Single(c));
    }

    /// <summary>
    /// <b>La hora se descarta.</b> Las fechas del calendario son días; una guardada con hora
    /// mandaba a un carril nuevo a un trabajo que empieza cuando el anterior ya ha acabado.
    /// </summary>
    [Fact]
    public void LaHoraNoAbreFilasDeMas()
    {
        var carriles = CarrilesDelCalendario.Repartir(
            new[]
            {
                new Trabajo("A", new DateTime(2026, 1, 1, 8, 30, 0), new DateTime(2026, 1, 10, 17, 0, 0)),
                new Trabajo("B", new DateTime(2026, 1, 11, 9, 0, 0), new DateTime(2026, 1, 15, 14, 0, 0))
            },
            t => (t.Desde, t.Hasta));

        Assert.Single(carriles);
    }

    /// <summary>
    /// Un fin anterior al inicio es un dato mal guardado. No puede dejar el carril ocupado
    /// hacia atrás: descolocaría a todos los que vengan después.
    /// </summary>
    [Fact]
    public void UnTramoDelRevesNoDescolocaAlResto()
    {
        var carriles = Repartir(En("Roto", 10, 2), En("Bueno", 12, 20));

        Assert.Single(carriles);
        Assert.Equal(["Roto", "Bueno"], Nombres(carriles[0]));
    }

    /// <summary>Un solo día ocupa su día, y el trabajo siguiente empieza al otro.</summary>
    [Fact]
    public void UnTrabajoDeUnSoloDiaOcupaEseDia()
    {
        Assert.Equal(2, Repartir(En("A", 5, 5), En("B", 5, 5)).Count);
        Assert.Single(Repartir(En("A", 5, 5), En("B", 6, 6)));
    }

    [Fact]
    public void SinTrabajosNoHayCarriles() => Assert.Empty(Repartir());
}
