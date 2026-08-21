using Vercom.Models;

namespace Vercom.ViewModels;

public class PlantillaStatusViewModel
{
    public List<PlantillaRow> Rows { get; set; } = new();
    public string Title { get; set; } = "Estado de Plantilla Aprobada vs. Cubierta";
}

public class PlantillaRow
{
    public string Cargo { get; set; } = null!;
    public string Sucursal { get; set; } = null!;
    public int Aprobadas { get; set; }
    public int Cubiertas { get; set; }
    public int Disponibles => Aprobadas - Cubiertas;
    public decimal PorcentajeOcupacion => Aprobadas > 0 ? (Cubiertas * 100m / Aprobadas) : 0;
}
