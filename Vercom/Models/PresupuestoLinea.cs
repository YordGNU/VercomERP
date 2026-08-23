namespace Vercom.Models;

public partial class PresupuestoLinea
{
    public Guid Id { get; set; }

    public Guid PresupuestoId { get; set; }

    public Guid CuentaId { get; set; }

    public Guid? CentroCostoId { get; set; }

    public short Mes { get; set; }

    public decimal MontoPlanificado { get; set; }

    public virtual CentroCosto? CentroCosto { get; set; }

    public virtual CuentaContable Cuenta { get; set; } = null!;

    public virtual Presupuesto Presupuesto { get; set; } = null!;
}
