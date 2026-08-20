using Microsoft.EntityFrameworkCore;
using Vercom.Models;

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
    Task<(bool Succeeded, string Message)> RecordCollectionAsync(Guid cxcId, decimal amount, string paymentMethod, string? reference, Guid userId);
    Task<(bool Succeeded, string Message)> RecordPaymentAsync(Guid cxpId, decimal amount, string paymentMethod, string? reference, Guid userId);
}

public class ReceivablesPayablesService : IReceivablesPayablesService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;

    public ReceivablesPayablesService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
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
            var cashAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "101" && c.EntidadId == cxc.EntidadId);
            if (cashAccount == null) throw new Exception("Cuenta de caja no configurada.");

            entry.AsientoDetalles.Add(new AsientoDetalle {
                Id = Guid.NewGuid(),
                CuentaId = cashAccount.Id,
                Debe = amount,
                Glosa = $"Cobro CxC {cxc.Id}"
            });

            // Haber: Cuentas por Cobrar
            var cxcAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "201" && c.EntidadId == cxc.EntidadId);
            if (cxcAccount == null) throw new Exception("Cuenta de CxC no configurada.");

            entry.AsientoDetalles.Add(new AsientoDetalle {
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
            var cxpAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "401" && c.EntidadId == cxp.EntidadId);
            if (cxpAccount == null) throw new Exception("Cuenta de CxP no configurada.");

            entry.AsientoDetalles.Add(new AsientoDetalle {
                Id = Guid.NewGuid(),
                CuentaId = cxpAccount.Id,
                Debe = amount,
                Glosa = $"Pago CxP {cxp.Id}"
            });

            // Haber: Caja o Banco
            var cashAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "101" && c.EntidadId == cxp.EntidadId);
            if (cashAccount == null) throw new Exception("Cuenta de caja no configurada.");

            entry.AsientoDetalles.Add(new AsientoDetalle {
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
}
