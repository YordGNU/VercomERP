using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class PeriodoNomina
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public short Anio { get; set; }

    public short Mes { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public Guid? AsientoId { get; set; }

    public DateTimeOffset? CalculadoEn { get; set; }

    public Guid? AprobadoPor { get; set; }

    public virtual Usuario? AprobadoPorNavigation { get; set; }

    public virtual AsientoContable? Asiento { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<NominaDetalle> NominaDetalles { get; set; } = new List<NominaDetalle>();
}
