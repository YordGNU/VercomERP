using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IIntelligenceService
{
    // KPIs y Dashboard (RF-60)
    Task<DashboardViewModel> GetDashboardContextAsync(Guid entidadId);
    Task<decimal?> CalculateLiquidityAsync(Guid entidadId, Guid periodId);
    Task<decimal?> CalculateProfitabilityAsync(Guid entidadId, Guid periodId);
    Task<decimal?> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId);
    Task<decimal?> CalculateCollectionDaysAsync(Guid entidadId, Guid periodId);
    Task<decimal?> CalculatePaymentCycleAsync(Guid entidadId, Guid periodId);
    Task<decimal?> CalculateSolvencyAsync(Guid entidadId, Guid periodId);
    Task<(bool Succeeded, string Message)> PersistKpisForPeriodAsync(Guid entidadId, Guid periodId);

    // Estados Financieros (RF-13)
    Task<FinancialReportViewModel> GetBalanceGeneralContextAsync(Guid entidadId, Guid periodId);
    Task<FinancialReportViewModel> GetEstadoResultadosContextAsync(Guid entidadId, Guid periodId);

    // Cierre y Auditoría
    Task<ClosureYearlyViewModel> GetYearlyClosureContextAsync(Guid entidadId, short year);
}

public class IntelligenceService : IIntelligenceService
{
    private static readonly string[] MesesAbrev =
        { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IInventoryService _inventoryService;
    private readonly IEntidadProvider _entidadProvider;

    public IntelligenceService(AppDbContext context, IAccountingService accountingService, IInventoryService inventoryService, IEntidadProvider entidadProvider)
    {
        _context = context;
        _accountingService = accountingService;
        _inventoryService = inventoryService;
        _entidadProvider = entidadProvider;
    }

    public async Task<DashboardViewModel> GetDashboardContextAsync(Guid entidadId)
    {
        // ============================================================
        // 1. CASO MASTER: Sin entidad asociada
        // ============================================================
        if (_entidadProvider.IsMaster)
        {
            return new DashboardViewModel
            {
                EsMaster = true,
                Mensaje = "Usuario Master: No se requiere período fiscal. Seleccione una entidad para ver datos específicos.",
                PeriodoActual = "Consolidado - Master",
                AlertasStock = new List<Existencium>()
            };
        }

        // ============================================================
        // 2. CASO NORMAL: Usuario con entidad específica
        // ============================================================
        var entidadExiste = await _context.Entidads.AnyAsync(e => e.Id == entidadId);
        if (!entidadExiste)
        {
            return new DashboardViewModel
            {
                Mensaje = "La entidad asociada al usuario no existe en el sistema. Contacte al administrador.",
                PeriodoActual = DateTime.Now.ToString("MMMM yyyy")
            };
        }

        var period = await _accountingService.GetOrCreateActivePeriodAsync(entidadId, DateTime.Now);
        var periodId = period?.Id ?? Guid.Empty;
        if (periodId == Guid.Empty)
        {
            return new DashboardViewModel
            {
                Mensaje = "No se pudo obtener o crear un período contable activo.",
                PeriodoActual = DateTime.Now.ToString("MMMM yyyy")
            };
        }

        var now = DateTimeOffset.Now;
        var inicioDia = new DateTimeOffset(now.Date, now.Offset);
        var inicioMes = new DateTimeOffset(new DateTime(now.Year, now.Month, 1), now.Offset);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var hoy = DateOnly.FromDateTime(now.DateTime);

        var vm = new DashboardViewModel
        {
            EsMaster = false,
            PeriodoActual = DateTime.Now.ToString("MMMM yyyy"),
            Liquidez = await CalculateLiquidityAsync(entidadId, periodId),
            Rentabilidad = await CalculateProfitabilityAsync(entidadId, periodId),
            RotacionStock = await CalculateInventoryTurnoverAsync(entidadId, periodId),
            DiasCobro = await CalculateCollectionDaysAsync(entidadId, periodId),
            DiasPago = await CalculatePaymentCycleAsync(entidadId, periodId),
            Solvencia = await CalculateSolvencyAsync(entidadId, periodId)
        };

        // --- Tendencias (una sola pasada agrupada por período) ---
        var trends = await GetTrendsAsync(entidadId, 6);
        vm.TrendRentabilidad = trends.Rentabilidad;
        vm.TrendLiquidez = trends.Liquidez;

        // --- Ventas ---
        var facturasHoy = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.Fecha >= inicioDia)
            .Select(f => new { f.Total })
            .ToListAsync();
        vm.VentasHoy = facturasHoy.Sum(f => f.Total);
        vm.FacturasHoy = facturasHoy.Count;

        var mesActual = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.Fecha >= inicioMes)
            .Select(f => new { f.Total })
            .ToListAsync();
        vm.VentasMes = mesActual.Sum(f => f.Total);
        vm.FacturasMes = mesActual.Count;
        vm.TicketPromedio = vm.FacturasMes > 0 ? Math.Round(vm.VentasMes / vm.FacturasMes, 2) : 0;

