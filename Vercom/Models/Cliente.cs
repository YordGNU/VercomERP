using System;
using System.Collections.Generic;

namespace Vercom.Models;

/// <summary>
/// El &quot;cliente mostrador&quot; del POS (venta anónima) se modela como registro fijo con nit_o_ci=NULL, nombre_razon_social=&apos;CONSUMIDOR FINAL&apos;.
/// </summary>
public partial class Cliente
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string TipoPersona { get; set; } = null!;

    public string? NitOCi { get; set; }

    public string NombreRazonSocial { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string Segmento { get; set; } = null!;

    public Guid? ListaPrecioId { get; set; }

    public decimal LimiteCredito { get; set; }

    public Guid? CuentaContableId { get; set; }

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ContratoEconomico> ContratoEconomicos { get; set; } = new List<ContratoEconomico>();

    public virtual CuentaContable? CuentaContable { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ListaPrecio? ListaPrecio { get; set; }
}
