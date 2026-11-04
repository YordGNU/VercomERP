using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VERCOM.Models;
using VERCOM.Services;

namespace Vercom.Tests.Infrastructure;

/// <summary>
/// Datos mínimos para probar los módulos de Inventario y Producción contra la
/// base sembrada: entidad, sucursal, unidad de medida, cuentas contables,
/// almacenes y productos de prueba con códigos únicos por ejecución.
/// </summary>
public sealed record DatosInventario(
    Guid EntidadId,
    Guid UsuarioId,
    Guid SucursalId,
    Guid AlmacenAId,
    Guid AlmacenBId,
    Guid MateriaPrimaId,
    Guid ProductoTerminadoId,
    Guid ProductoMercanciaId,
    int UnidadMedidaId,
    Guid CuentaInventarioId,
    Guid CuentaCostoVentaId,
    Guid CuentaIngresoId);

public static class InventarioProduccionFixture
{
    public sealed record ServiciosPrueba(
        InventoryService Inventario,
        ProductionService Produccion,
        ParametroSistemaService Parametros);

    public static ServiciosPrueba CrearServicios(AppDbContext db, bool isMaster = true)
    {
        var provider = new TestEntidadProvider(TestDb.EntidadIdSeed, TestDb.UsuarioIdSeed, isMaster);
        var parametros = new ParametroSistemaService(db);
        var consecutivos = new ConsecutivoService(db);
        var contabilidad = new AccountingService(db, null!, provider, consecutivos);
        var inventario = new InventoryService(db, contabilidad, parametros, provider, NullLogger<InventoryService>.Instance);
        var produccion = new ProductionService(db, inventario, contabilidad, parametros, provider, consecutivos);
        return new ServiciosPrueba(inventario, produccion, parametros);
    }

    public static async Task<DatosInventario> SembrarAsync(AppDbContext db)
    {
        var entidadId = TestDb.EntidadIdSeed;
        var usuarioId = TestDb.UsuarioIdSeed;

        var sucursalId = await db.Sucursals
            .Where(s => s.EntidadId == entidadId && s.Activo)
            .Select(s => s.Id)
            .FirstAsync();
        var unidadId = await db.UnidadMedida
            .Select(u => u.Id)
            .FirstAsync();

        var cuentas = await db.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento)
            .Where(c => c.Codigo == "183.0010" || c.Codigo == "810.0000" || c.Codigo == "900.0000")
            .ToDictionaryAsync(c => c.Codigo, c => c.Id);

        if (!cuentas.TryGetValue("183.0010", out var cuentaInventario) ||
            !cuentas.TryGetValue("810.0000", out var cuentaCostoVenta) ||
            !cuentas.TryGetValue("900.0000", out var cuentaIngreso))
        {
            throw new InvalidOperationException(
                "La base de pruebas no contiene las cuentas 183.0010, 810.0000 y 900.0000 requeridas por el fixture.");
        }

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var almacenA = new Almacen
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = sucursalId,
            Codigo = $"TST-A-{sufijo}",
            Nombre = $"Almacén de prueba A {sufijo}",
            Activo = true
        };
        var almacenB = new Almacen
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = sucursalId,
            Codigo = $"TST-B-{sufijo}",
            Nombre = $"Almacén de prueba B {sufijo}",
            Activo = true
        };

        var materiaPrima = NuevoProducto(entidadId, unidadId, $"MP-{sufijo}", "Materia prima de prueba", "MATERIA_PRIMA", cuentaInventario, null, null);
        var terminado = NuevoProducto(entidadId, unidadId, $"PT-{sufijo}", "Producto terminado de prueba", "TERMINADO", cuentaInventario, cuentaCostoVenta, cuentaIngreso);
        var mercancia = NuevoProducto(entidadId, unidadId, $"MC-{sufijo}", "Mercancía de prueba", "MERCANCIA", cuentaInventario, cuentaCostoVenta, cuentaIngreso);

        db.Almacens.AddRange(almacenA, almacenB);
        db.Productos.AddRange(materiaPrima, terminado, mercancia);
        await db.SaveChangesAsync();

        return new DatosInventario(
            entidadId,
            usuarioId,
            sucursalId,
            almacenA.Id,
            almacenB.Id,
            materiaPrima.Id,
            terminado.Id,
            mercancia.Id,
            unidadId,
            cuentaInventario,
            cuentaCostoVenta,
            cuentaIngreso);
    }

    private static Producto NuevoProducto(
        Guid entidadId,
        int unidadMedidaId,
        string codigo,
        string nombre,
        string tipo,
        Guid cuentaInventarioId,
        Guid? cuentaCostoVentaId,
        Guid? cuentaIngresoId)
        => new()
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            Codigo = codigo,
            Nombre = nombre,
            Tipo = tipo,
            UnidadMedidaId = unidadMedidaId,
            CuentaInventarioId = cuentaInventarioId,
            CuentaCostoVentaId = cuentaCostoVentaId,
            CuentaIngresoId = cuentaIngresoId,
            Activo = true,
            AplicaImpuestoVentas = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };

    /// <summary>
    /// Configura las subcuentas de conversión que exige el cierre de una orden
    /// cuando la ficha de costo tiene mano de obra o gastos indirectos.
    /// </summary>
    public static async Task CrearParametrosConversionAsync(AppDbContext db, DatosInventario datos, string codigoCuentaGasto = "810.0000")
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var existentes = await db.ParametroSistemas
            .Where(p => p.EntidadId == datos.EntidadId &&
                        (p.Codigo == "CTA_MANO_OBRA_PRODUCCION" || p.Codigo == "CTA_GIF_PRODUCCION"))
            .ToListAsync();

        foreach (var codigo in new[] { "CTA_MANO_OBRA_PRODUCCION", "CTA_GIF_PRODUCCION" })
        {
            var parametro = existentes.FirstOrDefault(p => p.Codigo == codigo);
            if (parametro is null)
            {
                db.ParametroSistemas.Add(new ParametroSistema
                {
                    EntidadId = datos.EntidadId,
                    Codigo = codigo,
                    Valor = codigoCuentaGasto,
                    TipoDato = "STRING",
                    Descripcion = "Parámetro creado por pruebas de integración.",
                    VigenteDesde = hoy.AddDays(-1)
                });
            }
            else
            {
                parametro.Valor = codigoCuentaGasto;
                parametro.VigenteDesde = hoy.AddDays(-1);
                parametro.VigenteHasta = null;
            }
        }

        await db.SaveChangesAsync();
    }

    public static async Task EliminarParametrosConversionAsync(AppDbContext db, DatosInventario datos)
    {
        var parametros = await db.ParametroSistemas
            .Where(p => p.EntidadId == datos.EntidadId &&
                        (p.Codigo == "CTA_MANO_OBRA_PRODUCCION" || p.Codigo == "CTA_GIF_PRODUCCION"))
            .ToListAsync();
        db.ParametroSistemas.RemoveRange(parametros);
        await db.SaveChangesAsync();
    }

    public static async Task<Entidad> CrearEntidadAlternaAsync(AppDbContext db)
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var entidad = new Entidad
        {
            Id = Guid.NewGuid(),
            RazonSocial = $"Entidad alterna {sufijo}",
            Nit = $"TEST-{sufijo}",
            CodigoReeup = $"REEUP-{sufijo}",
            FormaJuridica = "SURL",
            DireccionLegal = "Dirección de prueba",
            MonedaBase = "CUP",
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        db.Entidads.Add(entidad);
        await db.SaveChangesAsync();
        return entidad;
    }
}
