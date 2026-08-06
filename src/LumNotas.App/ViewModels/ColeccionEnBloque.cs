using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Una colección observable que se sustituye <b>de una vez</b> en lugar de vaciarse y
/// rellenarse elemento a elemento.
/// <para>
/// <b>Por qué existe</b> (2026‑08‑06, medido con 250 proyectos): el patrón de siempre
/// —<c>Clear()</c> y un <c>Add()</c> por elemento— levanta <b>un aviso por elemento</b>, y
/// WPF reacciona a cada uno: invalida la medida, genera o tira contenedores y vuelve a
/// maquetar. Con 250 proyectos eso salían 251 avisos por colección y <b>1,6 s de ventana
/// congelada en cada cambio de filtro</b>, repartidos por igual entre las cuatro vistas
/// —lo que delataba que el coste no era pintar una, sino avisar cuatro veces.
/// </para>
/// <para>
/// Con un solo aviso de reinicio, WPF hace el mismo trabajo <b>una vez</b>. Lo que se ve en
/// pantalla es idéntico: cambia cuántas veces se dice, no lo que se dice.
/// </para>
/// <para>
/// <b>Un reinicio no dice qué elementos entraron o salieron.</b> Da igual aquí —estas
/// listas se rehacen enteras— pero quien se enganche a <c>CollectionChanged</c> esperando
/// altas y bajas de una en una no las va a recibir.
/// </para>
/// </summary>
public sealed class ColeccionEnBloque<T> : ObservableCollection<T>
{
    public ColeccionEnBloque() { }

    public ColeccionEnBloque(IEnumerable<T> inicial) : base(inicial) { }

    /// <summary>
    /// Deja dentro exactamente eso, con un solo aviso. Sustituye al
    /// <c>Clear()</c> + <c>foreach Add()</c>.
    /// </summary>
    public void Reemplazar(IEnumerable<T> nuevos)
    {
        // Se toca la lista de dentro para que no salga un aviso por elemento; el de
        // reinicio va después, cuando ya está todo puesto.
        Items.Clear();
        foreach (var elemento in nuevos) Items.Add(elemento);

        Avisar();
    }

    /// <summary>Vacía con un solo aviso.</summary>
    public void Vaciar()
    {
        if (Items.Count == 0) return;

        Items.Clear();
        Avisar();
    }

    private void Avisar()
    {
        OnPropertyChanged(new(nameof(Count)));
        OnPropertyChanged(new("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
