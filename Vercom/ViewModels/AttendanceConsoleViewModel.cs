using Vercom.Models;

namespace Vercom.ViewModels;

public class AttendanceConsoleViewModel
{
    public DateOnly Date { get; set; }
    public List<AttendanceRow> Rows { get; set; } = new();
    public IEnumerable<TipoAusencium> TiposAusencia { get; set; } = new List<TipoAusencium>();
    public int? IncapacidadTipoAusenciaId { get; set; }
    public string Title { get; set; } = "Consola de Asistencia Diaria";
}

public class AttendanceRow
{
    public Guid EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public Guid? TurnoTrabajoId { get; set; }
    public string? TurnoNombre { get; set; }
    public TimeOnly? TurnoHoraEntrada { get; set; }
    public TimeOnly? TurnoHoraSalida { get; set; }
    public bool EsNocturno { get; set; }
    public int? RetardoMinutos { get; set; }
    public int? SalidaTempranaMinutos { get; set; }
    public bool EsIncapacidad { get; set; }
    public decimal? PorcentajeSubsidio { get; set; }
    public RegistroAsistencium Record { get; set; } = new();
}
