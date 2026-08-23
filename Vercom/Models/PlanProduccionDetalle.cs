namespace Vercom.Models;

public partial class PlanProduccionDetalle
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal CantidadPlanificada { get; set; }

    public decimal CantidadEjecutada { get; set; }

    public virtual PlanProduccion Plan { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
