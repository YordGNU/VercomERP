using Microsoft.EntityFrameworkCore;
using Vercom.Models;

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

    public WarehouseService(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<IEnumerable<ConteoFisico>> GetCountsAsync()
    {
        return await _context.ConteoFisicos.Include(c => c.Almacen).OrderByDescending(c => c.Fecha).ToListAsync();
    }

    public async Task<ConteoFisico?> GetCountByIdAsync(Guid id)
    {
        return await _context.ConteoFisicos.Include(c => c.Almacen).Include(c => c.ConteoFisicoDetalles).ThenInclude(d => d.Producto).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool Succeeded, string Message, ConteoFisico? Count)> StartPhysicalCountAsync(Guid almacenId, Guid userId)
    {
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

        detail.CantidadFisica = cantidadFisica;
        detail.Diferencia = cantidadFisica - detail.CantidadSistema;
        detail.Justificacion = reason;

        await _context.SaveChangesAsync();
        return (true, "Detalle guardado.");
    }

    public async Task<(bool Succeeded, string Message)> CloseAndAdjustCountAsync(Guid conteoId, Guid userId)
    {
        var conteo = await _context.ConteoFisicos
            .Include(c => c.ConteoFisicoDetalles)
            .FirstOrDefaultAsync(c => c.Id == conteoId);

        if (conteo == null || conteo.Estado != "EN_PROCESO") return (false, "Conteo no válido.");

        var diferencias = conteo.ConteoFisicoDetalles.Where(d => d.Diferencia != 0).ToList();

        if (diferencias.Any())
        {
            var mov = new MovimientoInventario
            {
                EntidadId = await _context.Almacens.Where(a => a.Id == conteo.AlmacenId).Select(a => a.EntidadId).FirstAsync(),
                TipoMovimientoId = 4, // AJU: Ajuste de Inventario
                NumeroDocumento = $"AJU-{conteo.Fecha:yyyyMMdd}",
                AlmacenDestinoId = conteo.AlmacenId, // Si es positivo suma, si es negativo resta
                Fecha = DateTimeOffset.Now,
                ReferenciaExternaTipo = "CONTEO_FISICO",
                ReferenciaExternaId = conteo.Id,
                Canal = "ERP",
                Observaciones = "Ajuste por conteo físico.",
                CreadoPor = userId
            };

            foreach (var d in diferencias)
            {
                mov.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = d.ProductoId,
                    Cantidad = d.Diferencia ?? 0,
                    Observaciones = d.Justificacion
                });
            }

            var result = await _inventoryService.ProcessMovementAsync(mov);
            if (!result.Succeeded) return (false, $"Error al ajustar: {result.Message}");
        }

        conteo.Estado = "CERRADO";
        await _context.SaveChangesAsync();

        return (true, "Conteo cerrado y existencias ajustadas.");
    }
}
