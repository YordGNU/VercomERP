namespace Vercom.ViewModels;

public class AttendanceMonthlyRow
{
    public Guid EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public string? Sucursal { get; set; }
    public string? Turno { get; set; }
    public int Asistencias { get; set; }
    public int Vacaciones { get; set; }
    public int Incapacidades { get; set; }
    public int OtrasAusencias { get; set; }
    public decimal HorasExtra { get; set; }
    public int CantidadRetardos { get; set; }
    public int MinutosRetardo { get; set; }
    public int CantidadSalidasTempranas { get; set; }
    public decimal ImporteSubsidio { get; set; }
}

public class AttendanceMonthlyReportViewModel
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public Guid? SucursalId { get; set; }
    public List<AttendanceMonthlyRow> Rows { get; set; } = new();

    public int TotalAsistencias => Rows.Sum(r => r.Asistencias);
    public int TotalVacaciones => Rows.Sum(r => r.Vacaciones);
    public int TotalIncapacidades => Rows.Sum(r => r.Incapacidades);
    public int TotalOtrasAusencias => Rows.Sum(r => r.OtrasAusencias);
    public decimal TotalHorasExtra => Rows.Sum(r => r.HorasExtra);
    public int TotalCantidadRetardos => Rows.Sum(r => r.CantidadRetardos);
    public int TotalMinutosRetardo => Rows.Sum(r => r.MinutosRetardo);
    public int TotalSalidasTempranas => Rows.Sum(r => r.CantidadSalidasTempranas);
    public decimal TotalSubsidio => Rows.Sum(r => r.ImporteSubsidio);
}
