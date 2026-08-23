namespace Vercom.Models;

public partial class AsientoContable
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public Guid PeriodoId { get; set; }

    public int TipoComprobanteId { get; set; }

    public long NumeroComprobante { get; set; }

    public DateOnly Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public string ModuloOrigen { get; set; } = null!;

    public string? DocumentoOrigenTipo { get; set; }

    public Guid? DocumentoOrigenId { get; set; }

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }

    public string Estado { get; set; } = null!;

    /// <summary>
    /// RF-12: un asiento contabilizado jamás se edita ni elimina; solo se revierte mediante un nuevo asiento de ajuste enlazado aquí.
    /// </summary>
    public Guid? AsientoReversionId { get; set; }

    public Guid CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ActivoFijoDepreciacion> ActivoFijoDepreciacions { get; set; } = new List<ActivoFijoDepreciacion>();

    public virtual ICollection<AsientoDetalle> AsientoDetalles { get; set; } = new List<AsientoDetalle>();

    public virtual AsientoContable? AsientoReversion { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual ICollection<CuentaPorCobrar> CuentaPorCobrars { get; set; } = new List<CuentaPorCobrar>();

    public virtual ICollection<CuentaPorPagar> CuentaPorPagars { get; set; } = new List<CuentaPorPagar>();

    public virtual ICollection<DeclaracionJuradum> DeclaracionJurada { get; set; } = new List<DeclaracionJuradum>();

    public virtual ICollection<DevolucionVentum> DevolucionVenta { get; set; } = new List<DevolucionVentum>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<AsientoContable> InverseAsientoReversion { get; set; } = new List<AsientoContable>();

    public virtual ICollection<Merma> Mermas { get; set; } = new List<Merma>();

    public virtual ICollection<MovimientoBancario> MovimientoBancarios { get; set; } = new List<MovimientoBancario>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<OrdenProduccion> OrdenProduccionAsientoConsumos { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<OrdenProduccion> OrdenProduccionAsientoTerminados { get; set; } = new List<OrdenProduccion>();

    public virtual ICollection<PagoAplicado> PagoAplicados { get; set; } = new List<PagoAplicado>();

    public virtual PeriodoContable Periodo { get; set; } = null!;

    public virtual ICollection<PeriodoNomina> PeriodoNominas { get; set; } = new List<PeriodoNomina>();

    public virtual ICollection<SesionCajaPo> SesionCajaPos { get; set; } = new List<SesionCajaPo>();

    public virtual Sucursal? Sucursal { get; set; }

    public virtual TipoComprobante TipoComprobante { get; set; } = null!;
}
