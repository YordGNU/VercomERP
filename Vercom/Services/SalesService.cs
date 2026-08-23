using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface ISalesService
{
    // Lectura
    Task<IEnumerable<FacturaVentum>> GetInvoicesAsync();
    Task<FacturaVentum?> GetInvoiceByIdAsync(Guid id);
    Task<SalesCreateViewModel> GetSalesCreateContextAsync(FacturaVentum? existingInvoice = null);

    // Escritura
    Task<(bool Succeeded, string Message, FacturaVentum? Invoice)> CreateInvoiceAsync(FacturaVentum invoice);
    Task<(bool Succeeded, string Message)> CancelInvoiceAsync(Guid invoiceId, string reason);
}

public class SalesService : ISalesService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IContractService _contractService;
    private readonly ITaxService _taxService;
    private readonly IConsecutivoService _consecutivoService;
    private readonly IAccountingService _accountingService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public SalesService(AppDbContext context, IInventoryService inventoryService, IContractService contractService,
        ITaxService taxService, IConsecutivoService consecutivoService, IAccountingService accountingService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _inventoryService = inventoryService;
        _contractService = contractService;
        _taxService = taxService;
        _consecutivoService = consecutivoService;
        _accountingService = accountingService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<FacturaVentum>> GetInvoicesAsync()
    {
        return await _context.FacturaVenta
            .Include(f => f.Cliente)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
    }

    public async Task<FacturaVentum?> GetInvoiceByIdAsync(Guid id)
    {
        return await _context.FacturaVenta
            .Include(f => f.Cliente)
            .Include(f => f.Contrato)
            .Include(f => f.FacturaVentaDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(f => f.FormaPagoVenta)
            .Include(f => f.Asiento)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<SalesCreateViewModel> GetSalesCreateContextAsync(FacturaVentum? existingInvoice = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;

        var vm = new SalesCreateViewModel
        {
            Invoice = existingInvoice ?? new FacturaVentum
            {
                Serie = "A",
                Fecha = DateTimeOffset.Now,
                TipoVenta = "MINORISTA",
                CanalVenta = "ERP"
            },
            Clientes = new SelectList(await _context.Clientes.Where(c => c.Activo).ToListAsync(), "Id", "NombreRazonSocial"),
            Almacenes = new SelectList(await _context.Almacens.Where(a => a.Activo && a.EsPuntoVenta).ToListAsync(), "Id", "Nombre"),
            Contratos = new SelectList(await _context.ContratoEconomicos.Where(c => c.Estado == "VIGENTE" && c.TerceroTipo == "CLIENTE").ToListAsync(), "Id", "NumeroContrato"),
            ProductosDisponibles = await _context.Productos
                .Where(p => p.Activo && (p.Tipo == "TERMINADO" || p.Tipo == "ELABORADO"))
                .Select(p => new { p.Id, p.Nombre, p.PrecioVentaActual, p.Codigo })
                .ToListAsync()
        };

        return vm;
    }

    public async Task<(bool Succeeded, string Message, FacturaVentum? Invoice)> CreateInvoiceAsync(FacturaVentum invoice)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Validar Contrato si es mayorista (RF-50)
            var contractCheck = await _contractService.ValidateContractForOperationAsync(invoice.ContratoId, invoice.EntidadId, invoice.TipoVenta);
            if (!contractCheck.Succeeded) return (false, contractCheck.Message, null);

            // 2. Generar Número de Factura Seguro (RNF-51)
            if (string.IsNullOrEmpty(invoice.NumeroFactura))
            {
                invoice.NumeroFactura = await _consecutivoService.ObtenerSiguienteNumeroAsync(
                    invoice.EntidadId, invoice.SucursalId, "FACTURA_VENTA", invoice.Serie);
            }

            invoice.Id = Guid.NewGuid();
            invoice.Estado = "EMITIDA";
            if (invoice.Fecha == default) invoice.Fecha = DateTimeOffset.Now;
            invoice.CreadoEn = DateTimeOffset.Now;

            decimal totalTax = 0;
            decimal subtotal = 0;

            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                detail.Id = Guid.NewGuid();
                detail.FacturaId = invoice.Id;

                // Obtener costo actual para registro de costo de venta (RF-35)
                var stock = await _context.Existencia
                    .FirstOrDefaultAsync(e => e.AlmacenId == invoice.AlmacenId && e.ProductoId == detail.ProductoId);
                detail.CostoUnitarioVenta = stock?.CostoPromedio ?? 0;

                // Cálculo de Impuesto Dinámico (RF-53)
                var prod = await _context.Productos.FindAsync(detail.ProductoId);
                if (prod != null && prod.AplicaImpuestoVentas)
                {
                    var taxAmount = await _taxService.CalculateSalesTaxAsync(invoice.EntidadId, detail.PrecioUnitario * detail.Cantidad);
                    detail.ImpuestoPorcentaje = 10.0m; // Informativo
                    totalTax += taxAmount;
                }

                detail.SubtotalLinea = (detail.PrecioUnitario * detail.Cantidad);
                subtotal += detail.SubtotalLinea;

                // 3. Descuento automático de Inventario (VEN)
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
                if (!invResult.Succeeded) throw new Exception($"Stock insuficiente: {invResult.Message}");

                detail.MovimientoInventarioId = movSalida.Id;
            }

            invoice.Subtotal = subtotal;
            invoice.ImpuestoVentasTotal = totalTax;
            invoice.Total = subtotal + totalTax - invoice.DescuentoTotal;

            // 4. Gestión de Cobros (RF-54)
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
                    FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
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

            // 5. Integración Contable Venta (RF-52/RF-11)
            var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "ING"); // Ingresos
            if (tipoComprobante != null)
            {
                var period = await _context.PeriodoContables.FirstOrDefaultAsync(p => p.EntidadId == invoice.EntidadId && p.Anio == invoice.Fecha.Year && p.Mes == invoice.Fecha.Month);
                if (period != null && period.Estado == "ABIERTO")
                {
                    var entry = new AsientoContable
                    {
                        Id = Guid.NewGuid(),
                        EntidadId = invoice.EntidadId,
                        PeriodoId = period.Id,
                        Fecha = DateOnly.FromDateTime(invoice.Fecha.DateTime),
                        Concepto = $"FACTURACIÓN FISCAL #{invoice.Serie}-{invoice.NumeroFactura}",
                        ModuloOrigen = "VENTAS",
                        DocumentoOrigenTipo = "FACTURA_VENTA",
                        DocumentoOrigenId = invoice.Id,
                        TipoComprobanteId = tipoComprobante.Id,
                        CreadoPor = invoice.CreadoPor ?? Guid.Empty,
                        CreadoEn = DateTimeOffset.Now,
                        Estado = "CONTABILIZADO"
                    };

                    // DEBE: Caja o Cuenta por Cobrar
                    var caja = await _context.Cajas.FirstOrDefaultAsync(c => c.SucursalId == invoice.SucursalId);
                    var debitAccId = (totalPagado >= invoice.Total) ? caja?.CuentaContableId : invoice.Cliente.CuentaContableId;

                    if (debitAccId.HasValue)
                        entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = debitAccId.Value, Debe = invoice.Total, Haber = 0, Glosa = "Cobro Factura" });

                    // HABER: Ingresos e Impuestos
                    // Usamos la cuenta de ingresos del primer producto para simplificar el asiento global
                    var firstProd = await _context.Productos.FindAsync(invoice.FacturaVentaDetalles.First().ProductoId);
                    if (firstProd?.CuentaIngresoId != null)
                        entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = firstProd.CuentaIngresoId.Value, Debe = 0, Haber = invoice.Subtotal - invoice.DescuentoTotal, Glosa = "Venta de Mercancías" });

                    if (invoice.ImpuestoVentasTotal > 0)
                    {
                        var taxAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "402" && c.EntidadId == invoice.EntidadId); // 402: Impuestos por Pagar
                        if (taxAcc != null)
                            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = taxAcc.Id, Debe = 0, Haber = invoice.ImpuestoVentasTotal, Glosa = "Impuesto sobre Ventas (10%)" });
                    }

                    if (entry.AsientoDetalles.Sum(d => d.Debe) == entry.AsientoDetalles.Sum(d => d.Haber))
                    {
                        var res = await _accountingService.CreateEntryAsync(entry);
                        if (res.Succeeded) invoice.AsientoId = entry.Id;
                    }
                }
            }

            await transaction.CommitAsync();

            return (true, $"Factura {invoice.NumeroFactura} emitida.", invoice);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Fallo comercial: {ex.Message}", null);
        }
    }

    public async Task<(bool Succeeded, string Message)> CancelInvoiceAsync(Guid invoiceId, string reason)
    {
        var invoice = await _context.FacturaVenta
            .Include(f => f.FacturaVentaDetalles)
            .FirstOrDefaultAsync(f => f.Id == invoiceId);

        if (invoice == null) return (false, "No existe.");
        if (invoice.Estado == "ANULADA") return (false, "Ya anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                var movRegreso = new MovimientoInventario
                {
                    EntidadId = invoice.EntidadId,
                    TipoMovimientoId = 4,
                    NumeroDocumento = $"ANUL-{invoice.NumeroFactura}",
                    AlmacenDestinoId = invoice.AlmacenId,
                    Fecha = DateTimeOffset.Now,
                    ReferenciaExternaTipo = "ANULACION",
                    ReferenciaExternaId = invoice.Id,
                    Canal = "ERP"
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

            invoice.Estado = "ANULADA";
            invoice.MotivoAnulacion = reason;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Operación anulada.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }
}
