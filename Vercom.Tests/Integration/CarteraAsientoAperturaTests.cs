using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Hueco #5 de AGENTS.md: las cuentas por cobrar/pagar dadas de alta a mano
/// quedaban registradas en cartera sin asiento contable, así que el balance no
/// reflejaba la deuda. El alta debe generar un asiento de apertura cuadrado y
/// quedar enlazada por <c>AsientoOrigenId</c>.
/// </summary>
public class CarteraAsientoAperturaTests
{
    private static ReceivablesPayablesService CrearServicio(AppDbContext db)
    {
        var provider = new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster: true);
        var accounting = new AccountingService(db, null!, provider, new ConsecutivoService(db));
        return new ReceivablesPayablesService(db, accounting, new ParametroSistemaService(db));
    }

    private static async Task<Guid> CuentaPorCodigoAsync(AppDbContext db, string codigoParametro, string codigoPorDefecto)
    {
        var codigo = await new ParametroSistemaService(db).ObtenerValorVigenteAsync(TestDb.EntidadIdSeed, codigoParametro)
                     ?? codigoPorDefecto;
        return await db.CuentaContables.AsNoTracking()
            .Where(c => c.Codigo == codigo && c.EntidadId == TestDb.EntidadIdSeed)
            .Select(c => c.Id)
            .FirstAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElAltaManualDeUnaCxCGeneraAsientoDeAperturaCuadrado()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var cliente = await db.Clientes.AsNoTracking().OrderBy(c => c.NombreRazonSocial).FirstAsync();
        var item = new CuentaPorCobrar
        {
            ClienteId = cliente.Id,
            DocumentoOrigenTipo = "MANUAL",
            DocumentoOrigenNumero = "MAN-TEST-CXC",
            FechaEmision = DateOnly.FromDateTime(DateTime.Today),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            MontoOriginal = 1500m,
            Moneda = "CUP"
        };

        var (exito, mensaje) = await CrearServicio(db).CreateCxCAsync(item, TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed);
        Assert.True(exito, mensaje);

        Assert.NotNull(item.AsientoOrigenId);
        var asiento = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.Id == item.AsientoOrigenId);

        var debe = asiento.AsientoDetalles.Sum(d => d.Debe);
        var haber = asiento.AsientoDetalles.Sum(d => d.Haber);
        Assert.Equal(debe, haber);
        Assert.Equal(1500m, debe);
        Assert.True(asiento.AsientoDetalles.Count >= 2, "El asiento debe tener al menos dos líneas.");
        Assert.Equal("CONTABILIZADO", asiento.Estado);
        Assert.Equal(item.Id, asiento.DocumentoOrigenId);

        // El DEBE tiene que incrementar la cuenta de cuentas por cobrar.
        var ctaCxC = await CuentaPorCodigoAsync(db, "CTA_CXC_CLIENTES", "135");
        Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == ctaCxC && d.Debe == 1500m);

        // El saldo de cartera sigue siendo el monto original: la contabilidad lo acompaña, no lo duplica.
        Assert.Equal(1500m, item.SaldoPendiente);
        Assert.Equal("PENDIENTE", item.Estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElCambioDeImporteDeUnaCxCContabilizadaRevierteYRepostaElAsiento()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var servicio = CrearServicio(db);
        var cliente = await db.Clientes.AsNoTracking().OrderBy(c => c.NombreRazonSocial).FirstAsync();
        var item = new CuentaPorCobrar
        {
            ClienteId = cliente.Id,
            DocumentoOrigenTipo = "MANUAL",
            DocumentoOrigenNumero = "MAN-TEST-EDIT",
            FechaEmision = DateOnly.FromDateTime(DateTime.Today),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            MontoOriginal = 1500m,
            Moneda = "CUP"
        };

        Assert.True((await servicio.CreateCxCAsync(item, TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed)).Succeeded);
        var asientoOriginalId = item.AsientoOrigenId!.Value;

        // Se corrige el importe: el asiento de apertura ya no puede quedar en 1500.
        item.MontoOriginal = 2000m;
        var (exito, mensaje) = await servicio.UpdateCxCAsync(item, TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed);
        Assert.True(exito, mensaje);

        // El asiento viejo se revierte (no se borra) y la reversión lo anula.
        var original = await db.AsientoContables.AsNoTracking().FirstAsync(a => a.Id == asientoOriginalId);
        Assert.Equal("REVERTIDO", original.Estado);
        Assert.NotNull(original.AsientoReversionId);

        var reversion = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.AsientoReversionId == asientoOriginalId);
        Assert.Equal(1500m, reversion.AsientoDetalles.Sum(d => d.Debe));
        Assert.Equal(1500m, reversion.AsientoDetalles.Sum(d => d.Haber));

        // Y se reposta por el importe nuevo, que es el que queda enlazado al documento.
        Assert.NotEqual(asientoOriginalId, item.AsientoOrigenId);
        var repuesto = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.Id == item.AsientoOrigenId);
        Assert.Equal("CONTABILIZADO", repuesto.Estado);
        Assert.Equal(2000m, repuesto.AsientoDetalles.Sum(d => d.Debe));
        Assert.Equal(2000m, repuesto.AsientoDetalles.Sum(d => d.Haber));
        Assert.Equal(2000m, item.MontoOriginal);
        Assert.Equal(2000m, item.SaldoPendiente);

        // Efecto neto en la cuenta de clientes: apertura (1500) + reversión (-1500) + repuesto (2000) = 2000.
        var ctaCxC = await CuentaPorCodigoAsync(db, "CTA_CXC_CLIENTES", "135");
        var ids = new[] { asientoOriginalId, reversion.Id, repuesto.Id };
        var neto = await db.AsientoDetalles.AsNoTracking()
            .Where(d => d.CuentaId == ctaCxC && ids.Contains(d.AsientoId))
            .SumAsync(d => d.Debe - d.Haber);
        Assert.Equal(2000m, neto);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task ElAltaManualDeUnaCxPGeneraAsientoDeAperturaCuadrado()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var proveedor = await db.Proveedors.AsNoTracking().OrderBy(p => p.RazonSocial).FirstAsync();
        var item = new CuentaPorPagar
        {
            ProveedorId = proveedor.Id,
            DocumentoOrigenTipo = "MANUAL",
            DocumentoOrigenNumero = "MAN-TEST-CXP",
            FechaEmision = DateOnly.FromDateTime(DateTime.Today),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            MontoOriginal = 800m,
            Moneda = "CUP"
        };

        var (exito, mensaje) = await CrearServicio(db).CreateCxPAsync(item, TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed);
        Assert.True(exito, mensaje);

        Assert.NotNull(item.AsientoOrigenId);
        var asiento = await db.AsientoContables.AsNoTracking()
            .Include(a => a.AsientoDetalles)
            .FirstAsync(a => a.Id == item.AsientoOrigenId);

        Assert.Equal(asiento.AsientoDetalles.Sum(d => d.Debe), asiento.AsientoDetalles.Sum(d => d.Haber));
        Assert.Equal(800m, asiento.AsientoDetalles.Sum(d => d.Debe));
        Assert.Equal("CONTABILIZADO", asiento.Estado);

        // El HABER tiene que incrementar la cuenta de cuentas por pagar.
        var ctaCxP = await CuentaPorCodigoAsync(db, "CTA_CXP_PROVEEDORES", "405");
        Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == ctaCxP && d.Haber == 800m);

        await tx.RollbackAsync();
    }
}
