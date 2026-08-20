using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IAccountingService
{
    Task<(bool Succeeded, string Message, AsientoContable? Entry)> CreateEntryAsync(AsientoContable entry);
    Task<(bool Succeeded, string Message)> PostEntryAsync(Guid entryId);
    Task<(bool Succeeded, string Message, AsientoContable? Adjustment)> ReverseEntryAsync(Guid entryId, string reason);
    Task<decimal> GetAccountBalanceAsync(Guid accountId, Guid? periodId = null);
    Task<List<AsientoContable>> GetEntriesByPeriodAsync(Guid periodId);
    Task<(bool Succeeded, string Message)> ClosePeriodAsync(Guid periodId, Guid userId);
    Task<(bool Succeeded, string Message)> ReopenPeriodAsync(Guid periodId, string reason, Guid userId);
    Task<PeriodoContable?> GetOrCreateActivePeriodAsync(Guid entidadId, DateTime date);
}

public class AccountingService : IAccountingService
{
    private readonly AppDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;

    public AccountingService(AppDbContext context, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _scopeFactory = scopeFactory;
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
        if (period == null || period.Estado != "ABIERTO")
            return (false, "El periodo contable para esta fecha no existe o está cerrado.", null);

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
