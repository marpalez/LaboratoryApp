using LumNotas.Core.Datos;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Gestion;

/// <summary>
/// Dar de alta un proyecto. Es lo que hace el responsable para tener una tarjeta que
/// planificar, <b>antes de que exista un solo dato de ensayo</b>.
/// <para>
/// <b>Solo hacen falta el nombre, el técnico 1 y la norma.</b> Lo demás —número de
/// muestras, clase, grado IP, partes ‑2— lo pide la plantilla de la norma y lo rellenará
/// el técnico cuando empiece; exigirlo aquí convertiría el alta en la toma de notas
/// entera, que es justo lo que se está sustituyendo. Un proyecto a medias es el estado
/// normal durante semanas, no un error.
/// </para>
/// <para>
/// Vive en el núcleo y no en la ventana porque la regla de qué se exige es del negocio,
/// no del formulario: si algún día se le añade un campo, el sitio donde se decide si
/// bloquea o no es este, y tiene tests.
/// </para>
/// </summary>
public static class AltaDeProyecto
{
    public const string CampoNombre = "Código de la toma de notas";
    public const string CampoTecnico = "Técnico 1";
    public const string CampoNorma = "Norma";

    /// <summary>
    /// Lo que ocupa un código completo: <c>TECNO260201-00</c>. Cinco del cliente, cuatro
    /// de año y mes, dos de familia, el guion y dos de edición.
    /// <para>
    /// <b>Se exige exacto al dar de alta y también en la cabecera</b> (2026‑08‑06). La regla
    /// vive en <see cref="CodigoDeServicio"/>, junto a las otras dos longitudes que se
    /// recortan de este mismo código, para que los dos caminos no puedan discrepar.
    /// </para>
    /// </summary>
    public const int LongitudDelCodigo = CodigoDeServicio.LongitudCompleta;

    /// <summary>Si el código está completo. Vacío no lo está.</summary>
    public static bool CodigoCompleto(string? codigo) => CodigoDeServicio.EstaCompleto(codigo);

    /// <summary>
    /// Qué falta para poder crear. <b>Esta lista es corta y tiene que seguir siéndolo</b>:
    /// no es la lista de lo que hay que rellenar para ensayar —de eso ya se ocupa
    /// <see cref="RequisitosDelProyecto"/>— sino de lo mínimo para que la toma de notas
    /// exista y se pueda planificar.
    /// </summary>
    /// <param name="norma">
    /// La norma con la que se va a ensayar. Empezó siendo opcional y el laboratorio la
    /// hizo obligatoria: una toma de notas sin norma no se puede rellenar —no tiene
    /// apartados— y además el nombre del fichero la lleva dentro, así que dejarla para
    /// después obligaba a renombrarlo.
    /// </param>
    public static IReadOnlyList<string> Faltan(string? nombre, string? tecnico1, string? norma)
    {
        var faltan = new List<string>();
        if (!CodigoCompleto(nombre)) faltan.Add(CampoNombre);
        if (string.IsNullOrWhiteSpace(tecnico1)) faltan.Add(CampoTecnico);
        if (string.IsNullOrWhiteSpace(norma)) faltan.Add(CampoNorma);
        return faltan;
    }

    public static bool SePuedeCrear(string? nombre, string? tecnico1, string? norma)
        => Faltan(nombre, tecnico1, norma).Count == 0;

    /// <summary>
    /// El proyecto recién nacido: código, responsable y, si se ha elegido, la norma con
    /// la que va a trabajar. Nada más — ni una muestra, ni una casilla.
    /// <para>
    /// Lo que se teclea es el <b>código de la toma de notas</b>, que es lo que da nombre
    /// al fichero; el de servicio sale de sus nueve primeras
    /// (<see cref="CodigoDeServicio"/>) y el técnico lo corrige después si hiciera falta.
    /// </para>
    /// </summary>
    /// <param name="principal">
    /// La norma con la que se va a ensayar, que queda apuntada como principal. El tipo
    /// admite <c>null</c> para no obligar a cargar una plantilla en los tests que solo
    /// miran la identidad; quien decide si se puede crear sin ella es
    /// <see cref="Faltan"/>, y dice que no.
    /// </param>
    public static DatosProyecto Crear(
        string nombre, string tecnico1, string? tecnico2 = null, PlantillaEnsayos? principal = null)
    {
        var datos = new DatosProyecto
        {
            CodigoTomaDeNotas = nombre.Trim(),
            CodigoServicio = CodigoDeServicio.Derivar(nombre),
            // Una muestra, que es lo que tiene un servicio hasta que se diga otra cosa.
            NumeroMuestras = 1,
            Tecnico1 = tecnico1.Trim()
        };

        if (!string.IsNullOrWhiteSpace(tecnico2)) datos.Tecnico2 = tecnico2.Trim();

        principal?.AplicarA(datos);

        return datos;
    }

    /// <summary>
    /// Sanea un texto para que valga como nombre de fichero. Lo teclea una persona y
    /// puede llevar barras o dos puntos —«ANTAR2504/01» es una forma natural de nombrar
    /// un servicio—, así que se limpia en vez de reventar al guardar.
    /// </summary>
    public static string NombreDeFichero(string nombre)
    {
        var limpio = new string([.. nombre.Trim()
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '-' : c)]);

        return string.IsNullOrWhiteSpace(limpio) ? "proyecto" : limpio;
    }
}
