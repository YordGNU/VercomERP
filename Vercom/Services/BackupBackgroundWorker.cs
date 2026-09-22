namespace Vercom.Services;

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
        _logger.LogInformation("Servicio de Backup Automático iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hour = _config.GetValue<int>("BackupSettings:ScheduleHour", 1); // 1 AM por defecto
                var now = DateTime.Now;

                // Calcular tiempo hasta la próxima ejecución
                var nextRun = now.Date.AddHours(hour);
                if (now > nextRun) nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("Próximo backup programado para: {Time}", nextRun);

                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Iniciando backup automático programado...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                    var result = await backupService.CreateBackupAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation("Backup automático completado: {Msg}", result.Message);
                    }
                    else
                    {
                        _logger.LogError("Fallo en backup automático: {Error}", result.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // El servicio se está deteniendo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el worker de backup.");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Esperar un poco antes de reintentar si falló por error de código
            }
        }
    }
}
