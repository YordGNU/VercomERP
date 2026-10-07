namespace Vercom.Models;

public partial class ContratoEconomicoSuplemento
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid ContratoId { get; set; }

    public int NumeroSuplemento { get; set; }

    public DateOnly FechaFirma { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Concepto { get; set; }

    public string? DocumentoUrl { get; set; }

    public string Estado { get; set; } = "VIGENTE";

    public string? MotivoAnulacion { get; set; }

    public decimal? MontoTotalNuevo { get; set; }

    public Guid? CreadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ContratoEconomico Contrato { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;
}