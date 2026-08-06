namespace LumNotas.Core.Gestion;

/// <summary>Qué de grave es un aviso: si algo no funciona o si algo está descuadrado.</summary>
public enum NivelDeAviso
{
    /// <summary>Algo no funciona y hay trabajo que no se puede hacer.</summary>
    Problema,

    /// <summary>Funciona, pero no como debería.</summary>
    Atencion
}

/// <summary>Qué se puede hacer con un aviso. Lo resuelve la ventana, que tiene los diálogos.</summary>
public enum AccionDeAviso
{
    ElegirCarpetas,
    VerNormas,
    IrAlTablero
}

/// <param name="Detalle">Segunda línea, para la ruta o los nombres. Puede faltar.</param>
public sealed record AvisoDeInicio(
    NivelDeAviso Nivel, string Texto, string? Detalle, AccionDeAviso Accion, string Boton);

/// <summary>
/// Lo que el equipo tiene mal configurado, dicho en la portada.
/// <para>
/// Casi todo esto <b>fallaba en silencio</b>: sin carpeta de proyectos las tres vistas de
/// gestión salen vacías, y eso es indistinguible de no tener trabajo; sin carpeta
/// compartida cada equipo usa sus propias normas y sus propios técnicos sin que nadie lo
/// diga. El programa lo sabía y no lo contaba.
/// </para>
/// <para>
/// <b>Un aviso solo existe si hay algo que hacer.</b> Nada en verde, nada informativo: un
/// recuadro que casi siempre está deja de leerse, y entonces no sirve el día que importa.
/// </para>
/// </summary>
public static class AvisosDeInicio
{
    /// <summary>
    /// Lo que hace falta saber del equipo para decidir qué avisar. Se le pasa ya resuelto
    /// —no mira el disco— para poder probarlo sin montar carpetas.
    /// </summary>
    /// <param name="HayNormasPublicadas">Si la carpeta compartida ya tiene su <c>plantilla/</c>.</param>
    public sealed record Estado(
        string? CarpetaDeProyectos = null,
        bool ProyectosAccesible = false,
        string? CarpetaCompartida = null,
        bool CompartidaAccesible = false,
        bool HayNormasPublicadas = false,
        IReadOnlyList<string>? NormasSinPublicar = null,
        IReadOnlyList<string>? NormasMasNuevas = null,
        int ProyectosIlegibles = 0);

    public static IReadOnlyList<AvisoDeInicio> Revisar(Estado estado)
    {
        var avisos = new List<AvisoDeInicio>();

        CarpetaDeProyectos(estado, avisos);
        CarpetaCompartida(estado, avisos);
        Normas(estado, avisos);
        Proyectos(estado, avisos);

        // Lo que no funciona, antes de lo que está descuadrado.
        return [.. avisos.OrderBy(a => a.Nivel)];
    }

    /// <summary>Sin ella no hay tablero, ni calendario, ni carga. Es lo primero.</summary>
    private static void CarpetaDeProyectos(Estado estado, List<AvisoDeInicio> avisos)
    {
        if (string.IsNullOrWhiteSpace(estado.CarpetaDeProyectos))
        {
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Problema,
                "No hay carpeta de proyectos elegida.",
                "El tablero, el calendario y la carga están vacíos hasta que se elija.",
                AccionDeAviso.ElegirCarpetas, "Elegir carpetas"));
        }
        else if (!estado.ProyectosAccesible)
        {
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Problema,
                "No se puede llegar a la carpeta de proyectos. Comprueba que OneDrive esté sincronizado.",
                estado.CarpetaDeProyectos,
                AccionDeAviso.ElegirCarpetas, "Elegir carpetas"));
        }
    }

    /// <summary>
    /// Los tres estados en los que la carpeta compartida no está cumpliendo su función.
    /// Son excluyentes: como mucho sale uno.
    /// </summary>
    private static void CarpetaCompartida(Estado estado, List<AvisoDeInicio> avisos)
    {
        if (string.IsNullOrWhiteSpace(estado.CarpetaCompartida))
        {
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Atencion,
                "No hay carpeta compartida.",
                "Las normas, los técnicos y la tarifa salen de este equipo y no los ve nadie más.",
                AccionDeAviso.ElegirCarpetas, "Elegir carpetas"));
            return;
        }

        if (!estado.CompartidaAccesible)
        {
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Problema,
                "No se puede llegar a la carpeta compartida. Se están usando las normas, "
                + "los técnicos y la tarifa de este equipo.",
                estado.CarpetaCompartida,
                AccionDeAviso.ElegirCarpetas, "Elegir carpetas"));
            return;
        }

        // Elegida, accesible y vacía: es el caso de una carpeta recién creada, donde
        // ninguna de las otras dos condiciones salta y nada estaría compartido.
        if (!estado.HayNormasPublicadas)
        {
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Atencion,
                "Las normas todavía no están publicadas en la carpeta compartida.",
                "Cada equipo está usando su copia.",
                AccionDeAviso.VerNormas, "Normas instaladas"));
        }
    }

    private static void Normas(Estado estado, List<AvisoDeInicio> avisos)
    {
        var sinPublicar = estado.NormasSinPublicar ?? [];
        var masNuevas = estado.NormasMasNuevas ?? [];

        if (sinPublicar.Count > 0)
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Atencion,
                sinPublicar.Count == 1
                    ? "Hay una norma en este equipo que el laboratorio no tiene."
                    : $"Hay {sinPublicar.Count} normas en este equipo que el laboratorio no tiene.",
                string.Join(" | ", sinPublicar),
                AccionDeAviso.VerNormas, "Normas instaladas"));

        // Aparte del anterior a propósito: no es «falta algo», es que este equipo está
        // trabajando con una plantilla corregida que los demás no tienen — que es
        // justamente lo que la carpeta compartida existe para evitar.
        if (masNuevas.Count > 0)
            avisos.Add(new AvisoDeInicio(
                NivelDeAviso.Atencion,
                masNuevas.Count == 1
                    ? "Una norma de este equipo es más nueva que la publicada. Los demás siguen con la anterior."
                    : $"{masNuevas.Count} normas de este equipo son más nuevas que las publicadas. Los demás siguen con las anteriores.",
                string.Join(" | ", masNuevas),
                AccionDeAviso.VerNormas, "Normas instaladas"));
    }

    /// <summary>
    /// Un <c>.lmnlab</c> corrupto sale marcado en el tablero, pero quien no lo abra no se
    /// entera — y es lo que nadie descubre hasta que necesita el fichero.
    /// </summary>
    private static void Proyectos(Estado estado, List<AvisoDeInicio> avisos)
    {
        if (estado.ProyectosIlegibles <= 0) return;

        avisos.Add(new AvisoDeInicio(
            NivelDeAviso.Atencion,
            estado.ProyectosIlegibles == 1
                ? "Una toma de notas no se pudo leer."
                : $"{estado.ProyectosIlegibles} tomas de notas no se pudieron leer.",
            "Salen marcadas en el tablero.",
            AccionDeAviso.IrAlTablero, "Ir al tablero"));
    }
}
