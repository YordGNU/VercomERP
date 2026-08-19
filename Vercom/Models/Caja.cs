using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Caja
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid SucursalId { get; set; }

    public string Nombre { get; set; } = null!;

    public Guid CuentaContableId { get; set; }

    public decimal? LimiteEfectivo { get; set; }

    public decimal SaldoActual { get; set; }

    public bool Activa { get; set; }

    public virtual CuentaContable CuentaContable { get; set; } = null!;

    public virtual ICollection<DispositivoPo> DispositivoPos { get; set; } = new List<DispositivoPo>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Sucursal Sucursal { get; set; } = null!;
}
