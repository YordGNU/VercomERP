using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class DispositivoPo
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid SucursalId { get; set; }

    public Guid AlmacenId { get; set; }

    public Guid? ApiClienteId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? IdentificadorHardware { get; set; }

    public Guid CajaId { get; set; }

    public DateTimeOffset? UltimaSincronizacion { get; set; }

    public string? VersionAppPos { get; set; }

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Almacen Almacen { get; set; } = null!;

    public virtual ApiCliente? ApiCliente { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual Caja Caja { get; set; } = null!;

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    public virtual ICollection<PosRangoNumeracion> PosRangoNumeracions { get; set; } = new List<PosRangoNumeracion>();

    public virtual ICollection<PosSyncLog> PosSyncLogs { get; set; } = new List<PosSyncLog>();

    public virtual ICollection<PosVentaPendiente> PosVentaPendientes { get; set; } = new List<PosVentaPendiente>();

    public virtual SesionCajaPo? SesionCajaPo { get; set; }

    public virtual Sucursal Sucursal { get; set; } = null!;
}
