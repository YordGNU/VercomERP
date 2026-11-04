using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VERCOM.Helpers;
using VERCOM.Models;
using VERCOM.ViewModels;

namespace VERCOM.Services;

public interface IProductionService
{
    // Lectura Órdenes
    Task<IEnumerable<OrdenProduccion>> GetOrdersAsync();
    Task<OrdenProduccion?> GetOrderByIdAsync(Guid id);
    Task<ProductionOrderViewModel> GetProductionOrderCreateContextAsync(OrdenProduccion? existing = null);
    Task<IEnumerable<AnalisisDesviacion>> GetDeviationsAsync();

    // Gestión de BOM (Lista de Materiales)
    Task<IEnumerable<ListaMateriale>> GetBomsAsync();
    Task<ListaMateriale?> GetBomByIdAsync(Guid id);
    Task<BomCreateViewModel> GetBomCreateContextAsync(ListaMateriale? existing = null);
    Task<(bool Succeeded, string Message)> CreateBomAsync(ListaMateriale bom);
    Task<(bool Succeeded, string Message)> UpdateBomAsync(ListaMateriale bom);

    // Gestión de Equipos
    Task<IEnumerable<Equipo>> GetEquipmentsAsync();
    Task<Equipo?> GetEquipmentByIdAsync(Guid id);
    Task<EquipoFormViewModel> GetEquipmentFormContextAsync(Equipo? existing = null);
    Task<(bool Succeeded, string Message)> CreateEquipmentAsync(Equipo equipment);
    Task<(bool Succeeded, string Message)> UpdateEquipmentAsync(Equipo equipment);

    // Gestión de Mantenimiento
    Task<IEnumerable<MantenimientoProgramado>> GetMaintenancesAsync();
    Task<MantenimientoFormViewModel> GetMaintenanceFormContextAsync(MantenimientoProgramado? existing = null);
    Task<(bool Succeeded, string Message)> CreateMaintenanceAsync(MantenimientoProgramado maintenance);
    Task<(bool Succeeded, string Message)> ExecuteMaintenanceAsync(Guid maintenanceId, DateOnly executionDate);

    // Gestión de Mermas
    Task<IEnumerable<Merma>> GetWastesAsync();
    Task<MermaFormViewModel> GetWasteFormContextAsync(Merma? existing = null);
    Task<(bool Succeeded, string Message)> CreateWasteAsync(Merma waste, Guid almacenId);

    // Gestión de Presupuestos
    Task<IEnumerable<Presupuesto>> GetBudgetsAsync();
    Task<Presupuesto?> GetBudgetByIdAsync(Guid id);
    Task<PresupuestoFormViewModel> GetBudgetFormContextAsync(Presupuesto? existing = null);
    Task<(bool Succeeded, string Message)> CreateBudgetAsync(Presupuesto budget);

    // Planes de Producción
    Task<IEnumerable<PlanProduccion>> GetProductionPlansAsync();
    Task<PlanProduccion?> GetProductionPlanByIdAsync(Guid id);
    Task<PlanProduccionFormViewModel> GetPlanFormContextAsync(PlanProduccion? existing = null);
    Task<(bool Succeeded, string Message)> CreatePlanAsync(PlanProduccion plan);
    Task<(bool Succeeded, string Message)> UpdatePlanAsync(PlanProduccion plan);
    Task<(bool Succeeded, string Message)> ApprovePlanAsync(Guid planId);
    Task<(bool Succeeded, string Message)> CancelPlannedOrderAsync(Guid orderId);
    Task<PlanProduccion> GenerateSuggestedPlanAsync(Guid entidadId, short year, short month);

    // Escritura Producción
    // Gestión de Fichas de Costo (RF-40)
    Task<IEnumerable<FichaCosto>> GetCostSheetsAsync();
    Task<FichaCosto?> GetCostSheetByIdAsync(Guid id);
    Task<CostSheetCreateViewModel> GetCostSheetCreateContextAsync(FichaCosto? existing = null);
    Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> CreateCostSheetAsync(FichaCosto costSheet);
    Task<(bool Succeeded, string Message)> ApproveCostSheetAsync(Guid costSheetId);
    Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> BuildCostSheetFromBomAsync(Guid productId);
    Task<(bool Succeeded, string Message, OrdenProduccion? Order)> CreateProductionOrderAsync(OrdenProduccion order);
    Task<(bool Succeeded, string Message)> StartProductionAndConsumeAsync(Guid orderId, Guid userId);
    Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions, Guid userId);
    Task<(bool Succeeded, string Message)> ReverseProductionOrderAsync(Guid orderId, Guid userId);
    Task<(bool Succeeded, string Message, int OrdersCreated)> GenerateOrdersFromPlanAsync(Guid planId, Guid userId);
    Task<decimal> CalculateStandardCostAsync(Guid productId);
}

