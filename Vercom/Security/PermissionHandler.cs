using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;

namespace Vercom.Security;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userId = Guid.Parse(userIdClaim.Value);

        // Verificar si el usuario tiene algún rol que contenga el permiso requerido
        var hasPermission = await dbContext.UsuarioRols
            .Where(ur => ur.UsuarioId == userId)
            .Select(ur => ur.Rol)
            .AnyAsync(r => r.Permisos.Any(p => p.Codigo == requirement.Permission));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
