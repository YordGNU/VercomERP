using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface ICommercialService
{
    // Clientes
    Task<IEnumerable<Cliente>> GetClientsAsync(string? search = null, string? segment = null, bool? active = null);
    Task<ClientListViewModel> GetClientIndexAsync(string? search = null, string? segment = null, bool? activo = null, int page = 1, int pageSize = 12);
    Task<Cliente?> GetClientByIdAsync(Guid id);
    Task<ClientFormViewModel> GetClientFormContextAsync(Cliente? existing = null);
    Task<(bool Succeeded, string Message)> CreateClientAsync(Cliente client);
    Task<(bool Succeeded, string Message)> UpdateClientAsync(Cliente client);
    Task<(bool Succeeded, string Message)> ValidateCreditLimitAsync(Guid clienteId, decimal additionalAmount);

    // Proveedores
    Task<IEnumerable<Proveedor>> GetProvidersAsync(string? search = null, bool? active = null);
    Task<ProviderListViewModel> GetProviderIndexAsync(string? search = null, string? tipoPersona = null, bool? activo = null, int page = 1, int pageSize = 12);
    Task<Proveedor?> GetProviderByIdAsync(Guid id);
    Task<ProviderFormViewModel> GetProviderFormContextAsync(Proveedor? existing = null);
    Task<(bool Succeeded, string Message)> CreateProviderAsync(Proveedor provider);
    Task<(bool Succeeded, string Message)> UpdateProviderAsync(Proveedor provider);

    // Contratos Económicos (RF-50)
    Task<IEnumerable<ContratoEconomico>> GetContractsAsync(string? search = null, string? type = null, string? status = null);
    Task<ContratoEconomico?> GetContractByIdAsync(Guid id);
    Task<EconomicContractViewModel> GetContractFormContextAsync(ContratoEconomico? existing = null);
    Task<(bool Succeeded, string Message)> CreateContractAsync(ContratoEconomico contract, IFormFile? document);
    Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract, IFormFile? document);

    // Topes de Precio MFP
    Task<IEnumerable<TopePrecioMfp>> GetPriceLimitsAsync(string? search = null);
    Task<PriceLimitFormViewModel> GetPriceLimitFormContextAsync(TopePrecioMfp? existing = null);
    Task<(bool Succeeded, string Message)> CreatePriceLimitAsync(TopePrecioMfp limit);

    // Devoluciones
    Task<IEnumerable<DevolucionVentum>> GetReturnsAsync();
    Task<SalesReturnFormViewModel> GetReturnFormContextAsync(Guid? invoiceId = null);
    Task<(bool Succeeded, string Message)> CreateReturnAsync(DevolucionVentum salesReturn);
}