public class ProductionService : IProductionService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IConsecutivoService _consecutivoService;

    public ProductionService(AppDbContext context, IInventoryService inventoryService, IAccountingService accountingService, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider, IConsecutivoService consecutivoService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _accountingService = accountingService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
        _consecutivoService = consecutivoService;
    }

    public async Task<IEnumerable<OrdenProduccion>> GetOrdersAsync()
    {
        return await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado)
            .Include(o => o.FichaCosto)
            .Include(o => o.AlmacenInsumos)
            .Include(o => o.AlmacenProducto)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync();
    }

    public async Task<OrdenProduccion?> GetOrderByIdAsync(Guid id)
    {
        return await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.AlmacenInsumos)
            .Include(o => o.AlmacenProducto)
            .Include(o => o.OrdenProduccionConsumos).ThenInclude(c => c.ProductoInsumo).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.FichaCosto)
            .Include(o => o.AsientoTerminado)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<ProductionOrderViewModel> GetProductionOrderCreateContextAsync(OrdenProduccion? existing = null)
    {
        return new ProductionOrderViewModel
        {
            Order = existing ?? new OrdenProduccion { FechaInicioPlan = DateOnly.FromDateTime(DateTime.Now) },
            ProductosElaborados = new SelectList(await _context.Productos
                .Where(p => p.Activo && p.Tipo == "TERMINADO").ToListAsync(), "Id", "Nombre"),
            Almacenes = new SelectList(await _context.Almacens.Where(a => a.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<IEnumerable<AnalisisDesviacion>> GetDeviationsAsync()
    {
        return await _context.AnalisisDesviacions
            .Include(a => a.OrdenProduccion).ThenInclude(o => o.ProductoTerminado)
            .OrderByDescending(a => a.AnalizadoEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListaMateriale>> GetBomsAsync()
    {
        return await _context.ListaMateriales
            .Include(l => l.ProductoTerminado)
            .OrderBy(l => l.ProductoTerminado.Nombre)
            .ToListAsync();
    }

    public async Task<ListaMateriale?> GetBomByIdAsync(Guid id)
    {
        return await _context.ListaMateriales
            .Include(l => l.ProductoTerminado)
            .Include(l => l.ListaMaterialesDetalles).ThenInclude(d => d.ProductoInsumo).ThenInclude(p => p.UnidadMedida)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<BomCreateViewModel> GetBomCreateContextAsync(ListaMateriale? existing = null)
    {
        return new BomCreateViewModel
        {
            Bom = existing ?? new ListaMateriale { Activa = true },
            ProductosElaborados = new SelectList(await _context.Productos
                .Where(p => p.Activo && p.Tipo == "TERMINADO").ToListAsync(), "Id", "Nombre"),
            InsumosDisponibles = new SelectList(await _context.Productos
                .Where(p => p.Activo && p.Tipo == "MATERIA_PRIMA")
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
                .ToListAsync(), "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateBomAsync(ListaMateriale bom)
    {
        var validation = await ValidateBomAsync(bom);
        if (validation != null) return (false, validation);

        var ownsTransaction = _context.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;
        await BeginSavepointAsync();
        try
        {
            await PersistBomVersionAsync(bom);
            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "BOM registrada y activada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string Message)> UpdateBomAsync(ListaMateriale bom)
    {
        var existing = await _context.ListaMateriales
            .Include(l => l.ListaMaterialesDetalles)
            .FirstOrDefaultAsync(l => l.Id == bom.Id);
        if (existing == null) return (false, "No se encontró la versión de BOM.");
        if (existing.ProductoTerminadoId != bom.ProductoTerminadoId)
            return (false, "No se puede cambiar el producto terminado al crear una nueva versión.");

        var validation = await ValidateBomAsync(bom);
        if (validation != null) return (false, validation);

        var ownsTransaction = _context.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;
        await BeginSavepointAsync();
        try
        {
            await PersistBomVersionAsync(bom);
            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Nueva versión de BOM creada y activada; la versión anterior se conserva para el historial.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, ex.Message);
        }
    }

    private async Task<string?> ValidateBomAsync(ListaMateriale bom)
    {
        if (bom.ListaMaterialesDetalles.Count == 0)
            return "La lista de materiales debe incluir al menos un insumo.";
        if (bom.ListaMaterialesDetalles.Any(d => d.CantidadRequerida <= 0 || d.PorcentajeMerma < 0))
            return "Las cantidades deben ser positivas y la merma no puede ser negativa.";
        if (bom.ListaMaterialesDetalles.Select(d => d.ProductoInsumoId).Distinct().Count() != bom.ListaMaterialesDetalles.Count)
            return "Cada producto debe aparecer una sola vez en la lista de materiales.";

        var entidadId = _entidadProvider.CurrentEntidadId;
        if (!await _context.Productos.AnyAsync(p => p.Id == bom.ProductoTerminadoId && p.EntidadId == entidadId && p.Activo && p.Tipo == "TERMINADO"))
            return "El producto terminado no existe, está inactivo o no pertenece a la entidad.";

        var insumosIds = bom.ListaMaterialesDetalles.Select(d => d.ProductoInsumoId).ToList();
        var insumosValidos = await _context.Productos.CountAsync(p => insumosIds.Contains(p.Id) && p.EntidadId == entidadId && p.Activo && p.Tipo == "MATERIA_PRIMA");
        return insumosValidos == insumosIds.Count ? null : "Todos los insumos deben ser materias primas activas de la entidad.";
    }

    private async Task PersistBomVersionAsync(ListaMateriale bom)
    {
        var previous = await _context.ListaMateriales
            .Where(l => l.ProductoTerminadoId == bom.ProductoTerminadoId && l.Activa)
            .ToListAsync();
        foreach (var active in previous) active.Activa = false;

        var latestVersion = await _context.ListaMateriales
            .Where(l => l.ProductoTerminadoId == bom.ProductoTerminadoId)
            .MaxAsync(l => (int?)l.Version) ?? 0;
        bom.Id = Guid.NewGuid();
        bom.Version = latestVersion + 1;
        bom.Activa = true;
        bom.CreadoEn = DateTimeOffset.Now;
        foreach (var detail in bom.ListaMaterialesDetalles)
        {
            detail.Id = Guid.NewGuid();
            detail.ListaMaterialesId = bom.Id;
        }
        _context.ListaMateriales.Add(bom);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FichaCosto>> GetCostSheetsAsync()
    {
        return await _context.FichaCostos
            .Include(f => f.Producto)
            .OrderBy(f => f.Producto.Nombre).ThenByDescending(f => f.Version)
            .ToListAsync();
    }

    public async Task<FichaCosto?> GetCostSheetByIdAsync(Guid id)
    {
        return await _context.FichaCostos
            .Include(f => f.Producto).ThenInclude(p => p.UnidadMedida)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<CostSheetCreateViewModel> GetCostSheetCreateContextAsync(FichaCosto? existing = null)
    {
        return new CostSheetCreateViewModel
        {
            CostSheet = existing ?? new FichaCosto { VigenteDesde = DateOnly.FromDateTime(DateTime.Now), MargenPorcentaje = 20 },
            ProductosElaborados = new SelectList(await _context.Productos
                .Where(p => p.Activo && p.Tipo == "TERMINADO").ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> CreateCostSheetAsync(FichaCosto costSheet)
    {
        if (costSheet.CostoMateriaPrima < 0 || costSheet.CostoManoObra < 0 || costSheet.GastosIndirectos < 0)
            return (false, "Los costos de materia prima, mano de obra y gastos indirectos no pueden ser negativos.", null);

        var entidadId = _entidadProvider.CurrentEntidadId;
        if (!await _context.Productos.AnyAsync(p => p.Id == costSheet.ProductoId && p.EntidadId == entidadId && p.Activo && p.Tipo == "TERMINADO"))
            return (false, "La ficha debe pertenecer a un producto terminado activo de la entidad.", null);

        var previous = await _context.FichaCostos
            .Where(f => f.EntidadId == entidadId && f.ProductoId == costSheet.ProductoId)
            .OrderByDescending(f => f.Version)
            .FirstOrDefaultAsync();

        costSheet.Version = previous == null ? 1 : previous.Version + 1;

        costSheet.Id = Guid.NewGuid();
        costSheet.EntidadId = entidadId;
        costSheet.CreadoPor = _entidadProvider.CurrentUsuarioId;
        costSheet.Estado = "BORRADOR";
        costSheet.VigenteDesde = DateOnly.FromDateTime(DateTime.Now);
        costSheet.CreadoEn = DateTimeOffset.Now;

        costSheet.CostoTotalUnitario = costSheet.CostoMateriaPrima + costSheet.CostoManoObra + costSheet.GastosIndirectos;

        if (costSheet.MargenPorcentaje.HasValue)
        {
            costSheet.PrecioSugerido = costSheet.CostoTotalUnitario * (1 + (costSheet.MargenPorcentaje.Value / 100));
        }

        _context.FichaCostos.Add(costSheet);
        await _context.SaveChangesAsync();

        return (true, "Ficha de costo creada en borrador; requiere aprobación para entrar en vigor.", costSheet);
    }

    public async Task<(bool Succeeded, string Message)> ApproveCostSheetAsync(Guid costSheetId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;
        await BeginSavepointAsync();
        try
        {
            var costSheet = await _context.FichaCostos
                .FirstOrDefaultAsync(f => f.Id == costSheetId && f.EntidadId == _entidadProvider.CurrentEntidadId);
            if (costSheet == null) return (false, "No se encontró la ficha de costo.");
            if (costSheet.Estado != "BORRADOR") return (false, "Solo se pueden aprobar fichas en borrador.");

            var today = DateOnly.FromDateTime(DateTime.Now);
            var previous = await _context.FichaCostos
                .Where(f => f.EntidadId == costSheet.EntidadId && f.ProductoId == costSheet.ProductoId && f.Estado == "VIGENTE")
                .ToListAsync();
            foreach (var active in previous)
            {
                active.Estado = "OBSOLETA";
                active.VigenteHasta = today;
            }

            costSheet.Estado = "VIGENTE";
            costSheet.VigenteDesde = today;
            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Ficha de costo aprobada y puesta en vigor.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"No se pudo aprobar la ficha de costo: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> BuildCostSheetFromBomAsync(Guid productId)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var product = await _context.Productos.FirstOrDefaultAsync(p => p.Id == productId && p.EntidadId == entidadId && p.Activo && p.Tipo == "TERMINADO");
        if (product == null) return (false, "El producto terminado no existe, esta inactivo o no pertenece a la entidad.", null);

        var bom = await _context.ListaMateriales
            .Include(l => l.ListaMaterialesDetalles)
            .FirstOrDefaultAsync(l => l.ProductoTerminadoId == productId && l.Activa);
        if (bom == null || !bom.ListaMaterialesDetalles.Any())
            return (false, "No existe una lista de materiales (BOM) activa con componentes para este producto.", null);

        // Costo MP estandar = suma de (cantidad requerida * costo promedio del insumo).
        decimal costoMateriaPrima = 0m;
        foreach (var item in bom.ListaMaterialesDetalles)
        {
            var insumo = await _context.Productos.FirstOrDefaultAsync(p => p.Id == item.ProductoInsumoId);
            var costoUnitario = 0m;
            if (insumo != null)
            {
                var stock = await _context.Existencia
                    .Where(e => e.ProductoId == insumo.Id)
                    .OrderByDescending(e => e.ActualizadoEn)
                    .FirstOrDefaultAsync();
                costoUnitario = stock?.CostoPromedio ?? 0m;
            }
            costoMateriaPrima += item.CantidadRequerida * (1 + (item.PorcentajeMerma / 100m)) * costoUnitario;
        }
        costoMateriaPrima = Math.Round(costoMateriaPrima, 4);

        var ficha = new FichaCosto
        {
            ProductoId = productId,
            CostoMateriaPrima = costoMateriaPrima,
            CostoManoObra = 0m,
            GastosIndirectos = 0m,
            MargenPorcentaje = null
        };
        return await CreateCostSheetAsync(ficha);
    }

    public async Task<IEnumerable<Equipo>> GetEquipmentsAsync()
    {
        return await _context.Equipos.Include(e => e.Sucursal).OrderBy(e => e.Nombre).ToListAsync();
    }

    public async Task<Equipo?> GetEquipmentByIdAsync(Guid id)
    {
        return await _context.Equipos.FindAsync(id);
    }

    public async Task<EquipoFormViewModel> GetEquipmentFormContextAsync(Equipo? existing = null)
    {
        return new EquipoFormViewModel
        {
            Equipo = existing ?? new Equipo { Estado = "OPERATIVO" },
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre"),
            ActivosFijos = new SelectList(await _context.ActivoFijos.Where(a => a.Estado == "ACTIVO").ToListAsync(), "Id", "CodigoInventario")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateEquipmentAsync(Equipo equipment)
    {
        try
        {
            equipment.Id = Guid.NewGuid();
            equipment.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.Equipos.Add(equipment);
            await _context.SaveChangesAsync();
            return (true, "Equipo registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateEquipmentAsync(Equipo equipment)
    {
        try
        {
            var existing = await _context.Equipos.FindAsync(equipment.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(equipment);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Equipo actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<MantenimientoProgramado>> GetMaintenancesAsync()
    {
        return await _context.MantenimientoProgramados.Include(m => m.Equipo).OrderByDescending(m => m.FechaProgramada).ToListAsync();
    }

    public async Task<MantenimientoFormViewModel> GetMaintenanceFormContextAsync(MantenimientoProgramado? existing = null)
    {
        return new MantenimientoFormViewModel
        {
            Mantenimiento = existing ?? new MantenimientoProgramado { FechaProgramada = DateOnly.FromDateTime(DateTime.Now.AddDays(7)), Estado = "PROGRAMADO", Tipo = "PREVENTIVO" },
            Equipos = new SelectList(await _context.Equipos
                .Where(e => e.EntidadId == _entidadProvider.CurrentEntidadId)
                .OrderBy(e => e.Nombre)
                .ToListAsync(), "Id", "Nombre"),
            Responsables = new SelectList(await _context.Usuarios
                .Where(u => u.EntidadId == _entidadProvider.CurrentEntidadId && u.Activo)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync(), "Id", "NombreCompleto")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateMaintenanceAsync(MantenimientoProgramado maintenance)
    {
        if (maintenance.Costo < 0) return (false, "El costo estimado no puede ser negativo.");
        if (!new[] { "PREVENTIVO", "CORRECTIVO", "REVISIÓN" }.Contains(maintenance.Tipo, StringComparer.OrdinalIgnoreCase))
            return (false, "El tipo de mantenimiento no es válido.");
        if (!await _context.Equipos.AnyAsync(e => e.Id == maintenance.EquipoId && e.EntidadId == _entidadProvider.CurrentEntidadId))
            return (false, "El equipo no existe o pertenece a otra entidad.");

        try
        {
            maintenance.Id = Guid.NewGuid();
            maintenance.Estado = "PROGRAMADO";
            maintenance.FechaEjecutada = null;
            _context.MantenimientoProgramados.Add(maintenance);
            await _context.SaveChangesAsync();
            return (true, "Mantenimiento programado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<Merma>> GetWastesAsync()
    {
        return await _context.Mermas.Include(m => m.Producto).Include(m => m.OrdenProduccion).OrderByDescending(m => m.Fecha).ToListAsync();
    }

    public async Task<MermaFormViewModel> GetWasteFormContextAsync(Merma? existing = null)
    {
        return new MermaFormViewModel
        {
            Merma = existing ?? new Merma { Fecha = DateOnly.FromDateTime(DateTime.Now) },
            OrdenesProduccion = new SelectList(await GetOrdersAsync(), "Id", "NumeroOrden"),
            Productos = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre"),
            Almacenes = new SelectList(await _context.Almacens.Where(a => a.Activo).OrderBy(a => a.Nombre).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateWasteAsync(Merma waste, Guid almacenId)
    {
        if (waste.Cantidad <= 0) return (false, "La cantidad de merma debe ser mayor a cero.");
        var entidadId = _entidadProvider.CurrentEntidadId;
        var product = await _context.Productos.FirstOrDefaultAsync(p => p.Id == waste.ProductoId && p.EntidadId == entidadId && p.Activo);
        if (product == null) return (false, "El producto no existe, está inactivo o pertenece a otra entidad.");
        if (!await _context.Almacens.AnyAsync(a => a.Id == almacenId && a.EntidadId == entidadId && a.Activo))
            return (false, "El almacén no existe, está inactivo o pertenece a otra entidad.");
        if (waste.OrdenProduccionId.HasValue && !await _context.OrdenProduccions.AnyAsync(o => o.Id == waste.OrdenProduccionId && o.EntidadId == entidadId))
            return (false, "La orden asociada no existe o pertenece a otra entidad.");

        var stock = await _context.Existencia.FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.ProductoId == waste.ProductoId);
        if (stock == null || stock.Cantidad < waste.Cantidad)
            return (false, $"Existencia insuficiente para registrar la merma. Disponible: {stock?.Cantidad ?? 0:N4}.");

        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            waste.Id = Guid.NewGuid();
            waste.ValorContable = Math.Round(waste.Cantidad * stock.CostoPromedio, 2, MidpointRounding.AwayFromZero);

            var movement = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("AJUSTE_NEGATIVO"),
                NumeroDocumento = $"MER-{waste.Id:N}"[..16],
                AlmacenOrigenId = almacenId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "MERMA",
                ReferenciaExternaId = waste.Id,
                Canal = "ERP",
                CreadoPor = _entidadProvider.CurrentUsuarioId
            };
            movement.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                ProductoId = waste.ProductoId,
                Cantidad = waste.Cantidad
            });

            var movementResult = await _inventoryService.ProcessMovementAsync(movement);
            if (!movementResult.Succeeded) throw new InvalidOperationException(movementResult.Message);

            waste.AsientoId = movementResult.Movement!.AsientoId;
            _context.Mermas.Add(waste);
            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Merma registrada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"No se pudo registrar la merma: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> ExecuteMaintenanceAsync(Guid maintenanceId, DateOnly executionDate)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            var maintenance = await _context.MantenimientoProgramados
                .Include(m => m.Equipo)
                .FirstOrDefaultAsync(m => m.Id == maintenanceId && m.Equipo.EntidadId == _entidadProvider.CurrentEntidadId);
            if (maintenance == null) return (false, "No se encontró el mantenimiento.");
            if (maintenance.Estado != "PROGRAMADO") return (false, "Solo se pueden ejecutar mantenimientos programados.");
            if (executionDate < maintenance.FechaProgramada) return (false, "La fecha de ejecución no puede ser anterior a la fecha programada.");

            maintenance.Estado = "EJECUTADO";
            maintenance.FechaEjecutada = executionDate;
            maintenance.Equipo.FechaUltimaRevision = executionDate;

            if (maintenance.Costo.HasValue && maintenance.Costo.Value > 0)
            {
                var cuentaGastoCodigo = await _paramService.ObtenerValorVigenteAsync(maintenance.Equipo.EntidadId, "CTA_MANTENIMIENTO") ?? "822.0000";
                var cuentaCajaCodigo = await _paramService.ObtenerValorVigenteAsync(maintenance.Equipo.EntidadId, "CTA_CAJA_MN") ?? "101.0000";
                var cuentaGasto = await _context.CuentaContables.FirstOrDefaultAsync(c => c.EntidadId == maintenance.Equipo.EntidadId && c.Codigo == cuentaGastoCodigo && c.Activo && c.AceptaMovimiento);
                var cuentaCaja = await _context.CuentaContables.FirstOrDefaultAsync(c => c.EntidadId == maintenance.Equipo.EntidadId && c.Codigo == cuentaCajaCodigo && c.Activo && c.AceptaMovimiento);
                if (cuentaGasto == null || cuentaCaja == null)
                    throw new InvalidOperationException("Configure las cuentas CTA_MANTENIMIENTO y CTA_CAJA_MN para contabilizar el mantenimiento.");

                var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "DIA");
                if (tipoComprobante == null)
                    throw new InvalidOperationException("No existe el tipo de comprobante DIA.");

                var entry = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    EntidadId = maintenance.Equipo.EntidadId,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Concepto = $"COSTO DE MANTENIMIENTO {maintenance.Tipo} - {maintenance.Equipo.Codigo}",
                    ModuloOrigen = "PRODUCCION",
                    DocumentoOrigenTipo = "MANTENIMIENTO",
                    DocumentoOrigenId = maintenance.Id,
                    TipoComprobanteId = tipoComprobante.Id,
                    Estado = "CONTABILIZADO",
                    CreadoPor = _entidadProvider.CurrentUsuarioId,
                    CreadoEn = DateTimeOffset.Now
                };
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaGasto.Id, Debe = maintenance.Costo.Value, Glosa = "Costo de mantenimiento" });
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaCaja.Id, Haber = maintenance.Costo.Value, Glosa = "Pago de mantenimiento" });

                var result = await _accountingService.CreateEntryAsync(entry);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"No se pudo contabilizar el mantenimiento: {result.Message}");
            }

            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Mantenimiento marcado como ejecutado y revision del equipo actualizada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"No se pudo ejecutar el mantenimiento: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Presupuesto>> GetBudgetsAsync()
    {
        return await _context.Presupuestos.OrderByDescending(p => p.Anio).ToListAsync();
    }

    public async Task<Presupuesto?> GetBudgetByIdAsync(Guid id)
    {
        return await _context.Presupuestos.FindAsync(id);
    }

    public async Task<PresupuestoFormViewModel> GetBudgetFormContextAsync(Presupuesto? existing = null)
    {
        return new PresupuestoFormViewModel
        {
            Presupuesto = existing ?? new Presupuesto { Anio = (short)DateTime.Now.Year, Estado = "BORRADOR" }
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateBudgetAsync(Presupuesto budget)
    {
        try
        {
            budget.Id = Guid.NewGuid();
            budget.EntidadId = _entidadProvider.CurrentEntidadId;
            budget.CreadoEn = DateTimeOffset.Now;
            _context.Presupuestos.Add(budget);
            await _context.SaveChangesAsync();
            return (true, "Presupuesto creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<PlanProduccion>> GetProductionPlansAsync()
    {
        return await _context.PlanProduccions.Include(p => p.Presupuesto).Include(p => p.PlanProduccionDetalles).OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes).ToListAsync();
    }

    public async Task<PlanProduccion?> GetProductionPlanByIdAsync(Guid id)
    {
        return await _context.PlanProduccions
            .Include(p => p.Presupuesto)
            .Include(p => p.PlanProduccionDetalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlanProduccionFormViewModel> GetPlanFormContextAsync(PlanProduccion? existing = null)
    {
        return new PlanProduccionFormViewModel
        {
            Plan = existing ?? new PlanProduccion { Anio = (short)DateTime.Now.Year, Mes = (short)DateTime.Now.Month, Estado = "BORRADOR" },
            Presupuestos = new SelectList(await _context.Presupuestos
                .Where(p => p.EntidadId == _entidadProvider.CurrentEntidadId && p.Estado == "APROBADO")
                .OrderByDescending(p => p.Anio)
                .ThenBy(p => p.Nombre)
                .ToListAsync(), "Id", "Nombre"),
            ProductosTerminados = new SelectList(await _context.Productos
                .Where(p => p.EntidadId == _entidadProvider.CurrentEntidadId && p.Activo && p.Tipo == "TERMINADO")
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
                .ToListAsync(), "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreatePlanAsync(PlanProduccion plan)
    {
        if (plan.Anio < 1 || plan.Mes is < 1 or > 12)
            return (false, "Indique un año y un mes válidos para el plan.");
        var lineValidation = await ValidatePlanLinesAsync(plan);
        if (lineValidation != null) return (false, lineValidation);
        if (await _context.PlanProduccions.AnyAsync(p => p.EntidadId == _entidadProvider.CurrentEntidadId && p.Anio == plan.Anio && p.Mes == plan.Mes))
            return (false, "Ya existe un plan de producción para ese período.");

        try
        {
            plan.Id = Guid.NewGuid();
            plan.EntidadId = _entidadProvider.CurrentEntidadId;
            plan.Estado = "BORRADOR";
            plan.CreadoEn = DateTimeOffset.Now;
            SetPlanDetailKeys(plan);
            _context.PlanProduccions.Add(plan);
            await _context.SaveChangesAsync();
            return (true, "Plan de producción creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdatePlanAsync(PlanProduccion plan)
    {
        try
        {
            var existing = await _context.PlanProduccions
                .Include(p => p.PlanProduccionDetalles)
                .FirstOrDefaultAsync(p => p.Id == plan.Id && p.EntidadId == _entidadProvider.CurrentEntidadId);
            if (existing == null) return (false, "No se encontró el plan de producción.");
            if (existing.Estado != "BORRADOR") return (false, "Solo se pueden editar planes en borrador.");
            if (plan.Anio < 1 || plan.Mes is < 1 or > 12) return (false, "Indique un año y un mes válidos para el plan.");
            var lineValidation = await ValidatePlanLinesAsync(plan);
            if (lineValidation != null) return (false, lineValidation);
            if (await _context.PlanProduccions.AnyAsync(p => p.Id != plan.Id && p.EntidadId == existing.EntidadId && p.Anio == plan.Anio && p.Mes == plan.Mes))
                return (false, "Ya existe otro plan de producción para ese período.");

            existing.Anio = plan.Anio;
            existing.Mes = plan.Mes;
            existing.PresupuestoId = plan.PresupuestoId;
            _context.PlanProduccionDetalles.RemoveRange(existing.PlanProduccionDetalles);
            existing.PlanProduccionDetalles.Clear();
            foreach (var detail in plan.PlanProduccionDetalles)
            {
                detail.Id = Guid.NewGuid();
                detail.PlanId = existing.Id;
                detail.CantidadEjecutada = 0;
                existing.PlanProduccionDetalles.Add(detail);
            }

            await _context.SaveChangesAsync();
            return (true, "Plan de producción actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<PlanProduccion> GenerateSuggestedPlanAsync(Guid entidadId, short year, short month)
    {
        var plan = new PlanProduccion
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            Anio = year,
            Mes = month,
            Estado = "BORRADOR",
            CreadoEn = DateTimeOffset.Now
        };

        // Obtener productos que están por debajo del stock mínimo (Existencia <= StockMinimo)
        var stockAlerts = await _inventoryService.GetLowStockAlertsAsync(entidadId);

        foreach (var alert in stockAlerts)
        {
            // Solo sugerir productos de tipo TERMINADO (producibles)
            if (alert.Producto.Tipo == "TERMINADO")
            {
                // Sugerir completar hasta el Stock Máximo (o el doble del mínimo si no hay máximo)
                var sugerencia = (alert.StockMaximo ?? (alert.StockMinimo * 2)) - alert.Cantidad;

                if (sugerencia > 0)
                {
                    plan.PlanProduccionDetalles.Add(new PlanProduccionDetalle
                    {
                        Id = Guid.NewGuid(),
                        PlanId = plan.Id,
                        ProductoId = alert.ProductoId,
                        Producto = alert.Producto,
                        CantidadPlanificada = sugerencia,
                        CantidadEjecutada = 0
                    });
                }
            }
        }

        return plan;
    }

    public async Task<(bool Succeeded, string Message, OrdenProduccion? Order)> CreateProductionOrderAsync(OrdenProduccion order)
    {
        order.Id = Guid.NewGuid();
        var numeroOp = await _consecutivoService.ObtenerSiguienteNumeroAsync(order.EntidadId, null, DocumentoTipo.OrdenProduccion, "A");
        order.NumeroOrden = $"OP-{numeroOp}";
        order.Estado = "PLANIFICADA";
        order.CreadoEn = DateTimeOffset.Now;

        if (order.CantidadPlanificada <= 0) return (false, "La cantidad planificada debe ser mayor a cero.", null);

        var bom = await _context.ListaMateriales
            .Include(l => l.ListaMaterialesDetalles)
            .FirstOrDefaultAsync(l => l.ProductoTerminadoId == order.ProductoTerminadoId && l.Activa);

        if (bom == null) return (false, "No existe una lista de materiales (BOM) activa para este producto.", null);
        if (!bom.ListaMaterialesDetalles.Any()) return (false, "La BOM activa no tiene componentes configurados.", null);

        order.ListaMaterialesId = bom.Id;

        var costSheet = await _context.FichaCostos
            .FirstOrDefaultAsync(f => f.ProductoId == order.ProductoTerminadoId && f.Estado == "VIGENTE");

        if (costSheet != null) order.FichaCostoId = costSheet.Id;

        foreach (var item in bom.ListaMaterialesDetalles)
        {
            order.OrdenProduccionConsumos.Add(new OrdenProduccionConsumo
            {
                Id = Guid.NewGuid(),
                ProductoInsumoId = item.ProductoInsumoId,
                CantidadPlanificada = item.CantidadRequerida * order.CantidadPlanificada * (1 + (item.PorcentajeMerma / 100)),
                CantidadReal = 0
            });
        }

        _context.OrdenProduccions.Add(order);
        await _context.SaveChangesAsync();

        return (true, "Orden de producción planificada.", order);
    }

    public async Task<(bool Succeeded, string Message)> StartProductionAndConsumeAsync(Guid orderId, Guid userId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            var order = await _context.OrdenProduccions
                .Include(o => o.OrdenProduccionConsumos)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return (false, "Orden no encontrada.");
            if (order.Estado != "PLANIFICADA") return (false, "La orden no está en estado planificado.");

            if (order.OrdenProduccionConsumos.Count == 0)
                return (false, "La orden no tiene insumos planificados.");

            var movSalida = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("CONSUMO_PRODUCCION"),
                NumeroDocumento = order.NumeroOrden,
                AlmacenOrigenId = order.AlmacenInsumosId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                ReferenciaExternaId = order.Id,
                Canal = "ERP",
                CreadoPor = userId
            };

            foreach (var item in order.OrdenProduccionConsumos)
            {
                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = item.ProductoInsumoId,
                    Cantidad = item.CantidadPlanificada
                });
            }

            var invResult = await _inventoryService.ProcessMovementAsync(movSalida);
            if (!invResult.Succeeded) throw new Exception($"Stock insuficiente para insumos: {invResult.Message}");

            decimal totalCostConsumed = 0;
            foreach (var item in order.OrdenProduccionConsumos)
            {
                var movimientoDetalle = invResult.Movement!.MovimientoInventarioDetalles
                    .First(d => d.ProductoId == item.ProductoInsumoId);
                item.CostoUnitario = movimientoDetalle.CostoUnitario ?? 0;
                totalCostConsumed += item.CantidadPlanificada * item.CostoUnitario.Value;
                item.MovimientoInventarioId = invResult.Movement.Id;
            }

            order.Estado = "EN_PROCESO";
            order.FechaInicioReal = DateTimeOffset.Now;
            order.CostoRealTotal = totalCostConsumed; // Acumular base de costo
            order.AsientoConsumoId = invResult.Movement!.AsientoId;

            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();

            return (true, "Producción iniciada y materiales contabilizados en WIP.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"Error al iniciar producción: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> ApprovePlanAsync(Guid planId)
    {
        var plan = await _context.PlanProduccions
            .Include(p => p.PlanProduccionDetalles)
            .FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) return (false, "No se encontró el plan de producción.");
        if (plan.Estado != "BORRADOR") return (false, "Solo se pueden aprobar planes en borrador.");
        if (plan.PlanProduccionDetalles.Count == 0)
            return (false, "El plan debe contener al menos un producto antes de aprobarse.");
        if (plan.PlanProduccionDetalles.Any(d => d.CantidadPlanificada <= 0))
            return (false, "Todas las cantidades planificadas deben ser mayores a cero.");

        var productIds = plan.PlanProduccionDetalles.Select(d => d.ProductoId).Distinct().ToList();
        var validProducts = await _context.Productos.CountAsync(p => productIds.Contains(p.Id) && p.EntidadId == plan.EntidadId && p.Activo && p.Tipo == "TERMINADO");
        if (validProducts != productIds.Count)
            return (false, "El plan contiene productos terminados inválidos para esta entidad.");

        plan.Estado = "APROBADO";
        await _context.SaveChangesAsync();
        return (true, "Plan de producción aprobado.");
    }

    public async Task<(bool Succeeded, string Message, int OrdersCreated)> GenerateOrdersFromPlanAsync(Guid planId, Guid userId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            var plan = await _context.PlanProduccions
                .Include(p => p.PlanProduccionDetalles)
                .FirstOrDefaultAsync(p => p.Id == planId && p.EntidadId == _entidadProvider.CurrentEntidadId);
            if (plan == null) return (false, "No se encontró el plan de producción.", 0);
            if (plan.Estado != "APROBADO")
                return (false, "Solo se pueden generar órdenes desde planes aprobados.", 0);

            int created = 0;
            foreach (var line in plan.PlanProduccionDetalles)
            {
                var remaining = line.CantidadPlanificada - line.CantidadEjecutada;
                if (remaining <= 0) continue;

                var bom = await _context.ListaMateriales
                    .Include(l => l.ListaMaterialesDetalles)
                    .FirstOrDefaultAsync(l => l.ProductoTerminadoId == line.ProductoId && l.Activa);
                if (bom == null) continue;

                var order = new OrdenProduccion
                {
                    EntidadId = plan.EntidadId,
                    ProductoTerminadoId = line.ProductoId,
                    AlmacenInsumosId = await _context.Almacens.Where(a => a.EntidadId == plan.EntidadId && a.Activo).Select(a => a.Id).FirstAsync(),
                    AlmacenProductoId = await _context.Almacens.Where(a => a.EntidadId == plan.EntidadId && a.Activo).Select(a => a.Id).FirstAsync(),
                    CantidadPlanificada = remaining,
                    FechaInicioPlan = new DateOnly(plan.Anio, plan.Mes ?? 1, 1),
                    FechaFinPlan = new DateOnly(plan.Anio, plan.Mes ?? 12, DateTime.DaysInMonth(plan.Anio, plan.Mes ?? 12)),
                    CreadoPor = userId
                };
                var result = await CreateProductionOrderAsync(order);
                if (result.Succeeded) created++;
            }

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, $"Se generaron {created} órdenes de producción desde el plan.", created);
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"No se pudieron generar las órdenes: {ex.Message}", 0);
        }
    }

    public async Task<(bool Succeeded, string Message)> CancelPlannedOrderAsync(Guid orderId)
    {
        var order = await _context.OrdenProduccions
            .FirstOrDefaultAsync(o => o.Id == orderId && o.EntidadId == _entidadProvider.CurrentEntidadId);
        if (order == null) return (false, "No se encontró la orden de producción.");
        if (order.Estado != "PLANIFICADA") return (false, "Solo se pueden cancelar órdenes que aún no se han iniciado.");

        var hasMovements = await _context.MovimientoInventarios.AnyAsync(m =>
            m.ReferenciaExternaTipo == "ORDEN_PRODUCCION" && m.ReferenciaExternaId == order.Id);
        if (hasMovements) return (false, "La orden tiene movimientos de inventario; requiere reversión antes de cancelarse.");

        order.Estado = "CANCELADA";
        await _context.SaveChangesAsync();
        return (true, "Orden planificada cancelada.");
    }

    public async Task<(bool Succeeded, string Message)> ReverseProductionOrderAsync(Guid orderId, Guid userId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            var order = await _context.OrdenProduccions
                .Include(o => o.OrdenProduccionConsumos)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.EntidadId == _entidadProvider.CurrentEntidadId);
            if (order == null) return (false, "No se encontro la orden de produccion.");
            if (order.Estado != "EN_PROCESO")
                return (false, "Solo se pueden revertir ordenes que estan en proceso.");

            // 1. Revertir el consumo inicial: devolver MP al almacen.
            var movSalida = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("DEVOLUCION_PRODUCCION"),
                NumeroDocumento = $"{order.NumeroOrden}-REV",
                AlmacenDestinoId = order.AlmacenInsumosId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                ReferenciaExternaId = order.Id,
                Canal = "ERP",
                CreadoPor = userId
            };
            foreach (var item in order.OrdenProduccionConsumos)
            {
                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = item.ProductoInsumoId,
                    Cantidad = item.CantidadPlanificada,
                    CostoUnitario = item.CostoUnitario
                });
            }
            var invResult = await _inventoryService.ProcessMovementAsync(movSalida);
            if (!invResult.Succeeded)
                throw new InvalidOperationException($"No se pudo revertir el consumo: {invResult.Message}");

            // 2. Revertir el asiento de consumo.
            if (order.AsientoConsumoId.HasValue)
            {
                var rev = await _accountingService.ReverseEntryAsync(order.AsientoConsumoId.Value, $"Reversion de orden {order.NumeroOrden}");
                if (!rev.Succeeded)
                    throw new InvalidOperationException($"No se pudo revertir el asiento de consumo: {rev.Message}");
            }

            // 3. Resetear la orden a PLANIFICADA.
            foreach (var c in order.OrdenProduccionConsumos)
            {
                c.CantidadReal = 0;
                c.CostoUnitario = null;
                c.MovimientoInventarioId = null;
            }
            order.Estado = "PLANIFICADA";
            order.FechaInicioReal = null;
            order.CostoRealTotal = null;
            order.AsientoConsumoId = null;
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Orden revertida a planificada; el consumo de materiales fue devuelto al inventario.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"No se pudo revertir la orden: {ex.Message}");
        }
    }

    private async Task<string?> ValidatePlanLinesAsync(PlanProduccion plan)
    {
        if (plan.PlanProduccionDetalles.Select(d => d.ProductoId).Distinct().Count() != plan.PlanProduccionDetalles.Count)
            return "Cada producto debe aparecer una sola vez en el plan.";
        if (plan.PlanProduccionDetalles.Any(d => d.CantidadPlanificada <= 0))
            return "Todas las cantidades del plan deben ser mayores a cero.";

        var productIds = plan.PlanProduccionDetalles.Select(d => d.ProductoId).Distinct().ToList();
        if (productIds.Count == 0) return null;
        var validProducts = await _context.Productos.CountAsync(p =>
            productIds.Contains(p.Id) && p.EntidadId == _entidadProvider.CurrentEntidadId && p.Activo && p.Tipo == "TERMINADO");
        return validProducts == productIds.Count ? null : "El plan contiene productos terminados inválidos para esta entidad.";
    }

    private static void SetPlanDetailKeys(PlanProduccion plan)
    {
        foreach (var detail in plan.PlanProduccionDetalles)
        {
            detail.Id = Guid.NewGuid();
            detail.PlanId = plan.Id;
            detail.CantidadEjecutada = 0;
        }
    }

    public async Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions, Guid userId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        await BeginSavepointAsync();
        try
        {
            var order = await _context.OrdenProduccions
                .Include(o => o.OrdenProduccionConsumos)
                .Include(o => o.FichaCosto)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return (false, "Orden no encontrada.");
            if (order.Estado != "EN_PROCESO") return (false, "La orden no está en proceso.");

            if (actualQuantity <= 0) return (false, "La cantidad producida debe ser mayor a cero.");
            if (actualConsumptions == null)
                return (false, "Debe informar el consumo real de todos los insumos.");

            var plannedConsumptionIds = order.OrdenProduccionConsumos
                .Select(c => c.ProductoInsumoId)
                .ToHashSet();
            if (actualConsumptions.Count != plannedConsumptionIds.Count ||
                actualConsumptions.Select(c => c.ProductoInsumoId).Distinct().Count() != actualConsumptions.Count ||
                actualConsumptions.Any(c => !plannedConsumptionIds.Contains(c.ProductoInsumoId)))
                return (false, "El consumo recibido no coincide con los insumos planificados de la orden.");

            var actualByProduct = actualConsumptions.ToDictionary(c => c.ProductoInsumoId, c => c.CantidadReal);
            decimal costMateriaPrimaReal = 0;

            // 1. Ajustar desviaciones de consumo (RF-41: descontar/reintegrar stock real y asiento compensatorio WIP)
            int lineaVariacion = 0;
            foreach (var stored in order.OrdenProduccionConsumos)
            {
                var cantidadReal = actualByProduct[stored.ProductoInsumoId];
                if (cantidadReal < 0) return (false, "El consumo real no puede ser negativo.");

                stored.CantidadReal = cantidadReal;
                costMateriaPrimaReal += (stored.CantidadReal * (stored.CostoUnitario ?? 0));

                var diff = stored.CantidadReal - stored.CantidadPlanificada;
                if (Math.Abs(diff) < 0.0001m) continue;

                lineaVariacion++;
                var movVar = new MovimientoInventario
                {
                    EntidadId = order.EntidadId,
                    Fecha = DateTimeOffset.Now,
                    NumeroDocumento = diff > 0 ? $"{order.NumeroOrden}-EXT-{lineaVariacion:00}" : $"{order.NumeroOrden}-DEV-{lineaVariacion:00}",
                    ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                    ReferenciaExternaId = order.Id,
                    Canal = "ERP",
                    CreadoPor = userId
                };

                if (diff > 0)
                {
                    // Consumió más de lo planificado: salida extra (DEBE WIP / HABER Inventario)
                    movVar.TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("CONSUMO_PRODUCCION");
                    movVar.AlmacenOrigenId = order.AlmacenInsumosId;
                }
                else
                {
                    // Consumió menos de lo planificado: reintegra material (DEBE Inventario / HABER WIP)
                    movVar.TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("DEVOLUCION_PRODUCCION");
                    movVar.AlmacenDestinoId = order.AlmacenInsumosId;
                }

                movVar.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = stored.ProductoInsumoId,
                    Cantidad = Math.Abs(diff),
                    CostoUnitario = stored.CostoUnitario ?? 0
                });

                var varResult = await _inventoryService.ProcessMovementAsync(movVar);
                if (!varResult.Succeeded) throw new Exception($"Error en ajuste de desviación de consumo: {varResult.Message}");
            }

            // 2. Sumar costos fijos de la ficha (MO y GIF)
            var costoManoObra = Math.Round((order.FichaCosto?.CostoManoObra ?? 0) * actualQuantity, 2, MidpointRounding.AwayFromZero);
            var costoIndirectos = Math.Round((order.FichaCosto?.GastosIndirectos ?? 0) * actualQuantity, 2, MidpointRounding.AwayFromZero);
            var costFijosReal = costoManoObra + costoIndirectos;

            decimal costTotalReal = costMateriaPrimaReal + costFijosReal;
            var finalUnitCost = actualQuantity > 0 ? Math.Round(costTotalReal / actualQuantity, 2) : 0;

            if (costFijosReal > 0)
            {
                var cuentaWipCodigo = await _paramService.ObtenerValorVigenteAsync(order.EntidadId, "CTA_WIP") ?? "185.0010";
                var cuentaManoObraCodigo = await _paramService.ObtenerValorVigenteAsync(order.EntidadId, "CTA_MANO_OBRA_PRODUCCION");
                var cuentaIndirectosCodigo = await _paramService.ObtenerValorVigenteAsync(order.EntidadId, "CTA_GIF_PRODUCCION");
                if ((costoManoObra > 0 && string.IsNullOrWhiteSpace(cuentaManoObraCodigo)) ||
                    (costoIndirectos > 0 && string.IsNullOrWhiteSpace(cuentaIndirectosCodigo)))
                    throw new InvalidOperationException("Configure las subcuentas imputables para mano de obra (CTA_MANO_OBRA_PRODUCCION) y gastos indirectos (CTA_GIF_PRODUCCION).");

                var codigosCuentas = new[] { cuentaWipCodigo, cuentaManoObraCodigo, cuentaIndirectosCodigo }
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();
                var cuentasConversion = await _context.CuentaContables
                    .Where(c => c.EntidadId == order.EntidadId && c.Activo && codigosCuentas.Contains(c.Codigo))
                    .ToDictionaryAsync(c => c.Codigo);

                if (!cuentasConversion.TryGetValue(cuentaWipCodigo, out var cuentaWip) || !cuentaWip.AceptaMovimiento || cuentaWip.Clase != "ACTIVO" ||
                    (costoManoObra > 0 && (!cuentasConversion.TryGetValue(cuentaManoObraCodigo!, out var cuentaManoObra) || !cuentaManoObra.AceptaMovimiento || cuentaManoObra.Clase != "GASTOS" || cuentaManoObra.Naturaleza != "DEUDORA")) ||
                    (costoIndirectos > 0 && (!cuentasConversion.TryGetValue(cuentaIndirectosCodigo!, out var cuentaIndirectos) || !cuentaIndirectos.AceptaMovimiento || cuentaIndirectos.Clase != "GASTOS" || cuentaIndirectos.Naturaleza != "DEUDORA")))
                    throw new InvalidOperationException("Las cuentas configuradas de WIP y costos de conversión deben existir, estar activas y ser imputables; los costos deben usar cuentas de gasto deudoras.");

                var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "DIA");
                if (tipoComprobante == null)
                    throw new InvalidOperationException("No existe el tipo de comprobante DIA para contabilizar los costos de conversión.");

                var conversionEntry = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    EntidadId = order.EntidadId,
                    TipoComprobanteId = tipoComprobante.Id,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Concepto = $"TRANSFERENCIA DE COSTOS DE CONVERSIÓN {order.NumeroOrden}",
                    ModuloOrigen = "PRODUCCION",
                    DocumentoOrigenTipo = "ORDEN_PRODUCCION_CONVERSION",
                    DocumentoOrigenId = order.Id,
                    Estado = "CONTABILIZADO",
                    CreadoPor = userId,
                    CreadoEn = DateTimeOffset.Now
                };

                conversionEntry.AsientoDetalles.Add(new AsientoDetalle
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaWip.Id,
                    Debe = costFijosReal,
                    Glosa = $"Costos de conversión {order.NumeroOrden}"
                });
                if (costoManoObra > 0)
                    conversionEntry.AsientoDetalles.Add(new AsientoDetalle
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = cuentasConversion[cuentaManoObraCodigo!].Id,
                        Haber = costoManoObra,
                        Glosa = $"Mano de obra aplicada {order.NumeroOrden}"
                    });
                if (costoIndirectos > 0)
                    conversionEntry.AsientoDetalles.Add(new AsientoDetalle
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = cuentasConversion[cuentaIndirectosCodigo!].Id,
                        Haber = costoIndirectos,
                        Glosa = $"Gastos indirectos aplicados {order.NumeroOrden}"
                    });

                var accountingResult = await _accountingService.CreateEntryAsync(conversionEntry);
                if (!accountingResult.Succeeded)
                    throw new InvalidOperationException($"No se pudieron transferir los costos de conversión: {accountingResult.Message}");
            }

            // 3. Entrada de Producto Terminado (RF-42)
            var movEntrada = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("ENTRADA_PRODUCCION"),
                NumeroDocumento = $"{order.NumeroOrden}-PT",
                AlmacenDestinoId = order.AlmacenProductoId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                ReferenciaExternaId = order.Id,
                Canal = "ERP",
                CreadoPor = userId
            };

            movEntrada.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                ProductoId = order.ProductoTerminadoId,
                Cantidad = actualQuantity,
                CostoUnitario = finalUnitCost
            });

            var invResult = await _inventoryService.ProcessMovementAsync(movEntrada);
            if (!invResult.Succeeded) throw new Exception($"Error al ingresar producto: {invResult.Message}");

            order.AsientoTerminadoId = invResult.Movement!.AsientoId;

            // 4. Análisis de Desviaciones (RF-43)
            if (order.FichaCosto != null)
            {
                // Desviación Materia Prima
                _context.AnalisisDesviacions.Add(new AnalisisDesviacion
                {
                    Id = Guid.NewGuid(),
                    OrdenProduccionId = order.Id,
                    Componente = "MATERIA_PRIMA",
                    CostoEstandar = order.FichaCosto.CostoMateriaPrima * actualQuantity,
                    CostoReal = costMateriaPrimaReal,
                    AnalizadoEn = DateTimeOffset.Now
                });

                // Desviaci?n de conversi?n (MANO_OBRA y GASTOS_INDIRECTOS)
                var moEstandar = (decimal)order.FichaCosto.CostoManoObra * actualQuantity;
                var gifEstandar = (decimal)order.FichaCosto.GastosIndirectos * actualQuantity;
                var convReal = costTotalReal - costMateriaPrimaReal;
                var totalConvStd = moEstandar + gifEstandar;
                if (moEstandar != 0 || convReal != 0)
                {
                    var realMo = convReal;
                    if (totalConvStd > 0) realMo = convReal * (moEstandar / totalConvStd);
                    if (moEstandar != 0 || realMo != 0)
                    {
                        _context.AnalisisDesviacions.Add(new AnalisisDesviacion
                        {
                            Id = Guid.NewGuid(),
                            OrdenProduccionId = order.Id,
                            Componente = "MANO_OBRA",
                            CostoEstandar = moEstandar,
                            CostoReal = Math.Round(realMo, 4),
                            AnalizadoEn = DateTimeOffset.Now
                        });
                    }
                }
                if (gifEstandar != 0 || convReal != 0)
                {
                    var realGif = convReal;
                    if (totalConvStd > 0) realGif = convReal * (gifEstandar / totalConvStd);
                    if (moEstandar == 0 && gifEstandar == 0 && totalConvStd == 0) realGif = convReal;
                    if (gifEstandar != 0 || realGif != 0)
                    {
                        _context.AnalisisDesviacions.Add(new AnalisisDesviacion
                        {
                            Id = Guid.NewGuid(),
                            OrdenProduccionId = order.Id,
                            Componente = "GASTOS_INDIRECTOS",
                            CostoEstandar = gifEstandar,
                            CostoReal = Math.Round(realGif, 4),
                            AnalizadoEn = DateTimeOffset.Now
                        });
                    }
                }
            }

            // --- ACTUALIZACIÓN DE PLAN DE PRODUCCIÓN (Iteración 7) ---
            var fechaPlanificada = order.FechaInicioPlan ?? DateOnly.FromDateTime(DateTime.Now);
            var planLine = await _context.PlanProduccionDetalles
                .Include(d => d.Plan)
                .FirstOrDefaultAsync(d => d.Plan.EntidadId == order.EntidadId
                                       && d.Plan.Anio == fechaPlanificada.Year
                                       && d.Plan.Mes == fechaPlanificada.Month
                                       && d.Plan.Estado == "APROBADO"
                                       && d.ProductoId == order.ProductoTerminadoId);

            if (planLine != null)
            {
                planLine.CantidadEjecutada += actualQuantity;
            }

            order.CantidadProducida = actualQuantity;
            order.FechaFinReal = DateTimeOffset.Now;
            order.Estado = "TERMINADA";
            order.CostoRealTotal = costTotalReal;

            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();

            return (true, $"Producción sellada. Costo Unitario Real: {finalUnitCost:C}. Contabilizado 188 <-> 185.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            else await RollbackSavepointAsync();
            return (false, $"Error al finalizar: {ex.Message} INNER:{ex.InnerException?.Message}");
        }
    }

    public async Task<decimal> CalculateStandardCostAsync(Guid productId)
    {
        return await _context.FichaCostos
            .Where(f => f.ProductoId == productId && f.Estado == "VIGENTE")
            .Select(f => f.CostoTotalUnitario ?? 0)
            .FirstOrDefaultAsync();
    }

    // Permite que los flujos de producción participen en una transacción ya abierta
    // (por ejemplo, pruebas de integración o composición con otros servicios) sin
    // perder la reversión atómica de sus propios cambios.
    private async Task<bool> BeginSavepointAsync()
    {
        if (_context.Database.CurrentTransaction == null) return false;
        await _context.Database.ExecuteSqlRawAsync("SAVE TRANSACTION VERCOM_PRODUCCION_SP");
        return true;
    }

    private async Task RollbackSavepointAsync()
    {
        if (_context.Database.CurrentTransaction == null) return;
        try
        {
            await _context.Database.ExecuteSqlRawAsync("ROLLBACK TRANSACTION VERCOM_PRODUCCION_SP");
        }
        catch (Exception)
        {
            // Si el proveedor no soporta savepoints, el llamador conserva la transacción.
        }
    }
}
