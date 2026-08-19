using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Sucursal
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Municipio { get; set; }

    public string? Provincia { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public virtual ICollection<ActivoFijo> ActivoFijos { get; set; } = new List<ActivoFijo>();

    public virtual ICollection<Almacen> Almacens { get; set; } = new List<Almacen>();

    public virtual ICollection<AsientoContable> AsientoContables { get; set; } = new List<AsientoContable>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<CentroCosto> CentroCostos { get; set; } = new List<CentroCosto>();

    public virtual ICollection<Consecutivo> Consecutivos { get; set; } = new List<Consecutivo>();

    public virtual ICollection<DispositivoPo> DispositivoPos { get; set; } = new List<DispositivoPo>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<PlantillaAprobadum> PlantillaAprobada { get; set; } = new List<PlantillaAprobadum>();

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
