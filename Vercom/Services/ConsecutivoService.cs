using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IConsecutivoService
{
    Task<string> ObtenerSiguienteNumeroAsync(Guid entidadId, Guid? sucursalId, string tipoDocumento, string serie);
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
        // RNF-51: Usar UPDLOCK para garantizar exclusión mutua en la transacción
        // En EF Core usamos FromSqlRaw para invocar el bloqueo de fila de SQL Server
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
                UltimoNumero = 1,
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

        return consecutivo.UltimoNumero.ToString().PadLeft(consecutivo.LongitudPadding, '0');
    }
}
