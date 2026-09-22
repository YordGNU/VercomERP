using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class ProductFormViewModel
{
    public Producto Producto { get; set; } = new();

    public IEnumerable<SelectListItem> Familias { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> UnidadesMedida { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasInventario { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasCostoVenta { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasIngresos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposProducto { get; set; } = new List<SelectListItem>();
    public Dictionary<string, CuentasContablesSugeridas> CuentasPorTipo { get; set; } = new();
    public string Title { get; set; } = "Ficha de Producto";
}

public class CuentasContablesSugeridas
{
    public Guid? InventarioId { get; set; }
    public Guid? CostoVentaId { get; set; }
    public Guid? IngresoId { get; set; }
}
