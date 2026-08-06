using System.Collections.ObjectModel;
using LumNotas.Core.Datos;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.App.ViewModels;

/// <summary>
/// Una muestra del proyecto. El identificador se compone del código de servicio y del
/// número; cambiar un número renumera consecutivamente las muestras que van por debajo.
/// </summary>
public sealed class MuestraViewModel : ObservableObject
{
    private readonly DatosProyecto _datos;
    private readonly int _posicion;
    private readonly Action<int, int> _renumerarDesde;
    private readonly Action _alCambiar;

    /// <param name="gradoIp">
    /// Campos de grado IP declarados <c>porMuestra</c>, si la norma los pide así. En la
    /// 60529 cada muestra tiene su objetivo, porque un servicio puede traer productos
    /// con grados distintos; en luminarias el grado es del proyecto y esto va vacío.
    /// </param>
    public MuestraViewModel(DatosProyecto datos, int posicion, Action<int, int> renumerarDesde,
                            IReadOnlyList<Campo>? gradoIp = null, Action? alCambiar = null,
                            bool ordinaria = false)
    {
        _datos = datos;
        _posicion = posicion;
        _renumerarDesde = renumerarDesde;
        _alCambiar = alCambiar ?? (() => { });

        TieneOrdinaria = ordinaria;
        GradosIp = [.. (gradoIp ?? [])
            .Select(c => new GradoDeMuestraViewModel(datos, posicion, c, AlElegirGrado))];
    }

    // ---- luminaria ordinaria ----------------------------------------------

    /// <summary>Grado de una luminaria ordinaria: IP20.</summary>
    private const string PrimeraOrdinaria = "IP2X";
    private const string SegundaOrdinaria = "IPX0";

    public bool TieneOrdinaria { get; }

    /// <summary>
    /// Atajo para el caso más frecuente: una luminaria ordinaria es IP20, así que
    /// marcarlo rellena las dos cifras. Si luego se elige otro grado, se desmarca solo,
    /// para que no quede una muestra que dice ser ordinaria con objetivo IPX5.
    /// </summary>
    public bool Ordinaria
    {
        get => _datos.Obtener("proyecto", "luminariaOrdinaria", _posicion) is true;
        set
        {
            _datos.Establecer("proyecto", "luminariaOrdinaria", value, _posicion);

            if (value)
            {
                _datos.Establecer("proyecto", "ipPrimeraCifra", PrimeraOrdinaria, _posicion);
                _datos.Establecer("proyecto", "ipSegundaCifra", SegundaOrdinaria, _posicion);
                foreach (var grado in GradosIp) grado.Refrescar();
            }

            Notificar();
            _alCambiar();
        }
    }

    private void AlElegirGrado()
    {
        if (Ordinaria && !EsOrdinaria())
        {
            _datos.Establecer("proyecto", "luminariaOrdinaria", false, _posicion);
            Notificar(nameof(Ordinaria));
        }

        _alCambiar();
    }

    private bool EsOrdinaria()
        => _datos.Obtener("proyecto", "ipPrimeraCifra", _posicion) as string == PrimeraOrdinaria
           && _datos.Obtener("proyecto", "ipSegundaCifra", _posicion) as string == SegundaOrdinaria;

    public int Posicion => _posicion;
    public string Etiqueta => $"Muestra {_posicion}";
    public string Identificador => _datos.IdentificadorDeMuestra(_posicion);

    /// <summary>Desplegables de grado IP objetivo de esta muestra.</summary>
    public IReadOnlyList<GradoDeMuestraViewModel> GradosIp { get; }

    public bool TieneGradosIp => GradosIp.Count > 0;

    public int Numero
    {
        get => _datos.NumeroDeMuestra(_posicion);
        set
        {
            if (value < 1 || value == _datos.NumeroDeMuestra(_posicion)) return;
            _renumerarDesde(_posicion, value);
        }
    }

    public void Refrescar()
    {
        Notificar(nameof(Numero));
        Notificar(nameof(Identificador));
        Notificar(nameof(Ordinaria));
        foreach (var grado in GradosIp) grado.Refrescar();
    }
}

