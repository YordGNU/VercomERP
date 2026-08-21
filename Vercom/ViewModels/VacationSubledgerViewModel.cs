using Vercom.Models;

namespace Vercom.ViewModels;

public class VacationSubledgerViewModel
{
    public IEnumerable<SaldoVacacione> Saldos { get; set; } = new List<SaldoVacacione>();
    public string Title { get; set; } = "Submayor de Vacaciones (9.09%)";
}
