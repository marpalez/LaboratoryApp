namespace LumNotas.Core.Gestion;

/// <summary>
/// Una barra del calendario mientras se arrastra: dónde se dibuja, qué fechas está
/// enseñando y si hay algo que guardar al soltarla.
/// <para>
/// Vive en el núcleo, y no en el modelo de vista, para poder probar el gesto entero
/// sin ratón: empezar, arrastrar, soltar y cancelar.
/// </para>
/// </summary>
public sealed class BarraDePlanificacion(Planificacion plan, EjeDeSemanas eje)
{
    private ModoArrastre _modo;
    private DateTime _inicioAlEmpezar;
    private DateTime _finAlEmpezar;

    /// <summary>Fecha de inicio que se está enseñando, que durante el arrastre no es la guardada.</summary>
    public DateTime? Inicio { get; private set; } = plan.Inicio;

    public DateTime? Fin { get; private set; } = plan.FinEfectivo;

    /// <summary>Solo se arrastra lo que ya está en la línea de tiempo.</summary>
    public bool SePuedeArrastrar => Inicio is not null && Fin is not null;

    public double Izquierda => Inicio is { } inicio ? eje.PosicionDe(inicio) : 0;

    public double Ancho => Inicio is { } inicio && Fin is { } fin ? eje.AnchoEntre(inicio, fin) : 0;

    /// <summary>Si lo que se enseña difiere de lo guardado, es decir, si hay que escribir.</summary>
    public bool HayCambio => Inicio != plan.Inicio || Fin != plan.FinEfectivo;

    public void Empezar(ModoArrastre modo)
    {
        if (!SePuedeArrastrar) return;

        _modo = modo;
        _inicioAlEmpezar = Inicio!.Value;
        _finAlEmpezar = Fin!.Value;
    }

    /// <summary>
    /// Coloca la barra a tantos píxeles de donde se pulsó. Siempre desde el punto de
    /// partida, no acumulando: así el gesto es reversible sin arrastrar la deriva del
    /// redondeo a días.
    /// </summary>
    public void Arrastrar(double pixeles)
    {
        if (!SePuedeArrastrar) return;

        var (inicio, fin) = ArrastreDeFechas.Aplicar(
            _inicioAlEmpezar, _finAlEmpezar, _modo, eje.DiasEn(pixeles));

        (Inicio, Fin) = Acotar(inicio, fin);
    }

    /// <summary>
    /// Deja la barra dentro del calendario que se está dibujando. Sin esto se puede
    /// arrastrar hasta un sitio donde no hay semanas: la barra queda flotando en blanco,
    /// sin saber en qué fecha se está soltando. Para llevar un servicio más lejos se pide
    /// sitio con «▶» y luego se arrastra.
    /// </summary>
    private (DateTime, DateTime) Acotar(DateTime inicio, DateTime fin)
    {
        var primero = eje.Desde;
        var ultimo = eje.Hasta.AddDays(-1);

        if (_modo == ModoArrastre.Mover)
        {
            // Mover conserva la duración, así que topar por un lado empuja el otro.
            var dias = (fin - inicio).Days;

            if (inicio < primero) (inicio, fin) = (primero, primero.AddDays(dias));
            if (fin > ultimo) (inicio, fin) = (ultimo.AddDays(-dias), ultimo);

            // Un servicio más largo que todo el calendario no se puede encajar; se deja
            // pegado al principio en vez de dar fechas absurdas.
            if (inicio < primero) inicio = primero;

            return (inicio, fin);
        }

        if (inicio < primero) inicio = primero;
        if (fin > ultimo) fin = ultimo;
        if (fin < inicio) fin = inicio;

        return (inicio, fin);
    }

    /// <summary>Vuelve a donde estaba antes de empezar el gesto.</summary>
    public void Cancelar()
    {
        if (!SePuedeArrastrar) return;

        Inicio = _inicioAlEmpezar;
        Fin = _finAlEmpezar;
    }

    /// <summary>La planificación con las fechas nuevas, para guardarla.</summary>
    public Planificacion Resultado()
    {
        var nueva = plan.Copia();
        nueva.Inicio = Inicio;
        nueva.Fin = Fin;
        return nueva;
    }
}
