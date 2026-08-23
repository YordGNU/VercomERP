namespace Vercom.Models;

public partial class MantenimientoProgramado
{
    public Guid Id { get; set; }

    public Guid EquipoId { get; set; }

    public DateOnly FechaProgramada { get; set; }

    public DateOnly? FechaEjecutada { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal? Costo { get; set; }

    public Guid? ResponsableId { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Equipo Equipo { get; set; } = null!;

    public virtual Usuario? Responsable { get; set; }
}
