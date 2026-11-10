using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Controllers;
using Xunit;

namespace Vercom.Tests.Security;

/// <summary>
/// Verificación estática de autorización y protección CSRF en el módulo de
/// Producción: cada acción pública debe declarar una policy PRODUCCION.* y
/// cada acción POST debe llevar [ValidateAntiForgeryToken].
/// </summary>
public class ProductionAuthorizationTests
{
    private static readonly Type[] Controladores =
    {
        typeof(ProductionController),
        typeof(PlanProduccionController),
        typeof(FichaCostoController),
        typeof(ListaMaterialeController),
        typeof(MermaController),
        typeof(MantenimientoProgramadoController)
    };

    private static IEnumerable<MethodInfo> Acciones()
    {
        foreach (var controlador in Controladores)
        {
            foreach (var metodo in controlador.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var tipoRetorno = metodo.ReturnType;
                if (tipoRetorno.IsGenericType && tipoRetorno.GetGenericTypeDefinition() == typeof(Task<>))
                    tipoRetorno = tipoRetorno.GetGenericArguments()[0];
                if (typeof(IActionResult).IsAssignableFrom(tipoRetorno))
                    yield return metodo;
            }
        }
    }

    [Fact]
    public void TodasLasAccionesDeProduccion_TienenPolicyProduccion()
    {
        var sinPolicy = Acciones()
            .Where(m =>
            {
                var authorize = m.GetCustomAttributes<AuthorizeAttribute>(inherit: false).FirstOrDefault();
                return authorize?.Policy == null || !authorize.Policy.StartsWith("PRODUCCION.", StringComparison.Ordinal);
            })
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
            .ToList();
        Assert.Empty(sinPolicy);
    }

    [Fact]
    public void TodasLasAccionesPostDeProduccion_TienenAntiforgery()
    {
        var sinAntiforgery = Acciones()
            .Where(m => m.GetCustomAttributes<HttpPostAttribute>(inherit: false).Any())
            .Where(m => !m.GetCustomAttributes<ValidateAntiForgeryTokenAttribute>(inherit: false).Any())
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
            .ToList();
        Assert.Empty(sinAntiforgery);
    }

    [Fact]
    public void ElModuloDeProduccion_TieneAccionesCubiertas()
    {
        var acciones = Acciones().ToList();
        Assert.True(acciones.Count >= 30, $"Se esperaban al menos 30 acciones, hay {acciones.Count}.");
    }
}
