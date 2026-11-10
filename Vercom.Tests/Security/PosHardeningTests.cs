using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VERCOM.Controllers;
using VERCOM.Security;
using Xunit;

namespace Vercom.Tests.Security;

/// <summary>
/// Endurecimiento del terminal POS visible desde HTTP: cabeceras de seguridad, respuestas
/// de API no cacheables y límites de peticiones declarados en los puntos sensibles
/// (login y auto-configuración anónima por hardware).
/// </summary>
public class PosHardeningTests
{
    private static async Task<HttpContext> EjecutarMiddleware(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        var middleware = new SecurityHeadersMiddleware(_ =>
        {
            _.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
        return context;
    }

    [Fact]
    public async Task TodaRespuestaIncluyeLasCabecerasDeEndurecimiento()
    {
        var context = await EjecutarMiddleware("/Dashboard");

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("SAMEORIGIN", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
        Assert.False(string.IsNullOrWhiteSpace(context.Response.Headers["Permissions-Policy"]));
    }

    [Fact]
    public async Task LasRespuestasDeLaApiNoSeCacheanNiSeIncrustan()
    {
        var context = await EjecutarMiddleware("/api/pos/auth/login");

        Assert.Contains("no-store", context.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("frame-ancestors 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
    }

    [Fact]
    public async Task LasPaginasDelErpNoRecibenLaCspRestrictivaDeLaApi()
    {
        var context = await EjecutarMiddleware("/Production");

        Assert.True(string.IsNullOrEmpty(context.Response.Headers["Content-Security-Policy"]));
    }

    private static string? PoliticaDe(Type controlador, string? accion = null)
    {
        var atributo = (accion is null
                ? controlador.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: false)
                : controlador.GetMethod(accion, BindingFlags.Public | BindingFlags.Instance)!
                    .GetCustomAttribute<EnableRateLimitingAttribute>(inherit: false))
            ?? (accion is null ? null : controlador.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: false));

        return atributo?.PolicyName;
    }

    [Fact]
    public void ElLoginDelPosTieneLimiteDeIntentosPorIp()
    {
        Assert.Equal("pos-login", PoliticaDe(typeof(PosAuthController), nameof(PosAuthController.Login)));
    }

    [Fact]
    public void LaAutoconfiguracionAnonimaTieneLimitePropio()
    {
        Assert.Equal("pos-config", PoliticaDe(typeof(PosConfigController), nameof(PosConfigController.GetConfiguracion)));
    }

    [Fact]
    public void TodosLosControladoresDeApiDelPosTienenLimiteGlobal()
    {
        var controladores = new[]
        {
            typeof(PosApiController),
            typeof(PosAuthController),
            typeof(PosCajaController),
            typeof(PosCatalogoController),
            typeof(PosConfigController),
            typeof(PosSeguridadController),
            typeof(PosSincronizacionController)
        };

        foreach (var controlador in controladores)
        {
            var politica = PoliticaDe(controlador) ?? PoliticaDe(controlador, "Login");
            Assert.True(politica == "pos-api" || politica == "pos-login",
                $"{controlador.Name} no declara [EnableRateLimiting] de la API (política: {politica ?? "ninguna"}).");
        }
    }

    [Fact]
    public void LasAccionesDeApiNoQuedanSinLimite()
    {
        var sinLimite = new List<string>();
        var controladores = new[]
        {
            typeof(PosApiController),
            typeof(PosAuthController),
            typeof(PosCajaController),
            typeof(PosCatalogoController),
            typeof(PosConfigController),
            typeof(PosSeguridadController),
            typeof(PosSincronizacionController)
        };

        foreach (var controlador in controladores)
        {
            var politicaClase = controlador.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: false)?.PolicyName;
            foreach (var accion in controlador.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (accion.GetCustomAttribute<NonActionAttribute>() is not null) continue;
                if (!typeof(IActionResult).IsAssignableFrom(accion.ReturnType) &&
                    !(accion.ReturnType.IsGenericType && accion.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                      typeof(IActionResult).IsAssignableFrom(accion.ReturnType.GetGenericArguments()[0])))
                    continue;

                var politicaAccion = accion.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: false)?.PolicyName;
                if (string.IsNullOrEmpty(politicaAccion) && string.IsNullOrEmpty(politicaClase))
                    sinLimite.Add($"{controlador.Name}.{accion.Name}");
            }
        }

        Assert.Empty(sinLimite);
    }
}
