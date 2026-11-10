using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Pruebas integrales del módulo de Inventario contra SQL Server. Cada prueba
/// corre dentro de una transacción que se revierte al final, por lo que no deja
/// rastros en la base de desarrollo ni depende del estado previo.
/// </summary>
public class InventoryIntegrationTests
{
    private static async Task EnTransaccionAsync(Func<AppDbContext, Task> cuerpo)
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await cuerpo(db);
        }
        finally
        {
            await tx.RollbackAsync();
        }
    }

    private static async Task<MovimientoInventario> MovimientoAsync(
        InventoryService inventario,
        DatosInventario datos,
        string tipoCodigo,
        Guid productoId,
        decimal cantidad,
        decimal? costo,
        string documento,
        Guid? almacenOrigen = null,
        Guid? almacenDestino = null)
    {
        var movimiento = new MovimientoInventario
        {
            EntidadId = datos.EntidadId,
            TipoMovimientoId = await inventario.EnsureMovementTypeAsync(tipoCodigo),
            NumeroDocumento = $"{documento}-{Guid.NewGuid():N}"[..24],
            Fecha = DateTimeOffset.Now,
            Canal = "ERP",
            CreadoPor = datos.UsuarioId,
            AlmacenOrigenId = almacenOrigen,
            AlmacenDestinoId = almacenDestino
        };
        movimiento.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
        {
            Id = Guid.NewGuid(),
            ProductoId = productoId,
            Cantidad = cantidad,
            CostoUnitario = costo
        });
        return movimiento;
    }

    private static async Task<decimal> StockAsync(AppDbContext db, Guid almacenId, Guid productoId)
        => await db.Existencia.AsNoTracking()
            .Where(e => e.AlmacenId == almacenId && e.ProductoId == productoId)
            .Select(e => e.Cantidad)
            .FirstOrDefaultAsync();

    [DbFact(RequiereDatosBase = true)]
    public async Task Entrada_CalculaCostoPromedioPonderadoYContabiliza()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var primera = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 5m, "ENT-A", almacenDestino: datos.AlmacenAId));
            Assert.True(primera.Succeeded, primera.Message);

            var segunda = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 7m, "ENT-B", almacenDestino: datos.AlmacenAId));
            Assert.True(segunda.Succeeded, segunda.Message);

            var existencia = await db.Existencia.AsNoTracking()
                .SingleAsync(e => e.AlmacenId == datos.AlmacenAId && e.ProductoId == datos.MateriaPrimaId);
            Assert.Equal(20m, existencia.Cantidad);
            Assert.Equal(6m, existencia.CostoPromedio); // (10*5 + 10*7) / 20

            Assert.NotNull(segunda.Movement!.AsientoId);
            var asiento = await db.AsientoContables.AsNoTracking()
                .Include(a => a.AsientoDetalles)
                .SingleAsync(a => a.Id == segunda.Movement.AsientoId);
            Assert.Equal("CONTABILIZADO", asiento.Estado);
            Assert.Equal(70m, asiento.TotalDebe);
            Assert.Equal(70m, asiento.TotalHaber);
            Assert.Equal(asiento.TotalDebe, asiento.AsientoDetalles.Sum(d => d.Debe));
            Assert.Equal(asiento.TotalHaber, asiento.AsientoDetalles.Sum(d => d.Haber));
            Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == datos.CuentaInventarioId && d.Debe == 70m);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Salida_ConservaElCostoPromedioYGeneraAsientoDeCosto()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var entrada = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 4m, "ENT-C", almacenDestino: datos.AlmacenAId));
            Assert.True(entrada.Succeeded, entrada.Message);

            var salida = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "VALE_ENTREGA", datos.MateriaPrimaId, 3m, null, "SAL-C", almacenOrigen: datos.AlmacenAId));
            Assert.True(salida.Succeeded, salida.Message);

            var detalle = salida.Movement!.MovimientoInventarioDetalles.Single();
            Assert.Equal(4m, detalle.CostoUnitario);
            Assert.Equal(7m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));

            var asiento = await db.AsientoContables.AsNoTracking()
                .Include(a => a.AsientoDetalles)
                .SingleAsync(a => a.Id == salida.Movement.AsientoId);
            Assert.Equal(12m, asiento.TotalDebe);
            Assert.Equal(12m, asiento.TotalHaber);
            Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == datos.CuentaCostoVentaId && d.Debe == 12m);
            Assert.Contains(asiento.AsientoDetalles, d => d.CuentaId == datos.CuentaInventarioId && d.Haber == 12m);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task SalidaSinStock_NoDejaMovimientoNiAlteraciones()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var entrada = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 5m, 2m, "ENT-D", almacenDestino: datos.AlmacenAId));
            Assert.True(entrada.Succeeded, entrada.Message);

            var fallida = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "VALE_ENTREGA", datos.MateriaPrimaId, 6m, null, "SAL-D", almacenOrigen: datos.AlmacenAId));
            Assert.False(fallida.Succeeded);
            Assert.Contains("Stock insuficiente", fallida.Message);

            db.ChangeTracker.Clear();
            Assert.Equal(5m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
            Assert.Equal(0, await db.MovimientoInventarios.AsNoTracking()
                .CountAsync(m => m.NumeroDocumento.StartsWith("SAL-D")));
            Assert.Equal(1, await db.MovimientoInventarios.AsNoTracking()
                .CountAsync(m => m.NumeroDocumento.StartsWith("ENT-D")));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Transferencia_MueveStockEntreAlmacenesSinAsientoContable()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var entrada = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 3m, "ENT-E", almacenDestino: datos.AlmacenAId));
            Assert.True(entrada.Succeeded, entrada.Message);

            var transferencia = await servicios.Inventario.TransferBetweenWarehousesAsync(
                datos.AlmacenAId,
                datos.AlmacenBId,
                new List<MovimientoInventarioDetalle>
                {
                    new() { Id = Guid.NewGuid(), ProductoId = datos.MateriaPrimaId, Cantidad = 4m }
                },
                datos.UsuarioId);

            Assert.True(transferencia.Succeeded, transferencia.Message);
            Assert.Equal(6m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
            Assert.Equal(4m, await StockAsync(db, datos.AlmacenBId, datos.MateriaPrimaId));

            var destino = await db.Existencia.AsNoTracking()
                .SingleAsync(e => e.AlmacenId == datos.AlmacenBId && e.ProductoId == datos.MateriaPrimaId);
            Assert.Equal(3m, destino.CostoPromedio);

            var movimiento = await db.MovimientoInventarios.AsNoTracking()
                .OrderByDescending(m => m.CreadoEn)
                .FirstAsync(m => m.NumeroDocumento.StartsWith("TRA-"));
            Assert.Equal(datos.AlmacenAId, movimiento.AlmacenOrigenId);
            Assert.Equal(datos.AlmacenBId, movimiento.AlmacenDestinoId);
            Assert.Null(movimiento.AsientoId);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task ConteoFisico_ExigeJustificacionYConciliaAjustesContabilizados()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var entrada = await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 2m, "ENT-F", almacenDestino: datos.AlmacenAId));
            Assert.True(entrada.Succeeded, entrada.Message);

            // El producto terminado parte con 1 unidad valorada para poder probar un sobrante.
            db.Existencia.Add(new Existencium
            {
                Id = Guid.NewGuid(),
                AlmacenId = datos.AlmacenAId,
                ProductoId = datos.ProductoTerminadoId,
                Cantidad = 1m,
                CostoPromedio = 4m,
                ActualizadoEn = DateTimeOffset.Now
            });
            await db.SaveChangesAsync();

            var conteo = new ConteoFisico
            {
                Id = Guid.NewGuid(),
                AlmacenId = datos.AlmacenAId,
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Tipo = "PARCIAL",
                Estado = "EN_PROCESO",
                CreadoEn = DateTimeOffset.Now
            };
            conteo.ConteoFisicoDetalles.Add(new ConteoFisicoDetalle
            {
                Id = Guid.NewGuid(),
                ConteoId = conteo.Id,
                ProductoId = datos.MateriaPrimaId,
                CantidadSistema = 10m,
                CantidadFisica = 8m,
                Diferencia = -2m
            });
            conteo.ConteoFisicoDetalles.Add(new ConteoFisicoDetalle
            {
                Id = Guid.NewGuid(),
                ConteoId = conteo.Id,
                ProductoId = datos.ProductoTerminadoId,
                CantidadSistema = 1m,
                CantidadFisica = 2m,
                Diferencia = 1m
            });
            db.ConteoFisicos.Add(conteo);
            await db.SaveChangesAsync();

            var sinJustificar = await servicios.Inventario.ConciliatePhysicalCountAsync(conteo.Id, datos.UsuarioId);
            Assert.False(sinJustificar.Succeeded);
            Assert.Contains("justificación", sinJustificar.Message);
            Assert.Equal(10m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
            Assert.Equal("EN_PROCESO", (await db.ConteoFisicos.AsNoTracking().SingleAsync(c => c.Id == conteo.Id)).Estado);

            var detalleFaltante = conteo.ConteoFisicoDetalles.Single(d => d.ProductoId == datos.MateriaPrimaId);
            detalleFaltante.Justificacion = "Rotura durante el traslado";
            await db.SaveChangesAsync();

            var conciliado = await servicios.Inventario.ConciliatePhysicalCountAsync(conteo.Id, datos.UsuarioId);
            Assert.True(conciliado.Succeeded, conciliado.Message);

            db.ChangeTracker.Clear();
            Assert.Equal(8m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
            Assert.Equal(2m, await StockAsync(db, datos.AlmacenAId, datos.ProductoTerminadoId));
            Assert.Equal("CERRADO", (await db.ConteoFisicos.AsNoTracking().SingleAsync(c => c.Id == conteo.Id)).Estado);

            var detalles = await db.ConteoFisicoDetalles.AsNoTracking()
                .Where(d => d.ConteoId == conteo.Id)
                .ToListAsync();
            Assert.All(detalles, d => Assert.NotNull(d.MovimientoAjusteId));

            var ajustes = await db.MovimientoInventarios.AsNoTracking()
                .Include(m => m.TipoMovimiento)
                .Where(m => m.MovimientoInventarioDetalles.Any(d => d.ProductoId == datos.MateriaPrimaId || d.ProductoId == datos.ProductoTerminadoId))
                .Where(m => m.TipoMovimiento.Codigo.StartsWith("AJUSTE_"))
                .ToListAsync();
            Assert.Equal(2, ajustes.Count);
            Assert.All(ajustes, a => Assert.NotNull(a.AsientoId));

            foreach (var asientoId in ajustes.Select(a => a.AsientoId!.Value))
            {
                var asiento = await db.AsientoContables.AsNoTracking()
                    .Include(a => a.AsientoDetalles)
                    .SingleAsync(a => a.Id == asientoId);
                Assert.Equal(asiento.AsientoDetalles.Sum(d => d.Debe), asiento.AsientoDetalles.Sum(d => d.Haber));
            }
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Kardex_MuestraEntradasSalidasYSaldoDelProducto()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            Assert.True((await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 10m, 5m, "ENT-G", almacenDestino: datos.AlmacenAId))).Succeeded);
            Assert.True((await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "VALE_ENTREGA", datos.MateriaPrimaId, 3m, null, "SAL-G", almacenOrigen: datos.AlmacenAId))).Succeeded);

            var kardex = await servicios.Inventario.GetKardexByProductAsync(datos.MateriaPrimaId, datos.AlmacenAId);

            Assert.Equal(2, kardex.Count);
            Assert.Equal(3m, kardex[0].Salida);
            Assert.Equal(7m, kardex[0].Saldo);
            Assert.Equal(10m, kardex[1].Entrada);
            Assert.Equal(10m, kardex[1].Saldo);
            Assert.All(kardex, fila => Assert.Equal(5m, fila.CostoUnitario));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Movimiento_RechazaProductoDeOtraEntidad()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);
            var entidadAlterna = await InventarioProduccionFixture.CrearEntidadAlternaAsync(db);

            var productoAjeno = new Producto
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadAlterna.Id,
                Codigo = $"AJENO-{Guid.NewGuid():N}"[..16],
                Nombre = "Producto de otra entidad",
                Tipo = "MATERIA_PRIMA",
                UnidadMedidaId = datos.UnidadMedidaId,
                CuentaInventarioId = datos.CuentaInventarioId,
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
                ActualizadoEn = DateTimeOffset.UtcNow
            };
            db.Productos.Add(productoAjeno);
            await db.SaveChangesAsync();

            var movimiento = new MovimientoInventario
            {
                EntidadId = datos.EntidadId,
                TipoMovimientoId = await servicios.Inventario.EnsureMovementTypeAsync("RECEPCION"),
                NumeroDocumento = $"AJENO-{Guid.NewGuid():N}"[..16],
                Fecha = DateTimeOffset.Now,
                Canal = "ERP",
                CreadoPor = datos.UsuarioId,
                AlmacenDestinoId = datos.AlmacenAId
            };
            movimiento.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                ProductoId = productoAjeno.Id,
                Cantidad = 1m,
                CostoUnitario = 1m
            });

            var resultado = await servicios.Inventario.ProcessMovementAsync(movimiento);

            Assert.False(resultado.Succeeded);
            Assert.Contains("pertenecen a otra entidad", resultado.Message);
            Assert.Equal(0m, await StockAsync(db, datos.AlmacenAId, productoAjeno.Id));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Producto_ValidaReferenciasYNoSeEliminaConExistencias()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var precioNegativo = new Producto
            {
                Codigo = $"NEG-{Guid.NewGuid():N}"[..12],
                Nombre = "Producto con precio negativo",
                Tipo = "MERCANCIA",
                UnidadMedidaId = datos.UnidadMedidaId,
                PrecioVentaActual = -1m,
                Activo = true
            };
            var resultadoPrecio = await servicios.Inventario.CreateProductAsync(precioNegativo);
            Assert.False(resultadoPrecio.Succeeded);
            Assert.Contains("precio", resultadoPrecio.Message, StringComparison.OrdinalIgnoreCase);

            var cuentaAjena = new Producto
            {
                Codigo = $"CTA-{Guid.NewGuid():N}"[..12],
                Nombre = "Producto con cuenta inexistente",
                Tipo = "MERCANCIA",
                UnidadMedidaId = datos.UnidadMedidaId,
                CuentaInventarioId = Guid.NewGuid(),
                Activo = true
            };
            var resultadoCuenta = await servicios.Inventario.CreateProductAsync(cuentaAjena);
            Assert.False(resultadoCuenta.Succeeded);
            Assert.Contains("cuenta de inventario", resultadoCuenta.Message, StringComparison.OrdinalIgnoreCase);

            Assert.True((await servicios.Inventario.ProcessMovementAsync(
                await MovimientoAsync(servicios.Inventario, datos, "RECEPCION", datos.MateriaPrimaId, 1m, 1m, "ENT-H", almacenDestino: datos.AlmacenAId))).Succeeded);

            var eliminarConStock = await servicios.Inventario.DeleteProductAsync(datos.MateriaPrimaId);
            Assert.False(eliminarConStock.Succeeded);
            Assert.Contains("existencias activas", eliminarConStock.Message);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Almacen_RechazaSucursalInexistenteYCreaUnoValido()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);

            var invalido = await servicios.Inventario.CreateWarehouseAsync(new Almacen
            {
                SucursalId = Guid.NewGuid(),
                Codigo = $"BAD-{Guid.NewGuid():N}"[..12],
                Nombre = "Almacén inválido",
                Activo = true
            });
            Assert.False(invalido.Succeeded);
            Assert.Contains("sucursal", invalido.Message, StringComparison.OrdinalIgnoreCase);

            var valido = await servicios.Inventario.CreateWarehouseAsync(new Almacen
            {
                SucursalId = datos.SucursalId,
                Codigo = $"OK-{Guid.NewGuid():N}"[..12],
                Nombre = "Almacén válido",
                Activo = true
            });
            Assert.True(valido.Succeeded, valido.Message);
            Assert.True(await db.Almacens.AsNoTracking().AnyAsync(a => a.EntidadId == datos.EntidadId && a.Nombre == "Almacén válido"));
        });
}
