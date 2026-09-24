using Microsoft.AspNetCore.Mvc;
using AeroRadar.Configuration;
using Microsoft.Extensions.Options;

namespace AeroRadar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{

    private readonly OpenSkyOptions _opcionesOpenSky;

    public StatusController(IOptions<OpenSkyOptions> opcionesOpenSky)
    {
        _opcionesOpenSky = opcionesOpenSky.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {

        var zona = _opcionesOpenSky.Zona;

        return Ok(new
        {
            service = "AeroRadar API",
            status = "running",
            serverTimeUtc = DateTime.UtcNow,
            intervaloConsultaSegundos = _opcionesOpenSky.IntervaloConsultaSegundos,
            zonaConsulta = new
            {
                zona.LatitudMin,
                zona.LatitudMax,
                zona.LongitudMin,
                zona.LongitudMax,
                areaGradosCuadrados = Math.Round(zona.AreaGradosCuadrados, 1)
            }

        });
    }
}