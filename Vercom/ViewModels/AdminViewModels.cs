using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;

namespace Vercom.ViewModels;

public class ConsecutivoFormViewModel
{
    public Consecutivo Consecutivo { get; set; } = new();
    public IEnumerable<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    public string Title { get; set; } = "Gestión de Numeración Consecutiva";
}

public class ParametroFormViewModel
{
    public ParametroSistema Parametro { get; set; } = new();
    public string Title { get; set; } = "Configuración de Parámetro";
}
