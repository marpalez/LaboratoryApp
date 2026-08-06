using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Que no acaben dos ficheros del mismo servicio.
/// <para>
/// El caso real: desde que el responsable da de alta los proyectos, un técnico puede
/// ponerse a tomar notas <b>sin saber que el suyo ya estaba creado</b>. Acabarían con el
/// mismo servicio partido en dos ficheros —uno con los datos y otro con las fechas— y
/// ninguno de los dos completo.
/// </para>
/// </summary>
public class ProyectosRepetidosTests
{
    private static ResumenDeProyecto Existente(string codigo, string ruta, string tecnico = "Javier Ibor")
        => new() { Ruta = ruta, Nombre = Path.GetFileNameWithoutExtension(ruta), CodigoServicio = codigo, Tecnico = tecnico };

    [Fact]
    public void ElMismoCodigoSeReconoce()
    {
        Assert.True(ProyectosRepetidos.EsElMismoCodigo("ANTAR2504", "ANTAR2504"));
        Assert.False(ProyectosRepetidos.EsElMismoCodigo("ANTAR2504", "ANTAR2505"));
    }

    /// <summary>
    /// <b>El código lo teclea una persona.</b> «ANTAR2504», «antar 2504» y «ANTAR-2504»
    /// son el mismo servicio para el laboratorio, y compararlos en crudo dejaría pasar
    /// justo el caso más probable: el técnico escribiéndolo a su manera.
    /// </summary>
    [Fact]
    public void MayusculasEspaciosYGuionesNoHacenUnServicioDistinto()
    {
        foreach (var variante in new[] { "antar2504", "ANTAR 2504", "ANTAR-2504", "  antar 2504  ", "ANTAR.2504" })
            Assert.True(ProyectosRepetidos.EsElMismoCodigo("ANTAR2504", variante), variante);
    }

    /// <summary>Un código en blanco no choca con nada: si no, todos los vacíos chocarían entre sí.</summary>
    [Fact]
    public void UnCodigoVacioNoChocaConNadie()
    {
        Assert.False(ProyectosRepetidos.EsElMismoCodigo("", ""));
        Assert.False(ProyectosRepetidos.EsElMismoCodigo(null, "ANTAR2504"));
        Assert.False(ProyectosRepetidos.EsElMismoCodigo("   ", "   "));
    }

    [Fact]
    public void EncuentraLosQueYaUsanEseCodigo()
    {
        var carpeta = new[]
        {
            Existente("ANTAR2504", @"C:\clientes\antares\a.lmnlab"),
            Existente("ANTAR2505", @"C:\clientes\antares\b.lmnlab"),
            Existente("antar 2504", @"C:\clientes\otro\c.lmnlab", "Mario Madrigal")
        };

        var repetidos = ProyectosRepetidos.ConElMismoCodigo(carpeta, "ANTAR2504");

        Assert.Equal(2, repetidos.Count);
        Assert.Contains(repetidos, p => p.Tecnico == "Mario Madrigal");
    }

    /// <summary>Al «Guardar como» de un proyecto ya guardado, no puede avisar de sí mismo.</summary>
    [Fact]
    public void UnProyectoNoChocaConsigoMismo()
    {
        var propio = @"C:\clientes\antares\a.lmnlab";
        var carpeta = new[] { Existente("ANTAR2504", propio) };

        Assert.Empty(ProyectosRepetidos.ConElMismoCodigo(carpeta, "ANTAR2504", propio));

        // Y la ruta se compara sin distinguir mayúsculas, que es como funciona Windows.
        Assert.Empty(ProyectosRepetidos.ConElMismoCodigo(carpeta, "ANTAR2504", propio.ToUpperInvariant()));
    }

    /// <summary>
    /// Un fichero corrupto no puede provocar un aviso: su código no es fiable y el
    /// técnico se encontraría un choque contra algo que no se puede ni abrir.
    /// </summary>
    [Fact]
    public void UnProyectoIlegibleNoCuentaComoRepetido()
    {
        var roto = AnalizadorDeProyectos.NoLegible(@"C:\clientes\roto.lmnlab", DateTime.Now, "json mal formado");

        Assert.Empty(ProyectosRepetidos.ConElMismoCodigo([roto], "ANTAR2504"));
        Assert.Empty(ProyectosRepetidos.ConElMismoCodigo([roto], ""));
    }
}
