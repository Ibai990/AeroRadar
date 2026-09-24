using Microsoft.AspNetCore.Mvc;

namespace AeroRadar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "AeroRadar API",
            status = "running",
            serverTimeUtc = DateTime.UtcNow
        });
    }
}