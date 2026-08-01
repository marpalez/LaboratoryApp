using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>Los cálculos de ingeniería de la sección 6 del documento de reglas.</summary>
public class CalculosTests
{
    private const string Ambito = "generales";

    private static MotorDeReglas ConTamano(double alto, double ancho, double profundo, params string[] gradosIp)
    {
        var datos = Contexto.ProyectoVacio();
        foreach (var g in gradosIp) datos.IpSegundaCifra.Add(g);
        datos.Establecer(Ambito, "tamano[0].alto", alto, 1);
        datos.Establecer(Ambito, "tamano[0].ancho", ancho, 1);
        datos.Establecer(Ambito, "tamano[0].profundo", profundo, 1);
        return Contexto.Motor(datos);
    }

    [Fact]
    public void C04_SemidiagonalDeLaBase()
    {
        var motor = ConTamano(alto: 0, ancho: 60, profundo: 80);
        // √((60/2)² + (80/2)²) = √(900 + 1600) = 50
        Assert.Equal(50, Calculos.SemidiagonalBase(motor, Ambito, 1), 6);
    }

    [Fact]
    public void C05_DistanciaDelArco()
    {
        var motor = ConTamano(alto: 120, ancho: 60, profundo: 80);
        // √(120² + 50²) = 130
        Assert.Equal(130, Calculos.DistanciaArco(motor, Ambito, 1), 6);
    }

    [Fact]
    public void C06_RadioMaximoAnadeVeinteCentimetros()
    {
        var motor = ConTamano(alto: 120, ancho: 60, profundo: 80);
        Assert.Equal(150, Calculos.RadioMaximo(motor, Ambito, 1), 6);
    }

    [Fact]
    public void C08_EnIpx4LaAlturaSeTomaAMitad()
    {
        Assert.Equal(50, Calculos.AlturaEfectiva(ConTamano(100, 0, 0, "IPX4"), Ambito, 1), 6);
        Assert.Equal(100, Calculos.AlturaEfectiva(ConTamano(100, 0, 0, "IPX3"), Ambito, 1), 6);
    }

    [Theory]
    [InlineData(10, 10, 10)]
    [InlineData(30, 40, 40)]
    [InlineData(90, 60, 80)]
    public void C07_EligeElArcoMasPequenoQueCubreElRadioMaximo(double alto, double ancho, double profundo)
    {
        var motor = ConTamano(alto, ancho, profundo, "IPX3");
        var rMax = Calculos.RadioMaximo(motor, Ambito, 1);
        var elegido = Assert.IsType<int>(Calculos.RadioEnsayo(motor, Ambito, 1));

        Assert.True(elegido >= rMax, $"El arco elegido ({elegido}) debe cubrir el radio máximo ({rMax:F1}).");
        Assert.All(Calculos.ArcosDisponibles.Where(a => a < elegido),
            a => Assert.True(a < rMax, $"El arco de {a} cm también cubría {rMax:F1} y debería haberse elegido."));
    }

    [Fact]
    public void C07_MuestraEnormeRequiereCabezaDeRegadera()
    {
        var motor = ConTamano(alto: 200, ancho: 100, profundo: 100, "IPX3");
        Assert.Equal(Calculos.CabezaRegadera, Calculos.RadioEnsayo(motor, Ambito, 1));
    }

    [Fact]
    public void C07_SinDimensionesElRadioNoAplica()
    {
        var motor = Contexto.Motor(Contexto.ProyectoVacio());
        Assert.Equal(Calculos.NoAplica, Calculos.RadioEnsayo(motor, Ambito, 1));
    }

    [Fact]
    public void C07_ElBloqueDeIpLeeElTamanoDeclaradoEnGenerales()
    {
        // El campo "tamano" de IP es derivado de "generales.tamano" (D-02 del libro:
        // la hoja copiaba las dimensiones). El motor debe resolver la redirección.
        var motor = ConTamano(alto: 120, ancho: 60, profundo: 80, "IPX3");
        Assert.Equal(150, Calculos.RadioMaximo(motor, "11", 1), 6);
    }

    [Theory]
    [InlineData(AlturaMontaje.HastaOchoMetros, 45)]
    [InlineData(AlturaMontaje.EntreOchoYQuinceMetros, 52)]
    [InlineData(AlturaMontaje.MasDeQuinceMetros, 57)]
    public void C01_VelocidadDeVientoSegunAlturaDeMontaje(AlturaMontaje altura, double esperada)
        => Assert.Equal(esperada, Calculos.VelocidadViento(altura));

    [Fact]
    public void C02_FuerzaDeVientoParaLaParte23()
        // 0,5 · 1,225 · 2 m² · 1,2 · 45² = 2976,75 N
        => Assert.Equal(2976.75, Calculos.FuerzaViento(areaM2: 2, AlturaMontaje.HastaOchoMetros), 2);

    [Fact]
    public void C03_LaParte25UsaUnCriterioDistinto()
        // Criterio distinto al de -2-3 (D-13, pendiente de confirmar con la norma)
        => Assert.Equal(4800, Calculos.FuerzaCargaEstatica25(areaM2: 2), 6);
}
