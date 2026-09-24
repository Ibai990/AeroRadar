namespace AeroRadar.Services;

public class ContadorConexiones
{
    private readonly object _bloqueo = new();
    private int _conexiones;
    private TaskCompletionSource _hayConexiones = CrearEspera();

    public int Conexiones
    {
        get
        {
            lock (_bloqueo)
            {
                return _conexiones;
            }
        }
    }

    public void Conectar()
    {
        lock (_bloqueo)
        {
            _conexiones++;
            _hayConexiones.TrySetResult();
        }
    }

    public void Desconectar()
    {
        lock (_bloqueo)
        {
            _conexiones--;

            if (_conexiones == 0)
            {
                _hayConexiones = CrearEspera();
            }
        }
    }

    public Task EsperarConexionAsync(CancellationToken cancellationToken)
    {
        lock (_bloqueo)
        {
            return _conexiones > 0
                ? Task.CompletedTask
                : _hayConexiones.Task.WaitAsync(cancellationToken);
        }
    }

    private static TaskCompletionSource CrearEspera() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}