using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class AlmacenFormViewModel
{
    public Almacen Almacen { get; set; } = new();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Almacén";
}

public class FamiliaFormViewModel
{
    public FamiliaProducto Familia { get; set; } = new();
    public IEnumerable<SelectListItem> FamiliasPadre { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Familia de Productos";
}

public class UnidadFormViewModel
{
    public UnidadMedidum Unidad { get; set; } = new();
    public string Title { get; set; } = "Gestión de Unidad de Medida";
}

public class ListaPrecioFormViewModel
{
    public ListaPrecio ListaPrecio { get; set; } = new();
    public string Title { get; set; } = "Gestión de Lista de Precios";
}
