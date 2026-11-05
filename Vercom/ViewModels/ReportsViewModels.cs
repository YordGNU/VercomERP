using Microsoft.AspNetCore.Mvc.Rendering;

namespace VERCOM.ViewModels;

public class ReportsIndexViewModel
{
    public IEnumerable<SelectListItem> Periods { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Generación de Estados Financieros";
}
