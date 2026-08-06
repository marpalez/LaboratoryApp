using System.Windows;
using LumNotas.App.ViewModels;

namespace LumNotas.App;

/// <summary>
/// Los filtros de gestión fuera de la barra: estado, técnico, norma, grado IP, grado IK y
/// acreditación.
/// <para>
/// Ocupaban media fila y en una ventana estrecha empujaban al resto; aquí caben con su
/// explicación al lado, que en la barra no cabía. <b>Se aplican al elegirlos</b>, no al
/// cerrar: son los mismos de siempre, atados al mismo modelo, y meter un «Aceptar» sería
/// cambiar cómo se comportan por haberlos movido de sitio.
/// </para>
/// <para>
/// Los tres últimos vivían en la barra de la BBDD, cada uno por su cuenta. Ahora son un
/// solo juego para las cuatro vistas: dos sitios donde elegir lo mismo solo servían para
/// que discreparan.
/// </para>
/// </summary>
public partial class DialogoFiltros : Window
{
    private readonly GestionViewModel _gestion;

    private DialogoFiltros(GestionViewModel gestion)
    {
        _gestion = gestion;
        InitializeComponent();
        DataContext = gestion;
    }

    public static void Mostrar(Window propietario, GestionViewModel gestion)
        => new DialogoFiltros(gestion) { Owner = propietario }.ShowDialog();

    private void AlQuitar(object remitente, RoutedEventArgs args) => _gestion.QuitarFiltros();

    private void AlCerrar(object remitente, RoutedEventArgs args) => Close();
}