/// <summary>Un desplegable de grado IP objetivo, propio de una muestra.</summary>
public sealed class GradoDeMuestraViewModel(
    DatosProyecto datos, int muestra, Campo campo, Action alCambiar) : ObservableObject
{
    public IReadOnlyList<string> Opciones => campo.Opciones;

    /// <summary>Rótulo corto: la fila de la muestra no da para la etiqueta completa.</summary>
    public string Titulo => campo.EtiquetaCorta ?? campo.Etiqueta;

    public bool Falta => campo.Obligatorio && string.IsNullOrWhiteSpace(Valor);

    /// <summary>
    /// Valor de la muestra. Si no hay nada guardado se enseña el valor por defecto de la
    /// plantilla —el grado IK de luminarias arranca en «No IK»— sin escribirlo: así se
    /// distingue lo que el técnico ha elegido de lo que aún no ha tocado.
    /// </summary>
    public string? Valor
    {
        get => datos.Obtener("proyecto", campo.Id, muestra) as string ?? campo.TextoPorDefecto;
        set
        {
            datos.Establecer("proyecto", campo.Id, value, muestra);
            Notificar();
            Notificar(nameof(Falta));
            alCambiar();
        }
    }

    public void Refrescar()
    {
        Notificar(nameof(Valor));
        Notificar(nameof(Falta));
    }
}

/// <summary>Una casilla que marca o desmarca un valor dentro de un conjunto.</summary>
/// <param name="marcar">
/// Cómo se apunta o se quita del conjunto. Se puede sustituir porque hay campos donde una
/// opción excluye a las demás —«Sin acreditar»— y entonces marcar una casilla también
/// desmarca otras. Por defecto, añadir y quitar a secas.
/// </param>
public sealed class SeleccionViewModel(string etiqueta, ISet<string> conjunto, Action alCambiar,
                                       Action<string, bool>? marcar = null) : ObservableObject
{
    public string Etiqueta { get; } = etiqueta;

    public bool Marcada
    {
        get => conjunto.Contains(Etiqueta);
        set
        {
            if (marcar is not null) marcar(Etiqueta, value);
            else if (value) conjunto.Add(Etiqueta);
            else conjunto.Remove(Etiqueta);

            Notificar();
            alCambiar();
        }
    }

    /// <summary>Relee el conjunto: hace falta cuando lo cambia algo que no es esta casilla.</summary>
    public void Refrescar() => Notificar(nameof(Marcada));
}

/// <summary>
/// Una fila de «Otros colaboradores»: qué laboratorio de fuera y qué hizo.
/// <para>
/// Los dos campos son texto libre. El del laboratorio no sale de una lista cerrada porque
/// los laboratorios cambian, y obligar a mantener un catálogo para poder escribir «IMQ
/// Italia» sería poner una puerta donde no hace falta.
/// </para>
/// </summary>
public sealed class ColaboradorViewModel : ObservableObject
{
    private readonly Action _alCambiar;

    public ColaboradorViewModel(Colaborador colaborador,
                                Action<ColaboradorViewModel> quitar,
                                Action alCambiar)
    {
        Colaborador = colaborador;
        _alCambiar = alCambiar;
        Quitar = new Comando(() => quitar(this));
    }

    public Colaborador Colaborador { get; }

    public string Laboratorio
    {
        get => Colaborador.Laboratorio;
        set { Colaborador.Laboratorio = value ?? ""; Notificar(); _alCambiar(); }
    }

    public string EnsayoYMotivo
    {
        get => Colaborador.EnsayoYMotivo;
        set { Colaborador.EnsayoYMotivo = value ?? ""; Notificar(); _alCambiar(); }
    }

    public Comando Quitar { get; }
}

/// <summary>
/// Cabecera del proyecto: lo que en el Excel era la hoja «RESUMEN PROYECTO LUM».
/// Se muestra como un apartado más, el primero de la lista.
/// </summary>
public sealed class ProyectoViewModel : ObservableObject
{
    private readonly PlantillaEnsayos _plantilla;

    /// <summary>
    /// Motor propio de la cabecera, para las reglas que deciden si un campo se muestra
    /// —la profundidad de inmersión solo aparece con objetivo IPX7 o IPX8—. Es aparte
    /// del de los apartados porque la cabecera se dibuja antes que ellos.
    /// </summary>
    private readonly MotorDeReglas _motorCabecera;
    private readonly DatosProyecto _datos;
    private readonly Action _alCambiar;
    private readonly Action _alCambiarMuestras;
    private readonly int _maxMuestras;

