using Vercom.Models;

namespace Vercom.ViewModels;

public class PayrollPreviewViewModel
{
    public short Anio { get; set; }
    public short Mes { get; set; }
    public Guid? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }

    public List<PayrollPreviewRow> Rows { get; set; } = new();

    public decimal TotalDevengado => Rows.Sum(r => r.Devengado);
    public decimal TotalDeducciones => Rows.Sum(r => r.Deducciones);
    public decimal TotalNeto => Rows.Sum(r => r.Neto);

    public int TotalTrabajadores => Rows.Count;

    public string Title => $"Simulación de Nómina - {Mes}/{Anio}";
}

public class PayrollPreviewRow
{
    public Guid EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public string Sucursal { get; set; } = null!;
    public decimal DiasTrabajados { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal Devengado { get; set; }
    public decimal Deducciones { get; set; }
    public decimal Neto => Devengado - Deducciones;

    // Desglose informativo
    public decimal RetencionSS { get; set; }
    public decimal RetencionIRP { get; set; }
}
