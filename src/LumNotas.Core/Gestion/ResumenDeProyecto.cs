using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Gestion;

/// <summary>Una sección con trabajo pendiente, para el tablero de gestión.</summary>
public sealed record SeccionPendiente(string Titulo, int Pendientes, int Aplicables);

/// <summary>
/// Estado de un proyecto visto desde fuera, sin abrirlo. Es lo que alimenta el tablero:
/// una columna por proyecto y una tarjeta por sección pendiente.
/// </summary>
public sealed record ResumenDeProyecto
{
    public required string Ruta { get; init; }
    public required string Nombre { get; init; }

    /// <summary>
    /// El de esta toma de notas. Es lo que distingue dos familias del mismo servicio, así
    /// que es con lo que se busca un duplicado de verdad — por el de servicio saldrían
    /// repetidas las cuatro familias de un trabajo, que es lo normal y no un error.
    /// </summary>
    public string CodigoTomaDeNotas { get; init; } = "";

    public string CodigoServicio { get; init; } = "";

    /// <summary>El responsable, por el que se filtra.</summary>
    public string Tecnico { get; init; } = "";

    /// <summary>El segundo, si lo hay. Solo se enseña; no filtra ni ordena nada.</summary>
    public string Tecnico2 { get; init; } = "";

    public int NumeroMuestras { get; init; }
    public DateTime Modificado { get; init; }

    // ---- lo que hace falta para buscar un servicio de hace meses -------------
    //
    // Sube aquí y no se relee del fichero a propósito: el listado recorre cientos de
    // proyectos, y el escaneo ya los ha abierto todos una vez.

    /// <summary>ENAC, ENEC, CB… o «Sin acreditar». Un servicio puede llevar varias.</summary>
    public IReadOnlyList<string> Acreditaciones { get; init; } = [];

    /// <summary>Laboratorios de fuera que participaron. Lo normal es ninguno.</summary>
    public IReadOnlyList<string> Colaboradores { get; init; } = [];

    /// <summary>El IP mayor de sus muestras, como <c>IP54</c>. Vacío si no lleva.</summary>
    public string GradoIp { get; init; } = "";

    /// <summary>El IK mayor de sus muestras, como <c>IK08</c>. Vacío si no lleva.</summary>
    public string GradoIk { get; init; } = "";

    /// <summary>
    /// La norma con la que nació, **ya legible** —«EN IEC 60598‑1:2024 + A11:2024»— y no
    /// su id. En una columna, el id no le dice nada a nadie; y guardarlo resuelto aquí
    /// evita que el listado tenga que cargar las plantillas para traducirlo.
    /// </summary>
    public string NormaPrincipal { get; init; } = "";

    /// <summary>Normas que lleva el servicio, para poder filtrar el calendario por ellas.</summary>
    public IReadOnlyList<string> Normas { get; init; } = [];

    /// <summary>Fechas y estado del servicio. Vacía en los proyectos aún sin planificar.</summary>
    public Planificacion Planificacion { get; init; } = new();

    public IReadOnlyList<SeccionPendiente> SeccionesPendientes { get; init; } = [];

    /// <summary>
    /// El avance se cuenta <b>por secciones</b>, no por apartados: la sección 7 cuenta
    /// como una aunque tenga trece apartados dentro. Es la vista que necesita el PM.
    /// </summary>
    public int SeccionesCompletadas { get; init; }
    public int SeccionesAplicables { get; init; }

    /// <summary>
    /// El porcentaje <b>ponderado</b>, redondeado. Es <b>el mismo número que estampa el
    /// informe</b>, y por eso es este y no otro: el laboratorio no puede tener dos «% del
    /// proyecto» que digan cosas distintas según dónde se mire.
    /// <para>
    /// Sale de los pesos que declara la plantilla —3, 5 y 10, heredados del Excel—, así que
    /// mide <b>esfuerzo declarado</b> y no apartados contados. Contar apartados haría que
    /// «Calentamiento y endurancia», que son días de cámara, valiera lo mismo que un
    /// marcado que se mira en un minuto.
    /// </para>
    /// <para>
    /// <b>Es nulo, no cero, cuando la norma no declara pesos.</b> Un cero sería una mentira
    /// fija: un servicio terminado seguiría enseñando «0 %» para siempre. Sin peso no hay
    /// número, igual que sin fechas no hay duración.
    /// </para>
    /// </summary>
    public int? PorcentajePonderado { get; init; }

    /// <summary>
    /// Cómo se encabeza este servicio en el tablero y en el calendario: <c>TECNO260201</c>,
    /// las once primeras del código de la toma de notas — servicio y número de familia,
    /// sin la edición del documento.
    /// <para>
    /// Vive aquí y no en cada vista para que las dos digan lo mismo. Si el proyecto es
    /// anterior a que existiera ese código, se cae al de servicio y, en último término, al
    /// nombre del fichero: una tarjeta sin rótulo no se puede ni señalar.
    /// </para>
    /// </summary>
    public string Rotulo
    {
        get
        {
            var conFamilia = CodigoDeServicio.ConFamilia(CodigoTomaDeNotas);
            if (!string.IsNullOrWhiteSpace(conFamilia)) return conFamilia;

            return string.IsNullOrWhiteSpace(CodigoServicio) ? Nombre : CodigoServicio;
        }
    }

