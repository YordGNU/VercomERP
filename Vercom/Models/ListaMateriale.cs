using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ListaMateriale
{
    public Guid Id { get; set; }

    public Guid ProductoTerminadoId { get; set; }

    public int Version { get; set; }

    public bool Activa { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ListaMaterialesDetalle> ListaMaterialesDetalles { get; set; } = new List<ListaMaterialesDetalle>();

    public virtual ICollection<OrdenProduccion> OrdenProduccions { get; set; } = new List<OrdenProduccion>();

    public virtual Producto ProductoTerminado { get; set; } = null!;
}
