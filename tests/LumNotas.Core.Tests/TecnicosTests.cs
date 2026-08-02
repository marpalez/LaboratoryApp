using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Storage;

namespace LumNotas.Core.Tests;

/// <summary>
/// La lista de técnicos del laboratorio y su efecto sobre los proyectos ya guardados.
/// <para>
/// La regla que se vigila aquí es la que pidió el laboratorio: <b>quitar</b> a un técnico
/// no toca ningún proyecto —el ensayo lo hizo esa persona—, pero <b>corregir</b> su
/// nombre sí, porque una errata no es una persona distinta.
/// </para>
/// </summary>
public class TecnicosTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "lumnotas-tec-" + Guid.NewGuid().ToString("N"));
    private readonly RepositorioDeProyectos _repositorio = new();

    public TecnicosTests() => Directory.CreateDirectory(_carpeta);

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
        GC.SuppressFinalize(this);
    }

    private string Guardar(string nombre, string? tecnico1, string? tecnico2 = null)
    {
        var datos = new DatosProyecto { CodigoServicio = nombre, NumeroMuestras = 1 };
        if (tecnico1 is not null) datos.Establecer("proyecto", "tecnico1", tecnico1);
        if (tecnico2 is not null) datos.Establecer("proyecto", "tecnico2", tecnico2);

        var ruta = Path.Combine(_carpeta, nombre + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0");
        return ruta;
    }

    private static string? Tecnico(DatosProyecto datos, string campo)
        => datos.Obtener("proyecto", campo) as string;

    // ---- el catálogo -------------------------------------------------------

    [Fact]
    public void LaListaDePartidaSonLosSeisTecnicosDelLaboratorio()
    {
        var catalogo = CatalogoDeTecnicos.DePartida();

        Assert.Equal(6, catalogo.Tecnicos.Count);
        Assert.Contains("Daniel Martínez", catalogo.Tecnicos);
        Assert.Contains("Raúl González", catalogo.Tecnicos);

        // Ordenada, que es como se busca un nombre en un desplegable.
        Assert.Equal(catalogo.Tecnicos.OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase), catalogo.Tecnicos);
    }

    [Fact]
    public void LaListaSobreviveAlGuardadoYALaLectura()
    {
        var catalogo = CatalogoDeTecnicos.DePartida();
        catalogo.Anadir("Ana Bellés");
        catalogo.Quitar("Javier Ibor");
        catalogo.Guardar(_carpeta);

        var leida = CatalogoDeTecnicos.Cargar(_carpeta);

        Assert.Contains("Ana Bellés", leida.Tecnicos);
        Assert.DoesNotContain("Javier Ibor", leida.Tecnicos);
    }

    [Fact]
    public void SinFicheroTodaviaSeOfreceLaListaDePartida()
        => Assert.Equal(CatalogoDeTecnicos.Iniciales, CatalogoDeTecnicos.Cargar(_carpeta).Tecnicos);

    /// <summary>Un fichero roto no puede dejar al laboratorio sin poder elegir técnico.</summary>
    [Fact]
    public void UnFicheroCorruptoNoDejaLaListaVacia()
    {
        File.WriteAllText(Path.Combine(_carpeta, CatalogoDeTecnicos.NombreDeFichero), "{ esto no es json");

        Assert.NotEmpty(CatalogoDeTecnicos.Cargar(_carpeta).Tecnicos);
    }

    [Fact]
    public void NoSeAdmitenNombresRepetidosNiVacios()
    {
        var catalogo = CatalogoDeTecnicos.DePartida();

        Assert.False(catalogo.Anadir("  "));
        Assert.False(catalogo.Anadir("Javier Ibor"));
        Assert.False(catalogo.Anadir("javier ibor"));      // el mismo con otra caja
        Assert.True(catalogo.Anadir("  Ana Bellés  "));    // se recorta y entra
        Assert.Contains("Ana Bellés", catalogo.Tecnicos);
    }

    /// <summary>
    /// Un proyecto guardado antes de existir la lista lleva el técnico escrito a mano. El
    /// desplegable tiene que seguir enseñándolo o el proyecto se quedaría sin técnico.
    /// </summary>
    [Fact]
    public void UnNombreQueNoEstaEnLaListaSeSigueOfreciendo()
    {
        var catalogo = CatalogoDeTecnicos.DePartida();

        var ofrecidos = catalogo.ConNombreSuelto("D. Martínez", null);

        Assert.Contains("D. Martínez", ofrecidos);
        Assert.Equal(catalogo.Tecnicos.Count + 1, ofrecidos.Count);

        // Y el que sí está no se duplica.
        Assert.Equal(catalogo.Tecnicos.Count, catalogo.ConNombreSuelto("Javier Ibor").Count);
    }

    // ---- efecto sobre los proyectos ---------------------------------------

    [Fact]
    public void CorregirElNombreLoCambiaEnLosProyectosQueLoLlevan()
    {
        var suyo = Guardar("111112026", "Javer Ibor");
        var comoSegundo = Guardar("222222026", "Mario Madrigal", "Javer Ibor");
        var ajeno = Guardar("333332026", "Mario Madrigal");

        var cambiados = _repositorio.RenombrarTecnicoEnLaCarpeta(_carpeta, "Javer Ibor", "Javier Ibor");

        Assert.Equal(2, cambiados);
        Assert.Equal("Javier Ibor", Tecnico(_repositorio.Cargar(suyo), "tecnico1"));
        Assert.Equal("Javier Ibor", Tecnico(_repositorio.Cargar(comoSegundo), "tecnico2"));
        Assert.Equal("Mario Madrigal", Tecnico(_repositorio.Cargar(comoSegundo), "tecnico1"));
        Assert.Equal("Mario Madrigal", Tecnico(_repositorio.Cargar(ajeno), "tecnico1"));
    }

    /// <summary>Corregir un nombre no puede tocar ni un dato de ensayo.</summary>
    [Fact]
    public void CorregirElNombreNoTocaNadaMas()
    {
        var datos = new DatosProyecto { CodigoServicio = "111112026", NumeroMuestras = 3 };
        datos.Establecer("proyecto", "tecnico1", "Javer Ibor");
        datos.Establecer("6", "ambiente.fecha", new DateTime(2026, 7, 20));
        datos.Marcar("7", "sujecion", "tornillo");
        datos.CargarNa("11.2", true);

        var ruta = Path.Combine(_carpeta, "111112026" + RepositorioDeProyectos.Extension);
        _repositorio.Guardar(datos, ruta, "1.0.0");
        _repositorio.ActualizarPlanificacion(ruta, new Planificacion { Inicio = new DateTime(2026, 8, 10) });

        _repositorio.RenombrarTecnico(ruta, "Javer Ibor", "Javier Ibor");

        var releido = _repositorio.Cargar(ruta);
        Assert.Equal("Javier Ibor", Tecnico(releido, "tecnico1"));
        Assert.Equal(3, releido.NumeroMuestras);
        Assert.Equal(new DateTime(2026, 7, 20), releido.Obtener("6", "ambiente.fecha"));
        Assert.True(releido.Marcada("7", "sujecion", "tornillo"));
        Assert.True(releido.Na("11.2"));
        Assert.Equal(new DateTime(2026, 8, 10), _repositorio.LeerPlanificacion(ruta).Inicio);
    }

    /// <summary>
    /// <b>Quitar a un técnico no toca los proyectos.</b> Decisión del laboratorio: el
    /// ensayo lo hizo esa persona, aunque ya no esté en la lista.
    /// </summary>
    [Fact]
    public void QuitarUnTecnicoNoCambiaNingunProyecto()
    {
        var ruta = Guardar("111112026", "Javier Ibor");

        var catalogo = CatalogoDeTecnicos.DePartida();
        catalogo.Quitar("Javier Ibor");
        catalogo.Guardar(_carpeta);

        Assert.DoesNotContain("Javier Ibor", CatalogoDeTecnicos.Cargar(_carpeta).Tecnicos);
        Assert.Equal("Javier Ibor", Tecnico(_repositorio.Cargar(ruta), "tecnico1"));

        // Y ese proyecto sigue ofreciendo su nombre en el desplegable.
        Assert.Contains("Javier Ibor", CatalogoDeTecnicos.Cargar(_carpeta).ConNombreSuelto("Javier Ibor"));
    }

    // ---- el técnico es del proyecto, no de la norma ------------------------

    /// <summary>
    /// El responsable se pide ya por <see cref="DatosProyecto.Tecnico1"/> y no buscando su
    /// clave en la cabecera. <b>Sigue guardándose en el mismo sitio</b>: los proyectos
    /// escritos antes de existir la propiedad —como los que crea <c>Guardar</c> aquí, a la
    /// vieja usanza— tienen que leerse igual, antes y después de pasar por el disco.
    /// </summary>
    [Fact]
    public void ElResponsableSeLeeIgualEnLosProyectosYaGuardados()
    {
        var ruta = Guardar("111112026", "Javier Ibor", "Mario Madrigal");

        var releido = _repositorio.Cargar(ruta);

        Assert.Equal("Javier Ibor", releido.Tecnico1);
        Assert.Equal("Mario Madrigal", releido.Tecnico2);
    }

    /// <summary>
    /// Y al revés: escribirlo por la propiedad lo deja donde la plantilla lo declara, que
    /// es de donde lo sacan el informe y las reglas de la norma. Si dejara de coincidir,
    /// el técnico desaparecería del informe sin que nadie se enterase.
    /// </summary>
    [Fact]
    public void EscribirElResponsableLoDejaDondeLaPlantillaLoDeclara()
    {
        var datos = new DatosProyecto { CodigoServicio = "111112026" };
        datos.Tecnico1 = "Javier Ibor";

        Assert.Equal("Javier Ibor", datos.Obtener(DatosProyecto.Cabecera, DatosProyecto.CampoTecnico1));

        // Y la clave es la misma que declaran las cuatro plantillas en su cabecera.
        foreach (var plantilla in Contexto.TodasLasPlantillas())
            Assert.Contains(plantilla.Proyecto.Campos, c => c.Id == DatosProyecto.CampoTecnico1);
    }

    /// <summary>
    /// <b>Un proyecto tiene un responsable, no uno por norma.</b> Es la razón de que el
    /// dato sea del proyecto: un servicio de luminarias con módulos LED no lo hacen dos
    /// personas distintas por llevar dos tomas de notas.
    /// </summary>
    [Fact]
    public void ElResponsableEsUnoAunqueElProyectoLleveVariasNormas()
    {
        var datos = new DatosProyecto { CodigoServicio = "111112026", NumeroMuestras = 1 };
        datos.Tecnico1 = "Javier Ibor";

        var normas = Contexto.TodasLasPlantillas().ToList();
        foreach (var norma in normas) datos.Normas.Add(norma.Meta.Id);

        // Se mire por donde se mire —cambiando cuál va primera— el responsable es el mismo.
        foreach (var orden in new[] { normas, Enumerable.Reverse(normas).ToList() })
            Assert.Equal("Javier Ibor",
                AnalizadorDeProyectos.Analizar(orden, datos, "x.lumproj", DateTime.Now).Tecnico);
    }

    [Fact]
    public void UnProyectoCorruptoNoDetieneLaCorreccionDeLosDemas()
    {
        Guardar("111112026", "Javer Ibor");
        File.WriteAllText(Path.Combine(_carpeta, "roto" + RepositorioDeProyectos.Extension), "{ no es json");
        Guardar("222222026", "Javer Ibor");

        Assert.Equal(2, _repositorio.RenombrarTecnicoEnLaCarpeta(_carpeta, "Javer Ibor", "Javier Ibor"));
    }

    [Fact]
    public void CorregirEnLaListaNoAdmiteChocarConOtroTecnico()
    {
        var catalogo = CatalogoDeTecnicos.DePartida();

        Assert.False(catalogo.Renombrar("Javier Ibor", "Mario Madrigal"));
        Assert.True(catalogo.Renombrar("Javier Ibor", "Javier Ibor Gil"));
        Assert.Contains("Javier Ibor Gil", catalogo.Tecnicos);
        Assert.DoesNotContain("Javier Ibor", catalogo.Tecnicos);
    }
}
