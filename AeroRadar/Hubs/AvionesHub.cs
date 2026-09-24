using AeroRadar.Services;
using Microsoft.AspNetCore.SignalR;

namespace AeroRadar.Hubs;

public class AvionesHub : Hub<IAvionesCliente>
{
    private readonly AlmacenAviones _almacen;

    public AvionesHub(AlmacenAviones almacen)
    {
        _almacen = almacen;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.RecibirAviones(_almacen.Actual);
        await base.OnConnectedAsync();
    }
}