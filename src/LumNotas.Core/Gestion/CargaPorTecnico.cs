namespace LumNotas.Core.Gestion;

/// <summary>Un servicio tal como lo ve el cálculo de carga: quién, cuándo y por cuánto.</summary>
public sealed record ServicioPlanificado(string Tecnico, DateTime Inicio, DateTime Fin, double? Importe);

/// <summary>Lo que un técnico tiene comprometido en un mes.</summary>
public sealed record CeldaDeCarga(int Ano, int Mes, double Dias, int Capacidad)
{
    /// <summary>Porcentaje de ocupación. Por encima de 100 el técnico está sobrevendido.</summary>
    public double Porcentaje => Capacidad > 0 ? Dias / Capacidad * 100 : 0;

    public bool Vacia => Dias <= 0.05;
}

/// <summary>Una fila de la tabla: un técnico y sus meses.</summary>
public sealed record FilaDeCarga(string Tecnico, IReadOnlyList<CeldaDeCarga> Meses, int SinImporte);

/// <summary>
/// La carga mensual de cada técnico, que es la pregunta que no contesta el calendario:
/// no <i>cuándo</i> está cada servicio, sino <b>si cabe</b>.
/// <para>
/// Cada servicio aporta <c>importe / 80</c> días de trabajo, repartidos entre los meses
/// que abarca en proporción a sus días entre semana. La suma de un técnico se compara
/// con la capacidad del mes, que no es la misma en agosto que en marzo.
/// </para>
/// </summary>
public static class CargaPorTecnico
{
    /// <summary>Etiqueta de los servicios que aún no tienen responsable.</summary>
    public const string SinTecnico = "(sin técnico)";

    /// <summary>
    /// Construye la tabla. Los meses son los que abarcan los servicios, de modo que no
    /// aparecen columnas vacías por delante ni por detrás.
    /// </summary>
    public static (IReadOnlyList<(int Ano, int Mes)> Meses, IReadOnlyList<FilaDeCarga> Filas) Calcular(
        IEnumerable<ServicioPlanificado> servicios, CapacidadMensual capacidad)
    {
        var lista = servicios.Where(s => s.Fin.Date >= s.Inicio.Date).ToList();
        if (lista.Count == 0) return ([], []);

        var meses = MesesQueAbarcan(lista);

        var filas = lista
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Tecnico) ? SinTecnico : s.Tecnico.Trim(),
                     StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(g => g.Key == SinTecnico)
            .ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => Fila(g.Key, [.. g], meses, capacidad))
            .ToList();

        return (meses, filas);
    }

    private static FilaDeCarga Fila(string tecnico, IReadOnlyList<ServicioPlanificado> suyos,
                                    IReadOnlyList<(int Ano, int Mes)> meses, CapacidadMensual capacidad)
    {
        var acumulado = new Dictionary<(int, int), double>();

        foreach (var servicio in suyos)
        {
            // Sin importe no hay trabajo que repartir: se cuenta aparte para que se vea
            // que falta el dato, en vez de rebajar la carga en silencio.
            if (servicio.Importe is not { } importe || importe <= 0) continue;

            foreach (var trozo in RepartoDeTrabajo.Repartir(
                         servicio.Inicio, servicio.Fin, capacidad.DiasDeTrabajo(importe)))
            {
                var clave = (trozo.Ano, trozo.Mes);
                acumulado[clave] = acumulado.GetValueOrDefault(clave) + trozo.Dias;
            }
        }

        var celdas = meses
            .Select(m => new CeldaDeCarga(m.Ano, m.Mes,
                                          acumulado.GetValueOrDefault((m.Ano, m.Mes)),
                                          capacidad.Dias(m.Mes)))
            .ToList();

        return new FilaDeCarga(tecnico, celdas, suyos.Count(s => s.Importe is not > 0));
    }

    private static IReadOnlyList<(int Ano, int Mes)> MesesQueAbarcan(IReadOnlyList<ServicioPlanificado> servicios)
    {
        var primero = new DateTime(servicios.Min(s => s.Inicio).Year, servicios.Min(s => s.Inicio).Month, 1);
        var ultimo = new DateTime(servicios.Max(s => s.Fin).Year, servicios.Max(s => s.Fin).Month, 1);

        var meses = new List<(int, int)>();

        // Tope de seguridad: una fecha disparatada no puede generar una tabla infinita.
        for (var mes = primero; mes <= ultimo && meses.Count < 36; mes = mes.AddMonths(1))
            meses.Add((mes.Year, mes.Month));

        return meses;
    }
}
