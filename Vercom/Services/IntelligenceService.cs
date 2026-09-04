using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IIntelligenceService
{
    // KPIs y Dashboard (RF-60)
    Task<DashboardViewModel> GetDashboardContextAsync(Guid entidadId);
    Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId);
    Task<decimal> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId);

    // Estados Financieros (RF-13)
    Task<FinancialReportViewModel> GetBalanceGeneralContextAsync(Guid entidadId, Guid periodId);
    Task<FinancialReportViewModel> GetEstadoResultadosContextAsync(Guid entidadId, Guid periodId);

    // Cierre y Auditoría
    Task<ClosureYearlyViewModel> GetYearlyClosureContextAsync(Guid entidadId, short year);
}

public class IntelligenceService : IIntelligenceService
{
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
                FechaCierreCaja = DateTime.Now.ToString("dd/MM/yyyy"),
                PeriodoActual = "Consolidado - Master",
                AlertasStock = new List<Existencium>(), // o cargar consolidado si existe
                Liquidez = 0,
                Rentabilidad = 0,
                RotacionStock = 0
            };
        }

        // ============================================================
        // 2. CASO NORMAL: Usuario con entidad específica
        // ============================================================
        // Validar que la entidad exista antes de intentar crear el período
        var entidadExiste = await _context.Entidads.AnyAsync(e => e.Id == entidadId);
        if (!entidadExiste)
        {
            return new DashboardViewModel
            {
                EsMaster = false,
                Mensaje = "La entidad asociada al usuario no existe en el sistema. Contacte al administrador.",
                FechaCierreCaja = DateTime.Now.ToString("dd/MM/yyyy"),
                PeriodoActual = DateTime.Now.ToString("MMMM yyyy"),
                AlertasStock = new List<Existencium>()
            };
        }

        var period = await _accountingService.GetOrCreateActivePeriodAsync(entidadId, DateTime.Now);
        var periodId = period?.Id ?? Guid.Empty;

        if (periodId == Guid.Empty)
        {
            return new DashboardViewModel
            {
                EsMaster = false,
                Mensaje = "No se pudo obtener o crear un período contable activo.",
                FechaCierreCaja = DateTime.Now.ToString("dd/MM/yyyy"),
                PeriodoActual = DateTime.Now.ToString("MMMM yyyy"),
                AlertasStock = new List<Existencium>()
            };
        }

        // Calcular indicadores
        return new DashboardViewModel
        {
            EsMaster = false,
            Liquidez = await CalculateLiquidityAsync(entidadId, periodId),
            Rentabilidad = await CalculateProfitabilityAsync(entidadId, periodId),
            RotacionStock = await CalculateInventoryTurnoverAsync(entidadId, periodId),
            FechaCierreCaja = DateTime.Now.ToString("dd/MM/yyyy"),
            PeriodoActual = DateTime.Now.ToString("MMMM yyyy"),
            TrendRentabilidad = await GetTrendDataAsync(entidadId, "RENTABILIDAD", 6),
            TrendLiquidez = await GetTrendDataAsync(entidadId, "LIQUIDEZ", 6),
            AlertasStock = await _inventoryService.GetLowStockAlertsAsync(entidadId)
        };
    }

    public async Task<decimal> CalculateLiquidityAsync(Guid entidadId, Guid periodId)
    {
        var activos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "ACTIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        var pasivos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "PASIVO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Haber - d.Debe);

        if (pasivos == 0) return activos > 0 ? 9.99m : 0;
        return Math.Round(activos / pasivos, 2);
    }

    public async Task<decimal> CalculateProfitabilityAsync(Guid entidadId, Guid periodId)
    {
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "INGRESO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTO" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        if (ingresos == 0) return 0;
        return Math.Round((ingresos - gastos) / ingresos * 100, 2);
    }

    public async Task<decimal> CalculateInventoryTurnoverAsync(Guid entidadId, Guid periodId)
    {
        // Costo de Ventas (Cuentas de Gasto asociadas a ventas)
        var costoVentas = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTO" && d.Cuenta.Codigo.StartsWith("810") && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        // Inventario Promedio (Cuentas de Activo de Inventario)
        var inventario = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "ACTIVO" && d.Cuenta.Codigo.StartsWith("181") && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        if (inventario <= 0) return 0;
        return Math.Round(costoVentas / inventario, 2);
    }

    private async Task<List<decimal>> GetTrendDataAsync(Guid entidadId, string kpiType, int periodsCount)
    {
        var lastPeriods = await _context.PeriodoContables
            .Where(p => p.EntidadId == entidadId)
            .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
            .Take(periodsCount)
            .Reverse()
            .ToListAsync();

        var trend = new List<decimal>();
        foreach (var p in lastPeriods)
        {
            decimal val = kpiType switch
            {
                "LIQUIDEZ" => await CalculateLiquidityAsync(entidadId, p.Id),
                "RENTABILIDAD" => await CalculateProfitabilityAsync(entidadId, p.Id),
                _ => await CalculateInventoryTurnoverAsync(entidadId, p.Id)
            };
            trend.Add(val);
        }
        return trend;
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
