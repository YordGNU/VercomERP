using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class ReporteGenerado
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string NombreReporte { get; set; } = null!;

    public string Formato { get; set; } = null!;

    public string? ParametrosJson { get; set; }

    public string RutaArchivo { get; set; } = null!;

    public Guid? GeneradoPor { get; set; }

    public DateTimeOffset GeneradoEn { get; set; }

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual Usuario? GeneradoPorNavigation { get; set; }
}
