using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// De dónde sale el código de servicio. Un trabajo puede llevar varias familias de
/// luminarias, cada una con su toma de notas: <c>TECNO260201-00</c> y
/// <c>TECNO260202-00</c> son dos del servicio <c>TECNO2602</c>.
/// </summary>
public class CodigoDeServicioTests
{
    [Fact]
    public void SonLasNuevePrimerasDelCodigoDeLaTomaDeNotas()
        => Assert.Equal("TECNO2602", CodigoDeServicio.Derivar("TECNO260201-00"));

    /// <summary>
    /// Dos familias del mismo trabajo dan el mismo servicio. Es lo que hace que el
    /// calendario pueda agruparlas y que el aviso de duplicados no salte por ellas.
    /// </summary>
    [Fact]
    public void DosFamiliasDelMismoTrabajoDanElMismoServicio()
        => Assert.Equal(
            CodigoDeServicio.Derivar("TECNO260201-00"),
            CodigoDeServicio.Derivar("TECNO260202-00"));

    /// <summary>
    /// Mientras se teclea todavía no hay nueve, y el campo de abajo tiene que ir
    /// rellenándose en vez de quedarse en blanco hasta el final.
    /// </summary>
    [Fact]
    public void UnCodigoACediasDevuelveLoQueHaya()
    {
        Assert.Equal("TECN", CodigoDeServicio.Derivar("TECN"));
        Assert.Equal("", CodigoDeServicio.Derivar(""));
        Assert.Equal("", CodigoDeServicio.Derivar(null));
    }

    // ---- lo que encabeza el tablero y el calendario ------------------------

    /// <summary>
    /// Once: servicio y número de familia, sin la edición del documento. El de servicio a
    /// secas no valía —las cuatro familias de un trabajo se llamaban igual—, y el completo
    /// tampoco: el <c>-00</c> se corrige por una errata del técnico y no dice nada de qué
    /// hay que ensayar.
    /// </summary>
    [Fact]
    public void ElRotuloEsElServicioMasLaFamilia()
        => Assert.Equal("TECNO260201", CodigoDeServicio.ConFamilia("TECNO260201-00"));

    [Fact]
    public void DosFamiliasDelMismoTrabajoSeDistinguenEnElRotulo()
        => Assert.NotEqual(CodigoDeServicio.ConFamilia("TECNO260201-00"),
                           CodigoDeServicio.ConFamilia("TECNO260202-00"));

    /// <summary>Reeditar un documento no cambia cómo se llama en el tablero.</summary>
    [Fact]
    public void LaEdicionDelDocumentoNoCambiaElRotulo()
        => Assert.Equal(CodigoDeServicio.ConFamilia("TECNO260201-00"),
                        CodigoDeServicio.ConFamilia("TECNO260201-03"));

    /// <summary>
    /// Un proyecto anterior a que existiera el código se cae al de servicio y, si tampoco
    /// lo hay, al nombre del fichero: una tarjeta sin rótulo no se puede ni señalar.
    /// </summary>
    [Fact]
    public void SinCodigoDeTomaDeNotasElRotuloNoSeQuedaEnBlanco()
    {
        var viejo = new ResumenDeProyecto
        { Ruta = @"C:\x.lmnlab", Nombre = "TdN_60598_ANTAR2504", CodigoServicio = "ANTAR2504" };

        Assert.Equal("ANTAR2504", viejo.Rotulo);

        var sinNada = new ResumenDeProyecto { Ruta = @"C:\x.lmnlab", Nombre = "TdN_60598_x" };
        Assert.Equal("TdN_60598_x", sinNada.Rotulo);
    }

    /// <summary>Con código, el rótulo sale de él y no del de servicio.</summary>
    [Fact]
    public void ConCodigoElRotuloSaleDeEl()
    {
        var resumen = new ResumenDeProyecto
        {
            Ruta = @"C:\x.lmnlab",
            Nombre = "TdN_60598_TECNO260201-00",
            CodigoTomaDeNotas = "TECNO260201-00",
            CodigoServicio = "TECNO2602"
        };

        Assert.Equal("TECNO260201", resumen.Rotulo);
    }

    [Fact]
    public void SeRellenaSoloCuandoEstabaVacio()
        => Assert.Equal("TECNO2602", CodigoDeServicio.Sugerir("", "TECNO260201-00", ""));

    /// <summary>
    /// Lo que puso el programa se sigue actualizando: el técnico corrige una letra arriba
    /// y el de abajo la sigue, que es lo que se espera de un campo que se rellena solo.
    /// </summary>
    [Fact]
    public void LoQuePusoElProgramaSeSigueActualizando()
        => Assert.Equal("TECNO2603",
            CodigoDeServicio.Sugerir("TECNO260201-00", "TECNO260301-00", "TECNO2602"));

