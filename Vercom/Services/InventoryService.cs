using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IInventoryService
{
    // Lectura Stock y Movimientos
    Task<IEnumerable<Existencium>> GetStocksAsync();
    Task<IEnumerable<MovimientoInventario>> GetMovementsAsync();
    Task<MovimientoInventario?> GetMovementByIdAsync(Guid id);
    Task<InventoryMovementCreateViewModel> GetMovementCreateContextAsync(MovimientoInventario? existingMovement = null);

    // Gestión de Catálogo (Productos)
    Task<IEnumerable<Producto>> GetCatalogAsync();
    Task<Producto?> GetProductByIdAsync(Guid id);
    Task<ProductFormViewModel> GetProductFormContextAsync(Producto? existing = null);
    Task<(bool Succeeded, string Message)> CreateProductAsync(Producto product);
    Task<(bool Succeeded, string Message)> UpdateProductAsync(Producto product);
    Task<(bool Succeeded, string Message)> DeleteProductAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<DeleteResult> DeleteSelectedAsync(List<Guid> ids);

    // Gestión de Almacenes
    Task<IEnumerable<Almacen>> GetWarehousesAsync();
    Task<Almacen?> GetWarehouseByIdAsync(Guid id);
    Task<AlmacenFormViewModel> GetWarehouseFormContextAsync(Almacen? existing = null);
    Task<(bool Succeeded, string Message)> CreateWarehouseAsync(Almacen warehouse);
    Task<(bool Succeeded, string Message)> UpdateWarehouseAsync(Almacen warehouse);

    // Gestión de Familias
    Task<IEnumerable<FamiliaProducto>> GetFamiliesAsync();
    Task<FamiliaProducto?> GetFamilyByIdAsync(Guid id);
    Task<FamiliaFormViewModel> GetFamilyFormContextAsync(FamiliaProducto? existing = null);
    Task<(bool Succeeded, string Message)> CreateFamilyAsync(FamiliaProducto family);
    Task<(bool Succeeded, string Message)> UpdateFamilyAsync(FamiliaProducto family);

    // Gestión de Unidades de Medida
    Task<IEnumerable<UnidadMedidum>> GetUnitsAsync();
    Task<UnidadMedidum?> GetUnitByIdAsync(int id);
    Task<UnidadFormViewModel> GetUnitFormContextAsync(UnidadMedidum? existing = null);
    Task<(bool Succeeded, string Message)> CreateUnitAsync(UnidadMedidum unit);
    Task<(bool Succeeded, string Message)> UpdateUnitAsync(UnidadMedidum unit);

    // Gestión de Listas de Precio
    Task<IEnumerable<ListaPrecio>> GetPriceListsAsync();
    Task<ListaPrecio?> GetPriceListByIdAsync(Guid id);
    Task<ListaPrecioFormViewModel> GetPriceListFormContextAsync(ListaPrecio? existing = null);
    Task<(bool Succeeded, string Message)> CreatePriceListAsync(ListaPrecio priceList);
    Task<(bool Succeeded, string Message)> UpdatePriceListAsync(ListaPrecio priceList);

    // Tipos de Movimiento
    Task<IEnumerable<TipoMovimiento>> GetMovementTypesAsync();
    /// <summary>Devuelve el Id del tipo de movimiento para un código, creándolo si no existe.</summary>
    Task<int> EnsureMovementTypeAsync(string codigo);

    // Escritura Almacén
    Task<(bool Succeeded, string Message, MovimientoInventario? Movement)> ProcessMovementAsync(MovimientoInventario movement);
    Task<decimal> GetStockAsync(Guid almacenId, Guid productoId);
    Task<List<Existencium>> GetLowStockAlertsAsync(Guid entidadId);
    /// <summary>Genera el asiento contable canónico (Inventario ↔ contrapartida) de un
    /// movimiento ya persistido que se procesó por fuera de ProcessMovementAsync (p. ej. POS).</summary>
    Task GenerateAccountingEntryForExistingMovementAsync(MovimientoInventario movement, CancellationToken ct = default);
    Task<List<ExistenciaLote>> GetExpiryAlertsAsync(Guid entidadId, int daysThreshold);
    Task<List<KardexRowViewModel>> GetKardexByProductAsync(Guid productoId, Guid? almacenId = null);
    Task<(bool Succeeded, string Message)> TransferBetweenWarehousesAsync(Guid origenId, Guid destinoId, List<MovimientoInventarioDetalle> items, Guid userId);
    Task<(bool Succeeded, string Message)> ConciliatePhysicalCountAsync(Guid countId, Guid userId);
    Task<(bool Succeeded, string Message)> DeleteFamiliaAsync(Guid id);
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IParametroSistemaService _paramService;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(AppDbContext context, IAccountingService accountingService,
        IParametroSistemaService paramService, Security.IEntidadProvider entidadProvider,
        ILogger<InventoryService> logger)
    {
        _context = context;
        _accountingService = accountingService;
        _paramService = paramService;
        _entidadProvider = entidadProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<Existencium>> GetStocksAsync()
    {
        return await _context.Existencia
            .Include(e => e.Producto)
            .Include(e => e.Almacen)
            .OrderBy(e => e.Almacen.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<MovimientoInventario>> GetMovementsAsync()
    {
        return await _context.MovimientoInventarios
            .Include(m => m.TipoMovimiento)
            .Include(m => m.AlmacenOrigen)
            .Include(m => m.AlmacenDestino)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<MovimientoInventario?> GetMovementByIdAsync(Guid id)
    {
        return await _context.MovimientoInventarios
            .Include(m => m.TipoMovimiento)
            .Include(m => m.AlmacenOrigen)
            .Include(m => m.AlmacenDestino)
            .Include(m => m.MovimientoInventarioDetalles).ThenInclude(d => d.Producto).ThenInclude(p => p.UnidadMedida)
            .Include(m => m.Asiento)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<InventoryMovementCreateViewModel> GetMovementCreateContextAsync(MovimientoInventario? existingMovement = null)
    {
        return new InventoryMovementCreateViewModel
        {
            Movement = existingMovement ?? new MovimientoInventario { Fecha = DateTimeOffset.Now, Canal = "ERP" },
            TiposMovimiento = new SelectList(await _context.TipoMovimientos.OrderBy(t => t.Nombre).ToListAsync(), "Id", "Nombre"),
            Almacenes = new SelectList(await _context.Almacens.Where(a => a.Activo).ToListAsync(), "Id", "Nombre"),
            ProductosDisponibles = await _context.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.Id, Display = p.Codigo + " - " + p.Nombre })
                .ToListAsync()
        };
    }

    public async Task<IEnumerable<Producto>> GetCatalogAsync()
    {
        return await _context.Productos
            .Include(p => p.Familia)
            .Include(p => p.UnidadMedida)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto?> GetProductByIdAsync(Guid id)
    {
        return await _context.Productos
            .Include(p => p.Familia)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Existencia).ThenInclude(e => e.Almacen)
            .Include(p => p.CuentaInventario)
            .Include(p => p.CuentaCostoVenta)
            .Include(p => p.CuentaIngreso)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ProductFormViewModel> GetProductFormContextAsync(Producto? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;

        // Inventario: cuentas de activo de naturaleza deudora. Se excluyen las
        // subcuentas reguladoras acreedoras (ej. 188.0030 / 188.0050).
        var cuentasActivo = await _context.CuentaContables
              .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento
                       && c.Clase == "ACTIVO" && c.Naturaleza == "DEUDORA"
                       && (c.Codigo.StartsWith("183") || c.Codigo.StartsWith("184")
                        || c.Codigo.StartsWith("185") || c.Codigo.StartsWith("187")
                        || c.Codigo.StartsWith("188") || c.Codigo.StartsWith("189")
                        || c.Codigo == "207" || c.Codigo == "208" || c.Codigo == "209"))
              .OrderBy(c => c.Codigo)
              .Select(c => new { c.Id, c.Codigo, Display = $"{c.Codigo} - {c.Nombre}" })
              .ToListAsync();

        // Costo de venta: cuentas de gasto de lo vendido (810/811/814).
        var cuentasGasto = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento
                     && c.Clase == "GASTOS" && c.Naturaleza == "DEUDORA"
                     && (c.Codigo == "810" || c.Codigo == "811" || c.Codigo == "814"))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, c.Codigo, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        // Ingreso por ventas (900/901/904).
        var cuentasIngreso = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento
                     && c.Clase == "INGRESOS" && c.Naturaleza == "ACREEDORA"
                     && (c.Codigo == "900" || c.Codigo == "901" || c.Codigo == "904"))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, c.Codigo, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        var invPorCodigo = cuentasActivo.ToDictionary(c => c.Codigo, c => c.Id);
        var costoPorCodigo = cuentasGasto.ToDictionary(c => c.Codigo, c => c.Id);
        var ingresoPorCodigo = cuentasIngreso.ToDictionary(c => c.Codigo, c => c.Id);

        Guid? Resolver(Dictionary<string, Guid> mapa, string? codigo) =>
            codigo != null && mapa.TryGetValue(codigo, out var id) ? id : (Guid?)null;

        var tipos = new[] { "MATERIA_PRIMA", "EN_PROCESO", "TERMINADO", "SERVICIO", "MERCANCIA" };

        var cuentasPorTipo = tipos.ToDictionary(tipo => tipo, tipo =>
        {
            var (inventario, costo, ingreso) = CuentasContablesSugeridasPorTipo(tipo);
            return new CuentasContablesSugeridas
            {
                InventarioId = Resolver(invPorCodigo, inventario),
                CostoVentaId = Resolver(costoPorCodigo, costo),
                IngresoId = Resolver(ingresoPorCodigo, ingreso)
            };
        });

        var producto = existing ?? new Producto { Activo = true, AplicaImpuestoVentas = true, Tipo = "TERMINADO" };

        // Asignación por defecto según el propósito/naturaleza del tipo de producto;
        // solo rellena cuentas aún no definidas, el usuario puede cambiarlas.
        if (cuentasPorTipo.TryGetValue(producto.Tipo, out var sugeridas))
        {
            producto.CuentaInventarioId ??= sugeridas.InventarioId;
            producto.CuentaCostoVentaId ??= sugeridas.CostoVentaId;
            producto.CuentaIngresoId ??= sugeridas.IngresoId;
        }

        return new ProductFormViewModel
        {
            Producto = producto,
            Familias = new SelectList(await _context.FamiliaProductos.ToListAsync(), "Id", "Nombre"),
            UnidadesMedida = new SelectList(await _context.UnidadMedida.ToListAsync(), "Id", "Nombre"),
            CuentasInventario = new SelectList(cuentasActivo, "Id", "Display"),
            CuentasCostoVenta = new SelectList(cuentasGasto, "Id", "Display"),
            CuentasIngresos = new SelectList(cuentasIngreso, "Id", "Display"),
            TiposProducto = new SelectList(tipos),
            CuentasPorTipo = cuentasPorTipo
        };
    }

    // Mapeo de cuentas según la norma cubana y el propósito del artículo:
    // inventario (183/185/188/189), costo de lo vendido (810/811/814) e ingreso (900/901/904).
    private static (string? Inventario, string? Costo, string? Ingreso) CuentasContablesSugeridasPorTipo(string tipo) => tipo switch
    {
        "MATERIA_PRIMA" => ("183.0010", null, null),
        "EN_PROCESO" => ("185.0010", null, null),
        "TERMINADO" => ("188.0020", "810", "900"),
        "SERVICIO" => (null, "811", "901"),
        "MERCANCIA" => ("189.0010", "814", "904"),
        _ => (null, null, null)
    };

    public async Task<(bool Succeeded, string Message)> CreateProductAsync(Producto product)
    {
        try
        {
            product.Id = Guid.NewGuid();
            product.EntidadId = _entidadProvider.CurrentEntidadId;
            product.CreadoEn = DateTimeOffset.Now;
            product.ActualizadoEn = DateTimeOffset.Now;
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();
            return (true, "Producto creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateProductAsync(Producto product)
    {
        try
        {
            var existing = await _context.Productos.FindAsync(product.Id);
            if (existing == null) return (false, "No existe.");

            _context.Entry(existing).CurrentValues.SetValues(product);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            existing.ActualizadoEn = DateTimeOffset.Now;

            await _context.SaveChangesAsync();
            return (true, "Producto actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteProductAsync(Guid id)
    {
        try
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null) return (false, "No existe.");

            var hasStock = await _context.Existencia.AnyAsync(e => e.ProductoId == id && e.Cantidad > 0);
            if (hasStock) return (false, "RF-34: No se puede eliminar con existencias activas.");

            _context.Productos.Remove(product);
            await _context.SaveChangesAsync();
            return (true, "Producto eliminado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<Almacen>> GetWarehousesAsync()
    {
        return await _context.Almacens
            .Include(a => a.Sucursal)
            .OrderBy(a => a.Nombre)
            .ToListAsync();
    }

    public async Task<Almacen?> GetWarehouseByIdAsync(Guid id)
    {
        return await _context.Almacens
            .Include(a => a.Sucursal)
            .Include(a => a.Existencia).ThenInclude(e => e.Producto)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AlmacenFormViewModel> GetWarehouseFormContextAsync(Almacen? existing = null)
    {
        return new AlmacenFormViewModel
        {
            Almacen = existing ?? new Almacen { Activo = true },
            Sucursales = new SelectList(await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateWarehouseAsync(Almacen warehouse)
    {
        try
        {
            warehouse.Id = Guid.NewGuid();
            warehouse.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.Almacens.Add(warehouse);
            await _context.SaveChangesAsync();
            return (true, "Almacén creado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateWarehouseAsync(Almacen warehouse)
    {
        try
        {
            var existing = await _context.Almacens.FindAsync(warehouse.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(warehouse);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Almacén actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<FamiliaProducto>> GetFamiliesAsync()
    {
        return await _context.FamiliaProductos
            .OrderBy(f => f.Nombre)
            .ToListAsync();
    }

    public async Task<FamiliaProducto?> GetFamilyByIdAsync(Guid id)
    {
        return await _context.FamiliaProductos.FindAsync(id);
    }

    public async Task<FamiliaFormViewModel> GetFamilyFormContextAsync(FamiliaProducto? existing = null)
    {
        var familiasPadre = await _context.FamiliaProductos
            .OrderBy(f => f.Nombre)
            .ToListAsync();

        return new FamiliaFormViewModel
        {
            Familia = existing ?? new FamiliaProducto(),
            FamiliasPadre = new SelectList(familiasPadre, "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateFamilyAsync(FamiliaProducto family)
    {
        try
        {
            family.Id = Guid.NewGuid();
            family.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.FamiliaProductos.Add(family);
            await _context.SaveChangesAsync();
            return (true, "Familia creada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateFamilyAsync(FamiliaProducto family)
    {
        try
        {
            var existing = await _context.FamiliaProductos.FindAsync(family.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(family);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Familia actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteFamiliaAsync(Guid id)
    {
        try
        {
            var familia = await _context.FamiliaProductos.FirstOrDefaultAsync(f => f.Id == id && f.EntidadId == _entidadProvider.CurrentEntidadId);

            if (familia == null)
                return (false, "Familia no encontrada.");

            // Validación 1: No tiene productos asociados
            var tieneProductos = await _context.Productos
                .AnyAsync(p => p.FamiliaId == id);

            if (tieneProductos)
                return (false, "No se puede eliminar la familia porque tiene productos asociados.");

            // Validación 2: No tiene subfamilias
            var tieneSubfamilias = await _context.FamiliaProductos
                .AnyAsync(f => f.FamiliaPadreId == id);

            if (tieneSubfamilias)
                return (false, "No se puede eliminar la familia porque tiene subfamilias asociadas.");

            _context.FamiliaProductos.Remove(familia);
            await _context.SaveChangesAsync();
            // El AuditInterceptor registra el DELETE automáticamente

            return (true, $"Familia '{familia.Nombre}' eliminada correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar: {ex.Message}");
        }
    }

    public async Task<IEnumerable<UnidadMedidum>> GetUnitsAsync()
    {
        return await _context.UnidadMedida.OrderBy(u => u.Nombre).ToListAsync();
    }

    public async Task<UnidadMedidum?> GetUnitByIdAsync(int id)
    {
        return await _context.UnidadMedida.FindAsync(id);
    }

    public async Task<UnidadFormViewModel> GetUnitFormContextAsync(UnidadMedidum? existing = null)
    {
        return new UnidadFormViewModel
        {
            Unidad = existing ?? new UnidadMedidum()
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateUnitAsync(UnidadMedidum unit)
    {
        try
        {
            _context.UnidadMedida.Add(unit);
            await _context.SaveChangesAsync();
            return (true, "Unidad creada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateUnitAsync(UnidadMedidum unit)
    {
        try
        {
            _context.Update(unit);
            await _context.SaveChangesAsync();
            return (true, "Unidad actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<ListaPrecio>> GetPriceListsAsync()
    {
        return await _context.ListaPrecios.OrderBy(l => l.Nombre).ToListAsync();
    }

    public async Task<ListaPrecio?> GetPriceListByIdAsync(Guid id)
    {
        return await _context.ListaPrecios.FindAsync(id);
    }

    public async Task<ListaPrecioFormViewModel> GetPriceListFormContextAsync(ListaPrecio? existing = null)
    {
        return new ListaPrecioFormViewModel
        {
            ListaPrecio = existing ?? new ListaPrecio { Activa = true }
        };
    }

    public async Task<(bool Succeeded, string Message)> CreatePriceListAsync(ListaPrecio priceList)
    {
        try
        {
            priceList.Id = Guid.NewGuid();
            priceList.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.ListaPrecios.Add(priceList);
            await _context.SaveChangesAsync();
            return (true, "Lista de precios creada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdatePriceListAsync(ListaPrecio priceList)
    {
        try
        {
            var existing = await _context.ListaPrecios.FindAsync(priceList.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(priceList);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Lista de precios actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<TipoMovimiento>> GetMovementTypesAsync()
    {
        return await _context.TipoMovimientos.OrderBy(t => t.Nombre).ToListAsync();
    }

    // Catálogo canónico de tipos de movimiento (fuente única en código). Evita depender
    // de Ids numéricos fijos, que en esta base de datos no coincidían con los sembrados.
    private static readonly (string Codigo, string Nombre, string Naturaleza, bool AfectaCosto)[] CatalogoTiposMovimiento =
    {
        ("RECEPCION", "Informe de Recepción", "ENTRADA", true),
        ("VALE_ENTREGA", "Vale de Entrega", "SALIDA", true),
        ("DEVOLUCION_ENTRADA", "Devolución de Cliente", "ENTRADA", true),
        ("DEVOLUCION_SALIDA", "Devolución a Proveedor", "SALIDA", true),
        ("TRANSFERENCIA_SALIDA", "Transferencia - Salida", "SALIDA", false),
        ("TRANSFERENCIA_ENTRADA", "Transferencia - Entrada", "ENTRADA", false),
        ("AJUSTE_POSITIVO", "Ajuste por Sobrante", "ENTRADA", true),
        ("AJUSTE_NEGATIVO", "Ajuste por Faltante", "SALIDA", true),
        ("CONSUMO_PRODUCCION", "Consumo en Producción", "SALIDA", true),
        ("DEVOLUCION_PRODUCCION", "Devolución de Materiales a Producción", "ENTRADA", true),
        ("ENTRADA_PRODUCCION", "Entrada de Producto Terminado", "ENTRADA", true),
        ("VENTA_POS", "Venta en Punto de Venta", "SALIDA", true),
    };

    public async Task<int> EnsureMovementTypeAsync(string codigo)
    {
        var existing = await _context.TipoMovimientos.FirstOrDefaultAsync(t => t.Codigo == codigo);
        if (existing != null) return existing.Id;

        var def = CatalogoTiposMovimiento.FirstOrDefault(c => c.Codigo == codigo);
        if (def.Codigo == null)
            throw new InvalidOperationException($"Tipo de movimiento '{codigo}' no reconocido.");

        var tipo = new TipoMovimiento
        {
            Codigo = def.Codigo,
            Nombre = def.Nombre,
            Naturaleza = def.Naturaleza,
            AfectaCosto = def.AfectaCosto
        };
        _context.TipoMovimientos.Add(tipo);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Carrera con otra creación: descartar el duplicado y releer.
            _context.Entry(tipo).State = EntityState.Detached;
            return (await _context.TipoMovimientos.FirstAsync(t => t.Codigo == codigo)).Id;
        }
        return tipo.Id;
    }

    public async Task<(bool Succeeded, string Message, MovimientoInventario? Movement)> ProcessMovementAsync(MovimientoInventario movement)
    {
        // Unirse a la transacción ambiente si el llamador ya abrió una (evita anidar transacciones).
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var tipo = await _context.TipoMovimientos.FindAsync(movement.TipoMovimientoId);
            if (tipo == null) return (false, "Tipo de movimiento no válido.", null);

            if (movement.Id == Guid.Empty) movement.Id = Guid.NewGuid();
            movement.CreadoEn = DateTimeOffset.Now;
            _context.MovimientoInventarios.Add(movement);

            foreach (var detail in movement.MovimientoInventarioDetalles)
            {
                if (movement.AlmacenOrigenId.HasValue)
                {
                    // SALIDA: Capturar PPP antes de descontar para el rastro de costo (RF-32)
                    var stock = await _context.Existencia.FirstOrDefaultAsync(e => e.AlmacenId == movement.AlmacenOrigenId && e.ProductoId == detail.ProductoId);
                    if (detail.CostoUnitario == null || detail.CostoUnitario == 0)
                        detail.CostoUnitario = stock?.CostoPromedio ?? 0;

                    var res = await UpdateStockAsync(movement.AlmacenOrigenId.Value, detail.ProductoId, -detail.QuantityNormalized(), detail.CostoUnitario, detail.Lote, detail.FechaVencimiento);
                    if (!res.Succeeded) throw new Exception(res.Message);
                }

                if (movement.AlmacenDestinoId.HasValue)
                {
                    var res = await UpdateStockAsync(movement.AlmacenDestinoId.Value, detail.ProductoId, detail.QuantityNormalized(), detail.CostoUnitario, detail.Lote, detail.FechaVencimiento);
                    if (!res.Succeeded) throw new Exception(res.Message);
                }
            }

            await _context.SaveChangesAsync();
            await CreateAccountingEntryAsync(movement, tipo);

            if (ownsTransaction) await transaction!.CommitAsync();

            return (true, "Movimiento procesado.", movement);
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            return (false, ex.Message, null);
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }

    private async Task<(bool Succeeded, string Message)> UpdateStockAsync(Guid almacenId, Guid productoId, decimal cantidad, decimal? costoEntrada, string? lote = null, DateOnly? fechaVencimiento = null)
    {
        // 1. Actualizar Existencia Global
        var existencia = await _context.Existencia
            .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.ProductoId == productoId);

        if (existencia == null)
        {
            if (cantidad < 0) return (false, "Existencia insuficiente (Registro no encontrado).");

            existencia = new Existencium
            {
                Id = Guid.NewGuid(),
                AlmacenId = almacenId,
                ProductoId = productoId,
                Cantidad = 0,
                CostoPromedio = costoEntrada ?? 0,
                ActualizadoEn = DateTimeOffset.Now
            };
            _context.Existencia.Add(existencia);
        }

        if (existencia.Cantidad + cantidad < 0)
            return (false, $"Stock insuficiente. Disponible: {existencia.Cantidad}");

        // 2. Actualizar Existencia por Lote (si se proporciona)
        if (!string.IsNullOrEmpty(lote))
        {
            var exLote = await _context.ExistenciaLotes
                .FirstOrDefaultAsync(l => l.AlmacenId == almacenId && l.ProductoId == productoId && l.Lote == lote);

            if (exLote == null)
            {
                if (cantidad < 0) return (false, $"El lote '{lote}' no tiene existencias registradas.");

                exLote = new ExistenciaLote
                {
                    Id = Guid.NewGuid(),
                    AlmacenId = almacenId,
                    ProductoId = productoId,
                    Lote = lote,
                    FechaVencimiento = fechaVencimiento,
                    Cantidad = 0,
                    ActualizadoEn = DateTimeOffset.Now
                };
                _context.ExistenciaLotes.Add(exLote);
            }

            if (exLote.Cantidad + cantidad < 0)
                return (false, $"Stock insuficiente en lote '{lote}'. Disponible: {exLote.Cantidad}");

            exLote.Cantidad += cantidad;
            exLote.ActualizadoEn = DateTimeOffset.Now;
        }

        // RF-32: PPP en entradas
        if (cantidad > 0 && costoEntrada.HasValue && costoEntrada.Value > 0)
        {
            var costoTotalActual = existencia.Cantidad * existencia.CostoPromedio;
            var costoNuevaEntrada = cantidad * costoEntrada.Value;
            existencia.CostoPromedio = (costoTotalActual + costoNuevaEntrada) / (existencia.Cantidad + cantidad);
        }

        existencia.Cantidad += cantidad;
        existencia.ActualizadoEn = DateTimeOffset.Now;

        return (true, "OK");
    }

    public async Task<(bool Succeeded, string Message)> TransferBetweenWarehousesAsync(Guid origenId, Guid destinoId, List<MovimientoInventarioDetalle> items, Guid userId)
    {
        var movement = new MovimientoInventario
        {
            EntidadId = await _context.Almacens.Where(a => a.Id == origenId).Select(a => a.EntidadId).FirstAsync(),
            TipoMovimientoId = await EnsureMovementTypeAsync("TRANSFERENCIA_SALIDA"), // ambas bodegas en un solo movimiento (no afecta costo)
            AlmacenOrigenId = origenId,
            AlmacenDestinoId = destinoId,
            NumeroDocumento = $"TRA-{DateTime.Now:yyyyMMddHHmm}",
            Fecha = DateTimeOffset.Now,
            Canal = "ERP",
            CreadoPor = userId,
            MovimientoInventarioDetalles = items
        };

        var result = await ProcessMovementAsync(movement);
        return (result.Succeeded, result.Message);
    }

    public async Task<(bool Succeeded, string Message)> ConciliatePhysicalCountAsync(Guid countId, Guid userId)
    {
        var count = await _context.ConteoFisicos
            .Include(c => c.ConteoFisicoDetalles)
            .FirstOrDefaultAsync(c => c.Id == countId);

        if (count == null || count.Estado != "EN_PROCESO") return (false, "Conteo no válido o ya cerrado.");

        var tipoFaltante = await EnsureMovementTypeAsync("AJUSTE_NEGATIVO");
        var tipoSobrante = await EnsureMovementTypeAsync("AJUSTE_POSITIVO");

        // Unirse a la transacción ambiente si el llamador ya abrió una.
        var ownsTransaction = _context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var entidadId = await _context.Almacens.Where(a => a.Id == count.AlmacenId).Select(a => a.EntidadId).FirstAsync();

            var index = 0;
            foreach (var detail in count.ConteoFisicoDetalles)
            {
                if (detail.Diferencia == 0) continue;

                var isFaltante = detail.Diferencia < 0;

                // RF-33: Exigir justificación obligatoria para faltantes (Expediente de Merma)
                if (isFaltante && string.IsNullOrWhiteSpace(detail.Justificacion))
                {
                    return (false, $"Se requiere justificación para el faltante del producto {detail.ProductoId}. Operación detenida.");
                }

                index++;
                var stockAdj = await _context.Existencia.FirstOrDefaultAsync(e => e.AlmacenId == count.AlmacenId && e.ProductoId == detail.ProductoId);
                var adjustment = new MovimientoInventario
                {
                    EntidadId = entidadId,
                    TipoMovimientoId = isFaltante ? tipoFaltante : tipoSobrante,
                    AlmacenOrigenId = isFaltante ? count.AlmacenId : null,
                    AlmacenDestinoId = isFaltante ? null : count.AlmacenId,
                    NumeroDocumento = $"AJU-{countId.ToString()[..8]}-{index}",
                    Fecha = DateTimeOffset.Now,
                    Observaciones = $"Ajuste automático por conteo físico. Justificación: {detail.Justificacion}",
                    Canal = "ERP",
                    CreadoPor = userId
                };

                adjustment.MovimientoInventarioDetalles.Add(new MovimientoInventarioDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = detail.ProductoId,
                    Cantidad = Math.Abs(detail.Diferencia ?? 0),
                    CostoUnitario = stockAdj?.CostoPromedio
                });

                var res = await ProcessMovementAsync(adjustment);
                if (!res.Succeeded) throw new Exception($"Fallo al procesar ajuste: {res.Message}");

                detail.MovimientoAjusteId = adjustment.Id;
            }

            count.Estado = "CERRADO";
            await _context.SaveChangesAsync();

            if (ownsTransaction) await transaction!.CommitAsync();
            return (true, "Conteo conciliado, inventario actualizado y asientos de ajuste generados.");
        }
        catch (Exception ex)
        {
            if (ownsTransaction) await transaction!.RollbackAsync();
            _logger.LogError(ex, "Error al conciliar conteo físico {CountId}", countId);
            return (false, ex.Message);
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }

    public async Task GenerateAccountingEntryForExistingMovementAsync(MovimientoInventario movement, CancellationToken ct = default)
    {
        var tipo = await _context.TipoMovimientos.FirstOrDefaultAsync(t => t.Id == movement.TipoMovimientoId, ct);
        if (tipo == null) return;
        await CreateAccountingEntryAsync(movement, tipo);
    }

    private async Task CreateAccountingEntryAsync(MovimientoInventario movement, TipoMovimiento tipo)
    {
        if (movement.AsientoId != null) return; // Evitar duplicidad

        // Los movimientos que no afectan costo (p. ej. transferencias entre almacenes) no generan asiento.
        if (!tipo.AfectaCosto) return;

        var tipoComprobante = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "DIA");
        if (tipoComprobante == null) return;

        var period = await _accountingService.GetOrCreateActivePeriodAsync(movement.EntidadId, movement.Fecha.DateTime);
        if (period == null || period.Estado != "ABIERTO") return;

        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = movement.EntidadId,
            PeriodoId = period.Id,
            Fecha = DateOnly.FromDateTime(movement.Fecha.DateTime),
            Concepto = $"INTEGRACIÓN INVENTARIO: {tipo.Nombre} #{movement.NumeroDocumento}",
            ModuloOrigen = "INVENTARIO",
            DocumentoOrigenTipo = "MOVIMIENTO_INVENTARIO",
            DocumentoOrigenId = movement.Id,
            TipoComprobanteId = tipoComprobante.Id,
            CreadoPor = movement.CreadoPor ?? Guid.Empty,
            CreadoEn = DateTimeOffset.Now,
            Estado = "CONTABILIZADO"
        };

        foreach (var det in movement.MovimientoInventarioDetalles)
        {
            var prod = await _context.Productos.FindAsync(det.ProductoId);
            if (prod == null) continue;

            var monto = det.Cantidad * (det.CostoUnitario ?? 0);
            if (monto <= 0) continue;

            // Cuentas del Producto (RF-35)
            var invAccId = prod.CuentaInventarioId;
            if (invAccId == null)
            {
                // Fallback a cuenta genérica de inventario (mantiene el comportamiento de compras).
                var ctaInv = await _paramService.ObtenerValorVigenteAsync(movement.EntidadId, "CTA_INV_GENERICA") ?? "183.0010";
                var genAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaInv && c.EntidadId == movement.EntidadId);
                invAccId = genAcc?.Id;
            }

            var expenseAccId = prod.CuentaCostoVentaId;
            if (invAccId == null) continue;

            // Contrapartida contable según el tipo de movimiento:
            // - RECEPCION / DEVOLUCION_SALIDA -> acredita CxP proveedores (no ingresos).
            // - DEVOLUCION_ENTRADA / VALE_ENTREGA / VENTA_POS -> costo de venta del producto.
            // - AJUSTE_POSITIVO -> ingreso por ajuste; AJUSTE_NEGATIVO -> pérdida por ajuste.
            // - CONSUMO/DESVIACION_PRODUCCION -> debita WIP; ENTRADA_PRODUCCION/DEVOLUCION_PRODUCCION -> acredita WIP.
            Guid? contraAccId = null;

            switch (tipo.Codigo)
            {
                case "RECEPCION":
                case "DEVOLUCION_SALIDA":
                    var ctaCpp = await _paramService.ObtenerValorVigenteAsync(movement.EntidadId, "CTA_CXP_PROVEEDORES") ?? "405.0020";
                    var cppAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaCpp && c.EntidadId == movement.EntidadId);
                    contraAccId = cppAcc?.Id;
                    break;
                case "AJUSTE_POSITIVO":
                    var ctaAjuste = await _paramService.ObtenerValorVigenteAsync(movement.EntidadId, "CTA_INGRESO_AJUSTE_INV") ?? "930.0010";
                    var ajusteAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaAjuste && c.EntidadId == movement.EntidadId);
                    contraAccId = ajusteAcc?.Id;
                    break;
                case "AJUSTE_NEGATIVO":
                    var ctaPerdida = await _paramService.ObtenerValorVigenteAsync(movement.EntidadId, "CTA_PERDIDA_INVENTARIO") ?? "850.0010";
                    var perdidaAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaPerdida && c.EntidadId == movement.EntidadId);
                    contraAccId = perdidaAcc?.Id;
                    break;
                case "CONSUMO_PRODUCCION":
                case "ENTRADA_PRODUCCION":
                case "DEVOLUCION_PRODUCCION":
                    var ctaWip = await _paramService.ObtenerValorVigenteAsync(movement.EntidadId, "CTA_WIP") ?? "185.0010";
                    var wipAcc = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == ctaWip && c.EntidadId == movement.EntidadId);
                    contraAccId = wipAcc?.Id;
                    break;
                default:
                    contraAccId = tipo.Naturaleza == "ENTRADA" ? null : expenseAccId;
                    break;
            }

            if (contraAccId == null) continue;

            if (tipo.Naturaleza == "ENTRADA")
            {
                // DEBE: Inventario | HABER: Contrapartida
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = invAccId.Value, Debe = monto, Haber = 0, Glosa = $"Alta Inventario {prod.Nombre}" });
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = contraAccId.Value, Debe = 0, Haber = monto, Glosa = $"Contrapartida {tipo.Nombre}" });
            }
            else
            {
                // DEBE: Contrapartida | HABER: Inventario
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = contraAccId.Value, Debe = monto, Haber = 0, Glosa = $"Costo/Gasto {prod.Nombre}" });
                entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = invAccId.Value, Debe = 0, Haber = monto, Glosa = $"Baja Inventario {prod.Nombre}" });
            }
        }

        if (entry.AsientoDetalles.Any() && entry.AsientoDetalles.Sum(d => d.Debe) == entry.AsientoDetalles.Sum(d => d.Haber))
        {
            var res = await _accountingService.CreateEntryAsync(entry);
            if (res.Succeeded)
            {
                movement.AsientoId = entry.Id;
            }
        }
    }

    public async Task<decimal> GetStockAsync(Guid almacenId, Guid productoId)
    {
        return await _context.Existencia
            .Where(e => e.AlmacenId == almacenId && e.ProductoId == productoId)
            .Select(e => e.Cantidad)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Existencium>> GetLowStockAlertsAsync(Guid entidadId)
    {
        return await _context.Existencia
            .Include(e => e.Producto)
            .Include(e => e.Almacen)
            .Where(e => e.Almacen.EntidadId == entidadId && e.Cantidad <= e.StockMinimo)
            .ToListAsync();
    }

    public async Task<List<ExistenciaLote>> GetExpiryAlertsAsync(Guid entidadId, int daysThreshold)
    {
        var limit = DateOnly.FromDateTime(DateTime.Now.AddDays(daysThreshold));
        return await _context.ExistenciaLotes
            .Include(l => l.Producto)
            .Include(l => l.Almacen)
            .Where(l => l.Almacen.EntidadId == entidadId && l.Cantidad > 0 && l.FechaVencimiento <= limit)
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }

    public async Task<List<KardexRowViewModel>> GetKardexByProductAsync(Guid productoId, Guid? almacenId = null)
    {
        var query = _context.MovimientoInventarioDetalles
            .Include(d => d.Movimiento).ThenInclude(m => m.TipoMovimiento)
            .Include(d => d.Movimiento).ThenInclude(m => m.AlmacenOrigen)
            .Include(d => d.Movimiento).ThenInclude(m => m.AlmacenDestino)
            .Where(d => d.ProductoId == productoId);

        if (almacenId.HasValue)
        {
            query = query.Where(d => d.Movimiento.AlmacenOrigenId == almacenId || d.Movimiento.AlmacenDestinoId == almacenId);
        }

        var details = await query
            .OrderBy(d => d.Movimiento.Fecha)
            .ToListAsync();

        var kardex = new List<KardexRowViewModel>();
        decimal saldoAcumulado = 0;

        foreach (var d in details)
        {
            decimal entrada = 0;
            decimal salida = 0;
            string almacenNombre = "";

            if (d.Movimiento.AlmacenDestinoId == (almacenId ?? d.Movimiento.AlmacenDestinoId))
            {
                entrada = d.Cantidad;
                almacenNombre = d.Movimiento.AlmacenDestino?.Nombre ?? "N/A";
            }

            if (d.Movimiento.AlmacenOrigenId == (almacenId ?? d.Movimiento.AlmacenOrigenId))
            {
                salida = d.Cantidad;
                almacenNombre = d.Movimiento.AlmacenOrigen?.Nombre ?? "N/A";
            }

            // Si es una transferencia interna y no estamos filtrando por un almacén específico,
            // el registro se duplicaría conceptualmente, pero aquí tratamos el flujo neto.
            // Para un Kardex global, las transferencias no cambian el saldo total.

            saldoAcumulado += (entrada - salida);

            kardex.Add(new KardexRowViewModel
            {
                Fecha = d.Movimiento.Fecha,
                TipoMovimiento = d.Movimiento.TipoMovimiento.Nombre,
                Documento = d.Movimiento.NumeroDocumento,
                Almacen = almacenNombre,
                Lote = d.Lote,
                Entrada = entrada,
                Salida = salida,
                Saldo = saldoAcumulado,
                CostoUnitario = d.CostoUnitario ?? 0,
                ValorSaldo = saldoAcumulado * (d.CostoUnitario ?? 0) // Simplificación, debería usar PPP histórico
            });
        }

        return kardex.OrderByDescending(k => k.Fecha).ToList();
    }

    public async Task<bool> HasMovimientosAsync(Guid id)
    {
        return await _context.MovimientoInventarioDetalles
            .AnyAsync(d => d.ProductoId == id);
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var producto = await GetProductByIdAsync(id);
            if (producto == null)
                return false;

            // Verificar si tiene movimientos de inventario
            if (await HasMovimientosAsync(id))
            {

                return false;
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {

            return false;
        }
    }

    public async Task<DeleteResult> DeleteSelectedAsync(List<Guid> ids)
    {
        var result = new DeleteResult();

        try
        {
            if (ids == null || !ids.Any())
            {
                result.Success = false;
                result.Message = "No se seleccionó ningún producto.";
                return result;
            }

            var productos = await _context.Productos
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            if (!productos.Any())
            {
                result.Success = false;
                result.Message = "No se encontraron productos para eliminar.";
                return result;
            }

            // Verificar movimientos asociados
            var idsConMovimientos = new List<Guid>();
            foreach (var p in productos)
            {
                if (await HasMovimientosAsync(p.Id))
                    idsConMovimientos.Add(p.Id);
            }

            if (idsConMovimientos.Any())
            {
                result.Success = false;
                result.Message = $"Los siguientes productos tienen movimientos y no pueden eliminarse: {string.Join(", ", idsConMovimientos)}";
                result.FailedIds = idsConMovimientos;
                return result;
            }

            _context.Productos.RemoveRange(productos);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.DeletedCount = productos.Count;
            result.Message = $"{productos.Count} producto(s) eliminado(s) correctamente.";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "Ocurrió un error al eliminar los productos.";
        }

        return result;
    }

}

// Extensión para normalizar cantidad (evitar problemas de tipos)
public static class DetailExtensions
{
    public static decimal QuantityNormalized(this MovimientoInventarioDetalle d) => d.Cantidad;
}

public class DeleteResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DeletedCount { get; set; }
    public List<Guid> FailedIds { get; set; } = new();
}
