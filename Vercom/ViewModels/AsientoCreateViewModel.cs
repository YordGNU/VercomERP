using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class AsientoCreateViewModel
{
    public AsientoContable Entry { get; set; } = new();
    public IEnumerable<SelectListItem> TiposComprobante { get; set; } = new List<SelectListItem>();
    public IEnumerable<dynamic> CuentasDisponibles { get; set; } = new List<dynamic>();
    public string Title { get; set; } = "Nuevo Asiento Manual";
}
