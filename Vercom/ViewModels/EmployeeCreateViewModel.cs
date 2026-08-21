using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EmployeeCreateViewModel
{
    public Empleado Empleado { get; set; } = new();

    public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Gestión de Expediente Laboral";
}
