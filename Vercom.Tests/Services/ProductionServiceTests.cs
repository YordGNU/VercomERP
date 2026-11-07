using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;

namespace Vercom.Tests.Services;

public sealed class ProductionServiceTests
{
    [Fact]
    public async Task CreateBomAsync_RejectsEmptyComponentList()
    {
        using var fixture = new ProductionServiceFixture();

        var result = await fixture.Service.CreateBomAsync(new ListaMateriale());

        Assert.False(result.Succeeded);
        Assert.Contains("al menos un insumo", result.Message);
    }

    [Fact]
    public async Task CreateBomAsync_RejectsNonPositiveQuantity()
    {
        using var fixture = new ProductionServiceFixture();
        var bom = new ListaMateriale();
        bom.ListaMaterialesDetalles.Add(new ListaMaterialesDetalle
        {
            ProductoInsumoId = Guid.NewGuid(),
            CantidadRequerida = 0,
            PorcentajeMerma = 0
        });

        var result = await fixture.Service.CreateBomAsync(bom);

        Assert.False(result.Succeeded);
        Assert.Contains("cantidades deben ser positivas", result.Message);
    }

    [Fact]
    public async Task CreateBomAsync_CreatesAndVersionsActiveBom()
    {
        using var fixture = new ProductionServiceFixture();
        await fixture.SeedProductsAsync();

        var first = await fixture.Service.CreateBomAsync(fixture.CreateBom());
        var second = await fixture.Service.CreateBomAsync(fixture.CreateBom());

        Assert.True(first.Succeeded, first.Message);
        Assert.True(second.Succeeded, second.Message);
        var boms = await fixture.Context.ListaMateriales.OrderBy(b => b.Version).ToListAsync();
        Assert.Equal(new[] { 1, 2 }, boms.Select(b => b.Version));
        Assert.False(boms[0].Activa);
        Assert.True(boms[1].Activa);
    }

    [Fact]
    public async Task CreateCostSheetAsync_RejectsNegativeComponentCost()
    {
        using var fixture = new ProductionServiceFixture();
        var costSheet = new FichaCosto { CostoMateriaPrima = 100, CostoManoObra = -1, GastosIndirectos = 10 };

        var result = await fixture.Service.CreateCostSheetAsync(costSheet);

        Assert.False(result.Succeeded);
        Assert.Null(result.CostSheet);
        Assert.Contains("no pueden ser negativos", result.Message);
    }

    [Theory]
    [InlineData((short)0, (short)1)]
    [InlineData((short)2026, (short)0)]
    [InlineData((short)2026, (short)13)]
    public async Task CreatePlanAsync_RejectsInvalidPeriod(short year, short month)
    {
        using var fixture = new ProductionServiceFixture();

        var result = await fixture.Service.CreatePlanAsync(new PlanProduccion { Anio = year, Mes = month });

        Assert.False(result.Succeeded);
        Assert.Contains("año y un mes válidos", result.Message);
    }

    [Fact]
    public async Task CreatePlanAsync_PersistsDraftAndRejectsDuplicatePeriod()
    {
        using var fixture = new ProductionServiceFixture();
        var plan = new PlanProduccion { Anio = 2026, Mes = 11, Estado = "APROBADO" };

        var created = await fixture.Service.CreatePlanAsync(plan);
        var duplicate = await fixture.Service.CreatePlanAsync(new PlanProduccion { Anio = 2026, Mes = 11 });

        Assert.True(created.Succeeded, created.Message);
        Assert.Equal("BORRADOR", plan.Estado);
        Assert.False(duplicate.Succeeded);
        Assert.Contains("Ya existe", duplicate.Message);
        Assert.Single(await fixture.Context.PlanProduccions.ToListAsync());
    }

    [Fact]
    public async Task CreateMaintenanceAsync_RejectsNegativeCost()
    {
        using var fixture = new ProductionServiceFixture();
        var maintenance = new MantenimientoProgramado { Costo = -0.01m, Tipo = "PREVENTIVO" };

        var result = await fixture.Service.CreateMaintenanceAsync(maintenance);

        Assert.False(result.Succeeded);
        Assert.Contains("no puede ser negativo", result.Message);
    }

    [Fact]
    public async Task CreateWasteAsync_RejectsZeroQuantity()
    {
        using var fixture = new ProductionServiceFixture();

        var result = await fixture.Service.CreateWasteAsync(new Merma { Cantidad = 0 }, Guid.NewGuid());

        Assert.False(result.Succeeded);
        Assert.Contains("mayor a cero", result.Message);
    }

