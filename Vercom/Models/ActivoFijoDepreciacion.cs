using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ActivoFijoDepreciacion
{
    public Guid Id { get; set; }

    public Guid ActivoFijoId { get; set; }

    public Guid PeriodoId { get; set; }

    public decimal Monto { get; set; }

    public Guid? AsientoId { get; set; }

    public DateTimeOffset CalculadoEn { get; set; }

    public virtual ActivoFijo ActivoFijo { get; set; } = null!;

    public virtual AsientoContable? Asiento { get; set; }

    public virtual PeriodoContable Periodo { get; set; } = null!;
}
