using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IAccountingService
{
    // Gestión de Periodos
    Task<IEnumerable<PeriodoContable>> GetPeriodsAsync();
    Task<AsientoIndexViewModel> GetAsientoIndexContextAsync(Guid? periodId);
    Task<AsientoContable?> GetEntryByIdAsync(Guid id);
    Task<AsientoCreateViewModel> GetAsientoCreateContextAsync(AsientoContable? existing = null);
    Task<decimal> GetAccountBalanceAsync(Guid accountId, Guid? periodId = null);
    Task<List<AsientoContable>> GetEntriesByPeriodAsync(Guid periodId);
    Task<PeriodoContable?> GetOrCreateActivePeriodAsync(Guid entidadId, DateTime date);

    // Gestión de Catálogo (Plan de Cuentas RF-10)
    Task<IEnumerable<CuentaContable>> GetAccountsAsync();
    Task<CuentaContable?> GetAccountByIdAsync(Guid id);
    Task<AccountFormViewModel> GetAccountFormContextAsync(CuentaContable? existing = null);
    Task<(bool Succeeded, string Message)> CreateAccountAsync(CuentaContable account);
    Task<(bool Succeeded, string Message)> UpdateAccountAsync(CuentaContable account);
    Task<(bool Succeeded, string Message)> DeleteAccountAsync(Guid id);

    // Gestión de Centros de Costo
    Task<IEnumerable<CentroCosto>> GetCostCentersAsync();
    Task<CentroCosto?> GetCostCenterByIdAsync(Guid id);
    Task<CentroCostoFormViewModel> GetCostCenterFormContextAsync(CentroCosto? existing = null);
    Task<(bool Succeeded, string Message)> CreateCostCenterAsync(CentroCosto costCenter);
    Task<(bool Succeeded, string Message)> UpdateCostCenterAsync(CentroCosto costCenter);

    // Gestión de Tipos de Comprobante
    Task<IEnumerable<TipoComprobante>> GetVoucherTypesAsync();

    // Escritura Asientos
    Task<(bool Succeeded, string Message, AsientoContable? Entry)> CreateEntryAsync(AsientoContable entry);
    Task<(bool Succeeded, string Message)> PostEntryAsync(Guid entryId);
    Task<(bool Succeeded, string Message, AsientoContable? Adjustment)> ReverseEntryAsync(Guid entryId, string reason);
    Task<(bool Succeeded, string Message)> DeleteDraftEntryAsync(Guid id);
    Task<(bool Succeeded, string Message)> ClosePeriodAsync(Guid periodId, Guid userId);
    Task<(bool Succeeded, string Message)> ReopenPeriodAsync(Guid periodId, string reason, Guid userId);
}

public class AccountingService : IAccountingService
{
    private readonly AppDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Security.IEntidadProvider _entidadProvider;

    public AccountingService(AppDbContext context, IServiceScopeFactory scopeFactory, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _scopeFactory = scopeFactory;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<PeriodoContable>> GetPeriodsAsync()
    {
        return await _context.PeriodoContables
            .OrderByDescending(p => p.Anio)
            .ThenByDescending(p => p.Mes)
            .ToListAsync();
    }

    public async Task<AsientoIndexViewModel> GetAsientoIndexContextAsync(Guid? periodId)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;

        if (periodId == null)
        {
            var currentPeriod = await GetOrCreateActivePeriodAsync(entidadId, DateTime.Now);
            periodId = currentPeriod?.Id;
        }

        return new AsientoIndexViewModel
        {
            SelectedPeriodId = periodId,
            Periods = new SelectList(await _context.PeriodoContables
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .ToListAsync(), "Id", "Mes", periodId),
            Entries = await GetEntriesByPeriodAsync(periodId ?? Guid.Empty)
        };
    }

    public async Task<AsientoContable?> GetEntryByIdAsync(Guid id)
    {
        return await _context.AsientoContables
            .Include(a => a.AsientoReversion)
            .Include(a => a.Periodo)
            .Include(a => a.TipoComprobante)
            .Include(a => a.AsientoDetalles).ThenInclude(d => d.Cuenta)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AsientoCreateViewModel> GetAsientoCreateContextAsync(AsientoContable? existing = null)
    {
        return new AsientoCreateViewModel
        {
            Entry = existing ?? new AsientoContable { Fecha = DateOnly.FromDateTime(DateTime.Now), Estado = "BORRADOR" },
            TiposComprobante = new SelectList(await _context.TipoComprobantes.OrderBy(t => t.Nombre).ToListAsync(), "Id", "Nombre"),
            CuentasDisponibles = await _context.CuentaContables
                .Where(c => c.AceptaMovimiento && c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre })
                .ToListAsync()
        };
    }