    [Fact]
    public async Task ApprovePlanAsync_RejectsDraftWithoutLines()
    {
        using var fixture = new ProductionServiceFixture();
        var entity = new Entidad
        {
            Id = fixture.EntityId,
            RazonSocial = "Entidad de prueba",
            Nit = "TEST",
            FormaJuridica = "SURL",
            DireccionLegal = "Dirección de prueba",
            MonedaBase = "CUP",
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        fixture.Context.Entidads.Add(entity);
        fixture.Context.PlanProduccions.Add(new PlanProduccion
        {
            Id = fixture.PlanId,
            EntidadId = fixture.EntityId,
            Anio = 2026,
            Mes = 11,
            Estado = "BORRADOR",
            CreadoEn = DateTimeOffset.UtcNow,
            Entidad = entity
        });
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.ApprovePlanAsync(fixture.PlanId);

        Assert.False(result.Succeeded);
        Assert.Contains("al menos un producto", result.Message);
        var savedPlan = await fixture.Context.PlanProduccions.SingleAsync(p => p.Id == fixture.PlanId);
        Assert.Equal("BORRADOR", savedPlan.Estado);
    }

    [Fact]
    public async Task ApprovePlanAsync_ApprovesDraftWithValidProduct()
    {
        using var fixture = new ProductionServiceFixture();
        await fixture.SeedProductsAsync();
        var entity = await fixture.Context.Entidads.SingleAsync(e => e.Id == fixture.EntityId);
        var finishedProduct = await fixture.Context.Productos.SingleAsync(p => p.Id == fixture.FinishedProductId);
        fixture.Context.PlanProduccions.Add(new PlanProduccion
        {
            Id = fixture.PlanId,
            EntidadId = fixture.EntityId,
            Anio = 2026,
            Mes = 11,
            Estado = "BORRADOR",
            CreadoEn = DateTimeOffset.UtcNow,
            Entidad = entity,
            PlanProduccionDetalles =
            {
                new PlanProduccionDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = fixture.FinishedProductId,
                    CantidadPlanificada = 12,
                    Producto = finishedProduct
                }
            }
        });
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.ApprovePlanAsync(fixture.PlanId);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("APROBADO", (await fixture.Context.PlanProduccions.SingleAsync()).Estado);
    }

    [Fact]
    public async Task ExecuteMaintenanceAsync_UpdatesMaintenanceAndEquipmentRevision()
    {
        using var fixture = new ProductionServiceFixture();
        var equipmentId = Guid.NewGuid();
        fixture.Context.Equipos.Add(new Equipo
        {
            Id = equipmentId,
            EntidadId = fixture.EntityId,
            Codigo = "EQ-1",
            Nombre = "Equipo de prueba",
            Estado = "OPERATIVO"
        });
        var maintenance = new MantenimientoProgramado
        {
            Id = Guid.NewGuid(),
            EquipoId = equipmentId,
            FechaProgramada = new DateOnly(2026, 11, 1),
            Tipo = "PREVENTIVO",
            Estado = "PROGRAMADO"
        };
        fixture.Context.MantenimientoProgramados.Add(maintenance);
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.ExecuteMaintenanceAsync(maintenance.Id, new DateOnly(2026, 11, 3));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("EJECUTADO", maintenance.Estado);
        Assert.Equal(new DateOnly(2026, 11, 3), maintenance.Equipo.FechaUltimaRevision);
    }
    [Fact]
    public async Task CreateEquipmentAsync_RejectsEmptyCode()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateEquipmentAsync(new Equipo { Codigo = "", Nombre = "Equipo" });
        Assert.False(result.Succeeded);
        Assert.Contains("código", result.Message);
    }

    [Fact]
    public async Task CreateEquipmentAsync_RejectsDuplicateCode()
    {
        using var fixture = new ProductionServiceFixture();
        var first = await fixture.Service.CreateEquipmentAsync(new Equipo { Codigo = "EQ-1", Nombre = "Equipo 1" });
        Assert.True(first.Succeeded, first.Message);
        var second = await fixture.Service.CreateEquipmentAsync(new Equipo { Codigo = "EQ-1", Nombre = "Equipo 2" });
        Assert.False(second.Succeeded);
        Assert.Contains("Ya existe", second.Message);
    }

    [Fact]
    public async Task CreateEquipmentAsync_RejectsNonPositiveFrequency()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateEquipmentAsync(new Equipo { Codigo = "EQ-9", Nombre = "Equipo", FrecuenciaMantenimientoDias = 0 });
        Assert.False(result.Succeeded);
        Assert.Contains("frecuencia", result.Message);
    }

    [Theory]
    [InlineData("Operativo", "OPERATIVO")]
    [InlineData("operativo", "OPERATIVO")]
    [InlineData("Mantenimiento", "MANTENIMIENTO")]
    public async Task CreateEquipmentAsync_NormalizesEstado(string input, string expected)
    {
        using var fixture = new ProductionServiceFixture();
        var equipo = new Equipo { Codigo = "EQ-EST", Nombre = "Equipo", Estado = input };
        var result = await fixture.Service.CreateEquipmentAsync(equipo);
        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(expected, equipo.Estado);
    }

    [Fact]
    public async Task CreateBudgetAsync_RejectsInvalidYear()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateBudgetAsync(new Presupuesto { Anio = 1999, Nombre = "Presupuesto" });
        Assert.False(result.Succeeded);
        Assert.Contains("año", result.Message);
    }

    [Fact]
    public async Task CreateProductionOrderAsync_RejectsUnknownWarehouse()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateProductionOrderAsync(new OrdenProduccion
        {
            EntidadId = fixture.EntityId,
            ProductoTerminadoId = Guid.NewGuid(),
            AlmacenInsumosId = Guid.NewGuid(),
            AlmacenProductoId = Guid.NewGuid(),
            CantidadPlanificada = 1
        });
        Assert.False(result.Succeeded);
        Assert.Contains("producto terminado", result.Message);
    }

    [Fact]
    public async Task CreateProductionOrderAsync_RejectsInvertedDates()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateProductionOrderAsync(new OrdenProduccion
        {
            EntidadId = fixture.EntityId,
            ProductoTerminadoId = Guid.NewGuid(),
            AlmacenInsumosId = Guid.NewGuid(),
            AlmacenProductoId = Guid.NewGuid(),
            CantidadPlanificada = 1,
            FechaInicioPlan = new DateOnly(2026, 11, 20),
            FechaFinPlan = new DateOnly(2026, 11, 10)
        });
        Assert.False(result.Succeeded);
        Assert.Contains("fecha fin", result.Message);
    }

    [Fact]
    public async Task CreateWasteAsync_RejectsInvalidCausa()
    {
        using var fixture = new ProductionServiceFixture();
        var result = await fixture.Service.CreateWasteAsync(new Merma { Cantidad = 1, Causa = "INVALIDA" }, Guid.NewGuid());
        Assert.False(result.Succeeded);
        Assert.Contains("causa", result.Message);
    }
}

