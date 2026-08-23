namespace Vercom.Models;

public partial class MovimientoCajaPo
{
    public Guid Id { get; set; }

    public Guid SesionCajaPosId { get; set; }

    public string Tipo { get; set; } = null!;

    public decimal Monto { get; set; }

    public Guid? FacturaId { get; set; }

    public string? Motivo { get; set; }

    public Guid? AutorizadoPor { get; set; }

    public DateTimeOffset OcurridoEn { get; set; }

    public virtual Usuario? AutorizadoPorNavigation { get; set; }

    public virtual FacturaVentum? Factura { get; set; }

    public virtual SesionCajaPo SesionCajaPos { get; set; } = null!;
}
