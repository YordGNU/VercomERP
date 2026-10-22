using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using VERCOM.Controllers;
using Xunit;

namespace Vercom.Tests.Security;

/// <summary>
/// Los endpoints <c>api/*</c> no pueden quedar públicos por descuido: el POS se autentica con
/// JWT (primero <c>/api/pos/auth/login</c>) y las únicas rutas anónimas legítimas están en la
/// lista blanca, con su motivo. Los endpoints anónimos que existían en el POS legacy
/// (login, ventas/sync, pos/productos, importacion/test-conexion) se detectan aquí si vuelven.
/// </summary>
public class ApiAuthorizationTests
{
    // Públicos por diseño: el terminal todavía no tiene token cuando los llama
    // (login y auto-configuración del terminal por identificador de hardware).
    private static readonly HashSet<string> PublicosPorDiseno = new()
    {
        "PosAuthController.Login",
        "PosConfigController.GetConfiguracion"
    };

    [Fact]
    public void NingunEndpointApiQuedaPublicoPorDescuido()
    {
        var publicos = new List<string>();
        var sinAutorizacion = new List<string>();
        var inspeccionados = 0;

        foreach (var controlador in typeof(PosApiController).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract))
        {
            foreach (var accion in controlador.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (accion.GetCustomAttribute<NonActionAttribute>() is not null) continue;
                if (!EsEndpointApi(controlador, accion)) continue;

                inspeccionados++;
                var nombre = $"{controlador.Name}.{accion.Name}";

                var anonimo = accion.GetCustomAttribute<AllowAnonymousAttribute>() is not null
                    || controlador.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
                var autorizado = accion.GetCustomAttribute<AuthorizeAttribute>() is not null
                    || controlador.GetCustomAttribute<AuthorizeAttribute>() is not null;

                if (anonimo)
                {
                    if (!PublicosPorDiseno.Contains(nombre)) publicos.Add(nombre);
                }
                else if (!autorizado)
                {
                    sinAutorizacion.Add(nombre);
                }
            }
        }

        Assert.True(inspeccionados >= 20, $"Solo se inspeccionaron {inspeccionados} endpoints api/*; revisar el escaneo.");
        Assert.Empty(publicos);
        Assert.Empty(sinAutorizacion);
    }

    private static bool EsEndpointApi(Type controlador, MethodInfo accion)
    {
        var verbo = accion.GetCustomAttributes().OfType<HttpMethodAttribute>().FirstOrDefault();
        if (verbo is null) return false; // Acción MVC (vista), no endpoint api/*

        var rutaClase = controlador.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
        var ruta = $"{rutaClase}/{verbo.Template}".TrimStart('/');
        return ruta.StartsWith("api/", StringComparison.OrdinalIgnoreCase);
    }
}
