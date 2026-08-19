using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class MovimientoBancario
{
    public Guid Id { get; set; }

    public Guid CuentaBancariaId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Tipo { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public string? Referencia { get; set; }

    public bool Conciliado { get; set; }

    public DateOnly? FechaConciliacion { get; set; }

    public Guid? AsientoId { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual CuentaBancarium CuentaBancaria { get; set; } = null!;
}
