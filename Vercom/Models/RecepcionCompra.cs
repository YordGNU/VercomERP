using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class RecepcionCompra
{
    public Guid Id { get; set; }

    public Guid OrdenCompraId { get; set; }

    public Guid? MovimientoInventarioId { get; set; }

    public Guid? CuentaPorPagarId { get; set; }

    public DateOnly Fecha { get; set; }

    public string? NumeroInformeRecepcion { get; set; }

    public Guid? RecibidoPor { get; set; }

    public virtual CuentaPorPagar? CuentaPorPagar { get; set; }

    public virtual MovimientoInventario? MovimientoInventario { get; set; }

    public virtual OrdenCompra OrdenCompra { get; set; } = null!;

    public virtual Usuario? RecibidoPorNavigation { get; set; }
}
