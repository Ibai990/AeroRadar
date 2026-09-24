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
    private readonly EstadoCuota _estadoCuota;
    public StatusController(IOptions<OpenSkyOptions> opcionesOpenSky, ContadorConexiones contador, EstadoCuota estadoCuota)
    {
        _opcionesOpenSky = opcionesOpenSky.Value;
        _contador = contador;
        _estadoCuota = estadoCuota;
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
            modoAcceso = _opcionesOpenSky.TieneCredenciales ? "autenticado" : "anonimo",
            cuota = new
            {
                _estadoCuota.Actual.CreditosRestantes,
                creditosDiarios = _opcionesOpenSky.CreditosDiarios,
                intervaloActualSegundos = _estadoCuota.Actual.IntervaloActual.TotalSeconds,
                _estadoCuota.Actual.PausadoHastaUtc
            },
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