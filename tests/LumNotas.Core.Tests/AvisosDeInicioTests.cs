using LumNotas.Core.Gestion;

namespace LumNotas.Core.Tests;

/// <summary>
/// Lo que el equipo tiene mal configurado, dicho en la portada.
/// <para>
/// Casi todo esto <b>fallaba en silencio</b>: sin carpeta de proyectos las tres vistas de
/// gestión salen vacías —indistinguible de no tener trabajo— y sin carpeta compartida
/// cada equipo usa sus normas y sus técnicos sin que nadie lo diga.
/// </para>
/// </summary>
public class AvisosDeInicioTests
{
    /// <summary>Un equipo bien configurado: nada que decir, y el recuadro no existe.</summary>
    private static AvisosDeInicio.Estado Correcto => new(
        CarpetaDeProyectos: @"C:\OneDrive\clientes",
        ProyectosAccesible: true,
        CarpetaCompartida: @"C:\OneDrive\compartida",
        CompartidaAccesible: true,
        HayNormasPublicadas: true);

    [Fact]
    public void SinNadaQueHacerNoHayAvisos()
        => Assert.Empty(AvisosDeInicio.Revisar(Correcto));

    // ---- carpeta de proyectos ----------------------------------------------

    [Fact]
    public void SinCarpetaDeProyectosSeAvisaComoProblema()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { CarpetaDeProyectos = "" }));

        Assert.Equal(NivelDeAviso.Problema, aviso.Nivel);
        Assert.Contains("carpeta de proyectos", aviso.Texto);
        Assert.Equal(AccionDeAviso.ElegirCarpetas, aviso.Accion);
    }

    /// <summary>OneDrive sin sincronizar o una unidad que ya no está: el escaneo devolvía
    /// cero proyectos en silencio, indistinguible de no tener ninguno.</summary>
    [Fact]
    public void UnaCarpetaDeProyectosInalcanzableEsProblemaYDiceLaRuta()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { ProyectosAccesible = false }));

        Assert.Equal(NivelDeAviso.Problema, aviso.Nivel);
        Assert.Equal(@"C:\OneDrive\clientes", aviso.Detalle);
    }

    [Fact]
    public void ElijaOFalleLaDeProyectosSoloSaleUnAviso()
        => Assert.Single(AvisosDeInicio.Revisar(
            Correcto with { CarpetaDeProyectos = "", ProyectosAccesible = false }));

    // ---- carpeta compartida -------------------------------------------------

    [Fact]
    public void SinCarpetaCompartidaSeAvisaQueNadieVeLoDeEsteEquipo()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { CarpetaCompartida = "" }));

        Assert.Equal(NivelDeAviso.Atencion, aviso.Nivel);
        Assert.Contains("no los ve nadie más", aviso.Detalle);
    }

    [Fact]
    public void UnaCompartidaInalcanzableEsProblema()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { CompartidaAccesible = false }));

        Assert.Equal(NivelDeAviso.Problema, aviso.Nivel);
        Assert.Equal(@"C:\OneDrive\compartida", aviso.Detalle);
    }

    /// <summary>
    /// El caso que se escapaba: carpeta compartida recién creada y vacía. No falta, no es
    /// inalcanzable — y sin embargo <b>nada está compartido</b>.
    /// </summary>
    [Fact]
    public void UnaCompartidaSinNormasPublicadasSeAvisa()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { HayNormasPublicadas = false }));

        Assert.Contains("todavía no están publicadas", aviso.Texto);
        Assert.Equal(AccionDeAviso.VerNormas, aviso.Accion);
    }

    /// <summary>Los tres estados de la carpeta compartida son excluyentes.</summary>
    [Fact]
    public void DeLaCompartidaSoloSaleUnAviso()
        => Assert.Single(AvisosDeInicio.Revisar(
            Correcto with { CarpetaCompartida = "", CompartidaAccesible = false, HayNormasPublicadas = false }));

    // ---- normas -------------------------------------------------------------

    [Fact]
    public void LasNormasSinPublicarSeCuentanYSeNombran()
    {
        var avisos = AvisosDeInicio.Revisar(
            Correcto with { NormasSinPublicar = ["Luminarias — EN 60598-1:2021"] });

        var aviso = Assert.Single(avisos);
        Assert.Contains("una norma", aviso.Texto);
        Assert.Contains("60598-1:2021", aviso.Detalle);
    }

    /// <summary>
    /// «Falta una norma» y «este equipo tiene una más nueva» no son el mismo problema: el
    /// segundo es exactamente lo que la carpeta compartida existe para evitar, así que va
    /// en su propia línea.
    /// </summary>
    [Fact]
    public void FaltarUnaNormaYTenerlaMasNuevaSonAvisosDistintos()
    {
        var avisos = AvisosDeInicio.Revisar(Correcto with
        {
            NormasSinPublicar = ["Grados IK"],
            NormasMasNuevas = ["Luminarias", "Grados IP"]
        });

        Assert.Equal(2, avisos.Count);
        Assert.Contains(avisos, a => a.Texto.Contains("que el laboratorio no tiene"));
        Assert.Contains(avisos, a => a.Texto.Contains("2 normas") && a.Texto.Contains("más nuevas"));
    }

    // ---- proyectos ilegibles ------------------------------------------------

    [Fact]
    public void LosFicherosIlegiblesSeCuentanYLlevanAlTablero()
    {
        var aviso = Assert.Single(AvisosDeInicio.Revisar(Correcto with { ProyectosIlegibles = 2 }));

        Assert.Contains("2 tomas de notas no se pudieron leer", aviso.Texto);
        Assert.Equal(AccionDeAviso.IrAlTablero, aviso.Accion);
    }

    // ---- orden --------------------------------------------------------------

    /// <summary>Lo que no funciona, antes de lo que solo está descuadrado.</summary>
    [Fact]
    public void LosProblemasVanAntesQueLasAtenciones()
    {
        var avisos = AvisosDeInicio.Revisar(Correcto with
        {
            CarpetaDeProyectos = "",
            NormasSinPublicar = ["Grados IK"]
        });

        Assert.Equal(2, avisos.Count);
        Assert.Equal(NivelDeAviso.Problema, avisos[0].Nivel);
        Assert.Equal(NivelDeAviso.Atencion, avisos[1].Nivel);
    }

    /// <summary>
    /// <b>Salen todos a la vez, no de uno en uno.</b> Nada los limita ni los va soltando
    /// según se resuelven: quien tiene el equipo a medio configurar necesita ver la lista
    /// entera para arreglarlo de una sentada, no descubrir el siguiente al arreglar uno.
    /// </summary>
    [Fact]
    public void TodoLoQueEsteMalSaleALaVez()
    {
        var avisos = AvisosDeInicio.Revisar(new AvisosDeInicio.Estado(
            CarpetaDeProyectos: "",
            CarpetaCompartida: "",
            NormasSinPublicar: ["Grados IK"],
            NormasMasNuevas: ["Luminarias"],
            ProyectosIlegibles: 3));

        Assert.Equal(5, avisos.Count);

        // Y siguen ordenados: lo que no funciona, primero.
        Assert.Equal(NivelDeAviso.Problema, avisos[0].Nivel);
        Assert.All(avisos.Skip(1), a => Assert.Equal(NivelDeAviso.Atencion, a.Nivel));
    }

    /// <summary>Un equipo recién instalado: nada configurado, y se dice todo de una vez.</summary>
    [Fact]
    public void UnEquipoRecienInstaladoLoDiceTodo()
    {
        var avisos = AvisosDeInicio.Revisar(new AvisosDeInicio.Estado());

        Assert.Equal(2, avisos.Count);
        Assert.Equal(NivelDeAviso.Problema, avisos[0].Nivel);   // sin carpeta de proyectos
        Assert.Equal(NivelDeAviso.Atencion, avisos[1].Nivel);   // sin carpeta compartida
        Assert.All(avisos, a => Assert.False(string.IsNullOrWhiteSpace(a.Boton)));
    }
}
