using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Services;

namespace VERCOM.Controllers;

[Authorize(Policy = "vender")]
public class PosController : Controller
{
    private readonly PosAuthService _posAuth;

    public PosController(PosAuthService posAuth) => _posAuth = posAuth;

    public IActionResult Index() => View();

    /// <summary>
    /// Token POS para el terminal web: se emite a partir de la sesión de cookie del ERP,
    /// de modo que el cajero no vuelve a escribir su contraseña.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Token(CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Sesión inválida." });

        var result = await _posAuth.IssueForUserAsync(userId, null, cancellationToken);
        if (result.Response is null) return BadRequest(new { message = result.Error });

        Response.Headers.CacheControl = "no-store";
        return Ok(new
        {
            accessToken = result.Response.AccessToken,
            expiresAt = result.Response.ExpiresAt,
            userId = result.Response.UserId,
            nombreCompleto = result.Response.NombreCompleto,
            entidadId = result.Response.EntidadId,
            sucursalId = result.Response.SucursalId,
            permisos = result.Response.Permisos
        });
    }
}
