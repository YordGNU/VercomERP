namespace Vercom.Models;

/// <summary>
/// Equivalente a modelo SC-4-08 u oficial vigente MTSS.
/// </summary>
public partial class RegistroSalarioTiempoServicio
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public short Anio { get; set; }

    public short Mes { get; set; }

    public decimal DiasTrabajados { get; set; }

    public decimal SalarioDevengado { get; set; }

    public int? TiempoServicioAcumuladoMeses { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;
}
