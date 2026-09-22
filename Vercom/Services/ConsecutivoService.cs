using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IConsecutivoService
{
    Task<string> ObtenerSiguienteNumeroAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie);
    Task<long> ObtenerSiguienteNumeroLongAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie);
}

public class ConsecutivoService : IConsecutivoService
{
    private readonly AppDbContext _context;

    public ConsecutivoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ObtenerSiguienteNumeroAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie)
    {
        var consecutivo = await ObtenerSiguienteCoreAsync(entidadId, sucursalId, tipoDocumento, serie);
        return consecutivo.UltimoNumero.ToString().PadLeft(consecutivo.LongitudPadding, '0');
    }

    public async Task<long> ObtenerSiguienteNumeroLongAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie)
    {
        var consecutivo = await ObtenerSiguienteCoreAsync(entidadId, sucursalId, tipoDocumento, serie);
        return consecutivo.UltimoNumero;
    }

    private async Task<Consecutivo> ObtenerSiguienteCoreAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie)
    {
        // RNF-51: Usar UPDLOCK para garantizar exclusión mutua en la transacción.
        // Si el llamador ya abrió una transacción, se reutiliza (el bloqueo dura hasta
        // que finalice la externa); de lo contrario se abre una propia.
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var sql = @"
            SELECT * FROM nucleo.consecutivo WITH (UPDLOCK, ROWLOCK)
            WHERE entidad_id = {0}
            AND (sucursal_id = {1} OR ({1} IS NULL AND sucursal_id IS NULL))
            AND tipo_documento = {2}
            AND serie = {3}";

            var consecutivo = await _context.Consecutivos
                .FromSqlRaw(sql, entidadId, (object?)sucursalId ?? DBNull.Value, tipoDocumento, serie)
                .FirstOrDefaultAsync();

            if (consecutivo == null)
            {
                // Inicializar si no existe
                consecutivo = new Consecutivo
                {
                    EntidadId = entidadId,
                    SucursalId = sucursalId,
                    TipoDocumento = tipoDocumento,
                    Serie = serie,
                    UltimoNumero = 0,
                    LongitudPadding = 8,
                    ActualizadoEn = DateTimeOffset.Now
                };
                _context.Consecutivos.Add(consecutivo);
            }
            else
            {
                consecutivo.UltimoNumero++;
                consecutivo.ActualizadoEn = DateTimeOffset.Now;
            }

            await _context.SaveChangesAsync();
            if (ownsTransaction) await transaction!.CommitAsync();

            return consecutivo;
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }
}