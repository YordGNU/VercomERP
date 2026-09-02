using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;


namespace Vercom.Services;

public interface IBackupService
{
    Task<(bool Success, string Message, string? Path)> CreateBackupAsync();
    Task<(bool Success, string Message)> RestoreBackupAsync(Guid backupId);
    Task<List<BackupLog>> GetBackupLogsAsync();
    Task<BackupLog?> GetBackupLogByIdAsync(Guid id);
    Task<int> DeleteOldBackupsAsync(int daysToKeep = 30);
}

public class BackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IAdminService _auditService;

    public BackupService(AppDbContext context, IConfiguration configuration, IAdminService auditService)
    {
        _context = context;
        _configuration = configuration;
        _auditService = auditService;
    }

    // ================================================================
    // 1. OBTENER RUTA PREDETERMINADA DE SQL SERVER
    // ================================================================
    private async Task<string> GetDefaultBackupPathAsync()
    {
        var connection = _context.Database.GetDbConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultBackupPath')";

        await connection.OpenAsync();
        var result = await cmd.ExecuteScalarAsync() as string;
        await connection.CloseAsync();

        return result?.Trim() ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Microsoft", "SQL Server", "Backup"
        );
    }

    // ================================================================
    // 2. CREAR RESPALDO (con carpeta predeterminada de SQL Server)
    // ================================================================
    public async Task<(bool Success, string Message, string? Path)> CreateBackupAsync()
    {
        var dbName = _context.Database.GetDbConnection().Database;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{dbName}_{timestamp}.bak";
        BackupLog? log = null;

        try
        {
            // Leer ruta desde configuración
            var backupDir = _configuration["BackupSettings:CustomBackupPath"];
            if (string.IsNullOrWhiteSpace(backupDir))
            {
                backupDir = await GetDefaultBackupPathAsync();
            }

            backupDir = backupDir.Trim();

            // Crear carpeta si no existe
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var fullPath = Path.Combine(backupDir, fileName);

            // Crear log en BD
            log = new BackupLog
            {
                Id = Guid.NewGuid(),
                Tipo = "MANUAL",
                IniciadoEn = DateTimeOffset.Now,
                Estado = "EN_PROGRESO",
                RutaArchivo = fullPath
            };

            _context.BackupLogs.Add(log);
            await _context.SaveChangesAsync();

            // Ejecutar BACKUP (con USE master para evitar bloqueos)
            var sql = $@"
            USE master;
            BACKUP DATABASE [{dbName}] 
            TO DISK = '{fullPath.Replace("\\", "\\\\")}' 
            WITH FORMAT, 
                 MEDIANAME = 'VercomBackup', 
                 NAME = 'Full Backup of {dbName}',
                 STATS = 10;";

            await _context.Database.ExecuteSqlRawAsync(sql);

            // Actualizar log
            log.Estado = "COMPLETADO";
            log.FinalizadoEn = DateTimeOffset.Now;
            var fileInfo = new FileInfo(fullPath);
            log.TamanoBytes = fileInfo.Exists ? fileInfo.Length : 0;
            await _context.SaveChangesAsync();


            return (true, $"Respaldo creado en: {fullPath}", fullPath);
        }
        catch (Exception ex)
        {
            if (log != null)
            {
                log.Estado = "FALLIDO";
                log.MensajeError = ex.Message;
                log.FinalizadoEn = DateTimeOffset.Now;
                await _context.SaveChangesAsync();
            }

            return (false, $"Error: {ex.Message}", null);
        }
    }

    // ================================================================
    // 3. RESTAURAR RESPALDO
    // ================================================================
    public async Task<(bool Success, string Message)> RestoreBackupAsync(Guid backupId)
    {
        var log = await _context.BackupLogs.FindAsync(backupId);
        if (log == null)
            return (false, "Respaldo no encontrado.");

        if (log.Estado != "COMPLETADO")
            return (false, "El respaldo no está completo o es inválido.");

        if (!File.Exists(log.RutaArchivo))
            return (false, "El archivo de respaldo no existe en el servidor.");

        var dbName = _context.Database.GetDbConnection().Database;
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            // Conectar a master para la restauración
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "master";

            using var masterConnection = new SqlConnection(builder.ConnectionString);
            await masterConnection.OpenAsync();

            // 1. Poner la BD en modo SINGLE_USER (forzar desconexión)
            var setSingleUser = $@"
                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            ";
            using var cmd = new SqlCommand(setSingleUser, masterConnection);
            await cmd.ExecuteNonQueryAsync();

            // 2. Restaurar desde el archivo
            var restoreSql = $@"
                RESTORE DATABASE [{dbName}] 
                FROM DISK = '{log.RutaArchivo}' 
                WITH REPLACE, RECOVERY;
            ";
            cmd.CommandText = restoreSql;
            await cmd.ExecuteNonQueryAsync();

            // 3. Volver a MULTI_USER
            var setMultiUser = $@"
                ALTER DATABASE [{dbName}] SET MULTI_USER;
            ";
            cmd.CommandText = setMultiUser;
            await cmd.ExecuteNonQueryAsync();

            return (true, "Base de datos restaurada exitosamente.");
        }
        catch (Exception ex)
        {
            // Intentar volver a MULTI_USER si falló
            try
            {
                var builderRetry = new SqlConnectionStringBuilder(connectionString);
                builderRetry.InitialCatalog = "master";
                using var conn = new SqlConnection(builderRetry.ConnectionString);
                await conn.OpenAsync();
                var revert = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                using var cmd = new SqlCommand(revert, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { /* Ignorar error al revertir */ }


            return (false, $"Error al restaurar: {ex.Message}");
        }
    }

    // ================================================================
    // 4. OBTENER LISTA DE RESPALDOS
    // ================================================================
    public async Task<List<BackupLog>> GetBackupLogsAsync()
    {
        return await _context.BackupLogs
            .OrderByDescending(l => l.IniciadoEn)
            .ToListAsync();
    }

    // ================================================================
    // 5. OBTENER RESPALDO POR ID
    // ================================================================
    public async Task<BackupLog?> GetBackupLogByIdAsync(Guid id)
    {
        return await _context.BackupLogs
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    // ================================================================
    // 6. ELIMINAR RESPALDOS ANTIGUOS (política de retención)
    // ================================================================
    public async Task<int> DeleteOldBackupsAsync(int daysToKeep = 30)
    {
        var threshold = DateTimeOffset.Now.AddDays(-daysToKeep);

        var oldLogs = await _context.BackupLogs
            .Where(l => l.IniciadoEn < threshold && l.Estado == "COMPLETADO")
            .ToListAsync();

        if (!oldLogs.Any())
            return 0;

        var deletedCount = 0;
        foreach (var log in oldLogs)
        {
            // Eliminar archivo físico
            if (File.Exists(log.RutaArchivo))
            {
                try
                {
                    File.Delete(log.RutaArchivo);
                }
                catch { /* Continuar con el siguiente */ }
            }

            _context.BackupLogs.Remove(log);
            deletedCount++;
        }

        await _context.SaveChangesAsync();

        return deletedCount;
    }
}