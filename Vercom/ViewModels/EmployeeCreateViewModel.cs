using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EmployeeCreateViewModel
{
    public Empleado Empleado { get; set; } = new();

    // Primer Contrato (Opcional pero recomendado en el alta)
    public ContratoLaboral Contrato { get; set; } = new();

    public IFormFile? DocumentoContrato { get; set; }

    public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposContrato { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Alta de Trabajador con Contrato";
}
