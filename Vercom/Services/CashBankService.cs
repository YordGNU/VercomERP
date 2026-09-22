using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface ICashBankService
{
    // Gestión de Cajas
    Task<IEnumerable<Caja>> GetCajasAsync();
    Task<Caja?> GetCajaByIdAsync(Guid id);
    Task<CajaFormViewModel> GetCajaFormContextAsync(Caja? existing = null);
    Task<(bool Succeeded, string Message)> CreateCajaAsync(Caja caja);
    Task<(bool Succeeded, string Message)> UpdateCajaAsync(Caja caja);

    // Gestión de Cuentas Bancarias
    Task<IEnumerable<CuentaBancarium>> GetBankAccountsAsync();
    Task<CuentaBancarium?> GetBankAccountByIdAsync(Guid id);
    Task<BankAccountFormViewModel> GetBankAccountFormContextAsync(CuentaBancarium? existing = null);
    Task<(bool Succeeded, string Message)> CreateBankAccountAsync(CuentaBancarium account);
    Task<(bool Succeeded, string Message)> UpdateBankAccountAsync(CuentaBancarium account);

    // Movimientos Bancarios
    Task<IEnumerable<MovimientoBancario>> GetBankMovementsAsync();
    Task<(bool Succeeded, string Message)> CreateBankMovementAsync(MovimientoBancario movement);

    // Operaciones
    Task<(bool Succeeded, string Message)> ReconcileBankMovementAsync(Guid movementId, Guid userId);
    Task<(bool Succeeded, string Message)> ValidateCashLimitAsync(Guid cashId, decimal amountToAdd);
    Task<List<MovimientoBancario>> GetPendingReconciliationAsync(Guid bankAccountId);
}

public class CashBankService : ICashBankService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;

    public CashBankService(AppDbContext context, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<Caja>> GetCajasAsync()
    {
        return await _context.Cajas.Include(c => c.CuentaContable).OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Caja?> GetCajaByIdAsync(Guid id)
    {
        return await _context.Cajas
            .Include(c => c.CuentaContable)
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<CajaFormViewModel> GetCajaFormContextAsync(Caja? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentasContables = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento && c.Clase == "ACTIVO"
                       && (c.Codigo.StartsWith("101") || c.Codigo.StartsWith("102")))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        return new CajaFormViewModel
        {
            Caja = existing ?? new Caja { Activa = true },
            CuentasContables = new SelectList(cuentasContables, "Id", "Display"),
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateCajaAsync(Caja caja)
    {
        try
        {
            caja.Id = Guid.NewGuid();
            caja.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync();
            return (true, "Caja creada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCajaAsync(Caja caja)
    {
        try
        {
            var existing = await _context.Cajas.FindAsync(caja.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(caja);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Caja actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<CuentaBancarium>> GetBankAccountsAsync()
    {
        return await _context.CuentaBancaria.Include(c => c.CuentaContable).OrderBy(c => c.Banco).ToListAsync();
    }

    public async Task<CuentaBancarium?> GetBankAccountByIdAsync(Guid id)
    {
        return await _context.CuentaBancaria
            .Include(c => c.CuentaContable)
            .Include(c => c.MovimientoBancarios).ThenInclude(m => m.Asiento)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<BankAccountFormViewModel> GetBankAccountFormContextAsync(CuentaBancarium? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentasContables = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento && c.Clase == "ACTIVO"
                       && (c.Codigo.StartsWith("109") || c.Codigo.StartsWith("111")))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        return new BankAccountFormViewModel
        {
            BankAccount = existing ?? new CuentaBancarium { Activa = true },
            CuentasContables = new SelectList(cuentasContables, "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateBankAccountAsync(CuentaBancarium account)
    {
        try
        {
            account.Id = Guid.NewGuid();
            account.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.CuentaBancaria.Add(account);
            await _context.SaveChangesAsync();
            return (true, "Cuenta bancaria registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateBankAccountAsync(CuentaBancarium account)
    {
        try
        {
            var existing = await _context.CuentaBancaria.FindAsync(account.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(account);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Cuenta bancaria actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<MovimientoBancario>> GetBankMovementsAsync()
    {
        return await _context.MovimientoBancarios.Include(m => m.CuentaBancaria).Include(m => m.Asiento).OrderByDescending(m => m.Fecha).ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> CreateBankMovementAsync(MovimientoBancario movement)
    {
        try
        {
            movement.Id = Guid.NewGuid();
            _context.MovimientoBancarios.Add(movement);
            await _context.SaveChangesAsync();
            return (true, "Movimiento registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
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
