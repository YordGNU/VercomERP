// ViewModels/EmpleadoExpedienteViewModel.cs
using Vercom.Models;

namespace Vercom.ViewModels;

public class EmpleadoExpedienteViewModel
{
    // Información del empleado
    public Empleado Empleado { get; set; } = new();

    // Estadísticas calculadas
    public int AntiguedadAnios { get; set; }
    public int Edad { get; set; }
    public decimal SaldoVacaciones { get; set; }
    public int ContratosVigentes { get; set; }
    public int DiasAusenciaMes { get; set; }
    public decimal PromedioSalarial { get; set; }

    // Listas para los tabs
    public List<ContratoLaboral> Contratos { get; set; } = new();
    public List<RegistroAsistencium> Asistencias { get; set; } = new();
    public List<CertificadoMedico> Certificados { get; set; } = new();
    public List<UtileResponsabilidad> Medios { get; set; } = new();
    public List<NominaDetalle> Nominas { get; set; } = new();

    // Estadísticas por tab
    public int TotalContratos { get; set; }
    public decimal TotalDevengadoAnio { get; set; }
    public int DiasVacacionesTomados { get; set; }
}