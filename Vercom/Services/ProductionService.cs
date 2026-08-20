using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IProductionService
{
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

    public ProductionService(AppDbContext context, IInventoryService inventoryService, IAccountingService accountingService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _accountingService = accountingService;
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
