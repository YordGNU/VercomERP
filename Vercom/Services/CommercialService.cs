using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface ICommercialService
{
    // Clientes
    Task<IEnumerable<Cliente>> GetClientsAsync();
    Task<Cliente?> GetClientByIdAsync(Guid id);
    Task<ClientFormViewModel> GetClientFormContextAsync(Cliente? existing = null);
    Task<(bool Succeeded, string Message)> CreateClientAsync(Cliente client);
    Task<(bool Succeeded, string Message)> UpdateClientAsync(Cliente client);

    // Proveedores
    Task<IEnumerable<Proveedor>> GetProvidersAsync();
    Task<Proveedor?> GetProviderByIdAsync(Guid id);
    Task<ProviderFormViewModel> GetProviderFormContextAsync(Proveedor? existing = null);
    Task<(bool Succeeded, string Message)> CreateProviderAsync(Proveedor provider);
    Task<(bool Succeeded, string Message)> UpdateProviderAsync(Proveedor provider);

    // Contratos Económicos (RF-50)
    Task<IEnumerable<ContratoEconomico>> GetContractsAsync();
    Task<ContratoEconomico?> GetContractByIdAsync(Guid id);
    Task<EconomicContractViewModel> GetContractFormContextAsync(ContratoEconomico? existing = null);
    Task<(bool Succeeded, string Message)> CreateContractAsync(ContratoEconomico contract);
    Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract);

    // Topes de Precio MFP
    Task<IEnumerable<TopePrecioMfp>> GetPriceLimitsAsync();
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

    public CommercialService(AppDbContext context, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<Cliente>> GetClientsAsync()
    {
        return await _context.Clientes.OrderBy(c => c.NombreRazonSocial).ToListAsync();
    }

    public async Task<Cliente?> GetClientByIdAsync(Guid id)
    {
        return await _context.Clientes.Include(c => c.ContratoEconomicos).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ClientFormViewModel> GetClientFormContextAsync(Cliente? existing = null)
    {
        return new ClientFormViewModel
        {
            Client = existing ?? new Cliente { Activo = true, TipoPersona = "JURIDICA", Segmento = "MINORISTA" },
            ListasPrecio = new SelectList(await _context.ListaPrecios.ToListAsync(), "Id", "Nombre"),
            CuentasContables = new SelectList(await _context.CuentaContables.Where(c => c.Activo && c.AceptaMovimiento).OrderBy(c => c.Codigo).Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre }).ToListAsync(), "Id", "Display")
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateClientAsync(Cliente client)
    {
        try
        {
            client.Id = Guid.NewGuid();
            client.EntidadId = _entidadProvider.CurrentEntidadId;
            client.CreadoEn = DateTimeOffset.Now;
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

    public async Task<IEnumerable<Proveedor>> GetProvidersAsync()
    {
        return await _context.Proveedors.OrderBy(p => p.RazonSocial).ToListAsync();
    }

    public async Task<Proveedor?> GetProviderByIdAsync(Guid id)
    {
        return await _context.Proveedors.FindAsync(id);
    }

    public async Task<ProviderFormViewModel> GetProviderFormContextAsync(Proveedor? existing = null)
    {
        return new ProviderFormViewModel
        {
            Provider = existing ?? new Proveedor { Activo = true, TipoPersona = "JURIDICA" },
            CuentasContables = new SelectList(await _context.CuentaContables.Where(c => c.Activo && c.AceptaMovimiento).OrderBy(c => c.Codigo).Select(c => new { c.Id, Display = c.Codigo + " " + c.Nombre }).ToListAsync(), "Id", "Display")
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

    public async Task<IEnumerable<ContratoEconomico>> GetContractsAsync()
    {
        return await _context.ContratoEconomicos.Include(c => c.Cliente).Include(c => c.Proveedor).OrderByDescending(c => c.FechaFirma).ToListAsync();
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

    public async Task<(bool Succeeded, string Message)> CreateContractAsync(ContratoEconomico contract)
    {
        try
        {
            contract.Id = Guid.NewGuid();
            contract.EntidadId = _entidadProvider.CurrentEntidadId;
            _context.ContratoEconomicos.Add(contract);
            await _context.SaveChangesAsync();
            return (true, "Contrato económico registrado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateContractAsync(ContratoEconomico contract)
    {
        try
        {
            var existing = await _context.ContratoEconomicos.FindAsync(contract.Id);
            if (existing == null) return (false, "El contrato no existe.");

            _context.Entry(existing).CurrentValues.SetValues(contract);
            existing.EntidadId = _entidadProvider.CurrentEntidadId; // Preservar multi-inquilino

            await _context.SaveChangesAsync();
            return (true, "Contrato económico actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<TopePrecioMfp>> GetPriceLimitsAsync()
    {
        return await _context.TopePrecioMfps.Include(t => t.Producto).Include(t => t.Familia).ToListAsync();
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
