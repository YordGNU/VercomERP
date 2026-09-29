using Microsoft.EntityFrameworkCore;

namespace Vercom.Models;

public partial class AppDbContext : DbContext
{
    private readonly Security.IEntidadProvider? _entidadProvider;

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, Security.IEntidadProvider entidadProvider)
        : base(options)
    {
        _entidadProvider = entidadProvider;
    }

    // Propiedades para filtros globales
    public Guid CurrentEntidadId => _entidadProvider?.CurrentEntidadId ?? Guid.Empty;
    public Guid? CurrentSucursalId => _entidadProvider?.CurrentSucursalId;
    public bool IsMaster => _entidadProvider?.IsMaster ?? false;

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

    public virtual DbSet<ExistenciaLote> ExistenciaLotes { get; set; }

    public virtual DbSet<FacturaVentaDetalle> FacturaVentaDetalles { get; set; }

    public virtual DbSet<FacturaVentum> FacturaVenta { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Notificacion> Notificaciones { get; set; }

    public virtual DbSet<FamiliaProducto> FamiliaProductos { get; set; }

    public virtual DbSet<FichaCosto> FichaCostos { get; set; }

    public virtual DbSet<FormaPagoVentum> FormaPagoVenta { get; set; }

    public virtual DbSet<Indicador> Indicadors { get; set; }

    public virtual DbSet<IndicadorValor> IndicadorValors { get; set; }

    public virtual DbSet<TurnoTrabajo> TurnoTrabajos { get; set; }

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

    public virtual DbSet<UtileResponsabilidad> UtileResponsabilidads { get; set; }

    public virtual DbSet<VAuditoriaAcceso> VAuditoriaAccesos { get; set; }

    public virtual DbSet<VAuditoriaReversione> VAuditoriaReversiones { get; set; }

    public virtual DbSet<VEjecucionPresupuesto> VEjecucionPresupuestos { get; set; }

    public virtual DbSet<VSaldoCuentum> VSaldoCuenta { get; set; }

    public virtual DbSet<WebhookEntrega> WebhookEntregas { get; set; }

    public virtual DbSet<WebhookSuscripcion> WebhookSuscripcions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback para herramientas de diseño o fallos de inyección
            optionsBuilder.UseSqlServer("Server=localhost;Database=VercomERP;User Id=sa;Password=sql2025*;TrustServerCertificate=True;MultipleActiveResultSets=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivoFijo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activo_f__3213E83F600B9AB6");

            entity.ToTable("activo_fijo", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.CodigoInventario }, "UQ__activo_f__5F57D4D48CFFE95B").IsUnique();

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
                .HasColumnType("numeric(18, 2)")
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
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("tasa_depreciacion_anual");
            entity.Property(e => e.ValorAdquisicion)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("valor_adquisicion");
            entity.Property(e => e.ValorResidual)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("valor_residual");
            entity.Property(e => e.VidaUtilMeses).HasColumnName("vida_util_meses");

            entity.HasOne(d => d.CuentaActivo).WithMany(p => p.ActivoFijoCuentaActivos)
                .HasForeignKey(d => d.CuentaActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__45BE5BA9");

            entity.HasOne(d => d.CuentaDepreciacion).WithMany(p => p.ActivoFijoCuentaDepreciacions)
                .HasForeignKey(d => d.CuentaDepreciacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__46B27FE2");

            entity.HasOne(d => d.CuentaGastoDep).WithMany(p => p.ActivoFijoCuentaGastoDeps)
                .HasForeignKey(d => d.CuentaGastoDepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__cuent__47A6A41B");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ActivoFijos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__entid__43D61337");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.ActivoFijos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__activo_fi__sucur__44CA3770");
        });

        modelBuilder.Entity<ActivoFijoDepreciacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activo_f__3213E83F75D87A1C");

            entity.ToTable("activo_fijo_depreciacion", "contabilidad");

            entity.HasIndex(e => new { e.ActivoFijoId, e.PeriodoId }, "UQ__activo_f__4DEF5209060FE480").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActivoFijoId).HasColumnName("activo_fijo_id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CalculadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("calculado_en");
            entity.Property(e => e.Monto)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");

            entity.HasOne(d => d.ActivoFijo).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.ActivoFijoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__activ__5224328E");

            entity.HasOne(d => d.Asiento).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__activo_fi__asien__540C7B00");

            entity.HasOne(d => d.Periodo).WithMany(p => p.ActivoFijoDepreciacions)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activo_fi__perio__531856C7");
        });

        modelBuilder.Entity<Almacen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__almacen__3213E83FA0632939");

            entity.ToTable("almacen", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__almacen__499C975593FF1F0D").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__analisis__3213E83FD3EEF74B");

            entity.ToTable("analisis_desviacion", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AnalizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("analizado_en");
            entity.Property(e => e.Componente)
                .HasMaxLength(20)
                .HasColumnName("componente");
            entity.Property(e => e.CostoEstandar)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("costo_estandar");
            entity.Property(e => e.CostoReal)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("costo_real");
            entity.Property(e => e.Desviacion)
                .HasComputedColumnSql("(CONVERT([numeric](16,4),[costo_real]-[costo_estandar]))", true)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("desviacion");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.AnalisisDesviacions)
                .HasForeignKey(d => d.OrdenProduccionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__analisis___orden__2B5F6B28");
        });

        modelBuilder.Entity<ApiCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_clie__3213E83FBC5895D4");

            entity.ToTable("api_cliente", "integracion");

            entity.HasIndex(e => e.ClientId, "UQ__api_clie__BF21A42586832AA8").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.RevocadoEn).HasColumnName("revocado_en");
            entity.Property(e => e.Scopes)
                .HasDefaultValue("[]")
                .HasColumnName("scopes");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasDefaultValue("POS")
                .HasColumnName("tipo");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.ApiClientes)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__api_clien__cread__673F4B05");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ApiClientes)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__api_clien__entid__618671AF");
        });

        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_log__3213E83F225EAC4B");

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("ocurrido_en");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.ApiLogs)
                .HasForeignKey(d => d.ApiClienteId)
                .HasConstraintName("FK__api_log__api_cli__7869D707");
        });

        modelBuilder.Entity<ApiRateLimit>(entity =>
        {
            entity.HasKey(e => e.ApiClienteId).HasName("PK__api_rate__089F45228E8DF79B");

            entity.ToTable("api_rate_limit", "integracion");

            entity.Property(e => e.ApiClienteId)
                .ValueGeneratedNever()
                .HasColumnName("api_cliente_id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.SolicitudesPorMinuto)
                .HasDefaultValue(120)
                .HasColumnName("solicitudes_por_minuto");

            entity.HasOne(d => d.ApiCliente).WithOne(p => p.ApiRateLimit)
                .HasForeignKey<ApiRateLimit>(d => d.ApiClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__api_rate___api_c__73A521EA");
        });

        modelBuilder.Entity<ApiToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__api_toke__3213E83F027D0F30");

            entity.ToTable("api_token", "integracion");

            entity.HasIndex(e => e.TokenHash, "UQ__api_toke__9F6BDB1318B88AEC").IsUnique();

            entity.HasIndex(e => e.ApiClienteId, "idx_api_token_cliente");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.EmitidoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__api_token__api_c__6CF8245B");
        });

        modelBuilder.Entity<AsientoContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asiento___3213E83FCD0DFDEB");

            entity.ToTable("asiento_contable", "contabilidad", tb =>
            {
                tb.HasTrigger("trg_auditar_asiento_contable");
                tb.HasTrigger("trg_bloquear_edicion_asiento");
                tb.HasTrigger("trg_validar_periodo_abierto");
            });

            entity.HasIndex(e => new { e.EntidadId, e.TipoComprobanteId, e.NumeroComprobante }, "UQ__asiento___FCE92A3DCDC841A0").IsUnique();

            entity.HasIndex(e => new { e.ModuloOrigen, e.DocumentoOrigenId }, "idx_asiento_origen");

            entity.HasIndex(e => e.PeriodoId, "idx_asiento_periodo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoReversionId)
                .HasComment("RF-12: un asiento contabilizado jamás se edita ni elimina; solo se revierte mediante un nuevo asiento de ajuste enlazado aquí.")
                .HasColumnName("asiento_reversion_id");
            entity.Property(e => e.Concepto).HasColumnName("concepto");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenTipo)
                .HasMaxLength(50)
                .HasColumnName("documento_origen_tipo");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE")
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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("total_debe");
            entity.Property(e => e.TotalHaber)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("total_haber");

            entity.HasOne(d => d.AsientoReversion).WithMany(p => p.InverseAsientoReversion)
                .HasForeignKey(d => d.AsientoReversionId)
                .HasConstraintName("FK__asiento_c__asien__2FCF1A8A");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__cread__30C33EC3");

            entity.HasOne(d => d.Entidad).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__entid__282DF8C2");

            entity.HasOne(d => d.Periodo).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__perio__2A164134");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__asiento_c__sucur__29221CFB");

            entity.HasOne(d => d.TipoComprobante).WithMany(p => p.AsientoContables)
                .HasForeignKey(d => d.TipoComprobanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_c__tipo___2B0A656D");
        });

        modelBuilder.Entity<AsientoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asiento___3213E83F53F71EA2");

            entity.ToTable("asiento_detalle", "contabilidad", tb => tb.HasTrigger("trg_bloquear_edicion_asiento_detalle"));

            entity.HasIndex(e => new { e.AsientoId, e.Linea }, "UQ__asiento___7D61AC4032C28A1E").IsUnique();

            entity.HasIndex(e => e.CuentaId, "idx_asiento_detalle_cuenta");

            entity.HasIndex(e => new { e.TerceroTipo, e.TerceroId }, "idx_asiento_detalle_tercero");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CentroCostoId).HasColumnName("centro_costo_id");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Debe)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("debe");
            entity.Property(e => e.Glosa)
                .HasMaxLength(255)
                .HasColumnName("glosa");
            entity.Property(e => e.Haber)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("haber");
            entity.Property(e => e.Linea).HasColumnName("linea");
            entity.Property(e => e.TerceroId).HasColumnName("tercero_id");
            entity.Property(e => e.TerceroTipo)
                .HasMaxLength(20)
                .HasColumnName("tercero_tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__asiento_d__asien__37703C52");

            entity.HasOne(d => d.CentroCosto).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.CentroCostoId)
                .HasConstraintName("FK__asiento_d__centr__395884C4");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.AsientoDetalles)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__asiento_d__cuent__3864608B");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__auditori__3213E83F7128C16E");

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__auditoria__usuar__6754599E");
        });

        modelBuilder.Entity<BackupLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__backup_l__3213E83F9557AA89");

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
            entity.HasKey(e => e.Id).HasName("PK__caja__3213E83F2408BC4B");

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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("limite_efectivo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.SaldoActual)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("saldo_actual");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.CuentaContableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__cuenta_con__03BB8E22");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__entidad_id__01D345B0");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__caja__sucursal_i__02C769E9");
        });

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cargo__3213E83F597E43A1");

            entity.ToTable("cargo", "rrhh");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__cargo__499C9755D9601510").IsUnique();

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
            entity.Property(e => e.CategoriaOcupacional)
                .HasMaxLength(50)
                .HasColumnName("categoria_ocupacional");
            entity.Property(e => e.Funciones)
                .HasColumnName("funciones");
            entity.Property(e => e.SalarioEscalaMax)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_escala_max");
            entity.Property(e => e.SalarioEscalaMin)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_escala_min");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Cargos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cargo__entidad_i__2BC97F7C");
        });

        modelBuilder.Entity<CentroCosto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__centro_c__3213E83FABBA15EB");

            entity.ToTable("centro_costo", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__centro_c__499C97557804F90E").IsUnique();

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
                .HasConstraintName("FK__centro_co__entid__160F4887");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.CentroCostos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__centro_co__sucur__17036CC0");
        });

        modelBuilder.Entity<CertificadoMedico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83F2CFE0D95");

            entity.ToTable("certificado_medico", "rrhh", tb => tb.HasComment("RNF-20: acceso restringido — datos de salud del trabajador, solo RR.HH. y dirección."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("porcentaje_subsidio");

            entity.HasOne(d => d.Empleado).WithMany(p => p.CertificadoMedicos)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__certifica__emple__5F492382");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cliente__3213E83FAA15231A");

            entity.ToTable("cliente", "comercial", tb => tb.HasComment("El \"cliente mostrador\" del POS (venta anónima) se modela como registro fijo con nit_o_ci=NULL, nombre_razon_social='CONSUMIDOR FINAL'."));

            entity.HasIndex(e => new { e.EntidadId, e.NitOCi }, "uq_cliente_nit_o_ci")
                .IsUnique()
                .HasFilter("([nit_o_ci] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.LimiteCredito)
                .HasColumnType("numeric(14, 2)")
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
                .HasConstraintName("FK__cliente__cuenta___592635D8");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cliente__entidad__52793849");

            entity.HasOne(d => d.ListaPrecio).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.ListaPrecioId)
                .HasConstraintName("FK__cliente__lista_p__573DED66");
        });

        modelBuilder.Entity<ConceptoNomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__concepto__3213E83FDC0F6066");

            entity.ToTable("concepto_nomina", "rrhh");

            entity.HasIndex(e => e.Codigo, "UQ__concepto__40F9A206E4F05171").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CuentaContableId).HasColumnName("cuenta_contable_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Formula).HasColumnName("formula");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .HasColumnName("tipo");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.ConceptoNominas)
                .HasForeignKey(d => d.CuentaContableId)
                .HasConstraintName("FK__concepto___cuent__65F62111");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ConceptoNominas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__concepto___entidad_95020317");
        });

        modelBuilder.Entity<Consecutivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__consecut__3213E83F59538ECE");

            entity.ToTable("consecutivo", "nucleo", tb => tb.HasComment("Actualizar con SELECT ... FOR UPDATE dentro de la transacción para evitar saltos/duplicados concurrentes (POS + ERP simultáneo)."));

            entity.HasIndex(e => new { e.EntidadId, e.SucursalId, e.TipoDocumento, e.Serie }, "UQ__consecut__F13666106111EF8C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__consecuti__entid__7E37BEF6");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Consecutivos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__consecuti__sucur__7F2BE32F");
        });

        modelBuilder.Entity<ConteoFisico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conteo_f__3213E83F21C3B626");

            entity.ToTable("conteo_fisico", "inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__conteo_fi__almac__595B4002");

            entity.HasOne(d => d.Responsable).WithMany(p => p.ConteoFisicos)
                .HasForeignKey(d => d.ResponsableId)
                .HasConstraintName("FK__conteo_fi__respo__5E1FF51F");
        });

        modelBuilder.Entity<ConteoFisicoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conteo_f__3213E83F3DBD93A6");

            entity.ToTable("conteo_fisico_detalle", "inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadFisica)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("cantidad_fisica");
            entity.Property(e => e.CantidadSistema)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("cantidad_sistema");
            entity.Property(e => e.ConteoId).HasColumnName("conteo_id");
            entity.Property(e => e.Diferencia)
                .HasComputedColumnSql("(CONVERT([numeric](16,4),[cantidad_fisica]-[cantidad_sistema]))", true)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("diferencia");
            entity.Property(e => e.Justificacion).HasColumnName("justificacion");
            entity.Property(e => e.MovimientoAjusteId).HasColumnName("movimiento_ajuste_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Conteo).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.ConteoId)
                .HasConstraintName("FK__conteo_fi__conte__62E4AA3C");

            entity.HasOne(d => d.MovimientoAjuste).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.MovimientoAjusteId)
                .HasConstraintName("FK__conteo_fi__movim__64CCF2AE");

            entity.HasOne(d => d.Producto).WithMany(p => p.ConteoFisicoDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__conteo_fi__produ__63D8CE75");
        });

        modelBuilder.Entity<ContratoEconomico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__contrato__3213E83F921A7545");

            entity.ToTable("contrato_economico", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroContrato }, "UQ__contrato__8A0FE5373F4C0DD6").IsUnique();

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
                .HasColumnType("numeric(16, 2)")
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
                .HasConstraintName("FK__contrato___clien__61BB7BD9");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ContratoEconomicos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___entid__5FD33367");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ContratoEconomicos)
                .HasForeignKey(d => d.ProveedorId)
                .HasConstraintName("FK__contrato___prove__62AFA012");
        });

        modelBuilder.Entity<ContratoLaboral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__contrato__3213E83FC89892BE");

            entity.ToTable("contrato_laboral", "rrhh");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CargoId).HasColumnName("cargo_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasColumnType("numeric(4, 1)")
                .HasColumnName("jornada_horas_semana");
            entity.Property(e => e.SalarioPactado)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_pactado");
            entity.Property(e => e.TipoContrato)
                .HasMaxLength(20)
                .HasColumnName("tipo_contrato");

            entity.HasOne(d => d.Cargo).WithMany(p => p.ContratoLaborals)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___cargo__44952D46");

            entity.HasOne(d => d.Empleado).WithMany(p => p.ContratoLaborals)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__contrato___emple__41B8C09B");
        });

        modelBuilder.Entity<CuentaBancarium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_b__3213E83FE67C0332");

            entity.ToTable("cuenta_bancaria", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroCuenta }, "UQ__cuenta_b__A1F879CD720D1023").IsUnique();

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
            entity.Property(e => e.Moneda)
                .HasMaxLength(20)
                .HasColumnName("moneda");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(40)
                .HasColumnName("numero_cuenta");
            entity.Property(e => e.SaldoActual)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("saldo_actual");
            entity.Property(e => e.TipoCuenta)
                .HasMaxLength(20)
                .HasColumnName("tipo_cuenta");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.CuentaBancaria)
                .HasForeignKey(d => d.CuentaContableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_ba__cuent__756D6ECB");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaBancaria)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_ba__entid__73852659");
        });

        modelBuilder.Entity<CuentaContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_c__3213E83F9C47060A");

            entity.ToTable("cuenta_contable", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__cuenta_c__499C97559ACEEB69").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__cuenta_co__cuent__08B54D69");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_co__entid__07C12930");
        });

        modelBuilder.Entity<CuentaPorCobrar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_p__3213E83F5A8D6815");

            entity.ToTable("cuenta_por_cobrar", "contabilidad");

            entity.HasIndex(e => e.ClienteId, "idx_cxc_cliente");

            entity.HasIndex(e => e.FechaVencimiento, "idx_cxc_vencimiento").HasFilter("([estado] IN ('PENDIENTE', 'PARCIAL'))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoOrigenId).HasColumnName("asiento_origen_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenNumero)
                .HasMaxLength(100)
                .HasColumnName("documento_origen_numero");
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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_original");
            entity.Property(e => e.SaldoPendiente)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("saldo_pendiente");

            entity.HasOne(d => d.AsientoOrigen).WithMany(p => p.CuentaPorCobrars)
                .HasForeignKey(d => d.AsientoOrigenId)
                .HasConstraintName("FK__cuenta_po__asien__59C55456");

            entity.HasOne(d => d.Cliente).WithMany(p => p.CuentaPorCobrars)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__asien__59785AS3");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaPorCobrars)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__entid__58D1301D");
        });

        modelBuilder.Entity<CuentaPorPagar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cuenta_p__3213E83FB3FB8498");

            entity.ToTable("cuenta_por_pagar", "contabilidad");

            entity.HasIndex(e => e.ProveedorId, "idx_cxp_proveedor");

            entity.HasIndex(e => e.FechaVencimiento, "idx_cxp_vencimiento").HasFilter("([estado] IN ('PENDIENTE', 'PARCIAL'))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoOrigenId).HasColumnName("asiento_origen_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.DocumentoOrigenId).HasColumnName("documento_origen_id");
            entity.Property(e => e.DocumentoOrigenNumero)
                .HasMaxLength(100)
                .HasColumnName("documento_origen_numero");
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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_original");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.SaldoPendiente)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("saldo_pendiente");

            entity.HasOne(d => d.AsientoOrigen).WithMany(p => p.CuentaPorPagars)
                .HasForeignKey(d => d.AsientoOrigenId)
                .HasConstraintName("FK__cuenta_po__asien__625A9A57");

            entity.HasOne(d => d.Entidad).WithMany(p => p.CuentaPorPagars)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__entid__6166761E");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.CuentaPorPagars)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cuenta_po__asien__625F763W");
        });

        modelBuilder.Entity<DeclaracionJuradum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__declarac__3213E83FDDADC5A3");

            entity.ToTable("declaracion_jurada", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.TipoObligacionId, e.PeriodoId }, "UQ__declarac__4B8346ED8FC6AEF4").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.BaseImponible)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("base_imponible");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_calculado");
            entity.Property(e => e.MontoPagado)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_pagado");
            entity.Property(e => e.NumeroDj)
                .HasMaxLength(40)
                .HasColumnName("numero_dj");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.TipoObligacionId).HasColumnName("tipo_obligacion_id");

            entity.HasOne(d => d.Asiento).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__declaraci__asien__13F1F5EB");

            entity.HasOne(d => d.Entidad).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__entid__0E391C95");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__declaraci__gener__14E61A24");

            entity.HasOne(d => d.Periodo).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__perio__10216507");

            entity.HasOne(d => d.TipoObligacion).WithMany(p => p.DeclaracionJurada)
                .HasForeignKey(d => d.TipoObligacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__declaraci__tipo___0F2D40CE");
        });

        modelBuilder.Entity<DevolucionVentaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__devoluci__3213E83F34A52FD5");

            entity.ToTable("devolucion_venta_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadDevuelta)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_devuelta");
            entity.Property(e => e.DevolucionId).HasColumnName("devolucion_id");
            entity.Property(e => e.FacturaDetalleId).HasColumnName("factura_detalle_id");

            entity.HasOne(d => d.Devolucion).WithMany(p => p.DevolucionVentaDetalles)
                .HasForeignKey(d => d.DevolucionId)
                .HasConstraintName("FK__devolucio__devol__35A7EF71");

            entity.HasOne(d => d.FacturaDetalle).WithMany(p => p.DevolucionVentaDetalles)
                .HasForeignKey(d => d.FacturaDetalleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__factu__369C13AA");
        });

        modelBuilder.Entity<DevolucionVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__devoluci__3213E83F8C7CA796");

            entity.ToTable("devolucion_venta", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.AutorizadoPor).HasColumnName("autorizado_por");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],sysdatetimeoffset()))")
                .HasColumnName("fecha");
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.TotalDevuelto)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_devuelto");

            entity.HasOne(d => d.Asiento).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__devolucio__asien__30E33A54");

            entity.HasOne(d => d.AutorizadoPorNavigation).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.AutorizadoPor)
                .HasConstraintName("FK__devolucio__autor__31D75E8D");

            entity.HasOne(d => d.Factura).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__factu__2E06CDA9");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.DevolucionVenta)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__devolucio__movim__2FEF161B");
        });

        modelBuilder.Entity<DispositivoPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposit__3213E83FFD54541B");

            entity.ToTable("dispositivo_pos", "integracion");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__disposit__499C97551CC6407D").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__dispositi__almac__000AF8CF");

            entity.HasOne(d => d.ApiCliente).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.ApiClienteId)
                .HasConstraintName("FK__dispositi__api_c__00FF1D08");

            entity.HasOne(d => d.Caja).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.CajaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__caja___01F34141");

            entity.HasOne(d => d.Entidad).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__entid__7E22B05D");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.DispositivoPos)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__dispositi__sucur__7F16D496");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__empleado__3213E83F74604E0A");

            entity.ToTable("empleado", "rrhh");

            entity.HasIndex(e => e.CarnetIdentidad, "UQ__empleado__9562E2D5300F5471").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
            entity.Property(e => e.TurnoTrabajoId).HasColumnName("turno_trabajo_id");

            entity.HasOne(d => d.Cargo).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__empleado__cargo___3A179ED3");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__empleado__entida__373B3228");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__empleado__sucurs__382F5661");

            entity.HasOne(d => d.TurnoTrabajo).WithMany()
                .HasForeignKey(d => d.TurnoTrabajoId)
                .HasConstraintName("FK_empleado_turno_trabajo");
        });

        modelBuilder.Entity<Entidad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__entidad__3213E83F1D76A5F7");

            entity.ToTable("entidad", "nucleo");

            entity.HasIndex(e => e.CodigoReeup, "UQ__entidad__2A391DFE56B1538E").IsUnique();

            entity.HasIndex(e => e.Nit, "UQ__entidad__DF97D0E44BCF5C57").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.CodigoReeup)
                .HasMaxLength(20)
                .HasColumnName("codigo_reeup");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
            entity.HasKey(e => e.Id).HasName("PK__equipo__3213E83FABEBDFEA");

            entity.ToTable("equipo", "produccion");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__equipo__499C97556EA5E3A5").IsUnique();

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
                .HasConstraintName("FK__equipo__activo_f__3B95D2F1");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__equipo__entidad___39AD8A7F");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__equipo__sucursal__3AA1AEB8");
        });

        modelBuilder.Entity<Existencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__existenc__3213E83FC7DC19FD");

            entity.ToTable("existencia", "inventario");

            entity.HasIndex(e => new { e.AlmacenId, e.ProductoId }, "UQ__existenc__9B6E2B784C522F05").IsUnique();

            entity.HasIndex(e => new { e.AlmacenId, e.Cantidad, e.StockMinimo }, "idx_existencia_bajo_minimo");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoPromedio)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_promedio");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.StockMaximo)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("stock_maximo");
            entity.Property(e => e.StockMinimo)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("stock_minimo");

            entity.HasOne(d => d.Almacen).WithMany(p => p.Existencia)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__existenci__almac__38EE7070");

            entity.HasOne(d => d.Producto).WithMany(p => p.Existencia)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__existenci__produ__39E294A9");
        });

        modelBuilder.Entity<ExistenciaLote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("existencia_lote", "inventario");

            entity.HasIndex(e => new { e.AlmacenId, e.ProductoId, e.Lote }).IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.AlmacenId).HasColumnName("almacen_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Lote).HasMaxLength(50).HasColumnName("lote");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Cantidad).HasColumnType("numeric(16, 4)").HasColumnName("cantidad");
            entity.Property(e => e.ActualizadoEn).HasDefaultValueSql("(sysdatetimeoffset())").HasColumnName("actualizado_en");

            entity.HasOne(d => d.Almacen).WithMany(p => p.ExistenciaLotes)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK__existencilote__almacen__3974");

            entity.HasOne(d => d.Producto).WithMany(p => p.ExistenciaLotes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK__existencilote__produ__39E2");
        });

        modelBuilder.Entity<FacturaVentaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__factura___3213E83F9412FA27");

            entity.ToTable("factura_venta_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitarioVenta)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_unitario_venta");
            entity.Property(e => e.DescuentoPorcentaje)
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("descuento_porcentaje");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.ImpuestoPorcentaje)
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("impuesto_porcentaje");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.SubtotalLinea)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("subtotal_linea");

            entity.HasOne(d => d.Factura).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__factura_v__factu__1FB8AE52");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__factura_v__movim__247D636F");

            entity.HasOne(d => d.Producto).WithMany(p => p.FacturaVentaDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__produ__20ACD28B");
        });

        modelBuilder.Entity<FacturaVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__factura___3213E83FF6104ED6");

            entity.ToTable("factura_venta", "comercial", tb => tb.HasTrigger("trg_auditar_factura_venta"));

            entity.HasIndex(e => new { e.EntidadId, e.SucursalId, e.Serie, e.NumeroFactura }, "UQ__factura___60E1B78A42C1DFC8").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.CuentaPorCobrarId).HasColumnName("cuenta_por_cobrar_id");
            entity.Property(e => e.DescuentoTotal)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("descuento_total");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("EMITIDA")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("fecha");
            entity.Property(e => e.ImpuestoVentasTotal)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("impuesto_ventas_total");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("CUP")
                .HasColumnName("moneda");
            entity.Property(e => e.MotivoAnulacion).HasColumnName("motivo_anulacion");
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(30)
                .HasComment("RNF-51: asignado vía nucleo.consecutivo con SELECT...FOR UPDATE; numeración reservada por sesión offline (ver integracion.pos_venta_pendiente) para sobrevivir cortes de red del POS.")
                .HasColumnName("numero_factura");
            entity.Property(e => e.Serie)
                .HasMaxLength(10)
                .HasDefaultValue("A")
                .HasColumnName("serie");
            entity.Property(e => e.SesionCajaPosId).HasColumnName("sesion_caja_pos_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.TipoVenta)
                .HasMaxLength(15)
                .HasDefaultValue("MINORISTA")
                .HasColumnName("tipo_venta");
            entity.Property(e => e.Total)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Almacen).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.AlmacenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__almac__0CA5D9DE");

            entity.HasOne(d => d.Asiento).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__factura_v__asien__190BB0C3");

            entity.HasOne(d => d.Cliente).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__clien__0ABD916C");

            entity.HasOne(d => d.Contrato).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.ContratoId)
                .HasConstraintName("FK__factura_v__contr__0BB1B5A5");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__factura_v__cread__1AF3F935");

            entity.HasOne(d => d.CuentaPorCobrar).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.CuentaPorCobrarId)
                .HasConstraintName("FK__factura_v__cuent__19FFD4FC");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.DispositivoPosId)
                .HasConstraintName("fk_factura_dispositivo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__entid__07E124C1");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.SesionCajaPosId)
                .HasConstraintName("fk_factura_sesion_caja");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.FacturaVenta)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__factura_v__sucur__08D548FA");
        });

        modelBuilder.Entity<FamiliaProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__familia___3213E83F5DC8DAA8");

            entity.ToTable("familia_producto", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__familia___499C9755249121A5").IsUnique();

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
                .HasConstraintName("FK__familia_p__entid__0FEC5ADD");

            entity.HasOne(d => d.FamiliaPadre).WithMany(p => p.InverseFamiliaPadre)
                .HasForeignKey(d => d.FamiliaPadreId)
                .HasConstraintName("FK__familia_p__famil__10E07F16");
        });

        modelBuilder.Entity<FichaCosto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ficha_co__3213E83F84D992D0");

            entity.ToTable("ficha_costo", "produccion", tb => tb.HasComment("RNF-40: versionada para permitir redefinición ágil ante cambios de precios de insumos."));

            entity.HasIndex(e => new { e.ProductoId, e.Version }, "UQ__ficha_co__2CC7B279602A14E8").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CostoManoObra)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_mano_obra");
            entity.Property(e => e.CostoMateriaPrima)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_materia_prima");
            entity.Property(e => e.CostoTotalUnitario)
                .HasComputedColumnSql("(CONVERT([numeric](14,4),([costo_materia_prima]+[costo_mano_obra])+[gastos_indirectos]))", true)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_total_unitario");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("VIGENTE")
                .HasColumnName("estado");
            entity.Property(e => e.GastosIndirectos)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("gastos_indirectos");
            entity.Property(e => e.MargenPorcentaje)
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("margen_porcentaje");
            entity.Property(e => e.PrecioSugerido)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("precio_sugerido");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__ficha_cos__cread__7132C993");

            entity.HasOne(d => d.Entidad).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ficha_cos__entid__6991A7CB");

            entity.HasOne(d => d.Producto).WithMany(p => p.FichaCostos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ficha_cos__produ__6A85CC04");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("feedback", "nucleo");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasColumnName("tipo");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.MetadataTecnica).HasColumnName("metadata_tecnica");
            entity.Property(e => e.Estado).HasMaxLength(15).HasDefaultValue("PENDIENTE").HasColumnName("estado");
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("(sysdatetimeoffset())").HasColumnName("creado_en");

            entity.HasOne(d => d.Entidad).WithMany().HasForeignKey(d => d.EntidadId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.UsuarioId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("notificacion", "nucleo", tb => tb.HasComment("Centro de notificaciones: eventos generados por el sistema (POS, inventario, seguridad) hacia la entidad, un usuario concreto o el maestro. Persistidas para mostrar bandeja e historial; además de emitirse en vivo por SignalR."));

            entity.HasIndex(e => new { e.EntidadId, e.Leida, e.CreadoEn }, "IX_notificacion_entidad_leida").HasFilter("[entidad_id] IS NOT NULL");
            entity.HasIndex(e => new { e.UsuarioId, e.Leida, e.CreadoEn }, "IX_notificacion_usuario_leida").HasFilter("[usuario_id] IS NOT NULL");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Titulo).HasMaxLength(200).HasColumnName("titulo");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasDefaultValue("info").HasColumnName("tipo");
            entity.Property(e => e.Enlace).HasMaxLength(500).HasColumnName("enlace");
            entity.Property(e => e.Leida).HasDefaultValue(false).HasColumnName("leida");
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("(sysdatetimeoffset())").HasColumnName("creado_en");

            entity.HasOne(d => d.Entidad).WithMany().HasForeignKey(d => d.EntidadId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.UsuarioId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<FormaPagoVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__forma_pa__3213E83FB0E72CD9");

            entity.ToTable("forma_pago_venta", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(24)
                .HasColumnName("forma_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.ReferenciaExterna)
                .HasMaxLength(100)
                .HasColumnName("referencia_externa");
            entity.Property(e => e.VueltoEntregado)
                .HasDefaultValue(0m)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("vuelto_entregado");

            entity.HasOne(d => d.Factura).WithMany(p => p.FormaPagoVenta)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__forma_pag__factu__284DF453");
        });

        modelBuilder.Entity<Indicador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__indicado__3213E83FF85E7D15");

            entity.ToTable("indicador", "reportes");

            entity.HasIndex(e => e.Codigo, "UQ__indicado__40F9A2060882F241").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__indicado__3213E83FFF93D126");

            entity.ToTable("indicador_valor", "reportes");

            entity.HasIndex(e => new { e.EntidadId, e.IndicadorId, e.PeriodoId }, "UQ__indicado__172352B7B461444C").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CalculadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("calculado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.IndicadorId).HasColumnName("indicador_id");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.Valor)
                .HasColumnType("numeric(18, 6)")
                .HasColumnName("valor");

            entity.HasOne(d => d.Entidad).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__entid__45DE573A");

            entity.HasOne(d => d.Indicador).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.IndicadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__indic__46D27B73");

            entity.HasOne(d => d.Periodo).WithMany(p => p.IndicadorValors)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__indicador__perio__47C69FAC");
        });

        modelBuilder.Entity<ListaMateriale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_ma__3213E83F6B259A62");

            entity.ToTable("lista_materiales", "produccion");

            entity.HasIndex(e => new { e.ProductoTerminadoId, e.Version }, "UQ__lista_ma__C659A7C16ACE7DB8").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.ProductoTerminadoId).HasColumnName("producto_terminado_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.ProductoTerminado).WithMany(p => p.ListaMateriales)
                .HasForeignKey(d => d.ProductoTerminadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_mat__produ__76EBA2E9");
        });

        modelBuilder.Entity<ListaMaterialesDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_ma__3213E83FAF37B3DD");

            entity.ToTable("lista_materiales_detalle", "produccion");

            entity.HasIndex(e => new { e.ListaMaterialesId, e.ProductoInsumoId }, "UQ__lista_ma__1951284B085C85AB").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadRequerida)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_requerida");
            entity.Property(e => e.ListaMaterialesId).HasColumnName("lista_materiales_id");
            entity.Property(e => e.PorcentajeMerma)
                .HasColumnType("numeric(5, 2)")
                .HasColumnName("porcentaje_merma");
            entity.Property(e => e.ProductoInsumoId).HasColumnName("producto_insumo_id");

            entity.HasOne(d => d.ListaMateriales).WithMany(p => p.ListaMaterialesDetalles)
                .HasForeignKey(d => d.ListaMaterialesId)
                .HasConstraintName("FK__lista_mat__lista__7E8CC4B1");

            entity.HasOne(d => d.ProductoInsumo).WithMany(p => p.ListaMaterialesDetalles)
                .HasForeignKey(d => d.ProductoInsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lista_mat__produ__7F80E8EA");
        });

        modelBuilder.Entity<ListaPrecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lista_pr__3213E83F6B0BECE8");

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
            entity.HasKey(e => e.Id).HasName("PK__lista_pr__3213E83F0A6F2AE8");

            entity.ToTable("lista_precio_detalle", "inventario");

            entity.HasIndex(e => new { e.ListaPrecioId, e.ProductoId }, "UQ__lista_pr__9974CE05F892D387").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.Precio)
                .HasColumnType("numeric(14, 2)")
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
            entity.HasKey(e => e.Id).HasName("PK__mantenim__3213E83F3C245BE3");

            entity.ToTable("mantenimiento_programado", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Costo)
                .HasColumnType("numeric(14, 2)")
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
                .HasConstraintName("FK__mantenimi__equip__414EAC47");

            entity.HasOne(d => d.Responsable).WithMany(p => p.MantenimientoProgramados)
                .HasForeignKey(d => d.ResponsableId)
                .HasConstraintName("FK__mantenimi__respo__442B18F2");
        });

        modelBuilder.Entity<Merma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__merma__3213E83F3B8A243A");

            entity.ToTable("merma", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Causa)
                .HasMaxLength(30)
                .HasColumnName("causa");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],sysdatetimeoffset()))")
                .HasColumnName("fecha");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.ValorContable)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("valor_contable");

            entity.HasOne(d => d.Asiento).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__merma__asiento_i__33F4B129");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.OrdenProduccionId)
                .HasConstraintName("FK__merma__orden_pro__3118447E");

            entity.HasOne(d => d.Producto).WithMany(p => p.Mermas)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__merma__producto___320C68B7");
        });

        modelBuilder.Entity<MovimientoBancario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FB3878FDC");

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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .HasColumnName("referencia");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.MovimientoBancarios)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__movimient__asien__7E02B4CC");

            entity.HasOne(d => d.CuentaBancaria).WithMany(p => p.MovimientoBancarios)
                .HasForeignKey(d => d.CuentaBancariaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__cuent__7B264821");
        });

        modelBuilder.Entity<MovimientoCajaPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FB6A7C38D");

            entity.ToTable("movimiento_caja_pos", "integracion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AutorizadoPor).HasColumnName("autorizado_por");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Monto)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.OcurridoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("ocurrido_en");
            entity.Property(e => e.SesionCajaPosId).HasColumnName("sesion_caja_pos_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");

            entity.HasOne(d => d.AutorizadoPorNavigation).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.AutorizadoPor)
                .HasConstraintName("FK__movimient__autor__1F83A428");

            entity.HasOne(d => d.Factura).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__movimient__factu__1E8F7FEF");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.MovimientoCajaPos)
                .HasForeignKey(d => d.SesionCajaPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__sesio__1CA7377D");
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FFD7DA5C8");

            entity.ToTable("movimiento_inventario", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroDocumento }, "UQ__movimien__1A2B8BD3FDDAD19D").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__movimient__almac__4A18FC72");

            entity.HasOne(d => d.AlmacenOrigen).WithMany(p => p.MovimientoInventarioAlmacenOrigens)
                .HasForeignKey(d => d.AlmacenOrigenId)
                .HasConstraintName("FK__movimient__almac__4924D839");

            entity.HasOne(d => d.Asiento).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__movimient__asien__4DE98D56");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__movimient__cread__4EDDB18F");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.DispositivoPosId)
                .HasConstraintName("fk_mov_inv_dispositivo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__entid__473C8FC7");

            entity.HasOne(d => d.TipoMovimiento).WithMany(p => p.MovimientoInventarios)
                .HasForeignKey(d => d.TipoMovimientoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__tipo___4830B400");
        });

        modelBuilder.Entity<MovimientoInventarioDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83F0859A736");

            entity.ToTable("movimiento_inventario_detalle", "inventario");

            entity.HasIndex(e => e.ProductoId, "idx_mov_inv_det_producto");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("numeric(14, 4)")
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
                .HasConstraintName("FK__movimient__movim__53A266AC");

            entity.HasOne(d => d.Producto).WithMany(p => p.MovimientoInventarioDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movimient__produ__54968AE5");
        });

        modelBuilder.Entity<NominaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nomina_d__3213E83F26196BE1");

            entity.ToTable("nomina_detalle", "rrhh", tb => tb.HasTrigger("trg_auditar_nomina_detalle"));

            entity.HasIndex(e => new { e.PeriodoNominaId, e.EmpleadoId }, "UQ__nomina_d__73AB3149BA092569").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DiasTrabajados)
                .HasColumnType("numeric(4, 1)")
                .HasColumnName("dias_trabajados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.HorasExtra)
                .HasColumnType("numeric(6, 2)")
                .HasColumnName("horas_extra");
            entity.Property(e => e.PeriodoNominaId).HasColumnName("periodo_nomina_id");
            entity.Property(e => e.SalarioDevengado)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_devengado");
            entity.Property(e => e.SalarioNeto)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_neto");
            entity.Property(e => e.TotalDeducciones)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("total_deducciones");

            entity.HasOne(d => d.Empleado).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__nomina_de__emple__7720AD13");

            entity.HasOne(d => d.PeriodoNomina).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.PeriodoNominaId)
                .HasConstraintName("FK__nomina_de__perio__762C88DA");
        });

        modelBuilder.Entity<NominaDetalleConcepto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nomina_d__3213E83F6FB41BCF");

            entity.ToTable("nomina_detalle_concepto", "rrhh", tb => tb.HasComment("RNF-22: histórico salarial inalterable — no se actualiza tras CONTABILIZADA, solo se referencia para reportes probatorios."));

            entity.HasIndex(e => new { e.NominaDetalleId, e.ConceptoId }, "UQ__nomina_d__7AD6F1A3AED02ED9").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ConceptoId).HasColumnName("concepto_id");
            entity.Property(e => e.Monto)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.NominaDetalleId).HasColumnName("nomina_detalle_id");

            entity.HasOne(d => d.Concepto).WithMany(p => p.NominaDetalleConceptos)
                .HasForeignKey(d => d.ConceptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__nomina_de__conce__019E3B86");

            entity.HasOne(d => d.NominaDetalle).WithMany(p => p.NominaDetalleConceptos)
                .HasForeignKey(d => d.NominaDetalleId)
                .HasConstraintName("FK__nomina_de__nomin__00AA174D");
        });

        modelBuilder.Entity<OrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_co__3213E83F54DE93BF");

            entity.ToTable("orden_compra", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroOrden }, "UQ__orden_co__7EE36A6467714701").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenDestinoId).HasColumnName("almacen_destino_id");
            entity.Property(e => e.AprobadoPor).HasColumnName("aprobado_por");
            entity.Property(e => e.ContratoId).HasColumnName("contrato_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("BORRADOR")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],sysdatetimeoffset()))")
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
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.AlmacenDestino).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.AlmacenDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__almac__6D2D2E85");

            entity.HasOne(d => d.AprobadoPorNavigation).WithMany(p => p.OrdenCompraAprobadoPorNavigations)
                .HasForeignKey(d => d.AprobadoPor)
                .HasConstraintName("FK__orden_com__aprob__73DA2C14");

            entity.HasOne(d => d.Contrato).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.ContratoId)
                .HasConstraintName("FK__orden_com__contr__6C390A4C");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.OrdenCompraCreadoPorNavigations)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__orden_com__cread__74CE504D");

            entity.HasOne(d => d.Entidad).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__entid__6A50C1DA");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.OrdenCompras)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__prove__6B44E613");
        });

        modelBuilder.Entity<OrdenCompraDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_co__3213E83F88BE58FC");

            entity.ToTable("orden_compra_detalle", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadRecibida)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_recibida");
            entity.Property(e => e.CantidadSolicitada)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_solicitada");
            entity.Property(e => e.OrdenCompraId).HasColumnName("orden_compra_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.SubtotalLinea)
                .HasComputedColumnSql("(CONVERT([numeric](16,2),[cantidad_solicitada]*[precio_unitario]))", true)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("subtotal_linea");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.OrdenCompraDetalles)
                .HasForeignKey(d => d.OrdenCompraId)
                .HasConstraintName("FK__orden_com__orden__7993056A");

            entity.HasOne(d => d.Producto).WithMany(p => p.OrdenCompraDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_com__produ__7A8729A3");
        });

        modelBuilder.Entity<OrdenProduccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_pr__3213E83F34539C4A");

            entity.ToTable("orden_produccion", "produccion");

            entity.HasIndex(e => new { e.EntidadId, e.NumeroOrden }, "UQ__orden_pr__7EE36A6421AFB15F").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenInsumosId).HasColumnName("almacen_insumos_id");
            entity.Property(e => e.AlmacenProductoId).HasColumnName("almacen_producto_id");
            entity.Property(e => e.AsientoConsumoId).HasColumnName("asiento_consumo_id");
            entity.Property(e => e.AsientoTerminadoId).HasColumnName("asiento_terminado_id");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.CantidadProducida)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_producida");
            entity.Property(e => e.CostoRealTotal)
                .HasColumnType("numeric(16, 4)")
                .HasColumnName("costo_real_total");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__orden_pro__almac__1940BAED");

            entity.HasOne(d => d.AlmacenProducto).WithMany(p => p.OrdenProduccionAlmacenProductos)
                .HasForeignKey(d => d.AlmacenProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__almac__1A34DF26");

            entity.HasOne(d => d.AsientoConsumo).WithMany(p => p.OrdenProduccionAsientoConsumos)
                .HasForeignKey(d => d.AsientoConsumoId)
                .HasConstraintName("FK__orden_pro__asien__1E05700A");

            entity.HasOne(d => d.AsientoTerminado).WithMany(p => p.OrdenProduccionAsientoTerminados)
                .HasForeignKey(d => d.AsientoTerminadoId)
                .HasConstraintName("FK__orden_pro__asien__1EF99443");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK__orden_pro__cread__1FEDB87C");

            entity.HasOne(d => d.Entidad).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__entid__15702A09");

            entity.HasOne(d => d.FichaCosto).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.FichaCostoId)
                .HasConstraintName("FK__orden_pro__ficha__184C96B4");

            entity.HasOne(d => d.ListaMateriales).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.ListaMaterialesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__lista__1758727B");

            entity.HasOne(d => d.ProductoTerminado).WithMany(p => p.OrdenProduccions)
                .HasForeignKey(d => d.ProductoTerminadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__produ__16644E42");
        });

        modelBuilder.Entity<OrdenProduccionConsumo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orden_pr__3213E83FC301D1AC");

            entity.ToTable("orden_produccion_consumo", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.CantidadReal)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_real");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("costo_unitario");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.OrdenProduccionId).HasColumnName("orden_produccion_id");
            entity.Property(e => e.ProductoInsumoId).HasColumnName("producto_insumo_id");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__orden_pro__movim__278EDA44");

            entity.HasOne(d => d.OrdenProduccion).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.OrdenProduccionId)
                .HasConstraintName("FK__orden_pro__orden__24B26D99");

            entity.HasOne(d => d.ProductoInsumo).WithMany(p => p.OrdenProduccionConsumos)
                .HasForeignKey(d => d.ProductoInsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__orden_pro__produ__25A691D2");
        });

        modelBuilder.Entity<PagoAplicado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pago_apl__3213E83FADE9302F");

            entity.ToTable("pago_aplicado", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoId).HasColumnName("asiento_id");
            entity.Property(e => e.CuentaPorCobrarId).HasColumnName("cuenta_por_cobrar_id");
            entity.Property(e => e.CuentaPorPagarId).HasColumnName("cuenta_por_pagar_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(24)
                .HasColumnName("forma_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.ReferenciaExterna)
                .HasMaxLength(100)
                .HasColumnName("referencia_externa");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Asiento).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__pago_apli__asien__6DCC4D03");

            entity.HasOne(d => d.CuentaPorCobrar).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.CuentaPorCobrarId)
                .HasConstraintName("FK__pago_apli__cuent__6AEFE058");

            entity.HasOne(d => d.CuentaPorPagar).WithMany(p => p.PagoAplicados)
                .HasForeignKey(d => d.CuentaPorPagarId)
                .HasConstraintName("FK__pago_apli__cuent__6BE40491");
        });

        modelBuilder.Entity<PaqueteInformacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__paquete___3213E83F6A3A5843");

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("generado_en");
            entity.Property(e => e.GeneradoPor).HasColumnName("generado_por");
            entity.Property(e => e.PeriodoId).HasColumnName("periodo_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasColumnName("tipo");
            entity.Property(e => e.Onat).HasMaxLength(50).HasColumnName("onat");
            entity.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("direccion");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__paquete_i__entid__4C8B54C9");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__paquete_i__gener__515009E6");

            entity.HasOne(d => d.Periodo).WithMany(p => p.PaqueteInformacions)
                .HasForeignKey(d => d.PeriodoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__paquete_i__perio__4D7F7902");
        });

        modelBuilder.Entity<ParametroSistema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__parametr__3213E83F1CAAB5CC");

            entity.ToTable("parametro_sistema", "nucleo", tb => tb.HasComment("Tasas fiscales, escalas salariales, % vacaciones, etc. Versionado por vigencia para resistir cambios normativos frecuentes del MFP/ONAT."));

            entity.HasIndex(e => new { e.EntidadId, e.Codigo, e.VigenteDesde }, "UQ__parametr__F1C002CF1573896B").IsUnique();

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
                .HasDefaultValueSql("(CONVERT([date],sysdatetimeoffset()))")
                .HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Entidad).WithMany(p => p.ParametroSistemas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__parametro__entid__778AC167");
        });

        modelBuilder.Entity<PeriodoContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__periodo___3213E83FDAF1F74A");

            entity.ToTable("periodo_contable", "contabilidad");

            entity.HasIndex(e => new { e.EntidadId, e.Anio, e.Mes }, "UQ__periodo___10577F90162B7583").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CerradoEn).HasColumnName("cerrado_en");
            entity.Property(e => e.CerradoPor).HasColumnName("cerrado_por");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en");
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
                .HasConstraintName("FK__periodo_c__cerra__208CD6FA");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PeriodoContables)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__periodo_c__entid__1CBC4616");
        });

        modelBuilder.Entity<PeriodoNomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__periodo___3213E83F00BA9E51");

            entity.ToTable("periodo_nomina", "rrhh");

            entity.HasIndex(e => new { e.EntidadId, e.Anio, e.Mes, e.Tipo }, "UQ__periodo___74C90005BCB04035").IsUnique();

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
                .HasConstraintName("FK__periodo_n__aprob__7167D3BD");

            entity.HasOne(d => d.Asiento).WithMany(p => p.PeriodoNominas)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__periodo_n__asien__7073AF84");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PeriodoNominas)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__periodo_n__entid__6ABAD62E");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__permiso__3213E83F914715C1");

            entity.ToTable("permiso", "nucleo");

            entity.HasIndex(e => e.Codigo, "UQ__permiso__40F9A2061C4B148D").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__plan_pro__3213E83F8BD8253D");

            entity.ToTable("plan_produccion", "produccion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__plan_prod__entid__0539C240");

            entity.HasOne(d => d.Presupuesto).WithMany(p => p.PlanProduccions)
                .HasForeignKey(d => d.PresupuestoId)
                .HasConstraintName("FK__plan_prod__presu__07220AB2");
        });

        modelBuilder.Entity<PlanProduccionDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__plan_pro__3213E83FDC4D9E9E");

            entity.ToTable("plan_produccion_detalle", "produccion");

            entity.HasIndex(e => new { e.PlanId, e.ProductoId }, "UQ__plan_pro__612A41F21D8F72D3").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CantidadEjecutada)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_ejecutada");
            entity.Property(e => e.CantidadPlanificada)
                .HasColumnType("numeric(14, 4)")
                .HasColumnName("cantidad_planificada");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanProduccionDetalles)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__plan_prod__plan___0EC32C7A");

            entity.HasOne(d => d.Producto).WithMany(p => p.PlanProduccionDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plan_prod__produ__0FB750B3");
        });

        modelBuilder.Entity<PlantillaAprobadum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__plantill__3213E83FE20D2E60");

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
                .HasConstraintName("FK__plantilla__cargo__318258D2");

            entity.HasOne(d => d.Entidad).WithMany(p => p.PlantillaAprobada)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__plantilla__entid__2F9A1060");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.PlantillaAprobada)
                .HasForeignKey(d => d.SucursalId)
                .HasConstraintName("FK__plantilla__sucur__308E3499");
        });

        modelBuilder.Entity<PosRangoNumeracion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_rang__3213E83F4CD36FB3");

            entity.ToTable("pos_rango_numeracion", "integracion", tb => tb.HasComment("Alternativa a reservar consecutivos: el servidor asigna bloques (p.ej. 1000 números) a cada terminal al sincronizar. El terminal numera localmente dentro de su rango incluso sin conexión, preservando RNF-51 (sin duplicados ni saltos) sin depender de la red para cada venta."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Agotado).HasColumnName("agotado");
            entity.Property(e => e.AsignadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__pos_rango__dispo__2FBA0BF1");
        });

        modelBuilder.Entity<PosSyncLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_sync__3213E83F7180B188");

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("iniciado_en");
            entity.Property(e => e.RegistrosProcesados).HasColumnName("registros_procesados");
            entity.Property(e => e.TipoSync)
                .HasMaxLength(20)
                .HasColumnName("tipo_sync");

            entity.HasOne(d => d.DispositivoPos).WithMany(p => p.PosSyncLogs)
                .HasForeignKey(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_sync___dispo__375B2DB9");
        });

        modelBuilder.Entity<PosVentaPendiente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pos_vent__3213E83F110240B8");

            entity.ToTable("pos_venta_pendiente", "integracion", tb => tb.HasComment("RNF-02/RNF-50: la app POS crea el registro localmente con idempotency_key propio y hace upsert al reconectar. El worker de sincronización procesa PENDIENTE -> crea factura_venta -> marca PROCESADO. Reintentos seguros gracias a la clave única (dispositivo, idempotency_key)."));

            entity.HasIndex(e => new { e.DispositivoPosId, e.IdempotencyKey }, "UQ__pos_vent__C11F94FFB26DE216").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__pos_venta__dispo__253C7D7E");

            entity.HasOne(d => d.Factura).WithMany(p => p.PosVentaPendientes)
                .HasForeignKey(d => d.FacturaId)
                .HasConstraintName("FK__pos_venta__factu__2AF556D4");

            entity.HasOne(d => d.SesionCajaPos).WithMany(p => p.PosVentaPendientes)
                .HasForeignKey(d => d.SesionCajaPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pos_venta__sesio__2630A1B7");
        });

        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__presupue__3213E83F69C953D1");

            entity.ToTable("presupuesto", "contabilidad");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.AprobadoPor).HasColumnName("aprobado_por");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__presupues__aprob__1C873BEC");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__presupues__entid__19AACF41");
        });

        modelBuilder.Entity<PresupuestoLinea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__presupue__3213E83F2039041B");

            entity.ToTable("presupuesto_linea", "contabilidad");

            entity.HasIndex(e => new { e.PresupuestoId, e.CuentaId, e.CentroCostoId, e.Mes }, "UQ__presupue__5D857A3F01D8058B").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CentroCostoId).HasColumnName("centro_costo_id");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.MontoPlanificado)
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_planificado");
            entity.Property(e => e.PresupuestoId).HasColumnName("presupuesto_id");

            entity.HasOne(d => d.CentroCosto).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.CentroCostoId)
                .HasConstraintName("FK__presupues__centr__24285DB4");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__presupues__cuent__2334397B");

            entity.HasOne(d => d.Presupuesto).WithMany(p => p.PresupuestoLineas)
                .HasForeignKey(d => d.PresupuestoId)
                .HasConstraintName("FK__presupues__presu__22401542");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__producto__3213E83FE5D6F560");

            entity.ToTable("producto", "inventario");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__producto__499C9755224255D6").IsUnique();

            entity.HasIndex(e => e.CodigoBarras, "idx_producto_codigo_barras");

            entity.HasIndex(e => e.Nombre, "idx_producto_nombre");

            entity.HasIndex(e => new { e.EntidadId, e.CodigoBarras }, "uq_producto_codigo_barras")
                .IsUnique()
                .HasFilter("([codigo_barras] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasColumnType("numeric(14, 2)")
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
            entity.HasKey(e => e.Id).HasName("PK__proveedo__3213E83FFE180069");

            entity.ToTable("proveedor", "comercial");

            entity.HasIndex(e => new { e.EntidadId, e.Nit }, "uq_proveedor_nit")
                .IsUnique()
                .HasFilter("([nit] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__proveedor__cuent__4CC05EF3");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__proveedor__entid__49E3F248");
        });

        modelBuilder.Entity<RecepcionCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recepcio__3213E83F7231445D");

            entity.ToTable("recepcion_compra", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CuentaPorPagarId).HasColumnName("cuenta_por_pagar_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(CONVERT([date],sysdatetimeoffset()))")
                .HasColumnName("fecha");
            entity.Property(e => e.MovimientoInventarioId).HasColumnName("movimiento_inventario_id");
            entity.Property(e => e.NumeroInformeRecepcion)
                .HasMaxLength(30)
                .HasColumnName("numero_informe_recepcion");
            entity.Property(e => e.OrdenCompraId).HasColumnName("orden_compra_id");
            entity.Property(e => e.RecibidoPor).HasColumnName("recibido_por");

            entity.HasOne(d => d.CuentaPorPagar).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.CuentaPorPagarId)
                .HasConstraintName("FK__recepcion__cuent__01342732");

            entity.HasOne(d => d.MovimientoInventario).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.MovimientoInventarioId)
                .HasConstraintName("FK__recepcion__movim__004002F9");

            entity.HasOne(d => d.OrdenCompra).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.OrdenCompraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recepcion__orden__7F4BDEC0");

            entity.HasOne(d => d.RecibidoPorNavigation).WithMany(p => p.RecepcionCompras)
                .HasForeignKey(d => d.RecibidoPor)
                .HasConstraintName("FK__recepcion__recib__031C6FA4");
        });

        modelBuilder.Entity<RegistroAsistencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registro__3213E83F87474EB7");

            entity.ToTable("registro_asistencia", "rrhh");

            entity.HasIndex(e => new { e.EmpleadoId, e.Fecha }, "UQ__registro__41AA24CE7A312BDE").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.HoraEntrada).HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida).HasColumnName("hora_salida");
            entity.Property(e => e.HorasExtra)
                .HasColumnType("numeric(4, 2)")
                .HasColumnName("horas_extra");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.RegistradoPor).HasColumnName("registrado_por");
            entity.Property(e => e.TipoAusenciaId).HasColumnName("tipo_ausencia_id");
            entity.Property(e => e.TurnoTrabajoId).HasColumnName("turno_trabajo_id");
            entity.Property(e => e.RetardoMinutos).HasColumnName("retardo_minutos");
            entity.Property(e => e.SalidaTempranaMinutos).HasColumnName("salida_temprana_minutos");

            entity.HasOne(d => d.Empleado).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__registro___emple__50FB042B");

            entity.HasOne(d => d.RegistradoPorNavigation).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.RegistradoPor)
                .HasConstraintName("FK__registro___regis__53D770D6");

            entity.HasOne(d => d.TipoAusencia).WithMany(p => p.RegistroAsistencia)
                .HasForeignKey(d => d.TipoAusenciaId)
                .HasConstraintName("FK__registro___tipo___52E34C9D");

            entity.HasOne(d => d.TurnoTrabajo).WithMany()
                .HasForeignKey(d => d.TurnoTrabajoId)
                .HasConstraintName("FK_registro_asistencia_turno_trabajo");
        });

        modelBuilder.Entity<RegistroSalarioTiempoServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registro__3213E83F016A89A5");

            entity.ToTable("registro_salario_tiempo_servicio", "rrhh", tb => tb.HasComment("Equivalente a modelo SC-4-08 u oficial vigente MTSS."));

            entity.HasIndex(e => new { e.EmpleadoId, e.Anio, e.Mes }, "UQ__registro__527F1719DF66C8C9").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.DiasTrabajados)
                .HasColumnType("numeric(4, 1)")
                .HasColumnName("dias_trabajados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.SalarioDevengado)
                .HasColumnType("numeric(12, 2)")
                .HasColumnName("salario_devengado");
            entity.Property(e => e.TiempoServicioAcumuladoMeses).HasColumnName("tiempo_servicio_acumulado_meses");

            entity.HasOne(d => d.Empleado).WithMany(p => p.RegistroSalarioTiempoServicios)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__registro___emple__0662F0A3");
        });

        modelBuilder.Entity<ReporteGenerado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reporte___3213E83F6974E4F0");

            entity.ToTable("reporte_generado", "reportes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Formato)
                .HasMaxLength(10)
                .HasColumnName("formato");
            entity.Property(e => e.GeneradoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__reporte_g__entid__5614BF03");

            entity.HasOne(d => d.GeneradoPorNavigation).WithMany(p => p.ReporteGenerados)
                .HasForeignKey(d => d.GeneradoPor)
                .HasConstraintName("FK__reporte_g__gener__58F12BAE");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__rol__3213E83FB67E5764");

            entity.ToTable("rol", "nucleo");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_Rol_Entidad_Codigo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EsSistema).HasColumnName("es_sistema");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Rols)
                .HasForeignKey(d => d.EntidadId)
                .HasConstraintName("FK_Rol_Entidad");

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
                        j.HasKey("RolId", "PermisoId").HasName("PK__rol_perm__0939B2DFA90EFCCB");
                        j.ToTable("rol_permiso", "nucleo");
                        j.IndexerProperty<int>("RolId").HasColumnName("rol_id");
                        j.IndexerProperty<int>("PermisoId").HasColumnName("permiso_id");
                    });
        });

        modelBuilder.Entity<SaldoVacacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__saldo_va__3213E83F1F6D9D00");

            entity.ToTable("saldo_vacaciones", "rrhh");

            entity.HasIndex(e => new { e.EmpleadoId, e.Anio }, "UQ__saldo_va__09A04708BA2452C1").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.DiasAcumulados)
                .HasColumnType("numeric(6, 2)")
                .HasColumnName("dias_acumulados");
            entity.Property(e => e.DiasCompensados)
                .HasColumnType("numeric(6, 2)")
                .HasColumnName("dias_compensados");
            entity.Property(e => e.DiasDisfrutados)
                .HasColumnType("numeric(6, 2)")
                .HasColumnName("dias_disfrutados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.SaldoActual)
                .HasComputedColumnSql("(CONVERT([numeric](6,2),([dias_acumulados]-[dias_disfrutados])-[dias_compensados]))", true)
                .HasColumnType("numeric(6, 2)")
                .HasColumnName("saldo_actual");

            entity.HasOne(d => d.Empleado).WithMany(p => p.SaldoVacaciones)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__saldo_vac__emple__589C25F3");
        });

        modelBuilder.Entity<SesionCajaPo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sesion_c__3213E83F408B869D");

            entity.ToTable("sesion_caja_pos", "integracion", tb => tb.HasComment("Una caja puede tener una única sesión ABIERTA, compartida por todos sus dispositivos (índice único parcial UNIQUE(caja_id) WHERE estado='ABIERTA')."));

            entity.HasIndex(e => new { e.DispositivoPosId, e.Estado }, "idx_sesion_caja_dispositivo");

            entity.HasIndex(e => new { e.CajaId, e.Estado }, "idx_sesion_caja_caja");

            entity.HasIndex(e => e.CajaId, "uq_sesion_caja_abierta")
                .IsUnique()
                .HasFilter("([estado]='ABIERTA')");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AsientoCierreId).HasColumnName("asiento_cierre_id");
            entity.Property(e => e.CajaId).HasColumnName("caja_id");
            entity.Property(e => e.CajeroId).HasColumnName("cajero_id");
            entity.Property(e => e.CantidadFacturas).HasColumnName("cantidad_facturas");
            entity.Property(e => e.DiferenciaArqueo)
                .HasComputedColumnSql("(CONVERT([numeric](14,2),[monto_cierre_declarado]-[monto_cierre_sistema]))", true)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("diferencia_arqueo");
            entity.Property(e => e.DispositivoPosId).HasColumnName("dispositivo_pos_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ABIERTA")
                .HasColumnName("estado");
            entity.Property(e => e.FechaApertura)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("fecha_apertura");
            entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
            entity.Property(e => e.MontoApertura)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("monto_apertura");
            entity.Property(e => e.MontoCierreDeclarado)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("monto_cierre_declarado");
            entity.Property(e => e.MontoCierreSistema)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("monto_cierre_sistema");
            entity.Property(e => e.ObservacionesCierre).HasColumnName("observaciones_cierre");
            entity.Property(e => e.SupervisorConciliacionId).HasColumnName("supervisor_conciliacion_id");
            entity.Property(e => e.TotalEfectivo)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_efectivo");
            entity.Property(e => e.TotalEnzona)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_enzona");
            entity.Property(e => e.TotalOtrosMedios)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_otros_medios");
            entity.Property(e => e.TotalTransfermovil)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_transfermovil");
            entity.Property(e => e.TotalVentas)
                .HasColumnType("numeric(16, 2)")
                .HasColumnName("total_ventas");

            entity.HasOne(d => d.AsientoCierre).WithMany(p => p.SesionCajaPos)
                .HasForeignKey(d => d.AsientoCierreId)
                .HasConstraintName("FK__sesion_ca__asien__16EE5E27");

            entity.HasOne(d => d.Caja).WithMany()
                .HasForeignKey(d => d.CajaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sesion_caja_caja");

            entity.HasOne(d => d.Cajero).WithMany(p => p.SesionCajaPoCajeros)
                .HasForeignKey(d => d.CajeroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sesion_ca__cajer__0C70CFB4");

            entity.HasOne(d => d.DispositivoPos).WithOne(p => p.SesionCajaPo)
                .HasForeignKey<SesionCajaPo>(d => d.DispositivoPosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sesion_ca__dispo__0B7CAB7B");

            entity.HasOne(d => d.SupervisorConciliacion).WithMany(p => p.SesionCajaPoSupervisorConciliacions)
                .HasForeignKey(d => d.SupervisorConciliacionId)
                .HasConstraintName("FK__sesion_ca__super__17E28260");
        });

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sucursal__3213E83F25E285D1");

            entity.ToTable("sucursal", "nucleo");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ__sucursal__499C9755E1E0BBC2").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
            entity.Property(e => e.Latitud)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("longitud");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasDefaultValue("ALMACEN")
                .HasColumnName("tipo");

            entity.HasOne(d => d.Entidad).WithMany(p => p.Sucursals)
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sucursal__entida__4316F928");
        });

        modelBuilder.Entity<TurnoTrabajo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_turno_trabajo");

            entity.ToTable("turno_trabajo", "rrhh");

            entity.HasIndex(e => new { e.EntidadId, e.Codigo }, "UQ_turno_trabajo_entidad_codigo").IsUnique();

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
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.EsNocturno).HasColumnName("es_nocturno");
            entity.Property(e => e.HoraEntrada)
                .HasColumnType("time")
                .HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida)
                .HasColumnType("time")
                .HasColumnName("hora_salida");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.ToleranciaMinutos).HasColumnName("tolerancia_minutos");

            entity.HasOne(d => d.Entidad).WithMany()
                .HasForeignKey(d => d.EntidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_turno_trabajo_entidad");
        });

        modelBuilder.Entity<TipoAusencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipo_aus__3213E83F4B273778");

            entity.ToTable("tipo_ausencia", "rrhh");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_aus__40F9A2066DBD0F0C").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__tipo_com__3213E83FF281429A");

            entity.ToTable("tipo_comprobante", "contabilidad");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_com__40F9A20657850D97").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__tipo_mov__3213E83F8B0F59DB");

            entity.ToTable("tipo_movimiento", "inventario");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_mov__40F9A206D0BEB252").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__tipo_obl__3213E83F34866A37");

            entity.ToTable("tipo_obligacion_fiscal", "contabilidad");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_obl__40F9A206C441B51B").IsUnique();

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
                .HasColumnType("numeric(6, 3)")
                .HasColumnName("tasa_actual");
        });

        modelBuilder.Entity<TopePrecioMfp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tope_pre__3213E83FA1FA6F18");

            entity.ToTable("tope_precio_mfp", "comercial");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.FamiliaId).HasColumnName("familia_id");
            entity.Property(e => e.PrecioMaximo)
                .HasColumnType("numeric(14, 2)")
                .HasColumnName("precio_maximo");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.ResolucionReferencia)
                .HasMaxLength(150)
                .HasColumnName("resolucion_referencia");
            entity.Property(e => e.VigenteDesde).HasColumnName("vigente_desde");
            entity.Property(e => e.VigenteHasta).HasColumnName("vigente_hasta");

            entity.HasOne(d => d.Familia).WithMany(p => p.TopePrecioMfps)
                .HasForeignKey(d => d.FamiliaId)
                .HasConstraintName("FK__tope_prec__famil__3C54ED00");

            entity.HasOne(d => d.Producto).WithMany(p => p.TopePrecioMfps)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK__tope_prec__produ__3B60C8C7");
        });

        modelBuilder.Entity<UnidadMedidum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__unidad_m__3213E83FB89A3587");

            entity.ToTable("unidad_medida", "inventario");

            entity.HasIndex(e => e.Codigo, "UQ__unidad_m__40F9A206E361980B").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__usuario__3213E83F4D90C42C");

            entity.ToTable("usuario", "nucleo");

            entity.HasIndex(e => e.Email, "UQ__usuario__AB6E616484E5B8F3").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__usuario__D4D22D747D6B2A8B").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.BloqueadoHasta).HasColumnName("bloqueado_hasta");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
            entity.HasKey(e => new { e.UsuarioId, e.RolId, e.SucursalId }).HasName("PK__usuario___421F1799D8133CF0");

            entity.ToTable("usuario_rol", "nucleo");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.AsignadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("asignado_en");
            entity.Property(e => e.AsignadoPor).HasColumnName("asignado_por");

            entity.HasOne(d => d.AsignadoPorNavigation).WithMany(p => p.UsuarioRolAsignadoPorNavigations)
                .HasForeignKey(d => d.AsignadoPor)
                .HasConstraintName("FK__usuario_r__asign__6477ECF3");

            entity.HasOne(d => d.Rol).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__usuario_r__rol_i__619B8048");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__usuario_r__sucur__628FA481");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioRolUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__usuario_r__usuar__60A75C0F");
        });

        modelBuilder.Entity<UtileResponsabilidad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("utile_responsabilidad", "rrhh");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())").HasColumnName("id");
            entity.Property(e => e.EntidadId).HasColumnName("entidad_id");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Descripcion).HasMaxLength(200).HasColumnName("descripcion");
            entity.Property(e => e.NumeroSerie).HasMaxLength(50).HasColumnName("numero_serie");
            entity.Property(e => e.FechaEntrega).HasDefaultValueSql("(CAST(GETDATE() AS DATE))").HasColumnName("fecha_entrega");
            entity.Property(e => e.FechaDevolucion).HasColumnName("fecha_devolucion");
            entity.Property(e => e.EstadoEntrega).HasMaxLength(50).HasColumnName("estado_entrega");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("(sysdatetimeoffset())").HasColumnName("creado_en");

            entity.HasOne(d => d.Entidad).WithMany().HasForeignKey(d => d.EntidadId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Empleado).WithMany(p => p.UtileResponsabilidades).HasForeignKey(d => d.EmpleadoId).OnDelete(DeleteBehavior.ClientSetNull);
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
                .HasDefaultValue("ERP")
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
                .HasDefaultValue("ERP")
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
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("monto_planificado");
            entity.Property(e => e.MontoReal)
                .HasColumnType("numeric(38, 2)")
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
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("saldo");
            entity.Property(e => e.TotalDebe)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("total_debe");
            entity.Property(e => e.TotalHaber)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("total_haber");
        });

        modelBuilder.Entity<WebhookEntrega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__webhook___3213E83F30008CA6");

            entity.ToTable("webhook_entrega", "integracion");

            entity.HasIndex(e => e.ProximoReintentoEn, "idx_webhook_entrega_pendiente").HasFilter("([exitoso]=(0))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CodigoRespuestaHttp).HasColumnName("codigo_respuesta_http");
            entity.Property(e => e.EnviadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("enviado_en");
            entity.Property(e => e.Exitoso).HasColumnName("exitoso");
            entity.Property(e => e.IntentoNumero)
                .HasDefaultValue((short)1)
                .HasColumnName("intento_numero");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.PayloadJson).HasColumnName("payload_json");
            entity.Property(e => e.ProximoReintentoEn).HasColumnName("proximo_reintento_en");
            entity.Property(e => e.SuscripcionId).HasColumnName("suscripcion_id");

            entity.HasOne(d => d.Suscripcion).WithMany(p => p.WebhookEntregas)
                .HasForeignKey(d => d.SuscripcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__webhook_e__suscr__469D7149");
        });

        modelBuilder.Entity<WebhookSuscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__webhook___3213E83F9F03D476");

            entity.ToTable("webhook_suscripcion", "integracion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ApiClienteId).HasColumnName("api_cliente_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysdatetimeoffset())")
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
                .HasConstraintName("FK__webhook_s__api_c__40E497F3");
        });

        OnModelCreatingPartial(modelBuilder);

        // --- FILTROS GLOBALES DE SEGURIDAD (Multi-tenancy) ---
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            System.Linq.Expressions.Expression? filterBody = null;

            // Referencias dinámicas a propiedades del Contexto (EF las convertirá en parámetros SQL)
            var contextExpr = System.Linq.Expressions.Expression.Constant(this);
            var isMasterExpr = System.Linq.Expressions.Expression.Property(contextExpr, nameof(IsMaster));
            var currentEntidadIdExpr = System.Linq.Expressions.Expression.Property(contextExpr, nameof(CurrentEntidadId));
            var currentSucursalIdExpr = System.Linq.Expressions.Expression.Property(contextExpr, nameof(CurrentSucursalId));

            // Función local para comparar Guids de forma segura (maneja Nullables)
            System.Linq.Expressions.Expression SafeEqual(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
            {
                if (left.Type != right.Type)
                {
                    if (left.Type == typeof(Guid?) && right.Type == typeof(Guid))
                        right = System.Linq.Expressions.Expression.Convert(right, typeof(Guid?));
                    else if (left.Type == typeof(Guid) && right.Type == typeof(Guid?))
                        left = System.Linq.Expressions.Expression.Convert(left, typeof(Guid?));
                }
                return System.Linq.Expressions.Expression.Equal(left, right);
            }

            // 1. Aislamiento por Entidad (Directo o Jerárquico para silenciar warnings 10622)
            var entidadIdProp = entityType.FindProperty("EntidadId");
            if (entidadIdProp != null)
            {
                var entidadIdExpr = System.Linq.Expressions.Expression.Property(parameter, "EntidadId");
                filterBody = SafeEqual(entidadIdExpr, currentEntidadIdExpr);

                // Caso especial: Si EntidadId es Nullable (ej. Roles de Sistema), permitir ver los Nulos
                if (entidadIdProp.ClrType == typeof(Guid?))
                {
                    var isNullExpr = System.Linq.Expressions.Expression.Equal(entidadIdExpr, System.Linq.Expressions.Expression.Constant(null, typeof(Guid?)));
                    filterBody = System.Linq.Expressions.Expression.OrElse(isNullExpr, filterBody);
                }
            }
            else
            {
                // Estrategia Jerárquica: Buscar el ancestro más cercano que tenga EntidadId (Máx 2 niveles)
                var parentNav = entityType.GetNavigations()
                    .FirstOrDefault(n => n.ForeignKey.DeclaringEntityType == entityType && n.ForeignKey.IsRequired);

                if (parentNav != null)
                {
                    if (parentNav.TargetEntityType.FindProperty("EntidadId") != null)
                    {
                        // Nivel 1: e.Parent.EntidadId
                        var parentExpr = System.Linq.Expressions.Expression.Property(parameter, parentNav.Name);
                        var propExpr = System.Linq.Expressions.Expression.Property(parentExpr, "EntidadId");
                        filterBody = SafeEqual(propExpr, currentEntidadIdExpr);
                    }
                    else
                    {
                        // Nivel 2: e.Parent.GrandParent.EntidadId
                        var grandParentNav = parentNav.TargetEntityType.GetNavigations()
                            .FirstOrDefault(n => n.ForeignKey.DeclaringEntityType == parentNav.TargetEntityType && n.ForeignKey.IsRequired && n.TargetEntityType.FindProperty("EntidadId") != null);

                        if (grandParentNav != null)
                        {
                            var parentExpr = System.Linq.Expressions.Expression.Property(parameter, parentNav.Name);
                            var gpExpr = System.Linq.Expressions.Expression.Property(parentExpr, grandParentNav.Name);
                            var propExpr = System.Linq.Expressions.Expression.Property(gpExpr, "EntidadId");
                            filterBody = SafeEqual(propExpr, currentEntidadIdExpr);
                        }
                    }
                }
            }
          
            // 2. Aplicar Excepción para el Usuario Master (Ve todo) y Sellar Filtro
            if (filterBody != null)
            {
                var finalFilter = System.Linq.Expressions.Expression.OrElse(isMasterExpr, filterBody);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(System.Linq.Expressions.Expression.Lambda(finalFilter, parameter));
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
