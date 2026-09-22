using Vercom.Models;

namespace Vercom.ViewModels;

public class MasterDashboardViewModel
{
    public int TotalEntidades { get; set; }
    public int EntidadesPendientes { get; set; }
    public int UsuariosTotales { get; set; }
    public int SesionesPosActivas { get; set; }
    public int DispositivosPos { get; set; }

    public decimal VentasGlobalesMes { get; set; }
    public int FacturasGlobalesMes { get; set; }
    public int PosPendientes { get; set; }
    public int PosConflictos { get; set; }

    public BackupLog? UltimoBackup { get; set; }
    public List<Auditorium> AlertasSeguridad { get; set; } = new();
    public List<ExistenciaLote> AlertasVencimiento { get; set; } = new();
    public List<TopEntidadItem> TopEntidades { get; set; } = new();
    public List<PosSesionItem> SesionesPos { get; set; } = new();
}
