using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class FormaPagoVentum
{
    public Guid Id { get; set; }

    public Guid FacturaId { get; set; }

    public string FormaPago { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? ReferenciaExterna { get; set; }

    public decimal? VueltoEntregado { get; set; }

    public virtual FacturaVentum Factura { get; set; } = null!;
}
