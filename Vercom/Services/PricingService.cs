using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public enum PriceSource
{
    SinPrecio = 0,
    Plano = 1,
    ListaPrecio = 2
}

public sealed record ResolvedPrice(decimal Precio, PriceSource Source, Guid? ListaPrecioId, string? ListaNombre)
{
    public string SourceName => Source switch
    {
        PriceSource.ListaPrecio => "LISTA",
        PriceSource.Plano => "PLANO",
        _ => "SIN_PRECIO"
    };
}

public sealed record PrecioVentaDto
{
    public Guid ProductoId { get; init; }
    public decimal Precio { get; init; }
    public string Origen { get; init; } = "SIN_PRECIO";
    public Guid? ListaPrecioId { get; init; }
    public string? ListaNombre { get; init; }
}

public interface IPricingService
{
    Task<IReadOnlyDictionary<Guid, ResolvedPrice>> GetPriceMapAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrecioVentaDto>> GetSalePricesAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default);

    Task<ResolvedPrice> ResolveAsync(Guid entidadId, Guid productoId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default);
}

public sealed class PricingService : IPricingService
{
    private readonly AppDbContext _db;

    public PricingService(AppDbContext db) => _db = db;

    private async Task<(Guid Id, string Nombre)?> GetListaVigenteAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken)
    {
        if (!clienteId.HasValue || clienteId.Value == Guid.Empty) return null;

        var listaPrecioId = await _db.Clientes.AsNoTracking()
            .Where(c => c.Id == clienteId.Value && c.EntidadId == entidadId && c.Activo)
            .Select(c => c.ListaPrecioId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!listaPrecioId.HasValue || listaPrecioId.Value == Guid.Empty) return null;

        var lista = await _db.ListaPrecios.AsNoTracking()
            .Where(l => l.Id == listaPrecioId.Value
                && l.EntidadId == entidadId
                && l.Activa
                && l.VigenteDesde <= fecha
                && (l.VigenteHasta == null || l.VigenteHasta >= fecha))
            .Select(l => new { l.Id, l.Nombre })
            .FirstOrDefaultAsync(cancellationToken);

        return lista == null ? null : (lista.Id, lista.Nombre);
    }

    public async Task<IReadOnlyDictionary<Guid, ResolvedPrice>> GetPriceMapAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var planos = await _db.Productos.AsNoTracking()
            .Where(p => p.EntidadId == entidadId && p.Activo)
            .Select(p => new { p.Id, p.PrecioVentaActual })
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<Guid, ResolvedPrice>(planos.Count);
        foreach (var p in planos)
        {
            var precio = p.PrecioVentaActual ?? 0m;
            resultado[p.Id] = new ResolvedPrice(precio, precio > 0m ? PriceSource.Plano : PriceSource.SinPrecio, null, null);
        }

        var lista = await GetListaVigenteAsync(entidadId, clienteId, fecha, cancellationToken);
        if (lista.HasValue)
        {
            var precios = await _db.ListaPrecioDetalles.AsNoTracking()
                .Where(d => d.ListaPrecioId == lista.Value.Id && d.Precio > 0m)
                .Select(d => new { d.ProductoId, d.Precio })
                .ToListAsync(cancellationToken);

            foreach (var d in precios)
            {
                resultado[d.ProductoId] = new ResolvedPrice(d.Precio, PriceSource.ListaPrecio, lista.Value.Id, lista.Value.Nombre);
            }
        }

        return resultado;
    }

    public async Task<IReadOnlyList<PrecioVentaDto>> GetSalePricesAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var map = await GetPriceMapAsync(entidadId, clienteId, fecha, cancellationToken);

        return map.Select(kv => new PrecioVentaDto
        {
            ProductoId = kv.Key,
            Precio = kv.Value.Precio,
            Origen = kv.Value.SourceName,
            ListaPrecioId = kv.Value.ListaPrecioId,
            ListaNombre = kv.Value.ListaNombre
        }).ToList();
    }

    public async Task<ResolvedPrice> ResolveAsync(Guid entidadId, Guid productoId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var lista = await GetListaVigenteAsync(entidadId, clienteId, fecha, cancellationToken);

        if (lista.HasValue)
        {
            var precioLista = await _db.ListaPrecioDetalles.AsNoTracking()
                .Where(d => d.ListaPrecioId == lista.Value.Id && d.ProductoId == productoId)
                .Select(d => (decimal?)d.Precio)
                .FirstOrDefaultAsync(cancellationToken);

            if (precioLista.HasValue && precioLista.Value > 0m)
            {
                return new ResolvedPrice(precioLista.Value, PriceSource.ListaPrecio, lista.Value.Id, lista.Value.Nombre);
            }
        }

        var plano = await _db.Productos.AsNoTracking()
            .Where(p => p.Id == productoId && p.EntidadId == entidadId)
            .Select(p => p.PrecioVentaActual)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        return new ResolvedPrice(plano, plano > 0m ? PriceSource.Plano : PriceSource.SinPrecio, null, null);
    }
}