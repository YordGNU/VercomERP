using Microsoft.AspNetCore.Mvc.Rendering;
using VERCOM.Models;

namespace VERCOM.ViewModels;

public class DispositivoPosFormViewModel
{
    public DispositivoPo Dispositivo { get; set; } = new();
    public string? EntidadNombre { get; set; }
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Almacenes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Cajas { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Dispositivo POS";
}
