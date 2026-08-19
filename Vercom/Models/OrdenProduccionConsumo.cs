using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class OrdenProduccionConsumo
{
    public Guid Id { get; set; }

    public Guid OrdenProduccionId { get; set; }

    public Guid ProductoInsumoId { get; set; }

    public decimal CantidadPlanificada { get; set; }

    public decimal CantidadReal { get; set; }

    public decimal? CostoUnitario { get; set; }

    public Guid? MovimientoInventarioId { get; set; }

    public virtual MovimientoInventario? MovimientoInventario { get; set; }

    public virtual OrdenProduccion OrdenProduccion { get; set; } = null!;

    public virtual Producto ProductoInsumo { get; set; } = null!;
}
