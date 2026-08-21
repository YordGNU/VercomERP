using Vercom.Models;

namespace Vercom.ViewModels;

public class PayrollIndexViewModel
{
    public IEnumerable<PeriodoNomina> Periodos { get; set; } = new List<PeriodoNomina>();
    public short CurrentAnio { get; set; } = (short)DateTime.Now.Year;
    public short CurrentMes { get; set; } = (short)DateTime.Now.Month;
    public string Title { get; set; } = "Gestión de Nóminas de Salarios";
}

public class PayrollDetailsViewModel
{
    public PeriodoNomina Periodo { get; set; } = null!;
    public IEnumerable<NominaDetalle> Detalles { get; set; } = new List<NominaDetalle>();
    public string Title { get; set; } = "Resumen de Nómina Detallada";
}
