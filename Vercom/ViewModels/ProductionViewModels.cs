using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EquipoFormViewModel
{
    public Equipo Equipo { get; set; } = new();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> ActivosFijos { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Equipo de Producción";
}

public class MantenimientoFormViewModel
{
    public MantenimientoProgramado Mantenimiento { get; set; } = new();
    public IEnumerable<SelectListItem> Equipos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Responsables { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Programar Mantenimiento";
}

public class MermaFormViewModel
{
    public Merma Merma { get; set; } = new();
    public IEnumerable<SelectListItem> OrdenesProduccion { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Productos { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Registro de Merma / Desperdicio";
}
