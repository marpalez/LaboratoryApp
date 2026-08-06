namespace LumNotas.Core.Gestion;

/// <summary>
/// Cuánto tiempo tiene comprometido un técnico.
/// <para>
/// <b>Se cuentan los días ocupados, no la suma de duraciones.</b> Dos servicios que se
/// solapan no ocupan el doble de tiempo: el técnico está ocupado una vez. Sumar las
/// duraciones exageraría la carga justo de quien lleva varios servicios a la vez, que es
/// lo que el responsable quiere detectar.
/// </para>
/// </summary>
public static class Ocupacion
{
    /// <summary>Días distintos cubiertos por unos rangos, uniendo los que se tocan.</summary>
    public static int Dias(IEnumerable<(DateTime Inicio, DateTime Fin)> rangos)
    {
        var ordenados = rangos
            .Select(r => (Inicio: r.Inicio.Date, Fin: r.Fin.Date))
            .Where(r => r.Fin >= r.Inicio)
            .OrderBy(r => r.Inicio)
            .ToList();

        if (ordenados.Count == 0) return 0;

        var total = 0;
        var inicio = ordenados[0].Inicio;
        var fin = ordenados[0].Fin;

        foreach (var rango in ordenados.Skip(1))
        {
            // Dos tramos pegados —uno acaba el lunes y otro empieza el martes— son un
            // solo tramo de trabajo, no dos.
            if (rango.Inicio <= fin.AddDays(1))
            {
                if (rango.Fin > fin) fin = rango.Fin;
                continue;
            }

            total += (int)(fin - inicio).TotalDays + 1;
            inicio = rango.Inicio;
            fin = rango.Fin;
        }

        return total + (int)(fin - inicio).TotalDays + 1;
    }

    /// <summary>
    /// La ocupación en el lenguaje del laboratorio. Por debajo de una semana se dice en
    /// días, que es como se habla de un ensayo corto.
    /// </summary>
    public static string Resumir(int proyectos, int dias)
    {
        var cuantos = $"{proyectos} proyecto{(proyectos == 1 ? "" : "s")}";

        if (dias <= 0) return cuantos;
        if (dias < 7) return $"{cuantos} | {dias} día{(dias == 1 ? "" : "s")}";

        var semanas = (int)Math.Ceiling(dias / 7.0);
        return $"{cuantos} | {semanas} semana{(semanas == 1 ? "" : "s")}";
    }
}