    /// <remarks>
    /// El tope de muestras y qué campos son obligatorios salen de la plantilla, no de
    /// aquí: cambiarlos es editar el JSON y no tocar código.
    /// </remarks>
    public ProyectoViewModel(PlantillaEnsayos plantilla, DatosProyecto datos,
                             Action alCambiar, Action alCambiarMuestras)
    {
        _plantilla = plantilla;
        _datos = datos;
        _alCambiar = alCambiar;
        _alCambiarMuestras = alCambiarMuestras;
        _maxMuestras = Math.Max(1, plantilla.Muestras.Max);
        OpcionesMuestras = [.. Enumerable.Range(1, _maxMuestras)];

        Partes2 = [.. new[] { "-2-1", "-2-2", "-2-3", "-2-4", "-2-5", "-2-10", "-2-13", "-2-18", "-2-22", "OTRO" }
            .Select(p => new SeleccionViewModel(p, datos.Partes2, alCambiar))];

        _motorCabecera = new MotorDeReglas(plantilla, datos);

        // Los campos por muestra no llevan tarjeta: se pintan junto a cada muestra.
        CamposExtra = [.. plantilla.Proyecto.Campos
            .Where(c => !ConSitioPropio.Contains(c.Id) && !c.PorMuestra)
            .Select(c => new CampoExtraViewModel(c, datos, alCambiar, EsVerdadera))];

        AnadirColaborador = new Comando(AnadirUnColaborador);
        foreach (var colaborador in datos.Colaboradores)
            Colaboradores.Add(new ColaboradorViewModel(colaborador, QuitarColaborador, alCambiar));

        RellenarTecnicos();
        ReconstruirMuestras();
    }

    // ---- listado de muestras ----------------------------------------------

    public ObservableCollection<MuestraViewModel> Muestras { get; } = [];

    public bool HayMuestras => Muestras.Count > 0;

    private void ReconstruirMuestras()
    {
        // Cualquier selección declarada por muestra se pinta en su fila: el grado IP en
        // luminarias y en la 60529, y además el grado IK en la 62262.
        var gradosPorMuestra = _plantilla.Proyecto.Campos
            .Where(c => c.PorMuestra && c.Tipo == "seleccion")
            .ToList();

        var ordinaria = _plantilla.Proyecto.Campos.Any(c => c.Id == "luminariaOrdinaria");

        Muestras.Clear();
        foreach (var posicion in _datos.Muestras)
            Muestras.Add(new MuestraViewModel(_datos, posicion, RenumerarDesde, gradosPorMuestra,
                                              _alCambiar, ordinaria));

        Notificar(nameof(HayMuestras));
    }

    /// <summary>
    /// Asigna un número a una muestra y renumera consecutivamente las siguientes.
    /// Indicar que la muestra 1 es la 03 deja las de debajo en 04, 05, 06…
    /// </summary>
    private void RenumerarDesde(int posicion, int numero)
    {
        foreach (var muestra in _datos.Muestras.Where(m => m >= posicion))
            _datos.EstablecerNumeroDeMuestra(muestra, numero + (muestra - posicion));

        foreach (var vista in Muestras) vista.Refrescar();
        _alCambiarMuestras();   // las cabeceras de columna llevan el número de muestra
    }

    public string Encabezado => "Datos del proyecto";
    public string Seccion => "Cabecera";

    /// <summary>
    /// Lo que falta por rellenar. Hasta que esté vacío, los apartados de ensayo no
    /// aparecen: no tiene sentido tomar notas sin saber muestras, clase ni grado IP.
    /// </summary>
    public string QueFalta
    {
        get
        {
            var faltan = RequisitosDelProyecto.Faltantes(_plantilla, _datos);
            // «Completar» y no «rellenar»: desde que el código se exige entero, un campo
            // puede estar escrito y aun así aparecer aquí.
            return faltan.Count == 0
                ? ""
                : "Para que aparezcan los apartados de ensayo falta por completar: " + string.Join(", ", faltan) + ".";
        }
    }

    public bool HayQueFalta => QueFalta.Length > 0;

    // Estado de cada dato obligatorio, para marcar en rojo lo que falta.