        vm.VentasMesAnterior = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.Fecha >= inicioMesAnterior && f.Fecha < inicioMes)
            .SumAsync(f => (decimal?)f.Total) ?? 0;
        vm.VariacionVentasPct = vm.VentasMesAnterior > 0
            ? Math.Round((vm.VentasMes - vm.VentasMesAnterior) / vm.VentasMesAnterior * 100, 1)
            : (vm.VentasMes > 0 ? 100m : 0m);

        // Serie de ventas de los últimos 14 días (agrupada en memoria; acotada por fecha)
        var desde14 = inicioDia.AddDays(-13);
        var ventasRaw = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.Fecha >= desde14)
            .Select(f => new { f.Fecha, f.Total })
            .ToListAsync();
        vm.VentasSerie = Enumerable.Range(0, 14)
            .Select(i =>
            {
                var dia = desde14.AddDays(i).Date;
                return new ChartSeriesPoint
                {
                    Label = dia.ToString("dd/MM"),
                    Value = ventasRaw.Where(v => v.Fecha.LocalDateTime.Date == dia).Sum(v => v.Total)
                };
            })
            .ToList();

        // Distribución del mes por canal de venta
        vm.VentasPorCanal = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.Fecha >= inicioMes)
            .GroupBy(f => f.CanalVenta)
            .Select(g => new MetricValue { Name = g.Key ?? "ERP", Value = g.Sum(x => x.Total) })
            .ToListAsync();

        // Distribución del mes por forma de pago
        vm.VentasPorFormaPago = await _context.FormaPagoVenta
            .Where(p => p.Factura.EntidadId == entidadId && p.Factura.Estado == "EMITIDA" && p.Factura.Fecha >= inicioMes)
            .GroupBy(p => p.FormaPago)
            .Select(g => new MetricValue { Name = g.Key ?? "OTROS", Value = g.Sum(x => x.Monto) })
            .ToListAsync();

        // Top productos del mes
        var topRaw = await _context.FacturaVentaDetalles
            .Where(d => d.Factura.EntidadId == entidadId && d.Factura.Estado == "EMITIDA" && d.Factura.Fecha >= inicioMes)
            .GroupBy(d => d.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad), Importe = g.Sum(x => x.SubtotalLinea) })
            .OrderByDescending(x => x.Importe)
            .Take(8)
            .ToListAsync();
        var topIds = topRaw.Select(t => t.ProductoId).ToList();
        var nombres = await _context.Productos
            .Where(p => topIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Nombre })
            .ToDictionaryAsync(p => p.Id, p => p.Nombre);
        vm.TopProductos = topRaw
            .Select(t => new TopProductoItem
            {
                Nombre = nombres.TryGetValue(t.ProductoId, out var n) ? n : "Producto",
                Cantidad = t.Cantidad,
                Importe = t.Importe
            })
            .ToList();

        // --- Cartera (CxC / CxP) con aging ---
        var cxc = await _context.CuentaPorCobrars
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .Select(c => new { c.SaldoPendiente, c.FechaVencimiento })
            .ToListAsync();
        vm.PorCobrar = cxc.Sum(c => c.SaldoPendiente);
        vm.CxcVencido = cxc.Where(c => c.FechaVencimiento < hoy).Sum(c => c.SaldoPendiente);
        vm.CxcPorVencer = vm.PorCobrar - vm.CxcVencido;

        var cxp = await _context.CuentaPorPagars
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .Select(c => new { c.SaldoPendiente, c.FechaVencimiento })
            .ToListAsync();
        vm.PorPagar = cxp.Sum(c => c.SaldoPendiente);
        vm.CxpVencido = cxp.Where(c => c.FechaVencimiento < hoy).Sum(c => c.SaldoPendiente);
        vm.CxpPorVencer = vm.PorPagar - vm.CxpVencido;

        // --- Posición de efectivo ---
        vm.SaldoCajas = await _context.Cajas
            .Where(c => c.EntidadId == entidadId && c.Activa)
            .SumAsync(c => (decimal?)c.SaldoActual) ?? 0;
        vm.SaldoBancos = await _context.CuentaBancaria
            .Where(c => c.EntidadId == entidadId && c.Activa)
            .SumAsync(c => (decimal?)c.SaldoActual) ?? 0;

        // --- POS operativo ---
        await FillPosAsync(vm, entidadId, inicioDia, now);

        // --- Alertas ---
        vm.AlertasStock = await _inventoryService.GetLowStockAlertsAsync(entidadId);
        vm.AlertasVencimiento = await _inventoryService.GetExpiryAlertsAsync(entidadId, 30);
        var limitDate = hoy.AddDays(15);
        vm.ContratosVencer = await _context.ContratoEconomicos
            .Include(c => c.Cliente).Include(c => c.Proveedor)
            .Where(c => c.EntidadId == entidadId && c.Estado == "VIGENTE" && c.FechaFin != null && c.FechaFin <= limitDate)
            .ToListAsync();

        return vm;
    }

    private async Task FillPosAsync(DashboardViewModel vm, Guid entidadId, DateTimeOffset inicioDia, DateTimeOffset now)
    {
        var dispositivos = await _context.DispositivoPos
            .Where(d => d.EntidadId == entidadId)
            .Select(d => new { d.Id, d.Codigo, d.Nombre, d.UltimaSincronizacion })
            .ToListAsync();
        var dispositivoIds = dispositivos.Select(d => d.Id).ToList();

        vm.PosTerminalesActivas = dispositivos
            .Count(d => d.UltimaSincronizacion != null && d.UltimaSincronizacion >= now.AddHours(-24));

        if (dispositivoIds.Count == 0) return;

        var sesiones = await _context.SesionCajaPos
            .Where(s => dispositivoIds.Contains(s.DispositivoPosId) && s.Estado == "ABIERTA")
            .Include(s => s.Cajero)
            .OrderByDescending(s => s.FechaApertura)
            .ToListAsync();
        vm.PosSesionesAbiertas = sesiones.Count;
        vm.PosSesiones = sesiones.Select(s => new PosSesionItem
        {
            Dispositivo = dispositivos.FirstOrDefault(d => d.Id == s.DispositivoPosId)?.Codigo ?? "N/D",
            Cajero = s.Cajero?.NombreCompleto ?? "N/D",
            FechaApertura = s.FechaApertura,
            MontoApertura = s.MontoApertura,
            TotalVentas = s.TotalVentas,
            TotalEfectivo = s.TotalEfectivo,
            CantidadFacturas = s.CantidadFacturas
        }).ToList();

        var pendientes = await _context.PosVentaPendientes
            .Where(p => dispositivoIds.Contains(p.DispositivoPosId) && p.Estado == "PENDIENTE")
            .Select(p => new { p.MensajeError })
            .ToListAsync();
        vm.PosPendientesSync = pendientes.Count;
        vm.PosConflictos = pendientes.Count(p => !string.IsNullOrEmpty(p.MensajeError));

        vm.PosVentasHoy = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.CanalVenta == "POS" && f.Estado == "EMITIDA" && f.Fecha >= inicioDia)
            .SumAsync(f => (decimal?)f.Total) ?? 0;
    }

    // ============================================================
    // KPIs financieros
    // ============================================================

    public async Task<decimal?> CalculateLiquidityAsync(Guid entidadId, Guid periodId)
    {
        var activos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "ACTIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Debe - d.Haber)) ?? 0;

        var pasivos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "PASIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Haber - d.Debe)) ?? 0;

        // Sin pasivo exigible el índice no está definido (no es infinito ni 0).
        if (pasivos <= 0) return null;
        return Math.Round(activos / pasivos, 2);
    }

    public async Task<decimal?> CalculateProfitabilityAsync(Guid entidadId, Guid periodId)
    {
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "INGRESO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Haber - d.Debe)) ?? 0;

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Debe - d.Haber)) ?? 0;

        if (ingresos == 0) return null;
        return Math.Round((ingresos - gastos) / ingresos * 100, 2);
    }

    public async Task<decimal?> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId)
    {
        var costoVentas = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTO" && d.Cuenta.Codigo.StartsWith("810") && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Debe - d.Haber)) ?? 0;

        var inventario = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "ACTIVO" && d.Cuenta.Codigo.StartsWith("183") && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Debe - d.Haber)) ?? 0;

        if (inventario <= 0) return null;
        return Math.Round(costoVentas / inventario, 2);
    }

    public async Task<decimal?> CalculateCollectionDaysAsync(Guid entidadId, Guid periodId)
    {
        var period = await _context.PeriodoContables.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return null;

        var saldoCxc = await _context.CuentaPorCobrars
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;

        var (inicio, fin) = PeriodBounds(period);
        var ventasCredito = await _context.FacturaVenta
            .Where(f => f.EntidadId == entidadId && f.Estado == "EMITIDA" && f.CuentaPorCobrarId != null
                        && f.Fecha >= inicio && f.Fecha < fin)
            .SumAsync(f => (decimal?)f.Total) ?? 0;

        if (ventasCredito <= 0) return null;
        var dias = (fin.Date - inicio.Date).TotalDays;
        return Math.Round((saldoCxc / ventasCredito) * (decimal)dias, 0);
    }

    public async Task<decimal?> CalculatePaymentCycleAsync(Guid entidadId, Guid periodId)
    {
        var period = await _context.PeriodoContables.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return null;

        var saldoCxp = await _context.CuentaPorPagars
            .Where(c => c.EntidadId == entidadId && (c.Estado == "PENDIENTE" || c.Estado == "PARCIAL"))
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0;

        var comprasCredito = await _context.OrdenCompras
            .Where(f => f.EntidadId == entidadId && f.Estado == "RECIBIDA"
                        && f.Fecha >= period.FechaInicio && f.Fecha <= period.FechaFin)
            .SumAsync(f => (decimal?)f.Total) ?? 0;

        if (comprasCredito <= 0) return null;
        var dias = (period.FechaFin.ToDateTime(TimeOnly.MinValue) - period.FechaInicio.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;
        return Math.Round((saldoCxp / comprasCredito) * (decimal)dias, 0);
    }

    public async Task<decimal?> CalculateSolvencyAsync(Guid entidadId, Guid periodId)
    {
        var activos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "ACTIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Debe - d.Haber)) ?? 0;

        var pasivos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "PASIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => (decimal?)(d.Haber - d.Debe)) ?? 0;

        if (pasivos <= 0) return null;
        return Math.Round(activos / pasivos, 2);
    }

    public async Task<(bool Succeeded, string Message)> PersistKpisForPeriodAsync(Guid entidadId, Guid periodId)
    {
        try
        {
            var kpis = new Dictionary<string, decimal>
            {
                { "LIQUIDEZ_CORRIENTE", await CalculateLiquidityAsync(entidadId, periodId) ?? 0 },
                { "RENTABILIDAD_NETA", await CalculateProfitabilityAsync(entidadId, periodId) ?? 0 },
                { "ROTACION_INVENTARIO", await CalculateInventoryTurnoverAsync(entidadId, periodId) ?? 0 },
                { "CICLO_COBRO", await CalculateCollectionDaysAsync(entidadId, periodId) ?? 0 },
                { "CICLO_PAGO", await CalculatePaymentCycleAsync(entidadId, periodId) ?? 0 },
                { "SOLVENCIA", await CalculateSolvencyAsync(entidadId, periodId) ?? 0 }
            };

            var indicators = await _context.Indicadors.ToListAsync();

            foreach (var kpi in kpis)
            {
                var indicator = indicators.FirstOrDefault(i => i.Codigo == kpi.Key);
                if (indicator == null) continue;

                var existingValue = await _context.IndicadorValors
                    .FirstOrDefaultAsync(v => v.EntidadId == entidadId && v.PeriodoId == periodId && v.IndicadorId == indicator.Id);

                if (existingValue != null)
                {
                    existingValue.Valor = kpi.Value;
                    existingValue.CalculadoEn = DateTimeOffset.Now;
                }
                else
                {
                    _context.IndicadorValors.Add(new IndicadorValor
                    {
                        Id = Guid.NewGuid(),
                        EntidadId = entidadId,
                        PeriodoId = periodId,
                        IndicadorId = indicator.Id,
                        Valor = kpi.Value,
                        CalculadoEn = DateTimeOffset.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return (true, "Indicadores persistidos correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    private static (DateTimeOffset Inicio, DateTimeOffset Fin) PeriodBounds(PeriodoContable period)
    {
        var inicio = new DateTimeOffset(period.FechaInicio.Year, period.FechaInicio.Month, period.FechaInicio.Day, 0, 0, 0, TimeSpan.Zero).ToLocalTime();
        var fin = new DateTimeOffset(period.FechaFin.Year, period.FechaFin.Month, period.FechaFin.Day, 0, 0, 0, TimeSpan.Zero).ToLocalTime().AddDays(1);
        return (inicio, fin);
    }

    private sealed record DashboardTrends(
        List<ChartSeriesPoint> Rentabilidad,
        List<ChartSeriesPoint> Liquidez,
        List<ChartSeriesPoint> Rotacion);

    /// <summary>
    /// Calcula las tendencias de rentabilidad, liquidez y rotación de los últimos
    /// períodos con un conjunto reducido de consultas agrupadas (evita el patrón
    /// de calcular cada KPI período por período).
    /// </summary>
    private async Task<DashboardTrends> GetTrendsAsync(Guid entidadId, int periodsCount)
    {
        var periods = await _context.PeriodoContables
            .Where(p => p.EntidadId == entidadId)
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
            .Take(periodsCount)
            .ToListAsync();
        periods.Reverse();
        var ids = periods.Select(p => p.Id).ToList();

        var sums = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && ids.Contains(d.Asiento.PeriodoId) && d.Asiento.Estado == "CONTABILIZADO")
            .GroupBy(d => new { d.Asiento.PeriodoId, d.Cuenta.Clase })
            .Select(g => new { g.Key.PeriodoId, g.Key.Clase, Debe = g.Sum(x => x.Debe), Haber = g.Sum(x => x.Haber) })
            .ToListAsync();

        var costoVentas = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && ids.Contains(d.Asiento.PeriodoId) && d.Asiento.Estado == "CONTABILIZADO"
                        && d.Cuenta.Clase == "GASTO" && d.Cuenta.Codigo.StartsWith("810"))
            .GroupBy(d => d.Asiento.PeriodoId)
            .Select(g => new { PeriodoId = g.Key, Valor = g.Sum(x => x.Debe - x.Haber) })
            .ToDictionaryAsync(x => x.PeriodoId, x => x.Valor);

        var inventario = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && ids.Contains(d.Asiento.PeriodoId) && d.Asiento.Estado == "CONTABILIZADO"
                        && d.Cuenta.Clase == "ACTIVO" && d.Cuenta.Codigo.StartsWith("183"))
            .GroupBy(d => d.Asiento.PeriodoId)
            .Select(g => new { PeriodoId = g.Key, Valor = g.Sum(x => x.Debe - x.Haber) })
            .ToDictionaryAsync(x => x.PeriodoId, x => x.Valor);

        var rentabilidad = new List<ChartSeriesPoint>();
        var liquidez = new List<ChartSeriesPoint>();
        var rotacion = new List<ChartSeriesPoint>();

        foreach (var p in periods)
        {
            var label = $"{MesesAbrev[p.Mes - 1]} {(p.Anio % 100):00}";

            decimal Clase(string c) => sums.Where(s => s.PeriodoId == p.Id && s.Clase == c).Sum(s => s.Haber - s.Debe);
            decimal ClaseDebe(string c) => sums.Where(s => s.PeriodoId == p.Id && s.Clase == c).Sum(s => s.Debe - s.Haber);

            var ingresos = Clase("INGRESO");
            var gastos = ClaseDebe("GASTO");
            var activos = ClaseDebe("ACTIVO");
            var pasivos = Clase("PASIVO");

            rentabilidad.Add(new ChartSeriesPoint
            {
                Label = label,
                Value = ingresos == 0 ? 0 : Math.Round((ingresos - gastos) / ingresos * 100, 2)
            });
            liquidez.Add(new ChartSeriesPoint
            {
                Label = label,
                Value = pasivos <= 0 ? 0 : Math.Round(activos / pasivos, 2)
            });

            var cv = costoVentas.TryGetValue(p.Id, out var c) ? c : 0;
            var inv = inventario.TryGetValue(p.Id, out var i) ? i : 0;
            rotacion.Add(new ChartSeriesPoint
            {
                Label = label,
                Value = inv <= 0 ? 0 : Math.Round(cv / inv, 2)
            });
        }

        return new DashboardTrends(rentabilidad, liquidez, rotacion);
    }

    public async Task<FinancialReportViewModel> GetBalanceGeneralContextAsync(Guid entidadId, Guid periodId)
    {
        var period = await _context.PeriodoContables
            .Include(p => p.Entidad)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        var statement = new FinancialStatement { Titulo = "Balance General" };
        var accounts = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && (c.Clase == "ACTIVO" || c.Clase == "PASIVO" || c.Clase == "PATRIMONIO"))
            .ToListAsync();

        foreach (var acc in accounts)
        {
            var saldo = await _accountingService.GetAccountBalanceAsync(acc.Id, periodId);
            if (saldo == 0) continue;

            var summary = new AccountSummary { Codigo = acc.Codigo, Nombre = acc.Nombre, Saldo = saldo };

            if (acc.Clase == "ACTIVO") statement.Activos.Add(summary);
            else if (acc.Clase == "PASIVO") statement.Pasivos.Add(summary);
            else statement.Patrimonio.Add(summary);
        }

        return new FinancialReportViewModel
        {
            ReportName = "Balance General",
            EntityName = period?.Entidad?.NombreComercial ?? "N/A",
            PeriodName = period != null ? $"{period.Mes}/{period.Anio}" : "N/A",
            Balance = statement
        };
    }

    public async Task<FinancialReportViewModel> GetEstadoResultadosContextAsync(Guid entidadId, Guid periodId)
    {
        var period = await _context.PeriodoContables
            .Include(p => p.Entidad)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        var results = new List<AccountSummary>();
        var accounts = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && (c.Clase == "INGRESO" || c.Clase == "GASTO"))
            .ToListAsync();

        foreach (var acc in accounts)
        {
            var saldo = await _accountingService.GetAccountBalanceAsync(acc.Id, periodId);
            if (saldo == 0) continue;

            results.Add(new AccountSummary { Codigo = acc.Codigo, Nombre = acc.Nombre, Saldo = saldo });
        }

        return new FinancialReportViewModel
        {
            ReportName = "Estado de Resultados",
            EntityName = period?.Entidad?.NombreComercial ?? "N/A",
            PeriodName = period != null ? $"{period.Mes}/{period.Anio}" : "N/A",
            Resultados = results
        };
    }

    public async Task<ClosureYearlyViewModel> GetYearlyClosureContextAsync(Guid entidadId, short year)
    {
        var openPeriods = await _context.PeriodoContables
            .Where(p => p.EntidadId == entidadId && p.Anio == year && p.Estado == "ABIERTO")
            .Select(p => p.Mes)
            .ToListAsync();

        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "INGRESO")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "GASTO")
            .SumAsync(d => d.Debe - d.Haber);

        var vm = new ClosureYearlyViewModel
        {
            Year = year,
            EstimatedProfit = ingresos - gastos,
            CanClose = !openPeriods.Any()
        };

        if (openPeriods.Any())
        {
            vm.ValidationMessages.Add($"No se puede cerrar. Periodos abiertos: {string.Join(", ", openPeriods)}");
        }

        return vm;
    }
}
