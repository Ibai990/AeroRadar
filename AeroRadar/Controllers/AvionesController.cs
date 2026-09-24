using AeroRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace AeroRadar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvionesController : ControllerBase
{
    private readonly AlmacenAviones _almacen;

    public AvionesController(AlmacenAviones almacen)
    {
        _almacen = almacen;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var instantanea = _almacen.Actual;

        return Ok(new
        {
            total = instantanea.Aviones.Count,
            actualizadoUtc = instantanea.ActualizadoUtc,
            aviones = instantanea.Aviones
        });
    }
}