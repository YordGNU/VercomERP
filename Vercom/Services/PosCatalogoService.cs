using Microsoft.EntityFrameworkCore;
using VERCOM.DTOs;
using VERCOM.Models;

namespace VERCOM.Services;

public sealed class PosCatalogoService
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricingService;

    public PosCatalogoService(AppDbContext db, IPricingService pricingService)
    {
        _db = db;
        _pricingService = pricingService;
    }

    public async Task<IReadOnlyList<PrecioVentaDto>> GetPreciosAsync(Guid entidadId, Guid? clienteId, CancellationToken cancellationToken = default)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Now);
        return await _pricingService.GetSalePricesAsync(entidadId, clienteId, fecha, cancellationToken);
    }

    public async Task<IReadOnlyList<PosProductoDto>> GetProductosAsync(Guid almacenId, Guid entidadId, string? search, DateTimeOffset? updatedSince, CancellationToken cancellationToken)
    {
        var query = _db.Productos.AsNoTracking().Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Codigo.Contains(search) || (x.CodigoBarras != null && x.CodigoBarras.Contains(search)) || x.Nombre.Contains(search));
        if (updatedSince.HasValue)
            query = query.Where(x => x.ActualizadoEn > updatedSince.Value);

        var items = await query.OrderBy(x => x.Nombre).Select(x => new PosProductoDto
        {
            Id = x.Id,
            Codigo = x.Codigo,
            CodigoBarras = x.CodigoBarras,
            Nombre = x.Nombre,
            Descripcion = x.Descripcion,
            FamiliaId = x.FamiliaId,
            UnidadMedidaId = x.UnidadMedidaId,
            PrecioVentaActual = x.PrecioVentaActual,
            AplicaImpuestoVentas = x.AplicaImpuestoVentas,
            Stock = x.Existencia.Where(s => s.AlmacenId == almacenId).Select(s => (decimal?)s.Cantidad).Sum() ?? 0,
            StockMinimo = x.Existencia.Where(s => s.AlmacenId == almacenId).Select(s => (decimal?)s.StockMinimo).FirstOrDefault() ?? 0,
            ActualizadoEn = x.ActualizadoEn
        }).ToListAsync(cancellationToken);

        // El precio plano (producto.precio_venta_actual) puede estar vacío: el precio real vive en
        // la lista vigente. Sin este respaldo el terminal recibe todos los productos "Sin precio"
        // y no puede cobrar (el catálogo es lo único que consulta el POS para precargar el carrito).
        var sinPrecio = items.Where(x => !(x.PrecioVentaActual > 0)).Select(x => x.Id).ToList();
        if (sinPrecio.Count > 0)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var listaId = await _db.ListaPrecios.AsNoTracking()
                .Where(l => l.EntidadId == entidadId && l.Activa
                    && l.VigenteDesde <= hoy && (l.VigenteHasta == null || l.VigenteHasta >= hoy))
                .OrderByDescending(l => l.VigenteDesde)
                .Select(l => (Guid?)l.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (listaId.HasValue)
            {
                var precios = await _db.ListaPrecioDetalles.AsNoTracking()
                    .Where(d => d.ListaPrecioId == listaId.Value && d.Precio > 0 && sinPrecio.Contains(d.ProductoId))
                    .Select(d => new { d.ProductoId, d.Precio })
                    .ToDictionaryAsync(d => d.ProductoId, d => d.Precio, cancellationToken);

                foreach (var item in items)
                {
                    if (!(item.PrecioVentaActual > 0) && precios.TryGetValue(item.Id, out var precio))
                        item.PrecioVentaActual = precio;
                }
            }
        }

        return items;
    }

    public async Task<IReadOnlyList<object>> GetClientesAsync(Guid entidadId, CancellationToken cancellationToken)
    {
        return await _db.Clientes.AsNoTracking()
            .Where(x => x.EntidadId == entidadId && x.Activo)
            .OrderBy(x => x.NombreRazonSocial)
            .Select(x => (object)new { x.Id, nombre = x.NombreRazonSocial, identificacion = x.NitOCi, x.Telefono, x.Email })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PosDispositivoDto>> GetDispositivosAsync(Guid entidadId, CancellationToken cancellationToken)
    {
        return await _db.DispositivoPos
            .AsNoTracking()
            .Where(x => x.EntidadId == entidadId && (x.Estado == "ACTIVO" || x.Estado == "ONLINE"))
            .OrderBy(x => x.Codigo)
            .Select(x => new PosDispositivoDto
            {
                Id = x.Id,
                Codigo = x.Codigo,
                Nombre = x.Nombre,
                SucursalId = x.SucursalId,
                AlmacenId = x.AlmacenId,
                AlmacenNombre = _db.Almacens.Where(a => a.Id == x.AlmacenId).Select(a => a.Nombre).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }
}