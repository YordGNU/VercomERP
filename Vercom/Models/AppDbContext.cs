using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Vercom.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivoFijo> ActivoFijos { get; set; }

    public virtual DbSet<ActivoFijoDepreciacion> ActivoFijoDepreciacions { get; set; }

    public virtual DbSet<Almacen> Almacens { get; set; }

    public virtual DbSet<AnalisisDesviacion> AnalisisDesviacions { get; set; }

    public virtual DbSet<ApiCliente> ApiClientes { get; set; }

    public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<ApiRateLimit> ApiRateLimits { get; set; }

    public virtual DbSet<ApiToken> ApiTokens { get; set; }

    public virtual DbSet<AsientoContable> AsientoContables { get; set; }

    public virtual DbSet<AsientoDetalle> AsientoDetalles { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<BackupLog> BackupLogs { get; set; }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<Cargo> Cargos { get; set; }

    public virtual DbSet<CentroCosto> CentroCostos { get; set; }

    public virtual DbSet<CertificadoMedico> CertificadoMedicos { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ConceptoNomina> ConceptoNominas { get; set; }

    public virtual DbSet<Consecutivo> Consecutivos { get; set; }

    public virtual DbSet<ConteoFisico> ConteoFisicos { get; set; }

    public virtual DbSet<ConteoFisicoDetalle> ConteoFisicoDetalles { get; set; }

    public virtual DbSet<ContratoEconomico> ContratoEconomicos { get; set; }

    public virtual DbSet<ContratoLaboral> ContratoLaborals { get; set; }

    public virtual DbSet<CuentaBancarium> CuentaBancaria { get; set; }

    public virtual DbSet<CuentaContable> CuentaContables { get; set; }

    public virtual DbSet<CuentaPorCobrar> CuentaPorCobrars { get; set; }

    public virtual DbSet<CuentaPorPagar> CuentaPorPagars { get; set; }

    public virtual DbSet<DeclaracionJuradum> DeclaracionJurada { get; set; }

    public virtual DbSet<DevolucionVentaDetalle> DevolucionVentaDetalles { get; set; }

    public virtual DbSet<DevolucionVentum> DevolucionVenta { get; set; }

    public virtual DbSet<DispositivoPo> DispositivoPos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Entidad> Entidads { get; set; }

    public virtual DbSet<Equipo> Equipos { get; set; }

    public virtual DbSet<Existencium> Existencia { get; set; }

    public virtual DbSet<FacturaVentaDetalle> FacturaVentaDetalles { get; set; }

    public virtual DbSet<FacturaVentum> FacturaVenta { get; set; }

    public virtual DbSet<FamiliaProducto> FamiliaProductos { get; set; }

    public virtual DbSet<FichaCosto> FichaCostos { get; set; }

    public virtual DbSet<FormaPagoVentum> FormaPagoVenta { get; set; }

    public virtual DbSet<Indicador> Indicadors { get; set; }

    public virtual DbSet<IndicadorValor> IndicadorValors { get; set; }

    public virtual DbSet<ListaMateriale> ListaMateriales { get; set; }

    public virtual DbSet<ListaMaterialesDetalle> ListaMaterialesDetalles { get; set; }

    public virtual DbSet<ListaPrecio> ListaPrecios { get; set; }

    public virtual DbSet<ListaPrecioDetalle> ListaPrecioDetalles { get; set; }

    public virtual DbSet<MantenimientoProgramado> MantenimientoProgramados { get; set; }

    public virtual DbSet<Merma> Mermas { get; set; }

    public virtual DbSet<MovimientoBancario> MovimientoBancarios { get; set; }

    public virtual DbSet<MovimientoCajaPo> MovimientoCajaPos { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientoInventarios { get; set; }

    public virtual DbSet<MovimientoInventarioDetalle> MovimientoInventarioDetalles { get; set; }

    public virtual DbSet<NominaDetalle> NominaDetalles { get; set; }

    public virtual DbSet<NominaDetalleConcepto> NominaDetalleConceptos { get; set; }

    public virtual DbSet<OrdenCompra> OrdenCompras { get; set; }

    public virtual DbSet<OrdenCompraDetalle> OrdenCompraDetalles { get; set; }

    public virtual DbSet<OrdenProduccion> OrdenProduccions { get; set; }

    public virtual DbSet<OrdenProduccionConsumo> OrdenProduccionConsumos { get; set; }

    public virtual DbSet<PagoAplicado> PagoAplicados { get; set; }

    public virtual DbSet<PaqueteInformacion> PaqueteInformacions { get; set; }

    public virtual DbSet<ParametroSistema> ParametroSistemas { get; set; }

    public virtual DbSet<PeriodoContable> PeriodoContables { get; set; }

    public virtual DbSet<PeriodoNomina> PeriodoNominas { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<PlanProduccion> PlanProduccions { get; set; }

    public virtual DbSet<PlanProduccionDetalle> PlanProduccionDetalles { get; set; }

    public virtual DbSet<PlantillaAprobadum> PlantillaAprobada { get; set; }

    public virtual DbSet<PosRangoNumeracion> PosRangoNumeracions { get; set; }

    public virtual DbSet<PosSyncLog> PosSyncLogs { get; set; }

    public virtual DbSet<PosVentaPendiente> PosVentaPendientes { get; set; }

    public virtual DbSet<Presupuesto> Presupuestos { get; set; }

    public virtual DbSet<PresupuestoLinea> PresupuestoLineas { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<RecepcionCompra> RecepcionCompras { get; set; }

    public virtual DbSet<RegistroAsistencium> RegistroAsistencia { get; set; }

    public virtual DbSet<RegistroSalarioTiempoServicio> RegistroSalarioTiempoServicios { get; set; }

    public virtual DbSet<ReporteGenerado> ReporteGenerados { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<SaldoVacacione> SaldoVacaciones { get; set; }

    public virtual DbSet<SesionCajaPo> SesionCajaPos { get; set; }

    public virtual DbSet<Sucursal> Sucursals { get; set; }

    public virtual DbSet<TipoAusencium> TipoAusencia { get; set; }

    public virtual DbSet<TipoComprobante> TipoComprobantes { get; set; }

    public virtual DbSet<TipoMovimiento> TipoMovimientos { get; set; }

    public virtual DbSet<TipoObligacionFiscal> TipoObligacionFiscals { get; set; }

    public virtual DbSet<TopePrecioMfp> TopePrecioMfps { get; set; }

    public virtual DbSet<UnidadMedidum> UnidadMedida { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRol> UsuarioRols { get; set; }

    public virtual DbSet<VAuditoriaAcceso> VAuditoriaAccesos { get; set; }

    public virtual DbSet<VAuditoriaReversione> VAuditoriaReversiones { get; set; }

    public virtual DbSet<VEjecucionPresupuesto> VEjecucionPresupuestos { get; set; }

    public virtual DbSet<VSaldoCuentum> VSaldoCuenta { get; set; }

    public virtual DbSet<WebhookEntrega> WebhookEntregas { get; set; }

    public virtual DbSet<WebhookSuscripcion> WebhookSuscripcions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=VercomERP;User ID=sa;Password=sql2025*;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivoFijo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activo_f__3213E83F18BF88AB");

            entity.ToTable("activo_fijo", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.CodigoInventario }, "UQ_activo_fijo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(30)
                .HasColumnName("codigo_inventario");
            entity.Property(e => e.CuentaActivoId).HasColumnName("cuenta_activo_id");
            entity.Property(e => e.CuentaDepreciacionId).HasColumnName("cuenta_depreciacion_id");
            entity.Property(e => e.CuentaGastoDepId).HasColumnName("cuenta_gasto_dep_id");
            entity.Property(e => e.DepreciacionAcumulada)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("depreciacion_acumulada");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaAdquisicion).HasColumnName("fecha_adquisicion");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.MetodoDepreciacion)
                .HasMaxLength(20)
                .HasDefaultValue("LINEA_RECTA")
                .HasColumnName("metodo_depreciacion");
            entity.Property(e => e.MotivoBaja).HasColumnName("motivo_baja");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.TasaDepreciacionAnual)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tasa_depreciacion_anual");
            entity.Property(e => e.ValorAdquisicion)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("valor_adquisicion");
            entity.Property(e => e.ValorResidual)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("valor_residual");
            entity.Property(e => e.VidaUtilMeses).HasColumnName("vida_util_meses");

            entity.HasOne(d => d.CuentaActivo).WithMany(p => p.ActivoFijoCuentaActivos)
                .HasForeignKey(d => d.CuentaActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__44CA3770");

            entity.HasOne(d => d.CuentaDepreciacion).WithMany(p => p.ActivoFijoCuentaDepreciacions)
                .HasForeignKey(d => d.CuentaDepreciacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__45BE5BA9");

            entity.HasOne(d => d.CuentaGastoDep).WithMany(p => p.ActivoFijoCuentaGastoDeps)
                .HasForeignKey(d => d.CuentaGastoDepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__46B27FE2");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ActivoFijos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__entid__42E1EEFE");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.ActivoFijos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__activo_fi__sucur__43D61337");
        });

        modelBuilder.Entity<ActivoFijoDepreciacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activo_f__3213E83F5F63E290");

            entity.ToTable("activo_fijo_depreciacion", "contabilidad");

            entity.HasIndex(e => new { e.ActivoFijoId, e.PeriodoId }, "UQ_activo_fijo_depreciacion").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActivoFijoId).HasColumnName("activo_fijo_id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CalculadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("calculado_en");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");

            entity.HasOne(d => d.ActivoFijo).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.ActivoFijoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__activ__51300E55");

            entity.HasOne(d => d.Asiento).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__activo_fi__asien__531856C7");

            entity.HasOne(d => d.Periodo).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__perio__5224328E");
        });

        modelBuilder.Entity<Almacen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__almacen__3213E83FC40BF5F5");

            entity.ToTable("almacen", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_almacen").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.EsPuntoVenta).HasColumnName("es_punto_venta");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Almacens)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__almacen__entidad__314D4EA8");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Almacens)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__almacen__sucursa__324172E1");
        });

        modelBuilder.Entity<AnalisisDesviacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__analisis__3213E83FB766ABC5");

            entity.ToTable("analisis_desviacion", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AnalizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("analizado_en");
            entity.Property(e => e.Componente)
                .HasMaxLength(20)
                .HasColumnName("componente");
            entity.Property(e => e.CostoEstandar)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("costo_estandar");
            entity.Property(e => e.CostoReal)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("costo_real");
            entity.Property(e => e.Desviacion)
                .HasComputedColumnSql("([costo_real]-[costo_estandar])", true)
                .HasColumnType("decimal(17, 4)")
                .HasColumnName("desviacion");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.AnalisisDesviacions)
                .HasForeignKey(d => d.OrdenProduccionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__analisis___orden__21D600EE");
        });

        modelBuilder.Entity<ApiCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_clie__3213E83FF0CAADF9");

            entity.ToTable("api_cliente", "integracion");

            entity.HasIndex(e => e.ClientId, "UQ__api_clie__BF21A4251CD407FB").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ClientId)
                .HasMaxLength(64)
                .HasColumnName("client_id");
            entity.Property(e => e.ClientSecretHash)
                .HasMaxLength(255)
                .HasColumnName("client_secret_hash");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.RevocadoEn).HasColumnName("revocado_en");
            entity.Property(e => e.Scopes).HasColumnName("scopes");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasDefaultValue("POS")
                .HasColumnName("tipo");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.ApiClientes)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__api_clien__cread__5CC1BC92");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ApiClientes)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__api_clien__entid__58F12BAE");
        });

        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_log__3213E83F7DE43663");

            entity.ToTable("api_log", "integracion");

            entity.HasIndex(e => new { e.ApiClienteId, e.OcurridoEn }, "idx_api_log_cliente_fecha");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.CodigoRespuesta).HasColumnName("codigo_respuesta");
            entity.Property(e => e.DuracionMs).HasColumnName("duracion_ms");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(255)
                .HasColumnName("endpoint");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.MetodoHttp)
                .HasMaxLength(10)
                .HasColumnName("metodo_http");
            entity.Property(e => e.OcurridoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("ocurrido_en");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.ApiLogs)
                .HasForeignKey(d => d.ApiClienteId)
                .HasConstraintName("FK__api_log__api_cli__6DEC4894");
        });

        modelBuilder.Entity<ApiRateLimit>(entity =>
        {
            entity.HasKey(e => e.ApiClienteId).HasName("PK__api_rate__089F45226BC1C59A");

            entity.ToTable("api_rate_limit", "integracion");

            entity.Property(e => e.ApiClienteId)
                .ValueGeneratedNever()
                .HasColumnName("api_cliente_id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.SolicitudesPorMinuto)
                .HasDefaultValue(120)
                .HasColumnName("solicitudes_por_minuto");

            entity.HasOne(d => d.ApiCliente).WithOne(p => p.ApiRateLimit)
                .HasForeignKey<ApiRateLimit>(d => d.ApiClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__api_rate___api_c__69279377");
        });

        modelBuilder.Entity<ApiToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_toke__3213E83FBF6BD59E");

            entity.ToTable("api_token", "integracion");

            entity.HasIndex(e => e.TokenHash, "UQ__api_toke__9F6BDB13DD7FAE82").IsUnique();

            entity.HasIndex(e => e.ApiClienteId, "idx_api_token_cliente");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.EmitidoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("emitido_en");
            entity.Property(e => e.ExpiraEn).HasColumnName("expira_en");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.Revocado).HasColumnName("revocado");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasDefaultValue("ACCESS")
                .HasColumnName("tipo");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(255)
                .HasColumnName("token_hash");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.ApiTokens)
                .HasForeignKey(d => d.ApiClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__api_token__api_c__627A95E8");
        });

        modelBuilder.Entity<AsientoContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asiento___3213E83FFB390ADA");

            entity.ToTable("asiento_contable", "contabilidad", tb => tb.HasTrigger("trg_auditar_asiento_contable"));

            entity.HasIndex(e => new { e.EntidadId, e.TipoComprobanteId, e.NumeroComprobante }, "UQ_asiento_comprobante").IsUnique();

            entity.HasIndex(e => new { e.ModuloOrigen, e.DocumentoOrigenId }, "idx_asiento_origen");

            entity.HasIndex(e => e.PeriodoId, "idx_asiento_periodo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoReversionId).HasColumnName("asiento_reversion_id");
            entity.Property(e => e.Concepto).HasColumnName("concepto");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenTipo)
                .HasMaxLength(50)
                .HasColumnName("documento_origen_tipo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("CONTABILIZADO")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.ModuloOrigen)
                .HasMaxLength(30)
                .HasDefaultValue("CONTABILIDAD")
                .HasColumnName("modulo_origen");
            entity.Property(e => e.NumeroComprobante).HasColumnName("numero_comprobante");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.TipoComprobanteId).HasColumnName("tipo_comprobante_id");
            entity.Property(e => e.TotalDebe)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_debe");
            entity.Property(e => e.TotalHaber)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_haber");

            entity.HasOne(d => d.AsientoReversion).WithMany(p => p.InverseAsientoReversion)
                .HasForeignKey(d => d.AsientoReversionId)
                .HasConstraintName("FK__asiento_c__asien__2EDAF651");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__cread__2FCF1A8A");

            entity.HasOne(d => d.Entidad).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__entid__2739D489");

            entity.HasOne(d => d.Periodo).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__perio__29221CFB");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__asiento_c__sucur__282DF8C2");

            entity.HasOne(d => d.TipoComprobante).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.TipoComprobanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__tipo___2A164134");
        });

        modelBuilder.Entity<AsientoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asiento___3213E83F9968017A");

            entity.ToTable("asiento_detalle", "contabilidad");

            entity.HasIndex(e => new { e.AsientoId, e.Linea }, "UQ_asiento_detalle_linea").IsUnique();

            entity.HasIndex(e => e.CuentaId, "idx_asiento_detalle_cuenta");

            entity.HasIndex(e => new { e.TerceroTipo, e.TerceroId }, "idx_asiento_detalle_tercero");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CentroCostoId).HasColumnName("centro_costo_id");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Debe)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("debe");
            entity.Property(e => e.Glosa)
                .HasMaxLength(255)
                .HasColumnName("glosa");
            entity.Property(e => e.Haber)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("haber");
            entity.Property(e => e.Linea).HasColumnName("linea");
            entity.Property(e => e.TerceroId).HasColumnName("tercero_id");
            entity.Property(e => e.TerceroTipo)
                .HasMaxLength(20)
                .HasColumnName("tercero_tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__asiento_d__asien__367C1819");

            entity.HasOne(d => d.CentroCosto).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.CentroCostoId)
                .HasConstraintName("FK__asiento_d__centr__3864608B");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_d__cuent__37703C52");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auditori__3213E83FE4032522");

            entity.ToTable("auditoria", "nucleo");

            entity.HasIndex(e => e.OcurridoEn, "idx_auditoria_fecha");

            entity.HasIndex(e => new { e.EsquemaTabla, e.RegistroId }, "idx_auditoria_tabla_registro");

            entity.HasIndex(e => e.UsuarioId, "idx_auditoria_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accion)
                .HasMaxLength(20)
                .HasColumnName("accion");
            entity.Property(e => e.Canal)
                .HasMaxLength(20)
                .HasDefaultValue("ERP")
                .HasColumnName("canal");
            entity.Property(e => e.DispositivoId).HasColumnName("dispositivo_id");
            entity.Property(e => e.EsquemaTabla)
                .HasMaxLength(100)
                .HasColumnName("esquema_tabla");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.OcurridoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("ocurrido_en");
            entity.Property(e => e.RegistroId)
                .HasMaxLength(100)
                .HasColumnName("registro_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ValoresAnteriores).HasColumnName("valores_anteriores");
            entity.Property(e => e.ValoresNuevos).HasColumnName("valores_nuevos");

            entity.HasOne(d => d.Dispositivo).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.DispositivoId)
                .HasConstraintName("fk_auditoria_dispositivo");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__auditoria__usuar__68487DD7");
        });

        modelBuilder.Entity<BackupLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__backup_l__3213E83F9AB22C49");

            entity.ToTable("backup_log", "nucleo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("EN_PROGRESO")
                .HasColumnName("estado");
            entity.Property(e => e.FinalizadoEn).HasColumnName("finalizado_en");
            entity.Property(e => e.IniciadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("iniciado_en");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.RutaArchivo).HasColumnName("ruta_archivo");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__caja__3213E83FF2DDCBB0");

            entity.ToTable("caja", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.LimiteEfectivo)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("limite_efectivo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.SaldoActual)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo_actual");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.CuentaContableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__cuenta_con__02C769E9");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__entidad_id__00DF2177");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__sucursal_i__01D345B0");
        });

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cargo__3213E83F74A08FD1");

            entity.ToTable("cargo", "rrhh");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_cargo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SalarioEscalaMax)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_escala_max");
            entity.Property(e => e.SalarioEscalaMin)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_escala_min");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Cargos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cargo__entidad_i__2AD55B43");
        });

        modelBuilder.Entity<CentroCosto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__centro_c__3213E83F93700311");

            entity.ToTable("centro_costo", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_centro_costo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CentroCostos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__centro_co__entid__151B244E");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.CentroCostos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__centro_co__sucur__160F4887");
        });

        modelBuilder.Entity<CertificadoMedico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83FEEC4C3B6");

            entity.ToTable("certificado_medico", "rrhh");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DiagnosticoCie)
                .HasMaxLength(20)
                .HasColumnName("diagnostico_cie");
            entity.Property(e => e.Dias)
                .HasComputedColumnSql("(datediff(day,[fecha_inicio],[fecha_fin])+(1))", true)
                .HasColumnName("dias");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.NumeroCertificado)
                .HasMaxLength(40)
                .HasColumnName("numero_certificado");
            entity.Property(e => e.PorcentajeSubsidio)
                .HasDefaultValue(100m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentaje_subsidio");

            entity.HasOne(d => d.Empleado).WithMany(p => p.CertificadoMedicos)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__certifica__emple__5E54FF49");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cliente__3213E83FE0ACB414");

            entity.ToTable("cliente", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.NitOCi }, "UQ_cliente_nit").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.LimiteCredito)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("limite_credito");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.NitOCi)
                .HasMaxLength(20)
                .HasColumnName("nit_o_ci");
            entity.Property(e => e.NombreRazonSocial)
                .HasMaxLength(255)
                .HasColumnName("nombre_razon_social");
            entity.Property(e => e.Segmento)
                .HasMaxLength(20)
                .HasDefaultValue("MINORISTA")
                .HasColumnName("segmento");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoPersona)
                .HasMaxLength(15)
                .HasDefaultValue("JURIDICA")
                .HasColumnName("tipo_persona");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.CuentaContableId)
                .HasConstraintName("FK__cliente__cuenta___51851410");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cliente__entidad__4AD81681");

            entity.HasOne(d => d.ListaPrecio).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.ListaPrecioId)
                .HasConstraintName("FK__cliente__lista_p__4F9CCB9E");
        });

        modelBuilder.Entity<ConceptoNomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__concepto__3213E83FEAFDC4BF");

            entity.ToTable("concepto_nomina", "rrhh");

            entity.HasIndex(e => e.Codigo, "UQ__concepto__40F9A2061CB83EBC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.Formula).HasColumnName("formula");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasColumnName("tipo");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.ConceptoNominas)
                .HasForeignKey(d => d.CuentaContableId)
                .HasConstraintName("FK__concepto___cuent__6501FCD8");
        });

        modelBuilder.Entity<Consecutivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__consecut__3213E83F5070AF09");

            entity.ToTable("consecutivo", "nucleo");

            entity.HasIndex(e => new { e.EntidadId, e.SucursalId, e.TipoDocumento, e.Serie }, "UQ_consecutivo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.LongitudPadding)
                .HasDefaultValue((short)8)
                .HasColumnName("longitud_padding");
            entity.Property(e => e.Serie)
                .HasMaxLength(10)
                .HasDefaultValue("A")
                .HasColumnName("serie");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(40)
                .HasColumnName("tipo_documento");
            entity.Property(e => e.UltimoNumero).HasColumnName("ultimo_numero");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Consecutivos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__consecuti__entid__7D439ABD");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Consecutivos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__consecuti__sucur__7E37BEF6");
        });

        modelBuilder.Entity<ConteoFisico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conteo_f__3213E83F4777EFE8");

            entity.ToTable("conteo_fisico", "inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("EN_PROCESO")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.ResponsableId).HasColumnName("responsable_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasDefaultValue("PARCIAL")
                .HasColumnName("tipo");

            entity.HasOne(d => d.Almacen).WithMany(p => p.ConteoFisicos)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__conteo_fi__almac__4FD1D5C8");

            entity.HasOne(d => d.Responsable).WithMany(p => p.ConteoFisicos)
                .HasForeignKey(d => d.ResponsableId)
                .HasConstraintName("FK__conteo_fi__respo__54968AE5");
        });

        modelBuilder.Entity<ConteoFisicoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conteo_f__3213E83F2734635A");

            entity.ToTable("conteo_fisico_detalle", "inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadFisica)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("cantidad_fisica");
            entity.Property(e => e.CantidadSistema)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("cantidad_sistema");
            entity.Property(e => e.ConteoId).HasColumnName("conteo_id");
            entity.Property(e => e.Diferencia)
                .HasComputedColumnSql("([cantidad_fisica]-[cantidad_sistema])", true)
                .HasColumnType("decimal(17, 4)")
                .HasColumnName("diferencia");
            entity.Property(e => e.Justificacion).HasColumnName("justificacion");
            entity.Property(e => e.MovimientoAjusteId).HasColumnName("movimiento_ajuste_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Conteo).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.ConteoId)
                .HasConstraintName("FK__conteo_fi__conte__595B4002");

            entity.HasOne(d => d.MovimientoAjuste).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.MovimientoAjusteId)
                .HasConstraintName("FK__conteo_fi__movim__5B438874");

            entity.HasOne(d => d.Producto).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__conteo_fi__produ__5A4F643B");
        });

        modelBuilder.Entity<ContratoEconomico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__contrato__3213E83F697B9821");

            entity.ToTable("contrato_economico", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroContrato }, "UQ_contrato_economico").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DocumentoUrl).HasColumnName("documento_url");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("VIGENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaFirma).HasColumnName("fecha_firma");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.NumeroContrato)
                .HasMaxLength(40)
                .HasColumnName("numero_contrato");
            entity.Property(e => e.Objeto).HasColumnName("objeto");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.TerceroTipo)
                .HasMaxLength(15)
                .HasColumnName("tercero_tipo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.ContratoEconomicos)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK__contrato___clien__5A1A5A11");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ContratoEconomicos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___entid__5832119F");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ContratoEconomicos)
                .HasForeignKey(d => d.ProveedorId)
                .HasConstraintName("FK__contrato___prove__5B0E7E4A");
        });

        modelBuilder.Entity<ContratoLaboral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__contrato__3213E83FF250B18F");

            entity.ToTable("contrato_laboral", "rrhh");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CargoId).HasColumnName("cargo_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DocumentoUrl).HasColumnName("documento_url");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("VIGENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.JornadaHorasSemana)
                .HasDefaultValue(44m)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("jornada_horas_semana");
            entity.Property(e => e.SalarioPactado)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_pactado");
            entity.Property(e => e.TipoContrato)
                .HasMaxLength(20)
                .HasColumnName("tipo_contrato");

            entity.HasOne(d => d.Cargo).WithMany(p => p.ContratoLaborals)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___cargo__43A1090D");

            entity.HasOne(d => d.Empleado).WithMany(p => p.ContratoLaborals)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___emple__40C49C62");
        });

        modelBuilder.Entity<CuentaBancarium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_b__3213E83F27FCAE9F");

            entity.ToTable("cuenta_bancaria", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroCuenta }, "UQ_cuenta_bancaria").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Banco)
                .HasMaxLength(100)
                .HasColumnName("banco");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(40)
                .HasColumnName("numero_cuenta");
            entity.Property(e => e.SaldoActual)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo_actual");
            entity.Property(e => e.TipoCuenta)
                .HasMaxLength(20)
                .HasColumnName("tipo_cuenta");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.CuentaBancaria)
                .HasForeignKey(d => d.CuentaContableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_ba__cuent__74794A92");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaBancaria)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_ba__entid__72910220");
        });

        modelBuilder.Entity<CuentaContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_c__3213E83F1C86FB58");

            entity.ToTable("cuenta_contable", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_cuenta_contable").IsUnique();

            entity.HasIndex(e => e.CuentaPadreId, "idx_cuenta_padre");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AceptaMovimiento)
                .HasDefaultValue(true)
                .HasColumnName("acepta_movimiento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clase)
                .HasMaxLength(20)
                .HasColumnName("clase");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaPadreId).HasColumnName("cuenta_padre_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.Naturaleza)
                .HasMaxLength(10)
                .HasColumnName("naturaleza");
            entity.Property(e => e.Nivel)
                .HasDefaultValue((short)1)
                .HasColumnName("nivel");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.RequiereCentroCosto).HasColumnName("requiere_centro_costo");
            entity.Property(e => e.RequiereTercero).HasColumnName("requiere_tercero");

            entity.HasOne(d => d.CuentaPadre).WithMany(p => p.InverseCuentaPadre)
                .HasForeignKey(d => d.CuentaPadreId)
                .HasConstraintName("FK__cuenta_co__cuent__07C12930");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_co__entid__06CD04F7");
        });

        modelBuilder.Entity<CuentaPorCobrar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_p__3213E83F7C403EFC");

            entity.ToTable("cuenta_por_cobrar", "contabilidad");

            entity.HasIndex(e => e.ClienteId, "idx_cxc_cliente");

            entity.HasIndex(e => e.FechaVencimiento, "idx_cxc_vencimiento").HasFilter("([estado] IN ('PENDIENTE', 'PARCIAL'))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoOrigenId).HasColumnName("asiento_origen_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenTipo)
                .HasMaxLength(50)
                .HasColumnName("documento_origen_tipo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.MontoOriginal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_original");
            entity.Property(e => e.SaldoPendiente)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo_pendiente");

            entity.HasOne(d => d.AsientoOrigen).WithMany(p => p.CuentaPorCobrars)
                .HasForeignKey(d => d.AsientoOrigenId)
                .HasConstraintName("FK__cuenta_po__asien__58D1301D");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaPorCobrars)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__entid__57DD0BE4");
        });

        modelBuilder.Entity<CuentaPorPagar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_p__3213E83F6F1F93FA");

            entity.ToTable("cuenta_por_pagar", "contabilidad");

            entity.HasIndex(e => e.ProveedorId, "idx_cxp_proveedor");

            entity.HasIndex(e => e.FechaVencimiento, "idx_cxp_vencimiento").HasFilter("([estado] IN ('PENDIENTE', 'PARCIAL'))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoOrigenId).HasColumnName("asiento_origen_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenTipo)
                .HasMaxLength(50)
                .HasColumnName("documento_origen_tipo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.MontoOriginal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_original");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.SaldoPendiente)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("saldo_pendiente");

            entity.HasOne(d => d.AsientoOrigen).WithMany(p => p.CuentaPorPagars)
                .HasForeignKey(d => d.AsientoOrigenId)
                .HasConstraintName("FK__cuenta_po__asien__6166761E");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaPorPagars)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__entid__607251E5");
        });

        modelBuilder.Entity<DeclaracionJuradum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__declarac__3213E83FAFC76116");

            entity.ToTable("declaracion_jurada", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.TipoObligacionId, e.PeriodoId }, "UQ_declaracion_jurada").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.BaseImponible)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("base_imponible");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FechaLimite).HasColumnName("fecha_limite");
            entity.Property(e => e.FechaPresentacion).HasColumnName("fecha_presentacion");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.MontoCalculado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_calculado");
            entity.Property(e => e.MontoPagado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_pagado");
            entity.Property(e => e.NumeroDj)
                .HasMaxLength(40)
                .HasColumnName("numero_dj");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.TipoObligacionId).HasColumnName("tipo_obligacion_id");

            entity.HasOne(d => d.Asiento).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__declaraci__asien__12FDD1B2");

            entity.HasOne(d => d.Entidad).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__entid__0D44F85C");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__declaraci__gener__13F1F5EB");

            entity.HasOne(d => d.Periodo).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__perio__0F2D40CE");

            entity.HasOne(d => d.TipoObligacion).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.TipoObligacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__tipo___0E391C95");
        });

        modelBuilder.Entity<DevolucionVentaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__devoluci__3213E83F17D7053B");

            entity.ToTable("devolucion_venta_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadDevuelta)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_devuelta");
            entity.Property(e => e.DevolucionId).HasColumnName("devolucion_id");
            entity.Property(e => e.FacturaDetalleId).HasColumnName("factura_detalle_id");

            entity.HasOne(d => d.Devolucion).WithMany(p => p.DevolucionVentaDetalles)
                .HasForeignKey(d => d.DevolucionId)
                .HasConstraintName("FK__devolucio__devol__2E06CDA9");

            entity.HasOne(d => d.FacturaDetalle).WithMany(p => p.DevolucionVentaDetalles)
                .HasForeignKey(d => d.FacturaDetalleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__factu__2EFAF1E2");
        });

        modelBuilder.Entity<DevolucionVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__devoluci__3213E83FF9957370");

            entity.ToTable("devolucion_venta", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.AutorizadoPor).HasColumnName("autorizado_por");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("fecha");
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.TotalDevuelto)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_devuelto");

            entity.HasOne(d => d.Asiento).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__devolucio__asien__2942188C");

            entity.HasOne(d => d.AutorizadoPorNavigation).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.AutorizadoPor)
                .HasConstraintName("FK__devolucio__autor__2A363CC5");

            entity.HasOne(d => d.Factura).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__factu__2665ABE1");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__devolucio__movim__284DF453");
        });

        modelBuilder.Entity<DispositivoPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposit__3213E83F8D125E5E");

            entity.ToTable("dispositivo_pos", "integracion");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_dispositivo_pos").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.CajaId).HasColumnName("caja_id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVO")
                .HasColumnName("estado");
            entity.Property(e => e.IdentificadorHardware)
                .HasMaxLength(100)
                .HasColumnName("identificador_hardware");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.UltimaSincronizacion).HasColumnName("ultima_sincronizacion");
            entity.Property(e => e.VersionAppPos)
                .HasMaxLength(20)
                .HasColumnName("version_app_pos");

            entity.HasOne(d => d.Almacen).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__almac__758D6A5C");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.ApiClienteId)
                .HasConstraintName("FK__dispositi__api_c__76818E95");

            entity.HasOne(d => d.Caja).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.CajaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__caja___7775B2CE");

            entity.HasOne(d => d.Entidad).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__entid__73A521EA");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__sucur__74994623");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__empleado__3213E83F07D30E27");

            entity.ToTable("empleado", "rrhh");

            entity.HasIndex(e => e.CarnetIdentidad, "UQ__empleado__9562E2D563B3051C").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.Calificacion)
                .HasMaxLength(100)
                .HasColumnName("calificacion");
            entity.Property(e => e.CargoId).HasColumnName("cargo_id");
            entity.Property(e => e.CarnetIdentidad)
                .HasMaxLength(11)
                .HasColumnName("carnet_identidad");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaBancariaPago)
                .HasMaxLength(40)
                .HasColumnName("cuenta_bancaria_pago");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.MotivoBaja).HasColumnName("motivo_baja");
            entity.Property(e => e.NivelEscolaridad)
                .HasMaxLength(50)
                .HasColumnName("nivel_escolaridad");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .HasColumnName("sexo");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");

            entity.HasOne(d => d.Cargo).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__empleado__cargo___39237A9A");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__empleado__entida__36470DEF");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__empleado__sucurs__373B3228");
        });

        modelBuilder.Entity<Entidad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__entidad__3213E83FBFDCAA27");

            entity.ToTable("entidad", "nucleo");

            entity.HasIndex(e => e.CodigoReeup, "UQ__entidad__2A391DFE1534D871").IsUnique();

            entity.HasIndex(e => e.Nit, "UQ__entidad__DF97D0E41670C511").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.CodigoReeup)
                .HasMaxLength(20)
                .HasColumnName("codigo_reeup");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DireccionLegal).HasColumnName("direccion_legal");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FechaConstitucion).HasColumnName("fecha_constitucion");
            entity.Property(e => e.FormaJuridica)
                .HasMaxLength(50)
                .HasDefaultValue("S.U.R.L.")
                .HasColumnName("forma_juridica");
            entity.Property(e => e.LicenciaActividad)
                .HasMaxLength(100)
                .HasColumnName("licencia_actividad");
            entity.Property(e => e.MonedaBase)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda_base");
            entity.Property(e => e.Municipio)
                .HasMaxLength(100)
                .HasColumnName("municipio");
            entity.Property(e => e.Nit)
                .HasMaxLength(20)
                .HasColumnName("nit");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(255)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.Provincia)
                .HasMaxLength(100)
                .HasColumnName("provincia");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .HasColumnName("razon_social");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__equipo__3213E83FA9916DC7");

            entity.ToTable("equipo", "produccion");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_equipo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActivoFijoId).HasColumnName("activo_fijo_id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("OPERATIVO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaUltimaRevision).HasColumnName("fecha_ultima_revision");
            entity.Property(e => e.FrecuenciaMantenimientoDias).HasColumnName("frecuencia_mantenimiento_dias");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.ActivoFijo).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.ActivoFijoId)
                .HasConstraintName("FK__equipo__activo_f__320C68B7");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__equipo__entidad___30242045");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__equipo__sucursal__3118447E");
        });

        modelBuilder.Entity<Existencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__existenc__3213E83F2961CDD9");

            entity.ToTable("existencia", "inventario");

            entity.HasIndex(e => new { e.AlmacenId, e.ProductoId }, "UQ_existencia").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoPromedio)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_promedio");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.StockMaximo)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("stock_maximo");
            entity.Property(e => e.StockMinimo)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("stock_minimo");

            entity.HasOne(d => d.Almacen).WithMany(p => p.Existencia)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__existenci__almac__44B528D7");

            entity.HasOne(d => d.Producto).WithMany(p => p.Existencia)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__existenci__produ__45A94D10");
        });

        modelBuilder.Entity<FacturaVentaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__factura___3213E83F34E7DA88");

            entity.ToTable("factura_venta_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitarioVenta)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_unitario_venta");
            entity.Property(e => e.DescuentoPorcentaje)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("descuento_porcentaje");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.ImpuestoPorcentaje)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("impuesto_porcentaje");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.SubtotalLinea)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("subtotal_linea");

            entity.HasOne(d => d.Factura).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__factura_v__factu__18178C8A");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__factura_v__movim__1CDC41A7");

            entity.HasOne(d => d.Producto).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__produ__190BB0C3");
        });

        modelBuilder.Entity<FacturaVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__factura___3213E83F09771637");

            entity.ToTable("factura_venta", "comercial", tb => tb.HasTrigger("trg_auditar_factura_venta"));

            entity.HasIndex(e => new { e.EntidadId, e.SucursalId, e.Serie, e.NumeroFactura }, "UQ_factura_venta").IsUnique();

            entity.HasIndex(e => new { e.CanalVenta, e.Fecha }, "idx_factura_canal");

            entity.HasIndex(e => e.ClienteId, "idx_factura_cliente");

            entity.HasIndex(e => e.SesionCajaPosId, "idx_factura_pos_sesion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CanalVenta)
                .HasMaxLength(20)
                .HasDefaultValue("ERP")
                .HasColumnName("canal_venta");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ContratoId).HasColumnName("contrato_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.CuentaPorCobrarId).HasColumnName("cuenta_por_cobrar_id");
            entity.Property(e => e.DescuentoTotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("descuento_total");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("EMITIDA")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("fecha");
            entity.Property(e => e.ImpuestoVentasTotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("impuesto_ventas_total");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.MotivoAnulacion).HasColumnName("motivo_anulacion");
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(30)
                .HasColumnName("numero_factura");
            entity.Property(e => e.Serie)
                .HasMaxLength(10)
                .HasDefaultValue("A")
                .HasColumnName("serie");
            entity.Property(e => e.SesionCajaPosId).HasColumnName("sesion_caja_pos_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.TipoVenta)
                .HasMaxLength(15)
                .HasDefaultValue("MINORISTA")
                .HasColumnName("tipo_venta");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Almacen).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__almac__0504B816");

            entity.HasOne(d => d.Asiento).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__factura_v__asien__116A8EFB");

            entity.HasOne(d => d.Cliente).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__clien__031C6FA4");

            entity.HasOne(d => d.Contrato).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.ContratoId)
                .HasConstraintName("FK__factura_v__contr__041093DD");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__factura_v__cread__1352D76D");

            entity.HasOne(d => d.CuentaPorCobrar).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.CuentaPorCobrarId)
                .HasConstraintName("FK__factura_v__cuent__125EB334");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.DispositivoPosId)
                .HasConstraintName("fk_factura_dispositivo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__entid__004002F9");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.SesionCajaPosId)
                .HasConstraintName("fk_factura_sesion_caja");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__sucur__01342732");
        });

        modelBuilder.Entity<FamiliaProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__familia___3213E83F5C6D229B");

            entity.ToTable("familia_producto", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_familia_producto").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.FamiliaPadreId).HasColumnName("familia_padre_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Entidad).WithMany(p => p.FamiliaProductos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__familia_p__entid__0EF836A4");

            entity.HasOne(d => d.FamiliaPadre).WithMany(p => p.InverseFamiliaPadre)
                .HasForeignKey(d => d.FamiliaPadreId)
                .HasConstraintName("FK__familia_p__famil__0FEC5ADD");
        });

        modelBuilder.Entity<FichaCosto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ficha_co__3213E83FAFD64332");

            entity.ToTable("ficha_costo", "produccion");

            entity.HasIndex(e => new { e.ProductoId, e.Version }, "UQ_ficha_costo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CostoManoObra)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_mano_obra");
            entity.Property(e => e.CostoMateriaPrima)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_materia_prima");
            entity.Property(e => e.CostoTotalUnitario)
                .HasComputedColumnSql("(([costo_materia_prima]+[costo_mano_obra])+[gastos_indirectos])", true)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("costo_total_unitario");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("VIGENTE")
                .HasColumnName("estado");
            entity.Property(e => e.GastosIndirectos)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("gastos_indirectos");
            entity.Property(e => e.MargenPorcentaje)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("margen_porcentaje");
            entity.Property(e => e.PrecioSugerido)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio_sugerido");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__ficha_cos__cread__67A95F59");

            entity.HasOne(d => d.Entidad).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ficha_cos__entid__60083D91");

            entity.HasOne(d => d.Producto).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ficha_cos__produ__60FC61CA");
        });

        modelBuilder.Entity<FormaPagoVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__forma_pa__3213E83F162354C2");

            entity.ToTable("forma_pago_venta", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(20)
                .HasColumnName("forma_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.ReferenciaExterna)
                .HasMaxLength(100)
                .HasColumnName("referencia_externa");
            entity.Property(e => e.VueltoEntregado)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("vuelto_entregado");

            entity.HasOne(d => d.Factura).WithMany(p => p.FormaPagoVenta)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__forma_pag__factu__20ACD28B");
        });

        modelBuilder.Entity<Indicador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__indicado__3213E83F63E15574");

            entity.ToTable("indicador", "reportes");

            entity.HasIndex(e => e.Codigo, "UQ__indicado__40F9A206C74DE20F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoria)
                .HasMaxLength(30)
                .HasColumnName("categoria");
            entity.Property(e => e.Codigo)
                .HasMaxLength(40)
                .HasColumnName("codigo");
            entity.Property(e => e.FormulaDescripcion).HasColumnName("formula_descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<IndicadorValor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__indicado__3213E83F8259BCB3");

            entity.ToTable("indicador_valor", "reportes");

            entity.HasIndex(e => new { e.EntidadId, e.IndicadorId, e.PeriodoId }, "UQ_indicador_valor").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CalculadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("calculado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.IndicadorId).HasColumnName("indicador_id");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("valor");

            entity.HasOne(d => d.Entidad).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__entid__3E3D3572");

            entity.HasOne(d => d.Indicador).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.IndicadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__indic__3F3159AB");

            entity.HasOne(d => d.Periodo).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__perio__40257DE4");
        });

        modelBuilder.Entity<ListaMateriale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_ma__3213E83F6C6F8D8C");

            entity.ToTable("lista_materiales", "produccion");

            entity.HasIndex(e => new { e.ProductoTerminadoId, e.Version }, "UQ_lista_materiales").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.ProductoTerminadoId).HasColumnName("producto_terminado_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.ProductoTerminado).WithMany(p => p.ListaMateriales)
                .HasForeignKey(d => d.ProductoTerminadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_mat__produ__6D6238AF");
        });

        modelBuilder.Entity<ListaMaterialesDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_ma__3213E83F0C82E4A5");

            entity.ToTable("lista_materiales_detalle", "produccion");

            entity.HasIndex(e => new { e.ListaMaterialesId, e.ProductoInsumoId }, "UQ_lista_materiales_detalle").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadRequerida)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_requerida");
            entity.Property(e => e.ListaMaterialesId).HasColumnName("lista_materiales_id");
            entity.Property(e => e.PorcentajeMerma)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentaje_merma");
            entity.Property(e => e.ProductoInsumoId).HasColumnName("producto_insumo_id");

            entity.HasOne(d => d.ListaMateriales).WithMany(p => p.ListaMaterialesDetalles)
                .HasForeignKey(d => d.ListaMaterialesId)
                .HasConstraintName("FK__lista_mat__lista__75035A77");

            entity.HasOne(d => d.ProductoInsumo).WithMany(p => p.ListaMaterialesDetalles)
                .HasForeignKey(d => d.ProductoInsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_mat__produ__75F77EB0");
        });

        modelBuilder.Entity<ListaPrecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_pr__3213E83F077C4993");

            entity.ToTable("lista_precio", "inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Canal)
                .HasMaxLength(15)
                .HasDefaultValue("GENERAL")
                .HasColumnName("canal");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ListaPrecios)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_pre__entid__23F3538A");
        });

        modelBuilder.Entity<ListaPrecioDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_pr__3213E83FB52E9336");

            entity.ToTable("lista_precio_detalle", "inventario");

            entity.HasIndex(e => new { e.ListaPrecioId, e.ProductoId }, "UQ_lista_precio_detalle").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.ListaPrecio).WithMany(p => p.ListaPrecioDetalles)
                .HasForeignKey(d => d.ListaPrecioId)
                .HasConstraintName("FK__lista_pre__lista__2B947552");

            entity.HasOne(d => d.Producto).WithMany(p => p.ListaPrecioDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_pre__produ__2C88998B");
        });

        modelBuilder.Entity<MantenimientoProgramado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__mantenim__3213E83F8ABEB321");

            entity.ToTable("mantenimiento_programado", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Costo)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("costo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EquipoId).HasColumnName("equipo_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("PROGRAMADO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEjecutada).HasColumnName("fecha_ejecutada");
            entity.Property(e => e.FechaProgramada).HasColumnName("fecha_programada");
            entity.Property(e => e.ResponsableId).HasColumnName("responsable_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasDefaultValue("PREVENTIVO")
                .HasColumnName("tipo");

            entity.HasOne(d => d.Equipo).WithMany(p => p.MantenimientoProgramados)
                .HasForeignKey(d => d.EquipoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__mantenimi__equip__37C5420D");

            entity.HasOne(d => d.Responsable).WithMany(p => p.MantenimientoProgramados)
                .HasForeignKey(d => d.ResponsableId)
                .HasConstraintName("FK__mantenimi__respo__3AA1AEB8");
        });

        modelBuilder.Entity<Merma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__merma__3213E83FE6F7D556");

            entity.ToTable("merma", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Causa)
                .HasMaxLength(30)
                .HasColumnName("causa");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("fecha");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.ValorContable)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("valor_contable");

            entity.HasOne(d => d.Asiento).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__merma__asiento_i__2A6B46EF");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.OrdenProduccionId)
                .HasConstraintName("FK__merma__orden_pro__278EDA44");

            entity.HasOne(d => d.Producto).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__merma__producto___2882FE7D");
        });

        modelBuilder.Entity<MovimientoBancario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83F83CA2A48");

            entity.ToTable("movimiento_bancario", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.Conciliado).HasColumnName("conciliado");
            entity.Property(e => e.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FechaConciliacion).HasColumnName("fecha_conciliacion");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .HasColumnName("referencia");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.MovimientoBancarios)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__movimient__asien__7D0E9093");

            entity.HasOne(d => d.CuentaBancaria).WithMany(p => p.MovimientoBancarios)
                .HasForeignKey(d => d.CuentaBancariaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__cuent__7A3223E8");
        });

        modelBuilder.Entity<MovimientoCajaPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FAE9CF459");

            entity.ToTable("movimiento_caja_pos", "integracion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AutorizadoPor).HasColumnName("autorizado_por");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.OcurridoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("ocurrido_en");
            entity.Property(e => e.SesionCajaPosId).HasColumnName("sesion_caja_pos_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");

            entity.HasOne(d => d.AutorizadoPorNavigation).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.AutorizadoPor)
                .HasConstraintName("FK__movimient__autor__150615B5");

            entity.HasOne(d => d.Factura).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__movimient__factu__1411F17C");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.SesionCajaPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__sesio__1229A90A");
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FB2020E7C");

            entity.ToTable("movimiento_inventario", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroDocumento }, "UQ_movimiento_inventario").IsUnique();

            entity.HasIndex(e => e.Fecha, "idx_mov_inv_fecha");

            entity.HasIndex(e => new { e.ReferenciaExternaTipo, e.ReferenciaExternaId }, "idx_mov_inv_referencia");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenDestinoId).HasColumnName("almacen_destino_id");
            entity.Property(e => e.AlmacenOrigenId).HasColumnName("almacen_origen_id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.Canal)
                .HasMaxLength(15)
                .HasDefaultValue("ERP")
                .HasColumnName("canal");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("fecha");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(30)
                .HasColumnName("numero_documento");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.ReferenciaExternaId).HasColumnName("referencia_externa_id");
            entity.Property(e => e.ReferenciaExternaTipo)
                .HasMaxLength(50)
                .HasColumnName("referencia_externa_tipo");
            entity.Property(e => e.TipoMovimientoId).HasColumnName("tipo_movimiento_id");

            entity.HasOne(d => d.AlmacenDestino).WithMany(p => p.MovimientoInventarioAlmacenDestinos)
                .HasForeignKey(d => d.AlmacenDestinoId)
                .HasConstraintName("FK__movimient__almac__408F9238");

            entity.HasOne(d => d.AlmacenOrigen).WithMany(p => p.MovimientoInventarioAlmacenOrigens)
                .HasForeignKey(d => d.AlmacenOrigenId)
                .HasConstraintName("FK__movimient__almac__3F9B6DFF");

            entity.HasOne(d => d.Asiento).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__movimient__asien__4460231C");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__movimient__cread__45544755");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.DispositivoPosId)
                .HasConstraintName("fk_mov_inv_dispositivo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__entid__3DB3258D");

            entity.HasOne(d => d.TipoMovimiento).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.TipoMovimientoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__tipo___3EA749C6");
        });

        modelBuilder.Entity<MovimientoInventarioDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83F8B395DCB");

            entity.ToTable("movimiento_inventario_detalle", "inventario");

            entity.HasIndex(e => e.ProductoId, "idx_mov_inv_det_producto");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_unitario");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Lote)
                .HasMaxLength(50)
                .HasColumnName("lote");
            entity.Property(e => e.MovimientoId).HasColumnName("movimiento_id");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Movimiento).WithMany(p => p.MovimientoInventarioDetalles)
                .HasForeignKey(d => d.MovimientoId)
                .HasConstraintName("FK__movimient__movim__4A18FC72");

            entity.HasOne(d => d.Producto).WithMany(p => p.MovimientoInventarioDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__produ__4B0D20AB");
        });

        modelBuilder.Entity<NominaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nomina_d__3213E83FC536B4CE");

            entity.ToTable("nomina_detalle", "rrhh");

            entity.HasIndex(e => new { e.PeriodoNominaId, e.EmpleadoId }, "UQ_nomina_detalle").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DiasTrabajados)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("dias_trabajados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.HorasExtra)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("horas_extra");
            entity.Property(e => e.PeriodoNominaId).HasColumnName("periodo_nomina_id");
            entity.Property(e => e.SalarioDevengado)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_devengado");
            entity.Property(e => e.SalarioNeto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_neto");
            entity.Property(e => e.TotalDeducciones)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_deducciones");

            entity.HasOne(d => d.Empleado).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__nomina_de__emple__762C88DA");

            entity.HasOne(d => d.PeriodoNomina).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.PeriodoNominaId)
                .HasConstraintName("FK__nomina_de__perio__753864A1");
        });

        modelBuilder.Entity<NominaDetalleConcepto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nomina_d__3213E83F37ED00C9");

            entity.ToTable("nomina_detalle_concepto", "rrhh");

            entity.HasIndex(e => new { e.NominaDetalleId, e.ConceptoId }, "UQ_nomina_detalle_concepto").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ConceptoId).HasColumnName("concepto_id");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.NominaDetalleId).HasColumnName("nomina_detalle_id");

            entity.HasOne(d => d.Concepto).WithMany(p => p.NominaDetalleConceptos)
                .HasForeignKey(d => d.ConceptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__nomina_de__conce__00AA174D");

            entity.HasOne(d => d.NominaDetalle).WithMany(p => p.NominaDetalleConceptos)
                .HasForeignKey(d => d.NominaDetalleId)
                .HasConstraintName("FK__nomina_de__nomin__7FB5F314");
        });

        modelBuilder.Entity<OrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_co__3213E83F20903626");

            entity.ToTable("orden_compra", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroOrden }, "UQ_orden_compra").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenDestinoId).HasColumnName("almacen_destino_id");
            entity.Property(e => e.AprobadoPor).HasColumnName("aprobado_por");
            entity.Property(e => e.ContratoId).HasColumnName("contrato_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("BORRADOR")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("fecha");
            entity.Property(e => e.FechaEntregaEsperada).HasColumnName("fecha_entrega_esperada");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.NumeroOrden)
                .HasMaxLength(30)
                .HasColumnName("numero_orden");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.AlmacenDestino).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.AlmacenDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__almac__658C0CBD");

            entity.HasOne(d => d.AprobadoPorNavigation).WithMany(p => p.OrdenCompraAprobadoPorNavigations)
                .HasForeignKey(d => d.AprobadoPor)
                .HasConstraintName("FK__orden_com__aprob__6C390A4C");

            entity.HasOne(d => d.Contrato).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.ContratoId)
                .HasConstraintName("FK__orden_com__contr__6497E884");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.OrdenCompraCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__orden_com__cread__6D2D2E85");

            entity.HasOne(d => d.Entidad).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__entid__62AFA012");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__prove__63A3C44B");
        });

        modelBuilder.Entity<OrdenCompraDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_co__3213E83FFE2DF40B");

            entity.ToTable("orden_compra_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadRecibida)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_recibida");
            entity.Property(e => e.CantidadSolicitada)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_solicitada");
            entity.Property(e => e.OrdenCompraId).HasColumnName("orden_compra_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.SubtotalLinea)
                .HasComputedColumnSql("([cantidad_solicitada]*[precio_unitario])", true)
                .HasColumnType("decimal(29, 8)")
                .HasColumnName("subtotal_linea");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.OrdenCompraDetalles)
                .HasForeignKey(d => d.OrdenCompraId)
                .HasConstraintName("FK__orden_com__orden__71F1E3A2");

            entity.HasOne(d => d.Producto).WithMany(p => p.OrdenCompraDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__produ__72E607DB");
        });

        modelBuilder.Entity<OrdenProduccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_pr__3213E83FB3199396");

            entity.ToTable("orden_produccion", "produccion");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroOrden }, "UQ_orden_produccion").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenInsumosId).HasColumnName("almacen_insumos_id");
            entity.Property(e => e.AlmacenProductoId).HasColumnName("almacen_producto_id");
            entity.Property(e => e.AsientoConsumoId).HasColumnName("asiento_consumo_id");
            entity.Property(e => e.AsientoTerminadoId).HasColumnName("asiento_terminado_id");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.CantidadProducida)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_producida");
            entity.Property(e => e.CostoRealTotal)
                .HasColumnType("decimal(16, 4)")
                .HasColumnName("costo_real_total");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("PLANIFICADA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFinPlan).HasColumnName("fecha_fin_plan");
            entity.Property(e => e.FechaFinReal).HasColumnName("fecha_fin_real");
            entity.Property(e => e.FechaInicioPlan).HasColumnName("fecha_inicio_plan");
            entity.Property(e => e.FechaInicioReal).HasColumnName("fecha_inicio_real");
            entity.Property(e => e.FichaCostoId).HasColumnName("ficha_costo_id");
            entity.Property(e => e.ListaMaterialesId).HasColumnName("lista_materiales_id");
            entity.Property(e => e.NumeroOrden)
                .HasMaxLength(30)
                .HasColumnName("numero_orden");
            entity.Property(e => e.ProductoTerminadoId).HasColumnName("producto_terminado_id");

            entity.HasOne(d => d.AlmacenInsumos).WithMany(p => p.OrdenProduccionAlmacenInsumos)
                .HasForeignKey(d => d.AlmacenInsumosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__almac__0FB750B3");

            entity.HasOne(d => d.AlmacenProducto).WithMany(p => p.OrdenProduccionAlmacenProductos)
                .HasForeignKey(d => d.AlmacenProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__almac__10AB74EC");

            entity.HasOne(d => d.AsientoConsumo).WithMany(p => p.OrdenProduccionAsientoConsumos)
                .HasForeignKey(d => d.AsientoConsumoId)
                .HasConstraintName("FK__orden_pro__asien__147C05D0");

            entity.HasOne(d => d.AsientoTerminado).WithMany(p => p.OrdenProduccionAsientoTerminados)
                .HasForeignKey(d => d.AsientoTerminadoId)
                .HasConstraintName("FK__orden_pro__asien__15702A09");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__orden_pro__cread__16644E42");

            entity.HasOne(d => d.Entidad).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__entid__0BE6BFCF");

            entity.HasOne(d => d.FichaCosto).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.FichaCostoId)
                .HasConstraintName("FK__orden_pro__ficha__0EC32C7A");

            entity.HasOne(d => d.ListaMateriales).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.ListaMaterialesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__lista__0DCF0841");

            entity.HasOne(d => d.ProductoTerminado).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.ProductoTerminadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__produ__0CDAE408");
        });

        modelBuilder.Entity<OrdenProduccionConsumo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_pr__3213E83F6303DAA5");

            entity.ToTable("orden_produccion_consumo", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.CantidadReal)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_real");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("costo_unitario");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");
            entity.Property(e => e.ProductoInsumoId).HasColumnName("producto_insumo_id");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__orden_pro__movim__1E05700A");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.OrdenProduccionId)
                .HasConstraintName("FK__orden_pro__orden__1B29035F");

            entity.HasOne(d => d.ProductoInsumo).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.ProductoInsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__produ__1C1D2798");
        });

        modelBuilder.Entity<PagoAplicado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pago_apl__3213E83FBF2A9228");

            entity.ToTable("pago_aplicado", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CuentaPorCobrarId).HasColumnName("cuenta_por_cobrar_id");
            entity.Property(e => e.CuentaPorPagarId).HasColumnName("cuenta_por_pagar_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(20)
                .HasColumnName("forma_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.ReferenciaExterna)
                .HasMaxLength(100)
                .HasColumnName("referencia_externa");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__pago_apli__asien__6CD828CA");

            entity.HasOne(d => d.CuentaPorCobrar).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.CuentaPorCobrarId)
                .HasConstraintName("FK__pago_apli__cuent__69FBBC1F");

            entity.HasOne(d => d.CuentaPorPagar).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.CuentaPorPagarId)
                .HasConstraintName("FK__pago_apli__cuent__6AEFE058");
        });

        modelBuilder.Entity<PaqueteInformacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__paquete___3213E83FFB7156C1");

            entity.ToTable("paquete_informacion", "reportes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("GENERADO")
                .HasColumnName("estado");
            entity.Property(e => e.GeneradoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("generado_en");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__paquete_i__entid__44EA3301");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__paquete_i__gener__49AEE81E");

            entity.HasOne(d => d.Periodo).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__paquete_i__perio__45DE573A");
        });

        modelBuilder.Entity<ParametroSistema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__parametr__3213E83F29E94D94");

            entity.ToTable("parametro_sistema", "nucleo");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo, e.VigenteDesde }, "UQ_parametro_sistema").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(80)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.TipoDato)
                .HasMaxLength(20)
                .HasDefaultValue("STRING")
                .HasColumnName("tipo_dato");
            entity.Property(e => e.Valor).HasColumnName("valor");
            entity.Property(e => e.VigenteDesde)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ParametroSistemas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__parametro__entid__76969D2E");
        });

        modelBuilder.Entity<PeriodoContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__periodo___3213E83F3C681EDD");

            entity.ToTable("periodo_contable", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Anio, e.Mes }, "UQ_periodo_contable").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CerradoEn).HasColumnName("cerrado_en");
            entity.Property(e => e.CerradoPor).HasColumnName("cerrado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ABIERTO")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Mes).HasColumnName("mes");

            entity.HasOne(d => d.CerradoPorNavigation).WithMany(p => p.PeriodoContables)
                .HasForeignKey(d => d.CerradoPor)
                .HasConstraintName("FK__periodo_c__cerra__1F98B2C1");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PeriodoContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__periodo_c__entid__1BC821DD");
        });

        modelBuilder.Entity<PeriodoNomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__periodo___3213E83FC15581C0");

            entity.ToTable("periodo_nomina", "rrhh");

            entity.HasIndex(e => new { e.EntidadId, e.Anio, e.Mes, e.Tipo }, "UQ_periodo_nomina").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.AprobadoPor).HasColumnName("aprobado_por");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CalculadoEn).HasColumnName("calculado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("PRENOMINA")
                .HasColumnName("estado");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasDefaultValue("MENSUAL")
                .HasColumnName("tipo");

            entity.HasOne(d => d.AprobadoPorNavigation).WithMany(p => p.PeriodoNominas)
                .HasForeignKey(d => d.AprobadoPor)
                .HasConstraintName("FK__periodo_n__aprob__7073AF84");

            entity.HasOne(d => d.Asiento).WithMany(p => p.PeriodoNominas)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__periodo_n__asien__6F7F8B4B");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PeriodoNominas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__periodo_n__entid__69C6B1F5");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__permiso__3213E83F3A32CA14");

            entity.ToTable("permiso", "nucleo");

            entity.HasIndex(e => e.Codigo, "UQ__permiso__40F9A2063F048303").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(80)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .HasColumnName("modulo");
        });

        modelBuilder.Entity<PlanProduccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__plan_pro__3213E83F874A8F9B");

            entity.ToTable("plan_produccion", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("BORRADOR")
                .HasColumnName("estado");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.PresupuestoId).HasColumnName("presupuesto_id");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PlanProduccions)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plan_prod__entid__7BB05806");

            entity.HasOne(d => d.Presupuesto).WithMany(p => p.PlanProduccions)
                .HasForeignKey(d => d.PresupuestoId)
                .HasConstraintName("FK__plan_prod__presu__7D98A078");
        });

        modelBuilder.Entity<PlanProduccionDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__plan_pro__3213E83FE30E4CB6");

            entity.ToTable("plan_produccion_detalle", "produccion");

            entity.HasIndex(e => new { e.PlanId, e.ProductoId }, "UQ_plan_produccion_detalle").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadEjecutada)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_ejecutada");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("decimal(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanProduccionDetalles)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__plan_prod__plan___0539C240");

            entity.HasOne(d => d.Producto).WithMany(p => p.PlanProduccionDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plan_prod__produ__062DE679");
        });

        modelBuilder.Entity<PlantillaAprobadum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__plantill__3213E83FDE06C978");

            entity.ToTable("plantilla_aprobada", "rrhh");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CargoId).HasColumnName("cargo_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.PlazasAprobadas)
                .HasDefaultValue(1)
                .HasColumnName("plazas_aprobadas");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Cargo).WithMany(p => p.PlantillaAprobada)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plantilla__cargo__308E3499");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PlantillaAprobada)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plantilla__entid__2EA5EC27");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.PlantillaAprobada)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__plantilla__sucur__2F9A1060");
        });

        modelBuilder.Entity<PosRangoNumeracion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_rang__3213E83F656D7646");

            entity.ToTable("pos_rango_numeracion", "integracion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Agotado).HasColumnName("agotado");
            entity.Property(e => e.AsignadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("asignado_en");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.NumeroDesde).HasColumnName("numero_desde");
            entity.Property(e => e.NumeroHasta).HasColumnName("numero_hasta");
            entity.Property(e => e.NumeroSiguienteLocal).HasColumnName("numero_siguiente_local");
            entity.Property(e => e.Serie)
                .HasMaxLength(10)
                .HasColumnName("serie");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(40)
                .HasDefaultValue("FACTURA_VENTA")
                .HasColumnName("tipo_documento");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.PosRangoNumeracions)
                .HasForeignKey(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_rango__dispo__24485945");
        });

        modelBuilder.Entity<PosSyncLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_sync__3213E83FBAE39DD9");

            entity.ToTable("pos_sync_log", "integracion");

            entity.HasIndex(e => new { e.DispositivoPosId, e.IniciadoEn }, "idx_pos_sync_dispositivo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DetalleError).HasColumnName("detalle_error");
            entity.Property(e => e.Direccion)
                .HasMaxLength(10)
                .HasColumnName("direccion");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("EN_PROGRESO")
                .HasColumnName("estado");
            entity.Property(e => e.FinalizadoEn).HasColumnName("finalizado_en");
            entity.Property(e => e.IniciadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("iniciado_en");
            entity.Property(e => e.RegistrosProcesados).HasColumnName("registros_procesados");
            entity.Property(e => e.TipoSync)
                .HasMaxLength(20)
                .HasColumnName("tipo_sync");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.PosSyncLogs)
                .HasForeignKey(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_sync___dispo__2BE97B0D");
        });

        modelBuilder.Entity<PosVentaPendiente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_vent__3213E83FDD59AF56");

            entity.ToTable("pos_venta_pendiente", "integracion");

            entity.HasIndex(e => new { e.DispositivoPosId, e.IdempotencyKey }, "UQ_pos_venta_pendiente").IsUnique();

            entity.HasIndex(e => e.Estado, "idx_pos_venta_pendiente_estado").HasFilter("([estado]='PENDIENTE')");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("PENDIENTE")
                .HasColumnName("estado");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.FechaRecibidoServidor)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("fecha_recibido_servidor");
            entity.Property(e => e.FechaVentaLocal).HasColumnName("fecha_venta_local");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(80)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.IntentosProcesamiento).HasColumnName("intentos_procesamiento");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.PayloadJson).HasColumnName("payload_json");
            entity.Property(e => e.ProcesadoEn).HasColumnName("procesado_en");
            entity.Property(e => e.SesionCajaPosId).HasColumnName("sesion_caja_pos_id");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.PosVentaPendientes)
                .HasForeignKey(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_venta__dispo__1ABEEF0B");

            entity.HasOne(d => d.Factura).WithMany(p => p.PosVentaPendientes)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__pos_venta__factu__1F83A428");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.PosVentaPendientes)
                .HasForeignKey(d => d.SesionCajaPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_venta__sesio__1BB31344");
        });

        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__presupue__3213E83FC336428E");

            entity.ToTable("presupuesto", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.AprobadoPor).HasColumnName("aprobado_por");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("BORRADOR")
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");

            entity.HasOne(d => d.AprobadoPorNavigation).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.AprobadoPor)
                .HasConstraintName("FK__presupues__aprob__1B9317B3");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__presupues__entid__18B6AB08");
        });

        modelBuilder.Entity<PresupuestoLinea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__presupue__3213E83F6B813DC4");

            entity.ToTable("presupuesto_linea", "contabilidad");

            entity.HasIndex(e => new { e.PresupuestoId, e.CuentaId, e.CentroCostoId, e.Mes }, "UQ_presupuesto_linea").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CentroCostoId).HasColumnName("centro_costo_id");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.MontoPlanificado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_planificado");
            entity.Property(e => e.PresupuestoId).HasColumnName("presupuesto_id");

            entity.HasOne(d => d.CentroCosto).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.CentroCostoId)
                .HasConstraintName("FK__presupues__centr__2334397B");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__presupues__cuent__22401542");

            entity.HasOne(d => d.Presupuesto).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.PresupuestoId)
                .HasConstraintName("FK__presupues__presu__214BF109");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__producto__3213E83F350C0211");

            entity.ToTable("producto", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.CodigoBarras }, "UQ_producto_barras").IsUnique();

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_producto_codigo").IsUnique();

            entity.HasIndex(e => e.CodigoBarras, "idx_producto_codigo_barras");

            entity.HasIndex(e => e.Nombre, "idx_producto_nombre");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.AplicaImpuestoVentas)
                .HasDefaultValue(true)
                .HasColumnName("aplica_impuesto_ventas");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(50)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaCostoVentaId).HasColumnName("cuenta_costo_venta_id");
            entity.Property(e => e.CuentaIngresoId).HasColumnName("cuenta_ingreso_id");
            entity.Property(e => e.CuentaInventarioId).HasColumnName("cuenta_inventario_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.FamiliaId).HasColumnName("familia_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioVentaActual)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio_venta_actual");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasDefaultValue("TERMINADO")
                .HasColumnName("tipo");
            entity.Property(e => e.UnidadMedidaId).HasColumnName("unidad_medida_id");

            entity.HasOne(d => d.CuentaCostoVenta).WithMany(p => p.ProductoCuentaCostoVenta)
                .HasForeignKey(d => d.CuentaCostoVentaId)
                .HasConstraintName("FK__producto__cuenta__1B5E0D89");

            entity.HasOne(d => d.CuentaIngreso).WithMany(p => p.ProductoCuentaIngresos)
                .HasForeignKey(d => d.CuentaIngresoId)
                .HasConstraintName("FK__producto__cuenta__1C5231C2");

            entity.HasOne(d => d.CuentaInventario).WithMany(p => p.ProductoCuentaInventarios)
                .HasForeignKey(d => d.CuentaInventarioId)
                .HasConstraintName("FK__producto__cuenta__1A69E950");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Productos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__producto__entida__15A53433");

            entity.HasOne(d => d.Familia).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FamiliaId)
                .HasConstraintName("FK__producto__famili__1699586C");

            entity.HasOne(d => d.UnidadMedida).WithMany(p => p.Productos)
                .HasForeignKey(d => d.UnidadMedidaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__producto__unidad__178D7CA5");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__proveedo__3213E83FFADEB65B");

            entity.ToTable("proveedor", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.Nit }, "UQ_proveedor_nit").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaBancaria)
                .HasMaxLength(40)
                .HasColumnName("cuenta_bancaria");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nit)
                .HasMaxLength(20)
                .HasColumnName("nit");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .HasColumnName("razon_social");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoPersona)
                .HasMaxLength(15)
                .HasDefaultValue("JURIDICA")
                .HasColumnName("tipo_persona");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.CuentaContableId)
                .HasConstraintName("FK__proveedor__cuent__442B18F2");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__proveedor__entid__414EAC47");
        });

        modelBuilder.Entity<RecepcionCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recepcio__3213E83F191166B9");

            entity.ToTable("recepcion_compra", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CuentaPorPagarId).HasColumnName("cuenta_por_pagar_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("fecha");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.NumeroInformeRecepcion)
                .HasMaxLength(30)
                .HasColumnName("numero_informe_recepcion");
            entity.Property(e => e.OrdenCompraId).HasColumnName("orden_compra_id");
            entity.Property(e => e.RecibidoPor).HasColumnName("recibido_por");

            entity.HasOne(d => d.CuentaPorPagar).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.CuentaPorPagarId)
                .HasConstraintName("FK__recepcion__cuent__7993056A");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__recepcion__movim__789EE131");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.OrdenCompraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recepcion__orden__77AABCF8");

            entity.HasOne(d => d.RecibidoPorNavigation).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.RecibidoPor)
                .HasConstraintName("FK__recepcion__recib__7B7B4DDC");
        });

        modelBuilder.Entity<RegistroAsistencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registro__3213E83F989785F7");

            entity.ToTable("registro_asistencia", "rrhh");

            entity.HasIndex(e => new { e.EmpleadoId, e.Fecha }, "UQ_registro_asistencia").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.HoraEntrada).HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida).HasColumnName("hora_salida");
            entity.Property(e => e.HorasExtra)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("horas_extra");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.RegistradoPor).HasColumnName("registrado_por");
            entity.Property(e => e.TipoAusenciaId).HasColumnName("tipo_ausencia_id");

            entity.HasOne(d => d.Empleado).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__registro___emple__5006DFF2");

            entity.HasOne(d => d.RegistradoPorNavigation).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.RegistradoPor)
                .HasConstraintName("FK__registro___regis__52E34C9D");

            entity.HasOne(d => d.TipoAusencia).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.TipoAusenciaId)
                .HasConstraintName("FK__registro___tipo___51EF2864");
        });

        modelBuilder.Entity<RegistroSalarioTiempoServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registro__3213E83F83F4617A");

            entity.ToTable("registro_salario_tiempo_servicio", "rrhh");

            entity.HasIndex(e => new { e.EmpleadoId, e.Anio, e.Mes }, "UQ_registro_salario_tiempo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.DiasTrabajados)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("dias_trabajados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.SalarioDevengado)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("salario_devengado");
            entity.Property(e => e.TiempoServicioAcumuladoMeses).HasColumnName("tiempo_servicio_acumulado_meses");

            entity.HasOne(d => d.Empleado).WithMany(p => p.RegistroSalarioTiempoServicios)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__registro___emple__056ECC6A");
        });

        modelBuilder.Entity<ReporteGenerado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reporte___3213E83FE786375B");

            entity.ToTable("reporte_generado", "reportes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Formato)
                .HasMaxLength(10)
                .HasColumnName("formato");
            entity.Property(e => e.GeneradoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("generado_en");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.NombreReporte)
                .HasMaxLength(150)
                .HasColumnName("nombre_reporte");
            entity.Property(e => e.ParametrosJson).HasColumnName("parametros_json");
            entity.Property(e => e.RutaArchivo).HasColumnName("ruta_archivo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ReporteGenerados)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reporte_g__entid__4E739D3B");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.ReporteGenerados)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__reporte_g__gener__505BE5AD");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__rol__3213E83FE147D5D7");

            entity.ToTable("rol", "nucleo");

            entity.HasIndex(e => e.Codigo, "UQ__rol__40F9A20672A7D80D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EsSistema).HasColumnName("es_sistema");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasMany(d => d.Permisos).WithMany(p => p.Rols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("PermisoId")
                        .HasConstraintName("FK__rol_permi__permi__52593CB8"),
                    l => l.HasOne<Rol>().WithMany()
                        .HasForeignKey("RolId")
                        .HasConstraintName("FK__rol_permi__rol_i__5165187F"),
                    j =>
                    {
                        j.HasKey("RolId", "PermisoId").HasName("PK__rol_perm__0939B2DF5D2C71F9");
                        j.ToTable("rol_permiso", "nucleo");
                        j.IndexerProperty<int>("RolId").HasColumnName("rol_id");
                        j.IndexerProperty<int>("PermisoId").HasColumnName("permiso_id");
                    });
        });

        modelBuilder.Entity<SaldoVacacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__saldo_va__3213E83F808B56A3");

            entity.ToTable("saldo_vacaciones", "rrhh");

            entity.HasIndex(e => new { e.EmpleadoId, e.Anio }, "UQ_saldo_vacaciones").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.DiasAcumulados)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("dias_acumulados");
            entity.Property(e => e.DiasCompensados)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("dias_compensados");
            entity.Property(e => e.DiasDisfrutados)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("dias_disfrutados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.SaldoActual)
                .HasComputedColumnSql("(([dias_acumulados]-[dias_disfrutados])-[dias_compensados])", true)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("saldo_actual");

            entity.HasOne(d => d.Empleado).WithMany(p => p.SaldoVacaciones)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__saldo_vac__emple__57A801BA");
        });

        modelBuilder.Entity<SesionCajaPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sesion_c__3213E83FA658AC5E");

            entity.ToTable("sesion_caja_pos", "integracion");

            entity.HasIndex(e => new { e.DispositivoPosId, e.Estado }, "idx_sesion_caja_dispositivo");

            entity.HasIndex(e => e.DispositivoPosId, "uq_sesion_caja_abierta")
                .IsUnique()
                .HasFilter("([estado]='ABIERTA')");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoCierreId).HasColumnName("asiento_cierre_id");
            entity.Property(e => e.CajeroId).HasColumnName("cajero_id");
            entity.Property(e => e.CantidadFacturas).HasColumnName("cantidad_facturas");
            entity.Property(e => e.DiferenciaArqueo)
                .HasComputedColumnSql("([monto_cierre_declarado]-[monto_cierre_sistema])", true)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("diferencia_arqueo");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ABIERTA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaApertura)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("fecha_apertura");
            entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
            entity.Property(e => e.MontoApertura)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("monto_apertura");
            entity.Property(e => e.MontoCierreDeclarado)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("monto_cierre_declarado");
            entity.Property(e => e.MontoCierreSistema)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("monto_cierre_sistema");
            entity.Property(e => e.ObservacionesCierre).HasColumnName("observaciones_cierre");
            entity.Property(e => e.SupervisorConciliacionId).HasColumnName("supervisor_conciliacion_id");
            entity.Property(e => e.TotalEfectivo)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_efectivo");
            entity.Property(e => e.TotalEnzona)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_enzona");
            entity.Property(e => e.TotalOtrosMedios)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_otros_medios");
            entity.Property(e => e.TotalTransfermovil)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_transfermovil");
            entity.Property(e => e.TotalVentas)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total_ventas");

            entity.HasOne(d => d.AsientoCierre).WithMany(p => p.SesionCajaPos)
                .HasForeignKey(d => d.AsientoCierreId)
                .HasConstraintName("FK__sesion_ca__asien__0C70CFB4");

            entity.HasOne(d => d.Cajero).WithMany(p => p.SesionCajaPoCajeros)
                .HasForeignKey(d => d.CajeroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sesion_ca__cajer__01F34141");

            entity.HasOne(d => d.DispositivoPos).WithOne(p => p.SesionCajaPo)
                .HasForeignKey<SesionCajaPo>(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sesion_ca__dispo__00FF1D08");

            entity.HasOne(d => d.SupervisorConciliacion).WithMany(p => p.SesionCajaPoSupervisorConciliacions)
                .HasForeignKey(d => d.SupervisorConciliacionId)
                .HasConstraintName("FK__sesion_ca__super__0D64F3ED");
        });

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sucursal__3213E83FC558B982");

            entity.ToTable("sucursal", "nucleo");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_sucursal_entidad_codigo").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Municipio)
                .HasMaxLength(100)
                .HasColumnName("municipio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Provincia)
                .HasMaxLength(100)
                .HasColumnName("provincia");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasDefaultValue("ALMACEN")
                .HasColumnName("tipo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Sucursals)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sucursal__entida__4316F928");
        });

        modelBuilder.Entity<TipoAusencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipo_aus__3213E83F37A356FA");

            entity.ToTable("tipo_ausencia", "rrhh");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_aus__40F9A206EE5596F0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AfectaVacaciones).HasColumnName("afecta_vacaciones");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Remunerada)
                .HasDefaultValue(true)
                .HasColumnName("remunerada");
        });

        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipo_com__3213E83FC13BC4C5");

            entity.ToTable("tipo_comprobante", "contabilidad");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_com__40F9A206B647C75F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipo_mov__3213E83F6615450E");

            entity.ToTable("tipo_movimiento", "inventario");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_mov__40F9A206704C72CD").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AfectaCosto)
                .HasDefaultValue(true)
                .HasColumnName("afecta_costo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.Naturaleza)
                .HasMaxLength(10)
                .HasColumnName("naturaleza");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoObligacionFiscal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipo_obl__3213E83FC6B25740");

            entity.ToTable("tipo_obligacion_fiscal", "contabilidad");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_obl__40F9A20641A676C6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BaseLegal)
                .HasMaxLength(150)
                .HasColumnName("base_legal");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Periodicidad)
                .HasMaxLength(15)
                .HasColumnName("periodicidad");
            entity.Property(e => e.TasaActual)
                .HasColumnType("decimal(6, 3)")
                .HasColumnName("tasa_actual");
        });

        modelBuilder.Entity<TopePrecioMfp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tope_pre__3213E83FA6CE5D90");

            entity.ToTable("tope_precio_mfp", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.FamiliaId).HasColumnName("familia_id");
            entity.Property(e => e.PrecioMaximo)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio_maximo");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.ResolucionReferencia)
                .HasMaxLength(150)
                .HasColumnName("resolucion_referencia");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Familia).WithMany(p => p.TopePrecioMfps)
                .HasForeignKey(d => d.FamiliaId)
                .HasConstraintName("FK__tope_prec__famil__34B3CB38");

            entity.HasOne(d => d.Producto).WithMany(p => p.TopePrecioMfps)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK__tope_prec__produ__33BFA6FF");
        });

        modelBuilder.Entity<UnidadMedidum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__unidad_m__3213E83F711ED4D7");

            entity.ToTable("unidad_medida", "inventario");

            entity.HasIndex(e => e.Codigo, "UQ__unidad_m__40F9A20684B1DD7A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.EsFraccionable).HasColumnName("es_fraccionable");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__usuario__3213E83FAE2AF5A3");

            entity.ToTable("usuario", "nucleo");

            entity.HasIndex(e => e.Email, "UQ__usuario__AB6E6164CDC74D1F").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__usuario__D4D22D742FD2AFED").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.BloqueadoHasta).HasColumnName("bloqueado_hasta");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DebeCambiarPass)
                .HasDefaultValue(true)
                .HasColumnName("debe_cambiar_pass");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.EsEmpleadoId).HasColumnName("es_empleado_id");
            entity.Property(e => e.HashPassword)
                .HasMaxLength(255)
                .HasColumnName("hash_password");
            entity.Property(e => e.IntentosFallidos).HasColumnName("intentos_fallidos");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(150)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.UltimoLogin).HasColumnName("ultimo_login");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__usuario__entidad__5812160E");

            entity.HasOne(d => d.EsEmpleado).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EsEmpleadoId)
                .HasConstraintName("fk_usuario_empleado");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__usuario__sucursa__59063A47");
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__usuario___3213E83FE04B4546");

            entity.ToTable("usuario_rol", "nucleo");

            entity.HasIndex(e => new { e.UsuarioId, e.RolId, e.SucursalId }, "UQ_usuario_rol").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AsignadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("asignado_en");
            entity.Property(e => e.AsignadoPor).HasColumnName("asignado_por");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.AsignadoPorNavigation).WithMany(p => p.UsuarioRolAsignadoPorNavigations)
                .HasForeignKey(d => d.AsignadoPor)
                .HasConstraintName("FK__usuario_r__asign__656C112C");

            entity.HasOne(d => d.Rol).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__usuario_r__rol_i__628FA481");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__usuario_r__sucur__6383C8BA");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioRolUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__usuario_r__usuar__619B8048");
        });

        modelBuilder.Entity<VAuditoriaAcceso>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_auditoria_accesos", "reportes");

            entity.Property(e => e.Accion)
                .HasMaxLength(20)
                .HasColumnName("accion");
            entity.Property(e => e.Canal)
                .HasMaxLength(20)
                .HasColumnName("canal");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.OcurridoEn).HasColumnName("ocurrido_en");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
        });

        modelBuilder.Entity<VAuditoriaReversione>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_auditoria_reversiones", "reportes");

            entity.Property(e => e.Canal)
                .HasMaxLength(20)
                .HasColumnName("canal");
            entity.Property(e => e.EsquemaTabla)
                .HasMaxLength(100)
                .HasColumnName("esquema_tabla");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.OcurridoEn).HasColumnName("ocurrido_en");
            entity.Property(e => e.RegistroId)
                .HasMaxLength(100)
                .HasColumnName("registro_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ValoresAnteriores).HasColumnName("valores_anteriores");
            entity.Property(e => e.ValoresNuevos).HasColumnName("valores_nuevos");
        });

        modelBuilder.Entity<VEjecucionPresupuesto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ejecucion_presupuesto", "contabilidad");

            entity.Property(e => e.CentroCostoId).HasColumnName("centro_costo_id");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.MontoPlanificado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_planificado");
            entity.Property(e => e.MontoReal)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("monto_real");
            entity.Property(e => e.PresupuestoId).HasColumnName("presupuesto_id");
        });

        modelBuilder.Entity<VSaldoCuentum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_saldo_cuenta", "contabilidad");

            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.Clase)
                .HasMaxLength(20)
                .HasColumnName("clase");
            entity.Property(e => e.CodigoCuenta)
                .HasMaxLength(20)
                .HasColumnName("codigo_cuenta");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.Naturaleza)
                .HasMaxLength(10)
                .HasColumnName("naturaleza");
            entity.Property(e => e.NombreCuenta)
                .HasMaxLength(200)
                .HasColumnName("nombre_cuenta");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.Saldo)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("saldo");
            entity.Property(e => e.TotalDebe)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("total_debe");
            entity.Property(e => e.TotalHaber)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("total_haber");
        });

        modelBuilder.Entity<WebhookEntrega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__webhook___3213E83F18801E82");

            entity.ToTable("webhook_entrega", "integracion");

            entity.HasIndex(e => e.ProximoReintentoEn, "idx_webhook_entrega_pendiente").HasFilter("([exitoso]=(0))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CodigoRespuestaHttp).HasColumnName("codigo_respuesta_http");
            entity.Property(e => e.EnviadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("enviado_en");
            entity.Property(e => e.Exitoso).HasColumnName("exitoso");
            entity.Property(e => e.IntentoNumero)
                .HasDefaultValue((short)1)
                .HasColumnName("intento_numero");
            entity.Property(e => e.PayloadJson).HasColumnName("payload_json");
            entity.Property(e => e.ProximoReintentoEn).HasColumnName("proximo_reintento_en");
            entity.Property(e => e.SuscripcionId).HasColumnName("suscripcion_id");

            entity.HasOne(d => d.Suscripcion).WithMany(p => p.WebhookEntregas)
                .HasForeignKey(d => d.SuscripcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__webhook_e__suscr__3B2BBE9D");
        });

        modelBuilder.Entity<WebhookSuscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__webhook___3213E83FD8BC2BCB");

            entity.ToTable("webhook_suscripcion", "integracion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Evento)
                .HasMaxLength(60)
                .HasColumnName("evento");
            entity.Property(e => e.SecretoFirmaHash)
                .HasMaxLength(255)
                .HasColumnName("secreto_firma_hash");
            entity.Property(e => e.UrlDestino).HasColumnName("url_destino");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.WebhookSuscripcions)
                .HasForeignKey(d => d.ApiClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__webhook_s__api_c__3572E547");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
