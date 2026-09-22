using Vercom.Models;

namespace Vercom.ViewModels;

/// <summary>Punto de una serie para gráficos (etiqueta + valor).</summary>
public class ChartSeriesPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

/// <summary>Par nombre/valor para distribuciones (canal, forma de pago, etc.).</summary>
public class MetricValue
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

/// <summary>Fila del ranking de productos más vendidos.</summary>
public class TopProductoItem
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Importe { get; set; }
}

/// <summary>Sesión de caja POS abierta.</summary>
public class PosSesionItem
{
    public string Dispositivo { get; set; } = string.Empty;
    public string Cajero { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalEfectivo { get; set; }
    public int CantidadFacturas { get; set; }
}

/// <summary>Entidad con su volumen de ventas (Monitor Maestro).</summary>
public class TopEntidadItem
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Ventas { get; set; }
    public int Facturas { get; set; }
}

public class DashboardViewModel
{
    // ===== KPIs financieros (null = sin información suficiente para calcular) =====
    public decimal? Liquidez { get; set; }
    public decimal? Rentabilidad { get; set; }
    public decimal? RotacionStock { get; set; }
    public decimal? DiasCobro { get; set; }
    public decimal? DiasPago { get; set; }
    public decimal? Solvencia { get; set; }

    // ===== Ventas y cartera =====
    public decimal VentasHoy { get; set; }
    public int FacturasHoy { get; set; }
    public decimal VentasMes { get; set; }
    public int FacturasMes { get; set; }
    public decimal VentasMesAnterior { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal VariacionVentasPct { get; set; }
    public decimal PorCobrar { get; set; }
    public decimal PorPagar { get; set; }

    // ===== Posición de efectivo =====
    public decimal SaldoCajas { get; set; }
    public decimal SaldoBancos { get; set; }
    public decimal PosicionEfectivo => SaldoCajas + SaldoBancos;

    // ===== Series y distribuciones para gráficos =====
    public List<ChartSeriesPoint> VentasSerie { get; set; } = new();
    public List<ChartSeriesPoint> TrendRentabilidad { get; set; } = new();
    public List<ChartSeriesPoint> TrendLiquidez { get; set; } = new();
    public List<MetricValue> VentasPorCanal { get; set; } = new();
    public List<MetricValue> VentasPorFormaPago { get; set; } = new();
    public List<TopProductoItem> TopProductos { get; set; } = new();

    // ===== Aging de cartera =====
    public decimal CxcVencido { get; set; }
    public decimal CxcPorVencer { get; set; }
    public decimal CxpVencido { get; set; }
    public decimal CxpPorVencer { get; set; }

    // ===== POS operativo =====
    public int PosSesionesAbiertas { get; set; }
    public int PosTerminalesActivas { get; set; }
    public decimal PosVentasHoy { get; set; }
    public int PosPendientesSync { get; set; }
    public int PosConflictos { get; set; }
    public List<PosSesionItem> PosSesiones { get; set; } = new();

    // ===== Alertas =====
    public List<Existencium> AlertasStock { get; set; } = new();
    public List<ExistenciaLote> AlertasVencimiento { get; set; } = new();
    public List<ContratoEconomico> ContratosVencer { get; set; } = new();

    // ===== Contexto =====
    public string PeriodoActual { get; set; } = string.Empty;
    public bool EsMaster { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
