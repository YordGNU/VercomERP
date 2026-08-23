namespace Vercom.Models;

public partial class PaqueteInformacion
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid PeriodoId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public Guid? GeneradoPor { get; set; }

    public DateTimeOffset GeneradoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Usuario? GeneradoPorNavigation { get; set; }

    public virtual PeriodoContable Periodo { get; set; } = null!;
}