    public async Task<IEnumerable<CuentaContable>> GetAccountsAsync()
    {
        return await _context.CuentaContables
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<CuentaContable?> GetAccountByIdAsync(Guid id)
    {
        return await _context.CuentaContables
            .Include(c => c.AsientoDetalles)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AccountFormViewModel> GetAccountFormContextAsync(CuentaContable? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentasPadre = await _context.CuentaContables
            .Where(c => !c.AceptaMovimiento)
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = c.Codigo + " - " + c.Nombre })
            .ToListAsync();

        return new AccountFormViewModel
        {
            Account = existing ?? new CuentaContable { Activo = true, Moneda = "CUP", Nivel = 1 },
            CuentasPadre = new SelectList(cuentasPadre, "Id", "Display"),
            Clases = new SelectList(new[] { "ACTIVO", "PASIVO", "PATRIMONIO", "INGRESO", "GASTO", "ORDEN" }),
            Naturalezas = new SelectList(new[] { "DEUDORA", "ACREEDORA" })
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateAccountAsync(CuentaContable account)
    {
        try
        {
            account.Id = Guid.NewGuid();
            account.EntidadId = _entidadProvider.CurrentEntidadId;
            account.CreadoEn = DateTimeOffset.Now;

            _context.CuentaContables.Add(account);
            await _context.SaveChangesAsync();
            return (true, "Cuenta contable creada correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateAccountAsync(CuentaContable account)
    {
        try
        {
            var existing = await _context.CuentaContables.FindAsync(account.Id);
            if (existing == null) return (false, "La cuenta no existe.");

            _context.Entry(existing).CurrentValues.SetValues(account);
            existing.EntidadId = _entidadProvider.CurrentEntidadId; // Garantizar multi-inquilino

            await _context.SaveChangesAsync();
            return (true, "Cuenta actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteAccountAsync(Guid id)
    {
        try
        {
            var account = await _context.CuentaContables.FindAsync(id);
            if (account == null) return (false, "No existe.");

            var hasMovements = await _context.AsientoDetalles.AnyAsync(d => d.CuentaId == id);
            if (hasMovements) return (false, "No se puede eliminar una cuenta con movimientos contables.");

            _context.CuentaContables.Remove(account);
            await _context.SaveChangesAsync();
            return (true, "Cuenta eliminada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }


    public async Task<IEnumerable<CentroCosto>> GetCostCentersAsync()
    {
        return await _context.CentroCostos.Include(c => c.Sucursal).OrderBy(c => c.Codigo).ToListAsync();
    }

    public async Task<CentroCosto?> GetCostCenterByIdAsync(Guid id)
    {
        return await _context.CentroCostos.Include(c => c.Sucursal).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<CentroCostoFormViewModel> GetCostCenterFormContextAsync(CentroCosto? existing = null)
    {
        return new CentroCostoFormViewModel
        {
            CentroCosto = existing ?? new CentroCosto { Activo = true },
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateCostCenterAsync(CentroCosto costCenter)
    {
        try
        {
            costCenter.Id = Guid.NewGuid();
            costCenter.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.CentroCostos.Add(costCenter);
            await _context.SaveChangesAsync();
            return (true, "Centro de costo creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateCostCenterAsync(CentroCosto costCenter)
    {
        try
        {
            var existing = await _context.CentroCostos.FindAsync(costCenter.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(costCenter);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Centro de costo actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<TipoComprobante>> GetVoucherTypesAsync()
    {
        return await _context.TipoComprobantes.OrderBy(t => t.Nombre).ToListAsync();
    }

    public async Task<(bool Succeeded, string Message, AsientoContable? Entry)> CreateEntryAsync(AsientoContable entry)
    {
        // 1. Validar cuadre (Partida Doble)
        var totalDebe = entry.AsientoDetalles.Sum(d => d.Debe);
        var totalHaber = entry.AsientoDetalles.Sum(d => d.Haber);

        if (totalDebe != totalHaber)
            return (false, $"El asiento está descuadrado. Debe: {totalDebe}, Haber: {totalHaber}", null);

        if (totalDebe == 0)
            return (false, "El asiento no puede tener totales en cero.", null);

        // 2. Validar Periodo
        var period = await GetOrCreateActivePeriodAsync(entry.EntidadId, entry.Fecha.ToDateTime(TimeOnly.MinValue));
        if (period == null || period.Estado != "ABIERTO") return (false, "El periodo contable para esta fecha no existe o está cerrado.", null);

        entry.PeriodoId = period.Id;
        entry.TotalDebe = totalDebe;
        entry.TotalHaber = totalHaber;
        entry.Estado = "BORRADOR";
        entry.CreadoEn = DateTimeOffset.Now;

        // 3. Generar número consecutivo si no existe
        if (entry.NumeroComprobante == 0)
        {
            var lastNum = await _context.AsientoContables
                .Where(a => a.EntidadId == entry.EntidadId && a.TipoComprobanteId == entry.TipoComprobanteId)
                .OrderByDescending(a => a.NumeroComprobante)
                .Select(a => a.NumeroComprobante)
                .FirstOrDefaultAsync();
            entry.NumeroComprobante = lastNum + 1;
        }

        _context.AsientoContables.Add(entry);
        await _context.SaveChangesAsync();
        return (true, "Asiento creado exitosamente.", entry);
    }

    public async Task<(bool Succeeded, string Message)> PostEntryAsync(Guid entryId)
    {
        var entry = await _context.AsientoContables
            .Include(a => a.Periodo)
            .FirstOrDefaultAsync(a => a.Id == entryId);

        if (entry == null) return (false, "Asiento no encontrado.");
        if (entry.Estado == "CONTABILIZADO") return (false, "El asiento ya está contabilizado.");
        if (entry.Periodo.Estado != "ABIERTO") return (false, "No se puede contabilizar en un periodo cerrado.");

        entry.Estado = "CONTABILIZADO";
        await _context.SaveChangesAsync();

        return (true, "Asiento contabilizado exitosamente.");
    }

    public async Task<(bool Succeeded, string Message, AsientoContable? Adjustment)> ReverseEntryAsync(Guid entryId, string reason)
    {
        var original = await _context.AsientoContables
            .Include(a => a.AsientoDetalles)
            .FirstOrDefaultAsync(a => a.Id == entryId);

        if (original == null) return (false, "Asiento original no encontrado.", null);
        if (original.Estado != "CONTABILIZADO") return (false, "Solo se pueden revertir asientos contabilizados.", null);

        var adjustment = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = original.EntidadId,
            SucursalId = original.SucursalId,
            TipoComprobanteId = original.TipoComprobanteId,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Concepto = $"REVERSIÓN ASIENTO #{original.NumeroComprobante}: {reason}",
            ModuloOrigen = "CONTABILIDAD",
            DocumentoOrigenTipo = "REVERSION",
            DocumentoOrigenId = original.Id,
            Estado = "CONTABILIZADO",
            AsientoReversionId = original.Id,
            CreadoPor = original.CreadoPor,
            CreadoEn = DateTimeOffset.Now
        };

        foreach (var det in original.AsientoDetalles)
        {
            adjustment.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = det.CuentaId,
                CentroCostoId = det.CentroCostoId,
                TerceroId = det.TerceroId,
                TerceroTipo = det.TerceroTipo,
                Debe = det.Haber,
                Haber = det.Debe,
                Glosa = $"REV: {det.Glosa}"
            });
        }

        var result = await CreateEntryAsync(adjustment);
        if (result.Succeeded)
        {
            original.Estado = "REVERTIDO";
            original.AsientoReversionId = adjustment.Id;
            await _context.SaveChangesAsync();
            return (true, "Asiento revertido con éxito.", adjustment);
        }

        return (false, result.Message, null);
    }

    public async Task<decimal> GetAccountBalanceAsync(Guid accountId, Guid? periodId = null)
    {
        var query = _context.AsientoDetalles
            .Where(d => d.CuentaId == accountId && d.Asiento.Estado == "CONTABILIZADO");

        if (periodId.HasValue)
        {
            query = query.Where(d => d.Asiento.PeriodoId == periodId.Value);
        }

        var totals = await query.Select(d => new { d.Debe, d.Haber }).ToListAsync();

        var account = await _context.CuentaContables.FindAsync(accountId);
        if (account == null) return 0;

        var sumDebe = totals.Sum(t => t.Debe);
        var sumHaber = totals.Sum(t => t.Haber);

        return account.Naturaleza == "DEUDORA" ? sumDebe - sumHaber : sumHaber - sumDebe;
    }

    public async Task<List<AsientoContable>> GetEntriesByPeriodAsync(Guid periodId)
    {
        return await _context.AsientoContables
            .Include(a => a.TipoComprobante)
            .Include(a => a.AsientoDetalles)
                .ThenInclude(d => d.Cuenta)
            .Where(a => a.PeriodoId == periodId)
            .OrderBy(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> DeleteDraftEntryAsync(Guid id)
    {
        var entry = await _context.AsientoContables.FindAsync(id);
        if (entry == null) return (false, "Asiento no encontrado.");
        if (entry.Estado == "CONTABILIZADO") return (false, "RF-12: No se puede eliminar un asiento ya contabilizado.");

        _context.AsientoContables.Remove(entry);
        await _context.SaveChangesAsync();
        return (true, "Asiento en borrador eliminado.");
    }

    public async Task<(bool Succeeded, string Message)> ClosePeriodAsync(Guid periodId, Guid userId)
    {
        var period = await _context.PeriodoContables.FindAsync(periodId);
        if (period == null) return (false, "Periodo no encontrado.");
        if (period.Estado == "CERRADO") return (false, "El periodo ya está cerrado.");

        // 1. Validar asientos en borrador
        var pending = await _context.AsientoContables
            .AnyAsync(a => a.PeriodoId == periodId && a.Estado == "BORRADOR");

        if (pending)
            return (false, "No se puede cerrar el periodo. Existen asientos en estado BORRADOR.");

        // 2. Disparar Depreciación de Activos (RF-14)
        using (var scope = _scopeFactory.CreateScope())
        {
            var assetService = scope.ServiceProvider.GetRequiredService<IFixedAssetService>();
            var depResult = await assetService.GenerateMonthlyDepreciationAsync(period.EntidadId, periodId);
            if (!depResult.Succeeded) return (false, $"Fallo en depreciación: {depResult.Message}");
        }

        period.Estado = "CERRADO";
        period.CerradoPor = userId;
        period.CerradoEn = DateTimeOffset.Now;

        await _context.SaveChangesAsync();
        return (true, "Periodo cerrado exitosamente. Depreciación de activos generada.");
    }

    public async Task<(bool Succeeded, string Message)> ReopenPeriodAsync(Guid periodId, string reason, Guid userId)
    {
        var period = await _context.PeriodoContables.FindAsync(periodId);
        if (period == null) return (false, "Periodo no encontrado.");
        if (period.Estado == "ABIERTO") return (false, "El periodo ya está abierto.");

        period.Estado = "ABIERTO";
        period.ActualizadoEn = DateTimeOffset.Now;

        // Registrar en auditoría manual (complementario al interceptor)
        var audit = new Auditorium
        {
            UsuarioId = userId,
            NombreUsuario = "SISTEMA",
            Accion = "REAPERTURA_PERIODO",
            EsquemaTabla = "contabilidad.periodo_contable",
            RegistroId = periodId.ToString(),
            ValoresAnteriores = "Estado: CERRADO",
            ValoresNuevos = $"Motivo: {reason}",
            Canal = "ERP",
            OcurridoEn = DateTime.Now
        };
        _context.Auditoria.Add(audit);

        await _context.SaveChangesAsync();
        return (true, "Periodo reabierto con éxito. Registrado en bitácora.");
    }

    public async Task<PeriodoContable?> GetOrCreateActivePeriodAsync(Guid entidadId, DateTime date)
    {
        if (entidadId == Guid.Empty) return null;

        var year = (short)date.Year;
        var month = (short)date.Month;

        var period = await _context.PeriodoContables
            .FirstOrDefaultAsync(p => p.EntidadId == entidadId && p.Anio == year && p.Mes == month);

        if (period == null)
        {
            period = new PeriodoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Anio = year,
                Mes = month,
                FechaInicio = new DateOnly(year, month, 1),
                FechaFin = new DateOnly(year, month, DateTime.DaysInMonth(year, month)),
                Estado = "ABIERTO",
                CreadoEn = DateTimeOffset.Now,
                ActualizadoEn = DateTimeOffset.Now
            };
            _context.PeriodoContables.Add(period);
            await _context.SaveChangesAsync();
        }

        return period;
    }
}
