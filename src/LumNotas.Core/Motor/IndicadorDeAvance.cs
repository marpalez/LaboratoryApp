using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Motor;

/// <summary>
/// Indicador de avance del proyecto. Mantiene los pesos del Excel y añade el
/// contador de apartados, según la decisión DD-11 / D-18.
/// </summary>
public sealed class IndicadorDeAvance(MotorDeReglas motor)
{
    public Resultado Calcular()
    {
        var aportaciones = new List<Aportacion>();

        foreach (var bloque in motor.Plantilla.Bloques())
            foreach (var regla in PlantillaEnsayos.ReglasDe(bloque))
                if (regla.Tipo == "peso" && motor.Evaluar(regla.Id) is Aportacion a)
                    aportaciones.Add(a);

        var total = aportaciones.Sum(a => a.PesoEnProyecto);
        var ejecutado = aportaciones.Sum(a => a.PesoFinalizado);

        return new Resultado(
            PesoTotal: total,
            PesoEjecutado: ejecutado,
            ApartadosAplicables: aportaciones.Count(a => a.Aplica),
            ApartadosCompletados: aportaciones.Count(a => a.Terminado),
            Aportaciones: aportaciones);
    }

    public sealed record Resultado(
        int PesoTotal,
        int PesoEjecutado,
        int ApartadosAplicables,
        int ApartadosCompletados,
        IReadOnlyList<Aportacion> Aportaciones)
    {
        /// <summary>Porcentaje ponderado, el indicador que ya existía en el Excel.</summary>
        public double PorcentajePonderado => PesoTotal == 0 ? 0 : (double)PesoEjecutado / PesoTotal * 100;

        /// <summary>Indicador secundario añadido en la aplicación (D-18).</summary>
        public string Contador => $"{ApartadosCompletados}/{ApartadosAplicables}";

        public int PesoPendiente => PesoTotal - PesoEjecutado;

        /// <summary>
        /// Avance de un proyecto que lleva varias normas: se suman los de cada una.
        /// Un servicio de luminarias con la 62031 añadida tiene un único avance.
        /// </summary>
        public static Resultado Sumar(IEnumerable<Resultado> partes)
        {
            var lista = partes.ToList();
            return new Resultado(
                PesoTotal: lista.Sum(r => r.PesoTotal),
                PesoEjecutado: lista.Sum(r => r.PesoEjecutado),
                ApartadosAplicables: lista.Sum(r => r.ApartadosAplicables),
                ApartadosCompletados: lista.Sum(r => r.ApartadosCompletados),
                Aportaciones: [.. lista.SelectMany(r => r.Aportaciones)]);
        }
    }
}
