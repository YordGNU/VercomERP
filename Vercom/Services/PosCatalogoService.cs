using Microsoft.EntityFrameworkCore;
using Vercom.DTOs;
using Vercom.Models;

namespace Vercom.Services;

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

    public async Task<IReadOnlyList<PosProductoDto>> GetProductosAsync(Guid almacenId, string? search, DateTimeOffset? updatedSince, CancellationToken cancellationToken)
    {
        var query = _db.Productos.AsNoTracking().Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Codigo.Contains(search) || (x.CodigoBarras != null && x.CodigoBarras.Contains(search)) || x.Nombre.Contains(search));
        if (updatedSince.HasValue)
            query = query.Where(x => x.ActualizadoEn > updatedSince.Value);

        return await query.OrderBy(x => x.Nombre).Select(x => new PosProductoDto
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