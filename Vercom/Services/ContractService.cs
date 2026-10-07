using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IContractService
{
    Task<bool> IsValidContractAsync(Guid? contractId, Guid entidadId);
    Task<(bool Succeeded, string Message)> ValidateContractForOperationAsync(Guid? contractId, Guid entidadId, string operationType);
    Task<ContractVigencia?> GetVigenciaEfectivaAsync(Guid contratoId, Guid entidadId, CancellationToken cancellationToken = default);
    Task<DateOnly?> GetFechaFinEfectivaAsync(Guid? contratoId, Guid entidadId, CancellationToken cancellationToken = default);
    Task<bool> TieneSuplementosAsync(Guid contratoId, CancellationToken cancellationToken = default);
}

public sealed record ContractVigencia(
    DateOnly FechaInicio,
    DateOnly? FechaFinOriginal,
    DateOnly? FechaFinEfectiva,
    decimal? MontoTotalOriginal,
    decimal? MontoTotalEfectivo,
    Guid? SuplementoMontoId,
    int? NumeroSuplementoMonto,
    int CantidadSuplementos,
    int? UltimoNumeroSuplemento,
    DateOnly? UltimoSuplementoFechaFin);

public class ContractService : IContractService
{
    private readonly AppDbContext _context;

    public ContractService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContractVigencia?> GetVigenciaEfectivaAsync(Guid contratoId, Guid entidadId, CancellationToken cancellationToken = default)
    {
        var contrato = await _context.ContratoEconomicos
            .AsNoTracking()
            .Where(c => c.Id == contratoId && c.EntidadId == entidadId)
            .Select(c => new { c.Id, c.FechaInicio, c.FechaFin, c.FechaFinOriginal, c.MontoTotal, c.MontoTotalOriginal })
            .FirstOrDefaultAsync(cancellationToken);

        if (contrato == null) return null;

        var ultimoVigente = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId && s.Estado == "VIGENTE" && s.FechaFin != null)
            .OrderByDescending(s => s.FechaFin)
            .ThenByDescending(s => s.NumeroSuplemento)
            .Select(s => new { s.Id, s.NumeroSuplemento, s.FechaFin })
            .FirstOrDefaultAsync(cancellationToken);

        var suplementoMonto = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId && s.Estado == "VIGENTE" && s.MontoTotalNuevo != null)
            .OrderByDescending(s => s.NumeroSuplemento)
            .Select(s => new { s.Id, s.NumeroSuplemento, s.MontoTotalNuevo })
            .FirstOrDefaultAsync(cancellationToken);

        var cantidad = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .CountAsync(s => s.ContratoId == contratoId && s.Estado == "VIGENTE", cancellationToken);

        var finEfectivo = ultimoVigente?.FechaFin ?? contrato.FechaFin;
        var montoEfectivo = suplementoMonto?.MontoTotalNuevo ?? contrato.MontoTotal;

        return new ContractVigencia(
            contrato.FechaInicio,
            contrato.FechaFinOriginal ?? contrato.FechaFin,
            finEfectivo,
            contrato.MontoTotalOriginal ?? contrato.MontoTotal,
            montoEfectivo,
            suplementoMonto?.Id,
            suplementoMonto?.NumeroSuplemento,
            cantidad,
            ultimoVigente?.NumeroSuplemento,
            ultimoVigente?.FechaFin);
    }

    public async Task<DateOnly?> GetFechaFinEfectivaAsync(Guid? contratoId, Guid entidadId, CancellationToken cancellationToken = default)
    {
        if (!contratoId.HasValue || contratoId.Value == Guid.Empty) return null;

        var vigencia = await GetVigenciaEfectivaAsync(contratoId.Value, entidadId, cancellationToken);
        return vigencia?.FechaFinEfectiva;
    }

    public async Task<bool> TieneSuplementosAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        return await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .AnyAsync(s => s.ContratoId == contratoId, cancellationToken);
    }

    public async Task<bool> IsValidContractAsync(Guid? contractId, Guid entidadId)
    {
        if (contractId == null) return false;

        var vigencia = await GetVigenciaEfectivaAsync(contractId.Value, entidadId);
        if (vigencia == null) return false;

        var contract = await _context.ContratoEconomicos
            .AsNoTracking()
            .Where(c => c.Id == contractId && c.EntidadId == entidadId)
            .Select(c => c.Estado)
            .FirstOrDefaultAsync();

        if (contract == null) return false;

        var today = DateOnly.FromDateTime(DateTime.Now);
        return contract == "VIGENTE" &&
               vigencia.FechaInicio <= today &&
               (vigencia.FechaFinEfectiva == null || vigencia.FechaFinEfectiva.Value >= today);
    }

    public async Task<(bool Succeeded, string Message)> ValidateContractForOperationAsync(Guid? contractId, Guid entidadId, string operationType)
    {
        // Operaciones minoristas (B2C) no requieren contrato por defecto
        if (operationType == "MINORISTA") return (true, "Venta minorista no requiere contrato.");

        if (contractId == null)
            return (false, "La operación mayorista requiere un contrato económico vigente.");

        if (await IsValidContractAsync(contractId, entidadId))
            return (true, "Contrato válido.");

        return (false, "El contrato seleccionado no está vigente o no pertenece a la entidad.");
    }
}