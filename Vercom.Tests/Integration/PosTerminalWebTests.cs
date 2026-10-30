using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VERCOM.Helpers;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using Vercom.Tests.Infrastructure;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Terminal POS web del ERP: token emitido desde la sesión de cookie (sin contraseña),
/// configuración por dispositivo y numeración de facturas asignada por el servidor.
/// </summary>
public class PosTerminalWebTests
{
    private static PosAuthService CrearAuth(VERCOM.Models.AppDbContext db)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "clave-de-pruebas-con-mas-de-32-bytes-1234567890",
                ["Jwt:Issuer"] = "vercomERP",
                ["Jwt:Audience"] = "vercomERPClientes",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        return new PosAuthService(db, configuration);
    }

    private static PosSincronizacionService CrearSync(VERCOM.Models.AppDbContext db, IEntidadProvider provider)
    {
        var parametros = new ParametroSistemaService(db);
        var contabilidad = new AccountingService(db, null!, provider, new ConsecutivoService(db));
        var inventario = new InventoryService(db, contabilidad, parametros, provider, NullLogger<InventoryService>.Instance);

        return new PosSincronizacionService(db, new NotificacionNula(), inventario, contabilidad, parametros,
            NullLogger<PosSincronizacionService>.Instance, provider, new ConsecutivoService(db));
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaConfiguracionDelTerminalUsaLaSerieParametrizada()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var device = await db.DispositivoPos.AsNoTracking().FirstAsync(d => d.Id == sesion.DispositivoPosId);
        var config = new PosConfigService(db, new ParametroSistemaService(db));

        var inicial = await config.GetConfiguracionPorDispositivoAsync(sesion.DispositivoPosId, device.EntidadId, CancellationToken.None);
        Assert.NotNull(inicial);
        Assert.False(string.IsNullOrWhiteSpace(inicial!.Serie));

        // POS_SERIE manda en la serie que usa el terminal para numerar.
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var parametro = await db.ParametroSistemas
            .FirstOrDefaultAsync(p => p.EntidadId == device.EntidadId && p.Codigo == "POS_SERIE" && p.VigenteDesde == hoy);
        if (parametro is null)
        {
            db.ParametroSistemas.Add(new ParametroSistema
            {
                EntidadId = device.EntidadId,
                Codigo = "POS_SERIE",
                Valor = "B",
                TipoDato = "STRING",
                Descripcion = "Serie POS (prueba)",
                VigenteDesde = hoy
            });
        }
        else
        {
            parametro.Valor = "B";
        }
        await db.SaveChangesAsync();

        var conParametro = await config.GetConfiguracionPorDispositivoAsync(sesion.DispositivoPosId, device.EntidadId, CancellationToken.None);
        Assert.Equal("B", conParametro!.Serie);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElTokenDelTerminalWebSeEmiteParaElUsuarioDeLaSesionSinPedirContrasena()
    {
        await using var db = TestDb.Create();
        var usuario = await db.Usuarios.IgnoreQueryFilters().AsNoTracking().FirstAsync(x => x.Id == TestDb.UsuarioIdSeed);

        var result = await CrearAuth(db).IssueForUserAsync(TestDb.UsuarioIdSeed, null, CancellationToken.None);

        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal(usuario.Id, result.Response!.UserId);
        Assert.Equal(usuario.EntidadId, result.Response.EntidadId);
        Assert.Equal(3, result.Response.AccessToken.Split('.').Length);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Response.AccessToken);
        Assert.Contains(token.Claims, c => c.Type == "EntidadId" && c.Value == usuario.EntidadId.ToString());
        Assert.Contains(token.Claims, c => c.Type == "Permission");
    }

    [DbFact]
    public async Task ElTokenDelTerminalWebRechazaUnUsuarioInexistente()
    {
        await using var db = TestDb.Create();

        var result = await CrearAuth(db).IssueForUserAsync(Guid.NewGuid(), null, CancellationToken.None);

        Assert.Null(result.Response);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionDelTerminalEsConsecutivaParaLaSerie()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var sync = CrearSync(db, new TestEntidadProvider(sesion.EntidadId, TestDb.UsuarioIdSeed, isMaster: true));

        var primero = await sync.SiguienteNumeroAsync(sesion.DispositivoPosId, "A", CancellationToken.None);
        var segundo = await sync.SiguienteNumeroAsync(sesion.DispositivoPosId, "A", CancellationToken.None);

        Assert.Null(primero.Error);
        Assert.Null(segundo.Error);
        Assert.False(string.IsNullOrWhiteSpace(primero.Numero));
        Assert.True(long.TryParse(primero.Numero, out var numero1));
        Assert.True(long.TryParse(segundo.Numero, out var numero2));
        Assert.Equal(numero1 + 1, numero2);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionRechazaUnDispositivoDeOtraEntidad()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var sync = CrearSync(db, new TestEntidadProvider(Guid.NewGuid(), TestDb.UsuarioIdSeed));

        var result = await sync.SiguienteNumeroAsync(sesion.DispositivoPosId, "A", CancellationToken.None);

        Assert.Null(result.Numero);
        Assert.NotNull(result.Error);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionRechazaUnDispositivoInexistente()
    {
        await using var db = TestDb.Create();
        var sync = CrearSync(db, new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster: true));

        var result = await sync.SiguienteNumeroAsync(Guid.NewGuid(), "A", CancellationToken.None);

        Assert.Null(result.Numero);
        Assert.NotNull(result.Error);
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task LaNumeracionSaleDelBloqueReservadoAlDispositivoYNoDelConsecutivo()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);
        var device = await db.DispositivoPos.IgnoreQueryFilters().FirstAsync(x => x.Id == sesion.DispositivoPosId);
        var cliente = await db.Clientes.IgnoreQueryFilters().OrderBy(c => c.Id).FirstAsync();

        // Serie propia de la prueba: la BD de desarrollo puede tener facturas POS reales
        // en otras series y la numeración dentro del bloque se calcula sobre la serie.
        var serie = "N" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        db.PosRangoNumeracions.Add(new PosRangoNumeracion
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = sesion.DispositivoPosId,
            TipoDocumento = DocumentoTipo.FacturaVenta,
            Serie = serie,
            NumeroDesde = 1,
            NumeroHasta = 1000,
            NumeroSiguienteLocal = 1,
            AsignadoEn = DateTimeOffset.UtcNow,
            Agotado = false
        });

        // "SMK-9001" convive en la tabla real: la numeración no numérica no puede romper la consulta.
        db.FacturaVenta.AddRange(
            FacturaDePrueba(sesion, device, cliente, serie, "SMK-9001"),
            FacturaDePrueba(sesion, device, cliente, serie, "00002"));
        await db.SaveChangesAsync();

        var sync = CrearSync(db, new TestEntidadProvider(sesion.EntidadId, TestDb.UsuarioIdSeed, isMaster: true));

        var siguiente = await sync.SiguienteNumeroAsync(sesion.DispositivoPosId, serie, CancellationToken.None);
        Assert.Null(siguiente.Error);
        Assert.Equal("0003", siguiente.Numero);

        var bloque = await db.PosRangoNumeracions.IgnoreQueryFilters().FirstAsync(r => r.DispositivoPosId == sesion.DispositivoPosId);
        bloque.NumeroSiguienteLocal = 1000;
        await db.SaveChangesAsync();

        var agotado = await sync.SiguienteNumeroAsync(sesion.DispositivoPosId, serie, CancellationToken.None);
        Assert.Null(agotado.Numero);
        Assert.NotNull(agotado.Error);

        await tx.RollbackAsync();
    }

    private static FacturaVentum FacturaDePrueba(PosFixture.SesionPos sesion, DispositivoPo device, Cliente cliente, string serie, string numero) => new()
    {
        Id = Guid.NewGuid(),
        EntidadId = sesion.EntidadId,
        SucursalId = device.SucursalId,
        AlmacenId = device.AlmacenId,
        ClienteId = cliente.Id,
        NumeroFactura = numero,
        Serie = serie,
        CanalVenta = "POS",
        TipoVenta = "MINORISTA",
        Fecha = DateTimeOffset.UtcNow,
        Moneda = "CUP",
        Estado = "EMITIDA",
        CreadoPor = sesion.CajeroId,
        CreadoEn = DateTimeOffset.UtcNow
    };
}
