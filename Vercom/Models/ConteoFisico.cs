namespace Vercom.Models;

public partial class ConteoFisico
{
    public Guid Id { get; set; }

    public Guid AlmacenId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public Guid? ResponsableId { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen Almacen { get; set; } = null!;

    public virtual ICollection<ConteoFisicoDetalle> ConteoFisicoDetalles { get; set; } = new List<ConteoFisicoDetalle>();

    public virtual Usuario? Responsable { get; set; }
}
