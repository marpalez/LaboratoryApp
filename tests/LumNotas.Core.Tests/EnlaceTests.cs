using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// Enlazar varias tomas de notas como un solo trabajo.
/// <para>
/// Un servicio del laboratorio puede llevar <b>cuatro familias de luminarias</b>, cada
/// una con su toma de notas. El jefe quiere planificar una cosa y el técnico seguir
/// viendo las cuatro; se resuelve enlazándolas, <b>sin</b> crear un fichero de proyecto
/// por encima que rompería la trazabilidad (DD‑89).
/// </para>
/// </summary>
public class EnlaceTests
{
    private static ResumenDeProyecto Toma(
        string codigo, string? grupo = null, DateTime? inicio = null, DateTime? fin = null,
        double? importe = null, int completadas = 0, int aplicables = 10)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lumproj",
            Nombre = codigo,
            CodigoServicio = codigo,
            SeccionesCompletadas = completadas,
            SeccionesAplicables = aplicables,
            Planificacion = new Planificacion
            {
                Grupo = grupo, Inicio = inicio, Fin = fin, Importe = importe
            }
        };

    [Fact]
    public void UnaTomaDeNotasSinGrupoVaSola()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([Toma("A"), Toma("B")]);

        Assert.Equal(2, entradas.Count);
        Assert.All(entradas, e => Assert.False(e.EsGrupo));
    }

    [Fact]
    public void LasEnlazadasSalenEnUnaSolaEntrada()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504"),
            Toma("FAM-B", "ANTAR2504"),
            Toma("FAM-C", "ANTAR2504"),
            Toma("OTRO")
        ]);

        Assert.Equal(2, entradas.Count);

        var grupo = entradas.Single(e => e.EsGrupo);
        Assert.Equal(3, grupo.Miembros.Count);
    }

    /// <summary>
    /// El nombre del grupo se teclea a mano en cada una de las cuatro, así que se compara
    /// con la misma manga ancha que los códigos: si no, una mayúscula las desenlaza.
    /// </summary>
    [Fact]
    public void ElNombreDelGrupoNoDistingueMayusculasNiEspacios()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504"),
            Toma("FAM-B", "antar 2504"),
            Toma("FAM-C", "Antar-2504")
        ]);

        Assert.Single(entradas);
        Assert.Equal(3, entradas[0].Miembros.Count);
    }

    /// <summary>
    /// <b>Manda la que lleva las fechas.</b> Es la cabecera del grupo: la que se dibuja,
    /// la que se arrastra y donde va el importe de la oferta.
    /// </summary>
    [Fact]
    public void LaCabeceraEsLaQueLlevaLasFechas()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504"),
            Toma("FAM-B", "ANTAR2504", new DateTime(2026, 8, 10), new DateTime(2026, 8, 21)),
            Toma("FAM-C", "ANTAR2504")
        ]);

        Assert.Equal("FAM-B", entradas[0].Cabecera.CodigoServicio);
    }

    /// <summary>Si ninguna tiene fechas el grupo sigue existiendo: va a la banda de sin planificar.</summary>
    [Fact]
    public void UnGrupoSinFechasNoSeCae()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-B", "ANTAR2504"),
            Toma("FAM-A", "ANTAR2504")
        ]);

        Assert.Single(entradas);
        Assert.Equal("FAM-A", entradas[0].Cabecera.CodigoServicio);   // la primera por código
        Assert.False(entradas[0].Cabecera.Planificacion.HayFechas);
    }

    /// <summary>
    /// El avance de la barra es el del trabajo entero. Un servicio con cuatro familias no
    /// está hecho porque lo esté la primera.
    /// </summary>
    [Fact]
    public void ElAvanceDeLaBarraEsElDeTodasLasEnlazadas()
    {
        var entradas = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504", completadas: 10, aplicables: 10),
            Toma("FAM-B", "ANTAR2504", completadas: 3, aplicables: 10)
        ]);

        Assert.Equal(13, entradas[0].SeccionesCompletadas);
        Assert.Equal(20, entradas[0].SeccionesAplicables);
        Assert.Equal("13/20 secciones", entradas[0].Avance);
    }

    /// <summary>
    /// El importe del grupo es la suma. Con la cabecera llevando el único importe —que es
    /// como debe hacerse— coincide con el de la oferta; si sale el cuádruple, es que se ha
    /// repetido en las cuatro, y ese es exactamente el error que hay que poder ver.
    /// </summary>
    [Fact]
    public void ElImporteDelGrupoEsLaSumaDeLoQueLlevenSusMiembros()
    {
        var bienPuesto = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504", importe: 2000),
            Toma("FAM-B", "ANTAR2504"),
            Toma("FAM-C", "ANTAR2504")
        ]);

        Assert.Equal(2000, bienPuesto[0].Importe);

        var repetido = EnlaceDeTomasDeNotas.Agrupar([
            Toma("FAM-A", "ANTAR2504", importe: 2000),
            Toma("FAM-B", "ANTAR2504", importe: 2000)
        ]);

        Assert.Equal(4000, repetido[0].Importe);

        // Y sin ningún importe, el grupo no cuenta en la carga.
        Assert.Null(EnlaceDeTomasDeNotas.Agrupar([Toma("FAM-A", "ANTAR2504")])[0].Importe);
    }

    /// <summary>Un grupo en blanco no enlaza nada: si no, todas las vacías serían un grupo.</summary>
    [Fact]
    public void ElGrupoVacioNoEnlaza()
    {
        Assert.False(EnlaceDeTomasDeNotas.EsElMismoGrupo("", ""));
        Assert.False(EnlaceDeTomasDeNotas.EsElMismoGrupo(null, null));
        Assert.False(EnlaceDeTomasDeNotas.EsElMismoGrupo("   ", "   "));

        Assert.Equal(2, EnlaceDeTomasDeNotas.Agrupar([Toma("A", "  "), Toma("B", null)]).Count);
    }

    /// <summary>El grupo se guarda con la planificación, así que tiene que copiarse con ella.</summary>
    [Fact]
    public void ElGrupoViajaConLaCopiaDeLaPlanificacion()
    {
        var plan = new Planificacion { Grupo = "ANTAR2504" };

        Assert.Equal("ANTAR2504", plan.Copia().Grupo);

        // Y una planificación que solo lleva grupo no está vacía: hay que guardarla.
        Assert.False(plan.EsVacia);
    }

    /// <summary>
    /// El enlace vive <b>dentro</b> de cada toma de notas y no en un fichero aparte, así
    /// que tiene que sobrevivir al ciclo de guardado como el resto de la planificación —y
    /// seguir ahí cuando el técnico guarde sus datos de ensayo, que no la escribe.
    /// </summary>
    [Fact]
    public void ElEnlaceSeGuardaEnLaTomaDeNotasYSobreviveAlTecnico()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-enlace-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);

        try
        {
            var repositorio = new RepositorioDeProyectos();
            var ruta = Path.Combine(carpeta, "FAM-A" + RepositorioDeProyectos.Extension);

            repositorio.Guardar(new DatosProyecto { CodigoServicio = "FAM-A" }, ruta, "1.0.0");
            repositorio.ActualizarPlanificacion(ruta, new Planificacion { Grupo = "ANTAR2504" });

            Assert.Equal("ANTAR2504", repositorio.LeerPlanificacion(ruta).Grupo);

            // El técnico guarda su toma de notas: la planificación se conserva releyéndola.
            repositorio.Guardar(new DatosProyecto { CodigoServicio = "FAM-A", NumeroMuestras = 3 }, ruta, "1.0.0");

            Assert.Equal("ANTAR2504", repositorio.LeerPlanificacion(ruta).Grupo);
        }
        finally
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
    }
}
