using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class CostSheetCreateViewModel
{
    public FichaCosto CostSheet { get; set; } = new();

    public IEnumerable<SelectListItem> ProductosElaborados { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Ficha de Costo Metodológica";
}
