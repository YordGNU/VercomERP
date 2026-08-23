using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.ViewModels;

namespace Vercom.Services;

public interface IAdminService
{
    // Auditoría (RF-03)
    Task<AuditIndexViewModel> GetAuditLogsAsync(string? filterUser, string? filterTable, DateTime? start, DateTime? end);
    Task<Auditorium?> GetAuditDetailAsync(long id);

    // Entidad (RF-04)
    Task<IEnumerable<Entidad>> GetEntidadesAsync(bool isMaster);
    Task<Entidad?> GetEntidadByIdAsync(Guid id, bool isMaster, Guid currentEntidadId);
    Task<(bool Succeeded, string Message)> UpdateEntidadAsync(Entidad entidad);

    // Consecutivos (RNF-51)
    Task<IEnumerable<Consecutivo>> GetConsecutivosAsync();
    Task<ConsecutivoFormViewModel> GetConsecutivoFormContextAsync(Guid? id = null);
    Task<(bool Succeeded, string Message)> SaveConsecutivoAsync(Consecutivo entry);

    // Parámetros del Sistema
    Task<IEnumerable<ParametroSistema>> GetParametersAsync();
    Task<ParametroFormViewModel> GetParameterFormContextAsync(Guid? id = null);
    Task<(bool Succeeded, string Message)> SaveParameterAsync(ParametroSistema entry);

    // Sucursales
    Task<IEnumerable<Sucursal>> GetSucursalesAsync();
    Task<Sucursal?> GetSucursalByIdAsync(Guid id);
    Task<(bool Succeeded, string Message)> CreateSucursalAsync(Sucursal sucursal);
    Task<(bool Succeeded, string Message)> UpdateSucursalAsync(Sucursal sucursal);

    // Dashboard Administrativo
    Task<dynamic> GetAdminStatsAsync();
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly Security.IEntidadProvider _entidadProvider;

    public AdminService(AppDbContext context, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _entidadProvider = entidadProvider;
    }

    public async Task<AuditIndexViewModel> GetAuditLogsAsync(string? filterUser, string? filterTable, DateTime? start, DateTime? end)
    {
        var query = _context.Auditoria.AsQueryable();

        if (!string.IsNullOrEmpty(filterUser))
            query = query.Where(a => a.NombreUsuario.Contains(filterUser));

        if (!string.IsNullOrEmpty(filterTable))
            query = query.Where(a => a.EsquemaTabla.Contains(filterTable));

        if (start.HasValue)
            query = query.Where(a => a.OcurridoEn >= start.Value);

        if (end.HasValue)
            query = query.Where(a => a.OcurridoEn <= end.Value);

        return new AuditIndexViewModel
        {
            FilterUser = filterUser,
            FilterTable = filterTable,
            StartDate = start,
            EndDate = end,
            Logs = await query.OrderByDescending(a => a.OcurridoEn).Take(500).ToListAsync()
        };
    }