public class CommercialService : ICommercialService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ICommercialService> _logger;

    public CommercialService(AppDbContext context, Security.IEntidadProvider entidadProvider, IFileStorageService fileStorage, ILogger<ICommercialService> logger)
    {
        _context = context;
        _entidadProvider = entidadProvider;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<IEnumerable<Cliente>> GetClientsAsync(string? search = null, string? segment = null, bool? active = null)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.NombreRazonSocial.Contains(search) || c.NitOCi.Contains(search));

        if (!string.IsNullOrEmpty(segment))
            query = query.Where(c => c.Segmento == segment);

        if (active.HasValue)
            query = query.Where(c => c.Activo == active.Value);

        return await query.OrderBy(c => c.NombreRazonSocial).ToListAsync();
    }

    public async Task<ClientListViewModel> GetClientIndexAsync(string? search = null, string? segment = null, bool? activo = null, int page = 1, int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 6, 48);

        var query = _context.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.NombreRazonSocial.Contains(term) || c.NitOCi.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(segment))
        {
            var seg = segment.Trim();
            query = query.Where(c => c.Segmento == seg);
        }

        if (activo.HasValue)
        {
            var a = activo.Value;
            query = query.Where(c => c.Activo == a);
        }

        var totalItems = await query.CountAsync();
        var items = await query.OrderBy(c => c.NombreRazonSocial)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ClientListViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Search = search,
            Segmento = segment,
            Activo = activo,
            Total = await _context.Clientes.CountAsync(),
            Activos = await _context.Clientes.CountAsync(c => c.Activo),
            Minoristas = await _context.Clientes.CountAsync(c => c.Segmento == "MINORISTA"),
            Mayoristas = await _context.Clientes.CountAsync(c => c.Segmento == "MAYORISTA")
        };
    }

    public async Task<Cliente?> GetClientByIdAsync(Guid id)
    {
        return await _context.Clientes
            .Include(c => c.ContratoEconomicos)
            .Include(c => c.FacturaVenta).ThenInclude(f => f.FacturaVentaDetalles)
            .Include(c => c.CuentaPorCobrars)
            .Include(c => c.CuentaContable)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ClientFormViewModel> GetClientFormContextAsync(Cliente? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentasContables = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento && c.Clase == "ACTIVO" && c.Codigo.StartsWith("135"))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        return new ClientFormViewModel
        {
            Client = existing ?? new Cliente { Activo = true, TipoPersona = "JURIDICA", Segmento = "MINORISTA" },
            ListasPrecio = new SelectList(await _context.ListaPrecios.ToListAsync(), "Id", "Nombre"),
            CuentasContables = new SelectList(cuentasContables, "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateClientAsync(Cliente client)
    {
        try
        {
            client.Id = Guid.NewGuid();
            client.EntidadId = _entidadProvider.CurrentEntidadId;
            client.CreadoEn = DateTimeOffset.Now;
            client.Activo = true;
            _context.Clientes.Add(client);
            await _context.SaveChangesAsync();
            return (true, "Cliente registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateClientAsync(Cliente client)
    {
        try
        {
            var existing = await _context.Clientes.FindAsync(client.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(client);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Cliente actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> ValidateCreditLimitAsync(Guid clienteId, decimal additionalAmount)
    {
        var client = await _context.Clientes
            .Select(c => new
            {
                c.Id,
                c.LimiteCredito,
                SaldoActual = _context.CuentaPorCobrars
                    .Where(cx => cx.ClienteId == c.Id && (cx.Estado == "PENDIENTE" || cx.Estado == "PARCIAL"))
                    .Sum(cx => (decimal?)cx.SaldoPendiente) ?? 0
            })
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        if (client == null) return (false, "Cliente no encontrado.");

        // Si el límite es 0, asumimos que no tiene crédito permitido (o es ilimitado según política, pero aquí bloqueamos)
        if (client.LimiteCredito > 0 && (client.SaldoActual + additionalAmount) > client.LimiteCredito)
        {
            return (false, $"Límite de crédito excedido. Disponible: {client.LimiteCredito - client.SaldoActual:C2}. Solicitado: {additionalAmount:C2}");
        }

        return (true, "Crédito disponible.");
    }

    public async Task<IEnumerable<Proveedor>> GetProvidersAsync(string? search = null, bool? active = null)
    {
        var query = _context.Proveedors.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.RazonSocial.Contains(search) || p.Nit.Contains(search));

        if (active.HasValue)
            query = query.Where(p => p.Activo == active.Value);

        return await query.OrderBy(p => p.RazonSocial).ToListAsync();
    }

    public async Task<ProviderListViewModel> GetProviderIndexAsync(string? search = null, string? tipoPersona = null, bool? activo = null, int page = 1, int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 6, 48);

        var query = _context.Proveedors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.RazonSocial.Contains(term) || p.Nit.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(tipoPersona))
        {
            var tp = tipoPersona.Trim();
            query = query.Where(p => p.TipoPersona == tp);
        }

        if (activo.HasValue)
        {
            var a = activo.Value;
            query = query.Where(p => p.Activo == a);
        }

        var totalItems = await query.CountAsync();
        var items = await query.OrderBy(p => p.RazonSocial)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _context.Proveedors.CountAsync();
        var juridicas = await _context.Proveedors.CountAsync(p => p.TipoPersona == "JURIDICA");
        var naturales = await _context.Proveedors.CountAsync(p => p.TipoPersona == "NATURAL");

        return new ProviderListViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Search = search,
            TipoPersona = tipoPersona,
            Activo = activo,
            Total = total,
            Activos = await _context.Proveedors.CountAsync(p => p.Activo),
            Juridicas = juridicas,
            Naturales = naturales
        };
    }

    public async Task<Proveedor?> GetProviderByIdAsync(Guid id)
    {
        return await _context.Proveedors
            .Include(p => p.ContratoEconomicos)
            .Include(p => p.OrdenCompras)
            .Include(p => p.CuentaPorPagars)
            .Include(p => p.CuentaContable)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ProviderFormViewModel> GetProviderFormContextAsync(Proveedor? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var cuentasContables = await _context.CuentaContables
            .Where(c => c.EntidadId == entidadId && c.Activo && c.AceptaMovimiento && c.Clase == "PASIVO" && c.Codigo.StartsWith("470"))
            .OrderBy(c => c.Codigo)
            .Select(c => new { c.Id, Display = $"{c.Codigo} - {c.Nombre}" })
            .ToListAsync();

        return new ProviderFormViewModel
        {
            Provider = existing ?? new Proveedor { Activo = true, TipoPersona = "JURIDICA" },
            CuentasContables = new SelectList(cuentasContables, "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateProviderAsync(Proveedor provider)
    {
        try
        {
            provider.Id = Guid.NewGuid();
            provider.EntidadId = _entidadProvider.CurrentEntidadId;
            provider.CreadoEn = DateTimeOffset.Now;
            _context.Proveedors.Add(provider);
            await _context.SaveChangesAsync();
            return (true, "Proveedor registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateProviderAsync(Proveedor provider)
    {
        try
        {
            var existing = await _context.Proveedors.FindAsync(provider.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(provider);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Proveedor actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<ContratoEconomico>> GetContractsAsync(string? search = null, string? type = null, string? status = null)
    {
        var query = _context.ContratoEconomicos
            .Include(c => c.Cliente)
            .Include(c => c.Proveedor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.NumeroContrato.Contains(search) || (c.Cliente != null && c.Cliente.NombreRazonSocial.Contains(search)) || (c.Proveedor != null && c.Proveedor.RazonSocial.Contains(search)));

        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.TerceroTipo == type);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Estado == status);

        return await query.OrderByDescending(c => c.FechaFirma).ToListAsync();
    }

    public async Task<ContratoEconomico?> GetContractByIdAsync(Guid id)
    {
        return await _context.ContratoEconomicos.Include(c => c.Cliente).Include(c => c.Proveedor).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<EconomicContractViewModel> GetContractFormContextAsync(ContratoEconomico? existing = null)
    {
        return new EconomicContractViewModel
        {
            Contract = existing ?? new ContratoEconomico { FechaFirma = DateOnly.FromDateTime(DateTime.Now), Estado = "VIGENTE" },
            Clientes = new SelectList(await _context.Clientes.ToListAsync(), "Id", "NombreRazonSocial"),
            Proveedores = new SelectList(await _context.Proveedors.ToListAsync(), "Id", "RazonSocial")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateContractAsync(ContratoEconomico contract, IFormFile? document)
    {
        string? uploadedRelativePath = null;
        try
        {
            // RF-50: Validación de integridad de la contraparte (Iteración 7)
            if (contract.TerceroTipo == "CLIENTE" && (contract.ClienteId == null || !await _context.Clientes.AnyAsync(x => x.Id == contract.ClienteId)))
                return (false, "Falta el cliente para un contrato de venta.");

            if (contract.TerceroTipo == "PROVEEDOR" && (contract.ProveedorId == null || !await _context.Proveedors.AnyAsync(x => x.Id == contract.ProveedorId)))
                return (false, "Falta el proveedor para un contrato de compra.");

            contract.Id = Guid.NewGuid();
            contract.EntidadId = _entidadProvider.CurrentEntidadId;

            // ============================================================
            // 1. GUARDAR DOCUMENTO ADJUNTO (usando el servicio)
            // ============================================================
            if (document != null && document.Length > 0)
            {
                var uploadResult = await _fileStorage.SaveFileAsync(
                    file: document,
                    subFolder: "contracts",
                    prefix: $"ce_{contract.NumeroContrato}");

                if (!uploadResult.Success)
                    return (false, uploadResult.Message);

                contract.DocumentoUrl = uploadResult.RelativePath;
                uploadedRelativePath = uploadResult.RelativePath;

                _logger.LogInformation(
                    "Documento de contrato subido: {Path} para contrato {NumeroContrato}",
                    uploadedRelativePath, contract.NumeroContrato);
            }
            _context.ContratoEconomicos.Add(contract);
            await _context.SaveChangesAsync();
            return (true, "Contrato económico registrado.");
        }
        catch (Exception ex)
        {
            if (uploadedRelativePath != null) await _fileStorage.DeleteFileAsync(uploadedRelativePath);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract, IFormFile? document)
    {
        string? uploadedRelativePath = null; // Para limpiar si falla la transacción
        try
        {
            // Ya se valida la existencia de la contraparte en el POST (RF-50),
            // aquí se confirma por seguridad.
            if (contract.TerceroTipo == "CLIENTE" && (contract.ClienteId == null || !await _context.Clientes.AnyAsync(x => x.Id == contract.ClienteId)))
                return (false, "Falta el cliente para un contrato de venta.");

            if (contract.TerceroTipo == "PROVEEDOR" && (contract.ProveedorId == null || !await _context.Proveedors.AnyAsync(x => x.Id == contract.ProveedorId)))
                return (false, "Falta el proveedor para un contrato de compra.");

            var existing = await _context.ContratoEconomicos.FindAsync(contract.Id);
            if (existing == null) return (false, "El contrato no existe.");

            // ============================================================
            // 1. GUARDAR NUEVO DOCUMENTO (si se adjuntó uno)
            // ============================================================
            if (document != null && document.Length > 0)
            {
                var uploadResult = await _fileStorage.SaveFileAsync(
                    file: document,
                    subFolder: "contracts",
                    prefix: $"ce_{contract.NumeroContrato}");

                if (!uploadResult.Success)
                    return (false, uploadResult.Message);

                uploadedRelativePath = uploadResult.RelativePath;

                _logger.LogInformation(
                    "Documento de contrato actualizado: {Path} para contrato {NumeroContrato}",
                    uploadedRelativePath, contract.NumeroContrato);
            }

            // ============================================================
            // 2. ACTUALIZAR EL CONTRATO
            // ============================================================
            _context.Entry(existing).CurrentValues.SetValues(contract);
            existing.EntidadId = _entidadProvider.CurrentEntidadId; // Preservar multi-inquilino

            // Reemplazar (o quitar) el documento en la entidad
            if (uploadedRelativePath != null)
            {
                // Borrar documento anterior si existía y es distinto
                if (!string.IsNullOrEmpty(existing.DocumentoUrl) && existing.DocumentoUrl != uploadedRelativePath)
                    await _fileStorage.DeleteFileAsync(existing.DocumentoUrl);

                existing.DocumentoUrl = uploadedRelativePath;
            }

            await _context.SaveChangesAsync();
            return (true, "Contrato económico actualizado.");
        }
        catch (Exception ex)
        {
            if (uploadedRelativePath != null)
                await _fileStorage.DeleteFileAsync(uploadedRelativePath);
            return (false, ex.Message);
        }
    }

    public async Task<IEnumerable<TopePrecioMfp>> GetPriceLimitsAsync(string? search = null)
    {
        var query = _context.TopePrecioMfps
            .Include(t => t.Producto)
            .Include(t => t.Familia)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => (t.Producto != null && t.Producto.Nombre.Contains(search)) || (t.Familia != null && t.Familia.Nombre.Contains(search)));

        return await query.OrderByDescending(t => t.VigenteDesde).ToListAsync();
    }

    public async Task<PriceLimitFormViewModel> GetPriceLimitFormContextAsync(TopePrecioMfp? existing = null)
    {
        return new PriceLimitFormViewModel
        {
            Limit = existing ?? new TopePrecioMfp { VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            Productos = new SelectList(await _context.Productos.ToListAsync(), "Id", "Nombre"),
            Familias = new SelectList(await _context.FamiliaProductos.ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreatePriceLimitAsync(TopePrecioMfp limit)
    {
        try
        {
            limit.Id = Guid.NewGuid();
            _context.TopePrecioMfps.Add(limit);
            await _context.SaveChangesAsync();
            return (true, "Tope de precio registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<DevolucionVentum>> GetReturnsAsync()
    {
        return await _context.DevolucionVenta.Include(d => d.Factura).OrderByDescending(d => d.Fecha).ToListAsync();
    }

    public async Task<SalesReturnFormViewModel> GetReturnFormContextAsync(Guid? invoiceId = null)
    {
        var invoice = invoiceId.HasValue ? await _context.FacturaVenta.FindAsync(invoiceId.Value) : null;
        return new SalesReturnFormViewModel
        {
            Return = new DevolucionVentum { FacturaId = invoiceId ?? Guid.Empty, Fecha = DateOnly.FromDateTime(DateTime.Now) },
            InvoiceNumber = invoice?.NumeroFactura ?? ""
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateReturnAsync(DevolucionVentum salesReturn)
    {
        try
        {
            salesReturn.Id = Guid.NewGuid();
            _context.DevolucionVenta.Add(salesReturn);
            await _context.SaveChangesAsync();
            return (true, "Devolución registrada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
