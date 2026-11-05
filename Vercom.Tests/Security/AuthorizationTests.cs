using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VERCOM.Security;
using Xunit;

namespace Vercom.Tests.Security;

public class PermissionPolicyProviderTests
{
    private static PermissionPolicyProvider CrearProvider() =>
        new(Options.Create(new AuthorizationOptions()));

    [Theory]
    [InlineData("INVENTARIO.PRODUCTO.CREAR")]
    [InlineData("CONTABILIDAD.CUENTA.ELIMINAR")]
    [InlineData("POS_GESTIONAR_USUARIOS")]
    [InlineData("RRHH.NOMINA.VER")]
    public async Task UnaPolicyConFormatoDePermisoSeConvierteEnPermissionRequirement(string policyName)
    {
        var policy = await CrearProvider().GetPolicyAsync(policyName);

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy!.Requirements);
        var permiso = Assert.IsType<PermissionRequirement>(requirement);
        Assert.Equal(policyName, permiso.Permission);
    }

    [Theory]
    [InlineData("pos_gestionar_usuarios")]
    [InlineData("Gestionar")]
    [InlineData("AdminOnly")]
    public async Task UnaPolicySinFormatoDePermisoDelegaEnElFallback(string policyName)
    {
        // El fallback (DefaultAuthorizationPolicyProvider) devuelve null si la
        // policy no fue registrada explícitamente en AuthorizationOptions.
        var policy = await CrearProvider().GetPolicyAsync(policyName);

        Assert.Null(policy);
    }

    [Fact]
    public async Task LaPolicyPorDefectoSeDelegaAlFallback()
    {
        var provider = CrearProvider();

        var porDefecto = await provider.GetDefaultPolicyAsync();

        Assert.NotNull(porDefecto);
        Assert.Same(porDefecto, await provider.FallbackPolicyProvider.GetDefaultPolicyAsync());
    }
}

public class PermissionHandlerTests
{
    private const string Permiso = "INVENTARIO.PRODUCTO.CREAR";

    private static ClaimsPrincipal Usuario(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Test"));

    private static AuthorizationHandlerContext Contexto(ClaimsPrincipal usuario) =>
        new(new IAuthorizationRequirement[] { new PermissionRequirement(Permiso) }, usuario, resource: null);

    private static Claim PermisoClaim(string valor = Permiso) =>
        new("Permission", valor);

    [Fact]
    public async Task ElRolMasterPasaSinNecesidadDeClaim()
    {
        var ctx = Contexto(Usuario(new Claim(ClaimTypes.Role, "MASTER")));

        await new PermissionHandler().HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task ElRolAdministradorPasaSinNecesidadDeClaim()
    {
        var ctx = Contexto(Usuario(new Claim(ClaimTypes.Role, "ADMINISTRADOR")));

        await new PermissionHandler().HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task ElClaimDePermisoExactoPasa()
    {
        var ctx = Contexto(Usuario(PermisoClaim()));

        await new PermissionHandler().HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task UnClaimDeOtroPermisoNoPasa()
    {
        var ctx = Contexto(Usuario(PermisoClaim("VENTAS.FACTURA.CREAR")));

        await new PermissionHandler().HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
        Assert.NotEmpty(ctx.PendingRequirements);
    }

    [Fact]
    public async Task SinRolNiClaimNoPasa()
    {
        var ctx = Contexto(Usuario(new Claim(ClaimTypes.Name, "vendedor")));

        await new PermissionHandler().HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
    }
}
