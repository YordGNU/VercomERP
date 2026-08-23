namespace Vercom.DTOs;

public class ProductoRequestDto
{
    public Guid? Id { get; set; }
    public string? Cod { get; set; }
    public string? Nombre { get; set; }
    public decimal? Precio { get; set; }
    public decimal? Costo { get; set; }
    public decimal? StockMinimo { get; set; }
    public Guid? Categoriaid { get; set; }
    public int? Unidadid { get; set; }
    public Guid? Areaid { get; set; }
    public bool? Activo { get; set; }
}

public class OperacionRequestDto
{
    public int LocalId { get; set; }
    public Guid? PuntoVentaid { get; set; }
    public Guid? Productoid { get; set; } // Server ID
    public Guid? ClienteId { get; set; }
    public Guid? SesionId { get; set; }
    public DateTime? Fecha { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? Importe { get; set; }
    public decimal MontoEfectivo { get; set; }
    public decimal MontoTransferencia { get; set; }
    public string? TransaccionId { get; set; }
    public int? TipoOperacionid { get; set; }
    public string Moneda { get; set; } = "CUP";
    public string? Nota { get; set; }
    public string? TerminalId { get; set; }
    public decimal? MontoRecibido { get; set; }
    public decimal? Cambio { get; set; }
    public int? ReferenciaLocalId { get; set; }
    public string? NumeroVale { get; set; }
}
