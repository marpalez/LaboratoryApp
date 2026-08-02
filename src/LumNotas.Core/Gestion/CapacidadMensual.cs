using System.Text.Json;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Cuánto trabajo cabe en un mes y cuánto trabajo supone una oferta.
/// <para>
/// El laboratorio mide el trabajo de un servicio dividiendo el importe de la oferta
/// entre 80 €: una oferta de 2 000 € son 25 días de trabajo. Y no todos los meses
/// rinden igual —agosto y diciembre están medio vacíos—, así que la capacidad se
/// declara mes a mes.
/// </para>
/// <para>
/// Vive junto a la lista de técnicos, en la carpeta compartida, para que la tarifa y el
/// calendario laboral sean los mismos para todos.
/// </para>
/// </summary>
public sealed class CapacidadMensual
{
    public const string NombreDeFichero = "capacidad.json";

    /// <summary>Euros de oferta que equivalen a un día de trabajo.</summary>
    public const double EurosPorDiaPorDefecto = 80;

    /// <summary>
    /// Días de trabajo que caben en cada mes, de enero a diciembre. Agosto y diciembre
    /// son cortos porque el laboratorio trabaja dos y tres semanas respectivamente.
    /// </summary>
    public static IReadOnlyList<int> DiasPorDefecto =>
        [22, 22, 22, 22, 22, 22, 22, 10, 22, 22, 22, 15];

    public double EurosPorDia { get; set; } = EurosPorDiaPorDefecto;

    public List<int> DiasPorMes { get; set; } = [.. DiasPorDefecto];

    /// <summary>Capacidad de un mes (1‑12). Nunca menos de un día, para no dividir por cero.</summary>
    public int Dias(int mes)
        => mes >= 1 && mes <= DiasPorMes.Count ? Math.Max(1, DiasPorMes[mes - 1]) : 22;

    /// <summary>Días de trabajo que supone una oferta.</summary>
    public double DiasDeTrabajo(double importe)
        => EurosPorDia > 0 ? importe / EurosPorDia : 0;

    public static CapacidadMensual Cargar(string carpeta)
    {
        try
        {
            var ruta = Path.Combine(carpeta, NombreDeFichero);
            if (!File.Exists(ruta)) return new CapacidadMensual();

            var leida = JsonSerializer.Deserialize<CapacidadMensual>(File.ReadAllText(ruta));
            if (leida is null) return new CapacidadMensual();

            // Un fichero a medias no puede dejar el cálculo sin base.
            if (leida.DiasPorMes.Count != 12) leida.DiasPorMes = [.. DiasPorDefecto];
            if (leida.EurosPorDia <= 0) leida.EurosPorDia = EurosPorDiaPorDefecto;

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
