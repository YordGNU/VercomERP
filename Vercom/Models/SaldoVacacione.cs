using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class SaldoVacacione
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public short Anio { get; set; }

    public decimal DiasAcumulados { get; set; }

    public decimal DiasDisfrutados { get; set; }

    public decimal DiasCompensados { get; set; }

    public decimal? SaldoActual { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;
}
