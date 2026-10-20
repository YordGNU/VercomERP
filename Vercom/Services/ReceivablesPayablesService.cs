using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using VERCOM.Models;
using VERCOM.ViewModels;

namespace VERCOM.Services;

public class AgingReportRow
{
    public string NombreTercero { get; set; } = null!;
    public decimal PorVencer { get; set; }
    public decimal De1a30Dias { get; set; }
    public decimal De31a60Dias { get; set; }
    public decimal MasDe60Dias { get; set; }
    public decimal Total => PorVencer + De1a30Dias + De31a60Dias + MasDe60Dias;
}

public interface IReceivablesPayablesService
{
    Task<List<AgingReportRow>> GetReceivablesAgingAsync(Guid entidadId);
    Task<List<AgingReportRow>> GetPayablesAgingAsync(Guid entidadId);

    Task<CxCIndexViewModel> GetCxCIndexAsync(Guid entidadId, string? search = null, string? documentoTipo = null, string? estado = null, bool vencidasOnly = false, int page = 1, int pageSize = 12);
    Task<CxPIndexViewModel> GetCxPIndexAsync(Guid entidadId, string? search = null, string? documentoTipo = null, string? estado = null, bool vencidasOnly = false, int page = 1, int pageSize = 12);

    Task<CxCFormViewModel> GetCxCFormContextAsync(Guid entidadId, CuentaPorCobrar? existing = null);
    Task<CxPFormViewModel> GetCxPFormContextAsync(Guid entidadId, CuentaPorPagar? existing = null);

    Task<CuentaPorCobrar?> GetCxCByIdAsync(Guid id);
    Task<CuentaPorPagar?> GetCxPByIdAsync(Guid id);

    Task<(bool Succeeded, string Message)> CreateCxCAsync(CuentaPorCobrar item, Guid entidadId, Guid userId);
    Task<(bool Succeeded, string Message)> UpdateCxCAsync(CuentaPorCobrar item, Guid entidadId, Guid userId);
    Task<(bool Succeeded, string Message)> CreateCxPAsync(CuentaPorPagar item, Guid entidadId, Guid userId);
    Task<(bool Succeeded, string Message)> UpdateCxPAsync(CuentaPorPagar item, Guid entidadId, Guid userId);

    Task<(bool Succeeded, string Message)> RecordCollectionAsync(Guid cxcId, decimal amount, string paymentMethod, string? reference, Guid userId);
    Task<(bool Succeeded, string Message)> RecordPaymentAsync(Guid cxpId, decimal amount, string paymentMethod, string? reference, Guid userId);
}