    /// <summary>Cómo lo declara la plantilla. Se lee del almacén general, como partes ‑2.</summary>
    public const string CampoDeAcreditacion = "acreditacion";

    /// <summary>Motivo por el que no se pudo leer. Si tiene valor, el resto no es fiable.</summary>
    public string? Error { get; init; }

    public bool Terminado => Error is null && SeccionesAplicables > 0
                             && SeccionesCompletadas == SeccionesAplicables;

    public string Avance => Error is not null
        ? "no se pudo leer"
        : $"{SeccionesCompletadas}/{SeccionesAplicables} secciones";

    /// <summary>
    /// Todo lo que resume el trabajo, en un renglón: <c>3W | 45 % | 7/16 secciones</c>.
    /// <para>
    /// Se arma aquí y no en cada vista para que el tablero y el calendario no se inventen
    /// dos formas de escribir lo mismo. <b>Lo que falta se cae del renglón</b> en vez de
    /// dejar un hueco entre barras: sin fechas no hay <c>3W</c>, y sin pesos no hay <c>%</c>.
    /// </para>
    /// </summary>
    public string LineaDeAvance => string.Join("  |  ",
        new[]
        {
            Planificacion.RotuloSemanas,
            PorcentajePonderado is { } porcentaje ? $"{porcentaje} %" : "",
            Avance
        }.Where(t => t.Length > 0));
}

/// <summary>Calcula el resumen de un proyecto reutilizando el motor de reglas.</summary>
public static class AnalizadorDeProyectos
{
    public static ResumenDeProyecto Analizar(
        PlantillaEnsayos plantilla, DatosProyecto datos, string ruta, DateTime modificado,
        Planificacion? planificacion = null)
        => Analizar([plantilla], datos, ruta, modificado, planificacion);

    /// <summary>
    /// Analiza el proyecto contra <b>todas las normas que lleva</b>.
    /// <para>
    /// La <b>principal</b> se detalla sección a sección, como siempre. Cada norma
    /// <b>añadida</b> —módulos LED 62031, grados IK— se resume en <b>una sola línea</b>,
    /// que desaparece cuando todas sus secciones están completas.
    /// </para>
    /// <para>
    /// Es lo que pidió el laboratorio: al responsable le interesa el detalle de lo que
    /// está ensayando y, de lo añadido, solo si queda algo por hacer. Desplegar la 62031
    /// entera dentro de un servicio de luminarias enterraba lo importante.
    /// </para>
    /// </summary>
    public static ResumenDeProyecto Analizar(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos, string ruta, DateTime modificado,
        Planificacion? planificacion = null)
    {
        var ordenadas = Ordenar(normas, datos);

        // Un motor por norma, construido una sola vez: lo usan tanto el recuento de
        // secciones como el indicador ponderado, y montarlo dos veces sería pagar dos
        // veces lo más caro del escaneo.
        var motores = ordenadas.Select(p => new MotorDeReglas(p, datos)).ToList();

        var pendientes = new List<SeccionPendiente>();
        var completadas = 0;
        var aplicables = 0;

        // La principal, sección a sección.
        foreach (var seccion in Contar(motores[0]))
        {
            aplicables++;
            if (seccion.Pendientes == 0) completadas++;
            else pendientes.Add(seccion);
        }

        // Cada añadida, en una línea.
        for (var i = 1; i < ordenadas.Count; i++)
        {
            var añadida = ordenadas[i];
            var suyas = Contar(motores[i]).ToList();
            if (suyas.Count == 0) continue;

            var pendientesEnLaNorma = suyas.Sum(s => s.Pendientes);

            aplicables++;

            if (pendientesEnLaNorma == 0) completadas++;
            else pendientes.Add(new SeccionPendiente(
                TituloDe(añadida), pendientesEnLaNorma, suyas.Sum(s => s.Aplicables)));
        }

        // El mismo indicador que estampa el informe, sumando todas las normas del servicio.
        var avance = IndicadorDeAvance.Resultado.Sumar(
            motores.Select(m => new IndicadorDeAvance(m).Calcular()));

        return new ResumenDeProyecto
        {
            PorcentajePonderado = Redondear(avance),
            Ruta = ruta,
            Nombre = Path.GetFileNameWithoutExtension(ruta),
            CodigoTomaDeNotas = datos.CodigoTomaDeNotas,
            CodigoServicio = datos.CodigoServicio,
            Tecnico = datos.Tecnico1 ?? "",
            Tecnico2 = datos.Tecnico2 ?? "",
            Acreditaciones = [.. datos.Seleccion(ResumenDeProyecto.CampoDeAcreditacion)
                                     .OrderBy(a => a, StringComparer.CurrentCulture)],
            Colaboradores = [.. datos.Colaboradores.Where(c => c.TieneAlgo).Select(c => c.Laboratorio.Trim())],
            GradoIp = GradosDelServicio.IpMaximo(datos),
            GradoIk = GradosDelServicio.IkMaximo(datos),
            NormaPrincipal = ordenadas[0].Meta.ComoSeLlamaLaNorma,
            NumeroMuestras = datos.NumeroMuestras,
            Modificado = modificado,
            Normas = [.. datos.Normas.OrderBy(n => n)],
            Planificacion = planificacion ?? new Planificacion(),
            SeccionesPendientes = pendientes,
            SeccionesCompletadas = completadas,
            SeccionesAplicables = aplicables
        };
    }

