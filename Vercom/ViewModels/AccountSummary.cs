namespace Vercom.ViewModels;

public class AccountSummary
{
    /// <summary>Código de la cuenta según nomenclador del MFP (ej. "1.1.01.001")</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre de la cuenta (ej. "Caja en Efectivo")</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Saldo calculado de la cuenta para el período (naturaleza: DEUDORA o ACREEDORA)</summary>
    public decimal Saldo { get; set; }

    /// <summary>Formato monetario del saldo (con separadores de miles y dos decimales)</summary>
    public string SaldoFormateado => Saldo.ToString("N2");
}