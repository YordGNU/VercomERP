using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface ISalesService
{
    Task<(bool Succeeded, string Message, FacturaVentum? Invoice)> CreateInvoiceAsync(FacturaVentum invoice);
    Task<(bool Succeeded, string Message)> CancelInvoiceAsync(Guid invoiceId, string reason);
}

public class SalesService : ISalesService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IContractService _contractService;
    private readonly ITaxService _taxService;

    public SalesService(AppDbContext context, IInventoryService inventoryService, IContractService contractService, ITaxService taxService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _contractService = contractService;
        _taxService = taxService;
    }

    public async Task<(bool Succeeded, string Message, FacturaVentum? Invoice)> CreateInvoiceAsync(FacturaVentum invoice)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Validar Contrato si es mayorista
            var contractCheck = await _contractService.ValidateContractForOperationAsync(invoice.ContratoId, invoice.EntidadId, invoice.TipoVenta);
            if (!contractCheck.Succeeded) return (false, contractCheck.Message, null);

            // 2. Generar Número de Factura (RNF-51)
            // Se usa el consecutivo centralizado del núcleo
            var lastNum = await _context.FacturaVenta
                .Where(f => f.EntidadId == invoice.EntidadId && f.Serie == invoice.Serie)
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (lastNum != null && int.TryParse(lastNum, out var parsed)) nextNum = parsed + 1;
            invoice.NumeroFactura = nextNum.ToString().PadLeft(8, '0');

            invoice.Id = Guid.NewGuid();
            invoice.Estado = "EMITIDA";
            invoice.Fecha = DateTimeOffset.Now;
            invoice.CreadoEn = DateTimeOffset.Now;

            decimal totalTax = 0;
            decimal subtotal = 0;

            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                detail.Id = Guid.NewGuid();
                detail.FacturaId = invoice.Id;

                // Obtener costo actual (para registro de costo de venta)
                var stock = await _context.Existencia
                    .FirstOrDefaultAsync(e => e.AlmacenId == invoice.AlmacenId && e.ProductoId == detail.ProductoId);
                detail.CostoUnitarioVenta = stock?.CostoPromedio ?? 0;

                // Calcular Impuesto
                var prod = await _context.Productos.FindAsync(detail.ProductoId);
                if (prod != null && prod.AplicaImpuestoVentas)
                {
                    detail.ImpuestoPorcentaje = 10.0m; // MiPyMES tasa general
                    totalTax += (detail.PrecioUnitario * detail.Cantidad * 0.10m);
                }

                detail.SubtotalLinea = (detail.PrecioUnitario * detail.Cantidad);
                subtotal += detail.SubtotalLinea;

                // 3. Descuento automático de Inventario (VEN: Salida por Venta)
                var movSalida = new MovimientoInventario
                {
                    EntidadId = invoice.EntidadId,
                    TipoMovimientoId = 3, // VEN
                    NumeroDocumento = invoice.NumeroFactura,
                    AlmacenOrigenId = invoice.AlmacenId,
                    Fecha = DateTimeOffset.Now,
                    ReferenciaExternaTipo = "FACTURA_VENTA",
                    ReferenciaExternaId = invoice.Id,
                    Canal = invoice.CanalVenta,
                    CreadoPor = invoice.CreadoPor
                };
                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = detail.ProductoId,
                    Cantidad = detail.Cantidad,
                    CostoUnitario = detail.CostoUnitarioVenta
                });

                var invResult = await _inventoryService.ProcessMovementAsync(movSalida);
                if (!invResult.Succeeded) throw new Exception($"Stock insuficiente para {prod?.Nombre}: {invResult.Message}");

                detail.MovimientoInventarioId = movSalida.Id;
            }

            invoice.Subtotal = subtotal;
            invoice.ImpuestoVentasTotal = totalTax;
            invoice.Total = subtotal + totalTax - invoice.DescuentoTotal;

            // 4. Generar Cuenta por Cobrar si no es pagada totalmente en efectivo al momento
            var totalPagado = invoice.FormaPagoVenta.Sum(p => p.Monto);
            if (totalPagado < invoice.Total)
            {
                var cxc = new CuentaPorCobrar
                {
                    Id = Guid.NewGuid(),
                    EntidadId = invoice.EntidadId,
                    ClienteId = invoice.ClienteId,
                    DocumentoOrigenTipo = "FACTURA_VENTA",
                    DocumentoOrigenId = invoice.Id,
                    FechaEmision = DateOnly.FromDateTime(DateTime.Now),
                    FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(15)), // Ejemplo
                    MontoOriginal = invoice.Total,
                    SaldoPendiente = invoice.Total - totalPagado,
                    Estado = totalPagado > 0 ? "PARCIAL" : "PENDIENTE",
                    CreadoEn = DateTimeOffset.Now
                };
                _context.CuentaPorCobrars.Add(cxc);
                invoice.CuentaPorCobrarId = cxc.Id;
            }

            _context.FacturaVenta.Add(invoice);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Factura {invoice.NumeroFactura} emitida con éxito.", invoice);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al facturar: {ex.Message}", null);
        }
    }

    public async Task<(bool Succeeded, string Message)> CancelInvoiceAsync(Guid invoiceId, string reason)
    {
        var invoice = await _context.FacturaVenta
            .Include(f => f.FacturaVentaDetalles)
            .FirstOrDefaultAsync(f => f.Id == invoiceId);

        if (invoice == null) return (false, "Factura no encontrada.");
        if (invoice.Estado == "ANULADA") return (false, "La factura ya está anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Revertir Inventario
            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                var movRegreso = new MovimientoInventario
                {
                    EntidadId = invoice.EntidadId,
                    TipoMovimientoId = 4, // AJU: Ajuste (Reingreso por anulación)
                    NumeroDocumento = $"REV-{invoice.NumeroFactura}",
                    AlmacenDestinoId = invoice.AlmacenId,
                    Fecha = DateTimeOffset.Now,
                    ReferenciaExternaTipo = "ANULACION_FACTURA",
                    ReferenciaExternaId = invoice.Id,
                    Canal = "ERP",
                    Observaciones = $"Reingreso por anulación: {reason}"
                };
                movRegreso.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = detail.ProductoId,
                    Cantidad = detail.Cantidad,
                    CostoUnitario = detail.CostoUnitarioVenta
                });
                await _inventoryService.ProcessMovementAsync(movRegreso);
            }

            // 2. Marcar como anulada
            invoice.Estado = "ANULADA";
            invoice.MotivoAnulacion = reason;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Factura anulada y stock devuelto.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al anular: {ex.Message}");
        }
    }
}