    /// <summary>
    /// <b>Lo escrito a mano no se pisa.</b> Hay servicios cuyo código no son las nueve
    /// primeras, y corregirlo tiene que aguantar que se siga tecleando arriba — si no, el
    /// técnico lo arregla y el programa se lo deshace en la siguiente pulsación.
    /// </summary>
    [Fact]
    public void LoEscritoAManoNoSePisa()
        => Assert.Equal("OTRO-9999",
            CodigoDeServicio.Sugerir("TECNO260201-00", "TECNO260202-00", "OTRO-9999"));

    /// <summary>El marcador que pone el alta cuenta como vacío, no como algo escrito.</summary>
    [Fact]
    public void ElMarcadorDeProyectoNuevoCuentaComoVacio()
        => Assert.Equal("TECNO2602",
            CodigoDeServicio.Sugerir("", "TECNO260201-00", RequisitosDelProyecto.CodigoSinAsignar));

    // ---- cómo llega a la cabecera y al fichero ------------------------------

    /// <summary>
    /// El alta rápida pregunta el código de la toma de notas y deja los dos puestos: el
    /// responsable teclea una vez y el técnico no tiene que rellenar el de servicio.
    /// </summary>
    [Fact]
    public void ElAltaDejaLosDosCodigosPuestos()
    {
        var datos = AltaDeProyecto.Crear("TECNO260201-00", "Javier Ibor");

        Assert.Equal("TECNO260201-00", datos.CodigoTomaDeNotas);
        Assert.Equal("TECNO2602", datos.CodigoServicio);
    }

    /// <summary>
    /// El fichero lo nombra el de la toma de notas, no el de servicio: si lo nombrara el
    /// de servicio, las cuatro familias de un trabajo pelearían por el mismo nombre.
    /// </summary>
    [Fact]
    public void ElFicheroLoNombraElCodigoDeLaTomaDeNotas()
    {
        var datos = AltaDeProyecto.Crear("TECNO260201-00", "Javier Ibor");

        Assert.Equal("TdN_60598_TECNO260201-00.lmnlab",
            NombreDeTomaDeNotas.ConExtension("60598", datos.CodigoTomaDeNotas, ".lmnlab"));
    }

    /// <summary>
    /// Es obligatorio: sin él no se puede nombrar el fichero ni saber de qué familia es la
    /// toma de notas, así que la cabecera no está completa.
    /// </summary>
    [Fact]
    public void SinCodigoDeTomaDeNotasLaCabeceraNoEstaCompleta()
    {
        var datos = Contexto.ProyectoVacio();
        datos.CodigoTomaDeNotas = "";

        Assert.Contains(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos),
            f => f.Contains("toma de notas"));
    }

    // ---- el código se exige entero -----------------------------------------

    /// <summary>
    /// <b>Catorce, exactos.</b> De este código se recortan los otros tres —el de servicio
    /// (nueve), el de familia (once) y el nombre del fichero—, así que uno corto los deja a
    /// los tres mal y arreglarlo después obliga a renombrar.
    /// </summary>
    [Theory]
    [InlineData("TECNO260201-00", true)]
    [InlineData("  TECNO260201-00  ", true)]
    [InlineData("TECNO2602", false)]       // solo el servicio
    [InlineData("TECNO260201", false)]     // servicio y familia, sin la edición
    [InlineData("TECNO260201-000", false)] // una de más
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void ElCodigoSeExigeEntero(string? codigo, bool entero)
        => Assert.Equal(entero, CodigoDeServicio.EstaCompleto(codigo));

    /// <summary>
    /// Las tres longitudes salen del mismo sitio y encajan: el de servicio y el de familia
    /// **se recortan** del entero, nunca al revés.
    /// </summary>
    [Fact]
    public void LasTresLongitudesEncajan()
    {
        Assert.True(CodigoDeServicio.Longitud < CodigoDeServicio.LongitudConFamilia);
        Assert.True(CodigoDeServicio.LongitudConFamilia < CodigoDeServicio.LongitudCompleta);

        const string entero = "TECNO260201-00";
        Assert.Equal(CodigoDeServicio.LongitudCompleta, entero.Length);
        Assert.Equal(CodigoDeServicio.Longitud, CodigoDeServicio.Derivar(entero).Length);
        Assert.Equal(CodigoDeServicio.LongitudConFamilia, CodigoDeServicio.ConFamilia(entero).Length);
    }

    /// <summary>
    /// <b>El alta y la cabecera exigen lo mismo</b> (2026‑08‑06). Antes solo lo hacía el
    /// alta, y entonces la regla dependía de por dónde hubiera entrado la toma de notas.
    /// </summary>
    [Fact]
    public void ElAltaYLaCabeceraExigenLoMismo()
    {
        var datos = Contexto.ProyectoVacio();
        datos.CodigoTomaDeNotas = "ANTAR2504";

        Assert.False(AltaDeProyecto.CodigoCompleto(datos.CodigoTomaDeNotas));
        Assert.Contains(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos),
            f => f.Contains("toma de notas"));

        datos.CodigoTomaDeNotas = "ANTAR250401-00";

        Assert.True(AltaDeProyecto.CodigoCompleto(datos.CodigoTomaDeNotas));
        Assert.DoesNotContain(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos),
            f => f.Contains("toma de notas"));
    }
}
