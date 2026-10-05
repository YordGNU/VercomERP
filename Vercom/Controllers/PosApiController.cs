using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[Route("api")]
[ApiController]
public class PosApiController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPosService _posService;
    private readonly IImportacionService _importService;

    public PosApiController(IAuthService authService, IPosService posService, IImportacionService importacion)
    {
        _authService = authService;
        _posService = posService;
        _importService = importacion;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Dictionary<string, string> credentials)
    {
        if (!credentials.TryGetValue("username", out var username) ||
            !credentials.TryGetValue("password", out var password))
        {
            return BadRequest(new { message = "Credenciales requeridas" });
        }

        var result = await _authService.LoginAsync(username, password, false);
        if (result.Succeeded)
        {
            return Ok(new { token = "auth-active", message = result.Message });
        }

        return Unauthorized(new { message = result.Message });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("pos/reserva-rango")]
    public async Task<IActionResult> ReservarRango([FromQuery] Guid dispositivoId, [FromQuery] string serie = "POS", [FromQuery] int cantidad = 100)
    {
        try
        {
            var range = await _posService.ReserveNumberRangeAsync(dispositivoId, serie, cantidad);
            return Ok(new { range.Serie, desde = range.Desde, hasta = range.Hasta });
        }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("pos/dispositivo/heartbeat")]
    public async Task<IActionResult> Heartbeat([FromQuery] Guid dispositivoPosId)
    {
        var result = await _posService.HeartbeatAsync(dispositivoPosId);
        if (!result.Succeeded) return NotFound(new { message = result.Message });
        return Ok(new { message = "OK", timestamp = DateTimeOffset.UtcNow });
    }

    [HttpPost("ventas/sync")]
    public async Task<IActionResult> SyncVentas([FromBody] List<OperacionRequestDto> operaciones)
    {
        var result = await _posService.QueuePendingSalesAsync(operaciones);
        if (result.Succeeded)
        {
            return Ok(new { status = "RECEIVED", count = operaciones.Count });
        }
        return StatusCode(500, result.Message);
    }

    [HttpGet("pos/productos")]
    public async Task<IActionResult> GetProductos([FromQuery] string? updatedSince = null)
    {
        DateTimeOffset? since = null;
        if (!string.IsNullOrEmpty(updatedSince) && DateTimeOffset.TryParse(updatedSince, out var date))
        {
            since = date;
        }

        var res = await _posService.GetCatalogForPosAsync(since);
        return Ok(res);
    }

    [HttpGet("importacion/test-conexion")]
    public async Task<IActionResult> TestConnection(ConexionRequest conexion)
    {
        var res = await _importService.TestConexionAsync(conexion);
        if (res.Exitoso)
        {
            return Ok(res);
        }
        else
        {
            return StatusCode(500, res.Mensaje);
        }
    }

}
