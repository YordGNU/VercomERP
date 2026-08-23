namespace Vercom.Models;

public partial class Entidad
{
    public Guid Id { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string? NombreComercial { get; set; }

    public string Nit { get; set; } = null!;

    public string? CodigoReeup { get; set; }

    public string FormaJuridica { get; set; } = null!;

    public string DireccionLegal { get; set; } = null!;

    public string? Municipio { get; set; }

    public string? Provincia { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public DateOnly? FechaConstitucion { get; set; }

    public string? LicenciaActividad { get; set; }

    public string MonedaBase { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual ICollection<ActivoFijo> ActivoFijos { get; set; } = new List<ActivoFijo>();

    public virtual ICollection<Almacen> Almacens { get; set; } = new List<Almacen>();

    public virtual ICollection<ApiCliente> ApiClientes { get; set; } = new List<ApiCliente>();

    public virtual ICollection<AsientoContable> AsientoContables { get; set; } = new List<AsientoContable>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();

    public virtual ICollection<CentroCosto> CentroCostos { get; set; } = new List<CentroCosto>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<ConceptoNomina> ConceptoNominas { get; set; } = new List<ConceptoNomina>();

    public virtual ICollection<Consecutivo> Consecutivos { get; set; } = new List<Consecutivo>();

    public virtual ICollection<ContratoEconomico> ContratoEconomicos { get; set; } = new List<ContratoEconomico>();

    public virtual ICollection<CuentaBancarium> CuentaBancaria { get; set; } = new List<CuentaBancarium>();

    public virtual ICollection<CuentaContable> CuentaContables { get; set; } = new List<CuentaContable>();

    public virtual ICollection<CuentaPorCobrar> CuentaPorCobrars { get; set; } = new List<CuentaPorCobrar>();

    public virtual ICollection<CuentaPorPagar> CuentaPorPagars { get; set; } = new List<CuentaPorPagar>();

    public virtual ICollection<DeclaracionJuradum> DeclaracionJurada { get; set; } = new List<DeclaracionJuradum>();

    public virtual ICollection<DispositivoPo> DispositivoPos { get; set; } = new List<DispositivoPo>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<FamiliaProducto> FamiliaProductos { get; set; } = new List<FamiliaProducto>();

    public virtual ICollection<FichaCosto> FichaCostos { get; set; } = new List<FichaCosto>();

    public virtual ICollection<IndicadorValor> IndicadorValors { get; set; } = new List<IndicadorValor>();

    public virtual ICollection<ListaPrecio> ListaPrecios { get; set; } = new List<ListaPrecio>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<OrdenCompra> OrdenCompras { get; set; } = new List<OrdenCompra>();

    public virtual ICollection<OrdenProduccion> OrdenProduccions { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<PaqueteInformacion> PaqueteInformacions { get; set; } = new List<PaqueteInformacion>();

    public virtual ICollection<ParametroSistema> ParametroSistemas { get; set; } = new List<ParametroSistema>();

    public virtual ICollection<PeriodoContable> PeriodoContables { get; set; } = new List<PeriodoContable>();

    public virtual ICollection<PeriodoNomina> PeriodoNominas { get; set; } = new List<PeriodoNomina>();

    public virtual ICollection<PlanProduccion> PlanProduccions { get; set; } = new List<PlanProduccion>();

    public virtual ICollection<PlantillaAprobadum> PlantillaAprobada { get; set; } = new List<PlantillaAprobadum>();

    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();

    public virtual ICollection<ReporteGenerado> ReporteGenerados { get; set; } = new List<ReporteGenerado>();

    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
