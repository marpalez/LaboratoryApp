namespace LumNotas.Core.Gestion;

/// <summary>Días de trabajo que un servicio deja caer en un mes concreto.</summary>
public sealed record TrabajoDeUnMes(int Ano, int Mes, double Dias);

/// <summary>
/// Cómo se reparte el trabajo de un servicio entre los meses que abarca.
/// <para>
/// El trabajo de un servicio sale de su oferta, pero no cae de golpe en un mes: se
/// prorratea según los <b>días entre semana</b> que el servicio tiene en cada uno. Los
/// fines de semana no cuentan.
/// </para>
/// <para>
/// <b>Supone esfuerzo uniforme</b>, y en un laboratorio no lo es: montaje, luego dos días
/// en cámara sin tocar nada, luego medir. En un servicio suelto el reparto es impreciso;
/// sobre la carga de un técnico con varios servicios los errores se compensan y sirve
/// para planificar, que es para lo que se usa.
/// </para>
/// </summary>
public static class RepartoDeTrabajo
{
    /// <summary>Días de lunes a viernes entre dos fechas, ambas incluidas.</summary>
    public static int DiasEntreSemana(DateTime inicio, DateTime fin)
    {
        if (fin.Date < inicio.Date) return 0;

        var cuenta = 0;
        for (var dia = inicio.Date; dia <= fin.Date; dia = dia.AddDays(1))
            if (dia.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) cuenta++;

        return cuenta;
    }

    /// <summary>
    /// Reparte los días de trabajo de un servicio entre los meses que toca, en
    /// proporción a los días entre semana que tiene en cada uno.
    /// </summary>
    public static IReadOnlyList<TrabajoDeUnMes> Repartir(DateTime inicio, DateTime fin, double diasDeTrabajo)
    {
        if (fin.Date < inicio.Date || diasDeTrabajo <= 0) return [];

        var porMes = new List<(int Ano, int Mes, int Laborables)>();

        for (var mes = new DateTime(inicio.Year, inicio.Month, 1);
             mes <= new DateTime(fin.Year, fin.Month, 1);
             mes = mes.AddMonths(1))
        {
            var desde = Mayor(mes, inicio.Date);
            var hasta = Menor(mes.AddMonths(1).AddDays(-1), fin.Date);
            porMes.Add((mes.Year, mes.Month, DiasEntreSemana(desde, hasta)));
        }

        var total = porMes.Sum(m => m.Laborables);

        // Un servicio que cae entero en un fin de semana no tiene dónde prorratearse; se
        // le imputa todo a su mes de inicio en vez de desaparecer del recuento.
        if (total == 0)
            return [new TrabajoDeUnMes(inicio.Year, inicio.Month, diasDeTrabajo)];

        return [.. porMes.Where(m => m.Laborables > 0)
                         .Select(m => new TrabajoDeUnMes(m.Ano, m.Mes, diasDeTrabajo * m.Laborables / total))];
    }

    private static DateTime Mayor(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Menor(DateTime a, DateTime b) => a < b ? a : b;
}
