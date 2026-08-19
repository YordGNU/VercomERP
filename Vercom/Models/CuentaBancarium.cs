using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class CuentaBancarium
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Banco { get; set; } = null!;

    public string NumeroCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public Guid CuentaContableId { get; set; }

    public decimal SaldoActual { get; set; }

    public bool Activa { get; set; }

    public virtual CuentaContable CuentaContable { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<MovimientoBancario> MovimientoBancarios { get; set; } = new List<MovimientoBancario>();
}
