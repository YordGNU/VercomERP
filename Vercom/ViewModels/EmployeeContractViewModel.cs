using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EmployeeContractViewModel
{
    public ContratoLaboral Contrato { get; set; } = new();

    public IFormFile? Documento { get; set; }

    public string NombreEmpleado { get; set; } = null!;

    public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposContrato { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Registrar Nuevo Contrato";
}
