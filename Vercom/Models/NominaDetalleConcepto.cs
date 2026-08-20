using System;
using System.Collections.Generic;

namespace Vercom.Models;

/// <summary>
/// RNF-22: histórico salarial inalterable — no se actualiza tras CONTABILIZADA, solo se referencia para reportes probatorios.
/// </summary>
public partial class NominaDetalleConcepto
{
    public Guid Id { get; set; }

    public Guid NominaDetalleId { get; set; }

    public int ConceptoId { get; set; }

    public decimal Monto { get; set; }

    public virtual ConceptoNomina Concepto { get; set; } = null!;

    public virtual NominaDetalle NominaDetalle { get; set; } = null!;
}
