using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/pos/caja")]
public sealed class PosCajaController : ControllerBase
{
    private readonly PosCajaService _service;

    public PosCajaController(PosCajaService service) => _service = service;

    [HttpPost("sesiones")]
    public async Task<ActionResult<SesionCajaPosDto>> Abrir([FromBody] AbrirSesionCajaPosRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.AbrirAsync(request, cancellationToken);
        if (result.Error is not null) return result.Conflict ? Conflict(result.Error) : BadRequest(result.Error);
        return result.Conflict ? Conflict(result.Sesion) : Created("api/pos/caja/sesiones/" + result.Sesion!.Id, result.Sesion);
    }

    [HttpGet("sesiones/{id:guid}")]
    public async Task<ActionResult<SesionCajaPosDto>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("sesiones/actual")]
    public async Task<ActionResult<SesionCajaPosDto>> GetActual([FromQuery] Guid dispositivoPosId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetActualAsync(dispositivoPosId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("sesiones/{id:guid}/cierre")]
    public async Task<ActionResult<SesionCajaPosDto>> Cerrar(Guid id, [FromBody] CerrarSesionCajaPosRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CerrarAsync(id, request, cancellationToken);
        if (result.Error is not null) return BadRequest(result.Error);
        if (result.Sesion is null) return NotFound();
        return result.Conflict ? Conflict(result.Sesion) : Ok(result.Sesion);
    }
}