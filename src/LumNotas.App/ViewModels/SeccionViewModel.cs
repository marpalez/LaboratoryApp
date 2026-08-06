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
    /// Semáforo de la sección: apagado si todos sus apartados son N/A, verde si están
    /// todos resueltos, ámbar si hay trabajo hecho pero queda algo, y sin empezar si no
    /// se ha tocado nada.
    /// <para>
    /// Un apartado completo cuenta como empezar: una sección con seis apartados y uno
    /// terminado está en marcha, aunque en los otros cinco no haya nada escrito.
    /// </para>
    /// </summary>
    public EstadoApartado Estado
    {
        get
        {
            if (Apartados.Count == 0) return EstadoApartado.SinReglas;
            if (Apartados.All(a => a.Estado == EstadoApartado.NoAplica)) return EstadoApartado.NoAplica;
            if (!Apartados.Any(a => EstadoDeApartado.EstaPendiente(a.Estado))) return EstadoApartado.Completo;

            return Apartados.Any(a => a.Estado is EstadoApartado.Empezado or EstadoApartado.Completo)
                ? EstadoApartado.Empezado
                : EstadoApartado.FaltanDatos;
        }
    }

    /// <summary>Frase entera de la sección. Va en la ayuda emergente, donde hay sitio.</summary>
    public string Resumen
    {
        get
        {
            var (completos, aplicables) = Cuenta();
            return aplicables == 0
                ? $"{Apartados.Count} apartados | no aplica"
                : $"{completos} de {aplicables} completos";
        }
    }

    /// <summary>
    /// La misma cuenta encogida —«3/12»— para la píldora del árbol. Ahí sustituye al
    /// punto de color: dice lo mismo que él y además cuánto queda, en el mismo sitio y
    /// sin gastar un segundo renglón.
    /// </summary>
    public string Progreso
    {
        get
        {
            var (completos, aplicables) = Cuenta();
            return aplicables == 0 ? "N/A" : $"{completos}/{aplicables}";
        }
    }

    private (int Completos, int Aplicables) Cuenta()
        => (Apartados.Count(a => a.Estado == EstadoApartado.Completo),
            Apartados.Count(a => a.Estado != EstadoApartado.NoAplica));

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
        Notificar(nameof(Progreso));
    }
}
