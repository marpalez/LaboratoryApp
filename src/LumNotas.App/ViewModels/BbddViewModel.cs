using System.Collections.ObjectModel;
using LumNotas.Core.Gestion;

namespace LumNotas.App.ViewModels;

/// <summary>
/// El listado de todas las tomas de notas del laboratorio, para encontrar una de hace
/// meses sin ir preguntando a los compañeros.
/// <para>
/// <b>Solo lee.</b> No es una base de datos aparte: es una lente sobre los mismos
/// <c>.lmnlab</c> que ya escanea el tablero. Un fichero índice que hubiera que mantener
/// sería una segunda verdad que se desincroniza, y eso ya se descartó dos veces (DD‑27,
/// DD‑89).
/// </para>
/// <para>
/// <b>Ya no filtra por su cuenta.</b> Tuvo su propia caja de buscar y sus desplegables de
/// IP, IK y acreditación; ahora los filtros son un solo juego para las cuatro vistas y
/// viven en el botón «Filtros». Elegir dos veces lo mismo en dos sitios distintos solo
/// servía para que discreparan.
/// </para>
/// <para>
/// A cambio, aquí manda también el estado: para encontrar algo archivado hay que poner
/// «Estado» en «(todos)». El botón lo dice sin abrirlo.
/// </para>
/// </summary>
public sealed class BbddViewModel : ObservableObject
{
    /// <summary>Las filas que se ven ahora mismo. Se sustituyen en bloque (ver
    /// <see cref="ColeccionEnBloque{T}"/>): son cientos y se rehacen enteras.</summary>
    public ColeccionEnBloque<FilaDeBbdd> Filas { get; } = [];

    // El listado tuvo su propio «47 tomas de notas» encima de la tabla. Se quitó: la línea
    // de estado de la barra ya dice «47 proyectos | 186 fuera del filtro» dos renglones más
    // arriba, y la misma cuenta escrita dos veces con dos palabras distintas hacía dudar de
    // si hablaban de lo mismo. La cuenta sigue estando en el HTML exportado, que se lee solo.

    public bool NoHayNada => Filas.Count == 0;

    /// <summary>Lo contrario, porque el convertidor de WPF a visibilidad no sabe invertir.</summary>
    public bool HayAlgo => Filas.Count > 0;

    /// <summary>Abrir la toma de notas de una fila; lo resuelve la ventana.</summary>
    public Action<string>? Abrir { get; set; }

    // ---- exportar ----------------------------------------------------------

    /// <summary>
    /// Sacar en papel lo que se está viendo.
    /// <para>
    /// <b>Salen todas las filas que hay tras el filtro, no las que caben en la pantalla.</b>
    /// Si el filtro deja cuatro, se exportan cuatro; si no hay filtro y hay cien, cien. Es
    /// un cuidado real y no una obviedad: la tabla está <b>virtualizada</b> (DD‑131), así
    /// que en el árbol visual **solo existen las quince filas que se ven**. Exportar
    /// recorriendo la pantalla daría quince y parecería correcto. Por eso se exporta
    /// <see cref="Filas"/>, que es el modelo, y nunca los elementos dibujados.
    /// </para>
    /// <para>
    /// <b>Solo aquí, y no en el tablero ni en el calendario</b> (DD‑140). Esta es la única
    /// de las cuatro vistas que ya es una tabla: exportarla es escribir en HTML las mismas
    /// filas y columnas. Las otras dos son dibujos —columnas con tarjetas, barras sobre un
    /// eje de semanas— y llevarlas a un papel no sería exportar sino inventarse otro
    /// documento, con otras decisiones y otro mantenimiento.
    /// </para>
    /// </summary>
    public Comando Exportar { get; }

    public BbddViewModel() => Exportar = new Comando(() => AlExportar?.Invoke(), () => HayAlgo);

    /// <summary>Lo resuelve la ventana: pedir el fichero, escribirlo y abrirlo en el visor.</summary>
    public Action? AlExportar { get; set; }

    /// <summary>
    /// Recibe lo que ya ha pasado el filtro. Las ilegibles se quedan fuera: una fila sin
    /// datos que leer no es un resultado de búsqueda, y la portada ya avisa de cuántas hay.
    /// </summary>
    public void Cargar(IReadOnlyList<ResumenDeProyecto> proyectos)
    {
        Filas.Reemplazar(proyectos.Where(p => p.Error is null)
                                  .OrderByDescending(p => p.Modificado)
                                  .Select(p => new FilaDeBbdd(p)));

        Notificar(nameof(NoHayNada));
        Notificar(nameof(HayAlgo));
        Exportar.Revisar();
    }
}

// FilaDeBbdd se mudó al núcleo (LumNotas.Core.Gestion) al poder exportarse el listado:
// la tabla del papel tiene que decir lo mismo que la de la pantalla, y para eso las dos
// tienen que salir de la misma definición.
