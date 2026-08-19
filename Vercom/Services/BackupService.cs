using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IBackupService
{
    Task<(bool Success, string Message, string? Path)> CreateBackupAsync();
    Task<List<BackupLog>> GetBackupLogsAsync();
}

public class BackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public BackupService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, string? Path)> CreateBackupAsync()
    {
        var dbName = _context.Database.GetDbConnection().Database;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{dbName}_{timestamp}.bak";
        var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        if (!Directory.Exists(backupPath))
            Directory.CreateDirectory(backupPath);

        var fullPath = Path.Combine(backupPath, fileName);

        var log = new BackupLog
        {
            Id = Guid.NewGuid(),
            Tipo = "COMPLETO",
            IniciadoEn = DateTime.Now,
            Estado = "EN_PROGRESO",
            RutaArchivo = fullPath
        };

        _context.BackupLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            // Nota: El usuario de SQL Server debe tener permisos de escritura en la carpeta de destino
            // En muchos entornos, SQL Server no puede escribir en carpetas de usuario.
            // Para esta implementación "al pie de la letra", usamos el comando SQL estándar.
            var sql = $"BACKUP DATABASE [{dbName}] TO DISK = '{fullPath}' WITH FORMAT, MEDIANAME = 'VercomBackup', NAME = 'Full Backup of {dbName}';";

            await _context.Database.ExecuteSqlRawAsync(sql);

            log.Estado = "EXITOSO";
            log.FinalizadoEn = DateTime.Now;
            var fileInfo = new FileInfo(fullPath);
            log.TamanoBytes = fileInfo.Exists ? fileInfo.Length : 0;

            await _context.SaveChangesAsync();
            return (true, "Respaldo creado con éxito.", fullPath);
        }
        catch (Exception ex)
        {
            log.Estado = "FALLIDO";
            log.MensajeError = ex.Message;
            log.FinalizadoEn = DateTime.Now;
            await _context.SaveChangesAsync();
            return (false, $"Error al crear respaldo: {ex.Message}", null);
        }
    }

    public async Task<List<BackupLog>> GetBackupLogsAsync()
    {
        return await _context.BackupLogs.OrderByDescending(l => l.IniciadoEn).ToListAsync();
    }
}
