using System.Windows;
using LumNotas.App.ViewModels;

namespace LumNotas.App;

/// <summary>
/// Ofrece cerrar el servicio cuando ya no queda nada por rellenar o se acaba de exportar.
/// <para>
/// Existe porque el estado lo pone una persona y se olvida: la toma de notas queda hecha,
/// el informe entregado, y el calendario sigue diciendo que ese trabajo está en curso
/// durante semanas. Preguntarlo en el momento en que de verdad se acabó es la única forma
/// de que el tablero se parezca a la realidad.
/// </para>
/// <para>
/// <b>Cerrar la ventana es cancelar</b>: no se toca nada. Y se pregunta una sola vez por
/// pestaña, que lo lleva <c>DocumentoViewModel</c>.
/// </para>
/// </summary>
public partial class DialogoCerrarProyecto : Window
{
    private RespuestaCierre _respuesta = RespuestaCierre.Cancelar;

    private DialogoCerrarProyecto(string motivo)
    {
        InitializeComponent();
        TextoMotivo.Text = motivo;
    }

    /// <summary>Pregunta y devuelve lo que se haya decidido.</summary>
    public static RespuestaCierre Preguntar(Window propietario, string motivo)
    {
        var dialogo = new DialogoCerrarProyecto(motivo) { Owner = propietario };

        // Se reclama el primer plano al aparecer. Esta ventana sale justo después de
        // exportar, y ahí alrededor hay otras aplicaciones abriéndose; una pregunta que
        // se queda detrás de otra ventana es una pregunta que nadie contesta.
        dialogo.Loaded += (_, _) => dialogo.Activate();

        dialogo.ShowDialog();
        return dialogo._respuesta;
    }

    private void AlTerminar(object remitente, RoutedEventArgs args) => Cerrar(RespuestaCierre.Terminado);

    private void AlArchivar(object remitente, RoutedEventArgs args) => Cerrar(RespuestaCierre.Archivado);

    private void AlCancelar(object remitente, RoutedEventArgs args) => Cerrar(RespuestaCierre.Cancelar);

    private void Cerrar(RespuestaCierre respuesta)
    {
        _respuesta = respuesta;
        Close();
    }
}
