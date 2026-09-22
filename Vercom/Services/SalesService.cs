using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Helpers;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface ISalesService
{
    // Lectura
    Task<IEnumerable<FacturaVentum>> GetInvoicesAsync(string? search = null, string? status = null, string? channel = null);
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
    private readonly ICommercialService _commercialService;
    private readonly ITaxService _taxService;
    private readonly IConsecutivoService _consecutivoService;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public SalesService(AppDbContext context, IInventoryService inventoryService, IContractService contractService,
        ICommercialService commercialService, ITaxService taxService, IConsecutivoService consecutivoService,
        IAccountingService accountingService, IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _inventoryService = inventoryService;
        _contractService = contractService;
        _commercialService = commercialService;
        _taxService = taxService;
        _consecutivoService = consecutivoService;
        _accountingService = accountingService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<FacturaVentum>> GetInvoicesAsync(string? search = null, string? status = null, string? channel = null)
    {
        var query = _context.FacturaVenta
            .Include(f => f.Cliente)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(f => f.NumeroFactura.Contains(search) || f.Cliente.NombreRazonSocial.Contains(search));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(f => f.Estado == status);

        if (!string.IsNullOrEmpty(channel))
            query = query.Where(f => f.CanalVenta == channel);

        return await query
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
                    invoice.EntidadId, invoice.SucursalId, DocumentoTipo.FacturaVenta, invoice.Serie);
            }

            invoice.Id = Guid.NewGuid();
            invoice.Estado = "EMITIDA";
            if (invoice.Fecha == default) invoice.Fecha = DateTimeOffset.Now;
            invoice.CreadoEn = DateTimeOffset.Now;

            decimal totalTax = 0;
            decimal subtotal = 0;
            decimal totalCostoReal = 0;

            // Pre-validar todos los productos y topes antes de iniciar movimientos
            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                var prod = await _context.Productos.Include(p => p.Familia).FirstOrDefaultAsync(p => p.Id == detail.ProductoId);
                if (prod == null) return (false, $"Producto {detail.ProductoId} no encontrado.", null);

                // --- RF-55: Validar Tope de Precio (MFP) ---
                var today = DateOnly.FromDateTime(DateTime.Now);
                var tope = await _context.TopePrecioMfps
                    .Where(t => (t.ProductoId == prod.Id || t.FamiliaId == prod.FamiliaId)
                                && t.VigenteDesde <= today && (t.VigenteHasta == null || t.VigenteHasta >= today))
                    .OrderByDescending(t => t.ProductoId)
                    .FirstOrDefaultAsync();

                if (tope != null && detail.PrecioUnitario > tope.PrecioMaximo)
                {
                    return (false, $"ALERTA LEGAL: El precio de '{prod.Nombre}' ($ {detail.PrecioUnitario:N2}) excede el tope máximo permitido por el MFP ($ {tope.PrecioMaximo:N2}). Operación bloqueada.", null);
                }
            }

            // 3. Procesar Movimiento de Inventario Centralizado (VALE_ENTREGA)
            var movSalida = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                EntidadId = invoice.EntidadId,
                TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("VALE_ENTREGA"),
                NumeroDocumento = invoice.NumeroFactura,
                AlmacenOrigenId = invoice.AlmacenId,
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "FACTURA_VENTA",
                ReferenciaExternaId = invoice.Id,
                Canal = invoice.CanalVenta,
                CreadoPor = invoice.CreadoPor
            };

            foreach (var detail in invoice.FacturaVentaDetalles)
            {
                detail.Id = Guid.NewGuid();
                detail.FacturaId = invoice.Id;

                var prod = await _context.Productos.FindAsync(detail.ProductoId);

                if (prod != null && prod.AplicaImpuestoVentas)
                {
                    var taxAmount = await _taxService.CalculateSalesTaxAsync(invoice.EntidadId, detail.PrecioUnitario * detail.Cantidad);
                    detail.ImpuestoPorcentaje = 10.0m;
                    totalTax += taxAmount;
                }

                detail.SubtotalLinea = (detail.PrecioUnitario * detail.Cantidad);
                subtotal += detail.SubtotalLinea;

                movSalida.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = detail.ProductoId,
                    Cantidad = detail.Cantidad
                });
            }

            var invResult = await _inventoryService.ProcessMovementAsync(movSalida);
            if (!invResult.Succeeded) return (false, $"Stock insuficiente: {invResult.Message}", null);

            // Recuperar costos reales calculados por el motor de inventario (RF-32)
            foreach (var detMov in invResult.Movement!.MovimientoInventarioDetalles)
            {
                var detFac = invoice.FacturaVentaDetalles.First(d => d.ProductoId == detMov.ProductoId);
                detFac.CostoUnitarioVenta = detMov.CostoUnitario ?? 0;
                detFac.MovimientoInventarioId = movSalida.Id;
                totalCostoReal += (decimal)(detFac.CostoUnitarioVenta * detFac.Cantidad);
            }

            invoice.Subtotal = subtotal;
            invoice.ImpuestoVentasTotal = totalTax;
            invoice.Total = subtotal + totalTax - invoice.DescuentoTotal;

            // 4. Gestión de Cobros y Límite de Crédito (RF-54)
            var totalPagado = invoice.FormaPagoVenta.Sum(p => p.Monto);
            if (totalPagado < invoice.Total)
            {
                // Validar Límite de Crédito
                var creditCheck = await _commercialService.ValidateCreditLimitAsync(invoice.ClienteId, invoice.Total - totalPagado);
                if (!creditCheck.Succeeded) return (false, creditCheck.Message, null);

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

            // 5. Integración Contable Refinada (RF-52)
            var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "ING");
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

                    // A. DEBE: Cobro (Caja/Banco o Cuenta por Cobrar)
                    var ctaCaja = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_CAJA_MN") ?? "101";
                    var ctaCxC = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_CXC_CLIENTES") ?? "135";

                    if (totalPagado > 0)
                    {
                        var acc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == invoice.EntidadId);
                        if (acc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = acc.Id, Debe = totalPagado, Glosa = "Cobro Contado" });
                    }
                    if (invoice.Total - totalPagado > 0)
                    {
                        var acc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCxC && c.EntidadId == invoice.EntidadId);
                        if (acc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = acc.Id, Debe = invoice.Total - totalPagado, Glosa = "Venta a Crédito" });
                    }

                    // B. HABER: Ingresos e Impuestos
                    var ctaVentas = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_VENTAS_GENERAL") ?? "500.0100";
                    var ctaImp = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_IMP_VENTAS") ?? "440.0001";

                    var salesAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaVentas && c.EntidadId == invoice.EntidadId);
                    if (salesAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = salesAcc.Id, Haber = invoice.Subtotal - invoice.DescuentoTotal, Glosa = "Ingresos por Ventas" });

                    if (invoice.ImpuestoVentasTotal > 0)
                    {
                        var taxAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaImp && c.EntidadId == invoice.EntidadId);
                        if (taxAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = taxAcc.Id, Haber = invoice.ImpuestoVentasTotal, Glosa = "Impuesto sobre Ventas" });
                    }

                    // C. COSTO DE VENTA (RF-35): DEBE Costo / HABER Inventario
                    if (totalCostoReal > 0)
                    {
                        var ctaCosto = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_COSTO_VENTAS") ?? "810";
                        var ctaInv = await _paramService.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_INV_GENERICA") ?? "183.0010";

                        var costAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCosto && c.EntidadId == invoice.EntidadId);
                        var invAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaInv && c.EntidadId == invoice.EntidadId);

                        if (costAcc != null && invAcc != null)
                        {
                            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = costAcc.Id, Debe = totalCostoReal, Glosa = "Costo de Mercancía Vendida" });
                            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = invAcc.Id, Haber = totalCostoReal, Glosa = "Baja de Inventario por Venta" });
                        }
                    }

                    if (entry.AsientoDetalles.Sum(d => d.Debe) == entry.AsientoDetalles.Sum(d => d.Haber))
                    {
                        var res = await _accountingService.CreateEntryAsync(entry);
                        if (res.Succeeded) invoice.AsientoId = entry.Id;
                    }
                }
            }

            await transaction.CommitAsync();
            return (true, $"Factura {invoice.NumeroFactura} emitida y contabilizada.", invoice);
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
                    TipoMovimientoId = await _inventoryService.EnsureMovementTypeAsync("DEVOLUCION_ENTRADA"),
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

            // Revertir Asiento Contable si existe (RF-12)
            if (invoice.AsientoId.HasValue)
            {
                var revResult = await _accountingService.ReverseEntryAsync(invoice.AsientoId.Value, $"Anulación Factura {invoice.NumeroFactura}: {reason}");
                if (!revResult.Succeeded) throw new Exception($"No se pudo revertir el asiento: {revResult.Message}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Operación anulada e impacto contable revertido.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }
}
