using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Vercom.DTOs;
using Vercom.Models;

namespace Vercom.Services;

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
        var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

        var pendingList = await context.PosVentaPendientes
            .IgnoreQueryFilters()
            .Where(p => p.Estado == "PENDIENTE")
            .OrderBy(p => p.FechaRecibidoServidor)
            .Take(20) // Procesar en lotes de 20
            .ToListAsync(ct);

        if (!pendingList.Any()) return;

        foreach (var pending in pendingList)
        {
            // Los pendientes del formato nuevo (VentaPosPayload) se procesan bajo
            // JWT vía /api/pos/sincronizacion/.../procesar; aquí solo legacy.
            if (pending.PayloadJson.IndexOf("TipoMovimientoId", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            try
            {
                var op = JsonSerializer.Deserialize<OperacionRequestDto>(pending.PayloadJson);
                if (op == null) throw new Exception("Payload JSON inválido");

                var device = await context.DispositivoPos.AsNoTracking()
                    .Where(d => d.Id == pending.DispositivoPosId)
                    .Select(d => new { d.EntidadId, d.Nombre })
                    .FirstAsync(ct);

                // Mapear DTO a modelo Factura
                var invoice = new FacturaVentum
                {
                    EntidadId = device.EntidadId,
                    SucursalId = await context.DispositivoPos.Where(d => d.Id == pending.DispositivoPosId).Select(d => d.SucursalId).FirstOrDefaultAsync(ct),
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
                        Cantidad = op.Cantidad ?? 0,
                        PrecioUnitario = (op.Importe / op.Cantidad) ?? 0,
                        DescuentoPorcentaje = 0
                    });
                }

                var result = await salesService.CreateInvoiceAsync(invoice);

                if (result.Succeeded)
                {
                    pending.Estado = "PROCESADA";
                    pending.FacturaId = result.Invoice?.Id;
                    pending.ProcesadoEn = DateTimeOffset.Now;

                    var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notifications.NotifyEntityAsync(device.EntidadId, "Venta POS procesada",
                        $"{device.Nombre}: venta por {invoice.Total:N2} {op.Moneda} procesada.", "success");
                }
                else
                {
                    pending.Estado = "ERROR";
                    pending.MensajeError = result.Message;
                }
            }
            catch (Exception ex)
            {
                pending.Estado = "ERROR";
                pending.MensajeError = ex.Message;
                pending.IntentosProcesamiento++;
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
