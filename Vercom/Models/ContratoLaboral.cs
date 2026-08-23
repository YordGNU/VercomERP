namespace Vercom.Models;

public partial class ContratoLaboral
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public string TipoContrato { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public decimal SalarioPactado { get; set; }

    public decimal JornadaHorasSemana { get; set; }

    public Guid CargoId { get; set; }

    public string? DocumentoUrl { get; set; }

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Cargo Cargo { get; set; } = null!;

    public virtual Empleado Empleado { get; set; } = null!;
}
