using Vercom.Models;

namespace Vercom.ViewModels;

public class AttendanceConsoleViewModel
{
    public DateOnly Date { get; set; }
    public List<AttendanceRow> Rows { get; set; } = new();
    public IEnumerable<TipoAusencium> TiposAusencia { get; set; } = new List<TipoAusencium>();
    public string Title { get; set; } = "Consola de Asistencia Diaria";
}

public class AttendanceRow
{
    public Guid EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public RegistroAsistencium Record { get; set; } = new();
}
