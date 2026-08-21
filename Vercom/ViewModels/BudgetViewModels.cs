using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class PresupuestoFormViewModel
{
    public Presupuesto Presupuesto { get; set; } = new();
    public string Title { get; set; } = "Gestión de Presupuesto";
}

public class PlanProduccionFormViewModel
{
    public PlanProduccion Plan { get; set; } = new();
    public IEnumerable<SelectListItem> Presupuestos { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Plan de Producción Mensual";
}
