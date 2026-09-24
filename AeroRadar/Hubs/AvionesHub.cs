using AeroRadar.Services;
using Microsoft.AspNetCore.SignalR;

namespace AeroRadar.Hubs;

public class AvionesHub : Hub<IAvionesCliente>
{
    private readonly AlmacenAviones _almacen;
    private readonly ContadorConexiones _contador;

    public AvionesHub(AlmacenAviones almacen, ContadorConexiones contador)
    {
        _almacen = almacen;
        _contador = contador;
    }

    public override async Task OnConnectedAsync()
    {
        _contador.Conectar();

        var instantanea = _almacen.Actual;
        if (instantanea.ActualizadoUtc is not null)
        {
            await Clients.Caller.RecibirAviones(instantanea);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _contador.Desconectar();
        await base.OnDisconnectedAsync(exception);
    }
}