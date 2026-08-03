using System.Collections.ObjectModel;
using System.Globalization;
using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Tabla de técnicos por meses con el porcentaje de ocupación. Es la tercera pregunta
/// del responsable: el tablero dice <i>qué falta</i>, el calendario <i>cuándo</i>, y esto
/// <b>si cabe</b>.
/// </summary>
public sealed class CargaViewModel : ObservableObject
{
    private IReadOnlyList<ResumenDeProyecto> _proyectos = [];
    private string _mensaje = "";

    public ObservableCollection<string> Meses { get; } = [];
    public ObservableCollection<FilaDeCargaViewModel> Filas { get; } = [];

    public bool HayDatos => Filas.Count > 0;

    public string Mensaje
    {
        get => _mensaje;
        private set => Establecer(ref _mensaje, value);
    }

    /// <summary>Recibe los proyectos ya filtrados por el tablero y rehace la tabla.</summary>
    public void Cargar(IReadOnlyList<ResumenDeProyecto> proyectos)
    {
        _proyectos = proyectos;
        Recalcular();
    }

    public void Recalcular()
    {
        var capacidad = ServicioDeCapacidad.Capacidad;

        var servicios = _proyectos
            .Where(p => p.Planificacion.HayFechas)
            .Select(p => new ServicioPlanificado(
                p.Tecnico, p.Planificacion.Inicio!.Value, p.Planificacion.FinEfectivo!.Value,
                p.Planificacion.Importe))
            .ToList();

        var (meses, filas) = CargaPorTecnico.Calcular(servicios, capacidad);

        Meses.Clear();
        foreach (var (ano, mes) in meses) Meses.Add(Rotulo(ano, mes));

        Filas.Clear();
        foreach (var fila in filas) Filas.Add(new FilaDeCargaViewModel(fila));

        var sinImporte = filas.Sum(f => f.SinImporte);
        Mensaje = servicios.Count == 0
            ? "No hay servicios con fechas que repartir."
            : $"Importe ÷ {capacidad.EurosPorHora:0.##} × {capacidad.Factor:0.###} = horas"
              + $"  ·  {capacidad.HorasPorDia:0.##} h por jornada"
              + (sinImporte == 0 ? "" : $"  ·  {sinImporte} servicio{(sinImporte == 1 ? "" : "s")} sin importe");

        Notificar(nameof(HayDatos));
    }

    private static string Rotulo(int ano, int mes)
    {
        var cultura = EjeDeSemanas.CulturaDelLaboratorio;
        var nombre = cultura.TextInfo.ToTitleCase(
            new DateTime(ano, mes, 1).ToString("MMM", cultura)).TrimEnd('.');

        return $"{nombre}\n{ano}";
    }
}

/// <summary>Una fila de la tabla: un técnico y su ocupación mes a mes.</summary>
public sealed class FilaDeCargaViewModel
{
    public FilaDeCargaViewModel(FilaDeCarga fila)
    {
        Tecnico = fila.Tecnico;
        Meses = [.. fila.Meses.Select(c => new CeldaDeCargaViewModel(c))];

        SinImporte = fila.SinImporte == 0
            ? ""
            : $"{fila.SinImporte} sin importe";
    }

    public string Tecnico { get; }
    public IReadOnlyList<CeldaDeCargaViewModel> Meses { get; }
    public string SinImporte { get; }
    public bool FaltaAlgunImporte => SinImporte.Length > 0;
}

/// <summary>Una celda: el porcentaje de un técnico en un mes, y de qué color va.</summary>
public sealed class CeldaDeCargaViewModel(CeldaDeCarga celda)
{
    public bool Vacia => celda.Vacia;

    public string Texto => celda.Vacia ? "—" : celda.Porcentaje.ToString("0", CultureInfo.CurrentCulture) + " %";

    /// <summary>Días comprometidos frente a los que caben, para el globo de ayuda.</summary>
    public string Detalle => celda.Vacia
        ? "Sin trabajo asignado"
        : $"{celda.Dias:0.#} de {celda.Capacidad} días de trabajo";

    /// <summary>
    /// Verde por debajo del 85 %, ámbar hasta el 100 %, rojo por encima: ahí el técnico
    /// está sobrevendido y el mes no da de sí.
    /// </summary>
    public string Fondo => celda switch
    {
        { Vacia: true } => "Transparent",
        { Porcentaje: > 100 } => "#FEE2E2",
        { Porcentaje: >= 85 } => "#FEF3C7",
        _ => "#DCFCE7"
    };

    public string Color => celda switch
    {
        { Vacia: true } => "#9CA3AF",
        { Porcentaje: > 100 } => "#B91C1C",
        { Porcentaje: >= 85 } => "#B45309",
        _ => "#15803D"
    };

    public string Peso => celda is { Vacia: false, Porcentaje: > 100 } ? "Bold" : "Normal";
}
