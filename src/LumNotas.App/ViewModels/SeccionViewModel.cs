using System.Collections.ObjectModel;
using LumNotas.Core.Motor;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Una sección de la norma en el árbol del índice. Agrupa sus apartados y resume su
/// estado, para que el técnico vea de un vistazo dónde falta algo sin desplegarla.
/// </summary>
public sealed class SeccionViewModel : ObservableObject
{
    private readonly IReadOnlyList<BloqueViewModel> _todos;

    public SeccionViewModel(string titulo, IReadOnlyList<BloqueViewModel> apartados)
    {
        Titulo = titulo;
        _todos = apartados;
        Refrescar();
    }

    public string Titulo { get; }

    /// <summary>
    /// Solo los apartados que aplican. Los ensayos de las partes -2 desaparecen del árbol
    /// cuando su parte no está marcada en el proyecto, en lugar de quedarse en gris.
    /// </summary>
    public ObservableCollection<BloqueViewModel> Apartados { get; } = [];

    /// <summary>
    /// Mientras la cabecera del proyecto esté incompleta, las secciones de ensayo no se
    /// muestran: primero hay que saber muestras, clase y grado IP.
    /// </summary>
    public bool Bloqueada { get; set; }

    /// <summary>Una sección bloqueada o sin apartados aplicables no se muestra.</summary>
    public bool Visible => !Bloqueada && Apartados.Count > 0;

    private bool _expandida;

    /// <summary>
    /// Si la rama está desplegada. Lo pone el documento al recuperar el apartado que
    /// estaba abierto: con el árbol dentro de una pestaña ya no se puede alcanzar por
    /// nombre desde la ventana.
    /// </summary>
    public bool Expandida
    {
        get => _expandida;
        set => Establecer(ref _expandida, value);
    }

    /// <summary>
    /// Semáforo de la sección: rojo si a cualquier apartado le faltan datos, gris si
    /// todos son N/A, verde si están todos resueltos.
    /// </summary>
    public EstadoApartado Estado
    {
        get
        {
            if (Apartados.Count == 0) return EstadoApartado.SinReglas;
            if (Apartados.All(a => a.Estado == EstadoApartado.NoAplica)) return EstadoApartado.NoAplica;
            if (Apartados.Any(a => a.Estado == EstadoApartado.FaltanDatos)) return EstadoApartado.FaltanDatos;
            return EstadoApartado.Completo;
        }
    }

    public string Resumen
    {
        get
        {
            var aplicables = Apartados.Count(a => a.Estado != EstadoApartado.NoAplica);
            if (aplicables == 0) return $"{Apartados.Count} apartados · no aplica";

            var completos = Apartados.Count(a => a.Estado == EstadoApartado.Completo);
            return $"{completos} de {aplicables} completos";
        }
    }

    public void Refrescar()
    {
        var visibles = _todos.Where(a => a.Visible).ToList();

        // Se reconstruye solo si ha cambiado, para no perder la selección del árbol.
        if (!visibles.SequenceEqual(Apartados))
        {
            Apartados.Clear();
            foreach (var apartado in visibles) Apartados.Add(apartado);
        }

        Notificar(nameof(Visible));
        Notificar(nameof(Estado));
        Notificar(nameof(Resumen));
    }
}
