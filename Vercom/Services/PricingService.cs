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

public sealed record PriceResolutionDecision(decimal Precio, PriceSource Source, Guid? ListaPrecioId, string? ListaNombre, string RuleName)
{
    public static PriceResolutionDecision Base(decimal basePrice)
        => new(basePrice, PriceSource.Plano, null, null, "BASE");

    public static PriceResolutionDecision List(decimal price, Guid listaId, string listaNombre)
        => new(price, PriceSource.ListaPrecio, listaId, listaNombre, "LISTA_CLIENTE");

    public static PriceResolutionDecision None()
        => new(0m, PriceSource.SinPrecio, null, null, "SIN_PRECIO");
}

public static class PriceResolutionEngine
{
    public static PriceResolutionDecision Resolve(decimal basePrice, decimal? listPrice, bool hasActiveList, bool isInsideValidityWindow, string? listName = null, Guid? listId = null)
    {
        if (hasActiveList && isInsideValidityWindow && listPrice.HasValue && listPrice.Value > 0m)
            return PriceResolutionDecision.List(listPrice.Value, listId ?? Guid.Empty, listName ?? "LISTA");

        if (basePrice > 0m)
            return PriceResolutionDecision.Base(basePrice);

        return PriceResolutionDecision.None();
    }
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
        var lista = await GetListaVigenteAsync(entidadId, clienteId, fecha, cancellationToken);

        var preciosLista = new Dictionary<Guid, decimal>();
        if (lista.HasValue)
        {
            preciosLista = await _db.ListaPrecioDetalles.AsNoTracking()
                .Where(d => d.ListaPrecioId == lista.Value.Id && d.Precio > 0m)
                .Select(d => new { d.ProductoId, d.Precio })
                .ToDictionaryAsync(d => d.ProductoId, d => d.Precio, cancellationToken);
        }

        foreach (var p in planos)
        {
            var basePrice = p.PrecioVentaActual ?? 0m;
            var listPrice = preciosLista.TryGetValue(p.Id, out var lp) ? lp : (decimal?)null;
            var decision = PriceResolutionEngine.Resolve(
                basePrice,
                listPrice,
                lista.HasValue,
                lista.HasValue,
                lista?.Nombre,
                lista?.Id);

            resultado[p.Id] = new ResolvedPrice(decision.Precio, decision.Source, decision.ListaPrecioId, decision.ListaNombre);
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

        var basePrice = await _db.Productos.AsNoTracking()
            .Where(p => p.Id == productoId && p.EntidadId == entidadId)
            .Select(p => p.PrecioVentaActual)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        decimal? precioLista = null;
        if (lista.HasValue)
        {
            precioLista = await _db.ListaPrecioDetalles.AsNoTracking()
                .Where(d => d.ListaPrecioId == lista.Value.Id && d.ProductoId == productoId)
                .Select(d => (decimal?)d.Precio)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var decision = PriceResolutionEngine.Resolve(
            basePrice,
            precioLista,
            lista.HasValue,
            lista.HasValue,
            lista?.Nombre,
            lista?.Id);

        return new ResolvedPrice(decision.Precio, decision.Source, decision.ListaPrecioId, decision.ListaNombre);
    }
}

public interface IPriceResolutionService
{
    Task<ResolvedPrice> ResolveEffectivePriceAsync(Guid entidadId, Guid productoId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, ResolvedPrice>> ResolveEffectivePriceMapAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default);
}

public sealed class PriceResolutionService : IPriceResolutionService
{
    private readonly AppDbContext _db;

    public PriceResolutionService(AppDbContext db) => _db = db;

    public async Task<ResolvedPrice> ResolveEffectivePriceAsync(Guid entidadId, Guid productoId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var pricing = new PricingService(_db);
        return await pricing.ResolveAsync(entidadId, productoId, clienteId, fecha, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, ResolvedPrice>> ResolveEffectivePriceMapAsync(Guid entidadId, Guid? clienteId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var pricing = new PricingService(_db);
        return await pricing.GetPriceMapAsync(entidadId, clienteId, fecha, cancellationToken);
    }
}