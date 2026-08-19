using System;
using System.Collections.Generic;

namespace Vercom.Models;

public partial class Empleado
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public Guid? SucursalId { get; set; }

    public string CarnetIdentidad { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public string? Sexo { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public Guid CargoId { get; set; }

    public string? Calificacion { get; set; }

    public string? NivelEscolaridad { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public DateOnly? FechaBaja { get; set; }

    public string? MotivoBaja { get; set; }

    public string? CuentaBancariaPago { get; set; }

    public string Estado { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }

    public virtual Cargo Cargo { get; set; } = null!;

    public virtual ICollection<CertificadoMedico> CertificadoMedicos { get; set; } = new List<CertificadoMedico>();

    public virtual ICollection<ContratoLaboral> ContratoLaborals { get; set; } = new List<ContratoLaboral>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<NominaDetalle> NominaDetalles { get; set; } = new List<NominaDetalle>();

    public virtual ICollection<RegistroAsistencium> RegistroAsistencia { get; set; } = new List<RegistroAsistencium>();

    public virtual ICollection<RegistroSalarioTiempoServicio> RegistroSalarioTiempoServicios { get; set; } = new List<RegistroSalarioTiempoServicio>();

    public virtual ICollection<SaldoVacacione> SaldoVacaciones { get; set; } = new List<SaldoVacacione>();

    public virtual Sucursal? Sucursal { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
