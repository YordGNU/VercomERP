namespace Vercom.Models;

public partial class Usuario
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string? Email { get; set; }

    public string HashPassword { get; set; } = null!;

    public bool DebeCambiarPass { get; set; }

    public short IntentosFallidos { get; set; }

    public DateTimeOffset? BloqueadoHasta { get; set; }

    public DateTimeOffset? UltimoLogin { get; set; }

    public bool Activo { get; set; }

    public Guid? EsEmpleadoId { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual ICollection<ApiCliente> ApiClientes { get; set; } = new List<ApiCliente>();

    public virtual ICollection<AsientoContable> AsientoContables { get; set; } = new List<AsientoContable>();

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<ConteoFisico> ConteoFisicos { get; set; } = new List<ConteoFisico>();

    public virtual ICollection<DeclaracionJuradum> DeclaracionJurada { get; set; } = new List<DeclaracionJuradum>();

    public virtual ICollection<DevolucionVentum> DevolucionVenta { get; set; } = new List<DevolucionVentum>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Empleado? EsEmpleado { get; set; }

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<FichaCosto> FichaCostos { get; set; } = new List<FichaCosto>();

    public virtual ICollection<MantenimientoProgramado> MantenimientoProgramados { get; set; } = new List<MantenimientoProgramado>();

    public virtual ICollection<MovimientoCajaPo> MovimientoCajaPos { get; set; } = new List<MovimientoCajaPo>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<OrdenCompra> OrdenCompraAprobadoPorNavigations { get; set; } = new List<OrdenCompra>();

    public virtual ICollection<OrdenCompra> OrdenCompraCreadoPorNavigations { get; set; } = new List<OrdenCompra>();

    public virtual ICollection<OrdenProduccion> OrdenProduccions { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<PaqueteInformacion> PaqueteInformacions { get; set; } = new List<PaqueteInformacion>();

    public virtual ICollection<PeriodoContable> PeriodoContables { get; set; } = new List<PeriodoContable>();

    public virtual ICollection<PeriodoNomina> PeriodoNominas { get; set; } = new List<PeriodoNomina>();

    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();

    public virtual ICollection<RecepcionCompra> RecepcionCompras { get; set; } = new List<RecepcionCompra>();

    public virtual ICollection<RegistroAsistencium> RegistroAsistencia { get; set; } = new List<RegistroAsistencium>();

    public virtual ICollection<ReporteGenerado> ReporteGenerados { get; set; } = new List<ReporteGenerado>();

    public virtual ICollection<SesionCajaPo> SesionCajaPoCajeros { get; set; } = new List<SesionCajaPo>();

    public virtual ICollection<SesionCajaPo> SesionCajaPoSupervisorConciliacions { get; set; } = new List<SesionCajaPo>();

    public virtual Sucursal? Sucursal { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRolAsignadoPorNavigations { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<UsuarioRol> UsuarioRolUsuarios { get; set; } = new List<UsuarioRol>();
}
