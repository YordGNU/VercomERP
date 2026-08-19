using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ActivoFijo
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public string CodigoInventario { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public Guid CuentaActivoId { get; set; }

    public Guid CuentaDepreciacionId { get; set; }

    public Guid CuentaGastoDepId { get; set; }

    public DateOnly FechaAdquisicion { get; set; }

    public decimal ValorAdquisicion { get; set; }

    public decimal ValorResidual { get; set; }

    public int VidaUtilMeses { get; set; }

    public decimal? TasaDepreciacionAnual { get; set; }

    public string MetodoDepreciacion { get; set; } = null!;

    public decimal DepreciacionAcumulada { get; set; }

    public string Estado { get; set; } = null!;

    public DateOnly? FechaBaja { get; set; }

    public string? MotivoBaja { get; set; }

    public virtual ICollection<ActivoFijoDepreciacion> ActivoFijoDepreciacions { get; set; } = new List<ActivoFijoDepreciacion>();

    public virtual CuentaContable CuentaActivo { get; set; } = null!;

    public virtual CuentaContable CuentaDepreciacion { get; set; } = null!;

    public virtual CuentaContable CuentaGastoDep { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();

    public virtual Sucursal? Sucursal { get; set; }
}
