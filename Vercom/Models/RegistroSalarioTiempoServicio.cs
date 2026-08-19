using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class RegistroSalarioTiempoServicio
{
    public Guid Id { get; set; }

    public Guid EmpleadoId { get; set; }

    public short Anio { get; set; }

    public short Mes { get; set; }

    public decimal DiasTrabajados { get; set; }

    public decimal SalarioDevengado { get; set; }

    public int? TiempoServicioAcumuladoMeses { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;
}
