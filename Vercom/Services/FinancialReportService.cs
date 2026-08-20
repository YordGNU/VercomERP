using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public class AccountSummary
{
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal Saldo { get; set; }
}

public class FinancialStatement
{
    public string Titulo { get; set; } = null!;
    public List<AccountSummary> Activos { get; set; } = new();
    public List<AccountSummary> Pasivos { get; set; } = new();
    public List<AccountSummary> Patrimonio { get; set; } = new();
    public decimal TotalActivos => Activos.Sum(a => a.Saldo);
    public decimal TotalPasivos => Pasivos.Sum(a => a.Saldo);
    public decimal TotalPatrimonio => Patrimonio.Sum(a => a.Saldo);
}

public interface IFinancialReportService
{
    Task<FinancialStatement> GetBalanceGeneralAsync(Guid entidadId, Guid periodId);
    Task<List<AccountSummary>> GetEstadoResultadosAsync(Guid entidadId, Guid periodId);
}

public class FinancialReportService : IFinancialReportService
{
   private readonly AppDbContext _context;   
    private readonly IAccountingService _accountingService;

    public FinancialReportService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }

    public async Task<FinancialStatement> GetBalanceGeneralAsync(Guid entidadId, Guid periodId)
    {
        var statement = new FinancialStatement { Titulo = "Balance General" };
        var accounts = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && (c.Clase == "ACTIVO" || c.Clase == "PASIVO" || c.Clase == "PATRIMONIO"))
            .ToListAsync();

        foreach (var acc in accounts)
        {
            var saldo = await _accountingService.GetAccountBalanceAsync(acc.Id, periodId);
            if (saldo == 0) continue;

            var summary = new AccountSummary { Codigo = acc.Codigo, Nombre = acc.Nombre, Saldo = saldo };

            if (acc.Clase == "ACTIVO") statement.Activos.Add(summary);
            else if (acc.Clase == "PASIVO") statement.Pasivos.Add(summary);
            else statement.Patrimonio.Add(summary);
        }

        return statement;
    }

    public async Task<List<AccountSummary>> GetEstadoResultadosAsync(Guid entidadId, Guid periodId)
    {
        var results = new List<AccountSummary>();
        var accounts = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && (c.Clase == "INGRESOS" || c.Clase == "GASTOS"))
            .ToListAsync();

        foreach (var acc in accounts)
        {
            var saldo = await _accountingService.GetAccountBalanceAsync(acc.Id, periodId);
            if (saldo == 0) continue;

            results.Add(new AccountSummary { Codigo = acc.Codigo, Nombre = acc.Nombre, Saldo = saldo });
        }

        return results;
    }
}
