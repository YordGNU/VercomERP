using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;

namespace Vercom.Services;

public interface ITaxService
{
    Task<decimal> CalculateSalesTaxAsync(Guid entidadId, decimal amount);
    Task<decimal> CalculateSocialSecurityContributionAsync(Guid entidadId, decimal totalSalary);
    Task<decimal> CalculateWorkforceTaxAsync(Guid entidadId, decimal totalSalary);
    Task<decimal> CalculateIncomeTaxAsync(Guid entidadId, Guid periodId);
    Task<Result> GenerateTaxDeclarationAsync(Guid entidadId, int tipoObligacionId, Guid periodoId);
    Task<(bool Succeeded, string Message)> RegisterPresentationAsync(Guid declarationId, string djNumber, DateOnly presentationDate);
}
public class Result
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? Data { get; set; }

    public static Result Success(Guid? data = null, string message = "Operación exitosa")
        => new Result { Succeeded = true, Message = message, Data = data };

    public static Result Failure(string message)
        => new Result { Succeeded = false, Message = message };
}

public class TaxService : ITaxService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IParametroSistemaService _paramService;

    public TaxService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IParametroSistemaService paramService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _paramService = paramService;
    }

    public async Task<decimal> CalculateSalesTaxAsync(Guid entidadId, decimal amount)
    {
        var tasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TAX_VENTA");
        return amount * tasa;
    }

    public async Task<decimal> CalculateSocialSecurityContributionAsync(Guid entidadId, decimal totalSalary)
    {
        var tasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "RET_SS_TRAB");
        if (tasa == 0) tasa = 0.05m; // Fallback
        return totalSalary * tasa;
    }

    public async Task<decimal> CalculateWorkforceTaxAsync(Guid entidadId, decimal totalSalary)
    {
        var tasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TAX_FUERZA_TRAB");
        if (tasa == 0) tasa = 0.05m;
        return totalSalary * tasa;
    }

    public async Task<decimal> CalculateIncomeTaxAsync(Guid entidadId, Guid periodId)
    {
        var ingresos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "INGRESOS" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Haber - d.Debe);

        var gastos = await _context.AsientoDetalles
            .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodId && d.Cuenta.Clase == "GASTOS" && d.Asiento.Estado == "CONTABILIZADO")
            .SumAsync(d => d.Debe - d.Haber);

        var utilidad = ingresos - gastos;
        if (utilidad <= 0) return 0;

        var tasa = await _paramService.ObtenerValorNumericoVigenteAsync(entidadId, "TAX_UTILIDAD");
        if (tasa == 0) tasa = 0.35m;

        return utilidad * tasa;
    }

    public async Task<Result> GenerateTaxDeclarationAsync(Guid entidadId, int tipoObligacionId, Guid periodoId)
    {
        if (entidadId == Guid.Empty) return Result.Failure("La entidad no es válida.");
        if (periodoId == Guid.Empty) return Result.Failure("El período contable no es válido.");

        var existing = await _context.DeclaracionJurada
            .FirstOrDefaultAsync(d => d.EntidadId == entidadId
                                      && d.TipoObligacionId == tipoObligacionId
                                      && d.PeriodoId == periodoId);

        if (existing != null) return Result.Success(existing.Id, "La declaración ya existe.");

        var tipoObligacion = await _context.TipoObligacionFiscals.FindAsync(tipoObligacionId);
        if (tipoObligacion == null) return Result.Failure("El tipo de obligación fiscal no existe.");

        var periodo = await _context.PeriodoContables.FindAsync(periodoId);
        if (periodo == null) return Result.Failure("El período contable no existe.");

        decimal baseImponible = 0;
        if (tipoObligacion.Codigo.Contains("VENTA"))
        {
            baseImponible = await _context.AsientoDetalles
                .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.PeriodoId == periodoId && d.Cuenta.Clase == "INGRESOS")
                .SumAsync(d => d.Haber - d.Debe);
        }
        else if (tipoObligacion.Codigo.Contains("UTILIDAD"))
        {
            baseImponible = await CalculateIncomeTaxAsync(entidadId, periodoId) / 0.35m;
        }

        var tasa = tipoObligacion.TasaActual ?? 0m;
        var montoCalculado = baseImponible * (tasa / 100m);

        var fechaLimite = new DateOnly(periodo.Anio, periodo.Mes, DateTime.DaysInMonth(periodo.Anio, periodo.Mes));

        var declaracion = new DeclaracionJuradum
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            TipoObligacionId = tipoObligacionId,
            PeriodoId = periodoId,
            BaseImponible = baseImponible,
            MontoCalculado = montoCalculado,
            MontoPagado = 0m,
            FechaLimite = fechaLimite,
            Estado = "PENDIENTE",
            GeneradoPor = GetCurrentUserId(),
            CreadoEn = DateTimeOffset.UtcNow
        };

        _context.DeclaracionJurada.Add(declaracion);
        await _context.SaveChangesAsync();

        return Result.Success(declaracion.Id, "Declaración generada correctamente.");
    }

    public async Task<(bool Succeeded, string Message)> RegisterPresentationAsync(Guid declarationId, string djNumber, DateOnly presentationDate)
    {
        var dj = await _context.DeclaracionJurada.FindAsync(declarationId);
        if (dj == null) return (false, "Declaración no encontrada.");

        dj.NumeroDj = djNumber;
        dj.FechaPresentacion = presentationDate;
        dj.Estado = "PRESENTADA";

        await _context.SaveChangesAsync();
        return (true, "Presentación ante la ONAT registrada correctamente.");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }
}
