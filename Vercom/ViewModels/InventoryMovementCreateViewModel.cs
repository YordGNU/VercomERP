using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class InventoryMovementCreateViewModel
{
    public MovimientoInventario Movement { get; set; } = new();

    public IEnumerable<SelectListItem> TiposMovimiento { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();

    // Para el selector dinámico
    public IEnumerable<dynamic> ProductosDisponibles { get; set; } = new List<dynamic>();

    public string Title { get; set; } = "Emisión de Documento Primario";
}
