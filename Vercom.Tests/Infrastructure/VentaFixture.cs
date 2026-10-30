using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace Vercom.Tests.Infrastructure;

/// <summary>
/// Resuelve el contexto mínimo para crear una factura de venta con datos
/// sembrados reales (sucursal con consecutivo FACTURA_VENTA, almacén, cliente y
/// producto con familia) y administra topes de precio temporales.
/// </summary>
public static class VentaFixture
{
    public sealed record ContextoVenta(
        Guid EntidadId,
        Guid SucursalId,
        string Serie,
        Guid AlmacenId,
        Guid ClienteId,
        Guid ProductoId,
        Guid? FamiliaId);

    public static async Task<ContextoVenta?> ResolverContextoAsync(AppDbContext db)
    {
        var consecutivo = await db.Consecutivos.AsNoTracking()
            .Where(c => c.TipoDocumento == "FACTURA_VENTA" && c.SucursalId != null)
            .OrderByDescending(c => c.UltimoNumero)
            .FirstOrDefaultAsync();
        if (consecutivo is null) return null;

        var sucursal = await db.Sucursals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == consecutivo.SucursalId);
        var almacen = await db.Almacens.AsNoTracking().OrderBy(a => a.Codigo).FirstOrDefaultAsync();
        var cliente = await db.Clientes.AsNoTracking().OrderBy(c => c.NombreRazonSocial).FirstOrDefaultAsync();
        var producto = await db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.FamiliaId != null)
            .OrderBy(p => p.Codigo)
            .FirstOrDefaultAsync();

        if (sucursal is null || almacen is null || cliente is null || producto is null) return null;

        return new ContextoVenta(
            sucursal.EntidadId,
            sucursal.Id,
            consecutivo.Serie,
            almacen.Id,
            cliente.Id,
            producto.Id,
            producto.FamiliaId);
    }

    /// <summary>
    /// Inserta un tope de precio vigente hoy. Debe eliminarse con
    /// <see cref="EliminarTopeAsync"/> porque la venta abre su propia transacción
    /// y no se puede envolver en la del test.
    /// </summary>
    public static async Task<Guid> InsertarTopeAsync(AppDbContext db, Guid? productoId, Guid? familiaId, decimal precioMaximo)
    {
        var tope = new TopePrecioMfp
        {
            Id = Guid.NewGuid(),
            ProductoId = productoId,
            FamiliaId = familiaId,
            PrecioMaximo = precioMaximo,
            VigenteDesde = DateOnly.FromDateTime(DateTime.Today).AddDays(-1),
            VigenteHasta = null,
            ResolucionReferencia = "TEST-TOPE-MFP"
        };

        db.TopePrecioMfps.Add(tope);
        await db.SaveChangesAsync();
        return tope.Id;
    }

    public static Task EliminarTopeAsync(AppDbContext db, Guid topeId)
        => db.TopePrecioMfps.Where(t => t.Id == topeId).ExecuteDeleteAsync();

    public static FacturaVentum CrearFactura(ContextoVenta ctx, decimal precioUnitario, Guid? productoId = null)
        => new()
        {
            EntidadId = ctx.EntidadId,
            SucursalId = ctx.SucursalId,
            AlmacenId = ctx.AlmacenId,
            ClienteId = ctx.ClienteId,
            Serie = ctx.Serie,
            TipoVenta = "MINORISTA",
            CanalVenta = "ERP",
            Moneda = "CUP",
            Fecha = DateTimeOffset.Now,
            FacturaVentaDetalles =
            {
                new FacturaVentaDetalle
                {
                    ProductoId = productoId ?? ctx.ProductoId,
                    Cantidad = 1,
                    PrecioUnitario = precioUnitario
                }
            }
        };
}