    /// <summary>
    /// Rojo mientras el código no esté <b>entero</b>, no solo mientras esté vacío: es la
    /// misma exigencia que el alta (`CodigoDeServicio.EstaCompleto`).
    /// </summary>
    public bool FaltaCodigoTomaDeNotas => !CodigoDeServicio.EstaCompleto(CodigoTomaDeNotas);
    public bool FaltaCodigo => string.IsNullOrWhiteSpace(CodigoServicio)
                               || CodigoServicio == RequisitosDelProyecto.CodigoSinAsignar;
    public bool FaltaTecnico1 => string.IsNullOrWhiteSpace(Tecnico1);
    public bool FaltaTa => Exige("ta") && _datos.Numero("proyecto", "ta") is null;
    public bool FaltaMuestras => _datos.NumeroMuestras < 1;
    public bool FaltaPartes2 => Exige("partes2") && _datos.Partes2.Count == 0;

    /// <summary>
    /// Si la norma exige ese dato. Sin esto, las tarjetas se pintaban en rojo también
    /// en las normas donde ese dato no se pide.
    /// </summary>
    private bool Exige(string campo)
        => _plantilla.Proyecto.Campos.Any(c => c.Id == campo && c.Obligatorio);

    /// <summary>
    /// Evalúa una regla de la cabecera. Si falla, el campo se muestra: es preferible
    /// enseñar un dato de más que esconder uno que hace falta.
    /// </summary>
    private bool EsVerdadera(string referencia)
    {
        try { return _motorCabecera.EsVerdadera(referencia); }
        catch (Exception) { return true; }
    }

