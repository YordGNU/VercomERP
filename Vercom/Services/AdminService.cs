using Microsoft.EntityFrameworkCore;
using Vercom.Helpers;
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
    Task<ConsecutivoFormViewModel> GetConsecutivoFormContextAsync(int? id = null);
    Task<Consecutivo?> GetConsecutivoByIdAsync(int id);
    Task<(bool Succeeded, string Message)> SaveConsecutivoAsync(Consecutivo entry);

    // Parámetros del Sistema
    Task<IEnumerable<ParametroSistema>> GetParametersAsync();
    Task<ParametroFormViewModel> GetParameterFormContextAsync(int? id = null);
    Task<(bool Succeeded, string Message)> SaveParameterAsync(ParametroSistema entry);

    // Sucursales
    Task<IEnumerable<Sucursal>> GetSucursalesAsync();
    Task<Sucursal?> GetSucursalByIdAsync(Guid id);
    Task<SucursalPagedResult> GetSucursalesPagedAsync(string? search = null, string? tipo = null, bool? activo = null, int page = 1, int pageSize = 12);
    Task<(bool Succeeded, string Message)> CreateSucursalAsync(Sucursal sucursal);
    Task<(bool Succeeded, string Message)> UpdateSucursalAsync(Sucursal sucursal);
    Task<(bool Succeeded, string Message)> DeleteSucursalAsync(Guid id);

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
            await _notificationService.NotifyMasterAsync("Nueva Entidad Activa", $"La S.U.R.L. {entidad.NombreComercial} ha sido aprobada y configurada.", "success");

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Entidad {entidad.NombreComercial} aprobada y configurada correctamente.");
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
        foreach (var doc in DocumentoTipo.Catalogo)
        {
            _context.Consecutivos.Add(new Consecutivo
            {
                EntidadId = entidadId,
                // La factura se numera por sucursal (RNF-51); el resto de las secuencias es central
                // porque sus tablas tienen índices únicos por entidad (asiento, OC, OP, vales).
                SucursalId = doc.Codigo == DocumentoTipo.FacturaVenta ? sucursal.Id : null,
                TipoDocumento = doc.Codigo,
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

    public async Task<Consecutivo?> GetConsecutivoByIdAsync(int id)
    {
        return await _context.Consecutivos
            .Include(c => c.Entidad)
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ConsecutivoFormViewModel> GetConsecutivoFormContextAsync(int? id = null)
    {
        var existing = id.HasValue
            ? await _context.Consecutivos
                .Include(c => c.Entidad)
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(c => c.Id == id.Value)
            : null;
        return new ConsecutivoFormViewModel
        {
            Consecutivo = existing ?? new Consecutivo { UltimoNumero = 0, LongitudPadding = 8 },
            Sucursales = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Sucursals.ToListAsync(), "Id", "Nombre"),
            TipoDocumentos = DocumentoTipo.Catalogo
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.Codigo, Text = $"{t.Nombre} ({t.Codigo})" })
                .ToList()
        };
    }

    public async Task<(bool Succeeded, string Message)> SaveConsecutivoAsync(Consecutivo entry)
    {
        if (string.IsNullOrWhiteSpace(entry.TipoDocumento))
            return (false, "Debe seleccionar un tipo de documento.");
        if (string.IsNullOrWhiteSpace(entry.Serie))
            return (false, "Debe indicar la serie del documento.");
        if (entry.LongitudPadding <= 0 || entry.LongitudPadding > 20)
            return (false, "La longitud de relleno debe estar entre 1 y 20.");
        if (entry.UltimoNumero < 0)
            return (false, "El último número no puede ser negativo.");

        try
        {
            entry.TipoDocumento = entry.TipoDocumento.Trim().ToUpperInvariant();
            entry.Serie = entry.Serie.Trim().ToUpperInvariant();

            if (entry.Id == 0)
            {
                entry.EntidadId = _entidadProvider.CurrentEntidadId;
                var exists = await _context.Consecutivos.AnyAsync(c =>
                    c.EntidadId == entry.EntidadId &&
                    c.SucursalId == entry.SucursalId &&
                    c.TipoDocumento == entry.TipoDocumento &&
                    c.Serie == entry.Serie);
                if (exists) return (false, "Ya existe un consecutivo para esa combinación de sucursal, tipo de documento y serie.");
                entry.ActualizadoEn = DateTimeOffset.Now;
                _context.Consecutivos.Add(entry);
            }
            else
            {
                var existing = await _context.Consecutivos.FindAsync(entry.Id);
                if (existing == null) return (false, "El consecutivo no existe.");
                entry.EntidadId = existing.EntidadId;
                var exists = await _context.Consecutivos.AnyAsync(c =>
                    c.Id != entry.Id &&
                    c.EntidadId == entry.EntidadId &&
                    c.SucursalId == entry.SucursalId &&
                    c.TipoDocumento == entry.TipoDocumento &&
                    c.Serie == entry.Serie);
                if (exists) return (false, "Ya existe un consecutivo para esa combinación de sucursal, tipo de documento y serie.");
                _context.Entry(existing).CurrentValues.SetValues(entry);
                existing.ActualizadoEn = DateTimeOffset.Now;
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

    public async Task<ParametroFormViewModel> GetParameterFormContextAsync(int? id = null)
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

    public async Task<SucursalPagedResult> GetSucursalesPagedAsync(string? search = null, string? tipo = null, bool? activo = null, int page = 1, int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 6, 48);

        var query = _context.Sucursals.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Nombre.Contains(term) || s.Codigo.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var t = tipo.Trim();
            query = query.Where(s => s.Tipo == t);
        }

        if (activo.HasValue)
        {
            var a = activo.Value;
            query = query.Where(s => s.Activo == a);
        }

        var totalItems = await query.CountAsync();
        var items = await query.OrderBy(s => s.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalSucursales = await _context.Sucursals.CountAsync();
        var activas = await _context.Sucursals.CountAsync(s => s.Activo);
        var tipos = await _context.Sucursals
            .Select(s => s.Tipo ?? "SIN TIPO")
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return new SucursalPagedResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Search = search,
            Tipo = tipo,
            Activo = activo,
            TotalSucursales = totalSucursales,
            Activas = activas,
            Inactivas = totalSucursales - activas,
            Tipos = tipos
        };
    }

    public async Task<Sucursal?> GetSucursalByIdAsync(Guid id)
    {
        return await _context.Sucursals
            .Include(s => s.Entidad)
            .Include(s => s.Usuarios)
            .Include(s => s.Almacens)
            .Include(s => s.Cajas)
            .Include(s => s.DispositivoPos)
            .Include(s => s.CentroCostos)
            .Include(s => s.Empleados)
            .Include(s => s.Consecutivos)
            .Include(s => s.ActivoFijos)
            .FirstOrDefaultAsync(s => s.Id == id);
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

    public async Task<(bool Succeeded, string Message)> DeleteSucursalAsync(Guid id)
    {
        try
        {
            var sucursal = await _context.Sucursals
                .Include(s => s.Usuarios)
                .Include(s => s.Empleados)
                .Include(s => s.Almacens)
                .Include(s => s.Cajas)
                .Include(s => s.CentroCostos)
                .Include(s => s.Consecutivos)
                .Include(s => s.DispositivoPos)
                .Include(s => s.ActivoFijos)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sucursal == null) return (false, "No existe.");

            var dependencias = new List<string>();
            if (sucursal.Usuarios.Any()) dependencias.Add($"{sucursal.Usuarios.Count} usuarios");
            if (sucursal.Empleados.Any()) dependencias.Add($"{sucursal.Empleados.Count} empleados");
            if (sucursal.Almacens.Any()) dependencias.Add($"{sucursal.Almacens.Count} almacenes");
            if (sucursal.Cajas.Any()) dependencias.Add($"{sucursal.Cajas.Count} cajas");
            if (sucursal.CentroCostos.Any()) dependencias.Add($"{sucursal.CentroCostos.Count} centros de costo");
            if (sucursal.Consecutivos.Any()) dependencias.Add($"{sucursal.Consecutivos.Count} consecutivos");
            if (sucursal.DispositivoPos.Any()) dependencias.Add($"{sucursal.DispositivoPos.Count} dispositivos POS");
            if (sucursal.ActivoFijos.Any()) dependencias.Add($"{sucursal.ActivoFijos.Count} activos fijos");

            if (dependencias.Any())
                return (false, $"No se puede eliminar: la sucursal tiene registros vinculados ({string.Join(", ", dependencias)}).");

            _context.Sucursals.Remove(sucursal);
            await _context.SaveChangesAsync();
            return (true, "Sucursal eliminada.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<MasterDashboardViewModel> GetMasterDashboardStatsAsync()
    {
        var now = DateTimeOffset.Now;
        var inicioMes = new DateTimeOffset(new DateTime(now.Year, now.Month, 1), now.Offset);

        // Ventas globales del mes agrupadas por entidad (alimenta KPI y ranking)
        var ventasMes = await _context.FacturaVenta.IgnoreQueryFilters()
            .Where(f => f.Estado == "EMITIDA" && f.Fecha >= inicioMes)
            .GroupBy(f => f.EntidadId)
            .Select(g => new { EntidadId = g.Key, Total = g.Sum(x => x.Total), Facturas = g.Count() })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var entidadIds = ventasMes.Select(v => v.EntidadId).ToList();
        var nombresEntidad = await _context.Entidads.IgnoreQueryFilters()
            .Where(e => entidadIds.Contains(e.Id))
            .Select(e => new { e.Id, e.NombreComercial })
            .ToDictionaryAsync(e => e.Id, e => e.NombreComercial);

        var pendientes = await _context.PosVentaPendientes.IgnoreQueryFilters()
            .Where(p => p.Estado == "PENDIENTE")
            .Select(p => new { p.MensajeError })
            .ToListAsync();

        var sesiones = await _context.SesionCajaPos.IgnoreQueryFilters()
            .Where(s => s.Estado == "ABIERTA")
            .Include(s => s.Cajero)
            .Include(s => s.DispositivoPos).ThenInclude(d => d.Entidad)
            .OrderByDescending(s => s.FechaApertura)
            .Take(10)
            .ToListAsync();

        return new MasterDashboardViewModel
        {
            TotalEntidades = await _context.Entidads.IgnoreQueryFilters().CountAsync(e => e.Activo),
            EntidadesPendientes = await _context.Entidads.IgnoreQueryFilters().CountAsync(e => !e.Activo),
            UsuariosTotales = await _context.Usuarios.IgnoreQueryFilters().CountAsync(),
            SesionesPosActivas = await _context.SesionCajaPos.IgnoreQueryFilters().CountAsync(s => s.Estado == "ABIERTA"),
            DispositivosPos = await _context.DispositivoPos.IgnoreQueryFilters().CountAsync(),
            VentasGlobalesMes = ventasMes.Sum(v => v.Total),
            FacturasGlobalesMes = ventasMes.Sum(v => v.Facturas),
            PosPendientes = pendientes.Count,
            PosConflictos = pendientes.Count(p => !string.IsNullOrEmpty(p.MensajeError)),
            TopEntidades = ventasMes.Take(5).Select(v => new TopEntidadItem
            {
                Nombre = nombresEntidad.TryGetValue(v.EntidadId, out var n) ? n : "Entidad",
                Ventas = v.Total,
                Facturas = v.Facturas
            }).ToList(),
            SesionesPos = sesiones.Select(s => new PosSesionItem
            {
                Dispositivo = s.DispositivoPos?.Codigo ?? "N/D",
                Entidad = s.DispositivoPos?.Entidad?.NombreComercial,
                Cajero = s.Cajero?.NombreCompleto ?? "N/D",
                FechaApertura = s.FechaApertura,
                MontoApertura = s.MontoApertura,
                TotalVentas = s.TotalVentas,
                TotalEfectivo = s.TotalEfectivo,
                CantidadFacturas = s.CantidadFacturas
            }).ToList(),
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
