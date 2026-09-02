using Vercom.Models;

namespace Vercom.ViewModels;

public class MasterDashboardViewModel
{
    public int TotalEntidades { get; set; }
    public int EntidadesPendientes { get; set; }
    public int UsuariosTotales { get; set; }
    public int SesionesPosActivas { get; set; }
    public BackupLog? UltimoBackup { get; set; }
    public List<Auditorium> AlertasSeguridad { get; set; } = new();
    public List<ExistenciaLote> AlertasVencimiento { get; set; } = new();
}
