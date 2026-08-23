namespace Vercom.Models;

public partial class RegistroAsistencium
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? HoraEntrada { get; set; }

    public TimeOnly? HoraSalida { get; set; }

    public decimal HorasExtra { get; set; }

    public int? TipoAusenciaId { get; set; }

    public string? Observaciones { get; set; }

    public Guid? RegistradoPor { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual Usuario? RegistradoPorNavigation { get; set; }

    public virtual TipoAusencium? TipoAusencia { get; set; }
}
