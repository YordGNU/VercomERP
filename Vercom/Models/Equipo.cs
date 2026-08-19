using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Equipo
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public Guid? ActivoFijoId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public DateOnly? FechaUltimaRevision { get; set; }

    public int? FrecuenciaMantenimientoDias { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ActivoFijo? ActivoFijo { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<MantenimientoProgramado> MantenimientoProgramados { get; set; } = new List<MantenimientoProgramado>();

    public virtual Sucursal? Sucursal { get; set; }
}
