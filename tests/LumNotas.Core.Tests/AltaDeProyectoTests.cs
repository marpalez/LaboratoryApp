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
        Assert.True(AltaDeProyecto.SePuedeCrear("ANTAR2504", "Javier Ibor", "60598"));

        Assert.Equal([AltaDeProyecto.CampoNombre], AltaDeProyecto.Faltan("", "Javier Ibor", "60598"));
        Assert.Equal([AltaDeProyecto.CampoTecnico], AltaDeProyecto.Faltan("ANTAR2504", null, "60598"));
        Assert.Equal([AltaDeProyecto.CampoNorma], AltaDeProyecto.Faltan("ANTAR2504", "Javier Ibor", null));
        Assert.Equal(3, AltaDeProyecto.Faltan(null, null, null).Count);

        // Los espacios no cuentan como nombre.
        Assert.False(AltaDeProyecto.SePuedeCrear("   ", "Javier Ibor", "60598"));
    }

    /// <summary>
    /// <b>La norma pasó a ser obligatoria</b> (antes no lo era): una toma de notas sin
    /// norma no tiene apartados que rellenar, y el nombre del fichero la lleva dentro, así
    /// que dejarla para después obligaba a renombrarlo.
    /// </summary>
    [Fact]
    public void SinNormaNoSePuedeDarDeAlta()
    {
        Assert.False(AltaDeProyecto.SePuedeCrear("ANTAR2504", "Javier Ibor", null));
        Assert.False(AltaDeProyecto.SePuedeCrear("ANTAR2504", "Javier Ibor", "  "));
    }

    /// <summary>El técnico 2 sigue sin bloquear: hay servicios con un solo técnico.</summary>
    [Fact]
    public void ElSegundoTecnicoNoHaceFalta()
    {
        Assert.True(AltaDeProyecto.SePuedeCrear("ANTAR2504", "Javier Ibor", "60598"));

        var datos = AltaDeProyecto.Crear("ANTAR2504", "Javier Ibor", principal: Contexto.Plantilla);

        Assert.Equal("ANTAR2504", datos.CodigoServicio);
        Assert.Equal("Javier Ibor", datos.Tecnico1);
        Assert.Null(datos.Tecnico2);
        Assert.Equal("60598", datos.NormaPrincipal);
    }

    [Fact]
    public void ConNormaElegidaQuedaApuntadaComoPrincipal()
    {
        var datos = AltaDeProyecto.Crear(
            "ANTAR2504", "Javier Ibor", "Mario Madrigal", Contexto.Plantilla);

        Assert.Equal("Mario Madrigal", datos.Tecnico2);
        Assert.Equal("60598", datos.NormaPrincipal);
        Assert.Contains("60598", datos.Normas);
    }

    /// <summary>
    /// <b>Lo que la norma exige para ensayar no puede impedir crear el proyecto.</b> Es la
    /// misma regla dicha al revés: un proyecto recién dado de alta tiene la cabecera casi
    /// entera por rellenar —clase, Ta, grado IP, partes ‑2— y eso es lo normal.
    /// </summary>
    [Fact]
    public void UnProyectoReciennacidoTieneLaCabeceraSinRellenarYNoPasaNada()
    {
        var datos = AltaDeProyecto.Crear("ANTAR2504", "Javier Ibor", principal: Contexto.Plantilla);

        // La toma de notas dirá que falta casi todo…
        Assert.NotEmpty(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos));

        // …y aun así el proyecto existe, tiene responsable y se puede analizar y planificar.
        var resumen = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x.lumproj", DateTime.Now);

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
        => Assert.Equal(1, AltaDeProyecto.Crear("ANTAR2504", "Javier Ibor").NumeroMuestras);
}
