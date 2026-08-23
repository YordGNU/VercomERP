namespace Vercom.Models;

public partial class TipoAusencium
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool Remunerada { get; set; }

    public bool AfectaVacaciones { get; set; }

    public virtual ICollection<RegistroAsistencium> RegistroAsistencia { get; set; } = new List<RegistroAsistencium>();
}
