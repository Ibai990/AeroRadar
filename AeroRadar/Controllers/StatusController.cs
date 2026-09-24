using Microsoft.AspNetCore.Mvc;
using AeroRadar.Configuration;
using Microsoft.Extensions.Options;
using AeroRadar.Services;

namespace AeroRadar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{

    private readonly OpenSkyOptions _opcionesOpenSky;
    private readonly ContadorConexiones _contador;
    public StatusController(IOptions<OpenSkyOptions> opcionesOpenSky, ContadorConexiones contador)
    {
        _opcionesOpenSky = opcionesOpenSky.Value;
        _contador = contador;
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
            clientesConectados = _contador.Conexiones,
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