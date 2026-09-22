using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/pos/seguridad")]
public sealed class PosSeguridadController : ControllerBase
{
    private readonly PosSeguridadService _service;

    public PosSeguridadController(PosSeguridadService service) => _service = service;

    [HttpGet("rbac")]
    public async Task<ActionResult<PosRbacSnapshotDto>> GetSnapshot(CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        return Ok(await _service.SnapshotAsync(entidadId, SucursalId(), cancellationToken));
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<PosUsuarioDto>>> GetUsuarios(CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        return Ok(await _service.GetUsuariosAsync(entidadId, SucursalId(), cancellationToken));
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpGet("usuarios/{id:guid}")]
    public async Task<ActionResult<PosUsuarioDto>> GetUsuario(Guid id, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var usuario = await _service.GetUsuarioAsync(entidadId, id, cancellationToken);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPost("usuarios")]
    public async Task<ActionResult<PosUsuarioDto>> CrearUsuario([FromBody] CrearUsuarioPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.CrearUsuarioAsync(entidadId, request, cancellationToken);
        return result.Error is null ? CreatedAtAction(nameof(GetUsuario), new { id = result.Usuario!.Id }, result.Usuario) : BadRequest(new { message = result.Error });
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPut("usuarios/{id:guid}")]
    public async Task<ActionResult<PosUsuarioDto>> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.ActualizarUsuarioAsync(entidadId, id, request, ActorId(), cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => Ok(result.Usuario),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpDelete("usuarios/{id:guid}")]
    public async Task<IActionResult> EliminarUsuario(Guid id, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.EliminarUsuarioAsync(entidadId, id, ActorId(), cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => Ok(result.Usuario),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPut("usuarios/{id:guid}/roles")]
    public async Task<IActionResult> AsignarRoles(Guid id, [FromBody] AsignarRolesPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.AsignarRolesAsync(entidadId, id, request, ActorId(), cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => NoContent(),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPut("usuarios/{id:guid}/password")]
    public async Task<IActionResult> CambiarPassword(Guid id, [FromBody] CambiarPasswordPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.CambiarPasswordAsync(entidadId, id, request, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => NoContent(),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<PosRolDto>>> GetRoles(CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        return Ok(await _service.GetRolesAsync(entidadId, cancellationToken));
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPost("roles")]
    public async Task<ActionResult<PosRolDto>> CrearRol([FromBody] CrearRolPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.CrearRolAsync(entidadId, request, cancellationToken);
        return result.Error is null ? Ok(result.Rol) : BadRequest(new { message = result.Error });
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPut("roles/{id:int}")]
    public async Task<ActionResult<PosRolDto>> ActualizarRol(int id, [FromBody] ActualizarRolPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.ActualizarRolAsync(entidadId, id, request, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => Ok(result.Rol),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpDelete("roles/{id:int}")]
    public async Task<IActionResult> EliminarRol(int id, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.EliminarRolAsync(entidadId, id, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => NoContent(),
            _ => BadRequest(new { message = result.Error })
        };
    }

    [Authorize(Policy = "pos_gestionar_usuarios")]
    [HttpPut("roles/{id:int}/permisos")]
    public async Task<ActionResult<PosRolDto>> AsignarPermisos(int id, [FromBody] AsignarPermisosPosRequest request, CancellationToken cancellationToken = default)
    {
        if (EntidadId() is not Guid entidadId) return Unauthorized();
        var result = await _service.AsignarPermisosAsync(entidadId, id, request, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            null => Ok(result.Rol),
            _ => BadRequest(new { message = result.Error })
        };
    }

    private Guid? EntidadId()
    {
        if (!Guid.TryParse(User.FindFirstValue("EntidadId"), out var id) || id == Guid.Empty) return null;
        return id;
    }

    private Guid? SucursalId()
    {
        if (!Guid.TryParse(User.FindFirstValue("SucursalId"), out var id) || id == Guid.Empty) return null;
        return id;
    }

    private Guid? ActorId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id == Guid.Empty) return null;
        return id;
    }
}