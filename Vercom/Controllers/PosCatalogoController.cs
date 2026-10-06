using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/pos/catalogo")]
public sealed class PosCatalogoController : ControllerBase
{
    private readonly PosCatalogoService _service;

    public PosCatalogoController(PosCatalogoService service) => _service = service;

    [HttpGet("productos")]
    public async Task<ActionResult<IReadOnlyList<PosProductoDto>>> GetProductos(
        [FromQuery] Guid almacenId,
        [FromQuery] string? search = null,
        [FromQuery] DateTimeOffset? updatedSince = null,
        CancellationToken cancellationToken = default)
    {
        if (almacenId == Guid.Empty) return BadRequest("almacenId es obligatorio.");
        return Ok(await _service.GetProductosAsync(almacenId, search, updatedSince, cancellationToken));
    }

    [HttpGet("precios")]
    public async Task<ActionResult> GetPrecios([FromQuery] Guid? clienteId, CancellationToken cancellationToken = default)
    {
        var entidadIdRaw = User.FindFirstValue("EntidadId");
        if (!Guid.TryParse(entidadIdRaw, out var entidadId) || entidadId == Guid.Empty) return Unauthorized();
        return Ok(await _service.GetPreciosAsync(entidadId, clienteId, cancellationToken));
    }

    [HttpGet("clientes")]
    public async Task<ActionResult> GetClientes([FromQuery] Guid entidadId, CancellationToken cancellationToken = default)
    {
        if (entidadId == Guid.Empty) return BadRequest("entidadId es obligatorio.");
        return Ok(await _service.GetClientesAsync(entidadId, cancellationToken));
    }

    [HttpGet("dispositivos")]
    public async Task<ActionResult<IReadOnlyList<PosDispositivoDto>>> GetDispositivos(CancellationToken cancellationToken = default)
    {
        var entidadIdRaw = User.FindFirstValue("EntidadId");
        if (!Guid.TryParse(entidadIdRaw, out var entidadId) || entidadId == Guid.Empty) return Unauthorized();
        return Ok(await _service.GetDispositivosAsync(entidadId, cancellationToken));
    }
}