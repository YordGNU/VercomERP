using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VERCOM.DTOs;
using VERCOM.Models;
using VERCOM.Services;
using Vercom.Tests.Infrastructure;
using Xunit;
using BC = BCrypt.Net.BCrypt;

namespace Vercom.Tests.Integration;

/// <summary>
/// Endurecimiento del login del terminal POS (API JWT): bloqueo por intentos fallidos,
/// mensajes que no delatan si el usuario existe, registro de la última conexión y
/// revocación de tokens cuando la cuenta se desactiva o cambia su contraseña.
/// </summary>
public class PosAuthSecurityTests
{
    private const string Clave = "ClavePos2026*";

    private static PosAuthService Crear(AppDbContext db) => new(db, Configuracion());

    private static IConfiguration Configuracion() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "clave-de-pruebas-con-mas-de-32-bytes-1234567890",
            ["Jwt:Issuer"] = "vercomERP",
            ["Jwt:Audience"] = "vercomERPClientes",
            ["Jwt:ExpirationMinutes"] = "60",
            ["Pos:MaxLoginAttempts"] = "5",
            ["Pos:LockoutMinutes"] = "15"
        })
        .Build();

    private static async Task<Usuario> CrearUsuarioAsync(AppDbContext db, bool activo = true)
    {
        var sufijo = Guid.NewGuid().ToString("N")[..10];
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = TestDb.EntidadIdSeed,
            NombreUsuario = $"pos-{sufijo}",
            NombreCompleto = $"Cajero {sufijo}",
            HashPassword = BC.HashPassword(Clave),
            Activo = activo,
            DebeCambiarPass = false,
            IntentosFallidos = 0,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    private static Task<(PosLoginResponse? Response, string? Error)> Login(PosAuthService auth, string usuario, string clave) =>
        auth.LoginAsync(new VERCOM.DTOs.PosLoginRequest { Username = usuario, Password = clave }, CancellationToken.None);

    [DbFact(RequiereDatosBase = true)]
    public async Task ElLoginCorrectoEmiteTokenYRegistraLaUltimaConexion()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var auth = Crear(db);

        var result = await Login(auth, usuario.NombreUsuario, Clave);

        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.False(string.IsNullOrWhiteSpace(result.Response!.AccessToken));

        await db.Entry(usuario).ReloadAsync();
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.NotNull(usuario.UltimoLogin);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task CincoIntentosFallidosBloqueanLaCuenta()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var auth = Crear(db);

        for (var i = 0; i < 5; i++)
        {
            var fallo = await Login(auth, usuario.NombreUsuario, "clave-incorrecta");
            Assert.Null(fallo.Response);
            Assert.Equal("Credenciales inválidas.", fallo.Error);
        }

        await db.Entry(usuario).ReloadAsync();
        Assert.NotNull(usuario.BloqueadoHasta);
        Assert.True(usuario.BloqueadoHasta > DateTimeOffset.UtcNow);
        Assert.Equal(0, usuario.IntentosFallidos);

        // Aun con la contraseña correcta, el bloqueo impide emitir el token.
        var bloqueado = await Login(auth, usuario.NombreUsuario, Clave);
        Assert.Null(bloqueado.Response);
        Assert.Contains("bloqueada", bloqueado.Error);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task UnLoginCorrectoReiniciaElContadorDeIntentos()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var auth = Crear(db);

        for (var i = 0; i < 4; i++)
            await Login(auth, usuario.NombreUsuario, "clave-incorrecta");

        var ok = await Login(auth, usuario.NombreUsuario, Clave);
        Assert.Null(ok.Error);

        await db.Entry(usuario).ReloadAsync();
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElMensajeDeErrorNoDelataSiElUsuarioExiste()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var auth = Crear(db);

        var inexistente = await Login(auth, "usuario-que-no-existe", Clave);
        var claveMala = await Login(auth, usuario.NombreUsuario, "otra-clave-mala");

        Assert.Equal(inexistente.Error, claveMala.Error);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task UnUsuarioInactivoNoObtieneToken()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db, activo: false);

        var result = await Login(Crear(db), usuario.NombreUsuario, Clave);

        Assert.Null(result.Response);
        Assert.Equal("Credenciales inválidas.", result.Error);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElTokenIncluyeIdentificadorUnicoYHuellaDeContrasena()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);

        var result = await Login(Crear(db), usuario.NombreUsuario, Clave);
        Assert.NotNull(result.Response);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Response!.AccessToken);
        var jti = Assert.Single(token.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.False(string.IsNullOrWhiteSpace(jti.Value));

        var huella = Assert.Single(token.Claims, c => c.Type == PosTokenValidator.PasswordStampClaim);
        Assert.Equal(PosTokenValidator.Stamp(usuario.HashPassword), huella.Value);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElTokenDejaDeSerValidoSiLaCuentaSeDesactiva()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var validador = new PosTokenValidator(db);

        Assert.True(await validador.IsValidAsync(usuario.Id, PosTokenValidator.Stamp(usuario.HashPassword)));

        usuario.Activo = false;
        await db.SaveChangesAsync();

        Assert.False(await validador.IsValidAsync(usuario.Id, PosTokenValidator.Stamp(usuario.HashPassword)));

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElTokenDejaDeSerValidoSiCambiaLaContrasena()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var usuario = await CrearUsuarioAsync(db);
        var huellaEmitida = PosTokenValidator.Stamp(usuario.HashPassword);
        var validador = new PosTokenValidator(db);

        usuario.HashPassword = BC.HashPassword("NuevaClave2026*");
        await db.SaveChangesAsync();

        Assert.False(await validador.IsValidAsync(usuario.Id, huellaEmitida));
        Assert.True(await validador.IsValidAsync(usuario.Id, PosTokenValidator.Stamp(usuario.HashPassword)));

        await tx.RollbackAsync();
    }

    [DbFact]
    public async Task ElValidadorRechazaTokensSinHuellaOUsuarioDesconocido()
    {
        await using var db = TestDb.Create();
        var validador = new PosTokenValidator(db);

        Assert.False(await validador.IsValidAsync(Guid.NewGuid(), "0123456789ABCDEF"));
        Assert.False(await validador.IsValidAsync(Guid.NewGuid(), null));
        Assert.False(await validador.IsValidAsync(Guid.Empty, "0123456789ABCDEF"));
    }
}
