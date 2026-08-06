using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// En qué se parte la barra de un trabajo con varias familias enlazadas. El laboratorio
/// planifica un trabajo, pero el técnico tiene que seguir viendo las familias que lo
/// componen.
/// </summary>
public class TramosDelGrupoTests
{
    private static ResumenDeProyecto Familia(string codigo, DateTime? inicio = null, DateTime? fin = null)
        => new()
        {
            Ruta = $@"C:\clientes\{codigo}.lmnlab",
            Nombre = codigo,
            CodigoTomaDeNotas = codigo,
            Planificacion = new Planificacion { Inicio = inicio, Fin = fin, Grupo = "ANTAR2504" }
        };

    private static EntradaDeCalendario Grupo(params ResumenDeProyecto[] familias)
        => Assert.Single(EnlaceDeTomasDeNotas.Agrupar(familias));

    private static readonly DateTime Sep1 = new(2026, 9, 1);
    private static readonly DateTime Sep15 = new(2026, 9, 15);
    private static readonly DateTime Sep30 = new(2026, 9, 30);
    private static readonly DateTime Oct10 = new(2026, 10, 10);

    // ---- el trabajo entero -------------------------------------------------

    /// <summary>
    /// <b>La barra abarca de la primera a la última.</b> Antes ocupaba solo las fechas de
    /// la cabecera, así que un trabajo de mes y medio se dibujaba como si durase dos
    /// semanas.
    /// </summary>
    [Fact]
    public void ElTrabajoVaDeLaPrimeraFechaALaUltima()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30),
            Familia("ANTAR250403-00", fin: Oct10));

        Assert.Equal(Sep1, grupo.Inicio);
        Assert.Equal(Oct10, grupo.Fin);
    }

    /// <summary>
    /// Una familia <b>sin fechas alarga el trabajo</b> lo que duran las demás. Antes se
    /// metía dentro del tramo de la que sí las tenía, y el resultado era que planificar
    /// dos semanas y anexar una segunda familia dejaba **una semana para cada una**: la
    /// primera perdía la mitad de su plazo por haberle puesto compañía. El trabajo son dos
    /// familias, así que dura el doble.
    /// </summary>
    [Fact]
    public void LaFamiliaSinFechasAlargaElTrabajo()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00"));

        Assert.Equal(Sep1, grupo.Inicio);

        // Del 1 al 15 son quince días contando los dos, así que dos familias son treinta.
        Assert.Equal(Sep1.AddDays(29), grupo.Fin);

        // Y la que sí las tiene conserva sus dos semanas, que es lo que se planificó.
        var tramos = grupo.Tramos;
        Assert.Equal(Sep1, tramos[0].Desde);
        Assert.Equal(Sep15, tramos[0].Hasta);
    }

    // ---- cómo se parte ------------------------------------------------------

    /// <summary>
    /// <b>Se dibujan una detrás de otra, sin huecos.</b> Cada familia empieza donde acabó
    /// la anterior y conserva su duración: la segunda lleva escritos 41 días y esos 41 días
    /// son suyos, solo que empezando donde le toque en la fila.
    /// <para>
    /// El orden lo dan <b>las fechas de inicio</b> (DD‑123), así que aquí encabeza la del 20
    /// de agosto aunque su código sea el segundo. Con datos ya encadenados por
    /// <see cref="CadenaDelGrupo"/> las dos cosas coinciden; esto cubre lo que se dibuja
    /// mientras no lo estén.
    /// </para>
    /// </summary>
    [Fact]
    public void SeDibujanUnaDetrasDeOtraSinHuecos()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", inicio: new DateTime(2026, 8, 20), fin: Sep30),
            Familia("ANTAR250403-00", fin: Oct10)).Tramos;

        Assert.Equal(3, tramos.Count);

        // Manda la fecha de inicio, no el código.
        Assert.Equal(["ANTAR250402", "ANTAR250401", "ANTAR250403"],
                     [.. tramos.Select(t => t.Miembro.Rotulo)]);

        Assert.Equal(new DateTime(2026, 8, 20), tramos[0].Desde);
        Assert.Equal(41, (tramos[0].Hasta - tramos[0].Desde).TotalDays);

        // Y ninguna deja hueco ni pisa a la de al lado: la siguiente arranca **al día
        // siguiente** del último día de la anterior, igual que lo escrito en el fichero.
        for (var i = 1; i < tramos.Count; i++)
            Assert.Equal(tramos[i - 1].Hasta.AddDays(1), tramos[i].Desde);

        Assert.All(tramos, t => Assert.True(t.Fraccion > 0, $"{t.Miembro.Rotulo} sin sitio"));
    }

    /// <summary>El orden lo da el código, no las fechas: así no bailan de sitio.</summary>
    [Fact]
    public void ElOrdenLoDaElCodigo()
    {
        var tramos = Grupo(
            Familia("ANTAR250403-00", fin: Oct10),
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30)).Tramos;

        Assert.Equal(["ANTAR250401", "ANTAR250402", "ANTAR250403"],
                     tramos.Select(t => t.Miembro.Rotulo));
    }

    /// <summary>Los tramos cubren la barra entera, sin huecos ni solapes.</summary>
    [Fact]
    public void LosTramosSumanLaBarraEntera()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30),
            Familia("ANTAR250403-00", fin: Oct10)).Tramos;

        Assert.Equal(1.0, tramos.Sum(t => t.Fraccion), 6);
    }

    /// <summary>
    /// <b>Las que no traen fecha no desaparecen</b>: se reparten a partes iguales lo que
    /// quede hasta la siguiente que sí la tenga.
    /// </summary>
    [Fact]
    public void LasQueNoTienenFechaSeReparteLoQueQueda()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00"),
            Familia("ANTAR250403-00"),
            Familia("ANTAR250404-00", fin: Oct10)).Tramos;

        Assert.Equal(4, tramos.Count);
        Assert.All(tramos, t => Assert.True(t.Fraccion > 0, $"{t.Miembro.Rotulo} sin ancho"));

        // Las dos de en medio se reparten por igual el hueco del 15-sep al 10-oct.
        Assert.Equal(tramos[1].Fraccion, tramos[2].Fraccion, 6);
    }

    /// <summary>
    /// Sin ninguna fecha por familia, partes iguales: es lo único que los datos permiten
    /// decir cuando nadie ha planificado familia a familia.
    /// </summary>
    [Fact]
    public void SinFechasPropiasSeParteEnPartesIguales()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Oct10),
            Familia("ANTAR250402-00"),
            Familia("ANTAR250403-00")).Tramos;

        Assert.Equal(3, tramos.Count);
        Assert.All(tramos, t => Assert.Equal(1d / 3, t.Fraccion, 6));
    }

    /// <summary>Una fecha que caiga hacia atrás no parte la barra al revés.</summary>
    [Fact]
    public void UnaFechaHaciaAtrasNoRompeElOrden()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep30),
            Familia("ANTAR250402-00", fin: Sep15),   // acaba antes que la anterior
            Familia("ANTAR250403-00", fin: Oct10)).Tramos;

        Assert.All(tramos, t => Assert.True(t.Hasta >= t.Desde));
        Assert.Equal(1.0, tramos.Sum(t => t.Fraccion), 6);
    }

    // ---- lo que no es un grupo ---------------------------------------------

    /// <summary>
    /// Una toma de notas suelta da un solo tramo con la barra entera, para que quien
    /// dibuje no tenga que distinguir casos.
    /// </summary>
    [Fact]
    public void UnaSueltaDaUnSoloTramoConTodoElAncho()
    {
        var suelta = new ResumenDeProyecto
        {
            Ruta = @"C:\x.lmnlab",
            Nombre = "MOONO230401-00",
            CodigoTomaDeNotas = "MOONO230401-00",
            Planificacion = new Planificacion { Inicio = Sep1, Fin = Sep15 }
        };

        var tramo = Assert.Single(Grupo(suelta).Tramos);

        Assert.Equal(1.0, tramo.Fraccion, 6);
        Assert.Equal(Sep1, tramo.Desde);
        Assert.Equal(Sep15, tramo.Hasta);
    }

    /// <summary>Sin planificar tampoco revienta: sigue habiendo un tramo por familia.</summary>
    [Fact]
    public void SinPlanificarSigueHabiendoUnTramoPorFamilia()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00"),
            Familia("ANTAR250402-00")).Tramos;

        Assert.Equal(2, tramos.Count);
    }

    /// <summary>
    /// <b>Una familia cuyo fin no sirve de corte no se queda sin sitio.</b> Pasa cuando su
    /// fecha cae donde ya vamos —aquí la primera acaba el mismo día que empieza el
    /// trabajo—: la que viene detrás sí tiene fecha buena, y las dos tienen que repartirse
    /// el hueco hasta ella. Antes esa combinación reventaba el reparto.
    /// </summary>
    [Fact]
    public void UnaFechaQueNoCortaNoDejaSinSitioALaSiguiente()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep1),
            Familia("ANTAR250402-00", fin: Sep15),
            Familia("ANTAR250403-00", fin: Sep30));

        var tramos = grupo.Tramos;

        Assert.Equal(3, tramos.Count);

        // Un solo día es sitio de sobra; cero no se vería. La primera ocupa exactamente eso.
        Assert.All(tramos, t => Assert.True(t.Fraccion > 0, $"{t.Miembro.Rotulo} sin sitio"));

        // Y entre las tres cubren el trabajo entero, sin huecos ni solapes.
        Assert.Equal(Sep1, tramos[0].Desde);
        Assert.Equal(grupo.Fin, tramos[^1].Hasta);
        Assert.Equal(1.0, tramos.Sum(t => t.Fraccion), 6);

        for (var i = 1; i < tramos.Count; i++)
            Assert.Equal(tramos[i - 1].Hasta.AddDays(1), tramos[i].Desde);
    }

    // ---- cada familia dura lo suyo -----------------------------------------

    /// <summary>
    /// <b>Dos familias planificadas de distinta duración se dibujan de distinto tamaño.</b>
    /// Es el fallo que dio la cara en el laboratorio: si una lleva cinco días y la anexada
    /// quince, salían las dos iguales. Pasaba porque el reparto razonaba con <b>fechas de
    /// corte</b>, y la fecha de la primera —que era además el final del trabajo— no servía
    /// de corte, así que se tiraba y las dos se repartían el hueco a partes iguales.
    /// </summary>
    [Fact]
    public void CadaFamiliaDuraLoSuyo()
    {
        var corta = Familia("ANTAR250401-00", Sep1, Sep1.AddDays(5));
        var larga = Familia("ANTAR250402-00", Sep1.AddDays(5), Sep1.AddDays(20));

        var tramos = Grupo(corta, larga).Tramos;

        Assert.Equal(5, (tramos[0].Hasta - tramos[0].Desde).TotalDays);
        Assert.Equal(15, (tramos[1].Hasta - tramos[1].Desde).TotalDays);
    }

    /// <summary>
    /// Y lo mismo cuando la <b>primera</b> es la que abarca todo el trabajo, que es como se
    /// planifica cuando se anexa una segunda a un servicio ya metido en el calendario. Su
    /// fecha de fin coincidía con la del trabajo, no servía de corte, y las dos salían por
    /// la mitad.
    /// </summary>
    [Fact]
    public void LaAnexadaNoCopiaElTamanoDeLaPrimera()
    {
        var primera = Familia("ANTAR250401-00", Sep1, Sep1.AddDays(21));
        var anexada = Familia("ANTAR250402-00", Sep1.AddDays(7), Sep1.AddDays(14));

        var tramos = Grupo(primera, anexada).Tramos;

        Assert.Equal(21, (tramos[0].Hasta - tramos[0].Desde).TotalDays);
        Assert.Equal(7, (tramos[1].Hasta - tramos[1].Desde).TotalDays);
    }

    /// <summary>
    /// El trabajo acaba <b>donde acaba la cadena</b>, no en la fecha más tardía que haya
    /// escrita. La anexada se planificó sin saber dónde iba a caer.
    /// </summary>
    [Fact]
    public void ElTrabajoAcabaDondeAcabaLaCadena()
    {
        var grupo = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(21)),
            Familia("ANTAR250402-00", Sep1.AddDays(7), Sep1.AddDays(14)));

        // 22 días la primera y 8 la anexada, contando los dos extremos: 30 en total.
        Assert.Equal(Sep1.AddDays(29), grupo.Fin);
    }

    /// <summary>
    /// La que no dice cuánto dura <b>dura como las demás</b>, no la mitad de todo. Antes se
    /// llevaba por delante el tamaño de las que sí lo decían.
    /// </summary>
    [Fact]
    public void LaQueNoDiceCuantoDuraDuraComoLasDemas()
    {
        var tramos = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(10)),
            Familia("ANTAR250402-00")).Tramos;

        Assert.Equal(10, (tramos[0].Hasta - tramos[0].Desde).TotalDays);
        Assert.Equal(10, (tramos[1].Hasta - tramos[1].Desde).TotalDays);
    }

    /// <summary>
    /// <b>Mover el trabajo no deforma nada.</b> Es lo que se ve al arrastrar: cada familia
    /// conserva su duración y solo cambia de sitio. Antes se encogía la primera, porque su
    /// fecha de fin seguía clavada donde estaba mientras la barra se iba.
    /// </summary>
    [Fact]
    public void MoverElTrabajoConservaLasDuraciones()
    {
        // Ya encadenadas como las deja CadenaDelGrupo: 5 días y 15, sin pisarse.
        var miembros = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep1.AddDays(4)),
            Familia("ANTAR250402-00", Sep1.AddDays(5), Sep1.AddDays(19))).EnOrden;

        // Una semana más tarde, de punta a punta.
        var tramos = TramosDelGrupo.Calcular(miembros, Sep1.AddDays(7), Sep1.AddDays(26));

        Assert.Equal(Sep1.AddDays(7), tramos[0].Desde);
        Assert.Equal(4, (tramos[0].Hasta - tramos[0].Desde).TotalDays);
        Assert.Equal(14, (tramos[1].Hasta - tramos[1].Desde).TotalDays);
        Assert.Equal(tramos[0].Hasta.AddDays(1), tramos[1].Desde);
    }

    // ---- mientras se arrastra ----------------------------------------------
    //
    // Arrastrando, el trabajo enseña unas fechas que todavía no están guardadas, y las
    // tarjetas tienen que seguirlas. Por eso se puede pedir el reparto diciendo de cuándo
    // a cuándo va el trabajo, en vez de leerlo de las familias.

    /// <summary>
    /// <b>Estirar por la derecha solo alarga la última.</b> Es lo que luego se guarda
    /// —RepartoDelArrastre toca solo esa—, así que si al arrastrar se ensancharan las
    /// cuatro a proporción, al soltar se verían saltar a otro sitio.
    /// </summary>
    [Fact]
    public void EstirandoPorLaDerechaSoloCreceLaUltima()
    {
        var miembros = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30)).EnOrden;

        var tramos = TramosDelGrupo.Calcular(miembros, Sep1, Oct10);

        // La frontera sigue siendo el 15: es una fecha de la primera familia, no un reparto.
        // Y la segunda arranca al día siguiente, como en su fichero.
        Assert.Equal(Sep15, tramos[0].Hasta);
        Assert.Equal(Sep15.AddDays(1), tramos[1].Desde);
        Assert.Equal(Oct10, tramos[1].Hasta);
    }

    /// <summary>Y por la izquierda, solo la primera.</summary>
    [Fact]
    public void EstirandoPorLaIzquierdaSoloCreceLaPrimera()
    {
        var miembros = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30)).EnOrden;

        var tramos = TramosDelGrupo.Calcular(miembros, Sep1.AddDays(-10), Sep30);

        Assert.Equal(Sep1.AddDays(-10), tramos[0].Desde);
        Assert.Equal(Sep15, tramos[0].Hasta);
        Assert.Equal(Sep30, tramos[1].Hasta);
    }

    /// <summary>
    /// Las fracciones suman uno pase lo que pase: son el ancho en píxeles de cada tarjeta y
    /// entre todas tienen que dar el del trabajo, ni más ni menos.
    /// </summary>
    [Fact]
    public void LasFraccionesSumanUno()
    {
        var miembros = Grupo(
            Familia("ANTAR250401-00", Sep1, Sep15),
            Familia("ANTAR250402-00", fin: Sep30),
            Familia("ANTAR250403-00")).EnOrden;

        var tramos = TramosDelGrupo.Calcular(miembros, Sep1, Oct10);

        Assert.Equal(1.0, tramos.Sum(t => t.Fraccion), 6);
    }

    /// <summary>
    /// Siempre hay <b>un tramo por familia</b>, en el orden recibido. Es lo que permite que
    /// las tarjetas se recalculen en el sitio durante el arrastre en vez de rehacerse: si
    /// el número bailara, habría que sustituirlas y se perdería el ratón a mitad del gesto.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void SiempreUnTramoPorFamiliaYEnSuOrden(int cuantas)
    {
        var miembros = Grupo([.. Enumerable.Range(1, cuantas)
            .Select(i => Familia($"ANTAR2504{i:00}-00", fin: Sep1.AddDays(i * 5)))]).EnOrden;

        var tramos = TramosDelGrupo.Calcular(miembros, Sep1, Oct10);

        Assert.Equal(cuantas, tramos.Count);
        Assert.Equal(miembros.Select(m => m.Rotulo), tramos.Select(t => t.Miembro.Rotulo));
    }
}
