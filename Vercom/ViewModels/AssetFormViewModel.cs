using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class AssetFormViewModel
{
    public ActivoFijo Asset { get; set; } = new();

    public IEnumerable<SelectListItem> CuentasActivo { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasDepreciacion { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasGasto { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Alta de Activo Fijo Tangible";
}
