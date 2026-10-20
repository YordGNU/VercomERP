using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace VERCOM.Services;

public class PosSyncBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PosSyncBackgroundWorker> _logger;

    public PosSyncBackgroundWorker(IServiceProvider serviceProvider, ILogger<PosSyncBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando Worker de Sincronización POS...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingSalesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del Worker POS.");
            }

            // Esperar 30 segundos entre barridos
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessPendingSalesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingList = await context.PosVentaPendientes
            .IgnoreQueryFilters()
            .Where(p => p.Estado == "PENDIENTE")
            .OrderBy(p => p.FechaRecibidoServidor)
            .Take(20) // Procesar en lotes de 20
            .ToListAsync(ct);

        if (!pendingList.Any()) return;

        foreach (var pending in pendingList)
        {
            try
            {
                await ProcesarPendienteAsync(context, scope.ServiceProvider, pending, ct);
            }
            catch (Exception ex)
            {
                // Una fila venenosa (FK rota, número duplicado, cuenta contable sin resolver...)
                // no debe abortar el barrido completo: se marca ERROR —visible para el operador
                // y sin reintento infinito— y el resto del lote sigue procesándose.
                _logger.LogError(ex, "POS: falló el procesamiento de la venta pendiente {PendienteId}.", pending.Id);
                await MarcarErrorAsync(context, pending, ex.Message, ct);
            }
        }
    }

    private async Task ProcesarPendienteAsync(AppDbContext context, IServiceProvider services, PosVentaPendiente pending, CancellationToken ct)
    {
        // Se procesa con la MISMA implementación que usa la API JWT
        // (/api/pos/sincronizacion/ventas/{dispositivo}/{clave}/procesar): una sola ruta de
        // facturación, inventario y contabilidad. El worker es la red de seguridad para los
        // terminales que quedaron offline y nunca llamaron a /procesar.
        var syncService = services.GetRequiredService<PosSincronizacionService>();

        var result = await syncService.ProcesarAsync(pending.DispositivoPosId, pending.IdempotencyKey, ct);

        if (result.Conflict)
        {
            // Carrera entre el worker y el endpoint JWT: la transacción del servicio es
            // SERIALIZABLE y reintenta 1205; se deja PENDIENTE para el siguiente barrido
            // en vez de marcarla ERROR.
            return;
        }

        if (result.Error is not null)
        {
            // El servicio no persiste nada en caso de error de validación; se marca aquí para
            // no reprocesarla sin fin (poison). El reintento del dispositivo por JWT sigue
            // siendo la ruta normal.
            pending.Estado = "ERROR";
            pending.MensajeError = result.Error;
            pending.IntentosProcesamiento++;
            await context.SaveChangesAsync(ct);
        }
    }

    // Marca la venta pendiente como ERROR sin arrastrar entidades a medias del intento
    // fallido, de modo que el error quede registrado aunque la fila sea irrecuperable.
    private async Task MarcarErrorAsync(AppDbContext context, PosVentaPendiente pending, string mensaje, CancellationToken ct)
    {
        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
            entry.State = EntityState.Detached;

        pending.Estado = "ERROR";
        pending.MensajeError = mensaje;
        pending.IntentosProcesamiento++;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POS: tampoco se pudo marcar como ERROR la venta pendiente {PendienteId}.", pending.Id);
        }
    }
}
