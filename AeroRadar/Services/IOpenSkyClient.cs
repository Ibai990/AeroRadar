using AeroRadar.Models;

namespace AeroRadar.Services
{
    public interface IOpenSkyClient
    {
        Task<List<Avion>> ObtenerAvionesAsync(CancellationToken cancellationToken = default);
    }
}
