using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vercom.DTOs;
using Vercom.Models;

namespace Vercom.Services;

public sealed class PosSincronizacionService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IInventoryService _inventory;
    private readonly IAccountingService _accounting;
    private readonly IParametroSistemaService _parametros;

    public PosSincronizacionService(AppDbContext db, INotificationService notifications, IInventoryService inventory, IAccountingService accounting, IParametroSistemaService parametros)
    {
        _db = db;
        _notifications = notifications;
        _inventory = inventory;
        _accounting = accounting;
        _parametros = parametros;
    }

    public async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict, bool Accepted)> RecibirAsync(RecibirVentaPosRequest request, CancellationToken cancellationToken)
    {
        if (request.DispositivoPosId == Guid.Empty || request.SesionCajaPosId == Guid.Empty || string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 80)
            return (null, "Dispositivo, sesión y clave idempotente son obligatorios.", false, false);
        if (request.Venta.ValueKind != JsonValueKind.Object) return (null, "Venta debe ser un objeto JSON.", false, false);

        var device = await _db.DispositivoPos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DispositivoPosId, cancellationToken);
        if (device is null) return (null, "Dispositivo POS no encontrado.", true, false);

        // Sesión compartida: cualquier dispositivo de la misma caja puede registrar ventas
        // contra la sesión ABIERTA de esa caja, aunque la haya abierto otro dispositivo.
        var validSession = await _db.SesionCajaPos.AsNoTracking()
            .AnyAsync(x => x.Id == request.SesionCajaPosId && x.CajaId == device.CajaId && x.Estado == "ABIERTA", cancellationToken);
        if (!validSession) return (null, "La sesión POS no existe o no está abierta.", true, false);

        var existing = await FindAsync(request.DispositivoPosId, request.IdempotencyKey, cancellationToken);
        if (existing is not null) return (Map(existing), null, false, false);

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

    public async Task<VentaPosPendienteDto?> EstadoAsync(Guid deviceId, string key, CancellationToken cancellationToken) =>
        Map(await FindAsync(deviceId, key, cancellationToken));

    public async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict)> ProcesarAsync(Guid deviceId, string key, CancellationToken cancellationToken)
    {
        // Bajo concurrencia de varios dispositivos sobre el mismo almacén puede ocurrir
        // un interbloqueo (1205) entre transacciones SERIALIZABLE; se reintenta de forma acotada.
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await ProcesarCoreAsync(deviceId, key, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 1205 })
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
        }
        return (null, "Ocurrió un conflicto de concurrencia al procesar la venta; reintente.", true);
    }

    private async Task<(VentaPosPendienteDto? Result, string? Error, bool Conflict)> ProcesarCoreAsync(Guid deviceId, string key, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
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
        if (device is null || device.EntidadId != data.EntidadId || device.AlmacenId != data.AlmacenId) return (null, "La venta no corresponde al dispositivo POS.", false);

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

        var subtotal = data.Lineas.Sum(x => x.Cantidad * x.PrecioUnitario);
        var discount = data.Lineas.Sum(x => Math.Round(x.Cantidad * x.PrecioUnitario * x.DescuentoPorcentaje / 100m, 2));
        var tax = data.Lineas.Sum(x => Math.Round((x.Cantidad * x.PrecioUnitario - x.Cantidad * x.PrecioUnitario * x.DescuentoPorcentaje / 100m) * x.ImpuestoPorcentaje / 100m, 2));
        var total = Math.Round(subtotal - discount + tax, 2);
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
            NumeroDocumento = data.NumeroFactura,
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
        await transaction.CommitAsync(cancellationToken);

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
                var ctaVentas = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_VENTAS_GENERAL")) ?? "900";
                var ctaImp = (await _parametros.ObtenerValorVigenteAsync(data.EntidadId, "CTA_IMP_VENTAS")) ?? "440.0001";

                var totalPagado = data.Pagos.Sum(x => x.Monto - (x.VueltoEntregado ?? 0m));
                var cajaAcc = totalPagado > 0
                    ? await _db.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCaja && c.EntidadId == data.EntidadId, ct)
                    : null;

                if (cajaAcc != null) entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cajaAcc.Id, Debe = Math.Round(totalPagado, 2), Glosa = "Cobro Efectivo" });
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
                }
            }
            else if (period == null)
            {
                // Sin periodo contable abierto no hay asiento de venta (misma salvaguarda que SalesService).
            }
        }

        // Asiento de COSTO (inventario ↔ costo): reutiliza la lógica canónica de InventoryService.
        await _inventory.GenerateAccountingEntryForExistingMovementAsync(movement, ct);
    }

    private async Task<PosVentaPendiente?> FindAsync(Guid deviceId, string key, CancellationToken token) =>
        await _db.PosVentaPendientes.AsNoTracking().FirstOrDefaultAsync(x => x.DispositivoPosId == deviceId && x.IdempotencyKey == key, token);

    private static VentaPosPendienteDto? Map(PosVentaPendiente? x) => x is null ? null : new()
    {
        Id = x.Id,
        IdempotencyKey = x.IdempotencyKey,
        Estado = x.Estado,
        FacturaId = x.FacturaId,
        MensajeError = x.MensajeError
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
        var paymentTypes = new[] { "EFECTIVO", "TRANSFERMOVIL", "ENZONA", "TRANSFERENCIA_BANCARIA", "CHEQUE", "CREDITO" };
        if (sale.Lineas.Any(x => x.ProductoId == Guid.Empty || x.Cantidad <= 0 || x.PrecioUnitario < 0 ||
            x.DescuentoPorcentaje is < 0 or > 100 || x.ImpuestoPorcentaje is < 0 or > 100))
            return "La venta contiene líneas inválidas.";
        return sale.Pagos.Any(x => !paymentTypes.Contains(x.FormaPago) || x.Monto < 0) ? "La venta contiene medios de pago inválidos." : null;
    }
}