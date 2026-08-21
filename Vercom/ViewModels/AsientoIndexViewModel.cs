using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class AsientoIndexViewModel
{
    public IEnumerable<AsientoContable> Entries { get; set; } = new List<AsientoContable>();
    public IEnumerable<SelectListItem> Periods { get; set; } = new List<SelectListItem>();
    public Guid? SelectedPeriodId { get; set; }
    public string Title { get; set; } = "Diario Contable";
}
