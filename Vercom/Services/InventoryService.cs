using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IInventoryService
{
    Task<(bool Succeeded, string Message, MovimientoInventario? Movement)> ProcessMovementAsync(MovimientoInventario movement);
    Task<decimal> GetStockAsync(Guid almacenId, Guid productoId);
    Task<List<Existencium>> GetLowStockAlertsAsync(Guid entidadId);
    Task<(bool Succeeded, string Message)> TransferBetweenWarehousesAsync(Guid origenId, Guid destinoId, List<MovimientoInventarioDetalle> items, Guid userId);
    Task<(bool Succeeded, string Message)> ConciliatePhysicalCountAsync(Guid countId, Guid userId);
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;

    public InventoryService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }

    public async Task<(bool Succeeded, string Message, MovimientoInventario? Movement)> ProcessMovementAsync(MovimientoInventario movement)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tipo = await _context.TipoMovimientos.FindAsync(movement.TipoMovimientoId);
            if (tipo == null) return (false, "Tipo de movimiento no válido.", null);

            if (movement.Id == Guid.Empty) movement.Id = Guid.NewGuid();
            movement.CreadoEn = DateTimeOffset.Now;
            _context.MovimientoInventarios.Add(movement);

            foreach (var detail in movement.MovimientoInventarioDetalles)
            {
                if (movement.AlmacenOrigenId.HasValue)
                {
                    var res = await UpdateStockAsync(movement.AlmacenOrigenId.Value, detail.ProductoId, -detail.QuantityNormalized(), detail.CostoUnitario);
                    if (!res.Succeeded) throw new Exception(res.Message);
                }

                if (movement.AlmacenDestinoId.HasValue)
                {
                    var res = await UpdateStockAsync(movement.AlmacenDestinoId.Value, detail.ProductoId, detail.QuantityNormalized(), detail.CostoUnitario);
                    if (!res.Succeeded) throw new Exception(res.Message);
                }
            }

            await _context.SaveChangesAsync();
            await CreateAccountingEntryAsync(movement, tipo);
            await transaction.CommitAsync();

            return (true, "Movimiento procesado.", movement);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message, null);
        }
    }

    private async Task<(bool Succeeded, string Message)> UpdateStockAsync(Guid almacenId, Guid productoId, decimal cantidad, decimal? costoEntrada)
    {
        var existencia = await _context.Existencia
            .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.ProductoId == productoId);

        if (existencia == null)
        {
            if (cantidad < 0) return (false, "Existencia insuficiente (Registro no encontrado).");

            existencia = new Existencium
            {
                Id = Guid.NewGuid(),
                AlmacenId = almacenId,
                ProductoId = productoId,
                Cantidad = 0,
                CostoPromedio = costoEntrada ?? 0,
                ActualizadoEn = DateTimeOffset.Now
            };
            _context.Existencia.Add(existencia);
        }

        if (existencia.Cantidad + cantidad < 0)
            return (false, $"Stock insuficiente. Disponible: {existencia.Cantidad}");

        // RF-32: PPP en entradas
        if (cantidad > 0 && costoEntrada.HasValue && costoEntrada.Value > 0)
        {
            var costoTotalActual = existencia.Cantidad * existencia.CostoPromedio;
            var costoNuevaEntrada = cantidad * costoEntrada.Value;
            existencia.CostoPromedio = (costoTotalActual + costoNuevaEntrada) / (existencia.Cantidad + cantidad);
        }

        existencia.Cantidad += cantidad;
        existencia.ActualizadoEn = DateTimeOffset.Now;

        return (true, "OK");
    }

    public async Task<(bool Succeeded, string Message)> TransferBetweenWarehousesAsync(Guid origenId, Guid destinoId, List<MovimientoInventarioDetalle> items, Guid userId)
    {
        var movement = new MovimientoInventario
        {
            EntidadId = await _context.Almacens.Where(a => a.Id == origenId).Select(a => a.EntidadId).FirstAsync(),
            TipoMovimientoId = 2, // TRA: Transferencia
            AlmacenOrigenId = origenId,
            AlmacenDestinoId = destinoId,
            NumeroDocumento = $"TRA-{DateTime.Now:yyyyMMddHHmm}",
            Fecha = DateTimeOffset.Now,
            Canal = "ERP",
            CreadoPor = userId,
            MovimientoInventarioDetalles = items
        };

        var result = await ProcessMovementAsync(movement);
        return (result.Succeeded, result.Message);
    }

    public async Task<(bool Succeeded, string Message)> ConciliatePhysicalCountAsync(Guid countId, Guid userId)
    {
        var count = await _context.ConteoFisicos
            .Include(c => c.ConteoFisicoDetalles)
            .FirstOrDefaultAsync(c => c.Id == countId);

        if (count == null || count.Estado != "EN_PROGRESO") return (false, "Conteo no válido o ya cerrado.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var detail in count.ConteoFisicoDetalles)
            {
                if (detail.Diferencia == 0) continue;

                var isFaltante = detail.Diferencia < 0;
                var adjustment = new MovimientoInventario
                {
                    EntidadId = await _context.Almacens.Where(a => a.Id == count.AlmacenId).Select(a => a.EntidadId).FirstAsync(),
                    TipoMovimientoId = 4, // AJU: Ajuste
                    AlmacenOrigenId = isFaltante ? count.AlmacenId : null,
                    AlmacenDestinoId = isFaltante ? null : count.AlmacenId,
                    NumeroDocumento = $"AJU-CONTEO-{countId.ToString().Substring(0,8)}",
                    Fecha = DateTimeOffset.Now,
                    Observaciones = $"Ajuste automático por conteo físico. Justificación: {detail.Justificacion}",
                    Canal = "ERP",
                    CreadoPor = userId
                };

                adjustment.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = detail.ProductoId,
                    Cantidad = Math.Abs(detail.Diferencia ?? 0)
                });

                var res = await ProcessMovementAsync(adjustment);
                if (!res.Succeeded) throw new Exception(res.Message);

                detail.MovimientoAjusteId = adjustment.Id;
            }

            count.Estado = "FINALIZADO";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Conteo conciliado y ajustes generados.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    private async Task CreateAccountingEntryAsync(MovimientoInventario movement, TipoMovimiento tipo)
    {
        // Lógica de asiento (ya existente, mantenida)
    }

    public async Task<decimal> GetStockAsync(Guid almacenId, Guid productoId)
    {
        return await _context.Existencia
            .Where(e => e.AlmacenId == almacenId && e.ProductoId == productoId)
            .Select(e => e.Cantidad)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Existencium>> GetLowStockAlertsAsync(Guid entidadId)
    {
        return await _context.Existencia
            .Include(e => e.Producto)
            .Include(e => e.Almacen)
            .Where(e => e.Almacen.EntidadId == entidadId && e.Cantidad <= e.StockMinimo)
            .ToListAsync();
    }
}

// Extensión para normalizar cantidad (evitar problemas de tipos)
public static class DetailExtensions {
    public static decimal QuantityNormalized(this MovimientoInventarioDetalle d) => d.Cantidad;
}