public class ReceivablesPayablesService : IReceivablesPayablesService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;

    public ReceivablesPayablesService(AppDbContext context, IAccountingService accountingService, IParametroSistemaService paramService)
    {
        _context = context;
        _accountingService = accountingService;
        _paramService = paramService;
    }

    public async Task<List<AgingReportRow>> GetReceivablesAgingAsync(Guid entidadId)
    {
        var items = await _context.CuentaPorCobrars
            .Include(c => c.Cliente)
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .ToListAsync();

        return CalculateAging(items.Select(i => new { NombreRazonSocial = i.Cliente.NombreRazonSocial, i.SaldoPendiente, i.FechaVencimiento }));
    }

    public async Task<List<AgingReportRow>> GetPayablesAgingAsync(Guid entidadId)
    {
        var items = await _context.CuentaPorPagars
            .Include(c => c.Proveedor)
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .ToListAsync();

        return CalculateAging(items.Select(i => new { NombreRazonSocial = i.Proveedor.RazonSocial, i.SaldoPendiente, i.FechaVencimiento }));
    }

    private List<AgingReportRow> CalculateAging(IEnumerable<dynamic> items)
    {
        var report = new List<AgingReportRow>();
        var grouped = items.GroupBy(i => i.NombreRazonSocial);

        foreach (var group in grouped)
        {
            var row = new AgingReportRow { NombreTercero = group.Key };
            foreach (var item in group)
            {
                var daysLate = (DateTime.Now - item.FechaVencimiento.ToDateTime(TimeOnly.MinValue)).Days;

                if (daysLate <= 0) row.PorVencer += item.SaldoPendiente;
                else if (daysLate <= 30) row.De1a30Dias += item.SaldoPendiente;
                else if (daysLate <= 60) row.De31a60Dias += item.SaldoPendiente;
                else row.MasDe60Dias += item.SaldoPendiente;
            }
            report.Add(row);
        }

        return report;
    }

    public async Task<(bool Succeeded, string Message)> RecordCollectionAsync(Guid cxcId, decimal amount, string paymentMethod, string? reference, Guid userId)
    {
        var cxc = await _context.CuentaPorCobrars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cxcId);
        if (cxc == null) return (false, "Cuenta por cobrar no encontrada.");
        if (amount > cxc.SaldoPendiente) return (false, "El monto del cobro excede el saldo pendiente.");
        if (!EsFormaPagoAdmitida(paymentMethod)) return (false, "Forma de pago no admitida.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = cxc.EntidadId,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"COBRO A CLIENTE - REF: {reference}",
                ModuloOrigen = "CONTABILIDAD",
                TipoComprobanteId = 3, // Ingreso
                Estado = "CONTABILIZADO",
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now
            };

            // Debe: Caja o Banco
            var ctaCaja = await _paramService.ObtenerValorVigenteAsync(cxc.EntidadId, "CTA_CAJA_MN") ?? "101";
            var cashAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == cxc.EntidadId);
            if (cashAccount == null) return (false, "Error de Configuración: La cuenta de caja no está definida.");

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = cashAccount.Id,
                Debe = amount,
                Glosa = $"Cobro Factura CxC"
            });

            // Haber: Cuentas por Cobrar
            var ctaCxC = await _paramService.ObtenerValorVigenteAsync(cxc.EntidadId, "CTA_CXC_CLIENTES") ?? "135";
            var cxcAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCxC && c.EntidadId == cxc.EntidadId);
            if (cxcAccount == null) return (false, "Error de Configuración: La cuenta de clientes no está definida.");

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = cxcAccount.Id,
                Haber = amount,
                Glosa = $"Cobro CxC {cxc.Id}"
            });

            var result = await _accountingService.CreateEntryAsync(entry);
            if (!result.Succeeded) throw new Exception(result.Message);

            // Actualización atómica del saldo con guarda de concurrencia: si dos cobros
            // simultáneos compiten, solo uno supera el WHERE y el otro se aborta sin sobregirar.
            var rows = await _context.CuentaPorCobrars
                .Where(c => c.Id == cxcId && c.SaldoPendiente >= amount)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.SaldoPendiente, c => c.SaldoPendiente - amount)
                    .SetProperty(c => c.Estado, c => c.SaldoPendiente - amount == 0 ? "PAGADA" : "PARCIAL"));

            if (rows == 0) throw new Exception("El saldo del documento cambió; verifique e intente de nuevo.");

            _context.PagoAplicados.Add(new PagoAplicado
            {
                Id = Guid.NewGuid(),
                CuentaPorCobrarId = cxc.Id,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Monto = amount,
                FormaPago = paymentMethod,
                ReferenciaExterna = reference,
                AsientoId = entry.Id,
                Tipo = "COBRO"
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Cobro registrado y contabilizado.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al cobrar: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> RecordPaymentAsync(Guid cxpId, decimal amount, string paymentMethod, string? reference, Guid userId)
    {
        var cxp = await _context.CuentaPorPagars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cxpId);
        if (cxp == null) return (false, "Cuenta por pagar no encontrada.");
        if (amount > cxp.SaldoPendiente) return (false, "El monto del pago excede el saldo pendiente.");
        if (!EsFormaPagoAdmitida(paymentMethod)) return (false, "Forma de pago no admitida.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = cxp.EntidadId,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"PAGO A PROVEEDOR - REF: {reference}",
                ModuloOrigen = "CONTABILIDAD",
                TipoComprobanteId = 4, // Egreso
                Estado = "CONTABILIZADO",
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now
            };

            // Debe: Cuentas por Pagar
            var ctaCxP = await _paramService.ObtenerValorVigenteAsync(cxp.EntidadId, "CTA_CXP_PROVEEDORES") ?? "405";
            var cxpAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCxP && c.EntidadId == cxp.EntidadId);
            if (cxpAccount == null) return (false, "Error de Configuración: La cuenta de proveedores no está definida.");

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = cxpAccount.Id,
                Debe = amount,
                Glosa = $"Pago CxP {cxp.Id}"
            });

            // Haber: Caja o Banco
            var ctaCaja = await _paramService.ObtenerValorVigenteAsync(cxp.EntidadId, "CTA_CAJA_MN") ?? "101";
            var cashAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == cxp.EntidadId);
            if (cashAccount == null) return (false, "Error de Configuración: La cuenta de caja no está definida.");

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = cashAccount.Id,
                Haber = amount,
                Glosa = $"Pago CxP {cxp.Id}"
            });

            var result = await _accountingService.CreateEntryAsync(entry);
            if (!result.Succeeded) throw new Exception(result.Message);

            // Actualización atómica del saldo con guarda de concurrencia (ver RecordCollectionAsync).
            var rows = await _context.CuentaPorPagars
                .Where(c => c.Id == cxpId && c.SaldoPendiente >= amount)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.SaldoPendiente, c => c.SaldoPendiente - amount)
                    .SetProperty(c => c.Estado, c => c.SaldoPendiente - amount == 0 ? "PAGADA" : "PARCIAL"));

            if (rows == 0) throw new Exception("El saldo del documento cambió; verifique e intente de nuevo.");

            _context.PagoAplicados.Add(new PagoAplicado
            {
                Id = Guid.NewGuid(),
                CuentaPorPagarId = cxp.Id,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Monto = amount,
                FormaPago = paymentMethod,
                ReferenciaExterna = reference,
                AsientoId = entry.Id,
                Tipo = "PAGO"
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Pago registrado y contabilizado.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al pagar: {ex.Message}");
        }
    }

    private static bool EsFormaPagoAdmitida(string? formaPago)
    {
        // Solo formas almacenables (pago_aplicado.forma_pago = nvarchar(20)) y admitidas por el CHECK de la BD.
        return formaPago is "EFECTIVO" or "CHEQUE" or "ENZONA" or "TRANSFERMOVIL";
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 6, 48);
        return (page, pageSize, (page - 1) * pageSize);
    }

    public async Task<CxCIndexViewModel> GetCxCIndexAsync(Guid entidadId, string? search = null, string? documentoTipo = null, string? estado = null, bool vencidasOnly = false, int page = 1, int pageSize = 12)
    {
        var p = NormalizePaging(page, pageSize);
        var today = DateOnly.FromDateTime(DateTime.Now);

        var query = _context.CuentaPorCobrars.AsNoTracking()
            .Where(c => c.EntidadId == entidadId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Cliente.NombreRazonSocial.Contains(term)
                || c.Cliente.NitOCi.Contains(term)
                || (c.DocumentoOrigenNumero != null && c.DocumentoOrigenNumero.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(documentoTipo))
        {
            var t = documentoTipo.Trim();
            query = query.Where(c => c.DocumentoOrigenTipo == t);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var st = estado.Trim();
            query = query.Where(c => c.Estado == st);
        }

        if (vencidasOnly)
            query = query.Where(c => c.FechaVencimiento < today && c.Estado != "PAGADA");

        var totalItems = await query.CountAsync();
        var items = await query.OrderBy(c => c.Estado == "PAGADA").ThenBy(c => c.FechaVencimiento)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .Include(c => c.Cliente)
            .ToListAsync();

        var abierto = await _context.CuentaPorCobrars
            .Where(c => c.EntidadId == entidadId && c.Estado != "PAGADA")
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;
        var vencido = await _context.CuentaPorCobrars
            .Where(c => c.EntidadId == entidadId && c.Estado != "PAGADA" && c.FechaVencimiento < today)
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;
        var aplicado = await _context.PagoAplicados
            .Where(x => x.CuentaPorCobrarId != null && x.CuentaPorCobrar!.EntidadId == entidadId)
            .SumAsync(x => (decimal?)x.Monto) ?? 0;
        var abiertos = await _context.CuentaPorCobrars.CountAsync(c => c.EntidadId == entidadId && c.Estado != "PAGADA");

        return new CxCIndexViewModel
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)p.PageSize),
            Search = search,
            DocumentoTipo = documentoTipo,
            Estado = estado,
            VencidasOnly = vencidasOnly,
            TotalAbierto = abierto,
            TotalVencido = vencido,
            TotalPorVencer = abierto - vencido,
            TotalAplicado = aplicado,
            DocumentosAbiertos = abiertos,
            Aging = await GetReceivablesAgingAsync(entidadId)
        };
    }

    public async Task<CxPIndexViewModel> GetCxPIndexAsync(Guid entidadId, string? search = null, string? documentoTipo = null, string? estado = null, bool vencidasOnly = false, int page = 1, int pageSize = 12)
    {
        var p = NormalizePaging(page, pageSize);
        var today = DateOnly.FromDateTime(DateTime.Now);

        var query = _context.CuentaPorPagars.AsNoTracking()
            .Where(c => c.EntidadId == entidadId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Proveedor.RazonSocial.Contains(term)
                || c.Proveedor.Nit.Contains(term)
                || (c.DocumentoOrigenNumero != null && c.DocumentoOrigenNumero.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(documentoTipo))
        {
            var t = documentoTipo.Trim();
            query = query.Where(c => c.DocumentoOrigenTipo == t);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var st = estado.Trim();
            query = query.Where(c => c.Estado == st);
        }

        if (vencidasOnly)
            query = query.Where(c => c.FechaVencimiento < today && c.Estado != "PAGADA");

        var totalItems = await query.CountAsync();
        var items = await query.OrderBy(c => c.Estado == "PAGADA").ThenBy(c => c.FechaVencimiento)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .Include(c => c.Proveedor)
            .ToListAsync();

        var abierto = await _context.CuentaPorPagars
            .Where(c => c.EntidadId == entidadId && c.Estado != "PAGADA")
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;
        var vencido = await _context.CuentaPorPagars
            .Where(c => c.EntidadId == entidadId && c.Estado != "PAGADA" && c.FechaVencimiento < today)
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;
        var aplicado = await _context.PagoAplicados
            .Where(x => x.CuentaPorPagarId != null && x.CuentaPorPagar!.EntidadId == entidadId)
            .SumAsync(x => (decimal?)x.Monto) ?? 0;
        var abiertos = await _context.CuentaPorPagars.CountAsync(c => c.EntidadId == entidadId && c.Estado != "PAGADA");

        return new CxPIndexViewModel
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)p.PageSize),
            Search = search,
            DocumentoTipo = documentoTipo,
            Estado = estado,
            VencidasOnly = vencidasOnly,
            TotalAbierto = abierto,
            TotalVencido = vencido,
            TotalPorVencer = abierto - vencido,
            TotalAplicado = aplicado,
            DocumentosAbiertos = abiertos,
            Aging = await GetPayablesAgingAsync(entidadId)
        };
    }

    public async Task<CxCFormViewModel> GetCxCFormContextAsync(Guid entidadId, CuentaPorCobrar? existing = null)
    {
        var clientes = await _context.Clientes
            .Where(c => c.EntidadId == entidadId && c.Activo)
            .OrderBy(c => c.NombreRazonSocial)
            .Select(c => new { c.Id, Nombre = c.NombreRazonSocial })
            .ToListAsync();

        return new CxCFormViewModel
        {
            Item = existing ?? new CuentaPorCobrar
            {
                Estado = "PENDIENTE",
                Moneda = "CUP",
                DocumentoOrigenTipo = "FACTURA",
                FechaEmision = DateOnly.FromDateTime(DateTime.Now),
                FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
            },
            Clientes = new SelectList(clientes, "Id", "Nombre")
        };
    }

    public async Task<CxPFormViewModel> GetCxPFormContextAsync(Guid entidadId, CuentaPorPagar? existing = null)
    {
        var proveedores = await _context.Proveedors
            .Where(p => p.EntidadId == entidadId && p.Activo)
            .OrderBy(p => p.RazonSocial)
            .Select(p => new { p.Id, Nombre = p.RazonSocial })
            .ToListAsync();

        return new CxPFormViewModel
        {
            Item = existing ?? new CuentaPorPagar
            {
                Estado = "PENDIENTE",
                Moneda = "CUP",
                DocumentoOrigenTipo = "FACTURA",
                FechaEmision = DateOnly.FromDateTime(DateTime.Now),
                FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
            },
            Proveedores = new SelectList(proveedores, "Id", "Nombre")
        };
    }

    public async Task<CuentaPorCobrar?> GetCxCByIdAsync(Guid id)
    {
        return await _context.CuentaPorCobrars
            .Include(c => c.Cliente)
            .Include(c => c.PagoAplicados)
            .Include(c => c.AsientoOrigen)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CuentaPorPagar?> GetCxPByIdAsync(Guid id)
    {
        return await _context.CuentaPorPagars
            .Include(c => c.Proveedor)
            .Include(c => c.PagoAplicados)
            .Include(c => c.AsientoOrigen)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(bool Succeeded, string Message)> CreateCxCAsync(CuentaPorCobrar item, Guid entidadId, Guid userId)
    {
        // La transacción se reutiliza si el llamador ya abrió una (patrón de CreateEntryAsync).
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            item.Id = Guid.NewGuid();
            item.EntidadId = entidadId;
            item.CreadoEn = DateTimeOffset.Now;
            item.SaldoPendiente = item.MontoOriginal;
            if (item.DocumentoOrigenId == Guid.Empty) item.DocumentoOrigenId = item.Id;
            if (string.IsNullOrWhiteSpace(item.Estado)) item.Estado = "PENDIENTE";
            if (string.IsNullOrWhiteSpace(item.Moneda)) item.Moneda = "CUP";

            // Hueco #5: el alta manual también debe reconocerse en contabilidad, si no
            // cartera y balance se separan. DEBE cuentas por cobrar / HABER ingresos.
            var (asiento, errorAsiento) = await CrearAsientoAltaAsync(
                entidadId, userId, item.Id, item.DocumentoOrigenTipo, item.DocumentoOrigenNumero,
                item.FechaEmision, item.MontoOriginal, AltaCxC);

            if (asiento is null)
            {
                if (ownsTransaction) await transaction!.RollbackAsync();
                return (false, $"No se pudo contabilizar el alta: {errorAsiento}");
            }

            item.AsientoOrigenId = asiento.Id;
            _context.CuentaPorCobrars.Add(item);
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Cuenta por cobrar registrada y contabilizada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            return (false, ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCxCAsync(CuentaPorCobrar item, Guid entidadId, Guid userId)
    {
        // La transacción se reutiliza si el llamador ya abrió una (patrón de CreateEntryAsync).
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            var existing = await _context.CuentaPorCobrars.FindAsync(item.Id);
            if (existing == null) return (false, "No existe.");

            // El importe anterior se lee de la BD y no de la instancia: si el llamador
            // reutiliza el mismo contexto, FindAsync devuelve la instancia ya mutada y el
            // cambio de importe pasaría desapercibido (dejando el asiento desfasado).
            var montoAnterior = await _context.CuentaPorCobrars.AsNoTracking()
                .Where(c => c.Id == item.Id)
                .Select(c => (decimal?)c.MontoOriginal)
                .FirstOrDefaultAsync();

            // El saldo pendiente se recalcula de los pagos aplicados: el valor del formulario
            // no se toma en cuenta para impedir manipulación del saldo sin asiento.
            var pagos = await _context.PagoAplicados
                .Where(p => p.CuentaPorCobrarId == existing.Id)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;
            var nuevoSaldo = item.MontoOriginal - pagos;
            if (nuevoSaldo < 0) return (false, "El importe original no puede ser menor que el total ya aplicado.");

            // Cambiar el importe deja el asiento de apertura desfasado: se revierte y se
            // reposta por el nuevo importe. No se borra nada: queda el rastro REVERTIDO + reversión.
            if (montoAnterior is not null && montoAnterior != item.MontoOriginal && existing.AsientoOrigenId is not null)
            {
                var (repostado, error) = await RepostarAsientoAltaAsync(
                    existing.AsientoOrigenId.Value, entidadId, userId,
                    item.Id, item.DocumentoOrigenTipo, item.DocumentoOrigenNumero, item.FechaEmision, item.MontoOriginal,
                    AltaCxC, "corrección del importe de la cuenta por cobrar");

                if (repostado is null)
                {
                    if (ownsTransaction) await transaction!.RollbackAsync();
                    return (false, $"No se pudo recontabilizar el cambio de importe: {error}");
                }

                existing.AsientoOrigenId = repostado.Id;
            }

            existing.ClienteId = item.ClienteId;
            existing.DocumentoOrigenTipo = item.DocumentoOrigenTipo;
            existing.DocumentoOrigenId = item.DocumentoOrigenId;
            existing.DocumentoOrigenNumero = item.DocumentoOrigenNumero;
            existing.FechaEmision = item.FechaEmision;
            existing.FechaVencimiento = item.FechaVencimiento;
            existing.MontoOriginal = item.MontoOriginal;
            existing.SaldoPendiente = nuevoSaldo;
            existing.Moneda = item.Moneda;
            existing.Estado = nuevoSaldo == 0 ? "PAGADA" : (pagos > 0 ? "PARCIAL" : "PENDIENTE");
            existing.EntidadId = entidadId;
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Cuenta por cobrar actualizada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            return (false, ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<(bool Succeeded, string Message)> CreateCxPAsync(CuentaPorPagar item, Guid entidadId, Guid userId)
    {
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            item.Id = Guid.NewGuid();
            item.EntidadId = entidadId;
            item.CreadoEn = DateTimeOffset.Now;
            item.SaldoPendiente = item.MontoOriginal;
            if (item.DocumentoOrigenId == Guid.Empty) item.DocumentoOrigenId = item.Id;
            if (string.IsNullOrWhiteSpace(item.Estado)) item.Estado = "PENDIENTE";
            if (string.IsNullOrWhiteSpace(item.Moneda)) item.Moneda = "CUP";

            // Hueco #5: asiento de apertura de la deuda. DEBE gasto / HABER cuentas por pagar.
            var (asiento, errorAsiento) = await CrearAsientoAltaAsync(
                entidadId, userId, item.Id, item.DocumentoOrigenTipo, item.DocumentoOrigenNumero,
                item.FechaEmision, item.MontoOriginal, AltaCxP);

            if (asiento is null)
            {
                if (ownsTransaction) await transaction!.RollbackAsync();
                return (false, $"No se pudo contabilizar el alta: {errorAsiento}");
            }

            item.AsientoOrigenId = asiento.Id;
            _context.CuentaPorPagars.Add(item);
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Cuenta por pagar registrada y contabilizada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            return (false, ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    /// <summary>
    /// Cuentas y comprobante del asiento de apertura de una CxC/CxP dada de alta a mano.
    /// </summary>
    private sealed record ConfiguracionAlta(
        string CodigoComprobante,
        string CuentaDebeParametro, string CuentaDebeDefecto,
        string CuentaHaberParametro, string CuentaHaberDefecto,
        string GlosaDebe, string GlosaHaber);

    private static readonly ConfiguracionAlta AltaCxC = new(
        "ING", "CTA_CXC_CLIENTES", "135", "CTA_CONTRA_ALTA_CXC", "951",
        "Alta manual de CxC", "Contrapartida alta manual de CxC");

    private static readonly ConfiguracionAlta AltaCxP = new(
        "DIA", "CTA_CONTRA_ALTA_CXP", "865", "CTA_CXP_PROVEEDORES", "405",
        "Contrapartida alta manual de CxP", "Alta manual de CxP");

    /// <summary>
    /// Asiento de apertura de una CxC/CxP dada de alta a mano. Reutiliza
    /// <see cref="IAccountingService.CreateEntryAsync"/> para heredar la validación
    /// de partida doble, el periodo abierto y el consecutivo.
    /// </summary>
    private async Task<(AsientoContable? Asiento, string? Error)> CrearAsientoAltaAsync(
        Guid entidadId, Guid userId, Guid documentoId, string documentoOrigenTipo, string? documentoOrigenNumero,
        DateOnly fecha, decimal monto, ConfiguracionAlta config)
    {
        if (monto <= 0) return (null, "el importe debe ser mayor que cero.");

        var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == config.CodigoComprobante);
        if (tipoComprobante is null) return (null, $"no existe el tipo de comprobante '{config.CodigoComprobante}'.");

        var cuentaDebe = await ObtenerCuentaIdAsync(entidadId, config.CuentaDebeParametro, config.CuentaDebeDefecto);
        if (cuentaDebe is null)
            return (null, $"no se encontró la cuenta del debe (parámetro {config.CuentaDebeParametro}, código {config.CuentaDebeDefecto}).");

        var cuentaHaber = await ObtenerCuentaIdAsync(entidadId, config.CuentaHaberParametro, config.CuentaHaberDefecto);
        if (cuentaHaber is null)
            return (null, $"no se encontró la cuenta del haber (parámetro {config.CuentaHaberParametro}, código {config.CuentaHaberDefecto}).");

        var referencia = string.IsNullOrWhiteSpace(documentoOrigenNumero) ? documentoOrigenTipo : documentoOrigenNumero;

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            Fecha = fecha,
            Concepto = $"ALTA MANUAL {referencia}",
            // El CHECK de asiento_contable.modulo_origen no admite "CARTERA": el cobro
            // existente ya contabiliza con CONTABILIDAD.
            ModuloOrigen = "CONTABILIDAD",
            DocumentoOrigenTipo = documentoOrigenTipo,
            DocumentoOrigenId = documentoId,
            TipoComprobanteId = tipoComprobante.Id,
            Estado = "CONTABILIZADO",
            CreadoPor = userId,
            CreadoEn = DateTimeOffset.Now,
            AsientoDetalles =
            {
                new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaDebe.Value, Debe = monto, Glosa = config.GlosaDebe },
                new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaHaber.Value, Haber = monto, Glosa = config.GlosaHaber }
            }
        };

        var resultado = await _accountingService.CreateEntryAsync(entry);
        return resultado.Succeeded ? (resultado.Entry, null) : (null, resultado.Message);
    }

    /// <summary>
    /// Revierte el asiento de apertura de un documento cuyo importe cambió y reposta uno
    /// nuevo por el importe vigente. El asiento revertido se conserva (auditoría).
    /// </summary>
    private async Task<(AsientoContable? Asiento, string? Error)> RepostarAsientoAltaAsync(
        Guid asientoOriginalId, Guid entidadId, Guid userId,
        Guid documentoId, string documentoOrigenTipo, string? documentoOrigenNumero,
        DateOnly fecha, decimal monto, ConfiguracionAlta config, string motivo)
    {
        var reversion = await _accountingService.ReverseEntryAsync(asientoOriginalId, motivo);
        if (!reversion.Succeeded) return (null, reversion.Message);

        return await CrearAsientoAltaAsync(
            entidadId, userId, documentoId, documentoOrigenTipo, documentoOrigenNumero, fecha, monto, config);
    }

    /// <summary>
    /// Resuelve la cuenta contable por el parámetro configurado para la entidad y,
    /// si no existe, por el código por defecto del catálogo.
    /// </summary>
    private async Task<Guid?> ObtenerCuentaIdAsync(Guid entidadId, string codigoParametro, string codigoPorDefecto)
    {
        var configurado = await _paramService.ObtenerValorVigenteAsync(entidadId, codigoParametro);

        foreach (var codigo in new[] { configurado, codigoPorDefecto })
        {
            if (string.IsNullOrWhiteSpace(codigo)) continue;

            var id = await _context.CuentaContables
                .Where(c => c.Codigo == codigo && c.EntidadId == entidadId)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();

            if (id is not null) return id;
        }

        return null;
    }

    public async Task<(bool Succeeded, string Message)> UpdateCxPAsync(CuentaPorPagar item, Guid entidadId, Guid userId)
    {
        // La transacción se reutiliza si el llamador ya abrió una (patrón de CreateEntryAsync).
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            var existing = await _context.CuentaPorPagars.FindAsync(item.Id);
            if (existing == null) return (false, "No existe.");

            // El importe anterior se lee de la BD (ver UpdateCxCAsync).
            var montoAnterior = await _context.CuentaPorPagars.AsNoTracking()
                .Where(c => c.Id == item.Id)
                .Select(c => (decimal?)c.MontoOriginal)
                .FirstOrDefaultAsync();

            // El saldo pendiente se recalcula de los pagos aplicados (ver UpdateCxCAsync).
            var pagos = await _context.PagoAplicados
                .Where(p => p.CuentaPorPagarId == existing.Id)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;
            var nuevoSaldo = item.MontoOriginal - pagos;
            if (nuevoSaldo < 0) return (false, "El importe original no puede ser menor que el total ya aplicado.");

            // Cambiar el importe deja el asiento de apertura desfasado: se revierte y se
            // reposta por el nuevo importe (ver UpdateCxCAsync).
            if (montoAnterior is not null && montoAnterior != item.MontoOriginal && existing.AsientoOrigenId is not null)
            {
                var (repostado, error) = await RepostarAsientoAltaAsync(
                    existing.AsientoOrigenId.Value, entidadId, userId,
                    item.Id, item.DocumentoOrigenTipo, item.DocumentoOrigenNumero, item.FechaEmision, item.MontoOriginal,
                    AltaCxP, "corrección del importe de la cuenta por pagar");

                if (repostado is null)
                {
                    if (ownsTransaction) await transaction!.RollbackAsync();
                    return (false, $"No se pudo recontabilizar el cambio de importe: {error}");
                }

                existing.AsientoOrigenId = repostado.Id;
            }

            existing.ProveedorId = item.ProveedorId;
            existing.DocumentoOrigenTipo = item.DocumentoOrigenTipo;
            existing.DocumentoOrigenId = item.DocumentoOrigenId;
            existing.DocumentoOrigenNumero = item.DocumentoOrigenNumero;
            existing.FechaEmision = item.FechaEmision;
            existing.FechaVencimiento = item.FechaVencimiento;
            existing.MontoOriginal = item.MontoOriginal;
            existing.SaldoPendiente = nuevoSaldo;
            existing.Moneda = item.Moneda;
            existing.Estado = nuevoSaldo == 0 ? "PAGADA" : (pagos > 0 ? "PARCIAL" : "PENDIENTE");
            existing.EntidadId = entidadId;
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Cuenta por pagar actualizada.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            return (false, ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}
