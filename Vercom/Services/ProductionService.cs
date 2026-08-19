using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IProductionService
{
    Task<(bool Succeeded, string Message, FichaCosto? CostSheet)> CreateCostSheetAsync(FichaCosto costSheet);
    Task<(bool Succeeded, string Message, OrdenProduccion? Order)> CreateProductionOrderAsync(OrdenProduccion order);
    Task<(bool Succeeded, string Message)> StartProductionAsync(Guid orderId);
    Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions);
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
        // 1. Desactivar versión anterior si existe
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

        // 2. Calcular costo total
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

        // Cargar BOM (Explosión de materiales)
        var bom = await _context.ListaMateriales
            .Include(l => l.ListaMaterialesDetalles)
            .FirstOrDefaultAsync(l => l.ProductoTerminadoId == order.ProductoTerminadoId && l.Activa);

        if (bom == null) return (false, "No existe una lista de materiales (BOM) activa para este producto.", null);

        order.ListaMaterialesId = bom.Id;

        // Cargar Ficha de Costo vigente
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

    public async Task<(bool Succeeded, string Message)> StartProductionAsync(Guid orderId)
    {
        var order = await _context.OrdenProduccions.FindAsync(orderId);
        if (order == null) return (false, "Orden no encontrada.");
        if (order.Estado != "PLANIFICADA") return (false, "La orden no está en estado planificado.");

        order.Estado = "EN_PROCESO";
        order.FechaInicioReal = DateTimeOffset.Now;

        await _context.SaveChangesAsync();
        return (true, "Producción iniciada.");
    }

    public async Task<(bool Succeeded, string Message)> FinishProductionAsync(Guid orderId, decimal actualQuantity, List<OrdenProduccionConsumo> actualConsumptions)
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

            // 1. Procesar consumos reales de insumos
            foreach (var consumption in actualConsumptions)
            {
                var stored = order.OrdenProduccionConsumos.First(c => c.ProductoInsumoId == consumption.ProductoInsumoId);
                stored.CantidadReal = consumption.CantidadReal;

                // Obtener costo actual del insumo en el almacén
                var stock = await _context.Existencia
                    .FirstOrDefaultAsync(e => e.AlmacenId == order.AlmacenInsumosId && e.ProductoId == consumption.ProductoInsumoId);

                stored.CostoUnitario = stock?.CostoPromedio ?? 0;
                costRealTotal += (stored.CantidadReal * (stored.CostoUnitario ?? 0));

                // Descontar del inventario (Movimiento tipo PRO: Consumo Producción)
                var movSalida = new MovimientoInventario
                {
                    EntidadId = order.EntidadId,
                    TipoMovimientoId = 6, // PRO
                    NumeroDocumento = order.NumeroOrden,
                    AlmacenOrigenId = order.AlmacenInsumosId,
                    Fecha = DateTimeOffset.Now,
                    ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                    ReferenciaExternaId = order.Id,
                    Canal = "ERP"
                };
                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = consumption.ProductoInsumoId,
                    Cantidad = consumption.CantidadReal,
                    CostoUnitario = stored.CostoUnitario
                });

                await _inventoryService.ProcessMovementAsync(movSalida);
            }

            // 2. Sumar costos de mano de obra y GIF de la ficha
            if (order.FichaCosto != null)
            {
                var moTotal = order.FichaCosto.CostoManoObra * actualQuantity;
                var gifTotal = order.FichaCosto.GastosIndirectos * actualQuantity;
                costRealTotal += (moTotal + gifTotal);
            }

            order.CostoRealTotal = costRealTotal;
            var finalUnitCost = actualQuantity > 0 ? costRealTotal / actualQuantity : 0;

            // 3. Entrada de Producto Terminado a Almacén
            var movEntrada = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = 1, // REC (Simplificado como entrada)
                NumeroDocumento = order.NumeroOrden,
                AlmacenDestinoId = order.AlmacenProductoId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "ORDEN_PRODUCCION",
                ReferenciaExternaId = order.Id,
                Canal = "ERP"
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
            var stdCostTotal = stdCostUnit * actualQuantity;

            _context.AnalisisDesviacions.Add(new AnalisisDesviacion
            {
                Id = Guid.NewGuid(),
                OrdenProduccionId = order.Id,
                Componente = "TOTAL",
                CostoEstandar = stdCostTotal,
                CostoReal = costRealTotal,
                AnalizadoEn = DateTimeOffset.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Producción finalizada. Costo Real Unitario: {finalUnitCost:C}.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al finalizar producción: {ex.Message}");
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
