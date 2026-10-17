using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VERCOM.DTOs;
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
                // Una fila venenosa (payload legacy, FK rota, número duplicado, cuenta
                // contable sin resolver...) no debe abortar el barrido completo: se marca
                // ERROR —visible para el operador y sin reintento infinito— y el resto del
                // lote sigue procesándose.
                _logger.LogError(ex, "POS: falló el procesamiento de la venta pendiente {PendienteId}.", pending.Id);
                await MarcarErrorAsync(context, pending, ex.Message, ct);
            }
        }
    }

    private async Task ProcesarPendienteAsync(AppDbContext context, IServiceProvider services, PosVentaPendiente pending, CancellationToken ct)
    {
        // Formato nuevo (VentaPosPayload): se procesa con la MISMA implementación que
        // usa la API JWT (/api/pos/sincronizacion/.../procesar), es decir
        // PosSincronizacionService.ProcesarAsync — transacción SERIALIZABLE, reintento
        // acotado de interbloqueo 1205 y decremento atómico condicional.
        //
        // Esto elimina la doble facturación (una para worker-legacy y otra para JWT)
        // y actúa además como red de seguridad: si el terminal queda offline y nunca
        // vuelve a llamar /procesar, el worker absorbe la venta en vez de dejarla
        // PENDIENTE-huérfana para siempre.
        if (pending.PayloadJson.IndexOf("TipoMovimientoId", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var syncService = services.GetRequiredService<PosSincronizacionService>();

            var result = await syncService.ProcesarAsync(pending.DispositivoPosId, pending.IdempotencyKey, ct);

            if (result.Conflict)
            {
                // Carrera entre el worker y el endpoint JWT: la transacción del servicio
                // es SERIALIZABLE y reintenta 1205; se deja PENDIENTE para el siguiente
                // barrido en vez de marcarla ERROR.
                return;
            }

            if (result.Error is not null)
            {
                // El servicio no persiste la venta en caso de error de validación; se
                // marca aquí para no reprocesarla sin fin (poison). El reintento del
                // dispositivo por JWT seguirá siendo la ruta normal.
                pending.Estado = "ERROR";
                pending.MensajeError = result.Error;
                pending.IntentosProcesamiento++;
                await context.SaveChangesAsync(ct);
            }

            // Si result.Result no es null y no hay conflicto, el servicio ya marcó la
            // venta como PROCESADO (CommitAsync con SERIALIZABLE) y este SaveChanges
            // es un no-op seguro.
            return;
        }

        // Compatibilidad legacy (OperacionRequestDto): su único productor era
        // POST /api/ventas/sync y ningún cliente vigente lo usa.
        var salesService = services.GetRequiredService<ISalesService>();

        var op = JsonSerializer.Deserialize<OperacionRequestDto>(pending.PayloadJson);
        if (op == null) throw new InvalidOperationException("Payload JSON inválido.");

        var device = await context.DispositivoPos.AsNoTracking()
            .Where(d => d.Id == pending.DispositivoPosId)
            .Select(d => new { d.EntidadId, d.Nombre, d.SucursalId })
            .FirstAsync(ct);

        var cantidad = op.Cantidad ?? 0m;

        // Mapear DTO a modelo Factura
        var invoice = new FacturaVentum
        {
            EntidadId = device.EntidadId,
            SucursalId = device.SucursalId,
            Serie = "POS",
            ClienteId = op.ClienteId ?? Guid.Empty,
            AlmacenId = op.PuntoVentaid ?? Guid.Empty,
            CanalVenta = "POS",
            TipoVenta = "MINORISTA",
            Fecha = op.Fecha ?? pending.FechaVentaLocal.DateTime,
            Moneda = op.Moneda,
            Total = op.Importe ?? 0,
            CreadoPor = Guid.Empty // Sistema
        };

        if (op.Productoid.HasValue)
        {
            invoice.FacturaVentaDetalles.Add(new FacturaVentaDetalle
            {
                ProductoId = op.Productoid.Value,
                Cantidad = cantidad,
                PrecioUnitario = cantidad > 0 ? (op.Importe ?? 0m) / cantidad : 0m,
                DescuentoPorcentaje = 0
            });
        }

        var resultadoVenta = await salesService.CreateInvoiceAsync(invoice);

        if (resultadoVenta.Succeeded)
        {
            pending.Estado = "PROCESADO";
            pending.FacturaId = resultadoVenta.Invoice?.Id;
            pending.ProcesadoEn = DateTimeOffset.Now;

            var notifications = services.GetRequiredService<INotificationService>();
            await notifications.NotifyEntityAsync(device.EntidadId, "Venta POS procesada",
                $"{device.Nombre}: venta por {invoice.Total:N2} {op.Moneda} procesada.", "success");
        }
        else
        {
            pending.Estado = "ERROR";
            pending.MensajeError = resultadoVenta.Message;
        }

        await context.SaveChangesAsync(ct);
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
