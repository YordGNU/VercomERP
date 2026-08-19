using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class DeclaracionJuradum
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public int TipoObligacionId { get; set; }

    public Guid PeriodoId { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal MontoCalculado { get; set; }

    public decimal MontoPagado { get; set; }

    public DateOnly FechaLimite { get; set; }

    public DateOnly? FechaPresentacion { get; set; }

    public string Estado { get; set; } = null!;

    public string? NumeroDj { get; set; }

    public Guid? AsientoId { get; set; }

    public Guid? GeneradoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Usuario? GeneradoPorNavigation { get; set; }

    public virtual PeriodoContable Periodo { get; set; } = null!;

    public virtual TipoObligacionFiscal TipoObligacion { get; set; } = null!;
}
