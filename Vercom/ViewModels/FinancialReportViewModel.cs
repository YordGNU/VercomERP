using Vercom.Services;

namespace Vercom.ViewModels;

public class FinancialReportViewModel
{
    public string ReportName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;

    // Para Balance General
    public FinancialStatement? Balance { get; set; }

    // Para Estado de Resultados
    public List<AccountSummary>? Resultados { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
