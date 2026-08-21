using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class UserFormViewModel
{
    public Usuario Usuario { get; set; } = new();
    public string? Password { get; set; }

    public IEnumerable<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Gestión de Usuario";
}
