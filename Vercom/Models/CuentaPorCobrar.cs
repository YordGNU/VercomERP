using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class CuentaPorCobrar
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid ClienteId { get; set; }

    public string DocumentoOrigenTipo { get; set; } = null!;

    public Guid DocumentoOrigenId { get; set; }

    public Guid? AsientoOrigenId { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal MontoOriginal { get; set; }

    public decimal SaldoPendiente { get; set; }

    public string Moneda { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual AsientoContable? AsientoOrigen { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<PagoAplicado> PagoAplicados { get; set; } = new List<PagoAplicado>();
}
