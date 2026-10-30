namespace VERCOM.Services;

public class BackupBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupBackgroundWorker> _logger;
    private readonly IConfiguration _config;

    public BackupBackgroundWorker(IServiceProvider serviceProvider, ILogger<BackupBackgroundWorker> logger, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de Backup Automatico iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hour = _config.GetValue<int>("BackupSettings:ScheduleHour", 1); // 1 AM por defecto
                var now = DateTime.Now;

                var nextRun = now.Date.AddHours(hour);
                if (now > nextRun) nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("Proximo backup programado para: {Time}", nextRun);

                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Iniciando backup automatico programado...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                    var result = await backupService.CreateBackupAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation("Backup automatico completado: {Msg}", result.Message);
                        try
                        {
                            var deleted = await backupService.DeleteOldBackupsAsync(30);
                            if (deleted > 0)
                                _logger.LogInformation("Se eliminaron {Count} respaldos con mas de 30 dias.", deleted);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogWarning(cleanupEx, "No se pudieron limpiar respaldos antiguos.");
                        }
                    }
                    else
                    {
                        _logger.LogError("Fallo en backup automatico: {Error}", result.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // El servicio se esta deteniendo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error critico en el worker de backup.");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
