using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class NominaDetalleConcepto
{
    public Guid Id { get; set; }

    public Guid NominaDetalleId { get; set; }

    public int ConceptoId { get; set; }

    public decimal Monto { get; set; }

    public virtual ConceptoNomina Concepto { get; set; } = null!;

    public virtual NominaDetalle NominaDetalle { get; set; } = null!;
}
