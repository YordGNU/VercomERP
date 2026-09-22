using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Route("api/pos/auth")]
public sealed class PosAuthController : ControllerBase
{
    private readonly PosAuthService _service;

    public PosAuthController(PosAuthService service) => _service = service;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<PosLoginResponse>> Login([FromBody] PosLoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.LoginAsync(request, cancellationToken);
        return result.Response is null ? Unauthorized(new { message = result.Error }) : Ok(result.Response);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        username = User.Identity?.Name,
        entidadId = User.FindFirst("EntidadId")?.Value,
        sucursalId = User.FindFirst("SucursalId")?.Value,
        roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value),
        permisos = User.FindAll("Permission").Select(x => x.Value)
    });
}