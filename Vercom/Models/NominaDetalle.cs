using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class NominaDetalle
{
    public Guid Id { get; set; }

    public Guid PeriodoNominaId { get; set; }

    public Guid EmpleadoId { get; set; }

    public decimal DiasTrabajados { get; set; }

    public decimal HorasExtra { get; set; }

    public decimal SalarioDevengado { get; set; }

    public decimal TotalDeducciones { get; set; }

    public decimal SalarioNeto { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual ICollection<NominaDetalleConcepto> NominaDetalleConceptos { get; set; } = new List<NominaDetalleConcepto>();

    public virtual PeriodoNomina PeriodoNomina { get; set; } = null!;
}
