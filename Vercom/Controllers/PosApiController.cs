using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Services;

namespace VERCOM.Controllers;

[Route("api")]
[ApiController]
public class PosApiController : ControllerBase
{
    private readonly IPosService _posService;

    public PosApiController(IPosService posService)
    {
        _posService = posService;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("pos/dispositivo/heartbeat")]
    public async Task<IActionResult> Heartbeat([FromQuery] Guid dispositivoPosId)
    {
        var result = await _posService.HeartbeatAsync(dispositivoPosId);
        if (!result.Succeeded) return NotFound(new { message = result.Message });
        return Ok(new { message = "OK", timestamp = DateTimeOffset.UtcNow });
    }
}
