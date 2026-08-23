namespace Vercom.Models;

public partial class PlantillaAprobadum
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public Guid CargoId { get; set; }

    public int PlazasAprobadas { get; set; }

    public DateOnly VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public virtual Cargo Cargo { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Sucursal? Sucursal { get; set; }
}
