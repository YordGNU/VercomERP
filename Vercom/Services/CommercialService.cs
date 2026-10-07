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
    Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract, IFormFile? document, bool quitarDocumento = false);

    Task<IReadOnlyList<ContratoEconomicoSuplemento>> GetContractSuplementosAsync(Guid contratoId);
    Task<ContractSuplementoFormViewModel> GetContractSuplementoFormContextAsync(Guid contratoId);
    Task<(bool Succeeded, string Message)> CreateContractSuplementoAsync(ContratoEconomicoSuplemento suplemento, IFormFile? document);
    Task<(bool Succeeded, string Message)> AnularContractSuplementoAsync(Guid suplementoId, string? motivo);

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

        var result = await query.OrderByDescending(c => c.FechaFirma).ToListAsync();
        await RecalcularVencimientosAsync(result); // Estado es del servidor: marcar VENCIDO al expirar
        return result;
    }

    public async Task<ContratoEconomico?> GetContractByIdAsync(Guid id)
    {
        var row = await _context.ContratoEconomicos
            .Include(c => c.Cliente)
            .Include(c => c.Proveedor)
            .Include(c => c.Suplementos)
            .FirstOrDefaultAsync(m => m.Id == id && m.EntidadId == _entidadProvider.CurrentEntidadId);
        await RecalcularVencimientosAsync(new[] { row });
        return row;
    }

    /// <summary>
    /// Marca como VENCIDO todo contrato VIGENTE cuya FechaFin ya pasó (una sola escritura al final).
    /// No revierte otros estados (RESCINDIDO se conserva).
    /// </summary>
    private async Task RecalcularVencimientosAsync(IEnumerable<ContratoEconomico?>? rows)
    {
        if (rows == null) return;
        try
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var candidatos = rows.Where(c => c != null).Select(c => c!).ToList();
            if (candidatos.Count == 0) return;

            var ids = candidatos.Select(c => c.Id).ToList();
            var finsEfectivos = await _context.ContratoEconomicoSuplementos
                .AsNoTracking()
                .Where(s => ids.Contains(s.ContratoId) && s.Estado == "VIGENTE" && s.FechaFin != null)
                .GroupBy(s => s.ContratoId)
                .Select(g => new { ContratoId = g.Key, FechaFin = g.Max(s => s.FechaFin) })
                .ToListAsync();
            var mapaFines = finsEfectivos.ToDictionary(x => x.ContratoId, x => x.FechaFin);

            var vencidos = candidatos
                .Where(c => c.Estado == "VIGENTE")
                .Select(c => new
                {
                    Contrato = c,
                    FechaFinEfectiva = mapaFines.TryGetValue(c.Id, out var f) ? f : c.FechaFin
                })
                .Where(x => x.FechaFinEfectiva.HasValue && x.FechaFinEfectiva.Value < hoy)
                .ToList();

            if (vencidos.Count == 0) return;

            foreach (var x in vencidos) x.Contrato.Estado = "VENCIDO";
            await _context.SaveChangesAsync();
            _logger.LogInformation("Se marcaron {Count} contratos como VENCIDO por expiración de vigencia efectiva.", vencidos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("No se pudieron recalcular los contratos vencidos: {Msg}", ex.Message);
        }
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
            contract.FechaFinOriginal = contract.FechaFin;
            contract.MontoTotalOriginal = contract.MontoTotal;

            var numeroContrato = contract.NumeroContrato?.Trim();
            if (await ExisteNumeroContratoParaElTerceroAsync(contract.EntidadId, numeroContrato, contract.ClienteId, contract.ProveedorId))
                return (false, MensajeNumeroContratoRepetido(numeroContrato, contract.ClienteId, contract.ProveedorId));

            contract.NumeroContrato = numeroContrato ?? contract.NumeroContrato;

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

    public async Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract, IFormFile? document, bool quitarDocumento = false)
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

            var existing = await _context.ContratoEconomicos
                .FirstOrDefaultAsync(c => c.Id == contract.Id && c.EntidadId == _entidadProvider.CurrentEntidadId);
            if (existing == null) return (false, "El contrato no existe.");

            var numeroContrato = contract.NumeroContrato?.Trim();
            if (await ExisteNumeroContratoParaElTerceroAsync(_entidadProvider.CurrentEntidadId, numeroContrato, contract.ClienteId, contract.ProveedorId, contract.Id))
                return (false, MensajeNumeroContratoRepetido(numeroContrato, contract.ClienteId, contract.ProveedorId));

            contract.NumeroContrato = numeroContrato ?? contract.NumeroContrato;

            var tieneSuplementos = await _context.ContratoEconomicoSuplementos
                .AsNoTracking()
                .AnyAsync(s => s.ContratoId == contract.Id && s.Estado == "VIGENTE");
            if (tieneSuplementos)
            {
                return (false, "El contrato tiene suplementos vigentes. La vigencia se extiende mediante un suplemento, no editando el contrato.");
            }

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
            var prevEstado = existing.Estado;
            var documentoAnterior = existing.DocumentoUrl; // para borrar del disco solo tras commit exitoso
            var fechaFinOriginalPrevia = existing.FechaFinOriginal;
            var montoTotalOriginalPrevia = existing.MontoTotalOriginal;
            _context.Entry(existing).CurrentValues.SetValues(contract);
            existing.EntidadId = _entidadProvider.CurrentEntidadId; // Preservar multi-inquilino
            existing.FechaFinOriginal = fechaFinOriginalPrevia; // Respaldo inmutable de la vigencia pactada
            existing.MontoTotalOriginal = montoTotalOriginalPrevia; // Respaldo inmutable del monto pactado

            // El Estado es responsabilidad del servidor: el formulario nunca lo modifica.
            existing.Estado = RecalcularEstadoContrato(prevEstado, contract.FechaFin);

            // Gestión del documento: reemplazar si se adjuntó uno nuevo; quitar si lo pide el usuario.
            if (uploadedRelativePath != null)
            {
                existing.DocumentoUrl = uploadedRelativePath;
            }
            else if (quitarDocumento)
            {
                existing.DocumentoUrl = null;
            }

            await _context.SaveChangesAsync();

            // Tras el commit exitoso, eliminar del disco el documento anterior (solo si ya no se usa).
            if (!string.IsNullOrEmpty(documentoAnterior) && existing.DocumentoUrl != documentoAnterior)
            {
                var del = await _fileStorage.DeleteFileAsync(documentoAnterior);
                if (!del.Success)
                    _logger.LogWarning("No se pudo eliminar el documento anterior ({Url}) de un contrato: {Msg}",
                        documentoAnterior, del.Message);
            }

            return (true, "Contrato económico actualizado.");
        }
        catch (Exception ex)
        {
            if (uploadedRelativePath != null)
                await _fileStorage.DeleteFileAsync(uploadedRelativePath);
            return (false, ex.Message);
        }
    }

    private static string RecalcularEstadoContrato(string? prevEstado, DateOnly? fechaFin)
    {
        if (fechaFin.HasValue && fechaFin.Value < DateOnly.FromDateTime(DateTime.Now))
            return "VENCIDO";

        return string.IsNullOrEmpty(prevEstado) || prevEstado == "VENCIDO" ? "VIGENTE" : prevEstado;
    }

    private async Task<bool> ExisteNumeroContratoParaElTerceroAsync(Guid entidadId, string? numeroContrato, Guid? clienteId, Guid? proveedorId, Guid? excluirId = null)
    {
        var numero = numeroContrato?.Trim();
        if (string.IsNullOrEmpty(numero)) return false;

        var query = _context.ContratoEconomicos
            .Where(c => c.EntidadId == entidadId && c.NumeroContrato == numero);

        query = clienteId.HasValue
            ? query.Where(c => c.ClienteId == clienteId)
            : query.Where(c => c.ClienteId == null);

        query = proveedorId.HasValue
            ? query.Where(c => c.ProveedorId == proveedorId)
            : query.Where(c => c.ProveedorId == null);

        if (excluirId.HasValue) query = query.Where(c => c.Id != excluirId.Value);

        return await query.AnyAsync();
    }

    private static string MensajeNumeroContratoRepetido(string? numeroContrato, Guid? clienteId, Guid? proveedorId)
    {
        var tercero = proveedorId.HasValue ? "proveedor" : "cliente";
        return $"Ya existe un contrato con el número {numeroContrato} para ese {tercero} en esta entidad.";
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

    public async Task<IReadOnlyList<ContratoEconomicoSuplemento>> GetContractSuplementosAsync(Guid contratoId)
    {
        return await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId)
            .OrderByDescending(s => s.NumeroSuplemento)
            .ToListAsync();
    }

    public async Task<ContractSuplementoFormViewModel> GetContractSuplementoFormContextAsync(Guid contratoId)
    {
        var contrato = await _context.ContratoEconomicos
            .AsNoTracking()
            .Where(c => c.Id == contratoId && c.EntidadId == _entidadProvider.CurrentEntidadId)
            .Select(c => new { c.Id, c.NumeroContrato, c.TerceroTipo, c.Estado, c.FechaInicio, c.FechaFin, c.FechaFinOriginal, c.MontoTotal, c.MontoTotalOriginal })
            .FirstOrDefaultAsync();

        if (contrato == null) return new ContractSuplementoFormViewModel();

        var ultimoVigente = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId && s.Estado == "VIGENTE" && s.FechaFin != null)
            .OrderByDescending(s => s.NumeroSuplemento)
            .Select(s => new { s.NumeroSuplemento, s.FechaFin })
            .FirstOrDefaultAsync();

        var suplementoMonto = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId && s.Estado == "VIGENTE" && s.MontoTotalNuevo != null)
            .OrderByDescending(s => s.NumeroSuplemento)
            .Select(s => new { s.NumeroSuplemento, s.MontoTotalNuevo })
            .FirstOrDefaultAsync();

        var finEfectivo = ultimoVigente?.FechaFin ?? contrato.FechaFin;
        var montoEfectivo = suplementoMonto?.MontoTotalNuevo ?? contrato.MontoTotal;

        var maximoNumero = await _context.ContratoEconomicoSuplementos
            .AsNoTracking()
            .Where(s => s.ContratoId == contratoId)
            .Select(s => (int?)s.NumeroSuplemento)
            .MaxAsync() ?? 0;
        var numeroSugerido = maximoNumero + 1;

        return new ContractSuplementoFormViewModel
        {
            ContratoId = contrato.Id,
            NumeroContrato = contrato.NumeroContrato,
            TerceroTipo = contrato.TerceroTipo,
            EstadoContrato = contrato.Estado,
            FechaFinPactada = contrato.FechaFinOriginal ?? contrato.FechaFin,
            FechaFinEfectiva = finEfectivo,
            MontoTotalPactada = contrato.MontoTotalOriginal ?? contrato.MontoTotal,
            MontoTotalEfectiva = montoEfectivo,
            NumeroSuplementoSugerido = numeroSugerido,
            Suplemento = new ContratoEconomicoSuplemento
            {
                ContratoId = contrato.Id,
                NumeroSuplemento = numeroSugerido,
                FechaFirma = DateOnly.FromDateTime(DateTime.Now),
                Estado = "VIGENTE"
            }
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateContractSuplementoAsync(ContratoEconomicoSuplemento suplemento, IFormFile? document)
    {
        string? uploadedRelativePath = null;
        try
        {
            var entidadId = _entidadProvider.CurrentEntidadId;

            var contrato = await _context.ContratoEconomicos
                .FirstOrDefaultAsync(c => c.Id == suplemento.ContratoId && c.EntidadId == entidadId);
            if (contrato == null) return (false, "El contrato no existe o no pertenece a la entidad.");

            if (contrato.Estado == "RESCINDIDO")
                return (false, "No se puede modificar un contrato rescindido.");

            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var ultimoVigente = await _context.ContratoEconomicoSuplementos
                .AsNoTracking()
                .Where(s => s.ContratoId == contrato.Id && s.Estado == "VIGENTE" && s.FechaFin != null)
                .OrderByDescending(s => s.NumeroSuplemento)
                .Select(s => new { s.NumeroSuplemento, s.FechaFin })
                .FirstOrDefaultAsync();

            var finEfectivoActual = ultimoVigente?.FechaFin ?? contrato.FechaFin;

            var fijaVigencia = suplemento.FechaInicio.HasValue || suplemento.FechaFin.HasValue;
            var fijaMonto = suplemento.MontoTotalNuevo.HasValue;

            if (!fijaVigencia && !fijaMonto)
                return (false, "El suplemento no modifica ningún término del contrato.");

            if (fijaVigencia)
            {
                if (suplemento.FechaInicio == null || suplemento.FechaFin == null)
                    return (false, "Para modificar la vigencia indique fecha de inicio y fecha de fin.");

                if (suplemento.FechaFin.Value <= suplemento.FechaInicio.Value)
                    return (false, "La fecha de fin del suplemento debe ser posterior a su fecha de inicio.");

                if (finEfectivoActual.HasValue && suplemento.FechaFin.Value <= finEfectivoActual.Value)
                    return (false, $"La nueva fecha de fin ({suplemento.FechaFin.Value:dd/MM/yyyy}) debe ser posterior a la vigencia actual ({finEfectivoActual.Value:dd/MM/yyyy}).");
            }

            if (suplemento.MontoTotalNuevo.HasValue && suplemento.MontoTotalNuevo.Value < 0)
                return (false, "El monto pactado no puede ser negativo.");

            if (suplemento.FechaFirma < contrato.FechaFirma)
                return (false, "El suplemento no puede firmarse antes de la fecha de firma del contrato.");

            if (suplemento.FechaFirma < hoy.AddDays(-365))
                return (false, "No se admite un suplemento con más de un año de antigüedad. Regularice el contrato.");

            if (string.IsNullOrWhiteSpace(suplemento.Concepto))
                return (false, "Describa el concepto del suplemento.");

            var contraparteActiva = contrato.TerceroTipo switch
            {
                "CLIENTE" => contrato.ClienteId.HasValue && await _context.Clientes.AnyAsync(c => c.Id == contrato.ClienteId.Value),
                "PROVEEDOR" => contrato.ProveedorId.HasValue && await _context.Proveedors.AnyAsync(p => p.Id == contrato.ProveedorId.Value),
                _ => false
            };
            if (!contraparteActiva)
                return (false, "La contraparte del contrato no está registrada o está inactiva.");

            var siguienteNumero = await _context.ContratoEconomicoSuplementos
                .Where(s => s.ContratoId == contrato.Id)
                .Select(s => (int?)s.NumeroSuplemento)
                .MaxAsync() ?? 0;
            siguienteNumero += 1;

            suplemento.Id = Guid.NewGuid();
            suplemento.EntidadId = entidadId;
            suplemento.NumeroSuplemento = siguienteNumero;
            suplemento.Estado = "VIGENTE";
            suplemento.CreadoEn = DateTimeOffset.Now;

            if (document != null && document.Length > 0)
            {
                var uploadResult = await _fileStorage.SaveFileAsync(
                    file: document,
                    subFolder: "contracts",
                    prefix: $"sup_{contrato.NumeroContrato}_{siguienteNumero}");

                if (!uploadResult.Success) return (false, uploadResult.Message);

                suplemento.DocumentoUrl = uploadResult.RelativePath;
                uploadedRelativePath = uploadResult.RelativePath;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.ContratoEconomicoSuplementos.Add(suplemento);

            var cambios = new List<string>();

            if (suplemento.FechaFin.HasValue)
            {
                contrato.FechaFin = suplemento.FechaFin;
                if (contrato.Estado == "VENCIDO" && suplemento.FechaFin.Value >= hoy)
                    contrato.Estado = "VIGENTE";
                cambios.Add($"vigencia hasta el {suplemento.FechaFin.Value:dd/MM/yyyy}");
            }

            if (suplemento.MontoTotalNuevo.HasValue)
            {
                contrato.MontoTotal = suplemento.MontoTotalNuevo;
                cambios.Add($"monto {suplemento.MontoTotalNuevo.Value:C2}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Suplemento {Num} registrado para contrato {Contrato}. {Cambios}.",
                siguienteNumero, contrato.NumeroContrato, string.Join("; ", cambios));

            return (true, $"Suplemento N.º {siguienteNumero} registrado: {string.Join(" y ", cambios)}.");
        }
        catch (Exception ex)
        {
            if (uploadedRelativePath != null) await _fileStorage.DeleteFileAsync(uploadedRelativePath);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string Message)> AnularContractSuplementoAsync(Guid suplementoId, string? motivo)
    {
        try
        {
            var entidadId = _entidadProvider.CurrentEntidadId;

            var suplemento = await _context.ContratoEconomicoSuplementos
                .FirstOrDefaultAsync(s => s.Id == suplementoId && s.EntidadId == entidadId);
            if (suplemento == null) return (false, "El suplemento no existe.");
            if (suplemento.Estado == "ANULADO") return (false, "El suplemento ya está anulado.");

            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Indique el motivo de la anulación.");

            var contrato = await _context.ContratoEconomicos
                .FirstOrDefaultAsync(c => c.Id == suplemento.ContratoId && c.EntidadId == entidadId);
            if (contrato == null) return (false, "El contrato no existe.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            suplemento.Estado = "ANULADO";
            suplemento.MotivoAnulacion = motivo.Trim();

            var vigentes = _context.ContratoEconomicoSuplementos
                .AsNoTracking()
                .Where(s => s.ContratoId == contrato.Id && s.Estado == "VIGENTE" && s.Id != suplemento.Id);

            var siguienteVigencia = await vigentes
                .Where(s => s.FechaFin != null)
                .OrderByDescending(s => s.NumeroSuplemento)
                .Select(s => (DateOnly?)s.FechaFin)
                .FirstOrDefaultAsync();

            var siguienteMonto = await vigentes
                .Where(s => s.MontoTotalNuevo != null)
                .OrderByDescending(s => s.NumeroSuplemento)
                .Select(s => (decimal?)s.MontoTotalNuevo)
                .FirstOrDefaultAsync();

            contrato.FechaFin = siguienteVigencia ?? contrato.FechaFinOriginal ?? contrato.FechaFin;
            contrato.MontoTotal = siguienteMonto ?? contrato.MontoTotalOriginal ?? contrato.MontoTotal;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Suplemento {Num} del contrato {Contrato} anulado. Vigencia {FechaFin}, monto {Monto}.",
                suplemento.NumeroSuplemento, contrato.NumeroContrato, contrato.FechaFin, contrato.MontoTotal);

            return (true, $"Suplemento N.º {suplemento.NumeroSuplemento} anulado.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
