using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ExistenciaLote
{
    public Guid Id { get; set; }

    public Guid AlmacenId { get; set; }

    public Guid ProductoId { get; set; }

    public string Lote { get; set; } = null!;

    public DateOnly? FechaVencimiento { get; set; }

    public decimal Cantidad { get; set; }

    public DateTimeOffset ActualizadoEn { get; set; }

    public virtual Almacen Almacen { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
