using Microsoft.AspNetCore.Mvc.Rendering;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.ViewModels;

public class CarteraIndexViewModel
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string? DocumentoTipo { get; set; }
    public string? Estado { get; set; }
    public bool VencidasOnly { get; set; }

    public decimal TotalAbierto { get; set; }
    public decimal TotalVencido { get; set; }
    public decimal TotalPorVencer { get; set; }
    public decimal TotalAplicado { get; set; }
    public int DocumentosAbiertos { get; set; }

    public List<AgingReportRow> Aging { get; set; } = new();

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class CxCIndexViewModel : CarteraIndexViewModel
{
    public IEnumerable<CuentaPorCobrar> Items { get; set; } = new List<CuentaPorCobrar>();
}

public class CxPIndexViewModel : CarteraIndexViewModel
{
    public IEnumerable<CuentaPorPagar> Items { get; set; } = new List<CuentaPorPagar>();
}

public class CxCFormViewModel
{
    public CuentaPorCobrar Item { get; set; } = new();
    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
}

public class CxPFormViewModel
{
    public CuentaPorPagar Item { get; set; } = new();
    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
}

public static class CarteraCatalogos
{
    public static readonly string[] TiposDocumento = { "FACTURA", "ORDEN", "RECEPCION", "CONTRATO", "AJUSTE", "OTROS" };
    public static readonly string[] Estados = { "PENDIENTE", "PARCIAL", "PAGADA" };
    public static readonly string[] Monedas = { "CUP", "MLC" };
    public static readonly string[] FormasPago = { "EFECTIVO", "TRANSFERENCIA", "CHEQUE", "TARJETA", "OTRO" };
}