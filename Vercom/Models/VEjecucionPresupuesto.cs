using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class VEjecucionPresupuesto
{
    public Guid PresupuestoId { get; set; }

    public Guid CuentaId { get; set; }

    public Guid? CentroCostoId { get; set; }

    public short Mes { get; set; }

    public decimal MontoPlanificado { get; set; }

    public decimal? MontoReal { get; set; }
}
