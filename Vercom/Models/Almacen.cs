namespace Vercom.Models;

public partial class Almacen
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid SucursalId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool EsPuntoVenta { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ConteoFisico> ConteoFisicos { get; set; } = new List<ConteoFisico>();

    public virtual ICollection<DispositivoPo> DispositivoPos { get; set; } = new List<DispositivoPo>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<Existencium> Existencia { get; set; } = new List<Existencium>();

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarioAlmacenDestinos { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarioAlmacenOrigens { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<OrdenCompra> OrdenCompras { get; set; } = new List<OrdenCompra>();

    public virtual ICollection<OrdenProduccion> OrdenProduccionAlmacenInsumos { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<OrdenProduccion> OrdenProduccionAlmacenProductos { get; set; } = new List<OrdenProduccion>();

    public virtual Sucursal Sucursal { get; set; } = null!;
}
