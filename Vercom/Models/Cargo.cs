namespace Vercom.Models;

public partial class Cargo
{
    public Guid Id { get; set; }

    public Guid EntidadId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? CategoriaOcupacional { get; set; }

    public string? Funciones { get; set; }

    public decimal? SalarioEscalaMin { get; set; }

    public decimal? SalarioEscalaMax { get; set; }

    public virtual ICollection<ContratoLaboral> ContratoLaborals { get; set; } = new List<ContratoLaboral>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Entidad Entidad { get; set; } = null!;

    public virtual ICollection<PlantillaAprobadum> PlantillaAprobada { get; set; } = new List<PlantillaAprobadum>();
}
