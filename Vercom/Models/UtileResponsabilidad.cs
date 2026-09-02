using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class UtileResponsabilidad
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid EmpleadoId { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? NumeroSerie { get; set; }

    public DateOnly FechaEntrega { get; set; }

    public DateOnly? FechaDevolucion { get; set; }

    public string? EstadoEntrega { get; set; }

    public string? Observaciones { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;
}
