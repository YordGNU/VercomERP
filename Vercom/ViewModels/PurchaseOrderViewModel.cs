using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class PurchaseOrderViewModel
{
    public OrdenCompra Order { get; set; } = new();

    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Contratos { get; set; } = new List<SelectListItem>();

    // Para el selector de productos en el detalle
    public IEnumerable<dynamic> ProductosDisponibles { get; set; } = new List<dynamic>();

    public string Title { get; set; } = "Orden de Compra";
}
