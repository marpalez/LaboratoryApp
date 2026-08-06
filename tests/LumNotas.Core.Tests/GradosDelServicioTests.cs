using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// El IP y el IK del servicio entero, sacados de los de sus muestras. Es lo que enseña
/// una sola celda del listado cuando el servicio trae varias luminarias.
/// </summary>
public class GradosDelServicioTests
{
    private static DatosProyecto Con(params (string Primera, string Segunda)[] muestras)
    {
        var datos = new DatosProyecto { NumeroMuestras = muestras.Length };

        for (var i = 0; i < muestras.Length; i++)
        {
            datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoPrimeraCifra,
                             muestras[i].Primera, i + 1);
            datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoSegundaCifra,
                             muestras[i].Segunda, i + 1);
        }

        return datos;
    }

    // ---- IP ----------------------------------------------------------------

    [Fact]
    public void ConUnaSolaMuestraEsElSuyo()
        => Assert.Equal("IP65", GradosDelServicio.IpMaximo(Con(("IP6X", "IPX5"))));

    /// <summary>
    /// <b>La regla del laboratorio: manda la segunda cifra.</b> IP28 es mayor que IP54
    /// aunque su primera cifra sea menor. No es un orden físico —protegerse del polvo y
    /// del agua no se comparan—, es el criterio con el que el laboratorio ordena.
    /// </summary>
    [Fact]
    public void MandaLaSegundaCifra()
        => Assert.Equal("IP28", GradosDelServicio.IpMaximo(Con(("IP5X", "IPX4"), ("IP2X", "IPX8"))));

    /// <summary>Con la segunda igualada, desempata la primera.</summary>
    [Fact]
    public void ConLaSegundaIgualDesempataLaPrimera()
        => Assert.Equal("IP54", GradosDelServicio.IpMaximo(Con(("IP2X", "IPX4"), ("IP5X", "IPX4"))));

    /// <summary>La «X» no es una cifra: significa «sin declarar», y ordena como 0.</summary>
    [Fact]
    public void LaEquisCuentaComoCero()
    {
        Assert.Equal("IP07", GradosDelServicio.IpMaximo(Con(("", "IPX7"))));
        Assert.Equal("IP60", GradosDelServicio.IpMaximo(Con(("IP6X", ""))));
    }

    /// <summary>Sin ninguna muestra rellena no se inventa un grado: se queda en blanco.</summary>
    [Fact]
    public void SinGradosNoSeInventaNada()
    {
        Assert.Equal("", GradosDelServicio.IpMaximo(Con(("", ""))));
        Assert.Equal("", GradosDelServicio.IpMaximo(new DatosProyecto { NumeroMuestras = 0 }));
    }

    /// <summary>Una muestra a medio rellenar no arrastra a las que sí lo están.</summary>
    [Fact]
    public void UnaMuestraVaciaNoBajaElMaximo()
        => Assert.Equal("IP65", GradosDelServicio.IpMaximo(Con(("IP6X", "IPX5"), ("", ""))));

    /// <summary>«Luminaria ordinaria» es el atajo del laboratorio para IP20.</summary>
    [Fact]
    public void LaLuminariaOrdinariaEsIp20()
    {
        var datos = new DatosProyecto { NumeroMuestras = 1 };
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoOrdinaria, true, 1);

        Assert.Equal("IP20", GradosDelServicio.IpMaximo(datos));
    }

    // ---- IK ----------------------------------------------------------------

    [Fact]
    public void ElIkMayorGana()
    {
        var datos = new DatosProyecto { NumeroMuestras = 3 };
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, "IK07", 1);
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, "IK10", 2);
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, "IK02", 3);

        Assert.Equal("IK10", GradosDelServicio.IkMaximo(datos));
    }

    /// <summary>
    /// <c>No IK</c> no es un grado bajo: es no haber ensayo de impacto. Contarlo como
    /// cero haría que un servicio sin IK apareciera como «IK00», que no existe.
    /// </summary>
    [Fact]
    public void SinIkNoEsUnGradoBajo()
    {
        var datos = new DatosProyecto { NumeroMuestras = 2 };
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, GradosDelServicio.SinIk, 1);
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, GradosDelServicio.SinIk, 2);

        Assert.Equal("", GradosDelServicio.IkMaximo(datos));
    }

    /// <summary>Con una sola muestra ensayada, esa manda aunque las demás no lleven IK.</summary>
    [Fact]
    public void UnaMuestraConIkBastaParaQueElServicioLoTenga()
    {
        var datos = new DatosProyecto { NumeroMuestras = 2 };
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, GradosDelServicio.SinIk, 1);
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, "IK08", 2);

        Assert.Equal("IK08", GradosDelServicio.IkMaximo(datos));
    }

    /// <summary>Se escribe con dos cifras, como en la norma: IK08, no IK8.</summary>
    [Fact]
    public void ElIkSeEscribeConDosCifras()
    {
        var datos = new DatosProyecto { NumeroMuestras = 1 };
        datos.Establecer(DatosProyecto.Cabecera, GradosDelServicio.CampoIk, "IK08", 1);

        Assert.Equal("IK08", GradosDelServicio.IkMaximo(datos));
    }
}
