using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

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

    Task<(bool Succeeded, string Message)> CreateCxCAsync(CuentaPorCobrar item, Guid entidadId);
    Task<(bool Succeeded, string Message)> UpdateCxCAsync(CuentaPorCobrar item, Guid entidadId);
    Task<(bool Succeeded, string Message)> CreateCxPAsync(CuentaPorPagar item, Guid entidadId);
    Task<(bool Succeeded, string Message)> UpdateCxPAsync(CuentaPorPagar item, Guid entidadId);

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
        var cxc = await _context.CuentaPorCobrars.FindAsync(cxcId);
        if (cxc == null) return (false, "Cuenta por cobrar no encontrada.");
        if (amount > cxc.SaldoPendiente) return (false, "El monto del cobro excede el saldo pendiente.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = cxc.EntidadId,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"COBRO A CLIENTE - REF: {reference}",
                ModuloOrigen = "CARTERA",
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

            cxc.SaldoPendiente -= amount;
            cxc.Estado = cxc.SaldoPendiente == 0 ? "PAGADA" : "PARCIAL";

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
        var cxp = await _context.CuentaPorPagars.FindAsync(cxpId);
        if (cxp == null) return (false, "Cuenta por pagar no encontrada.");
        if (amount > cxp.SaldoPendiente) return (false, "El monto del pago excede el saldo pendiente.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = cxp.EntidadId,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"PAGO A PROVEEDOR - REF: {reference}",
                ModuloOrigen = "CARTERA",
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

            cxp.SaldoPendiente -= amount;
            cxp.Estado = cxp.SaldoPendiente == 0 ? "PAGADA" : "PARCIAL";

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

    public async Task<(bool Succeeded, string Message)> CreateCxCAsync(CuentaPorCobrar item, Guid entidadId)
    {
        try
        {
            item.Id = Guid.NewGuid();
            item.EntidadId = entidadId;
            item.CreadoEn = DateTimeOffset.Now;
            item.SaldoPendiente = item.MontoOriginal;
            if (item.DocumentoOrigenId == Guid.Empty) item.DocumentoOrigenId = item.Id;
            if (string.IsNullOrWhiteSpace(item.Estado)) item.Estado = "PENDIENTE";
            if (string.IsNullOrWhiteSpace(item.Moneda)) item.Moneda = "CUP";
            _context.CuentaPorCobrars.Add(item);
            await _context.SaveChangesAsync();
            return (true, "Cuenta por cobrar registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCxCAsync(CuentaPorCobrar item, Guid entidadId)
    {
        try
        {
            var existing = await _context.CuentaPorCobrars.FindAsync(item.Id);
            if (existing == null) return (false, "No existe.");
            existing.ClienteId = item.ClienteId;
            existing.DocumentoOrigenTipo = item.DocumentoOrigenTipo;
            existing.DocumentoOrigenId = item.DocumentoOrigenId;
            existing.DocumentoOrigenNumero = item.DocumentoOrigenNumero;
            existing.FechaEmision = item.FechaEmision;
            existing.FechaVencimiento = item.FechaVencimiento;
            existing.MontoOriginal = item.MontoOriginal;
            existing.SaldoPendiente = item.SaldoPendiente;
            existing.Moneda = item.Moneda;
            existing.Estado = item.Estado;
            existing.EntidadId = entidadId;
            await _context.SaveChangesAsync();
            return (true, "Cuenta por cobrar actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> CreateCxPAsync(CuentaPorPagar item, Guid entidadId)
    {
        try
        {
            item.Id = Guid.NewGuid();
            item.EntidadId = entidadId;
            item.CreadoEn = DateTimeOffset.Now;
            item.SaldoPendiente = item.MontoOriginal;
            if (item.DocumentoOrigenId == Guid.Empty) item.DocumentoOrigenId = item.Id;
            if (string.IsNullOrWhiteSpace(item.Estado)) item.Estado = "PENDIENTE";
            if (string.IsNullOrWhiteSpace(item.Moneda)) item.Moneda = "CUP";
            _context.CuentaPorPagars.Add(item);
            await _context.SaveChangesAsync();
            return (true, "Cuenta por pagar registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCxPAsync(CuentaPorPagar item, Guid entidadId)
    {
        try
        {
            var existing = await _context.CuentaPorPagars.FindAsync(item.Id);
            if (existing == null) return (false, "No existe.");
            existing.ProveedorId = item.ProveedorId;
            existing.DocumentoOrigenTipo = item.DocumentoOrigenTipo;
            existing.DocumentoOrigenId = item.DocumentoOrigenId;
            existing.DocumentoOrigenNumero = item.DocumentoOrigenNumero;
            existing.FechaEmision = item.FechaEmision;
            existing.FechaVencimiento = item.FechaVencimiento;
            existing.MontoOriginal = item.MontoOriginal;
            existing.SaldoPendiente = item.SaldoPendiente;
            existing.Moneda = item.Moneda;
            existing.Estado = item.Estado;
            existing.EntidadId = entidadId;
            await _context.SaveChangesAsync();
            return (true, "Cuenta por pagar actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
