using AeroRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace AeroRadar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvionesController : ControllerBase
{
    private readonly IOpenSkyClient _openSkyClient;

    public AvionesController(IOpenSkyClient openSkyClient)
    {
        _openSkyClient = openSkyClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var aviones = await _openSkyClient.ObtenerAvionesAsync(cancellationToken);
        return Ok(new { total = aviones.Count, aviones });
    }
}