using AeroRadar.Models;

namespace AeroRadar.Hubs;

public interface IAvionesCliente
{
    Task RecibirAviones(InstantaneaAviones instantanea);
}