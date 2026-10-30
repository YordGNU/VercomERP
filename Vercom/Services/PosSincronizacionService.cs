using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VERCOM.DTOs;
using VERCOM.Helpers;
using VERCOM.Models;
using VERCOM.Security;

namespace VERCOM.Services;

public sealed class PosSincronizacionService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IInventoryService _inventory;
    private readonly IAccountingService _accounting;
    private readonly IParametroSistemaService _parametros;
    private readonly ILogger<PosSincronizacionService> _logger;
    private readonly IEntidadProvider _entidad;
    private readonly IConsecutivoService _consecutivo;

    public PosSincronizacionService(AppDbContext db, INotificationService notifications, IInventoryService inventory, IAccountingService accounting, IParametroSistemaService parametros, ILogger<PosSincronizacionService> logger, IEntidadProvider entidad, IConsecutivoService consecutivo)
    {
        _db = db;
        _notifications = notifications;
        _inventory = inventory;
        _accounting = accounting;
        _parametros = parametros;
        _logger = logger;
        _entidad = entidad;
        _consecutivo = consecutivo;
    }

    // Entidad del solicitante autenticado; null cuando no corresponde restringir (rol MASTER o
    // scope sin HttpContext, como el background worker), mismo criterio que PosService.
    private Guid? EntidadSolicitante()
    {
        if (_entidad.IsMaster) return null;
        var id = _entidad.CurrentEntidadId;
        return id == Guid.Empty ? null : id;
    }

    public async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict, bool Accepted)> RecibirAsync(RecibirVentaPosRequest request, CancellationToken cancellationToken)
    {
        if (request.DispositivoPosId == Guid.Empty || request.SesionCajaPosId == Guid.Empty || string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 80)
            return (null, "Dispositivo, sesión y clave idempotente son obligatorios.", false, false);
        if (request.Venta.ValueKind != JsonValueKind.Object) return (null, "Venta debe ser un objeto JSON.", false, false);

        var device = await _db.DispositivoPos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DispositivoPosId, cancellationToken);
        if (device is null) return (null, "Dispositivo POS no encontrado.", true, false);
        if (EntidadSolicitante() is Guid entidad && device.EntidadId != entidad) return (null, "Dispositivo POS no encontrado.", true, false);

        // Sesión compartida: cualquier dispositivo de la misma caja puede registrar ventas
        // contra la sesión ABIERTA de esa caja, aunque la haya abierto otro dispositivo.
        var validSession = await _db.SesionCajaPos.AsNoTracking()
            .AnyAsync(x => x.Id == request.SesionCajaPosId && x.CajaId == device.CajaId && x.Estado == "ABIERTA", cancellationToken);
        if (!validSession) return (null, "La sesión POS no existe o no está abierta.", true, false);

        var existing = await FindAsync(request.DispositivoPosId, request.IdempotencyKey, cancellationToken);
        if (existing is not null) return (Map(existing), null, false, false);

        // Validación temprana del contenido: una venta inválida (pago que no cuadra, total cero,
        // medio no admitido) se rechaza aquí y no se encola. Antes quedaba PENDIENTE y, hasta que
        // el barrido la marcaba ERROR (30 s), bloqueaba el cierre de caja.
        VentaPosPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<VentaPosPayload>(request.Venta.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return (null, "El payload de venta no es válido.", false, false);
        }

        var validation = Validate(payload);
        if (validation is not null) return (null, validation, false, false);

        var pending = new PosVentaPendiente
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = request.DispositivoPosId,
            SesionCajaPosId = request.SesionCajaPosId,
            IdempotencyKey = request.IdempotencyKey,
            PayloadJson = request.Venta.GetRawText(),
            FechaVentaLocal = request.FechaVentaLocal == default ? DateTimeOffset.UtcNow : request.FechaVentaLocal,
            FechaRecibidoServidor = DateTimeOffset.UtcNow,
            Estado = "PENDIENTE"
        };
        _db.PosVentaPendientes.Add(pending);
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            var concurrent = await FindAsync(request.DispositivoPosId, request.IdempotencyKey, cancellationToken);
            if (concurrent is null) throw;
            return (Map(concurrent), null, false, false);
        }
        return (Map(pending), null, false, true);
    }

    public async Task<VentaPosPendienteDto?> EstadoAsync(Guid deviceId, string key, CancellationToken cancellationToken)
    {
        var consulta = _db.PosVentaPendientes.AsNoTracking()
            .Where(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == key);
        if (EntidadSolicitante() is Guid entidad)
            consulta = consulta.Where(x => x.DispositivoPos.EntidadId == entidad);
        return Map(await consulta.FirstOrDefaultAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<VentaPosPendienteDto>> PendientesAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        if (deviceId == Guid.Empty) return Array.Empty<VentaPosPendienteDto>();
        var items = await _db.PosVentaPendientes.AsNoTracking()
            .Where(x => x.DispositivoPosId == deviceId && (x.Estado == "PENDIENTE" || x.Estado == "ERROR"))
            .OrderByDescending(x => x.FechaRecibidoServidor)
            .ToListAsync(cancellationToken);
        return items.Select(Map).Where(x => x is not null).Cast<VentaPosPendienteDto>().ToList();
    }

    public async Task<(bool Succeeded, string Message)> DescartarAsync(Guid deviceId, string idempotencyKey, CancellationToken cancellationToken)
    {
        if (deviceId == Guid.Empty || string.IsNullOrWhiteSpace(idempotencyKey)) return (false, "Dispositivo y clave son obligatorios.");
        var pending = await _db.PosVentaPendientes.FirstOrDefaultAsync(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (pending is null) return (false, "Venta pendiente no encontrada.");
        if (pending.Estado == "PROCESADO") return (false, "La venta ya fue procesada; no se puede descartar.");
        pending.Estado = "DUPLICADO";
        pending.MensajeError = "Descartada por el cajero desde el terminal web.";
        pending.IntentosProcesamiento++;
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return (false, "No se pudo descartar la venta."); }
        return (true, "Venta descartada.");
    }

    public async Task MarcarPendienteErrorAsync(Guid deviceId, string idempotencyKey, string mensaje, CancellationToken cancellationToken)
    {
        var pending = await _db.PosVentaPendientes.FirstOrDefaultAsync(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (pending is null || pending.Estado != "PENDIENTE") return;
        pending.Estado = "ERROR";
        pending.MensajeError = mensaje;
        pending.IntentosProcesamiento++;
        try { await _db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { /* best effort */ }
    }

    public async Task<(string? Numero, string? Error)> SiguienteNumeroAsync(Guid deviceId, string serie, CancellationToken cancellationToken)
    {
        var device = await _db.DispositivoPos.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == deviceId, cancellationToken);
        if (device is null || (EntidadSolicitante() is Guid entidad && device.EntidadId != entidad))
            return (null, "Dispositivo POS no encontrado.");
        if (string.IsNullOrWhiteSpace(serie) || serie.Length > 10)
            return (null, "Serie de facturación inválida.");

        // Si el servidor reservó bloques a este dispositivo para la serie, la numeración tiene que
        // salir del bloque: es exactamente lo que exige ProcesarCoreAsync al validar la venta. Sin
        // bloques reservados la asigna el consecutivo de la entidad, igual que el resto del ERP.
        var rangos = await _db.PosRangoNumeracions.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.DispositivoPosId == deviceId && r.Serie == serie && !r.Agotado)
            .OrderBy(r => r.NumeroDesde)
            .Select(r => new { r.NumeroDesde, r.NumeroHasta, r.NumeroSiguienteLocal })
            .ToListAsync(cancellationToken);

        foreach (var rango in rangos)
        {
            var ultimo = await UltimoNumeroEnBloqueAsync(device, serie, rango.NumeroDesde, rango.NumeroHasta, cancellationToken);
            var siguiente = Math.Max(rango.NumeroDesde, Math.Max(ultimo, rango.NumeroSiguienteLocal) + 1);
            if (siguiente <= rango.NumeroHasta)
                return (siguiente.ToString().PadLeft(rango.NumeroHasta.ToString().Length, '0'), null);
        }

        if (rangos.Count > 0)
            return (null, $"Los bloques de numeración reservados al dispositivo para la serie {serie} están agotados.");

        var numero = await _consecutivo.ObtenerSiguienteNumeroAsync(
            device.EntidadId, device.SucursalId, DocumentoTipo.FacturaVenta, serie);
        return (numero, null);
    }

    // TRY_CAST del lado del servidor: en comercial.factura_venta conviven numeraciones no
    // numéricas (p. ej. "SMK-9001" de las pruebas por HTTP), que un CAST directo haría estallar.
    private async Task<long> UltimoNumeroEnBloqueAsync(DispositivoPo device, string serie, long desde, long hasta, CancellationToken cancellationToken)
    {
        return await _db.Database.SqlQuery<long>($@"
            SELECT ISNULL(MAX(TRY_CAST(numero_factura AS bigint)), 0) AS [Value]
            FROM comercial.factura_venta
            WHERE entidad_id = {device.EntidadId}
              AND sucursal_id = {device.SucursalId}
              AND serie = {serie}
              AND TRY_CAST(numero_factura AS bigint) BETWEEN {desde} AND {hasta}").FirstAsync(cancellationToken);
    }

    public async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict)> ProcesarAsync(Guid deviceId, string key, CancellationToken cancellationToken)
    {
        // Bajo concurrencia de varios dispositivos sobre el mismo almacén puede ocurrir
        // un interbloqueo (1205) entre transacciones SERIALIZABLE; se reintenta de forma acotada.
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var resultado = await ProcesarCoreAsync(deviceId, key, cancellationToken);
                if (resultado.Error is not null && !resultado.Conflict)
                    await MarcarPendienteErrorAsync(deviceId, key, resultado.Error, cancellationToken);
                return resultado;
            }
            catch (DbUpdateException ex) when (EsNumeracionDuplicada(ex))
            {
                await RevertirCambiosFallidosAsync(cancellationToken);
                // Carrera entre el worker y la API: si esta misma venta ya quedó procesada
                // (otra transacción insertó la factura), el duplicado no es un error de datos
                // sino que ya está hecha; se devuelve la factura existente para no marcarla
                // ERROR ni provocar un reintento/venta duplicada en el terminal.
                var yaProcesada = await _db.PosVentaPendientes.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.DispositivoPosId == deviceId && p.IdempotencyKey == key, cancellationToken);
                if (yaProcesada is { Estado: "PROCESADO", FacturaId: not null })
                    return (Map(yaProcesada), null, false);

                await MarcarPendienteErrorAsync(deviceId, key, "Ya existe una factura con esa numeracion (serie y numero) en la entidad y sucursal de la venta.", cancellationToken);


                return (null, "Ya existe una factura con esa numeración (serie y número) en la entidad y sucursal de la venta.", false);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 1205 })
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
            catch (DbUpdateException)
            {
                await RevertirCambiosFallidosAsync(cancellationToken);
                throw;
            }
        }
        return (null, "Ocurrió un conflicto de concurrencia al procesar la venta; reintente.", true);
    }

    private static bool EsNumeracionDuplicada(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 } ||
        ex.InnerException?.InnerException is SqlException { Number: 2601 or 2627 };

    // Tras un SaveChanges fallido el contexto queda sucio: ProcesarCoreAsync ya marcó el
    // pendiente PROCESADO (y sumó los totales de la sesión) en memoria antes de guardar, así
    // que un SaveChanges posterior —el de MarcarErroresAsync del worker, por ejemplo— persistiría
    // un PROCESADO con un FacturaId inexistente. Se descartan los Added y se recarga lo modificado.
    private async Task RevertirCambiosFallidosAsync(CancellationToken cancellationToken)
    {
        foreach (var entrada in _db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
            entrada.State = EntityState.Detached;

        foreach (var entrada in _db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList())
            await entrada.ReloadAsync(cancellationToken);
    }

    private async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict)> ProcesarCoreAsync(Guid deviceId, string key, CancellationToken cancellationToken)
    {
        // Unirse a la transacción ambiente si el llamador ya abrió una (mismo patrón que
        // InventoryService y CreateEntryAsync): sin ella se abre una SERIALIZABLE propia.
        var ownsTransaction = _db.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
        // IgnoreQueryFilters: el query filter global (entidad) depende del HttpContext,
        // que no existe en el scope del background worker => excluiría TODAS las filas.
        var pending = await _db.PosVentaPendientes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == key, cancellationToken);
        if (pending is null) return (null, null, false);
        if (pending.Estado == "PROCESADO") return (Map(pending), null, false);
        if (pending.Estado != "PENDIENTE") return (Map(pending), null, true);

        VentaPosPayload? sale;
        try { sale = JsonSerializer.Deserialize<VentaPosPayload>(pending.PayloadJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch (JsonException) { return (null, "El payload de venta no es válido.", false); }

        var error = Validate(sale);
        if (error is not null) return (null, error, false);
        var data = sale!;

        var device = await _db.DispositivoPos.AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == deviceId, cancellationToken);
        var entidadSolicitante = EntidadSolicitante();
        if (device is null || device.EntidadId != data.EntidadId || device.AlmacenId != data.AlmacenId ||
            (entidadSolicitante is Guid ent && device.EntidadId != ent))
            return (null, "La venta no corresponde al dispositivo POS.", false);

        // Numeración: si el servidor reservó rangos a este dispositivo para la serie, el número
        // debe caer dentro de uno de ellos. Sin rango reservado se conserva el comportamiento
        // previo (el terminal asigna la numeración y el único freno es el índice único).
        var rangos = await _db.PosRangoNumeracions.IgnoreQueryFilters()
            .Where(r => r.DispositivoPosId == deviceId && r.Serie == data.Serie)
            .Select(r => new { r.NumeroDesde, r.NumeroHasta })
            .ToListAsync(cancellationToken);
        if (rangos.Count > 0)
        {
            if (!long.TryParse(data.NumeroFactura, out var numero))
                return (null, $"La numeración '{data.NumeroFactura}' no es válida para la serie {data.Serie}.", false);
            if (!rangos.Any(r => numero >= r.NumeroDesde && numero <= r.NumeroHasta))
                return (null, $"La numeración {data.Serie}-{data.NumeroFactura} está fuera del rango reservado al dispositivo.", true);
        }

        var entidadValida = await _db.Entidads.IgnoreQueryFilters().AnyAsync(x => x.Id == data.EntidadId && x.Activo, cancellationToken);
        var sucursalValida = await _db.Sucursals.IgnoreQueryFilters().AnyAsync(x => x.Id == data.SucursalId && x.EntidadId == data.EntidadId && x.Activo, cancellationToken);
        var clienteValido = await _db.Clientes.IgnoreQueryFilters().AnyAsync(x => x.Id == data.ClienteId && x.EntidadId == data.EntidadId && x.Activo, cancellationToken);
        var almacenValido = await _db.Almacens.IgnoreQueryFilters().AnyAsync(x => x.Id == data.AlmacenId && x.EntidadId == data.EntidadId && x.Activo, cancellationToken);
        var tipoMovimientoValido = await _db.TipoMovimientos.AnyAsync(x => x.Id == data.TipoMovimientoId, cancellationToken);
        if (!entidadValida || !sucursalValida || !clienteValido || !almacenValido || !tipoMovimientoValido)
            return (null, "Una referencia de la venta no existe o está inactiva.", false);

        var ids = data.Lineas.Select(x => x.ProductoId).Distinct().ToList();
        var products = await _db.Productos.IgnoreQueryFilters().Where(x => x.Activo && ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var stock = await _db.Existencia.IgnoreQueryFilters().Where(x => x.AlmacenId == data.AlmacenId && ids.Contains(x.ProductoId)).ToDictionaryAsync(x => x.ProductoId, cancellationToken);

        // Validación por SUMA de cantidades por producto (varias líneas del mismo artículo),
        // no línea a línea, para evitar dejar stock negativo.
        var demand = data.Lineas
            .GroupBy(x => x.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));
        if (products.Count != ids.Count ||
            demand.Any(kv => !stock.ContainsKey(kv.Key) || stock[kv.Key].Cantidad < kv.Value))
            return (null, "Producto inexistente o existencia insuficiente.", true);

        var (subtotal, discount, tax, total) = Totales(data);
        // Una venta de importe cero no debe facturarse (Validate ya lo rechaza al recibir; se
        // mantiene aquí porque el worker o un payload antiguo podrían llegar sin pasar por Recibir).
        if (total <= 0)
            return (null, "El total de la venta debe ser mayor que cero.", false);
        if (Math.Abs(data.Pagos.Sum(x => x.Monto - (x.VueltoEntregado ?? 0m)) - total) > 0.01m)
            return (null, "Los pagos no coinciden con el total de la venta.", false);

        // IgnoreQueryFilters: el query filter global (vía caja → entidad) depende del HttpContext;
        // en el scope del worker no existe y vaciaría la sesión. La pertenencia a la caja/entidad
        // ya se valida explícitamente por ids aquí abajo (data.CajaId, data.EntidadId).
        var session = await _db.SesionCajaPos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == pending.SesionCajaPosId, cancellationToken);
        if (session is null) return (null, "La sesión POS no existe.", false);
        if (session.Estado != "ABIERTA") return (null, "La sesión de caja está cerrada; procese la venta antes del cierre.", true);

        var now = DateTimeOffset.UtcNow;
        var invoiceId = Guid.NewGuid();
        var movementId = Guid.NewGuid();
        var facturaFecha = data.Fecha == default ? pending.FechaVentaLocal : data.Fecha;

        var invoice = new FacturaVentum
        {
            Id = invoiceId,
            EntidadId = data.EntidadId,
            SucursalId = data.SucursalId,
            NumeroFactura = data.NumeroFactura,
            Serie = data.Serie,
            ClienteId = data.ClienteId,
            AlmacenId = data.AlmacenId,
            CanalVenta = "POS",
            TipoVenta = "MINORISTA",
            DispositivoPosId = deviceId,
            SesionCajaPosId = pending.SesionCajaPosId,
            Fecha = facturaFecha,
            Subtotal = Math.Round(subtotal, 2),
            DescuentoTotal = discount,
            ImpuestoVentasTotal = tax,
            Total = total,
            Moneda = data.Moneda,
            Estado = "EMITIDA",
            CreadoPor = session.CajeroId,
            CreadoEn = now
        };
        _db.FacturaVenta.Add(invoice);

        var movement = new MovimientoInventario
        {
            Id = movementId,
            EntidadId = data.EntidadId,
            TipoMovimientoId = data.TipoMovimientoId,
            NumeroDocumento = string.IsNullOrWhiteSpace(data.Serie)
                ? data.NumeroFactura
                : $"{data.Serie}-{data.NumeroFactura}",
            AlmacenOrigenId = data.AlmacenId,
            Fecha = facturaFecha,
            ReferenciaExternaTipo = "FACTURA_VENTA",
            ReferenciaExternaId = invoiceId,
            Canal = "POS",
            DispositivoPosId = deviceId,
            CreadoPor = session.CajeroId,
            CreadoEn = now
        };
        _db.MovimientoInventarios.Add(movement);

        foreach (var line in data.Lineas)
        {
            var gross = line.Cantidad * line.PrecioUnitario;
            var lineDiscount = Math.Round(gross * line.DescuentoPorcentaje / 100m, 2);
            _db.FacturaVentaDetalles.Add(new FacturaVentaDetalle
            {
                Id = Guid.NewGuid(),
                FacturaId = invoiceId,
                ProductoId = line.ProductoId,
                Cantidad = line.Cantidad,
                PrecioUnitario = line.PrecioUnitario,
                DescuentoPorcentaje = line.DescuentoPorcentaje,
                CostoUnitarioVenta = stock[line.ProductoId].CostoPromedio,
                ImpuestoPorcentaje = line.ImpuestoPorcentaje,
                SubtotalLinea = Math.Round(gross - lineDiscount, 2),
                MovimientoInventarioId = movementId
            });
            movement.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                MovimientoId = movementId,
                ProductoId = line.ProductoId,
                Cantidad = line.Cantidad,
                CostoUnitario = stock[line.ProductoId].CostoPromedio
            });
        }

        // Decremento atómico y condicional por producto: si otro dispositivo ya agotó el
        // remanente, el UPDATE afecta 0 filas y esta transacción se revierte con Conflict.
        // El orden por ProductoId evita interbloqueos entre dispositivos con el mismo set de ítems.
        foreach (var kv in demand.OrderBy(x => x.Key))
        {
            var affected = await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE inventario.existencia SET cantidad = cantidad - {kv.Value}, actualizado_en = SYSDATETIMEOFFSET() WHERE almacen_id = {data.AlmacenId} AND producto_id = {kv.Key} AND cantidad >= {kv.Value}",
                cancellationToken);
            if (affected == 0) return (null, "Producto inexistente o existencia insuficiente.", true);
        }

        foreach (var payment in data.Pagos)
        {
            _db.FormaPagoVenta.Add(new FormaPagoVentum
            {
                Id = Guid.NewGuid(),
                FacturaId = invoiceId,
                FormaPago = payment.FormaPago,
                Monto = payment.Monto,
                ReferenciaExterna = payment.ReferenciaExterna,
                VueltoEntregado = payment.VueltoEntregado
            });
        }

        session.TotalVentas += total;
        session.TotalEfectivo += data.Pagos.Where(x => x.FormaPago == "EFECTIVO").Sum(x => x.Monto - (x.VueltoEntregado ?? 0m));
        session.TotalTransfermovil += data.Pagos.Where(x => x.FormaPago == "TRANSFERMOVIL").Sum(x => x.Monto);
        session.TotalEnzona += data.Pagos.Where(x => x.FormaPago == "ENZONA").Sum(x => x.Monto);
        session.TotalOtrosMedios += data.Pagos.Where(x => x.FormaPago is not "EFECTIVO" and not "TRANSFERMOVIL" and not "ENZONA").Sum(x => x.Monto);
        session.CantidadFacturas++;

        // Contabilidad de la venta POS (P0-4): la factura y el movimiento se crean aquí
        // (fuera de SalesService/InventoryService), así que registramos los asientos de
        // venta (caja/ingresos) y de costo (inventario/costo) dentro de la misma transacción.
        await CrearAsientoVentaYCosteAsync(data, invoice, movement, subtotal, discount, tax, total, facturaFecha, session.CajeroId, cancellationToken);

        pending.Estado = "PROCESADO";
        pending.FacturaId = invoiceId;
        pending.ProcesadoEn = now;
        pending.IntentosProcesamiento++;

        await _db.SaveChangesAsync(cancellationToken);
        if (ownsTransaction) await transaction!.CommitAsync(cancellationToken);

        await _notifications.NotifyEntityAsync(data.EntidadId, "Venta POS procesada",
            $"{device.Nombre}: venta {data.Serie}{data.NumeroFactura} por {total:N2} {data.Moneda}.", "success");

        return (Map(pending), null, false);
    }

    // Asiento de VENTA (caja / ingresos: replica el patrón contable de SalesService) y de
    // COSTO (inventario / costo de venta: delegado a InventoryService, fuente única contable).
    private async Task CrearAsientoVentaYCosteAsync(VentaPosPayload data, FacturaVentum invoice, MovimientoInventario movement,
        decimal subtotal, decimal discount, decimal tax, decimal total, DateTimeOffset facturaFecha, Guid cajeroId, CancellationToken ct)
    {
        var tipoCompIng = await _db.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "ING", ct);
        if (tipoCompIng != null)
        {
            var period = await _accounting.GetOrCreateActivePeriodAsync(data.EntidadId, facturaFecha.DateTime);
            if (period != null && period.Estado == "ABIERTO")
            {
                var entry = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    EntidadId = data.EntidadId,
                    PeriodoId = period.Id,
                    Fecha = DateOnly.FromDateTime(facturaFecha.DateTime),
                    Concepto = $"VENTA POS {data.Serie}-{data.NumeroFactura}",
                    ModuloOrigen = "VENTAS",
                    DocumentoOrigenTipo = "FACTURA_VENTA",
                    DocumentoOrigenId = invoice.Id,
                    TipoComprobanteId = tipoCompIng.Id,
                    CreadoPor = cajeroId,
                    CreadoEn = DateTimeOffset.UtcNow,
                    Estado = "CONTABILIZADO"
                };

                var ctaCaja = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_CAJA_MN")) ?? "101";
                var ctaBanco = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_BANCO_MN")) ?? "109";
                var ctaVentas = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_VENTAS_GENERAL")) ?? "900.0000";
                var ctaImp = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_IMP_VENTAS")) ?? "440.0001";

                // Cada medio de pago entra en su cuenta: el efectivo a caja y las transferencias
                // (Transfermóvil, Enzona, banco, cheque) a la cuenta bancaria configurada. Antes
                // todo el cobrado se debitaba en caja, incluso lo que nunca pasó por el cajón.
                var cajaAcc = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == data.EntidadId, ct);
                var bancoAcc = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaBanco && c.EntidadId == data.EntidadId, ct) ?? cajaAcc;

                var pagosNetos = data.Pagos
                    .Select(p => new { p.FormaPago, Neto = p.Monto - (p.VueltoEntregado ?? 0m) })
                    .GroupBy(p => p.FormaPago)
                    .Select(g => new { FormaPago = g.Key, Monto = Math.Round(g.Sum(x => x.Neto), 2) })
                    .Where(x => x.Monto > 0)
                    .ToList();

                foreach (var pago in pagosNetos)
                {
                    var cuenta = pago.FormaPago == "EFECTIVO" ? cajaAcc : bancoAcc;
                    if (cuenta != null)
                        entry.AsientoDetalles.Add(new AsientoDetalle
                        {
                            Id = Guid.NewGuid(),
                            CuentaId = cuenta.Id,
                            Debe = pago.Monto,
                            Glosa = "Cobro " + pago.FormaPago
                        });
                }

                var totalPagado = pagosNetos.Sum(x => x.Monto);

                if (invoice.Total - totalPagado > 0)
                {
                    var ctaCxC = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_CXC_CLIENTES")) ?? "135";
                    var cxcAcc = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCxC && c.EntidadId == data.EntidadId, ct);
                    if (cxcAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cxcAcc.Id, Debe = Math.Round(invoice.Total - totalPagado, 2), Glosa = "Venta a Crédito" });
                }

                var ventasAcc = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaVentas && c.EntidadId == data.EntidadId, ct);
                if (ventasAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ventasAcc.Id, Haber = Math.Round(subtotal - discount, 2), Glosa = "Ingresos por Ventas" });

                if (tax > 0)
                {
                    var impAcc = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaImp && c.EntidadId == data.EntidadId, ct);
                    if (impAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = impAcc.Id, Haber = Math.Round(tax, 2), Glosa = "Impuesto sobre Ventas" });
                }

                if (entry.AsientoDetalles.Sum(d => d.Debe) == entry.AsientoDetalles.Sum(d => d.Haber))
                {
                    var res = await _accounting.CreateEntryAsync(entry);
                    if (res.Succeeded) invoice.AsientoId = entry.Id;
                    else _logger.LogWarning("POS: la venta {Serie}-{Numero} no se pudo contabilizar: {Motivo}",
                        data.Serie, data.NumeroFactura, res.Message);
                }
                else
                {
                    _logger.LogWarning(
                        "POS: asiento de venta descuadrado {Serie}-{Numero} (debe {Debe}, haber {Haber}); " +
                        "revisar parámetros CTA_CAJA_MN/CTA_VENTAS_GENERAL/CTA_IMP_VENTAS/CTA_CXC_CLIENTES.",
                        data.Serie, data.NumeroFactura,
                        entry.AsientoDetalles.Sum(d => d.Debe), entry.AsientoDetalles.Sum(d => d.Haber));
                }
            }
            else if (period == null)
            {
                // Sin periodo contable abierto no hay asiento de venta (misma salvaguarda que SalesService).
                _logger.LogWarning("POS: no existe periodo contable abierto para {Fecha}; la venta {Serie}-{Numero} queda sin asiento de venta.",
                    facturaFecha, data.Serie, data.NumeroFactura);
            }
        }

        // Asiento de COSTO (inventario ↔ costo): reutiliza la lógica canónica de InventoryService.
        // Un fallo de datos (producto sin cuentas y sin cuenta genérica válida) avisa pero no
        // tumba la venta: el movimiento queda pendiente de contabilizar (AsientoId NULL).
        try
        {
            await _inventory.GenerateAccountingEntryForExistingMovementAsync(movement, ct);
        }
        catch (InvalidOperationException ex)
        {
            foreach (var huerfano in _db.ChangeTracker.Entries<AsientoContable>()
                .Where(e => e.State == EntityState.Added && e.Entity.DocumentoOrigenId == movement.Id).ToList())
                huerfano.State = EntityState.Detached;

            movement.AsientoId = null;
            _logger.LogWarning(ex,
                "POS: no se pudo contabilizar el costo del movimiento {Movimiento} de la venta {Serie}-{Numero}; queda pendiente de contabilizar.",
                movement.Id, data.Serie, data.NumeroFactura);
        }
    }

    public async Task<(FacturaPosDto? Result, string? Error)> ObtenerFacturaAsync(Guid deviceId, string serie, string numero, CancellationToken cancellationToken)
    {
        if (deviceId == Guid.Empty || string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(numero))
            return (null, "Indique el terminal y el número de factura.");

        var device = await _db.DispositivoPos.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == deviceId, cancellationToken);
        if (device is null || (EntidadSolicitante() is Guid entidad && device.EntidadId != entidad))
            return (null, "Dispositivo POS no encontrado.");

        var factura = await _db.FacturaVenta.IgnoreQueryFilters().AsNoTracking()
            .Include(f => f.FacturaVentaDetalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.EntidadId == device.EntidadId && f.SucursalId == device.SucursalId
                && f.Serie == serie && f.NumeroFactura == numero && f.CanalVenta == "POS", cancellationToken);
        if (factura is null)
            return (null, "No se encontró una factura POS con esa numeración en la sucursal del terminal.");

        var devueltos = await DevolucionesPorDetalleAsync(factura.Id, cancellationToken);
        return (new FacturaPosDto
        {
            FacturaId = factura.Id,
            Serie = factura.Serie,
            NumeroFactura = factura.NumeroFactura,
            Fecha = factura.Fecha,
            Total = factura.Total,
            Estado = factura.Estado,
            Lineas = factura.FacturaVentaDetalles
                .OrderBy(d => d.Producto.Nombre)
                .Select(d => new FacturaPosLineaDto
                {
                    FacturaDetalleId = d.Id,
                    ProductoId = d.ProductoId,
                    Producto = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    CantidadDevuelta = devueltos.TryGetValue(d.Id, out var devuelta) ? devuelta : 0m,
                    PrecioUnitario = d.PrecioUnitario,
                    DescuentoPorcentaje = d.DescuentoPorcentaje,
                    ImpuestoPorcentaje = d.ImpuestoPorcentaje,
                    SubtotalLinea = d.SubtotalLinea
                })
                .ToList()
        }, null);
    }

    private async Task<Dictionary<Guid, decimal>> DevolucionesPorDetalleAsync(Guid facturaId, CancellationToken cancellationToken) =>
        await _db.DevolucionVentaDetalles.AsNoTracking()
            .Where(d => d.Devolucion.FacturaId == facturaId)
            .GroupBy(d => d.FacturaDetalleId)
            .Select(g => new { g.Key, Cantidad = g.Sum(x => x.CantidadDevuelta) })
            .ToDictionaryAsync(x => x.Key, x => x.Cantidad, cancellationToken);

    public async Task<(DevolucionPosResultDto? Result, string? Error)> ProcesarDevolucionAsync(DevolucionPosRequest request, CancellationToken cancellationToken)
    {
        if (request.DispositivoPosId == Guid.Empty || request.SesionCajaPosId == Guid.Empty)
            return (null, "Dispositivo y sesión son obligatorios.");
        if (string.IsNullOrWhiteSpace(request.Serie) || string.IsNullOrWhiteSpace(request.NumeroFactura))
            return (null, "Indique la serie y el número de la factura.");
        if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length > 200)
            return (null, "El motivo de la devolución es obligatorio (hasta 200 caracteres).");
        if (request.Lineas.Count == 0 || request.Lineas.Any(l => l.FacturaDetalleId == Guid.Empty || l.Cantidad <= 0))
            return (null, "Indique al menos un artículo con cantidad mayor que cero.");

        var formas = new[] { "EFECTIVO", "TRANSFERMOVIL", "ENZONA", "TRANSFERENCIA_BANCARIA", "CHEQUE" };
        if (!formas.Contains(request.FormaPagoReembolso))
            return (null, "La forma de reembolso no es válida.");

        var device = await _db.DispositivoPos.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DispositivoPosId, cancellationToken);
        if (device is null || (EntidadSolicitante() is Guid entidad && device.EntidadId != entidad))
            return (null, "Dispositivo POS no encontrado.");

        var session = await _db.SesionCajaPos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.SesionCajaPosId, cancellationToken);
        if (session is null || session.CajaId != device.CajaId)
            return (null, "La sesión POS no corresponde al terminal.");
        if (session.Estado != "ABIERTA")
            return (null, "La caja está cerrada; abra un turno para registrar devoluciones.");

        var invoice = await _db.FacturaVenta.IgnoreQueryFilters()
            .Include(f => f.FacturaVentaDetalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.EntidadId == device.EntidadId && f.SucursalId == device.SucursalId
                && f.Serie == request.Serie.Trim() && f.NumeroFactura == request.NumeroFactura.Trim()
                && f.CanalVenta == "POS", cancellationToken);
        if (invoice is null)
            return (null, "No se encontró una factura POS con esa numeración en la sucursal del terminal.");
        if (invoice.Estado != "EMITIDA")
            return (null, "La factura no está vigente (anulada o ya devuelta).");

        var devueltos = await DevolucionesPorDetalleAsync(invoice.Id, cancellationToken);
        var detalles = invoice.FacturaVentaDetalles.ToDictionary(d => d.Id);
        var reclamadas = new Dictionary<Guid, decimal>();

        foreach (var linea in request.Lineas)
        {
            if (!detalles.TryGetValue(linea.FacturaDetalleId, out var detalle))
                return (null, "Una de las líneas no pertenece a la factura.");
            var yaDevuelta = devueltos.TryGetValue(detalle.Id, out var d) ? d : 0m;
            reclamadas[detalle.Id] = (reclamadas.TryGetValue(detalle.Id, out var r) ? r : 0m) + linea.Cantidad;
            if (reclamadas[detalle.Id] + yaDevuelta > detalle.Cantidad)
                return (null, $"La cantidad a devolver de {detalle.Producto?.Nombre ?? detalle.ProductoId.ToString()} excede lo vendido (vendido {detalle.Cantidad:0.###}, ya devuelto {yaDevuelta:0.###}).");
        }

        // Importe a reembolsar: parte proporcional del neto de la línea más su impuesto.
        var lineas = request.Lineas.Select(linea =>
        {
            var detalle = detalles[linea.FacturaDetalleId];
            var proporcion = detalle.Cantidad <= 0 ? 0m : linea.Cantidad / detalle.Cantidad;
            var neto = Math.Round(detalle.SubtotalLinea * proporcion, 2);
            var impuesto = Math.Round(neto * detalle.ImpuestoPorcentaje / 100m, 2);
            return new { Detalle = detalle, Linea = linea, Neto = neto, Impuesto = impuesto, Total = neto + impuesto };
        }).ToList();

        var totalDevuelto = Math.Round(lineas.Sum(x => x.Total), 2);
        if (totalDevuelto <= 0)
            return (null, "El importe de la devolución debe ser mayor que cero.");

        var ownsTransaction = _db.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

        var devolucionId = Guid.NewGuid();
        var movementId = Guid.NewGuid();
        var cajeroId = EntidadSolicitanteUsuario() ?? session.CajeroId;
        var ahora = DateTimeOffset.UtcNow;

        var movement = new MovimientoInventario
        {
            Id = movementId,
            EntidadId = invoice.EntidadId,
            TipoMovimientoId = await _inventory.EnsureMovementTypeAsync("DEVOLUCION_ENTRADA"),
            NumeroDocumento = $"DEV-{invoice.Serie}-{invoice.NumeroFactura}-{devolucionId.ToString("N")[..8]}",
            AlmacenDestinoId = device.AlmacenId,
            Fecha = ahora,
            ReferenciaExternaTipo = "DEVOLUCION_VENTA",
            ReferenciaExternaId = devolucionId,
            Canal = "POS",
            DispositivoPosId = device.Id,
            CreadoPor = cajeroId
        };
        foreach (var linea in lineas)
        {
            movement.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                MovimientoId = movementId,
                ProductoId = linea.Detalle.ProductoId,
                Cantidad = linea.Linea.Cantidad,
                CostoUnitario = linea.Detalle.CostoUnitarioVenta
            });
        }

        var movimiento = await _inventory.ProcessMovementAsync(movement);
        if (!movimiento.Succeeded)
        {
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            return (null, $"No se pudo devolver el inventario: {movimiento.Message}");
        }

        var devolucion = new DevolucionVentum
        {
            Id = devolucionId,
            FacturaId = invoice.Id,
            Fecha = DateOnly.FromDateTime(ahora.DateTime),
            Motivo = request.Motivo.Trim(),
            TotalDevuelto = totalDevuelto,
            MovimientoInventarioId = movementId,
            AutorizadoPor = cajeroId
        };
        foreach (var linea in lineas)
        {
            devolucion.DevolucionVentaDetalles.Add(new DevolucionVentaDetalle
            {
                Id = Guid.NewGuid(),
                DevolucionId = devolucionId,
                FacturaDetalleId = linea.Detalle.Id,
                CantidadDevuelta = linea.Linea.Cantidad
            });
        }
        _db.DevolucionVenta.Add(devolucion);
        await _db.SaveChangesAsync(cancellationToken);

        // Reverso de la venta: se acredita el medio con el que se reembolsa y se debitan ingresos
        // e impuesto por la parte devuelta. El costo lo revierte InventoryService con el movimiento.
        var tipoCompIng = await _db.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "ING", cancellationToken);
        var period = await _accounting.GetOrCreateActivePeriodAsync(invoice.EntidadId, ahora.DateTime);
        if (tipoCompIng is null || period is null || period.Estado != "ABIERTO")
        {
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            return (null, "No hay periodo contable abierto para registrar el reembolso.");
        }

        var ctaCaja = (await _parametros.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_CAJA_MN")) ?? "101";
        var ctaBanco = (await _parametros.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_BANCO_MN")) ?? "109";
        var ctaVentas = (await _parametros.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_VENTAS_GENERAL")) ?? "900.0000";
        var ctaImp = (await _parametros.ObtenerValorVigenteAsync(invoice.EntidadId, "CTA_IMP_VENTAS")) ?? "440.0001";
        var codigoReembolso = request.FormaPagoReembolso == "EFECTIVO" ? ctaCaja : ctaBanco;

        var cuentaVentas = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaVentas && c.EntidadId == invoice.EntidadId, cancellationToken);
        var cuentaReembolso = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == codigoReembolso && c.EntidadId == invoice.EntidadId, cancellationToken)
            ?? await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == invoice.EntidadId, cancellationToken);
        var cuentaImp = await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaImp && c.EntidadId == invoice.EntidadId, cancellationToken);

        if (cuentaVentas is null || cuentaReembolso is null)
        {
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            return (null, "Faltan cuentas contables (ventas/caja) para registrar el reembolso.");
        }

        var netoDevuelto = Math.Round(lineas.Sum(x => x.Neto), 2);
        var impuestoDevuelto = Math.Round(lineas.Sum(x => x.Impuesto), 2);

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = invoice.EntidadId,
            SucursalId = invoice.SucursalId,
            PeriodoId = period.Id,
            Fecha = DateOnly.FromDateTime(ahora.DateTime),
            Concepto = $"DEVOLUCION POS {invoice.Serie}-{invoice.NumeroFactura}",
            ModuloOrigen = "VENTAS",
            DocumentoOrigenTipo = "DEVOLUCION_VENTA",
            DocumentoOrigenId = devolucion.Id,
            TipoComprobanteId = tipoCompIng.Id,
            CreadoPor = cajeroId,
            CreadoEn = ahora,
            Estado = "CONTABILIZADO"
        };
        entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaVentas.Id, Debe = netoDevuelto, Glosa = "Devolución de venta" });
        if (impuestoDevuelto > 0 && cuentaImp != null)
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaImp.Id, Debe = impuestoDevuelto, Glosa = "Impuesto devuelto" });
        entry.AsientoDetalles.Add(new AsientoDetalle
        {
            Id = Guid.NewGuid(),
            CuentaId = cuentaReembolso.Id,
            Haber = totalDevuelto,
            Glosa = "Reembolso " + request.FormaPagoReembolso
        });

        var asiento = await _accounting.CreateEntryAsync(entry);
        if (!asiento.Succeeded)
        {
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            return (null, $"No se pudo contabilizar la devolución: {asiento.Message}");
        }
        devolucion.AsientoId = entry.Id;

        session.TotalVentas -= totalDevuelto;
        switch (request.FormaPagoReembolso)
        {
            case "EFECTIVO": session.TotalEfectivo -= totalDevuelto; break;
            case "TRANSFERMOVIL": session.TotalTransfermovil -= totalDevuelto; break;
            case "ENZONA": session.TotalEnzona -= totalDevuelto; break;
            default: session.TotalOtrosMedios -= totalDevuelto; break;
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (ownsTransaction) await transaction!.CommitAsync(cancellationToken);

        return (new DevolucionPosResultDto
        {
            DevolucionId = devolucion.Id,
            FacturaId = invoice.Id,
            Serie = invoice.Serie,
            NumeroFactura = invoice.NumeroFactura,
            TotalDevuelto = totalDevuelto,
            FormaPagoReembolso = request.FormaPagoReembolso
        }, null);
    }

    private Guid? EntidadSolicitanteUsuario()
    {
        var id = _entidad.CurrentUsuarioId;
        return id == Guid.Empty ? null : id;
    }

    private async Task<PosVentaPendiente?> FindAsync(Guid deviceId, string key, CancellationToken token) =>
        await _db.PosVentaPendientes.AsNoTracking().FirstOrDefaultAsync(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == key, token);

    private static VentaPosPendienteDto? Map(PosVentaPendiente? x) => x is null ? null : new()
    {
        Id = x.Id,
        IdempotencyKey = x.IdempotencyKey,
        Estado = x.Estado,
        FacturaId = x.FacturaId,
        MensajeError = x.MensajeError,
        FechaRecibidoServidor = x.FechaRecibidoServidor,
        SesionCajaPosId = x.SesionCajaPosId
    };

    private static string? Validate(VentaPosPayload? sale)
    {
        if (sale is null || sale.EntidadId == Guid.Empty || sale.SucursalId == Guid.Empty || sale.ClienteId == Guid.Empty ||
            sale.AlmacenId == Guid.Empty || sale.TipoMovimientoId <= 0)
            return "Faltan referencias obligatorias de la venta.";
        if (string.IsNullOrWhiteSpace(sale.NumeroFactura) || sale.NumeroFactura.Length > 30 ||
            string.IsNullOrWhiteSpace(sale.Serie) || sale.Serie.Length > 10 ||
            sale.Moneda.Length != 3 || sale.Lineas.Count == 0 || sale.Pagos.Count == 0)
            return "La venta debe incluir numeración, moneda, líneas y pagos.";
        // El POS solo cobra el total: no hay crédito con CuentaPorCobrar ni verificación de
        // límite de crédito (a diferencia de SalesService), así que una venta a crédito
        // dejaría la deuda invisible en cartera. Se rechaza hasta que exista ese flujo.
        if (sale.Pagos.Any(x => x.FormaPago == "CREDITO"))
            return "El POS no admite ventas a crédito; cobre el total de la venta.";
        var paymentTypes = new[] { "EFECTIVO", "TRANSFERMOVIL", "ENZONA", "TRANSFERENCIA_BANCARIA", "CHEQUE" };
        if (sale.Lineas.Any(x => x.ProductoId == Guid.Empty || x.Cantidad <= 0 || x.PrecioUnitario < 0 ||
            x.DescuentoPorcentaje is < 0 or > 100 || x.ImpuestoPorcentaje is < 0 or > 100))
            return "La venta contiene líneas inválidas.";
        if (sale.Pagos.Any(x => !paymentTypes.Contains(x.FormaPago) || x.Monto < 0))
            return "La venta contiene medios de pago inválidos.";

        var totales = Totales(sale);
        if (totales.Total <= 0)
            return "El total de la venta debe ser mayor que cero.";
        if (Math.Abs(sale.Pagos.Sum(x => x.Monto - (x.VueltoEntregado ?? 0m)) - totales.Total) > 0.01m)
            return "Los pagos no coinciden con el total de la venta.";
        return null;
    }

    private static (decimal Subtotal, decimal Descuento, decimal Impuesto, decimal Total) Totales(VentaPosPayload sale)
    {
        var subtotal = sale.Lineas.Sum(x => x.Cantidad * x.PrecioUnitario);
        var descuento = sale.Lineas.Sum(x => Math.Round(x.Cantidad * x.PrecioUnitario * x.DescuentoPorcentaje / 100m, 2));
        var impuesto = sale.Lineas.Sum(x => Math.Round(
            (x.Cantidad * x.PrecioUnitario - x.Cantidad * x.PrecioUnitario * x.DescuentoPorcentaje / 100m) * x.ImpuestoPorcentaje / 100m, 2));
        return (subtotal, descuento, impuesto, Math.Round(subtotal - descuento + impuesto, 2));
    }
}