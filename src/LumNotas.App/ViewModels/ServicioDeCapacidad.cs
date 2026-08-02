using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// La tarifa y el calendario laboral del laboratorio. Viven junto a la lista de técnicos,
/// en la carpeta compartida, para que todos calculen la carga con los mismos números.
/// </summary>
public static class ServicioDeCapacidad
{
    private static CapacidadMensual? _capacidad;

    public static CapacidadMensual Capacidad => _capacidad ??= CapacidadMensual.Cargar(ServicioDeTecnicos.Carpeta());

    public static void Recargar() => _capacidad = CapacidadMensual.Cargar(ServicioDeTecnicos.Carpeta());

    public static void Guardar() => Capacidad.Guardar(ServicioDeTecnicos.Carpeta());
}
