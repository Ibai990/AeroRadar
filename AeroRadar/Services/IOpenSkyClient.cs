using AeroRadar.Models;

namespace AeroRadar.Services
{
    public interface IOpenSkyClient
    {
        Task<ResultadoConsultaOpenSky> ObtenerAvionesAsync(CancellationToken cancellationToken = default);
    }
}
