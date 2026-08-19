using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ListaPrecioDetalle
{
    public Guid Id { get; set; }

    public Guid ListaPrecioId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Precio { get; set; }

    public virtual ListaPrecio ListaPrecio { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
