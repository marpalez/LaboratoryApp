using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Dar de alta un proyecto para planificarlo.
/// <para>
/// La regla que se vigila aquí la puso el laboratorio y está por encima de cualquier
/// comodidad: <b>solo hacen falta el nombre, el técnico 1 y la norma</b>. El responsable
/// planifica antes de que exista un dato de ensayo, y todo lo que se añada al alta le
/// quita valor a eso.
/// </para>
/// </summary>
public class AltaDeProyectoTests
{
    [Fact]
    public void HacenFaltaElNombreElTecnicoYLaNorma()
    {
        Assert.True(AltaDeProyecto.SePuedeCrear("ANTAR250401-00", "Javier Ibor", "60598"));

        Assert.Equal([AltaDeProyecto.CampoNombre], AltaDeProyecto.Faltan("", "Javier Ibor", "60598"));
        Assert.Equal([AltaDeProyecto.CampoTecnico], AltaDeProyecto.Faltan("ANTAR250401-00", null, "60598"));
        Assert.Equal([AltaDeProyecto.CampoNorma], AltaDeProyecto.Faltan("ANTAR250401-00", "Javier Ibor", null));
        Assert.Equal(3, AltaDeProyecto.Faltan(null, null, null).Count);

        // Los espacios no cuentan como nombre.
        Assert.False(AltaDeProyecto.SePuedeCrear("   ", "Javier Ibor", "60598"));
    }

    /// <summary>
    /// <b>El código se exige completo: 14 caracteres</b> (2026‑08‑06). De él salen el
    /// código de servicio, el identificador de las muestras y el nombre del fichero, así
    /// que uno a medias los deja a los tres mal y corregirlo obliga a renombrar.
    /// </summary>
    [Theory]
    [InlineData("TECNO260201-00", true)]
    [InlineData("  TECNO260201-00  ", true)]
    [InlineData("TECNO260201", false)]
    [InlineData("TECNO260201-000", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ElCodigoSeExigeCompleto(string? codigo, bool vale)
    {
        Assert.Equal(vale, AltaDeProyecto.CodigoCompleto(codigo));
        Assert.Equal(vale, AltaDeProyecto.SePuedeCrear(codigo, "Javier Ibor", "60598"));
    }

    /// <summary>
    /// <b>Se exige por los tres caminos</b>: al dar de alta, para guardar y para empezar a
    /// ensayar. Durante unas horas del 2026‑08‑06 guardar fue la excepción —para no dejar
    /// atrapados a los proyectos anteriores a la regla—, y el laboratorio la retiró: con
    /// ella puesta, esos proyectos se quedaban con el código a medias para siempre.
    /// </summary>
    [Fact]
    public void UnCodigoAMediasNoSeGuardaAunqueElProyectoYaExista()
    {
        var datos = Contexto.ProyectoVacio();
        datos.CodigoTomaDeNotas = "ANTAR2504";
        datos.Tecnico1 = "Javier Ibor";

        Assert.False(RequisitosParaGuardar.SePuede(datos));

        datos.CodigoTomaDeNotas = "ANTAR250401-00";
        Assert.True(RequisitosParaGuardar.SePuede(datos));
    }

    /// <summary>
    /// <b>La norma pasó a ser obligatoria</b> (antes no lo era): una toma de notas sin
    /// norma no tiene apartados que rellenar, y el nombre del fichero la lleva dentro, así
    /// que dejarla para después obligaba a renombrarlo.
    /// </summary>
    [Fact]
    public void SinNormaNoSePuedeDarDeAlta()
    {
        Assert.False(AltaDeProyecto.SePuedeCrear("ANTAR250401-00", "Javier Ibor", null));
        Assert.False(AltaDeProyecto.SePuedeCrear("ANTAR250401-00", "Javier Ibor", "  "));
    }

    /// <summary>El técnico 2 sigue sin bloquear: hay servicios con un solo técnico.</summary>
    [Fact]
    public void ElSegundoTecnicoNoHaceFalta()
    {
        Assert.True(AltaDeProyecto.SePuedeCrear("ANTAR250401-00", "Javier Ibor", "60598"));

        var datos = AltaDeProyecto.Crear("ANTAR250401-00", "Javier Ibor", principal: Contexto.Plantilla);

        // El de servicio son las nueve primeras del de la toma de notas.
        Assert.Equal("ANTAR2504", datos.CodigoServicio);
        Assert.Equal("Javier Ibor", datos.Tecnico1);
        Assert.Null(datos.Tecnico2);
        Assert.Equal(Contexto.Plantilla.Meta.Id, datos.NormaPrincipal);
    }

    [Fact]
    public void ConNormaElegidaQuedaApuntadaComoPrincipal()
    {
        var datos = AltaDeProyecto.Crear(
            "ANTAR250401-00", "Javier Ibor", "Mario Madrigal", Contexto.Plantilla);

        Assert.Equal("Mario Madrigal", datos.Tecnico2);
        Assert.Equal(Contexto.Plantilla.Meta.Id, datos.NormaPrincipal);
        Assert.Contains(Contexto.Plantilla.Meta.Id, datos.Normas);
    }

    /// <summary>
    /// <b>Lo que la norma exige para ensayar no puede impedir crear el proyecto.</b> Es la
    /// misma regla dicha al revés: un proyecto recién dado de alta tiene la cabecera casi
    /// entera por rellenar —clase, Ta, grado IP, partes ‑2— y eso es lo normal.
    /// </summary>
    [Fact]
    public void UnProyectoReciennacidoTieneLaCabeceraSinRellenarYNoPasaNada()
    {
        var datos = AltaDeProyecto.Crear("ANTAR250401-00", "Javier Ibor", principal: Contexto.Plantilla);

        // La toma de notas dirá que falta casi todo…
        Assert.NotEmpty(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos));

        // …y aun así el proyecto existe, tiene responsable y se puede analizar y planificar.
        var resumen = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x.lmnlab", DateTime.Now);

        Assert.Null(resumen.Error);
        Assert.Equal("Javier Ibor", resumen.Tecnico);
        Assert.Equal("ANTAR2504", resumen.CodigoServicio);
        Assert.False(resumen.Terminado);
    }

    /// <summary>
    /// El nombre lo teclea una persona: «ANTAR2504/01» es una forma natural de nombrar un
    /// servicio y reventaría al componer la ruta del fichero.
    /// </summary>
    [Fact]
    public void UnNombreConBarrasNoRompeElNombreDelFichero()
    {
        Assert.Equal("ANTAR2504-01", AltaDeProyecto.NombreDeFichero("ANTAR2504/01"));
        Assert.Equal("ANTAR2504", AltaDeProyecto.NombreDeFichero("  ANTAR2504  "));

        // Y un nombre que fuera solo signos no puede dejar el fichero sin nombre.
        Assert.False(string.IsNullOrWhiteSpace(AltaDeProyecto.NombreDeFichero("///")));
    }

    /// <summary>Se da de alta con una muestra: el número lo ajusta el técnico al empezar.</summary>
    [Fact]
    public void NaceConUnaMuestra()
        => Assert.Equal(1, AltaDeProyecto.Crear("ANTAR250401-00", "Javier Ibor").NumeroMuestras);
}
