namespace Vercom.Models;

public partial class CuentaContable
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public Guid? CuentaPadreId { get; set; }

    public short Nivel { get; set; }

    public string Clase { get; set; } = null!;

    public string Naturaleza { get; set; } = null!;

    public bool AceptaMovimiento { get; set; }

    public bool RequiereCentroCosto { get; set; }

    public bool RequiereTercero { get; set; }

    public string Moneda { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ActivoFijo> ActivoFijoCuentaActivos { get; set; } = new List<ActivoFijo>();

    public virtual ICollection<ActivoFijo> ActivoFijoCuentaDepreciacions { get; set; } = new List<ActivoFijo>();

    public virtual ICollection<ActivoFijo> ActivoFijoCuentaGastoDeps { get; set; } = new List<ActivoFijo>();

    public virtual ICollection<AsientoDetalle> AsientoDetalles { get; set; } = new List<AsientoDetalle>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<ConceptoNomina> ConceptoNominas { get; set; } = new List<ConceptoNomina>();

    public virtual ICollection<CuentaBancarium> CuentaBancaria { get; set; } = new List<CuentaBancarium>();

    public virtual CuentaContable? CuentaPadre { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<CuentaContable> InverseCuentaPadre { get; set; } = new List<CuentaContable>();

    public virtual ICollection<PresupuestoLinea> PresupuestoLineas { get; set; } = new List<PresupuestoLinea>();

    public virtual ICollection<Producto> ProductoCuentaCostoVenta { get; set; } = new List<Producto>();

    public virtual ICollection<Producto> ProductoCuentaIngresos { get; set; } = new List<Producto>();

    public virtual ICollection<Producto> ProductoCuentaInventarios { get; set; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
