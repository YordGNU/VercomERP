using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/pos/sincronizacion")]
public sealed class PosSincronizacionController : ControllerBase
{
    private readonly PosSincronizacionService _service;

    public PosSincronizacionController(PosSincronizacionService service) => _service = service;

    [HttpPost("ventas")]
    public async Task<ActionResult<VentaPosPendienteDto>> Recibir([FromBody] RecibirVentaPosRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.RecibirAsync(request, cancellationToken);
        if (result.Error is not null) return result.Conflict ? Conflict(result.Error) : BadRequest(result.Error);
        return result.Accepted ? Accepted(result.Result) : Ok(result.Result);
    }

    [HttpGet("ventas/{dispositivoPosId:guid}/{idempotencyKey}")]
    public async Task<ActionResult<VentaPosPendienteDto>> Estado(Guid dispositivoPosId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await _service.EstadoAsync(dispositivoPosId, idempotencyKey, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("ventas/{dispositivoPosId:guid}/{idempotencyKey}/procesar")]
    public async Task<ActionResult<VentaPosPendienteDto>> Procesar(Guid dispositivoPosId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await _service.ProcesarAsync(dispositivoPosId, idempotencyKey, cancellationToken);
        if (result.Error is not null) return result.Conflict ? Conflict(result.Error) : BadRequest(result.Error);
        if (result.Result is null) return NotFound();
        return result.Conflict ? Conflict(result.Result) : Ok(result.Result);
    }
}