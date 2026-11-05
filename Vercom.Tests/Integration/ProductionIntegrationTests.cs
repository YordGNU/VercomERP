using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Pruebas integrales del módulo de Producción contra SQL Server: BOM, ficha de
/// costo, plan, orden, consumo, cierre, desviaciones, merma y mantenimiento.
/// Todo se ejecuta dentro de una transacción que se revierte al final.
/// </summary>
public class ProductionIntegrationTests
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

    private sealed record Preparacion(
        DatosInventario Datos,
        InventarioProduccionFixture.ServiciosPrueba Servicios,
        OrdenProduccion Orden);

    private static async Task<Preparacion> PrepararOrdenAsync(
        AppDbContext db,
        bool conParametrosConversion = true,
        decimal stockInicial = 100m,
        decimal costoInicial = 5m,
        decimal costoManoObra = 3m,
        decimal gastosIndirectos = 2m,
        decimal cantidad = 10m)
    {
        var datos = await InventarioProduccionFixture.SembrarAsync(db);
        if (conParametrosConversion)
            await InventarioProduccionFixture.CrearParametrosConversionAsync(db, datos);
        var servicios = InventarioProduccionFixture.CrearServicios(db);

        var bom = new ListaMateriale { ProductoTerminadoId = datos.ProductoTerminadoId };
        bom.ListaMaterialesDetalles.Add(new ListaMaterialesDetalle
        {
            ProductoInsumoId = datos.MateriaPrimaId,
            CantidadRequerida = 2m,
            PorcentajeMerma = 1m
        });
        var bomResultado = await servicios.Produccion.CreateBomAsync(bom);
        if (!bomResultado.Succeeded)
            throw new InvalidOperationException($"No se pudo crear la BOM de prueba: {bomResultado.Message}");

        var fichaResultado = await servicios.Produccion.CreateCostSheetAsync(new FichaCosto
        {
            ProductoId = datos.ProductoTerminadoId,
            CostoMateriaPrima = 10m,
            CostoManoObra = costoManoObra,
            GastosIndirectos = gastosIndirectos,
            MargenPorcentaje = 20m
        });
        if (!fichaResultado.Succeeded || fichaResultado.CostSheet is null)
            throw new InvalidOperationException($"No se pudo crear la ficha de costo: {fichaResultado.Message}");
        var aprobacionFicha = await servicios.Produccion.ApproveCostSheetAsync(fichaResultado.CostSheet.Id);
        if (!aprobacionFicha.Succeeded)
            throw new InvalidOperationException($"No se pudo aprobar la ficha de costo: {aprobacionFicha.Message}");

        if (stockInicial > 0)
        {
            var movimiento = new MovimientoInventario
            {
                EntidadId = datos.EntidadId,
                TipoMovimientoId = await servicios.Inventario.EnsureMovementTypeAsync("RECEPCION"),
                NumeroDocumento = $"INIPROD-{Guid.NewGuid():N}"[..20],
                Fecha = DateTimeOffset.Now,
                Canal = "ERP",
                CreadoPor = datos.UsuarioId,
                AlmacenDestinoId = datos.AlmacenAId
            };
            movimiento.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                ProductoId = datos.MateriaPrimaId,
                Cantidad = stockInicial,
                CostoUnitario = costoInicial
            });
            var entrada = await servicios.Inventario.ProcessMovementAsync(movimiento);
            if (!entrada.Succeeded)
                throw new InvalidOperationException($"No se pudo cargar el stock inicial: {entrada.Message}");
        }

        var orden = new OrdenProduccion
        {
            EntidadId = datos.EntidadId,
            ProductoTerminadoId = datos.ProductoTerminadoId,
            AlmacenInsumosId = datos.AlmacenAId,
            AlmacenProductoId = datos.AlmacenAId,
            CantidadPlanificada = cantidad,
            FechaInicioPlan = new DateOnly(2026, 11, 15),
            FechaFinPlan = new DateOnly(2026, 11, 20),
            CreadoPor = datos.UsuarioId
        };
        var creada = await servicios.Produccion.CreateProductionOrderAsync(orden);
        if (!creada.Succeeded || creada.Order is null)
            throw new InvalidOperationException($"No se pudo crear la orden de prueba: {creada.Message}");

        return new Preparacion(datos, servicios, creada.Order);
    }

    private static async Task<PlanProduccion> CrearPlanAprobadoAsync(Preparacion prep, short anio = 2026, short mes = 11)
    {
        var plan = new PlanProduccion
        {
            Anio = anio,
            Mes = mes,
            PlanProduccionDetalles =
            {
                new PlanProduccionDetalle
                {
                    ProductoId = prep.Datos.ProductoTerminadoId,
                    CantidadPlanificada = 10m
                }
            }
        };
        var creado = await prep.Servicios.Produccion.CreatePlanAsync(plan);
        if (!creado.Succeeded)
            throw new InvalidOperationException($"No se pudo crear el plan: {creado.Message}");
        var aprobado = await prep.Servicios.Produccion.ApprovePlanAsync(plan.Id);
        if (!aprobado.Succeeded)
            throw new InvalidOperationException($"No se pudo aprobar el plan: {aprobado.Message}");
        return plan;
    }

    private static async Task<decimal> StockAsync(AppDbContext db, Guid almacenId, Guid productoId)
        => await db.Existencia.AsNoTracking()
            .Where(e => e.AlmacenId == almacenId && e.ProductoId == productoId)
            .Select(e => e.Cantidad)
            .FirstOrDefaultAsync();

    [DbFact(RequiereDatosBase = true)]
    public async Task FlujoCompleto_ConsumeIniciaTerminaYActualizaInventarioPlanYContabilidad()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db);
            var plan = await CrearPlanAprobadoAsync(prep);

            var inicio = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.True(inicio.Succeeded, inicio.Message);

            var consumoPlanificado = prep.Orden.OrdenProduccionConsumos.Single().CantidadPlanificada;
            Assert.Equal(20.2m, consumoPlanificado); // 2 por unidad * 10 * (1 + 1% merma)

            var enProceso = await db.OrdenProduccions.AsNoTracking()
                .Include(o => o.OrdenProduccionConsumos)
                .SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("EN_PROCESO", enProceso.Estado);
            Assert.NotNull(enProceso.AsientoConsumoId);
            Assert.Equal(79.8m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));

            var consumosReales = new List<OrdenProduccionConsumo>
            {
                new()
                {
                    ProductoInsumoId = prep.Datos.MateriaPrimaId,
                    CantidadReal = 20m
                }
            };
            var cierre = await prep.Servicios.Produccion.FinishProductionAsync(prep.Orden.Id, 10m, consumosReales, prep.Datos.UsuarioId);
            Assert.True(cierre.Succeeded, cierre.Message);

            db.ChangeTracker.Clear();
            var terminada = await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("TERMINADA", terminada.Estado);
            Assert.Equal(10m, terminada.CantidadProducida);
            Assert.Equal(150m, terminada.CostoRealTotal); // MP 20*5 + MO 30 + GIF 20
            Assert.NotNull(terminada.AsientoConsumoId);
            Assert.NotNull(terminada.AsientoTerminadoId);

            // Materia prima: 100 - 20.2 consumidas + 0.2 devueltas = 80.
            Assert.Equal(80m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));
            // Producto terminado: 10 unidades a 15 de costo unitario real.
            var existenciaPt = await db.Existencia.AsNoTracking()
                .SingleAsync(e => e.AlmacenId == prep.Datos.AlmacenAId && e.ProductoId == prep.Datos.ProductoTerminadoId);
            Assert.Equal(10m, existenciaPt.Cantidad);
            Assert.Equal(15m, existenciaPt.CostoPromedio);

            // El plan aprobado acumula la cantidad ejecutada.
            var lineaPlan = await db.PlanProduccionDetalles.AsNoTracking()
                .SingleAsync(d => d.PlanId == plan.Id && d.ProductoId == prep.Datos.ProductoTerminadoId);
            Assert.Equal(10m, lineaPlan.CantidadEjecutada);

            // Desviaciones estándar vs real para MP, MO y GIF.
            var desviaciones = await db.AnalisisDesviacions.AsNoTracking()
                .Where(d => d.OrdenProduccionId == prep.Orden.Id)
                .ToListAsync();
            Assert.Equal(3, desviaciones.Count);
            var mp = desviaciones.Single(d => d.Componente == "MATERIA_PRIMA");
            Assert.Equal(100m, mp.CostoEstandar);
            Assert.Equal(100m, mp.CostoReal);
            var mo = desviaciones.Single(d => d.Componente == "MANO_OBRA");
            Assert.Equal(30m, mo.CostoEstandar);
            Assert.Equal(30m, mo.CostoReal);
            var gif = desviaciones.Single(d => d.Componente == "GASTOS_INDIRECTOS");
            Assert.Equal(20m, gif.CostoEstandar);
            Assert.Equal(20m, gif.CostoReal);

            // Todos los asientos del ciclo cuadran y el WIP queda en cero.
            var movimientos = await db.MovimientoInventarios.AsNoTracking()
                .Where(m => m.ReferenciaExternaTipo == "ORDEN_PRODUCCION" && m.ReferenciaExternaId == prep.Orden.Id)
                .ToListAsync();
            var asientoIds = movimientos.Where(m => m.AsientoId.HasValue).Select(m => m.AsientoId!.Value)
                .Concat(new[] { terminada.AsientoConsumoId!.Value, terminada.AsientoTerminadoId!.Value })
                .Distinct()
                .ToList();
            var conversionId = await db.AsientoContables.AsNoTracking()
                .Where(a => a.DocumentoOrigenTipo == "ORDEN_PRODUCCION_CONVERSION" && a.DocumentoOrigenId == prep.Orden.Id)
                .Select(a => a.Id)
                .SingleAsync();
            asientoIds.Add(conversionId);
            var asientos = await db.AsientoContables.AsNoTracking()
                .Include(a => a.AsientoDetalles)
                .Where(a => asientoIds.Contains(a.Id))
                .ToListAsync();
            Assert.True(asientos.Count >= 4);
            Assert.All(asientos, a =>
            {
                Assert.Equal(a.AsientoDetalles.Sum(d => d.Debe), a.AsientoDetalles.Sum(d => d.Haber));
                Assert.Equal("CONTABILIZADO", a.Estado);
            });

            var cuentaWipId = await db.CuentaContables.AsNoTracking()
                .Where(c => c.EntidadId == prep.Datos.EntidadId && c.Codigo == "185.0010")
                .Select(c => c.Id)
                .SingleAsync();
            var saldoWip = asientos.SelectMany(a => a.AsientoDetalles)
                .Where(d => d.CuentaId == cuentaWipId)
                .Sum(d => d.Debe - d.Haber);
            Assert.Equal(0m, saldoWip);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task IniciarSinStockSuficiente_DejaLaOrdenPlanificadaYSinMovimientos()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db, stockInicial: 5m);

            var inicio = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.False(inicio.Succeeded);
            Assert.Contains("Stock insuficiente", inicio.Message);

            db.ChangeTracker.Clear();
            var orden = await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("PLANIFICADA", orden.Estado);
            Assert.Equal(5m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));
            Assert.Equal(0, await db.MovimientoInventarios.AsNoTracking()
                .CountAsync(m => m.ReferenciaExternaId == prep.Orden.Id));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task FinalizarConConsumosQueNoCoinciden_RechazaSinTocarInventario()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db);
            var inicio = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.True(inicio.Succeeded, inicio.Message);

            var cierre = await prep.Servicios.Produccion.FinishProductionAsync(
                prep.Orden.Id,
                10m,
                new List<OrdenProduccionConsumo>(),
                prep.Datos.UsuarioId);

            Assert.False(cierre.Succeeded);
            Assert.Contains("no coincide", cierre.Message);

            db.ChangeTracker.Clear();
            var orden = await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("EN_PROCESO", orden.Estado);
            Assert.Equal(79.8m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));
            Assert.Equal(0m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.ProductoTerminadoId));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task CierreSinCuentasDeConversion_RevierteAjustesYPTSinAfectarLaOrden()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db, conParametrosConversion: false);
            await InventarioProduccionFixture.EliminarParametrosConversionAsync(db, prep.Datos);

            var inicio = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.True(inicio.Succeeded, inicio.Message);

            var cierre = await prep.Servicios.Produccion.FinishProductionAsync(
                prep.Orden.Id,
                10m,
                new List<OrdenProduccionConsumo>
                {
                    new() { ProductoInsumoId = prep.Datos.MateriaPrimaId, CantidadReal = 19m }
                },
                prep.Datos.UsuarioId);

            Assert.False(cierre.Succeeded);
            Assert.Contains("CTA_MANO_OBRA_PRODUCCION", cierre.Message);

            db.ChangeTracker.Clear();
            var orden = await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("EN_PROCESO", orden.Estado);
            Assert.Equal(79.8m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));
            Assert.Equal(0m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.ProductoTerminadoId));
            Assert.DoesNotContain(
                await db.MovimientoInventarios.AsNoTracking()
                    .Where(m => m.ReferenciaExternaId == prep.Orden.Id)
                    .Select(m => m.NumeroDocumento)
                    .ToListAsync(),
                documento => documento.Contains("-DEV-"));
            Assert.Equal(0, await db.AnalisisDesviacions.AsNoTracking().CountAsync(d => d.OrdenProduccionId == prep.Orden.Id));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task CancelarOrden_SoloPermiteOrdenesPlanificadas()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db);

            var cancelada = await prep.Servicios.Produccion.CancelPlannedOrderAsync(prep.Orden.Id);
            Assert.True(cancelada.Succeeded, cancelada.Message);
            Assert.Equal("CANCELADA", (await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id)).Estado);

            var tercera = await PrepararOrdenAsync(db, stockInicial: 100m);
            var inicioTercera = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(tercera.Orden.Id, tercera.Datos.UsuarioId);
            Assert.True(inicioTercera.Succeeded, inicioTercera.Message);
            var cancelarEnProceso = await prep.Servicios.Produccion.CancelPlannedOrderAsync(tercera.Orden.Id);
            Assert.False(cancelarEnProceso.Succeeded);
            Assert.Contains("aún no se han iniciado", cancelarEnProceso.Message);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Merma_DescuentaStockContabilizaYRechazaStockInsuficiente()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);
            var entrada = new MovimientoInventario
            {
                EntidadId = datos.EntidadId,
                TipoMovimientoId = await servicios.Inventario.EnsureMovementTypeAsync("RECEPCION"),
                NumeroDocumento = $"MERMA-IN-{Guid.NewGuid():N}"[..20],
                Fecha = DateTimeOffset.Now,
                Canal = "ERP",
                CreadoPor = datos.UsuarioId,
                AlmacenDestinoId = datos.AlmacenAId
            };
            entrada.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
            {
                Id = Guid.NewGuid(),
                ProductoId = datos.MateriaPrimaId,
                Cantidad = 10m,
                CostoUnitario = 3m
            });
            Assert.True((await servicios.Inventario.ProcessMovementAsync(entrada)).Succeeded);

            var merma = new Merma
            {
                ProductoId = datos.MateriaPrimaId,
                Cantidad = 2m,
                Causa = "DANO",
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Observaciones = "Prueba de integración"
            };
            var registrada = await servicios.Produccion.CreateWasteAsync(merma, datos.AlmacenAId);
            Assert.True(registrada.Succeeded, registrada.Message);

            db.ChangeTracker.Clear();
            Assert.Equal(8m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
            var mermaDb = await db.Mermas.AsNoTracking().SingleAsync(m => m.Id == merma.Id);
            Assert.Equal(6m, mermaDb.ValorContable); // 2 * 3
            Assert.NotNull(mermaDb.AsientoId);
            var asiento = await db.AsientoContables.AsNoTracking()
                .Include(a => a.AsientoDetalles)
                .SingleAsync(a => a.Id == mermaDb.AsientoId);
            Assert.Equal(asiento.AsientoDetalles.Sum(d => d.Debe), asiento.AsientoDetalles.Sum(d => d.Haber));
            Assert.Equal(6m, asiento.TotalDebe);

            var exceso = new Merma
            {
                ProductoId = datos.MateriaPrimaId,
                Cantidad = 50m,
                Causa = "DANO",
                Fecha = DateOnly.FromDateTime(DateTime.Today)
            };
            var rechazada = await servicios.Produccion.CreateWasteAsync(exceso, datos.AlmacenAId);
            Assert.False(rechazada.Succeeded);
            Assert.Contains("insuficiente", rechazada.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(8m, await StockAsync(db, datos.AlmacenAId, datos.MateriaPrimaId));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task Mantenimiento_ActualizaEquipoYRechazaFechaAnteriorALaProgramada()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);
            var equipo = new Equipo
            {
                Id = Guid.NewGuid(),
                EntidadId = datos.EntidadId,
                Codigo = $"EQ-{Guid.NewGuid():N}"[..12],
                Nombre = "Equipo de prueba",
                Estado = "OPERATIVO"
            };
            db.Equipos.Add(equipo);
            var mantenimiento = new MantenimientoProgramado
            {
                Id = Guid.NewGuid(),
                EquipoId = equipo.Id,
                FechaProgramada = new DateOnly(2026, 11, 10),
                Tipo = "PREVENTIVO",
                Estado = "PROGRAMADO"
            };
            db.MantenimientoProgramados.Add(mantenimiento);
            await db.SaveChangesAsync();

            var temprano = await servicios.Produccion.ExecuteMaintenanceAsync(mantenimiento.Id, new DateOnly(2026, 11, 9));
            Assert.False(temprano.Succeeded);
            Assert.Equal("PROGRAMADO", (await db.MantenimientoProgramados.AsNoTracking().SingleAsync(m => m.Id == mantenimiento.Id)).Estado);

            var ejecutado = await servicios.Produccion.ExecuteMaintenanceAsync(mantenimiento.Id, new DateOnly(2026, 11, 11));
            Assert.True(ejecutado.Succeeded, ejecutado.Message);

            db.ChangeTracker.Clear();
            var mantenimientoDb = await db.MantenimientoProgramados.AsNoTracking().SingleAsync(m => m.Id == mantenimiento.Id);
            Assert.Equal("EJECUTADO", mantenimientoDb.Estado);
            Assert.Equal(new DateOnly(2026, 11, 11), mantenimientoDb.FechaEjecutada);
            Assert.Equal(new DateOnly(2026, 11, 11), (await db.Equipos.AsNoTracking().SingleAsync(e => e.Id == equipo.Id)).FechaUltimaRevision);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task PlanSugerido_IncluyeProductosTerminadosBajoStockMinimo()
        => await EnTransaccionAsync(async db =>
        {
            var datos = await InventarioProduccionFixture.SembrarAsync(db);
            var servicios = InventarioProduccionFixture.CrearServicios(db);
            db.Existencia.Add(new Existencium
            {
                Id = Guid.NewGuid(),
                AlmacenId = datos.AlmacenAId,
                ProductoId = datos.ProductoTerminadoId,
                Cantidad = 2m,
                CostoPromedio = 4m,
                StockMinimo = 5m,
                ActualizadoEn = DateTimeOffset.Now
            });
            await db.SaveChangesAsync();

            var plan = await servicios.Produccion.GenerateSuggestedPlanAsync(datos.EntidadId, 2026, 12);

            var linea = plan.PlanProduccionDetalles.Single(d => d.ProductoId == datos.ProductoTerminadoId);
            Assert.Equal(8m, linea.CantidadPlanificada);
            Assert.DoesNotContain(plan.PlanProduccionDetalles, d => d.ProductoId == datos.MateriaPrimaId);
        });
    [DbFact(RequiereDatosBase = true)]
    public async Task RevertirOrden_EnProceso_VuelveAPlanificadaYRestauraStock()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db);
            var inicio = await prep.Servicios.Produccion.StartProductionAndConsumeAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.True(inicio.Succeeded, inicio.Message);
            Assert.Equal(79.8m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));

            var reversion = await prep.Servicios.Produccion.ReverseProductionOrderAsync(prep.Orden.Id, prep.Datos.UsuarioId);
            Assert.True(reversion.Succeeded, reversion.Message);

            db.ChangeTracker.Clear();
            var orden = await db.OrdenProduccions.AsNoTracking().SingleAsync(o => o.Id == prep.Orden.Id);
            Assert.Equal("PLANIFICADA", orden.Estado);
            Assert.Null(orden.FechaInicioReal);
            Assert.Null(orden.AsientoConsumoId);
            Assert.Equal(100m, await StockAsync(db, prep.Datos.AlmacenAId, prep.Datos.MateriaPrimaId));
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task GenerarOrdenesDesdePlan_CreaOrdenesPlanificadas()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db, stockInicial: 0m);
            var plan = await CrearPlanAprobadoAsync(prep);

            var resultado = await prep.Servicios.Produccion.GenerateOrdersFromPlanAsync(plan.Id, prep.Datos.UsuarioId);
            Assert.True(resultado.Succeeded, resultado.Message);
            Assert.True(resultado.OrdersCreated >= 1);

            var ordenes = await db.OrdenProduccions.AsNoTracking()
                .Where(o => o.ProductoTerminadoId == prep.Datos.ProductoTerminadoId && o.Estado == "PLANIFICADA")
                .ToListAsync();
            Assert.NotEmpty(ordenes);
        });

    [DbFact(RequiereDatosBase = true)]
    public async Task GenerarFichaDesdeBOM_CreaBorradorConCostoMP()
        => await EnTransaccionAsync(async db =>
        {
            var prep = await PrepararOrdenAsync(db);
            var resultado = await prep.Servicios.Produccion.BuildCostSheetFromBomAsync(prep.Datos.ProductoTerminadoId);
            Assert.True(resultado.Succeeded, resultado.Message);
            Assert.NotNull(resultado.CostSheet);
            Assert.Equal("BORRADOR", resultado.CostSheet!.Estado);
            Assert.True(resultado.CostSheet.CostoMateriaPrima > 0);
        });}
