using System.IO;
using System.IO.Pipes;

namespace LumNotas.App;

/// <summary>
/// Un solo programa abierto por usuario, y los dobles clics posteriores van a parar a la
/// ventana que ya está.
///
/// <para>
/// <b>Por qué.</b> Dos ventanas del programa pueden tener abierta la misma toma de notas
/// —dentro de una sola, abrir un fichero ya abierto salta a su pestaña, pero eso no cruza
/// de un programa a otro—, y entonces gana el último que guarda: el otro pierde su trabajo
/// sin que nada se lo diga. Además comparten los ajustes y la caché del escaneo.
/// </para>
///
/// <para>
/// <b>Y ahora hace más falta que antes</b>: la extensión <c>.lmnlab</c> la abre el
/// lanzador, así que cada doble clic arrancaría un programa nuevo.
/// </para>
///
/// <para>
/// <b>No basta con no arrancar dos veces.</b> Si el segundo arranque se limitara a cerrarse,
/// el doble clic sobre una toma de notas no haría nada: el técnico creería que el programa
/// está roto. Por eso el segundo le pasa la ruta al primero y se va.
/// </para>
/// </summary>
public static class UnaSolaInstancia
{
    // Local\ y no Global\: uno por sesión de usuario. En Global\ dos técnicos con sesión
    // abierta en el mismo equipo se estorbarían.
    private const string NombreDelCerrojo = @"Local\LumenLab.instancia";
    private const string NombreDelCanal = "LumenLab.instancia.canal";

    // Se guarda para que viva lo que viva el programa: un Mutex que recoja el recolector
    // de basura suelta el cerrojo, y entonces el siguiente arranque se creería el primero.
    private static Mutex? _cerrojo;

    /// <summary>
    /// Intenta ser la única instancia. Si ya había otra, le manda los ficheros y devuelve
    /// <c>false</c> para que esta se cierre.
    /// </summary>
    public static bool Reclamar(string[] argumentos)
    {
        try
        {
            _cerrojo = new Mutex(initiallyOwned: true, NombreDelCerrojo, out var soyLaPrimera);
            if (soyLaPrimera) return true;
        }
        catch
        {
            // Si el cerrojo falla —permisos raros—, más vale dos programas que ninguno.
            return true;
        }

        foreach (var fichero in argumentos.Where(File.Exists)) Mandar(fichero);
        return false;
    }

    /// <summary>Empieza a escuchar lo que manden los arranques siguientes.</summary>
    public static void Atender(Action<string> alRecibir)
        => new Thread(() => Escuchar(alRecibir)) { IsBackground = true, Name = "canal" }.Start();

    private static void Escuchar(Action<string> alRecibir)
    {
        while (true)
        {
            try
            {
                // Uno cada vez: llegan de un doble clic, no de una ráfaga.
                using var canal = new NamedPipeServerStream(
                    NombreDelCanal, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                canal.WaitForConnection();

                using var lector = new StreamReader(canal);
                if (lector.ReadLine() is { Length: > 0 } ruta && File.Exists(ruta)) alRecibir(ruta);
            }
            catch
            {
                // Que se rompa una conexión no puede tumbar la escucha. Se vuelve a
                // esperar: el programa sigue siendo perfectamente usable a mano.
                Thread.Sleep(200);
            }
        }
    }

    private static void Mandar(string ruta)
    {
        try
        {
            using var canal = new NamedPipeClientStream(".", NombreDelCanal, PipeDirection.Out);

            // Con un segundo basta y sobra en local. Si no contesta, el programa que
            // había estará colgado o cerrándose: no se insiste.
            canal.Connect(1000);

            using var escritor = new StreamWriter(canal) { AutoFlush = true };
            escritor.WriteLine(ruta);
        }
        catch
        {
            // Sin canal, el fichero no se abre. Es preferible a dos programas peleándose
            // por el mismo .lmnlab, que es lo que se viene a evitar.
        }
    }
}
