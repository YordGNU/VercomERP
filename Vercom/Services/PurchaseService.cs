using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IPurchaseService
{
    // Lectura
    Task<IEnumerable<OrdenCompra>> GetPurchaseOrdersAsync(string? search = null, string? status = null);
    Task<OrdenCompra?> GetPurchaseOrderByIdAsync(Guid id);
    Task<PurchaseOrderViewModel> GetPurchaseOrderCreateContextAsync(OrdenCompra? existing = null);

    // Escritura
    Task<(bool Succeeded, string Message, OrdenCompra? Order)> CreatePurchaseOrderAsync(OrdenCompra order);
    Task<(bool Succeeded, string Message)> ApprovePurchaseOrderAsync(Guid orderId, Guid userId);
    Task<(bool Succeeded, string Message)> ReceivePurchaseAsync(Guid orderId, Guid almacenId, Guid userId, string receiptNumber);
}

public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IContractService _contractService;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public PurchaseService(AppDbContext context, IInventoryService inventoryService, IContractService _contractService,
        IAccountingService accountingService, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _inventoryService = inventoryService;
        this._contractService = _contractService;
        _accountingService = accountingService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<OrdenCompra>> GetPurchaseOrdersAsync(string? search = null, string? status = null)
    {
        var query = _context.OrdenCompras
            .Include(o => o.Proveedor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(o => o.NumeroOrden.Contains(search) || o.Proveedor.RazonSocial.Contains(search));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Estado == status);

        return await query
            .OrderByDescending(o => o.Fecha)
            .ToListAsync();
    }

    public async Task<OrdenCompra?> GetPurchaseOrderByIdAsync(Guid id)
    {
        return await _context.OrdenCompras
            .Include(o => o.Proveedor)
            .Include(o => o.Contrato)
            .Include(o => o.AlmacenDestino)
            .Include(o => o.OrdenCompraDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(o => o.RecepcionCompras)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<PurchaseOrderViewModel> GetPurchaseOrderCreateContextAsync(OrdenCompra? existing = null)
    {
        return new PurchaseOrderViewModel
        {
            Order = existing ?? new OrdenCompra { Fecha = DateOnly.FromDateTime(DateTime.Now) },
            Proveedores = new SelectList(await _context.Proveedors.Where(p => p.Activo).ToListAsync(), "Id", "RazonSocial"),
            Almacenes = new SelectList(await _context.Almacens.Where(a => a.Activo).ToListAsync(), "Id", "Nombre"),
            Contratos = new SelectList(await _context.ContratoEconomicos.Where(c => c.Estado == "VIGENTE" && c.TerceroTipo == "PROVEEDOR").ToListAsync(), "Id", "NumeroContrato"),
            ProductosDisponibles = await _context.Productos.Where(p => p.Activo)
                .Select(p => new { p.Id, p.Nombre, p.Codigo }).ToListAsync()
        };
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

            // 4. Integración Contable de Compra (REC - Recepción)
            var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "REC")
                                ?? await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "DIA");

            if (tipoComprobante != null)
            {
                var period = await _context.PeriodoContables.FirstOrDefaultAsync(p => p.EntidadId == order.EntidadId && p.Anio == order.Fecha.Year && p.Mes == order.Fecha.Month);
                if (period != null && period.Estado == "ABIERTO")
                {
                    var entry = new AsientoContable
                    {
                        Id = Guid.NewGuid(),
                        EntidadId = order.EntidadId,
                        PeriodoId = period.Id,
                        Fecha = DateOnly.FromDateTime(DateTime.Now),
                        Concepto = $"RECEPCIÓN DE COMPRA - ORDEN #{order.NumeroOrden} - PROV: {order.Proveedor.RazonSocial}",
                        ModuloOrigen = "COMPRAS",
                        DocumentoOrigenTipo = "ORDEN_COMPRA",
                        DocumentoOrigenId = order.Id,
                        TipoComprobanteId = tipoComprobante.Id,
                        CreadoPor = userId,
                        CreadoEn = DateTimeOffset.Now,
                        Estado = "CONTABILIZADO"
                    };

                    // DEBE: Inventario (por cada producto o agrupado)
                    // Para simplificar el piloto, agrupamos por cuenta de inventario
                    var detailsGrouped = order.OrdenCompraDetalles
                        .GroupBy(d => d.Producto.CuentaInventarioId)
                        .ToList();

                    foreach (var group in detailsGrouped)
                    {
                        var accountId = group.Key;
                        if (accountId == null)
                        {
                            // Fallback a parámetro o cuenta genérica 183
                            var ctaInv = await _paramService.ObtenerValorVigenteAsync(order.EntidadId, "CTA_INV_GENERICA") ?? "183.0010";
                            var acc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaInv && c.EntidadId == order.EntidadId);
                            accountId = acc?.Id;
                        }

                        if (accountId.HasValue)
                        {
                            entry.AsientoDetalles.Add(new AsientoDetalle
                            {
                                Id = Guid.NewGuid(),
                                CuentaId = accountId.Value,
                                Debe = group.Sum(d => d.CantidadSolicitada * d.PrecioUnitario),
                                Haber = 0,
                                Glosa = $"Entrada Almacén - OC {order.NumeroOrden}"
                            });
                        }
                    }

                    // HABER: Cuentas por Pagar (Proveedor)
                    var ctaPasivo = await _paramService.ObtenerValorVigenteAsync(order.EntidadId, "CTA_CXP_PROVEEDORES") ?? "405.0020";
                    var payableAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaPasivo && c.EntidadId == order.EntidadId);

                    if (payableAcc != null)
                    {
                        entry.AsientoDetalles.Add(new AsientoDetalle
                        {
                            Id = Guid.NewGuid(),
                            CuentaId = payableAcc.Id,
                            Debe = 0,
                            Haber = order.Total,
                            Glosa = $"Obligación con Proveedor - {order.Proveedor.RazonSocial}"
                        });
                    }

                    // Validar Partida Doble antes de registrar
                    if (entry.AsientoDetalles.Any() && entry.AsientoDetalles.Sum(d => d.Debe) == entry.AsientoDetalles.Sum(d => d.Haber))
                    {
                        await _accountingService.CreateEntryAsync(entry);
                    }
                }
            }

            await transaction.CommitAsync();
            return (true, "Compra recibida, inventario actualizado y obligación contable registrada.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al recibir compra: {ex.Message}");
        }
    }
}
