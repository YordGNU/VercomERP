using VERCOM.Models;

namespace VERCOM.ViewModels;

public class AuditIndexViewModel
{
    public IEnumerable<Auditorium> Logs { get; set; } = new List<Auditorium>();

    // Filtros
    public string? FilterUser { get; set; }
    public string? FilterTable { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string Title { get; set; } = "Bitácora de Auditoría Inmutable";
}
