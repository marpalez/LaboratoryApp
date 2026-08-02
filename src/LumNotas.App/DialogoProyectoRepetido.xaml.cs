using System.Windows;
using LumNotas.App.ViewModels;
using LumNotas.Core.Gestion;

namespace LumNotas.App;

/// <summary>
/// Aviso de que el servicio que se va a guardar ya existe en la carpeta del laboratorio.
/// <para>
/// Los tres botones dicen lo que hacen, como en el aviso de cambios sin guardar: aquí un
/// «¿Continuar? Sí / No» sería especialmente malo, porque las dos salidas razonables
/// —abrir el que hay o crear otro a sabiendas— no son sí ni no.
/// </para>
/// <para>
/// <b>«Crear otro igualmente» existe y no es un descuido.</b> Hay motivos legítimos para
/// repetir código: un reensayo, un servicio partido en dos. El programa avisa; quien
/// decide es la persona.
/// </para>
/// </summary>
public partial class DialogoProyectoRepetido : Window
{
    private RespuestaRepetido _respuesta = RespuestaRepetido.Cancelar;

    private DialogoProyectoRepetido() => InitializeComponent();

    public static RespuestaRepetido Preguntar(
        Window propietario, string codigo, IReadOnlyList<ResumenDeProyecto> existentes)
    {
        var dialogo = new DialogoProyectoRepetido { Owner = propietario };

        dialogo.Titulo.Text = existentes.Count == 1
            ? $"Ya hay un proyecto «{codigo}»"
            : $"Ya hay {existentes.Count} proyectos «{codigo}»";

        dialogo.Existentes.ItemsSource = existentes;

        // Abrir solo tiene sentido cuando no hay duda de cuál.
        dialogo.BotonAbrir.IsEnabled = existentes.Count == 1;

        dialogo.ShowDialog();
        return dialogo._respuesta;
    }

    private void AlAbrir(object remitente, RoutedEventArgs args) => Cerrar(RespuestaRepetido.Abrir);

    private void AlCrearIgual(object remitente, RoutedEventArgs args) => Cerrar(RespuestaRepetido.CrearIgualmente);

    private void AlCancelar(object remitente, RoutedEventArgs args) => Cerrar(RespuestaRepetido.Cancelar);

    private void Cerrar(RespuestaRepetido respuesta)
    {
        _respuesta = respuesta;
        Close();
    }
}
