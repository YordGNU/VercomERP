namespace Vercom.ViewModels;

/// <summary>
/// Balance General (Estado de Situación Financiera) según Normas Cubanas de Contabilidad.
/// Contiene la clasificación tradicional: Activo, Pasivo y Patrimonio.
/// </summary>
public class FinancialStatement
{
    /// <summary>Título del estado (ej. "Balance General al 31/12/2025")</summary>
    public string Titulo { get; set; } = "Balance General";

    /// <summary>Lista de cuentas de activo (corrientes y no corrientes)</summary>
    public List<AccountSummary> Activos { get; set; } = new();

    /// <summary>Lista de cuentas de pasivo (corrientes y no corrientes)</summary>
    public List<AccountSummary> Pasivos { get; set; } = new();

    /// <summary>Lista de cuentas de patrimonio (capital, reservas, resultados)</summary>
    public List<AccountSummary> Patrimonio { get; set; } = new();

    // ============================
    // TOTALES CALCULADOS
    // ============================

    /// <summary>Suma de todos los activos</summary>
    public decimal TotalActivos => Activos.Sum(a => a.Saldo);

    /// <summary>Suma de todos los pasivos</summary>
    public decimal TotalPasivos => Pasivos.Sum(p => p.Saldo);

    /// <summary>Suma de todo el patrimonio</summary>
    public decimal TotalPatrimonio => Patrimonio.Sum(p => p.Saldo);

    /// <summary>Indica si el balance cuadra: Activo = Pasivo + Patrimonio</summary>
    public bool IsBalanced => Math.Abs(TotalActivos - (TotalPasivos + TotalPatrimonio)) < 0.01m;

    /// <summary>Diferencia (útil para depuración o para mostrar en la vista si no cuadra)</summary>
    public decimal Diferencia => TotalActivos - (TotalPasivos + TotalPatrimonio);

    // ============================
    // MÉTODOS AUXILIARES
    // ============================

    /// <summary>
    /// Obtiene una vista plana de todas las cuentas (útil para exportación a Excel/CSV).
    /// </summary>
    public IEnumerable<AccountSummary> TodasLasCuentas()
    {
        return Activos.Concat(Pasivos).Concat(Patrimonio);
    }

    /// <summary>
    /// Obtiene el resumen en formato de texto para depuración o auditoría.
    /// </summary>
    public override string ToString()
    {
        return $"Activos: {TotalActivos:N2}, Pasivos: {TotalPasivos:N2}, Patrimonio: {TotalPatrimonio:N2}, Cuadra: {IsBalanced}";
    }
}