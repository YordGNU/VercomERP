using System;

namespace Vercom.ViewModels;

public class KardexRowViewModel
{
    public DateTimeOffset Fecha { get; set; }
    public string TipoMovimiento { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string Almacen { get; set; } = null!;
    public string? Lote { get; set; }
    public decimal Entrada { get; set; }
    public decimal Salida { get; set; }
    public decimal Saldo { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal ValorSaldo { get; set; }
}
