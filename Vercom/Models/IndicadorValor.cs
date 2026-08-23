namespace Vercom.Models;

public partial class IndicadorValor
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public int IndicadorId { get; set; }

    public Guid PeriodoId { get; set; }

    public decimal Valor { get; set; }

    public DateTimeOffset CalculadoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Indicador Indicador { get; set; } = null!;

    public virtual PeriodoContable Periodo { get; set; } = null!;
}
