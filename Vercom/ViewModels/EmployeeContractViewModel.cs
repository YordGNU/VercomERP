using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EmployeeContractViewModel
{
    public ContratoLaboral Contrato { get; set; } = new();
    public string NombreEmpleado { get; set; } = null!;
    public Guid EmpleadoId { get; set; } = Guid.Empty;

    public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposContrato { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Registrar Nuevo Contrato";
    public IFormFile? DocumentoContrato { get; set; }
}
