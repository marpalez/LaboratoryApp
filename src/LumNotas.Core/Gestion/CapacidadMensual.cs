using System.Text.Json;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Cuánto trabajo cabe en un mes y cuánto trabajo supone una oferta.
/// <para>
/// El laboratorio mide el trabajo de un servicio <b>en horas</b>: divide el importe de la
/// oferta entre 105 y multiplica por 1,3. Una oferta de 2 000 € son unas 25 horas, algo
/// más de tres jornadas. Y no todos los meses rinden igual —agosto y diciembre están
/// medio vacíos—, así que la capacidad se declara mes a mes.
/// </para>
/// <para>
/// Vive junto a la lista de técnicos, en la carpeta compartida, para que la tarifa y el
/// calendario laboral sean los mismos para todos.
/// </para>
/// </summary>
public sealed class CapacidadMensual
{
    public const string NombreDeFichero = "capacidad.json";

    /// <summary>
    /// Euros de oferta entre los que se divide para sacar las horas. <b>No es la tarifa
    /// que se factura</b>: la tarifa del laboratorio es de 80 €/hora, y este número junto
    /// con <see cref="FactorPorDefecto"/> es la cuenta con la que se estima el trabajo.
    /// Multiplicando, sale una hora por cada 80,77 € — de ahí que cuadre con la tarifa.
    /// </summary>
    public const double EurosPorHoraPorDefecto = 105;

    /// <summary>
    /// Lo que se multiplica después de dividir. Sale de la práctica del laboratorio, no
    /// de una fórmula: se deja configurable porque es justo lo que se ajusta con los años.
    /// </summary>
    public const double FactorPorDefecto = 1.3;

    /// <summary>
    /// Horas de una jornada. Hace falta para comparar las horas de trabajo con la
    /// capacidad del mes, que el laboratorio declara <b>en días</b>.
    /// </summary>
    public const double HorasPorDiaPorDefecto = 8;

    /// <summary>
    /// Días de trabajo que caben en cada mes, de enero a diciembre. Agosto y diciembre
    /// son cortos porque el laboratorio trabaja dos y tres semanas respectivamente.
    /// </summary>
    public static IReadOnlyList<int> DiasPorDefecto =>
        [22, 22, 22, 22, 22, 22, 22, 10, 22, 22, 22, 15];

    public double EurosPorHora { get; set; } = EurosPorHoraPorDefecto;

    public double Factor { get; set; } = FactorPorDefecto;

    public double HorasPorDia { get; set; } = HorasPorDiaPorDefecto;

    public List<int> DiasPorMes { get; set; } = [.. DiasPorDefecto];

    /// <summary>Capacidad de un mes (1‑12). Nunca menos de un día, para no dividir por cero.</summary>
    public int Dias(int mes)
        => mes >= 1 && mes <= DiasPorMes.Count ? Math.Max(1, DiasPorMes[mes - 1]) : 22;

    /// <summary>
    /// Horas de trabajo que supone una oferta: <c>importe / 105 × 1,3</c>, que es como lo
    /// mide el laboratorio.
    /// </summary>
    public double HorasDeTrabajo(double importe)
        => EurosPorHora > 0 ? importe / EurosPorHora * Factor : 0;

    /// <summary>
    /// Las mismas horas, en jornadas, que es la unidad en la que está la capacidad del
    /// mes. Todo el reparto entre meses trabaja en días.
    /// </summary>
    public double DiasDeTrabajo(double importe)
        => HorasPorDia > 0 ? HorasDeTrabajo(importe) / HorasPorDia : 0;

    /// <summary>
    /// Euros por hora que salen de la cuenta, para poder comprobar de un vistazo que
    /// cuadra con la tarifa del laboratorio.
    /// </summary>
    public double EurosPorHoraEfectivos => Factor > 0 ? EurosPorHora / Factor : 0;

    public static CapacidadMensual Cargar(string carpeta)
    {
        try
        {
            var ruta = Path.Combine(carpeta, NombreDeFichero);
            if (!File.Exists(ruta)) return new CapacidadMensual();

            var leida = JsonSerializer.Deserialize<CapacidadMensual>(File.ReadAllText(ruta));
            if (leida is null) return new CapacidadMensual();

            // Un fichero a medias no puede dejar el cálculo sin base. También cubre los
            // guardados antes de pasar a horas, que no traen ninguno de estos campos.
            if (leida.DiasPorMes.Count != 12) leida.DiasPorMes = [.. DiasPorDefecto];
            if (leida.EurosPorHora <= 0) leida.EurosPorHora = EurosPorHoraPorDefecto;
            if (leida.Factor <= 0) leida.Factor = FactorPorDefecto;
            if (leida.HorasPorDia <= 0) leida.HorasPorDia = HorasPorDiaPorDefecto;

            return leida;
        }
        catch
        {
            return new CapacidadMensual();
        }
    }

    public void Guardar(string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        var texto = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        var temporal = Path.Combine(carpeta, Path.GetRandomFileName());
        File.WriteAllText(temporal, texto, new System.Text.UTF8Encoding(false));

        var ruta = Path.Combine(carpeta, NombreDeFichero);
        if (File.Exists(ruta)) File.Replace(temporal, ruta, destinationBackupFileName: null);
        else File.Move(temporal, ruta);
    }
}
