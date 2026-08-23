namespace Vercom.Models;

public partial class FacturaVentum
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid SucursalId { get; set; }

    /// <summary>
    /// RNF-51: asignado vía nucleo.consecutivo con SELECT...FOR UPDATE; numeración reservada por sesión offline (ver integracion.pos_venta_pendiente) para sobrevivir cortes de red del POS.
    /// </summary>
    public string NumeroFactura { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public Guid ClienteId { get; set; }

    public Guid? ContratoId { get; set; }

    public Guid AlmacenId { get; set; }

    public string CanalVenta { get; set; } = null!;

    public string TipoVenta { get; set; } = null!;

    public Guid? DispositivoPosId { get; set; }

    public Guid? SesionCajaPosId { get; set; }

    public DateTimeOffset Fecha { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal ImpuestoVentasTotal { get; set; }

    public decimal Total { get; set; }

    public string Moneda { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string? MotivoAnulacion { get; set; }

    public Guid? AsientoId { get; set; }

    public Guid? CuentaPorCobrarId { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen Almacen { get; set; } = null!;

    public virtual AsientoContable? Asiento { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ContratoEconomico? Contrato { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual CuentaPorCobrar? CuentaPorCobrar { get; set; }

    public virtual ICollection<DevolucionVentum> DevolucionVenta { get; set; } = new List<DevolucionVentum>();

    public virtual DispositivoPo? DispositivoPos { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentaDetalle> FacturaVentaDetalles { get; set; } = new List<FacturaVentaDetalle>();

    public virtual ICollection<FormaPagoVentum> FormaPagoVenta { get; set; } = new List<FormaPagoVentum>();

    public virtual ICollection<MovimientoCajaPo> MovimientoCajaPos { get; set; } = new List<MovimientoCajaPo>();

    public virtual ICollection<PosVentaPendiente> PosVentaPendientes { get; set; } = new List<PosVentaPendiente>();

    public virtual SesionCajaPo? SesionCajaPos { get; set; }

    public virtual Sucursal Sucursal { get; set; } = null!;
}
