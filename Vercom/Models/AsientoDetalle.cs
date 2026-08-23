namespace Vercom.Models;

public partial class AsientoDetalle
{
    public Guid Id { get; set; }

    public Guid AsientoId { get; set; }

    public short Linea { get; set; }

    public Guid CuentaId { get; set; }

    public Guid? CentroCostoId { get; set; }

    public string? TerceroTipo { get; set; }

    public Guid? TerceroId { get; set; }

    public decimal Debe { get; set; }

    public decimal Haber { get; set; }

    public string? Glosa { get; set; }

    public virtual AsientoContable Asiento { get; set; } = null!;

    public virtual CentroCosto? CentroCosto { get; set; }

    public virtual CuentaContable Cuenta { get; set; } = null!;
}
