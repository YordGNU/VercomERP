using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

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

    // Gestión de Mermas
    Task<IEnumerable<Merma>> GetWastesAsync();
    Task<MermaFormViewModel> GetWasteFormContextAsync(Merma? existing = null);
    Task<(bool Succeeded, string Message)> CreateWasteAsync(Merma waste);

    // Gestión de Presupuestos
    Task<IEnumerable<Presupuesto>> GetBudgetsAsync();
    Task<Presupuesto?> GetBudgetByIdAsync(Guid id);
    Task<PresupuestoFormViewModel> GetBudgetFormContextAsync(Presupuesto? existing = null);
    Task<(bool Succeeded, string Message)> CreateBudgetAsync(Presupuesto budget);

    // Planes de Producción
    Task<IEnumerable<PlanProduccion>> GetProductionPlansAsync();
    Task<PlanProduccionFormViewModel> GetPlanFormContextAsync(PlanProduccion? existing = null);
    Task<(bool Succeeded, string Message)> CreatePlanAsync(PlanProduccion plan);

    // Escritura Producción
    // Gestión de Fichas de Costo (RF-40)
    Task<IEnumerable<FichaCosto>> GetCostSheetsAsync();
    Task<FichaCosto?> GetCostSheetByIdAsync(Guid id);
    Task<CostSheetCreateViewModel> GetCostSheetCreateContextAsync(FichaCosto? existing = null);
    Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> CreateCostSheetAsync(FichaCosto costSheet);
    Task<(bool Succeeded, string Message, OrdenProduccion? Order)> CreateProductionOrderAsync(OrdenProduccion order);
    Task<(bool Succeeded, string Message)> StartProductionAndConsumeAsync(Guid orderId, Guid userId);
    Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions, Guid userId);
    Task<decimal> CalculateStandardCostAsync(Guid productId);
}

