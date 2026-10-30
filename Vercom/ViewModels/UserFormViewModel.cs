using Microsoft.AspNetCore.Mvc.Rendering;
using VERCOM.Models;

namespace VERCOM.ViewModels;

public class UserFormViewModel
{
    public Usuario Usuario { get; set; } = new();
    public string? Password { get; set; }

    public IEnumerable<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Entidades { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();
    public List<int> SelectedRoles { get; set; } = new();

    public string Title { get; set; } = "Gestión de Usuario";
}
