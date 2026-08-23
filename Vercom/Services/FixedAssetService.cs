using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IFixedAssetService
{
    // Lectura
    Task<IEnumerable<ActivoFijo>> GetAssetsAsync();
    Task<ActivoFijo?> GetAssetByIdAsync(Guid id);
    Task<AssetFormViewModel> GetAssetFormContextAsync(ActivoFijo? existing = null);

    // Escritura
    Task<(bool Succeeded, string Message)> CreateAssetAsync(ActivoFijo asset);
    Task<(bool Succeeded, string Message)> UpdateAssetAsync(ActivoFijo asset);
    Task<(bool Succeeded, string Message)> GenerateMonthlyDepreciationAsync(Guid entidadId, Guid periodId);
    Task<List<ActivoFijo>> GetActiveAssetsAsync(Guid entidadId);
    Task<(bool Succeeded, string Message)> RetireAssetAsync(Guid assetId, string reason, Guid userId);
    Task<(bool Succeeded, string Message)> DeleteAssetAsync(Guid id);
}

public class FixedAssetService : IFixedAssetService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public FixedAssetService(AppDbContext context, IAccountingService accountingService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _accountingService = accountingService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<ActivoFijo>> GetAssetsAsync()
    {
        return await _context.ActivoFijos
            .Include(a => a.CuentaActivo)
            .Include(a => a.CuentaDepreciacion)
            .Include(a => a.CuentaGastoDep)
            .OrderByDescending(a => a.FechaAdquisicion)
            .ToListAsync();
    }

    public async Task<ActivoFijo?> GetAssetByIdAsync(Guid id)
    {
        return await _context.ActivoFijos
            .Include(a => a.CuentaActivo)
            .Include(a => a.CuentaDepreciacion)
            .Include(a => a.CuentaGastoDep)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AssetFormViewModel> GetAssetFormContextAsync(ActivoFijo? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentas = await _context.CuentaContables
            .Where(c => c.Activo && c.AceptaMovimiento)
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre })
            .ToListAsync();

        return new AssetFormViewModel
        {
            Asset = existing ?? new ActivoFijo { Estado = "ACTIVO", MetodoDepreciacion = "LINEA_RECTA", FechaAdquisicion = DateOnly.FromDateTime(DateTime.Now) },
            CuentasActivo = new SelectList(cuentas, "Id", "Display"),
            CuentasDepreciacion = new SelectList(cuentas, "Id", "Display"),
            CuentasGasto = new SelectList(cuentas, "Id", "Display"),
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateAssetAsync(ActivoFijo asset)
    {
        try
        {
            asset.Id = Guid.NewGuid();
            asset.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.ActivoFijos.Add(asset);
            await _context.SaveChangesAsync();
            return (true, "Activo fijo registrado correctamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateAssetAsync(ActivoFijo asset)
    {
        try
        {
            var existing = await _context.ActivoFijos.FindAsync(asset.Id);
            if (existing == null) return (false, "No existe.");

            _context.Entry(existing).CurrentValues.SetValues(asset);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;

            await _context.SaveChangesAsync();
            return (true, "Activo actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteAssetAsync(Guid id)
    {
        try
        {
            var asset = await _context.ActivoFijos.FindAsync(id);
            if (asset == null) return (false, "No existe.");

            var hasDep = await _context.ActivoFijoDepreciacions.AnyAsync(d => d.ActivoFijoId == id);
            if (hasDep) return (false, "No se puede eliminar un activo que ya tiene historial de depreciación.");

            _context.ActivoFijos.Remove(asset);
            await _context.SaveChangesAsync();
            return (true, "Activo eliminado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> GenerateMonthlyDepreciationAsync(Guid entidadId, Guid periodId)
    {
        var period = await _context.PeriodoContables.FindAsync(periodId);
        if (period == null || period.Estado != "ABIERTO")
            return (false, "El periodo no es válido o está cerrado.");

        var assets = await _context.ActivoFijos
            .Where(a => a.EntidadId == entidadId && a.Estado == "ACTIVO")
            .ToListAsync();

        if (!assets.Any()) return (true, "No hay activos fijos para depreciar.");

        // Verificar si ya se corrió la depreciación para este periodo
        var alreadyDone = await _context.ActivoFijoDepreciacions.AnyAsync(d => d.PeriodoId == periodId);
        if (alreadyDone) return (true, "La depreciación de este mes ya fue procesada.");

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            PeriodoId = periodId,
            TipoComprobanteId = 7, // Ajustes (AJ)
            Fecha = period.FechaFin,
            Concepto = $"DEPRECIACIÓN MENSUAL {period.Mes}/{period.Anio}",
            ModuloOrigen = "ACTIVOS_FIJOS",
            Estado = "CONTABILIZADO",
            CreadoEn = DateTimeOffset.Now
        };

        decimal totalDepreciacion = 0;

        foreach (var asset in assets)
        {
            var cuotaMensual = (asset.ValorAdquisicion - asset.ValorResidual) / asset.VidaUtilMeses;

            if (asset.DepreciacionAcumulada + cuotaMensual > asset.ValorAdquisicion)
                cuotaMensual = asset.ValorAdquisicion - asset.DepreciacionAcumulada;

            if (cuotaMensual <= 0) continue;

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = asset.CuentaGastoDepId,
                Debe = cuotaMensual,
                Glosa = $"Gasto Dep. {asset.CodigoInventario}"
            });

            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = asset.CuentaDepreciacionId,
                Haber = cuotaMensual,
                Glosa = $"Acum. Dep. {asset.CodigoInventario}"
            });

            asset.DepreciacionAcumulada += cuotaMensual;
            totalDepreciacion += cuotaMensual;

            _context.ActivoFijoDepreciacions.Add(new ActivoFijoDepreciacion
            {
                Id = Guid.NewGuid(),
                ActivoFijoId = asset.Id,
                PeriodoId = periodId,
                Monto = cuotaMensual,
                AsientoId = entry.Id,
                CalculadoEn = DateTimeOffset.Now
            });
        }

        if (totalDepreciacion > 0)
        {
            var result = await _accountingService.CreateEntryAsync(entry);
            if (!result.Succeeded) return (false, result.Message);

            await _context.SaveChangesAsync();
            return (true, $"Depreciación generada por un total de {totalDepreciacion:C}.");
        }

        return (true, "No se generó depreciación en este periodo.");
    }

    public async Task<List<ActivoFijo>> GetActiveAssetsAsync(Guid entidadId)
    {
        return await _context.ActivoFijos
            .Where(a => a.EntidadId == entidadId && a.Estado == "ACTIVO")
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string Message)> RetireAssetAsync(Guid assetId, string reason, Guid userId)
    {
        var asset = await _context.ActivoFijos.FindAsync(assetId);
        if (asset == null) return (false, "Activo no encontrado.");
        if (asset.Estado == "BAJA") return (false, "El activo ya está de baja.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = asset.EntidadId,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Concepto = $"BAJA DE ACTIVO {asset.CodigoInventario}: {reason}",
                ModuloOrigen = "ACTIVOS_FIJOS",
                TipoComprobanteId = 7, // Ajustes
                Estado = "CONTABILIZADO",
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now
            };

            // 1. Revertir Depreciación Acumulada (Debe)
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = asset.CuentaDepreciacionId,
                Debe = asset.DepreciacionAcumulada,
                Glosa = $"Cancel. Dep. Acum. por Baja"
            });

            // 2. Reconocer Pérdida por Baja (si aplica) (Debe)
            var valorNeto = asset.ValorAdquisicion - asset.DepreciacionAcumulada;
            if (valorNeto > 0)
            {
                // Buscar cuenta de gastos por pérdida de activos (ej: 7xx)
                var lossAccount = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "701" && c.EntidadId == asset.EntidadId);
                if (lossAccount != null)
                {
                    entry.AsientoDetalles.Add(new AsientoDetalle
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = lossAccount.Id,
                        Debe = valorNeto,
                        Glosa = $"Pérdida por Baja de Activo"
                    });
                }
            }

            // 3. Cancelar Valor de Adquisición (Haber)
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = asset.CuentaActivoId,
                Haber = asset.ValorAdquisicion,
                Glosa = $"Baja Activo {asset.CodigoInventario}"
            });

            var accResult = await _accountingService.CreateEntryAsync(entry);
            if (!accResult.Succeeded) throw new Exception(accResult.Message);

            asset.Estado = "BAJA";
            asset.FechaBaja = DateOnly.FromDateTime(DateTime.Now);
            asset.MotivoBaja = reason;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Activo dado de baja y asiento contable generado.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al dar de baja: {ex.Message}");
        }
    }
}
