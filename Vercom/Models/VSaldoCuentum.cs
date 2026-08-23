namespace Vercom.Models;

public partial class VSaldoCuentum
{
    public Guid EntidadId { get; set; }

    public Guid CuentaId { get; set; }

    public string CodigoCuenta { get; set; } = null!;

    public string NombreCuenta { get; set; } = null!;

    public string Clase { get; set; } = null!;

    public string Naturaleza { get; set; } = null!;

    public Guid PeriodoId { get; set; }

    public short Anio { get; set; }

    public short Mes { get; set; }

    public decimal? TotalDebe { get; set; }

    public decimal? TotalHaber { get; set; }

    public decimal? Saldo { get; set; }
}