    public async Task<Auditorium?> GetAuditDetailAsync(long id)
    {
        return await _context.Auditoria.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Entidad>> GetEntidadesAsync(bool isMaster)
    {
        if (!isMaster)
            return await _context.Entidads.Where(e => e.Id == _entidadProvider.CurrentEntidadId).ToListAsync();

        return await _context.Entidads.ToListAsync();
    }

    public async Task<Entidad?> GetEntidadByIdAsync(Guid id, bool isMaster, Guid currentEntidadId)
    {
        if (!isMaster && id != currentEntidadId) return null;
        return await _context.Entidads.FindAsync(id);
    }

    public async Task<(bool Succeeded, string Message)> UpdateEntidadAsync(Entidad entidad)
    {
        try
        {
            var existing = await _context.Entidads.FindAsync(entidad.Id);
            if (existing == null) return (false, "Entidad no encontrada.");

            _context.Entry(existing).CurrentValues.SetValues(entidad);
            existing.ActualizadoEn = DateTimeOffset.Now;

            await _context.SaveChangesAsync();
            return (true, "Datos de entidad actualizados.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<IEnumerable<Consecutivo>> GetConsecutivosAsync()
    {
        return await _context.Consecutivos.Include(c => c.Sucursal).ToListAsync();
    }

    public async Task<ConsecutivoFormViewModel> GetConsecutivoFormContextAsync(Guid? id = null)
    {
        var existing = id.HasValue ? await _context.Consecutivos.FindAsync(id.Value) : null;
        return new ConsecutivoFormViewModel
        {
            Consecutivo = existing ?? new Consecutivo { UltimoNumero = 0, LongitudPadding = 8 },
            Sucursales = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Sucursals.ToListAsync(), "Id", "Nombre")
        };
    }

    public async Task<(bool Succeeded, string Message)> SaveConsecutivoAsync(Consecutivo entry)
    {
        try
        {
            if (entry.Id == 0)
            {
                entry.EntidadId = _entidadProvider.CurrentEntidadId;
                _context.Consecutivos.Add(entry);
            }
            else
            {
                var existing = await _context.Consecutivos.FindAsync(entry.Id);
                if (existing == null) return (false, "No existe.");
                _context.Entry(existing).CurrentValues.SetValues(entry);
            }
            await _context.SaveChangesAsync();
            return (true, "Consecutivo guardado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<ParametroSistema>> GetParametersAsync()
    {
        return await _context.ParametroSistemas.ToListAsync();
    }

    public async Task<ParametroFormViewModel> GetParameterFormContextAsync(Guid? id = null)
    {
        var existing = id.HasValue ? await _context.ParametroSistemas.FindAsync(id.Value) : null;
        return new ParametroFormViewModel
        {
            Parametro = existing ?? new ParametroSistema { VigenteDesde = DateOnly.FromDateTime(DateTime.Now), TipoDato = "STRING" }
        };
    }

    public async Task<(bool Succeeded, string Message)> SaveParameterAsync(ParametroSistema entry)
    {
        try
        {
            if (entry.Id == 0)
            {
                entry.EntidadId = _entidadProvider.CurrentEntidadId;
                _context.ParametroSistemas.Add(entry);
            }
            else
            {
                var existing = await _context.ParametroSistemas.FindAsync(entry.Id);
                if (existing == null) return (false, "No existe.");
                _context.Entry(existing).CurrentValues.SetValues(entry);
            }
            await _context.SaveChangesAsync();
            return (true, "Parámetro guardado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<Sucursal>> GetSucursalesAsync()
    {
        return await _context.Sucursals.OrderBy(s => s.Nombre).ToListAsync();
    }

    public async Task<Sucursal?> GetSucursalByIdAsync(Guid id)
    {
        return await _context.Sucursals.FindAsync(id);
    }

    public async Task<(bool Succeeded, string Message)> CreateSucursalAsync(Sucursal sucursal)
    {
        try
        {
            sucursal.Id = Guid.NewGuid();
            sucursal.EntidadId = _entidadProvider.CurrentEntidadId;
            sucursal.CreadoEn = DateTimeOffset.Now;
            _context.Sucursals.Add(sucursal);
            await _context.SaveChangesAsync();
            return (true, "Sucursal creada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateSucursalAsync(Sucursal sucursal)
    {
        try
        {
            var existing = await _context.Sucursals.FindAsync(sucursal.Id);
            if (existing == null) return (false, "No existe.");
            _context.Entry(existing).CurrentValues.SetValues(sucursal);
            existing.EntidadId = _entidadProvider.CurrentEntidadId;
            await _context.SaveChangesAsync();
            return (true, "Sucursal actualizada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<dynamic> GetAdminStatsAsync()
    {
        return new
        {
            TotalUsers = await _context.Usuarios.CountAsync(),
            ActiveSessions = await _context.SesionCajaPos.CountAsync(s => s.Estado == "ABIERTA"),
            FailedAudits = await _context.Auditoria.CountAsync(a => a.Accion == "LOGIN_FALLIDO"),
            LastBackup = await _context.BackupLogs.Where(l => l.Estado == "EXITOSO").OrderByDescending(l => l.FinalizadoEn).FirstOrDefaultAsync()
        };
    }
}
