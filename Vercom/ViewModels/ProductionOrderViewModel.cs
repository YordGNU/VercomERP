using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class ProductionOrderViewModel
{
    public OrdenProduccion Order { get; set; } = new();

    public IEnumerable<SelectListItem> ProductosElaborados { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Planificación de Producción";
}
