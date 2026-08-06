using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;

namespace LumNotas.Core.Tests;

/// <summary>
/// Un apartado vacío y otro a medias tienen el mismo problema para la plantilla —le
/// faltan datos a los dos— pero no para quien tiene que terminar el ensayo. Estos tests
/// fijan esa diferencia (DD-124) y, sobre todo, que <b>no</b> cambia lo que el tablero
/// llama pendiente: a medias no está hecho.
/// </summary>
public class EstadoDeApartadoTests
{
    // 7.9 «Revestimientos y manguitos aislantes»: tiene condiciones de ensayo, equipos y
    // dos duraciones, así que escribir un dato suelto no lo termina ni por casualidad.
    private const string Apartado = "7.9";

    // 7.12 «Tornillos» no tiene campos propios: todo lo suyo vive en sus subapartados.
    private const string ConSubapartados = "7.12";
    private const string Subapartado = "7.12.1";

    private static EstadoApartado Estado(DatosProyecto datos, string bloque)
        => EstadoDeApartado.De(Contexto.Motor(datos), datos, Contexto.Plantilla.Bloque(bloque));

    [Fact]
    public void UnApartadoSinTocarEstaSinEmpezar()
    {
        var datos = Contexto.ProyectoVacio();

        Assert.Equal(EstadoApartado.FaltanDatos, Estado(datos, Apartado));
    }

    [Fact]
    public void EnCuantoHayAlgoEscritoEstaEmpezado()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer(Apartado, "ambiente.temperatura", 25d);

