using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class AccountFormViewModel
{
    public CuentaContable Account { get; set; } = new();

    public IEnumerable<SelectListItem> CuentasPadre { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Clases { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Naturalezas { get; set; } = new List<SelectListItem>();

    public string Title { get; set; } = "Configuración de Cuenta Contable";
}
