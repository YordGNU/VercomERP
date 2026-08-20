using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IContractService
{
    Task<bool> IsValidContractAsync(Guid? contractId, Guid entidadId);
    Task<(bool Succeeded, string Message)> ValidateContractForOperationAsync(Guid? contractId, Guid entidadId, string operationType);
}

public class ContractService : IContractService
{
   private readonly AppDbContext _context;  

    public ContractService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsValidContractAsync(Guid? contractId, Guid entidadId)
    {
        if (contractId == null) return false;

        var contract = await _context.ContratoEconomicos
            .FirstOrDefaultAsync(c => c.Id == contractId && c.EntidadId == entidadId);

        if (contract == null) return false;

        var today = DateOnly.FromDateTime(DateTime.Now);
        return contract.Estado == "VIGENTE" &&
               contract.FechaInicio <= today &&
               (contract.FechaFin == null || contract.FechaFin >= today);
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
