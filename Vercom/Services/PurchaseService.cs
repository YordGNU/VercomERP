using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IPurchaseService
{
    Task<(bool Succeeded, string Message, OrdenCompra? Order)> CreatePurchaseOrderAsync(OrdenCompra order);
    Task<(bool Succeeded, string Message)> ApprovePurchaseOrderAsync(Guid orderId, Guid userId);
    Task<(bool Succeeded, string Message)> ReceivePurchaseAsync(Guid orderId, Guid almacenId, Guid userId, string receiptNumber);
}

public class PurchaseService : IPurchaseService
{
   private readonly AppDbContext _context;  
    private readonly IInventoryService _inventoryService;
    private readonly IContractService _contractService;

    public PurchaseService(AppDbContext context, IInventoryService inventoryService, IContractService _contractService)
    {
        _context = context;
        _inventoryService = inventoryService;
        this._contractService = _contractService;
    }

    public async Task<(bool Succeeded, string Message, OrdenCompra? Order)> CreatePurchaseOrderAsync(OrdenCompra order)
    {
        // Validar contrato si existe
        if (order.ContratoId.HasValue)
        {
            var contractValid = await _contractService.IsValidContractAsync(order.ContratoId, order.EntidadId);
            if (!contractValid) return (false, "El contrato seleccionado no es válido o está vencido.", null);
        }

        order.Id = Guid.NewGuid();
        order.NumeroOrden = $"OC-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        order.Estado = "BORRADOR";
        order.Fecha = DateOnly.FromDateTime(DateTime.Now);
        order.CreadoEn = DateTimeOffset.Now;

        // Calcular totales
        order.Subtotal = order.OrdenCompraDetalles.Sum(d => d.CantidadSolicitada * d.PrecioUnitario);
        order.Total = order.Subtotal; // En compras mayoristas a veces no se aplica tax directo aquí

        _context.OrdenCompras.Add(order);
        await _context.SaveChangesAsync();

        return (true, "Orden de compra creada.", order);
    }

    public async Task<(bool Succeeded, string Message)> ApprovePurchaseOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.OrdenCompras.FindAsync(orderId);
        if (order == null) return (false, "Orden no encontrada.");
        if (order.Estado != "BORRADOR") return (false, "La orden ya ha sido procesada.");

        order.Estado = "APROBADA";
        order.AprobadoPor = userId;
        await _context.SaveChangesAsync();

        return (true, "Orden de compra aprobada.");
    }

    public async Task<(bool Succeeded, string Message)> ReceivePurchaseAsync(Guid orderId, Guid almacenId, Guid userId, string receiptNumber)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.OrdenCompras
                .Include(o => o.OrdenCompraDetalles)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return (false, "Orden no encontrada.");
            if (order.Estado != "APROBADA") return (false, "La orden no está aprobada para recepción.");

            // 1. Crear Movimiento de Inventario (REC: Recepción)
            var movement = new MovimientoInventario
            {
                EntidadId = order.EntidadId,
                TipoMovimientoId = 1, // REC
                NumeroDocumento = receiptNumber,
                AlmacenDestinoId = almacenId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "ORDEN_COMPRA",
                ReferenciaExternaId = order.Id,
                Canal = "ERP",
                CreadoPor = userId
            };

            foreach (var item in order.OrdenCompraDetalles)
            {
                movement.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = item.ProductoId,
                    Cantidad = item.CantidadSolicitada,
                    CostoUnitario = item.PrecioUnitario
                });
                item.CantidadRecibida = item.CantidadSolicitada;
            }

            var invResult = await _inventoryService.ProcessMovementAsync(movement);
            if (!invResult.Succeeded) throw new Exception(invResult.Message);

            // 2. Crear Cuenta por Pagar (RF-15)
            var cxp = new CuentaPorPagar
            {
                Id = Guid.NewGuid(),
                EntidadId = order.EntidadId,
                ProveedorId = order.ProveedorId,
                DocumentoOrigenTipo = "RECEPCION_COMPRA",
                DocumentoOrigenId = movement.Id,
                FechaEmision = DateOnly.FromDateTime(DateTime.Now),
                FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)), // Ejemplo
                MontoOriginal = order.Total,
                SaldoPendiente = order.Total,
                Estado = "PENDIENTE",
                CreadoEn = DateTimeOffset.Now
            };
            _context.CuentaPorPagars.Add(cxp);

            // 3. Crear Registro de Recepción
            var recepcion = new RecepcionCompra
            {
                Id = Guid.NewGuid(),
                OrdenCompraId = order.Id,
                MovimientoInventarioId = movement.Id,
                CuentaPorPagarId = cxp.Id,
                NumeroInformeRecepcion = receiptNumber,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                RecibidoPor = userId
            };
            _context.RecepcionCompras.Add(recepcion);

            order.Estado = "RECIBIDA";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Compra recibida, inventario actualizado y cuenta por pagar generada.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al recibir compra: {ex.Message}");
        }
    }
}
