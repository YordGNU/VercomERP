using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class ProductFormViewModel
{
    public Producto Producto { get; set; } = new();

    public IEnumerable<SelectListItem> Familias { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> UnidadesMedida { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasContables { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposProducto { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Ficha de Producto";
}