    /// <summary>
    /// Título de la tarjeta de partes ‑2. Lo pone la norma: en luminarias son las partes
    /// ‑2 aplicables y en el IK 62262 es la tipología del producto, porque un servicio de
    /// IK puede ser de una luminaria o de otra cosa.
    /// </summary>
    public string EtiquetaPartes2
    {
        get
        {
            var campo = _plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == "partes2");
            var texto = campo?.Etiqueta ?? "Partes -2 aplicables";
            return campo?.Obligatorio == true ? texto + " *" : texto;
        }
    }

    /// <summary>La tarjeta solo se muestra en las normas que declaran el campo.</summary>
    public bool MuestraPartes2 => _plantilla.Proyecto.Campos.Any(c => c.Id == "partes2");

    /// <summary>
    /// Casillas para añadir otras normas al servicio. Las lleva el documento, que es
    /// quien sabe cuáles hay cargadas; aquí solo se pintan.
    /// </summary>
    public IReadOnlyList<NormaAnadibleViewModel> NormasAnadibles { get; set; } = [];

    public bool HayNormasAnadibles => NormasAnadibles.Count > 0;

    /// <summary>El documento avisa cuando cambia la lista de normas añadibles.</summary>
    public void RefrescarNormas()
    {
        Notificar(nameof(NormasAnadibles));
        Notificar(nameof(HayNormasAnadibles));
    }

    public void Refrescar()
    {
        Notificar(nameof(QueFalta));
        Notificar(nameof(HayQueFalta));
        Notificar(nameof(FaltaCodigoTomaDeNotas));
        Notificar(nameof(FaltaCodigo));
        Notificar(nameof(FaltaTecnico1));
        Notificar(nameof(FaltaTa));
        Notificar(nameof(FaltaMuestras));
        Notificar(nameof(FaltaPartes2));
        Notificar(nameof(FaltaClase));
        Notificar(nameof(Clase));

        _motorCabecera.Invalidar();
        foreach (var campo in CamposExtra) campo.Refrescar();
        foreach (var muestra in Muestras) muestra.Refrescar();
    }

    // El índice de la izquierda es común a todos los paneles: la cabecera no tiene
    // semáforo y siempre se muestra.
    public EstadoApartado Estado => EstadoApartado.SinReglas;
    public string EstadoTexto => "";
    public bool Visible => true;

    public IReadOnlyList<SeleccionViewModel> Partes2 { get; }

    /// <summary>Campos propios de la norma. En luminarias está vacía.</summary>
    public IReadOnlyList<CampoExtraViewModel> CamposExtra { get; }

    // ---- laboratorios de fuera ----------------------------------------------

    /// <summary>
    /// Los colaboradores del servicio. Es una colección que <b>no se sustituye nunca</b>,
    /// solo se añade y se quita: la lista está atada a la ventana y cambiarla entera
    /// perdería el foco de la casilla que se estuviera escribiendo.
    /// </summary>
    public ObservableCollection<ColaboradorViewModel> Colaboradores { get; } = [];

    public Comando AnadirColaborador { get; }

    /// <summary>
    /// Añade una fila en blanco para rellenar. No se pide confirmación ni se valida nada:
    /// una fila vacía no llega al fichero —la descarta el repositorio al guardar—, así que
    /// pulsar el botón por error no deja rastro.
    /// </summary>
    private void AnadirUnColaborador()
    {
        var colaborador = new Colaborador();
        _datos.Colaboradores.Add(colaborador);
        Colaboradores.Add(new ColaboradorViewModel(colaborador, QuitarColaborador, _alCambiar));
        _alCambiar();
    }

    private void QuitarColaborador(ColaboradorViewModel fila)
    {
        _datos.Colaboradores.Remove(fila.Colaborador);
        Colaboradores.Remove(fila);
        _alCambiar();
    }

    /// <summary>
    /// Ids que ya tienen su propio sitio en la ventana. Todo lo demás que declare la
    /// plantilla se pinta en <see cref="CamposExtra"/>.
    /// </summary>
    private static readonly HashSet<string> ConSitioPropio =
    [
        "codigoTomaDeNotas", "codigoServicio", "tecnico1", "tecnico2", "numeroMuestras",
        "numeracionMuestras", "inicioNumeracion", "comentariosGenerales",
        "ta", "clase", "partes2"
    ];

    /// <summary>
    /// El que identifica esta toma de notas y da nombre al fichero. Al cambiarlo,
    /// <b>arrastra el código de servicio</b> con sus nueve primeras, salvo que alguien lo
    /// haya corregido a mano — quién decide eso es <see cref="CodigoDeServicio.Sugerir"/>,
    /// en el núcleo y con tests.
    /// </summary>
    public string CodigoTomaDeNotas
    {
        get => _datos.CodigoTomaDeNotas;
        set
        {
            var anterior = _datos.CodigoTomaDeNotas;
            var nuevo = value ?? "";
            if (nuevo == anterior) return;

            _datos.CodigoTomaDeNotas = nuevo;
            CodigoServicio = CodigoDeServicio.Sugerir(anterior, nuevo, _datos.CodigoServicio);

            Notificar();
            Notificar(nameof(FaltaCodigoTomaDeNotas));
        }
    }

    public string CodigoServicio
    {
        get => _datos.CodigoServicio;
        set
        {
            _datos.CodigoServicio = value ?? "";
            Notificar();
            Notificar(nameof(FaltaCodigo));
            // Los identificadores de muestra se componen con el código de servicio.
            foreach (var muestra in Muestras) muestra.Refrescar();
            _alCambiar();
        }
    }

    /// <summary>
    /// Técnicos que ofrece el desplegable, incluidos los nombres que ya tuviera el
    /// proyecto aunque no estén en la lista: los servicios guardados antes de existir el
    /// catálogo llevan el técnico escrito a mano y <b>no pueden quedarse en blanco</b>.
    /// <para>
    /// Es una colección que <b>no se sustituye nunca</b>, solo se ajusta por dentro.
    /// Cambiarla entera desde el <c>set</c> de Técnico 1 hacía que el desplegable
    /// perdiera la selección, volviera a escribirla y se llamara a sí mismo sin fin:
    /// desbordamiento de pila y la aplicación cerrándose sin dejar ni registro.
    /// </para>
    /// </summary>
    public ObservableCollection<string> Tecnicos { get; } = [];

    /// <summary>
    /// Técnico 1, el <b>responsable</b> del proyecto: es el que sale en el tablero y por
    /// el que se filtra el calendario.
    /// </summary>
    public string? Tecnico1
    {
        get => _datos.Tecnico1;
        set
        {
            if (Tecnico1 == value) return;

            _datos.Tecnico1 = value;
            Notificar();
            Notificar(nameof(FaltaTecnico1));
            _alCambiar();
        }
    }

    public string? Tecnico2
    {
        get => _datos.Tecnico2;
        set
        {
            if (Tecnico2 == value) return;

            _datos.Tecnico2 = value;
            Notificar();
            _alCambiar();
        }
    }

    /// <summary>
    /// Vuelve a leer la lista tras editarla desde «Configuración | Técnicos…».
    /// </summary>
    /// <param name="renombrados">
    /// Correcciones de nombre hechas en el editor. Se aplican también al proyecto abierto:
    /// en el fichero ya se han corregido, y si la pestaña se quedara con el nombre viejo lo
    /// devolvería al guardar.
    /// </param>
    public void RefrescarTecnicos(IReadOnlyList<(string Viejo, string Nuevo)>? renombrados = null)
    {
        foreach (var (viejo, nuevo) in renombrados ?? [])
        {
            // Sin pasar por el 'set': el fichero ya lo tiene así, no es un cambio del técnico.
            if (Coincide(Tecnico1, viejo)) _datos.Tecnico1 = nuevo;
            if (Coincide(Tecnico2, viejo)) _datos.Tecnico2 = nuevo;
        }

        RellenarTecnicos();

        Notificar(nameof(Tecnico1));
        Notificar(nameof(Tecnico2));
        Notificar(nameof(FaltaTecnico1));
    }

    private static bool Coincide(string? valor, string nombre)
        => string.Equals(valor?.Trim(), nombre.Trim(), StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Ajusta el contenido de <see cref="Tecnicos"/> conservando la colección. Se quita y
    /// se añade lo justo para no tocar el elemento que esté seleccionado.
    /// </summary>
    private void RellenarTecnicos()
    {
        var deben = ServicioDeTecnicos.Catalogo.ConNombreSuelto(Tecnico1, Tecnico2);

        for (var i = Tecnicos.Count - 1; i >= 0; i--)
            if (!deben.Contains(Tecnicos[i])) Tecnicos.RemoveAt(i);

        for (var i = 0; i < deben.Count; i++)
            if (!Tecnicos.Contains(deben[i])) Tecnicos.Insert(Math.Min(i, Tecnicos.Count), deben[i]);
    }

    public string Ta
    {
        get => _datos.Obtener("proyecto", "ta")?.ToString() ?? "";
        set
        {
            _datos.Establecer("proyecto", "ta", double.TryParse(value, out var n) ? n : null);
            Notificar();
            _alCambiar();
        }
    }

    /// <summary>
    /// Opciones del selector. Empieza sin valor: 0 no es válido y obliga a elegir,
    /// para que nadie se deje puesto un 1 por descuido.
    /// </summary>
    public IReadOnlyList<int> OpcionesMuestras { get; }

    /// <summary>Cambiarlo obliga a reconstruir los formularios: hay una columna por muestra.</summary>
    public int NumeroMuestras
    {
        get => _datos.NumeroMuestras;
        set
        {
            var nuevo = Math.Clamp(value, 0, _maxMuestras);
            if (nuevo == _datos.NumeroMuestras) return;
            _datos.NumeroMuestras = nuevo;
            Notificar();
            ReconstruirMuestras();
            _alCambiarMuestras();
        }
    }

    /// <summary>Opciones de clase, tomadas de la norma. Vacío si no la declara.</summary>
    public IReadOnlyList<string> Clases =>
        _plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == "clase")?.Opciones ?? [];

    /// <summary>La fila de clase solo se enseña en las normas que la piden.</summary>
    public bool MuestraClase => Clases.Count > 0;

    /// <summary>
    /// Arranca <b>vacía</b> y hay que elegir, igual que el nº de muestras. Antes el
    /// desplegable enseñaba «I» aunque nadie lo hubiera elegido, así que un proyecto de
    /// Clase II se guardaba como I si el técnico no caía en tocarlo.
    /// </summary>
    public string? Clase
    {
        get => _datos.Obtener("proyecto", "clase") as string;
        set
        {
            _datos.Establecer("proyecto", "clase", value);
            Notificar();
            Notificar(nameof(FaltaClase));
            _alCambiar();
        }
    }

    public bool FaltaClase => Exige("clase") && string.IsNullOrWhiteSpace(Clase);
}