        Assert.Equal(EstadoApartado.Empezado, Estado(datos, Apartado));
    }

    [Fact]
    public void UnaCasillaMarcadaTambienEsHaberEmpezado()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar(Apartado, "equipoAcondicionamiento", "eqSafe305");

        Assert.Equal(EstadoApartado.Empezado, Estado(datos, Apartado));
    }

    /// <summary>
    /// Lo escrito en un subapartado cuenta para el apartado que lo contiene. Es el caso
    /// normal, no el raro: la sección 12 y la de calentamiento guardan <b>todo</b> bajo
    /// los ids de sus subapartados, así que mirar solo el del padre los dejaría siempre
    /// pareciendo sin empezar.
    /// </summary>
    [Fact]
    public void LoEscritoEnUnSubapartadoCuentaParaElPadre()
    {
        var datos = Contexto.ProyectoVacio();
        Assert.Equal(EstadoApartado.FaltanDatos, Estado(datos, ConSubapartados));

        datos.Establecer(Subapartado, "ambiente.temperatura", 25d);

        Assert.Equal(EstadoApartado.Empezado, Estado(datos, ConSubapartados));
    }

    [Fact]
    public void MarcarNoAplicaManda()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer(Apartado, "ambiente.temperatura", 25d);
        datos.EstablecerNa($"{Apartado}/na", true);

        Assert.Equal(EstadoApartado.NoAplica, Estado(datos, Apartado));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(false)]
    public void LoQueNoDiceNadaNoEsHaberEmpezado(object? valor)
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer(Apartado, "ambiente.temperatura", valor);

        Assert.Equal(EstadoApartado.FaltanDatos, Estado(datos, Apartado));
    }

    /// <summary>
    /// Un <c>false</c> guardado no es haber contestado: en el almacén, «sin marcar» y
    /// «marcado que no» se guardan igual, así que darlo por escrito dejaría medio
    /// proyecto en ámbar sin que nadie hubiera tocado nada.
    /// </summary>
    [Fact]
    public void UnaCasillaDesmarcadaTampocoCuenta()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Marcar(Apartado, "equipoAcondicionamiento", "eqSafe305", false);

        Assert.False(datos.HayAlgoEn(Apartado));
    }

    /// <summary>
    /// Un apartado no se lleva lo escrito en otro que empieza igual. Con los códigos de
    /// la norma esto no es rebuscado: «7.1» y «7.18.1» conviven en la misma sección.
    /// </summary>
    [Fact]
    public void UnApartadoNoSeLlevaLoDelQueEmpiezaIgual()
    {
        var datos = Contexto.ProyectoVacio();
        datos.Establecer("7.18.1", "ambiente.temperatura", 25d);
        datos.Marcar("7.18.1", "lista", "opcion");

        Assert.False(datos.HayAlgoEn("7.18"));
        Assert.False(datos.HayAlgoEn("7.1"));
        Assert.True(datos.HayAlgoEn("7.18.1"));
    }

    private const string Ik = "62262.generales";

    private static EstadoApartado EstadoIk(DatosProyecto datos)
    {
        var norma = Contexto.Norma("62262");
        return EstadoDeApartado.De(new MotorDeReglas(norma, datos), datos, norma.Bloque(Ik));
    }

    private static void Medir(DatosProyecto datos, int muestra, params string[] campos)
    {
        foreach (var campo in campos) datos.Establecer(Ik, $"tamano[0].{campo}", 10d, muestra);
    }

    /// <summary>
    /// Un apartado relleno del todo se pone verde <b>con las muestras que sean</b>.
    /// <para>
    /// Con dos no lo hacía: <c>umbralPorMuestra</c> multiplicaba el listón por el número
    /// de muestras pero el recuento solo miraba la primera, así que crecía lo pedido sin
    /// crecer lo contado. Con tres campos por muestra y dos muestras hacían falta seis
    /// datos de una columna que solo tiene cuatro: no se podía terminar.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void UnApartadoRellenoSeCompletaConLasMuestrasQueSea(int muestras)
    {
        var datos = Contexto.ProyectoVacio(muestras);
        foreach (var muestra in datos.Muestras) Medir(datos, muestra, "alto", "ancho", "profundo");

        Assert.Equal(EstadoApartado.Completo, EstadoIk(datos));
    }

    /// <summary>
    /// <b>El listón es por muestra, no un total.</b> Una muestra medida de sobra no puede
    /// tapar a otra sin medir: la regla dice «de cada muestra».
    /// <para>
    /// Este es el caso que se coló al arreglar lo de arriba sumando: cuatro medidas de la
    /// primera muestra y dos de la segunda son seis, que es el total que se pedía con dos
    /// muestras, y el apartado se ponía verde con media muestra sin medir.
    /// </para>
    /// </summary>
    [Fact]
    public void LoQueSobraEnUnaMuestraNoTapaLoQueFaltaEnOtra()
    {
        var datos = Contexto.ProyectoVacio(muestras: 2);
        Medir(datos, 1, "alto", "ancho", "profundo", "peso");
        Medir(datos, 2, "alto", "ancho");

        Assert.Equal(EstadoApartado.Empezado, EstadoIk(datos));
    }

    [Fact]
    public void ConUnaMuestraSinMedirElApartadoNoSeDaPorTerminado()
    {
        var datos = Contexto.ProyectoVacio(muestras: 2);
        Medir(datos, 1, "alto", "ancho", "profundo", "peso");

        Assert.Equal(EstadoApartado.Empezado, EstadoIk(datos));
    }

    /// <summary>
    /// La última medida que falta es la que lo termina. Sin esto, un test podría pasar
    /// por casualidad con una regla que no mirara nada.
    /// </summary>
    [Fact]
    public void AlMedirLaUltimaMuestraElApartadoSeTermina()
    {
        var datos = Contexto.ProyectoVacio(muestras: 2);
        Medir(datos, 1, "alto", "ancho", "profundo");
        Medir(datos, 2, "alto", "ancho");
        Assert.Equal(EstadoApartado.Empezado, EstadoIk(datos));

        Medir(datos, 2, "profundo");

        Assert.Equal(EstadoApartado.Completo, EstadoIk(datos));
    }

    [Fact]
    public void EmpezadoSigueSiendoTrabajoPendiente()
    {
        Assert.True(EstadoDeApartado.EstaPendiente(EstadoApartado.Empezado));
        Assert.True(EstadoDeApartado.EstaPendiente(EstadoApartado.FaltanDatos));
        Assert.False(EstadoDeApartado.EstaPendiente(EstadoApartado.Completo));
        Assert.False(EstadoDeApartado.EstaPendiente(EstadoApartado.NoAplica));
        Assert.False(EstadoDeApartado.EstaPendiente(EstadoApartado.SinReglas));
    }

    /// <summary>
    /// El tablero no cambia. Empezar un apartado da color en el índice, pero no descuenta
    /// nada del trabajo que queda: si esto se rompiera, un proyecto a medias se anunciaría
    /// como más terminado de lo que está.
    /// </summary>
    [Fact]
    public void EmpezarUnApartadoNoLoDescuentaDelTablero()
    {
        var datos = Contexto.ProyectoVacio();
        var antes = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x", DateTime.Now)
            .SeccionesPendientes.Single(s => s.Titulo.StartsWith("Sección 7"));

        datos.Establecer(Apartado, "ambiente.temperatura", 25d);
        var despues = AnalizadorDeProyectos.Analizar(Contexto.Plantilla, datos, "x", DateTime.Now)
            .SeccionesPendientes.Single(s => s.Titulo.StartsWith("Sección 7"));

        Assert.Equal(EstadoApartado.Empezado, Estado(datos, Apartado));
        Assert.Equal(antes.Pendientes, despues.Pendientes);
    }
}