    /// <summary>
    /// El porcentaje que se enseña, a partir del indicador ponderado.
    /// <para>
    /// Dos cuidados. <b>Sin peso declarado no hay número</b> —nulo, no cero—, porque un cero
    /// fijo diría «0 %» hasta en un servicio terminado. Y <b>solo dice 100 % cuando no queda
    /// peso</b>: redondear al alza un 99,6 % pondría el cartel de acabado en un trabajo al
    /// que todavía le falta un ensayo, que es justo el error que nadie perdona.
    /// </para>
    /// </summary>
    private static int? Redondear(IndicadorDeAvance.Resultado avance)
    {
        if (avance.PesoTotal == 0) return null;
        return avance.PesoEjecutado == avance.PesoTotal
            ? 100
            : (int)Math.Floor(avance.PorcentajePonderado);
    }

    /// <summary>
    /// Las secciones de una norma que aportan algo: las que tienen al menos un apartado
    /// aplicable. Una sección entera que no aplica no cuenta para el avance.
    /// </summary>
    private static IEnumerable<SeccionPendiente> Contar(MotorDeReglas motor)
    {
        var datos = motor.Datos;

        foreach (var seccion in motor.Plantilla.Secciones)
        {
            var visibles = seccion.Bloques.Where(b => EstadoDeApartado.EsVisible(motor, b)).ToList();
            var estados = visibles.Select(b => EstadoDeApartado.De(motor, datos, b)).ToList();

            var aplicables = estados.Count(e => e != EstadoApartado.NoAplica);
            if (aplicables == 0) continue;

            // Un apartado empezado sigue contando como pendiente: al tablero le importa
            // lo que queda por hacer, y a medias no está hecho.
            yield return new SeccionPendiente(
                seccion.Titulo, estados.Count(EstadoDeApartado.EstaPendiente), aplicables);
        }
    }

    /// <summary>
    /// La norma principal primero y las añadidas detrás.
    /// <para>
    /// <b>Lo dice el proyecto</b>: se apunta al elegirla y aquí solo se lee. Es un dato
    /// suyo, igual que el responsable, y no algo que haya que reconstruir cada vez.
    /// </para>
    /// </summary>
    private static IReadOnlyList<PlantillaEnsayos> Ordenar(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos)
    {
        if (normas.Count <= 1) return normas;

        // Si la que dice el proyecto ya no está entre las suyas —se quitó desde la toma de
        // notas— no vale de nada: se vuelve a deducir en lugar de detallar una norma que
        // el servicio ya no lleva.
        var principal = normas.FirstOrDefault(p => p.Meta.Responde(datos.NormaPrincipal))
                        ?? Deducir(normas, datos);

        return [principal, .. normas.Where(p => !ReferenceEquals(p, principal))
                                    .OrderBy(p => p.Meta.Id, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Cuál era la principal en un proyecto guardado antes de que se apuntara.
    /// <para>
    /// Lo delata <b>cómo se nombran las muestras</b>: las de seguridad son
    /// <c>EBP_SAFE…</c> y las de IK <c>EBP_CLIM…</c>, y ese patrón lo fijó la norma con la
    /// que nació el proyecto. Cuando eso no lo aclara —varias normas comparten patrón—
    /// manda luminarias, que es la de uso más frecuente; y si tampoco está, el orden
    /// alfabético, que al menos es estable.
    /// </para>
    /// </summary>
    private static PlantillaEnsayos Deducir(
        IReadOnlyList<PlantillaEnsayos> normas, DatosProyecto datos)
    {
        var porPatron = normas
            .Where(p => p.Muestras.Identificador?.Patron == datos.PatronIdentificador)
            .ToList();

        return normas.FirstOrDefault(p => p.Meta.CodigoParaFichero == "60598")
               ?? (porPatron.Count == 1 ? porPatron[0] : null)
               ?? normas.OrderBy(p => p.Meta.Id, StringComparer.Ordinal).First();
    }

    /// <summary>Cómo se llama la línea de una norma añadida.</summary>
    private static string TituloDe(PlantillaEnsayos plantilla)
        => string.IsNullOrWhiteSpace(plantilla.Meta.Titulo) ? plantilla.Meta.Id : plantilla.Meta.Titulo!;

    /// <summary>Resumen de un proyecto que no se pudo leer: el tablero lo muestra igualmente.</summary>
    public static ResumenDeProyecto NoLegible(string ruta, DateTime modificado, string motivo)
        => new()
        {
            Ruta = ruta,
            Nombre = Path.GetFileNameWithoutExtension(ruta),
            Modificado = modificado,
            Error = motivo
        };
}
