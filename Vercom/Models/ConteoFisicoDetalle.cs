using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ConteoFisicoDetalle
{
    public Guid Id { get; set; }

    public Guid ConteoId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal CantidadSistema { get; set; }

    public decimal? CantidadFisica { get; set; }

    public decimal? Diferencia { get; set; }

    public string? Justificacion { get; set; }

    public Guid? MovimientoAjusteId { get; set; }

    public virtual ConteoFisico Conteo { get; set; } = null!;

    public virtual MovimientoInventario? MovimientoAjuste { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
