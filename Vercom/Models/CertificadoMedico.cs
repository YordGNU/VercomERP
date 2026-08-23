namespace Vercom.Models;

/// <summary>
/// RNF-20: acceso restringido — datos de salud del trabajador, solo RR.HH. y dirección.
/// </summary>
public partial class CertificadoMedico
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int? Dias { get; set; }

    public string? DiagnosticoCie { get; set; }

    public decimal PorcentajeSubsidio { get; set; }

    public string? NumeroCertificado { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;
}
