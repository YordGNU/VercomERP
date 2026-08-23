namespace Vercom.Models;

public partial class Indicador
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public string FormulaDescripcion { get; set; } = null!;

    public virtual ICollection<IndicadorValor> IndicadorValors { get; set; } = new List<IndicadorValor>();
}
