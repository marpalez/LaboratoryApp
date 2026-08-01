namespace LumNotas.Core.Motor;

/// <summary>
/// Los cálculos de ingeniería del libro (sección 6 de docs/REGLAS-NEGOCIO.md).
/// Están en código y no en la plantilla a propósito: son fórmulas de norma, no configuración.
/// Leen los campos a través del motor para que funcionen también con campos derivados
/// (el tamaño de muestra se declara en «generales» y se reutiliza en IP).
/// </summary>
public static class Calculos
{
    /// <summary>Valor devuelto por <see cref="RadioEnsayo"/> cuando ningún arco disponible sirve.</summary>
    public const string CabezaRegadera = "Cab. Regadera";

    /// <summary>Valor devuelto cuando no hay dimensiones y el radio no es aplicable.</summary>
    public const string NoAplica = "N/A";

    /// <summary>Arcos de lluvia disponibles en el laboratorio, en cm.</summary>
    public static readonly int[] ArcosDisponibles = [20, 40, 60, 80, 100, 120, 140];

    private static double Dimension(MotorDeReglas motor, string ambito, string campo, int muestra)
    {
        var ruta = motor.Resolver(ambito, campo);
        return motor.Datos.Numero(ruta.Ambito, ruta.Campo, muestra) ?? 0;
    }

    /// <summary>C-08 · Altura efectiva: la mitad si el objetivo es IPX4 (Datos!AQ25).</summary>
    public static double AlturaEfectiva(MotorDeReglas motor, string ambito, int muestra)
    {
        var alto = Dimension(motor, ambito, "tamano[0].alto", muestra);

        // El objetivo puede ser distinto en cada muestra, así que se mira el suyo.
        return motor.Datos.GradosDe("ipSegundaCifra", muestra).Contains("IPX4") ? alto / 2 : alto;
    }

    /// <summary>C-04 · Semidiagonal de la base (Datos!AQ29).</summary>
    public static double SemidiagonalBase(MotorDeReglas motor, string ambito, int muestra)
    {
        var ancho = Dimension(motor, ambito, "tamano[0].ancho", muestra);
        var profundo = Dimension(motor, ambito, "tamano[0].profundo", muestra);
        return Math.Sqrt(Math.Pow(ancho / 2, 2) + Math.Pow(profundo / 2, 2));
    }

    /// <summary>C-05 · Distancia d desde el centro del arco (Datos!AQ30).</summary>
    public static double DistanciaArco(MotorDeReglas motor, string ambito, int muestra)
        => Math.Sqrt(Math.Pow(AlturaEfectiva(motor, ambito, muestra), 2)
                     + Math.Pow(SemidiagonalBase(motor, ambito, muestra), 2));

    /// <summary>C-06 · Radio máximo necesario = d + 20 cm (Datos!AQ32).</summary>
    public static double RadioMaximo(MotorDeReglas motor, string ambito, int muestra)
        => DistanciaArco(motor, ambito, muestra) + 20;

    /// <summary>
    /// C-07 · Arco a utilizar (Datos!AQ33). Devuelve un <see cref="int"/> con el arco en cm,
    /// <see cref="CabezaRegadera"/> si ninguno sirve, o <see cref="NoAplica"/> si no hay dimensiones.
    /// </summary>
    public static object RadioEnsayo(MotorDeReglas motor, string ambito, int muestra)
    {
        var rMax = RadioMaximo(motor, ambito, muestra);

        // Sin dimensiones, d = 0 y el radio máximo se queda en los 20 cm del margen fijo.
        if (Math.Abs(rMax - 20) < 1e-9) return NoAplica;

        foreach (var arco in ArcosDisponibles)
            if (arco >= rMax) return arco;

        return CabezaRegadera;
    }

    /// <summary>C-01 · Velocidad de viento según altura de montaje, en m/s (Datos!CF28).</summary>
    public static double VelocidadViento(AlturaMontaje altura) => altura switch
    {
        AlturaMontaje.HastaOchoMetros => 45,
        AlturaMontaje.EntreOchoYQuinceMetros => 52,
        AlturaMontaje.MasDeQuinceMetros => 57,
        _ => 0
    };

    /// <summary>C-02 · Fuerza de carga estática para la parte -2-3, en N (Datos!CF29).</summary>
    public static double FuerzaViento(double areaM2, AlturaMontaje altura)
        => 0.5 * 1.225 * areaM2 * 1.2 * Math.Pow(VelocidadViento(altura), 2);

    /// <summary>C-03 · Fuerza de carga estática para la parte -2-5, en N (Toma de notas!G428).</summary>
    public static double FuerzaCargaEstatica25(double areaM2) => areaM2 * 2400;

    internal static object? Evaluar(string nombre, MotorDeReglas motor, string ambito) => nombre switch
    {
        "alturaEfectiva" => AlturaEfectiva(motor, ambito, 1),
        "semidiagonalBase" => SemidiagonalBase(motor, ambito, 1),
        "distanciaArco" => DistanciaArco(motor, ambito, 1),
        "radioMaximo" => RadioMaximo(motor, ambito, 1),
        "radioEnsayo" => RadioEnsayo(motor, ambito, 1),
        _ => throw new KeyNotFoundException(
            $"Cálculo '{nombre}' no registrado. Añádelo en Calculos.cs o revisa la plantilla.")
    };
}

public enum AlturaMontaje
{
    SinIndicar = 0,
    HastaOchoMetros = 1,
    EntreOchoYQuinceMetros = 2,
    MasDeQuinceMetros = 3
}
