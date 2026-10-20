namespace VERCOM.DTOs;

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