/// <summary>
/// Campo de cabecera propio de una norma, generado desde la plantilla. Cubre los tres
/// casos que hacen falta: un número (Tc), una opción única (clasificación del módulo
/// LED) y una selección múltiple (grado IK).
/// </summary>
public sealed class CampoExtraViewModel : ObservableObject
{
    private readonly Campo _campo;
    private readonly DatosProyecto _datos;
    private readonly Action _alCambiar;
    private readonly Func<string, bool> _esVerdadera;

    public CampoExtraViewModel(Campo campo, DatosProyecto datos, Action alCambiar,
                               Func<string, bool> esVerdadera)
    {
        _campo = campo;
        _datos = datos;
        _alCambiar = alCambiar;
        _esVerdadera = esVerdadera;

        // Con una opción excluyente, marcar una casilla puede desmarcar otras, así que
        // después hay que releerlas todas: la que se acaba de apagar no se entera sola.
        Marcas = EsMultiple
            ? [.. campo.Opciones.Select(o => new SeleccionViewModel(
                o, datos.Seleccion(campo.Id), alCambiar,
                campo.OpcionExcluyente is null ? null : Marcar))]
            : [];

        AplicarValorPorDefecto();
    }

    /// <summary>
    /// Marca o desmarca aplicando la exclusión que declare la plantilla, y repinta todas
    /// las casillas del campo: al marcar «Sin acreditar» se apagan ENAC, ENEC y CB, y
    /// ninguna de las tres se entera por su cuenta.
    /// </summary>
    private void Marcar(string opcion, bool marcada)
    {
        SeleccionExcluyente.Aplicar(
            _datos.Seleccion(_campo.Id), opcion, marcada, _campo.OpcionExcluyente);

        foreach (var marca in Marcas) marca.Refrescar();
    }

