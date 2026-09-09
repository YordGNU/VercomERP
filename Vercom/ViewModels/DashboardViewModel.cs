using Vercom.Models;

namespace Vercom.ViewModels;

public class DashboardViewModel
{
    public decimal Liquidez { get; set; }
    public decimal Rentabilidad { get; set; }
    public decimal RotacionStock { get; set; }
    public string FechaCierreCaja { get; set; } = string.Empty;

    public List<decimal> TrendRentabilidad { get; set; } = new();
    public List<decimal> TrendLiquidez { get; set; } = new();

    public List<Existencium> AlertasStock { get; set; } = new();
    public List<ContratoEconomico> ContratosVencer { get; set; } = new();

    public string PeriodoActual { get; set; } = string.Empty;
    public bool EsMaster { get; internal set; }
    public string Mensaje { get; internal set; }
}
