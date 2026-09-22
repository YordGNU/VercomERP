namespace Vercom.Models;

public partial class TurnoTrabajo
{
    public Guid Id { get; set; }
    public Guid EntidadId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public TimeOnly HoraEntrada { get; set; }
    public TimeOnly HoraSalida { get; set; }
    public int ToleranciaMinutos { get; set; } = 15;
    public bool EsNocturno { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.UtcNow;

    public virtual Entidad Entidad { get; set; } = null!;
}
