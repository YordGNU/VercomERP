using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Presupuesto
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public short Anio { get; set; }

    public string Nombre { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public Guid? AprobadoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Usuario? AprobadoPorNavigation { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<PlanProduccion> PlanProduccions { get; set; } = new List<PlanProduccion>();

    public virtual ICollection<PresupuestoLinea> PresupuestoLineas { get; set; } = new List<PresupuestoLinea>();
}