    /// <summary>
    /// Si la norma declara un <c>visibleSi</c>, el campo solo aparece cuando esa regla se
    /// cumple: la profundidad de inmersión únicamente con objetivo IPX7 o IPX8.
    /// </summary>
    public bool Visible => _campo.VisibleSi is null || _esVerdadera(_campo.VisibleSi);

    /// <summary>
    /// Rellena el valor por defecto en cuanto el campo pasa a mostrarse y sigue vacío.
    /// No se escribe antes: no tiene sentido dejar apuntada la temperatura del agua en
    /// un servicio que no sumerge nada.
    /// </summary>
    private void AplicarValorPorDefecto()
    {
        if (_campo.NumeroPorDefecto is not { } porDefecto) return;
        if (!Visible || _datos.Obtener("proyecto", _campo.Id) is not null) return;

        _datos.Establecer("proyecto", _campo.Id, porDefecto);
    }

    public string Etiqueta
    {
        get
        {
            var texto = _campo.Unidad is null ? _campo.Etiqueta : $"{_campo.Etiqueta} ({_campo.Unidad})";
            return _campo.Obligatorio ? texto + " *" : texto;
        }
    }

    public string? Nota => _campo.Nota;
    public bool HayNota => !string.IsNullOrWhiteSpace(_campo.Nota);

    public bool EsMultiple => _campo.Tipo == "seleccion" && _campo.Multiple;
    public bool EsUnica => _campo.Tipo == "seleccion" && !_campo.Multiple;
    public bool EsNumero => _campo.Tipo is "numero" or "entero";

    public IReadOnlyList<string> Opciones => _campo.Opciones;
    public IReadOnlyList<SeleccionViewModel> Marcas { get; }

    /// <summary>Obligatorio y sin rellenar: se pinta en rojo, igual que los de luminarias.</summary>
    public bool Falta => _campo.Obligatorio && !TieneValor;

    private bool TieneValor => EsMultiple
        ? _datos.Seleccion(_campo.Id).Count > 0
        : _datos.Obtener("proyecto", _campo.Id) switch
        {
            null => false,
            string texto => !string.IsNullOrWhiteSpace(texto),
            _ => true
        };

    public string? Seleccionada
    {
        get => _datos.Obtener("proyecto", _campo.Id) as string;
        set { _datos.Establecer("proyecto", _campo.Id, value); Notificar(); Notificar(nameof(Falta)); _alCambiar(); }
    }

    public string Texto
    {
        get => _datos.Obtener("proyecto", _campo.Id)?.ToString() ?? "";
        set
        {
            _datos.Establecer("proyecto", _campo.Id, double.TryParse(value, out var n) ? n : null);
            Notificar();
            Notificar(nameof(Falta));
            _alCambiar();
        }
    }

    public void Refrescar()
    {
        AplicarValorPorDefecto();
        Notificar(nameof(Visible));
        Notificar(nameof(Falta));
        Notificar(nameof(Texto));
        Notificar(nameof(Seleccionada));
    }
}
