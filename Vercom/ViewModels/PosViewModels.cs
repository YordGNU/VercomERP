using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class DispositivoPosFormViewModel
{
    public DispositivoPo Dispositivo { get; set; } = new();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Cajas { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Dispositivo POS";
}
