namespace LumNotas.Core.Gestion;

/// <summary>
/// Si la toma de notas que se va a guardar ya existe en la carpeta del laboratorio.
/// <para>
/// Hace falta desde que el responsable da de alta los proyectos: un técnico puede
/// ponerse a tomar notas <b>sin saber que su proyecto ya estaba creado</b>, y acabar con
/// dos ficheros del mismo servicio —el suyo, con los datos, y el del responsable, con la
/// planificación y la tarjeta del calendario—. Ninguno de los dos estaría completo.
/// </para>
/// <para>
/// <b>Se compara el código de la toma de notas, no el de servicio.</b> Un trabajo puede
/// llevar cuatro familias de luminarias con cuatro tomas de notas, y las cuatro comparten
/// el de servicio: avisar por él convertiría en sospechoso lo que es el caso normal, y un
/// aviso que salta siempre deja de leerse. Lo que sí es un descuido es la misma familia
/// dos veces, y eso lo delata su código completo.
/// </para>
/// <para>
/// Es una red de seguridad, no una garantía: solo puede reconocer lo que se parece. Que
/// nadie llegue a esta situación es cosa de la pantalla de inicio, no de aquí.
/// </para>
/// </summary>
public static class ProyectosRepetidos
{
    /// <summary>
    /// Dos códigos son el mismo servicio si solo se diferencian en <b>mayúsculas,
    /// espacios o guiones</b>. Se compara así porque el código lo teclea una persona y
    /// «ANTAR2504», «antar 2504» y «ANTAR-2504» son el mismo servicio para el
    /// laboratorio; compararlos en crudo dejaría pasar justo el caso más probable, que
    /// es el técnico escribiéndolo a su manera.
    /// </summary>
    public static bool EsElMismoCodigo(string? uno, string? otro)
    {
        var a = Normalizar(uno);
        var b = Normalizar(otro);
        return a.Length > 0 && a == b;
    }

    /// <summary>
    /// Los proyectos de la carpeta que ya usan ese código de toma de notas, sin contar el
    /// que se está guardando. Se devuelven todos y no solo el primero: si ya hay dos
    /// repetidos, el aviso tiene que enseñarlos, no elegir uno.
    /// </summary>
    /// <param name="codigo">El código de la toma de notas, no el de servicio.</param>
    /// <param name="rutaPropia">
    /// El fichero que se está guardando, para no avisar de sí mismo al «Guardar como».
    /// </param>
    public static IReadOnlyList<ResumenDeProyecto> ConElMismoCodigo(
        IEnumerable<ResumenDeProyecto> proyectos, string? codigo, string? rutaPropia = null)
        => [.. proyectos.Where(p => p.Error is null
                                    && !EsElMismoFichero(p.Ruta, rutaPropia)
                                    && EsElMismoCodigo(CodigoDe(p), codigo))];

    /// <summary>
    /// Con qué se compara ese proyecto. Los guardados antes de existir el código de toma
    /// de notas no lo traen, y para ellos se usa el de servicio: es lo único que tienen, y
    /// dejarlos fuera del aviso sería perder justo la red de seguridad en los ficheros más
    /// antiguos.
    /// </summary>
    private static string CodigoDe(ResumenDeProyecto proyecto)
        => string.IsNullOrWhiteSpace(proyecto.CodigoTomaDeNotas)
            ? proyecto.CodigoServicio
            : proyecto.CodigoTomaDeNotas;

    private static bool EsElMismoFichero(string ruta, string? otra)
        => otra is not null && string.Equals(ruta, otra, StringComparison.OrdinalIgnoreCase);

    private static string Normalizar(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return "";

        return new string([.. codigo.Where(c => c is not (' ' or '-' or '_' or '.' or '/'))])
            .ToUpperInvariant();
    }
}
