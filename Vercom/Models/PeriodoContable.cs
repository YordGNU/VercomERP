namespace Vercom.Models;

public partial class PeriodoContable
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public short Anio { get; set; }

    public short Mes { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public Guid? CerradoPor { get; set; }

    public DateTimeOffset? CerradoEn { get; set; }

    public DateTimeOffset? CreadoEn { get; set; }

    public DateTimeOffset? ActualizadoEn { get; set; }

    public virtual ICollection<ActivoFijoDepreciacion> ActivoFijoDepreciacions { get; set; } = new List<ActivoFijoDepreciacion>();

    public virtual ICollection<AsientoContable> AsientoContables { get; set; } = new List<AsientoContable>();

    public virtual Usuario? CerradoPorNavigation { get; set; }

    public virtual ICollection<DeclaracionJuradum> DeclaracionJurada { get; set; } = new List<DeclaracionJuradum>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<IndicadorValor> IndicadorValors { get; set; } = new List<IndicadorValor>();

    public virtual ICollection<PaqueteInformacion> PaqueteInformacions { get; set; } = new List<PaqueteInformacion>();
}
