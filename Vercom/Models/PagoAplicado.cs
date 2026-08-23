namespace Vercom.Models;

public partial class PagoAplicado
{
    public Guid Id { get; set; }

    public string Tipo { get; set; } = null!;

    public Guid? CuentaPorCobrarId { get; set; }

    public Guid? CuentaPorPagarId { get; set; }

    public DateOnly Fecha { get; set; }

    public decimal Monto { get; set; }

    public string FormaPago { get; set; } = null!;

    public Guid? AsientoId { get; set; }

    public string? ReferenciaExterna { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual CuentaPorCobrar? CuentaPorCobrar { get; set; }

    public virtual CuentaPorPagar? CuentaPorPagar { get; set; }
}
