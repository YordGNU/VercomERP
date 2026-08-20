using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface ICashBankService
{
    Task<(bool Succeeded, string Message)> ReconcileBankMovementAsync(Guid movementId, Guid userId);
    Task<(bool Succeeded, string Message)> ValidateCashLimitAsync(Guid cashId, decimal amountToAdd);
    Task<List<MovimientoBancario>> GetPendingReconciliationAsync(Guid bankAccountId);
}

public class CashBankService : ICashBankService
{
    private readonly AppDbContext _context;

    public CashBankService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Succeeded, string Message)> ReconcileBankMovementAsync(Guid movementId, Guid userId)
    {
        var movement = await _context.MovimientoBancarios.FindAsync(movementId);
        if (movement == null) return (false, "Movimiento no encontrado.");
        if (movement.Conciliado) return (false, "El movimiento ya está conciliado.");

        movement.Conciliado = true;
        movement.FechaConciliacion = DateOnly.FromDateTime(DateTime.Now);

        await _context.SaveChangesAsync();
        return (true, "Movimiento conciliado correctamente.");
    }

    public async Task<(bool Succeeded, string Message)> ValidateCashLimitAsync(Guid cashId, decimal amountToAdd)
    {
        var cash = await _context.Cajas.FindAsync(cashId);
        if (cash == null) return (false, "Caja no encontrada.");

        if (cash.SaldoActual + amountToAdd > cash.LimiteEfectivo)
        {
            return (false, $"Operación rechazada por control interno. El ingreso de {amountToAdd:C} excede el límite de efectivo configurado ({cash.LimiteEfectivo:C}).");
        }

        return (true, "Dentro del límite permitido.");
    }

    public async Task<List<MovimientoBancario>> GetPendingReconciliationAsync(Guid bankAccountId)
    {
        return await _context.MovimientoBancarios
            .Where(m => m.CuentaBancariaId == bankAccountId && !m.Conciliado)
            .OrderBy(m => m.Fecha)
            .ToListAsync();
    }
}
