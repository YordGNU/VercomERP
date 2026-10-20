using Microsoft.AspNetCore.Mvc.Rendering;
using VERCOM.Models;

namespace VERCOM.ViewModels;

public class BomCreateViewModel
{
    public ListaMateriale Bom { get; set; } = new();

    public IEnumerable<SelectListItem> ProductosElaborados { get; set; } = new List<SelectListItem>();
    public IEnumerable<dynamic> InsumosDisponibles { get; set; } = new List<dynamic>();

    public string Title { get; set; } = "Lista de Materiales (BOM)";
}
