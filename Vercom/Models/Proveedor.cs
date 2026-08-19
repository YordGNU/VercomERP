using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Proveedor
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string TipoPersona { get; set; } = null!;

    public string? Nit { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? CuentaBancaria { get; set; }

    public Guid? CuentaContableId { get; set; }

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ContratoEconomico> ContratoEconomicos { get; set; } = new List<ContratoEconomico>();

    public virtual CuentaContable? CuentaContable { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<OrdenCompra> OrdenCompras { get; set; } = new List<OrdenCompra>();
}
