using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vercom.Filters;

public class MustChangePasswordFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var mustChangeClaim = user.FindFirst("MustChangePassword")?.Value;

            if (mustChangeClaim == "True")
            {
                var controller = context.RouteData.Values["controller"]?.ToString();
                var action = context.RouteData.Values["action"]?.ToString();

                // No redirigir si ya está en la acción de cambio de contraseña o logout
                if (controller != "Account" || (action != "ChangePassword" && action != "Logout"))
                {
                    context.Result = new RedirectToActionResult("ChangePassword", "Account", null);
                    return;
                }
            }
        }

        await next();
    }
}
