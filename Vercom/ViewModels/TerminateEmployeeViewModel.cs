namespace Vercom.ViewModels;

public class TerminateEmployeeViewModel
{
    public Guid EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public DateOnly FechaIngreso { get; set; }
    public DateOnly FechaBaja { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? MotivoBaja { get; set; }

    public decimal VacacionesPendientes { get; set; }
    public decimal SalarioPactado { get; set; }
    public decimal EstimadoLiquidacion => (SalarioPactado / 24) * VacacionesPendientes;

    public string Title { get; set; } = "Cierre de Expediente y Liquidación";
}
