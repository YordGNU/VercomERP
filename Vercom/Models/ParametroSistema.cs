using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ParametroSistema
{
    public int Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string TipoDato { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateOnly VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;
}