public class ProductionService : IProductionService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IAccountingService _accountingService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public ProductionService(AppDbContext context, IInventoryService inventoryService, IAccountingService accountingService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _inventoryService = inventoryService;
        _accountingService = accountingService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<OrdenProduccion>> GetOrdersAsync()
    {
        return await _context.OrdenProduccions
            .Include(o => o.ProductoTerminado)
            .Include(o => o.FichaCosto)
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
                .Where(p => p.Activo && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")).ToListAsync(), "Id", "Nombre"),
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
                .Where(p => p.Activo && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")).ToListAsync(), "Id", "Nombre"),
            InsumosDisponibles = await _context.Productos
                .Where(p => p.Activo && p.Tipo == "INSUMO")
                .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
                .ToListAsync()
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateBomAsync(ListaMateriale bom)
    {
        try
        {
            bom.Id = Guid.NewGuid();
            bom.CreadoEn = DateTimeOffset.Now;

            // RF-41: Inactivar versiones anteriores del mismo producto
            var previous = await _context.ListaMateriales
                .Where(l => l.ProductoTerminadoId == bom.ProductoTerminadoId && l.Activa)
                .ToListAsync();
            foreach (var p in previous) p.Activa = false;

            _context.ListaMateriales.Add(bom);
            await _context.SaveChangesAsync();
            return (true, "BOM registrada y activada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
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
                .Where(p => p.Activo && (p.Tipo == "ELABORADO" || p.Tipo == "TERMINADO")).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> CreateCostSheetAsync(FichaCosto costSheet)
    {
        var previous = await _context.FichaCostos
            .Where(f => f.ProductoId == costSheet.ProductoId && f.Estado == "VIGENTE")
            .FirstOrDefaultAsync();

        if (previous != null)
        {
            previous.Estado = "HISTORICA";
            previous.VigenteHasta = DateOnly.FromDateTime(DateTime.Now);
            costSheet.Version = previous.Version + 1;
        }
        else
        {
            costSheet.Version = 1;
        }

        costSheet.Id = Guid.NewGuid();
        costSheet.Estado = "VIGENTE";
        costSheet.VigenteDesde = DateOnly.FromDateTime(DateTime.Now);
        costSheet.CreadoEn = DateTimeOffset.Now;

        costSheet.CostoTotalUnitario = costSheet.CostoMateriaPrima + costSheet.CostoManoObra + costSheet.GastosIndirectos;

        if (costSheet.MargenPorcentaje.HasValue)
        {
            costSheet.PrecioSugerido = costSheet.CostoTotalUnitario * (1 + (costSheet.MargenPorcentaje.Value / 100));
        }

        _context.FichaCostos.Add(costSheet);
        await _context.SaveChangesAsync();

        return (true, "Ficha de costo creada y activada.", costSheet);
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
            Equipos = new SelectList(await GetEquipmentsAsync(), "Id", "Nombre"),
            Responsables = new SelectList(await _context.Empleados.Where(e => e.Estado == "ACTIVO").ToListAsync(), "Id", "NombreCompleto")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateMaintenanceAsync(MantenimientoProgramado maintenance)
    {
        try
        {
            maintenance.Id = Guid.NewGuid();
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
            Productos = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateWasteAsync(Merma waste)
    {
        try
        {
            waste.Id = Guid.NewGuid();
            _context.Mermas.Add(waste);
            await _context.SaveChangesAsync();
            return (true, "Merma registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
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
        return await _context.PlanProduccions.Include(p => p.Presupuesto).OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes).ToListAsync();
    }

    public async Task<PlanProduccionFormViewModel> GetPlanFormContextAsync(PlanProduccion? existing = null)
    {
        return new PlanProduccionFormViewModel
        {
            Plan = existing ?? new PlanProduccion { Anio = (short)DateTime.Now.Year, Mes = (short)DateTime.Now.Month, Estado = "BORRADOR" },
            Presupuestos = new SelectList(await GetBudgetsAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreatePlanAsync(PlanProduccion plan)
    {
        try
        {
            plan.Id = Guid.NewGuid();
            plan.EntidadId = _entidadProvider.CurrentEntidadId;
            plan.CreadoEn = DateTimeOffset.Now;
            _context.PlanProduccions.Add(plan);
            await _context.SaveChangesAsync();
            return (true, "Plan de producción creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message, OrdenProduccion? Order)> CreateProductionOrderAsync(OrdenProduccion order)
    {
        order.Id = Guid.NewGuid();
        order.NumeroOrden = $"OP-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        order.Estado = "PLANIFICADA";
        order.CreadoEn = DateTimeOffset.Now;

        var bom = await _context.ListaMateriales
            .Include(l => l.ListaMaterialesDetalles)
            .FirstOrDefaultAsync(l => l.ProductoTerminadoId == order.ProductoTerminadoId && l.Activa);

        if (bom == null) return (false, "No existe una lista de materiales (BOM) activa para este producto.", null);

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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.OrdenProduccions
                .Include(o => o.OrdenProduccionConsumos)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return (false, "Orden no encontrada.");
            if (order.Estado != "PLANIFICADA") return (false, "La orden no está en estado planificado.");

            // RF-41: Consumo automático al iniciar (PUSH)
            foreach (var item in order.OrdenProduccionConsumos)
            {
                var movSalida = new MovimientoInventario
                {
                    EntidadId = order.EntidadId,
                    TipoMovimientoId = 6, // PRO: Consumo Producción
                    NumeroDocumento = order.NumeroOrden,
                    AlmacenOrigenId = order.AlmacenInsumosId,
                    Fecha = DateTimeOffset.Now,
                    ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                    ReferenciaExternaId = order.Id,
                    Canal = "ERP",
                    CreadoPor = userId
                };

                // Obtener costo actual para la salida
                var stock = await _context.Existencia
                    .FirstOrDefaultAsync(e => e.AlmacenId == order.AlmacenInsumosId && e.ProductoId == item.ProductoInsumoId);

                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = item.ProductoInsumoId,
                    Cantidad = item.CantidadPlanificada,
                    CostoUnitario = stock?.CostoPromedio ?? 0
                });

                var invResult = await _inventoryService.ProcessMovementAsync(movSalida);
                if (!invResult.Succeeded) throw new Exception($"Stock insuficiente para insumo: {invResult.Message}");
            }

            order.Estado = "EN_PROCESO";
            order.FechaInicioReal = DateTimeOffset.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Producción iniciada y materiales consumidos.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al iniciar producción: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions, Guid userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.OrdenProduccions
                .Include(o => o.OrdenProduccionConsumos)
                .Include(o => o.FichaCosto)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return (false, "Orden no encontrada.");
            if (order.Estado != "EN_PROCESO") return (false, "La orden no está en proceso.");

            order.CantidadProducida = actualQuantity;
            order.FechaFinReal = DateTimeOffset.Now;
            order.Estado = "FINALIZADA";

            decimal costRealTotal = 0;

            // 1. Ajustar desviaciones de consumo (si las hay)
            foreach (var consumption in actualConsumptions)
            {
                var stored = order.OrdenProduccionConsumos.First(c => c.ProductoInsumoId == consumption.ProductoInsumoId);

                // Si consumió MÁS de lo planificado, generar movimiento extra de salida
                if (consumption.CantidadReal > stored.CantidadPlanificada)
                {
                    var extra = consumption.CantidadReal - stored.CantidadPlanificada;
                    // (Lógica de movimiento extra aquí si se desea precisión total en tiempo real)
                }

                stored.CantidadReal = consumption.CantidadReal;
                costRealTotal += (stored.CantidadReal * (stored.CostoUnitario ?? 0));
            }

            // 2. Sumar costos fijos de la ficha
            if (order.FichaCosto != null)
            {
                var moTotal = order.FichaCosto.CostoManoObra * actualQuantity;
                var gifTotal = order.FichaCosto.GastosIndirectos * actualQuantity;
                costRealTotal += (moTotal + gifTotal);
            }

            order.CostoRealTotal = costRealTotal;
            var finalUnitCost = actualQuantity > 0 ? costRealTotal / actualQuantity : 0;

            // 3. Entrada de Producto Terminado (RF-42)
            var movEntrada = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = 1, // REC: Entrada
                NumeroDocumento = order.NumeroOrden,
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

            await _inventoryService.ProcessMovementAsync(movEntrada);

            // 4. Análisis de Desviaciones (RF-43)
            var stdCostUnit = order.FichaCosto?.CostoTotalUnitario ?? 0;
            _context.AnalisisDesviacions.Add(new AnalisisDesviacion
            {
                Id = Guid.NewGuid(),
                OrdenProduccionId = order.Id,
                Componente = "TOTAL",
                CostoEstandar = stdCostUnit * actualQuantity,
                CostoReal = costRealTotal,
                AnalizadoEn = DateTimeOffset.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Producción sellada. Costo Real: {finalUnitCost:C}.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al finalizar: {ex.Message}");
        }
    }

    public async Task<decimal> CalculateStandardCostAsync(Guid productId)
    {
        return await _context.FichaCostos
            .Where(f => f.ProductoId == productId && f.Estado == "VIGENTE")
            .Select(f => f.CostoTotalUnitario ?? 0)
            .FirstOrDefaultAsync();
    }
}
