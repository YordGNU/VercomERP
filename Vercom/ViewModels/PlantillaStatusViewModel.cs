
namespace Vercom.ViewModels;

public class PlantillaStatusViewModel
{
    public List<PlantillaRow> Rows { get; set; } = new();
    public int TotalPlazasAprobadas { get; set; }
    public int TotalPlazasCubiertas { get; set; }
    public int TotalVacantes { get; set; }
    public decimal PorcentajeCoberturaTotal { get; set; }
    public DateTime FechaCalculo { get; set; }
}

public class PlantillaRow
{
    public Guid Id { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public string EntidadNombre { get; set; } = string.Empty;
    public int Aprobadas { get; set; }
    public int Cubiertas { get; set; }

    // Propiedades calculadas (útiles para la vista)
    public int Vacantes => Aprobadas - Cubiertas;
    public decimal PorcentajeCobertura => Aprobadas > 0 ? (decimal)Cubiertas / Aprobadas * 100 : 0;
    public string Estado => Aprobadas > 0 ? (Cubiertas >= Aprobadas ? "Completo" : "Vacante") : "Sin plazas";
    public string ColorEstado => Aprobadas > 0 ? (Cubiertas >= Aprobadas ? "success" : "warning") : "secondary";
}