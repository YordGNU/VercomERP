using Vercom.Models;

namespace Vercom.ViewModels;

public class SucursalPagedResult
{
    public IEnumerable<Sucursal> Items { get; set; } = new List<Sucursal>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string? Tipo { get; set; }
    public bool? Activo { get; set; }

    public int TotalSucursales { get; set; }
    public int Activas { get; set; }
    public int Inactivas { get; set; }
    public List<string> Tipos { get; set; } = new();

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}