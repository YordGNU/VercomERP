using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class SalesCreateViewModel
{
    public FacturaVentum Invoice { get; set; } = new();

    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Contratos { get; set; } = new List<SelectListItem>();

    // Datos de productos para el selector dinámico (JSON en la vista)
    public IEnumerable<dynamic> ProductosDisponibles { get; set; } = new List<dynamic>();

    public string Title { get; set; } = "Nueva Emisión Fiscal";
}