internal sealed class ProductionServiceFixture : IDisposable
{
    public Guid EntityId { get; } = Guid.NewGuid();
    public Guid PlanId { get; } = Guid.NewGuid();
    public Guid FinishedProductId { get; } = Guid.NewGuid();
    public Guid MaterialProductId { get; } = Guid.NewGuid();
    public AppDbContext Context { get; }
    public ProductionService Service { get; }

    public ProductionServiceFixture()
    {
        var entityProvider = new Mock<IEntidadProvider>();
        entityProvider.SetupGet(p => p.CurrentEntidadId).Returns(EntityId);
        entityProvider.SetupGet(p => p.CurrentUsuarioId).Returns(Guid.NewGuid());
        entityProvider.SetupGet(p => p.IsMaster).Returns(false);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        Context = new AppDbContext(options, entityProvider.Object);

        Service = new ProductionService(
            Context,
            Mock.Of<IInventoryService>(),
            Mock.Of<IAccountingService>(),
            Mock.Of<IParametroSistemaService>(),
            entityProvider.Object,
            Mock.Of<IConsecutivoService>());
    }

    public void Dispose() => Context.Dispose();

    public Entidad CreateEntity() => new()
    {
        Id = EntityId,
        RazonSocial = "Entidad de prueba",
        Nit = "TEST",
        FormaJuridica = "SURL",
        DireccionLegal = "Dirección de prueba",
        MonedaBase = "CUP",
        Activo = true,
        CreadoEn = DateTimeOffset.UtcNow,
        ActualizadoEn = DateTimeOffset.UtcNow
    };

    public async Task SeedProductsAsync()
    {
        var entity = await Context.Entidads.FindAsync(EntityId);
        if (entity == null)
        {
            entity = CreateEntity();
            Context.Entidads.Add(entity);
        }

        Context.Productos.AddRange(
            new Producto
            {
                Id = FinishedProductId,
                EntidadId = EntityId,
                Codigo = "PT-1",
                Nombre = "Producto terminado",
                Tipo = "TERMINADO",
                UnidadMedidaId = 1,
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
                ActualizadoEn = DateTimeOffset.UtcNow,
                Entidad = entity
            },
            new Producto
            {
                Id = MaterialProductId,
                EntidadId = EntityId,
                Codigo = "MP-1",
                Nombre = "Materia prima",
                Tipo = "MATERIA_PRIMA",
                UnidadMedidaId = 1,
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
                ActualizadoEn = DateTimeOffset.UtcNow,
                Entidad = entity
            });
        await Context.SaveChangesAsync();
    }

    public ListaMateriale CreateBom()
    {
        var finishedProduct = Context.Productos.Local.Single(p => p.Id == FinishedProductId);
        var materialProduct = Context.Productos.Local.Single(p => p.Id == MaterialProductId);
        var bom = new ListaMateriale { ProductoTerminadoId = FinishedProductId, ProductoTerminado = finishedProduct };
        bom.ListaMaterialesDetalles.Add(new ListaMaterialesDetalle
        {
            ProductoInsumoId = MaterialProductId,
            CantidadRequerida = 2,
            PorcentajeMerma = 1,
            ProductoInsumo = materialProduct
        });
        return bom;
    }
}
