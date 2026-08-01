using System.Collections.ObjectModel;
using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>Un elemento del grupo: «Tornillo 1» con sus cuatro campos.</summary>
public sealed class ElementoRepetidoViewModel(string titulo, IReadOnlyList<CampoViewModel> campos)
{
    public string Titulo { get; } = titulo;
    public IReadOnlyList<CampoViewModel> Campos { get; } = campos;
}

/// <summary>
/// Un grupo repetido de la plantilla —tornillos, uniones, prensaestopas, distancias—
/// presentado como bloques separados en lugar de una lista plana de filas.
/// <para>
/// El número de elementos es el mayor entre los que declara la plantilla, los que ya
/// tienen datos guardados y los que el técnico añada con el botón.
/// </para>
/// </summary>
public sealed class GrupoRepetidoViewModel : ObservableObject
{
    private readonly DatosProyecto _datos;
    private readonly string _ambito;
    private readonly Campo _campo;
    private readonly int _numeroMuestras;
    private readonly Action _alCambiar;
    private int _anadidos;

    public GrupoRepetidoViewModel(DatosProyecto datos, string ambito, Campo campo,
                                  int numeroMuestras, Action alCambiar)
    {
        _datos = datos;
        _ambito = ambito;
        _campo = campo;
        _numeroMuestras = numeroMuestras;
        _alCambiar = alCambiar;

        Etiqueta = campo.Etiqueta;
        Ampliable = campo.Ampliable;
        Anadir = new Comando(() => { _anadidos++; Reconstruir(); });

        Reconstruir();
    }

    public string Etiqueta { get; }
    public bool Ampliable { get; }
    public Comando Anadir { get; }
    public ObservableCollection<ElementoRepetidoViewModel> Elementos { get; } = [];

    public string TextoAnadir => $"+ Añadir {Etiqueta.ToLowerInvariant()}";

    /// <summary>Cabecera M1…Mn, una sola vez para todo el grupo.</summary>
    public IReadOnlyList<string> Muestras { get; private set; } = [];
    public bool TieneMuestras => Muestras.Count > 0;

    private void Reconstruir()
    {
        var minimo = Math.Max(_campo.Elementos ?? 1, 1);
        var guardados = _datos.MaximoIndiceDe(_ambito, _campo.Id) + 1;
        var total = Math.Max(minimo, guardados) + _anadidos;

        Elementos.Clear();
        for (var i = 0; i < total; i++)
        {
            // Con un solo elemento no tiene sentido numerarlo.
            var titulo = total == 1 ? Etiqueta : $"{Etiqueta} {i + 1}";
            var indice = i;

            Elementos.Add(new ElementoRepetidoViewModel(titulo,
            [
                .. _campo.Campos.Select(hijo => new CampoViewModel(
                    _datos, _ambito, hijo, $"{_campo.Id}[{indice}].{hijo.Id}",
                    _numeroMuestras, _alCambiar, porMuestra: _campo.PorMuestra))
            ]));
        }

        Muestras = Elementos.FirstOrDefault()?.Campos.FirstOrDefault(c => c.PorMuestra) is { } conMuestras
            ? [.. conMuestras.Celdas.Select(c => c.EtiquetaMuestra)]
            : [];

        Notificar(nameof(Muestras));
        Notificar(nameof(TieneMuestras));
    }
}
