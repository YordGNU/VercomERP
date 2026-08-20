using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Vercom.Security;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Verificamos si el usuario tiene el claim de permiso correspondiente
        // Los permisos se cargaron en el AuthService durante el Login
        var hasPermission = context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission);

        // El Administrador del Sistema (Rol) tiene acceso total por defecto
        var isAdmin = context.User.IsInRole("ADMINISTRADOR");

        if (hasPermission || isAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
