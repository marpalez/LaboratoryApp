using LumNotas.Core.Datos;

namespace LumNotas.Core.Motor;

/// <summary>
/// Reglas que no se pueden expresar de forma declarativa en la plantilla.
/// Son deliberadamente pocas: si esta lista crece mucho, es señal de que hace
/// falta un patrón nuevo en la plantilla en vez de más código a medida.
/// </summary>
/// <remarks>
/// Firma: <c>(motor, ámbito de datos de la regla, parámetro declarado en la plantilla)</c>.
/// </remarks>
public static class Predicados
{
    /// <summary>Opción del grado IK que significa que la muestra no se ensaya a impacto.</summary>
    public const string SinIk = "No IK";

    private static readonly Dictionary<string, Func<MotorDeReglas, string, string?, bool>> Registro =
        new(StringComparer.Ordinal)
    {
        // R-PROY-completo · RESUMEN PROYECTO LUM!B11
        // Sin la cabecera rellena no se empieza a ensayar.
        ["proyectoCompleto"] = (motor, _, _) => RequisitosDelProyecto.Completo(motor.Plantilla, motor.Datos),

        // R-07.10-01 · Datos ensayos LUM.!L99 y R-09-esClaseI · T21
        // El doble aislamiento no aplica en Clase I; la tierra solo aplica en Clase I.
        ["esClaseI"] = (motor, _, _) => motor.Datos.Clase == Clase.I,

        // R-12.3-kikusui · Datos ensayos LUM.!BE34
        // Una portátil de Clase I obliga a usar la red especial de Kikusui para fugas.
        ["portatilClaseI"] = (motor, _, _) =>
            motor.Datos.Partes2.Contains("-2-4") && motor.Datos.Clase == Clase.I,

        // R-P2-01 · Datos ensayos LUM.!CF20, CF68, CM35, CM68, CM89, CF89, CF132
        // Un ensayo de una parte -2 solo aplica si esa parte está marcada en el proyecto.
        ["aplicaParte2"] = (motor, _, parte) =>
            parte is not null && motor.Datos.Partes2.Contains(parte),

        // R-GEN-01 · Datos ensayos LUM.!D21
        // Los tres miran también el grado declarado por muestra, porque la 60529 lo pide
        // así: un mismo servicio puede traer productos con objetivos distintos.
        ["requiereDimensiones"] = (motor, _, _) =>
            motor.Datos.GradosDe("ipSegundaCifra").Any(g => g is "IPX3" or "IPX4"),

        // R-11-16 · Datos ensayos LUM.!AR59
        ["hayGradoSegundaCifra"] = (motor, _, _) => motor.Datos.GradosDe("ipSegundaCifra").Any(),

        // R-11-20 · Datos ensayos LUM.!AR70
        ["hayGradoPrimeraCifra"] = (motor, _, _) => motor.Datos.GradosDe("ipPrimeraCifra").Any(),

        // R-60529-esIp5x · Toma de Notas IP-IK!D27
        // Solo el IP5X pide anotar cuánto duró el ensayo de polvo.
        ["requiereDuracionIp5x"] = (motor, _, _) => motor.Datos.GradosDe("ipPrimeraCifra").Contains("IP5X"),

        // R-60529-inmersion · RESUMEN PROYECTO IP-IK!M23:M24
        // La profundidad y la temperatura del agua solo se anotan si alguna muestra se
        // sumerge, que es lo que distingue al IPX7 y al IPX8.
        ["requiereInmersion"] = (motor, _, _) =>
            motor.Datos.GradosDe("ipSegundaCifra").Any(g => g is "IPX7" or "IPX8"),

        // R-62262-ik-metodoElegible · Toma de Notas IP-IK!C151
        // De IK01 a IK06 se usa siempre el martillo de resorte (Ehb) y no hay nada que
        // elegir; de IK07 en adelante el técnico decide entre péndulo y caída vertical.
        // R-60598-hayIk · el IK dejó de ser una toma de notas aparte para luminarias:
        // se elige por muestra y su sección aparece en cuanto alguna lo lleva.
        ["hayGradoIk"] = (motor, _, _) =>
            motor.Datos.GradosDe("gradoIk").Any(g => g != SinIk),

        ["requiereMetodoDeGolpeo"] = (motor, _, _) =>
            motor.Datos.GradosDe("gradoIk").Any(g =>
                g != SinIk && int.TryParse(g.AsSpan(2), out var grado) && grado >= 7),

        // R-11-06 · Datos ensayos LUM.!AV35
        ["requiereCabezaRegadera"] = (motor, ambito, _) =>
            motor.Datos.Muestras.Any(m => Calculos.RadioEnsayo(motor, ambito, m) is Calculos.CabezaRegadera),

        // R-07.12-06 · Datos ensayos LUM.!L147
        ["prensaestopasDeclarados"] = (motor, ambito, _) =>
            motor.Datos.Marcada(ambito, "sinPrensaestopas", "sinPrensaestopas")
            || motor.Datos.ValoresDeTodasLasMuestras(ambito, "prensaestopas")
                   .Any(v => v is string s && !string.IsNullOrWhiteSpace(s)),

        // R-07.12-08b · compara los diámetros introducidos con los prensaestopas declarados
        ["diametrosCubrenPrensaestopas"] = (motor, _, _) =>
        {
            var declarados = motor.Evaluar("R-07.12-07") as int? ?? 0;
            var introducidos = motor.Evaluar("R-07.12-08") as int? ?? 0;
            return introducidos >= declarados;
        }
    };

    public static bool Evaluar(string nombre, MotorDeReglas motor, string ambito, string? parametro = null)
        => Registro.TryGetValue(nombre, out var predicado)
            ? predicado(motor, ambito, parametro)
            : throw new KeyNotFoundException(
                $"Predicado '{nombre}' no registrado. Añádelo en Predicados.cs o revisa la plantilla.");

    public static IReadOnlyCollection<string> Registrados => Registro.Keys;
}
