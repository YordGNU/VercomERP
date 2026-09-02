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
    Task<(bool Succeeded, string Message)> RegisterEntidadAsync(EntityRegistrationViewModel vm);
    Task<(bool Succeeded, string Message)> ApproveEntidadAsync(Guid id, Guid masterUserId);
    Task<IEnumerable<Entidad>> GetPendingEntidadesAsync();

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
    Task<MasterDashboardViewModel> GetMasterDashboardStatsAsync();
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly Security.IEntidadProvider _entidadProvider;

    public AdminService(AppDbContext context, INotificationService notificationService, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _notificationService = notificationService;
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

        return await _context.Entidads.IgnoreQueryFilters().OrderByDescending(e => e.CreadoEn).ToListAsync();
    }

    public async Task<IEnumerable<Entidad>> GetPendingEntidadesAsync()
    {
        return await _context.Entidads.IgnoreQueryFilters().Where(e => !e.Activo).OrderByDescending(e => e.CreadoEn).ToListAsync();
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

    public async Task<(bool Succeeded, string Message)> RegisterEntidadAsync(EntityRegistrationViewModel vm)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Validar NIT único
            if (await _context.Entidads.AnyAsync(e => e.Nit == vm.Entidad.Nit))
                return (false, "Ya existe una entidad registrada con este NIT.");

            // 2. Crear Entidad Inactiva
            var entidad = vm.Entidad;
            entidad.Id = Guid.NewGuid();
            entidad.Activo = false;
            entidad.CreadoEn = DateTimeOffset.Now;
            entidad.ActualizadoEn = DateTimeOffset.Now;
            if (string.IsNullOrEmpty(entidad.MonedaBase)) entidad.MonedaBase = "CUP";
            if (string.IsNullOrEmpty(entidad.FormaJuridica)) entidad.FormaJuridica = "S.U.R.L.";

            _context.Entidads.Add(entidad);

            // 3. Crear Usuario Administrador (Inactivo)
            var adminUser = new Usuario
            {
                Id = Guid.NewGuid(),
                EntidadId = entidad.Id,
                NombreUsuario = vm.AdminUsername,
                NombreCompleto = vm.AdminFullName,
                Email = vm.AdminEmail,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Activo = false, // Inactivo hasta que se apruebe la entidad
                DebeCambiarPass = false,
                CreadoEn = DateTimeOffset.Now,
                ActualizadoEn = DateTimeOffset.Now
            };
            _context.Usuarios.Add(adminUser);

            await _context.SaveChangesAsync();

            // Asignar Rol ADMIN (si existe)
            var adminRole = await _context.Rols.FirstOrDefaultAsync(r => r.Codigo == "ADMIN");
            if (adminRole != null)
            {
                _context.UsuarioRols.Add(new UsuarioRol { UsuarioId = adminUser.Id, RolId = adminRole.Id, AsignadoEn = DateTimeOffset.Now });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return (true, "Solicitud de registro enviada. Un administrador revisará su solicitud pronto.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al registrar entidad: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> ApproveEntidadAsync(Guid id, Guid masterUserId)
    {
        var entidad = await _context.Entidads.FindAsync(id);
        if (entidad == null) return (false, "Entidad no encontrada.");
        if (entidad.Activo) return (false, "La entidad ya está activa.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Activar Entidad
            entidad.Activo = true;
            entidad.ActualizadoEn = DateTimeOffset.Now;

            // 2. Activar Usuario(s) asociados (el primer admin al menos)
            var users = await _context.Usuarios.Where(u => u.EntidadId == id).ToListAsync();
            foreach (var user in users) user.Activo = true;

            // 3. Inicializar Parámetros y Consecutivos
            await InitializeEntidadDefaultsAsync(id);

            // 4. Notificar a Maestros
            await _notificationService.NotifyMasterAsync("Nueva Entidad Activa", $"La S.U.R.L. {entidad.RazonSocial} ha sido aprobada y configurada.", "success");

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Entidad {entidad.RazonSocial} aprobada y configurada correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error durante la activación: {ex.Message}");
        }
    }

    private async Task InitializeEntidadDefaultsAsync(Guid entidadId)
    {
        // 1. Sucursal Base
        var sucursal = new Sucursal
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            Codigo = "MATRIZ",
            Nombre = "Casa Matriz / Oficina Central",
            Tipo = "OFICINA",
            Activo = true,
            CreadoEn = DateTimeOffset.Now
        };
        _context.Sucursals.Add(sucursal);
        await _context.SaveChangesAsync();

        // 2. Parámetros Fiscales Cubanos
        var parametros = new List<ParametroSistema>
        {
            new ParametroSistema { EntidadId = entidadId, Codigo = "TASA_SS_PATRONAL", Valor = "0.125", TipoDato = "NUMERIC", Descripcion = "Tasa Seguridad Social Patronal (12.5%)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "TASA_FUERZA_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "Impuesto Fuerza de Trabajo (5%)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "RET_SS_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "Retención SS Trabajador (5%)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "FACTOR_VAC", Valor = "0.0909", TipoDato = "NUMERIC", Descripcion = "Factor Acumulación Vacaciones (9.09%)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) }
        };
        _context.ParametroSistemas.AddRange(parametros);

        // 3. Consecutivos Base (Serie A)
        var documentos = new[] { "FACTURA_VENTA", "ORDEN_COMPRA", "ORDEN_PRODUCCION", "ASIENTO_CONTABLE", "VALE_ENTRADA", "VALE_SALIDA" };
        foreach (var doc in documentos)
        {
            _context.Consecutivos.Add(new Consecutivo
            {
                EntidadId = entidadId,
                SucursalId = sucursal.Id,
                TipoDocumento = doc,
                Serie = "A",
                UltimoNumero = 0,
                LongitudPadding = 8,
                ActualizadoEn = DateTimeOffset.Now
            });
        }
    }

    public async Task<IEnumerable<Consecutivo>> GetConsecutivosAsync()
    {
        return await _context.Consecutivos.Include(c => c.Sucursal).ToListAsync();
    }

    public async Task<ConsecutivoFormViewModel> GetConsecutivoFormContextAsync(Guid? id = null)
    {
        var existing = id.HasValue ? await _context.Consecutivos.FindAsync(id.Value) : null;
        var tipos_documentos = new List<string> {
            "VALE","FACTURA","REPORTE","EMISION"
        };
        return new ConsecutivoFormViewModel
        {
            Consecutivo = existing ?? new Consecutivo { UltimoNumero = 0, LongitudPadding = 8 },
            Sucursales = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Sucursals.ToListAsync(), "Id", "Nombre"),
            TipoDocumentos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tipos_documentos, "Id", "Nombre")
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

    public async Task<MasterDashboardViewModel> GetMasterDashboardStatsAsync()
    {
        return new MasterDashboardViewModel
        {
            TotalEntidades = await _context.Entidads.IgnoreQueryFilters().CountAsync(e => e.Activo),
            EntidadesPendientes = await _context.Entidads.IgnoreQueryFilters().CountAsync(e => !e.Activo),
            UsuariosTotales = await _context.Usuarios.IgnoreQueryFilters().CountAsync(),
            SesionesPosActivas = await _context.SesionCajaPos.IgnoreQueryFilters().CountAsync(s => s.Estado == "ABIERTA"),
            UltimoBackup = await _context.BackupLogs.IgnoreQueryFilters()
                .Where(l => l.Estado == "EXITOSO")
                .OrderByDescending(l => l.FinalizadoEn)
                .FirstOrDefaultAsync(),
            AlertasSeguridad = await _context.Auditoria.IgnoreQueryFilters()
                .Where(a => a.Accion == "LOGIN_FALLIDO" || a.Accion == "ACCESO_DENEGADO")
                .OrderByDescending(a => a.OcurridoEn)
                .Take(5)
                .ToListAsync(),
            AlertasVencimiento = await _context.ExistenciaLotes.IgnoreQueryFilters()
                .Include(l => l.Producto)
                .Include(l => l.Almacen)
                .Where(l => l.Cantidad > 0 && l.FechaVencimiento != null && l.FechaVencimiento <= DateOnly.FromDateTime(DateTime.Now.AddDays(30)))
                .OrderBy(l => l.FechaVencimiento)
                .Take(5)
                .ToListAsync()
        };
    }
}
