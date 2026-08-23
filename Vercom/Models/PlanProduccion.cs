namespace Vercom.Models;

public partial class PlanProduccion
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public short Anio { get; set; }

    public short? Mes { get; set; }

    public Guid? PresupuestoId { get; set; }

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<PlanProduccionDetalle> PlanProduccionDetalles { get; set; } = new List<PlanProduccionDetalle>();

    public virtual Presupuesto? Presupuesto { get; set; }
}
