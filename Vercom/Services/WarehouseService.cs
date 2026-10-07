using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Security;

namespace Vercom.Services;

public interface IWarehouseService
{
    // Lectura
    Task<IEnumerable<ConteoFisico>> GetCountsAsync();
    Task<ConteoFisico?> GetCountByIdAsync(Guid id);

    // Operaciones
    Task<(bool Succeeded, string Message, ConteoFisico? Count)> StartPhysicalCountAsync(Guid almacenId, Guid userId);
    Task<(bool Succeeded, string Message)> SubmitCountDetailAsync(Guid conteoId, Guid productoId, decimal cantidadFisica, string? reason);
    Task<(bool Succeeded, string Message)> CloseAndAdjustCountAsync(Guid conteoId, Guid userId);
}

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IEntidadProvider _entidadProvider;

    public WarehouseService(AppDbContext context, IInventoryService inventoryService, IEntidadProvider entidadProvider)
    {
        _context = context;
        _inventoryService = inventoryService;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<ConteoFisico>> GetCountsAsync()
    {
        return await _context.ConteoFisicos.Include(c => c.Almacen).OrderByDescending(c => c.Fecha).ToListAsync();
    }

    public async Task<ConteoFisico?> GetCountByIdAsync(Guid id)
    {
        return await _context.ConteoFisicos
            .Include(c => c.Almacen)
            .Include(c => c.Responsable)
            .Include(c => c.ConteoFisicoDetalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool Succeeded, string Message, ConteoFisico? Count)> StartPhysicalCountAsync(Guid almacenId, Guid userId)
    {
        if (!await _context.Almacens.AnyAsync(a =>
            a.Id == almacenId && a.EntidadId == _entidadProvider.CurrentEntidadId && a.Activo))
            return (false, "El almacén no existe, está inactivo o pertenece a otra entidad.", null);

        var active = await _context.ConteoFisicos.AnyAsync(c => c.AlmacenId == almacenId && c.Estado == "EN_PROCESO");
        if (active) return (false, "Ya hay un conteo físico activo en este almacén.", null);

        var conteo = new ConteoFisico
        {
            Id = Guid.NewGuid(),
            AlmacenId = almacenId,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Tipo = "PARCIAL",
            Estado = "EN_PROCESO",
            ResponsableId = userId,
            CreadoEn = DateTimeOffset.Now
        };

        _context.ConteoFisicos.Add(conteo);
        await _context.SaveChangesAsync();

        return (true, "Conteo físico iniciado.", conteo);
    }

    public async Task<(bool Succeeded, string Message)> SubmitCountDetailAsync(Guid conteoId, Guid productoId, decimal cantidadFisica, string? reason)
    {
        var conteo = await _context.ConteoFisicos.FindAsync(conteoId);
        if (conteo == null || conteo.Estado != "EN_PROCESO") return (false, "El conteo no está activo.");
        if (cantidadFisica < 0) return (false, "La cantidad física no puede ser negativa.");
        if (!await _context.Almacens.AnyAsync(a => a.Id == conteo.AlmacenId && a.EntidadId == _entidadProvider.CurrentEntidadId))
            return (false, "El conteo pertenece a otra entidad.");
        if (!await _context.Productos.AnyAsync(p => p.Id == productoId && p.EntidadId == _entidadProvider.CurrentEntidadId && p.Activo))
            return (false, "El producto no existe, está inactivo o pertenece a otra entidad.");

        var stockSistema = await _context.Existencia
            .Where(e => e.AlmacenId == conteo.AlmacenId && e.ProductoId == productoId)
            .Select(e => e.Cantidad)
            .FirstOrDefaultAsync();

        var detail = await _context.ConteoFisicoDetalles
            .FirstOrDefaultAsync(d => d.ConteoId == conteoId && d.ProductoId == productoId);

        if (detail == null)
        {
            detail = new ConteoFisicoDetalle
            {
                Id = Guid.NewGuid(),
                ConteoId = conteoId,
                ProductoId = productoId,
                CantidadSistema = stockSistema
            };
            _context.ConteoFisicoDetalles.Add(detail);
        }
        else
        {
            detail.CantidadSistema = stockSistema;
        }

        detail.CantidadFisica = cantidadFisica;
        detail.Diferencia = cantidadFisica - detail.CantidadSistema;
        detail.Justificacion = reason;

        await _context.SaveChangesAsync();
        return (true, "Detalle guardado.");
    }

    // Delegado al motor de inventario: una sola implementación de conciliación
    // (tipos de ajuste correctos, cantidades positivas y transacción atómica).
    public Task<(bool Succeeded, string Message)> CloseAndAdjustCountAsync(Guid conteoId, Guid userId)
        => _inventoryService.ConciliatePhysicalCountAsync(conteoId, userId);
}
