using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ConceptoNomina
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public Guid? CuentaContableId { get; set; }

    public Guid EntidadId { get; internal set; }
    public bool Activo { get; internal set; }

    public string? Formula { get; set; }

    public virtual CuentaContable? CuentaContable { get; set; }
    public virtual Entidad? Entidad { get; set; }

    public virtual ICollection<NominaDetalleConcepto> NominaDetalleConceptos { get; set; } = new List<NominaDetalleConcepto>();

}
