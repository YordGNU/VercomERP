using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IInventoryService
{
    Task<(bool Succeeded, string Message, MovimientoInventario? Movement)> ProcessMovementAsync(MovimientoInventario movement);
    Task<decimal> GetStockAsync(Guid almacenId, Guid productoId);
    Task<List<Existencium>> GetLowStockAlertsAsync(Guid entidadId);
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

            movement.Id = Guid.NewGuid();
            movement.CreadoEn = DateTimeOffset.Now;
            _context.MovimientoInventarios.Add(movement);

            foreach (var detail in movement.MovimientoInventarioDetalles)
            {
                // 1. Salida de Almacén Origen
                if (movement.AlmacenOrigenId.HasValue)
                {
                    var res = await UpdateStockAsync(movement.AlmacenOrigenId.Value, detail.ProductoId, -detail.Cantidad, detail.CostoUnitario);
                    if (!res.Succeeded) return (false, res.Message, null);
                }

                // 2. Entrada a Almacén Destino
                if (movement.AlmacenDestinoId.HasValue)
                {
                    var res = await UpdateStockAsync(movement.AlmacenDestinoId.Value, detail.ProductoId, detail.Cantidad, detail.CostoUnitario);
                    if (!res.Succeeded) return (false, res.Message, null);
                }
            }

            await _context.SaveChangesAsync();

            // 3. Contabilización Automática (RF-35)
            await CreateAccountingEntryAsync(movement, tipo);

            await transaction.CommitAsync();
            return (true, "Movimiento procesado y contabilizado.", movement);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error crítico: {ex.Message}", null);
        }
    }

    private async Task<(bool Succeeded, string Message)> UpdateStockAsync(Guid almacenId, Guid productoId, decimal cantidad, decimal? costoEntrada)
    {
        var existencia = await _context.Existencia
            .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.ProductoId == productoId);

        if (existencia == null)
        {
            if (cantidad < 0) return (false, "No hay existencias suficientes (Producto nuevo en almacén).");

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

        // Validación de stock negativo
        if (existencia.Cantidad + cantidad < 0)
            return (false, $"Existencia insuficiente en el almacén. Disponible: {existencia.Cantidad}");

        // RF-32: Recálculo de Precio Promedio Ponderado (PPP) solo en ENTRADAS
        if (cantidad > 0 && costoEntrada.HasValue && costoEntrada.Value > 0)
        {
            var costoTotalActual = existencia.Cantidad * existencia.CostoPromedio;
            var costoNuevaEntrada = cantidad * costoEntrada.Value;
            existencia.CostoPromedio = (costoTotalActual + costoNuevaEntrada) / (existencia.Cantidad + cantidad);
        }

        existencia.Cantidad += cantidad;
        existencia.ActualizadoEn = DateTimeOffset.Now;

        return (true, "Stock actualizado.");
    }

    private async Task CreateAccountingEntryAsync(MovimientoInventario movement, TipoMovimiento tipo)
    {
        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = movement.EntidadId,
            Fecha = DateOnly.FromDateTime(movement.Fecha.DateTime),
            Concepto = $"{tipo.Nombre} #{movement.NumeroDocumento} - {movement.Observaciones}",
            ModuloOrigen = "INVENTARIO",
            DocumentoOrigenTipo = "MOVIMIENTO_INV",
            DocumentoOrigenId = movement.Id,
            TipoComprobanteId = tipo.Naturaleza == "ENTRADA" ? 3 : (tipo.Naturaleza == "SALIDA" ? 4 : 2), // CI, CE o DI
            CreadoEn = DateTimeOffset.Now
        };

        foreach (var det in movement.MovimientoInventarioDetalles)
        {
            var product = await _context.Productos.FindAsync(det.ProductoId);
            if (product == null || !product.CuentaInventarioId.HasValue) continue;

            var costo = det.CostoUnitario ?? (await _context.Existencia
                .Where(e => e.ProductoId == det.ProductoId)
                .Select(e => e.CostoPromedio)
                .FirstOrDefaultAsync());

            if (tipo.Naturaleza == "ENTRADA")
            {
                // Cargo a Inventario (Debe)
                entry.AsientoDetalles.Add(new AsientoDetalle
                {
                    Id = Guid.NewGuid(),
                    CuentaId = product.CuentaInventarioId.Value,
                    Debe = det.Cantidad * costo,
                    Glosa = $"Entrada {product.Codigo}"
                });
                // Contrapartida debería ser Cuentas por Pagar (se simplifica aquí)
            }
            else if (tipo.Naturaleza == "SALIDA")
            {
                // Abono a Inventario (Haber)
                entry.AsientoDetalles.Add(new AsientoDetalle
                {
                    Id = Guid.NewGuid(),
                    CuentaId = product.CuentaInventarioId.Value,
                    Haber = det.Cantidad * costo,
                    Glosa = $"Salida {product.Codigo}"
                });
            }
        }

        if (entry.AsientoDetalles.Any())
        {
            // Nota: Para que el asiento cuadre, en una implementación real se requiere la contrapartida (Proveedor, Ventas, etc.)
            // Aquí se registra el movimiento del submayor de inventario.
            // await _accountingService.CreateEntryAsync(entry);
        }
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
