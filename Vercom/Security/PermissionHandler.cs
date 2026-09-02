using Microsoft.AspNetCore.Authorization;

namespace Vercom.Security;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 1. El Administrador Global (MASTER) tiene acceso total omnipresente
        if (context.User.IsInRole("MASTER"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Verificamos si el usuario tiene el claim de permiso específico
        var hasPermission = context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission);

        // 3. El Administrador de Entidad (ADMINISTRADOR) tiene acceso total a su propia empresa
        var isAdmin = context.User.IsInRole("ADMINISTRADOR");

        if (hasPermission || isAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
