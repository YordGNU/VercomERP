namespace VERCOM.Security;

/// <summary>
/// Cabeceras de endurecimiento comunes a toda la aplicación y específicas para la API
/// del POS (respuestas no cacheables y sin capacidad de incrustarse ni ejecutar contenido).
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            // La API no se cachea ni se incrusta: los datos de la terminal (token, ventas,
            // configuración de la entidad) no deben quedar en cachés intermedias.
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        await _next(context);
    }
}
