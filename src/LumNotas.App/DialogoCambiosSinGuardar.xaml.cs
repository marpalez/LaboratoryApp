using System.Windows;
using LumNotas.App.ViewModels;

namespace LumNotas.App;

/// <summary>
/// Aviso de cambios sin guardar. Sustituye al <c>MessageBox</c> de Windows, que solo
/// admite «Sí / No» y obligaba al técnico a traducir mentalmente qué significaba cada uno.
/// <para>Cerrar la ventana equivale a cancelar: no se pierde nada.</para>
/// </summary>
public partial class DialogoCambiosSinGuardar : Window
{
    private RespuestaCambios _respuesta = RespuestaCambios.Cancelar;

    public DialogoCambiosSinGuardar() => InitializeComponent();

    /// <summary>Muestra el aviso y devuelve lo que ha decidido el técnico.</summary>
    public static RespuestaCambios Preguntar(Window propietario)
    {
        var dialogo = new DialogoCambiosSinGuardar { Owner = propietario };
        dialogo.ShowDialog();
        return dialogo._respuesta;
    }

    private void AlGuardar(object sender, RoutedEventArgs e) => Cerrar(RespuestaCambios.Guardar);

    private void AlDescartar(object sender, RoutedEventArgs e) => Cerrar(RespuestaCambios.Descartar);

    private void Cerrar(RespuestaCambios respuesta)
    {
        _respuesta = respuesta;
        Close();
    }
}
