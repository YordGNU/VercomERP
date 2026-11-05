using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VERCOM.DTOs;
using VERCOM.Models;

namespace Vercom.Tests.Infrastructure;

/// <summary>
/// Prepara el contexto mínimo para ejercitar una venta POS de extremo a extremo
/// (producto con existencia en el almacén del dispositivo, cliente, tipo de
/// movimiento y numeración libre de colisiones) dentro de la transacción del test.
/// </summary>
public static class PosVentaFixture
{
    /// <summary>inventario.tipo_movimiento "VENTA A CLIENTE" (salida de almacén).</summary>
    public const int TipoMovimientoVentaACliente = 2334;

    public sealed record ContextoVenta(
        Guid EntidadId,
        Guid SucursalId,
        Guid AlmacenId,
        Guid ClienteId,
        Guid ProductoId,
        decimal ExistenciaInicial,
        decimal CostoPromedio,
        string Serie,
        string NumeroFactura);

    public static async Task<ContextoVenta> PrepararAsync(AppDbContext db, PosFixture.SesionPos sesion, decimal existenciaInicial = 1000m)
    {
        var dispositivo = await db.DispositivoPos.AsNoTracking().FirstAsync(d => d.Id == sesion.DispositivoPosId);

        var cliente = await db.Clientes.AsNoTracking()
            .Where(c => c.EntidadId == sesion.EntidadId && c.Activo)
            .OrderBy(c => c.Id)
            .FirstAsync();

        var producto = await db.Productos.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Id)
            .FirstAsync();

        // El sync decrementa por (almacén, producto): la existencia del dispositivo
        // es la que se consume, aunque existan filas de otros almacenes.
        var existencia = await db.Existencia
            .FirstOrDefaultAsync(e => e.AlmacenId == dispositivo.AlmacenId && e.ProductoId == producto.Id);

        if (existencia is null)
        {
            existencia = new Existencium
            {
                Id = Guid.NewGuid(),
                AlmacenId = dispositivo.AlmacenId,
                ProductoId = producto.Id,
                Cantidad = existenciaInicial,
                CostoPromedio = 60m,
                StockMinimo = 0m,
                ActualizadoEn = DateTimeOffset.UtcNow
            };
            db.Existencia.Add(existencia);
        }
        else
        {
            existencia.Cantidad = existenciaInicial;
            // La BD de desarrollo puede traer la existencia sembrada con otro costo;
            // el test necesita un costo conocido para comprobar el asiento de costo.
            existencia.CostoPromedio = 60m;
        }

        await db.SaveChangesAsync();

        // Serie/número fuera de los rangos reales (POS/A/ERP) para no chocar con el
        // índice único (entidad, sucursal, serie, número).
        var serie = "TST";
        var numero = "T" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

        return new ContextoVenta(
            sesion.EntidadId,
            dispositivo.SucursalId,
            dispositivo.AlmacenId,
            cliente.Id,
            producto.Id,
            existenciaInicial,
            existencia.CostoPromedio,
            serie,
            numero);
    }

    /// <summary>
    /// Deja el producto sin cuentas propias y apunta la cuenta genérica de inventario a un
    /// código inexistente, de modo que el asiento de COSTO no se pueda resolver.
    /// </summary>
    public static async Task ForzarFalloDelAsientoDeCostoAsync(AppDbContext db, ContextoVenta ctx)
    {
        var producto = await db.Productos.FirstAsync(p => p.Id == ctx.ProductoId);
        producto.CuentaInventarioId = null;
        producto.CuentaCostoVentaId = null;

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var parametro = await db.ParametroSistemas
            .FirstOrDefaultAsync(p => p.EntidadId == ctx.EntidadId && p.Codigo == "CTA_INV_GENERICA" && p.VigenteDesde == hoy);

        if (parametro is null)
        {
            db.ParametroSistemas.Add(new ParametroSistema
            {
                EntidadId = ctx.EntidadId,
                Codigo = "CTA_INV_GENERICA",
                Valor = "999.9999",
                TipoDato = "NUMERIC",
                Descripcion = "Cuenta inexistente (prueba)",
                VigenteDesde = hoy
            });
        }
        else
        {
            parametro.Valor = "999.9999";
        }

        await db.SaveChangesAsync();
    }

    public static decimal Total(decimal cantidad, decimal precioUnitario, decimal descuentoPorcentaje = 0m, decimal impuestoPorcentaje = 0m)
    {
        var bruto = cantidad * precioUnitario;
        var descuento = Math.Round(bruto * descuentoPorcentaje / 100m, 2);
        var impuesto = Math.Round((bruto - bruto * descuentoPorcentaje / 100m) * impuestoPorcentaje / 100m, 2);
        return Math.Round(bruto - descuento + impuesto, 2);
    }

    public static RecibirVentaPosRequest Request(
        PosFixture.SesionPos sesion,
        ContextoVenta ctx,
        decimal cantidad = 2m,
        decimal precioUnitario = 100m,
        decimal descuentoPorcentaje = 0m,
        decimal impuestoPorcentaje = 0m,
        string formaPago = "EFECTIVO",
        decimal? montoPago = null,
        string? idempotencyKey = null,
        string? numeroFactura = null)
    {
        var total = Total(cantidad, precioUnitario, descuentoPorcentaje, impuestoPorcentaje);

        var payload = new VentaPosPayload
        {
            EntidadId = ctx.EntidadId,
            SucursalId = ctx.SucursalId,
            ClienteId = ctx.ClienteId,
            AlmacenId = ctx.AlmacenId,
            TipoMovimientoId = TipoMovimientoVentaACliente,
            NumeroFactura = numeroFactura ?? ctx.NumeroFactura,
            Serie = ctx.Serie,
            Fecha = DateTimeOffset.UtcNow,
            Moneda = "CUP",
            Lineas =
            {
                new VentaPosLineaPayload
                {
                    ProductoId = ctx.ProductoId,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    DescuentoPorcentaje = descuentoPorcentaje,
                    ImpuestoPorcentaje = impuestoPorcentaje
                }
            },
            Pagos =
            {
                new VentaPosPagoPayload
                {
                    FormaPago = formaPago,
                    Monto = montoPago ?? total,
                    VueltoEntregado = 0m
                }
            }
        };

        return new RecibirVentaPosRequest
        {
            DispositivoPosId = sesion.DispositivoPosId,
            SesionCajaPosId = sesion.SesionId,
            IdempotencyKey = idempotencyKey ?? $"test-{Guid.NewGuid():N}",
            FechaVentaLocal = DateTimeOffset.UtcNow,
            Venta = JsonSerializer.SerializeToElement(payload)
        };
    }
}
