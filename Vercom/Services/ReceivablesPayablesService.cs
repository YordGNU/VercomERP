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
}

public class ReceivablesPayablesService : IReceivablesPayablesService
{
    private readonly AppDbContext _context;

    public ReceivablesPayablesService(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<AgingReportRow>> GetPayablesAgingAsync(Guid entidadId)
    {
        throw new NotImplementedException();
    }

    public Task<List<AgingReportRow>> GetReceivablesAgingAsync(Guid entidadId)
    {
        throw new NotImplementedException();
    }

    //public async Task<List<AgingReportRow>> GetReceivablesAgingAsync(Guid entidadId)
    //{
    //    var items = await _context.CuentaPorCobrars
    //        .Include(c => c.Cliente)
    //        .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
    //        .ToListAsync();

    //    return CalculateAging(items.Select(i => new { i.Cliente.NombreRazonSocial, i.SaldoPendiente, i.FechaVencimiento }));
    //}

    //public async Task<List<AgingReportRow>> GetPayablesAgingAsync(Guid entidadId)
    //{
    //    var items = await _context.CuentaPorPagars
    //        .Include(c => c.Proveedor)
    //        .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
    //        .ToListAsync();

    //    return CalculateAging(items.Select(i => new { NombreRazonSocial = i.Proveedor.RazonSocial, i.SaldoPendiente, i.FechaVencimiento }));
    //}

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
}
