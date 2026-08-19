using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class SesionCajaPo
{
    public Guid Id { get; set; }

    public Guid DispositivoPosId { get; set; }

    public Guid CajeroId { get; set; }

    public DateTimeOffset FechaApertura { get; set; }

    public decimal MontoApertura { get; set; }

    public DateTimeOffset? FechaCierre { get; set; }

    public decimal? MontoCierreDeclarado { get; set; }

    public decimal? MontoCierreSistema { get; set; }

    public decimal? DiferenciaArqueo { get; set; }

    public decimal TotalVentas { get; set; }

    public decimal TotalEfectivo { get; set; }

    public decimal TotalTransfermovil { get; set; }

    public decimal TotalEnzona { get; set; }

    public decimal TotalOtrosMedios { get; set; }

    public int CantidadFacturas { get; set; }

    public string Estado { get; set; } = null!;

    public Guid? AsientoCierreId { get; set; }

    public string? ObservacionesCierre { get; set; }

    public Guid? SupervisorConciliacionId { get; set; }

    public virtual AsientoContable? AsientoCierre { get; set; }

    public virtual Usuario Cajero { get; set; } = null!;

    public virtual DispositivoPo DispositivoPos { get; set; } = null!;

    public virtual ICollection<FacturaVentum> FacturaVenta { get; set; } = new List<FacturaVentum>();

    public virtual ICollection<MovimientoCajaPo> MovimientoCajaPos { get; set; } = new List<MovimientoCajaPo>();

    public virtual ICollection<PosVentaPendiente> PosVentaPendientes { get; set; } = new List<PosVentaPendiente>();

    public virtual Usuario? SupervisorConciliacion { get; set; }
}
