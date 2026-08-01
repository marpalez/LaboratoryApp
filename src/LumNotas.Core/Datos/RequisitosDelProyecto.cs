using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Datos;

/// <summary>
/// Datos de cabecera sin los que no se puede empezar a ensayar. En el Excel eran los
/// avisos «FALTA POR MARCAR INFORMACIÓN DE PROYECTO», «FALTA POR MARCAR PRIMERA/SEGUNDA
/// CIFRA IP» y «FALTA POR MARCAR NORMAS A APLICAR» de la hoja RESUMEN.
/// </summary>
/// <remarks>
/// Qué es obligatorio <b>lo decide la plantilla</b>, no este código: cada norma pide
/// cosas distintas —luminarias exige Ta, clase, grado IP y partes ‑2; la 62031 pide Tc
/// y la clasificación del módulo— y ninguna de esas listas debe vivir en C#.
/// El técnico 2 es opcional: hay proyectos con un solo técnico.
/// </remarks>
public static class RequisitosDelProyecto
{
    /// <summary>Marcador que la aplicación pone al crear un proyecto: cuenta como vacío.</summary>
    public const string CodigoSinAsignar = "NUEVO";

    public static IReadOnlyList<string> Faltantes(PlantillaEnsayos plantilla, DatosProyecto datos)
    {
        var faltan = new List<string>();
        Motor.MotorDeReglas? motor = null;

        foreach (var campo in plantilla.Proyecto.Campos.Where(c => c.Obligatorio))
        {
            // Un campo condicionado solo se exige cuando toca: la profundidad de
            // inmersión es obligatoria, pero únicamente si hay objetivo IPX7 o IPX8.
            if (campo.VisibleSi is { } condicion && !Aplica(condicion))
                continue;

            if (!TieneValor(campo, datos)) faltan.Add(campo.Etiqueta);
        }

        return faltan;

        bool Aplica(string condicion)
        {
            motor ??= new Motor.MotorDeReglas(plantilla, datos);

            // Si la regla falla, el campo se exige: mejor pedir un dato de más que
            // dar por buena una cabecera incompleta.
            try { return motor.EsVerdadera(condicion); }
            catch (Exception) { return true; }
        }
    }

    public static bool Completo(PlantillaEnsayos plantilla, DatosProyecto datos)
        => Faltantes(plantilla, datos).Count == 0;

    private static bool TieneValor(Campo campo, DatosProyecto datos) => campo.Id switch
    {
        // Los dos campos que no viven en el almacén general.
        "codigoServicio" => !string.IsNullOrWhiteSpace(datos.CodigoServicio)
                            && datos.CodigoServicio != CodigoSinAsignar,
        "numeroMuestras" => datos.NumeroMuestras >= 1,

        _ when campo.Multiple => datos.Seleccion(campo.Id).Count > 0,

        // Declarado por muestra: hay que rellenarlo en todas, no en una cualquiera.
        _ when campo.PorMuestra => datos.NumeroMuestras >= 1
                                   && datos.Muestras.All(m =>
                                       datos.Obtener("proyecto", campo.Id, m) is string v
                                       && !string.IsNullOrWhiteSpace(v)),

        _ => datos.Obtener("proyecto", campo.Id) switch
        {
            null => false,
            string texto => !string.IsNullOrWhiteSpace(texto),
            _ => true
        }
    };
}
