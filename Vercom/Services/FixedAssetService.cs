using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IFixedAssetService
{
    Task<(bool Succeeded, string Message)> GenerateMonthlyDepreciationAsync(Guid entidadId, Guid periodId);
    Task<List<ActivoFijo>> GetActiveAssetsAsync(Guid entidadId);
}

public class FixedAssetService : IFixedAssetService
{
   private readonly AppDbContext _context;   
    private readonly IAccountingService _accountingService;

    public FixedAssetService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
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
            // Cálculo simplificado: (Valor - Residual) / Vida Útil
            var cuotaMensual = (asset.ValorAdquisicion - asset.ValorResidual) / asset.VidaUtilMeses;

            if (asset.DepreciacionAcumulada + cuotaMensual > asset.ValorAdquisicion)
                cuotaMensual = asset.ValorAdquisicion - asset.DepreciacionAcumulada;

            if (cuotaMensual <= 0) continue;

            // Detalle Gasto Depreciación (Debe)
            entry.AsientoDetalles.Add(new AsientoDetalle
            {
                Id = Guid.NewGuid(),
                CuentaId = asset.CuentaGastoDepId,
                Debe = cuotaMensual,
                Glosa = $"Gasto Dep. {asset.CodigoInventario}"
            });

            // Detalle Depreciación Acumulada (Haber)
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
}
