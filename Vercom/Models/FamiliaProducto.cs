using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class FamiliaProducto
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public Guid? FamiliaPadreId { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual FamiliaProducto? FamiliaPadre { get; set; }

    public virtual ICollection<FamiliaProducto> InverseFamiliaPadre { get; set; } = new List<FamiliaProducto>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<TopePrecioMfp> TopePrecioMfps { get; set; } = new List<TopePrecioMfp>();
}
