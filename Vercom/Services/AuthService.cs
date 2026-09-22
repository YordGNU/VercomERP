using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;
using Vercom.ViewModels;
using BC = BCrypt.Net.BCrypt;

namespace Vercom.Services;

public interface IAuthService
{
    // Autenticación
    Task<(bool Succeeded, string Message, bool MustChangePassword)> LoginAsync(string username, string password, bool rememberMe);
    Task LogoutAsync();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    Task<(bool Succeeded, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    // Gestión de Usuarios
    Task<IEnumerable<Usuario>> GetUsersAsync();
    Task<Usuario?> GetUserByIdAsync(Guid id);
    Task<UserFormViewModel> GetUserFormContextAsync(Usuario? existing = null);
    Task<(bool Succeeded, string Message)> CreateUserAsync(Usuario usuario, string password, List<int> selectedRoles);
    Task<(bool Succeeded, string Message)> UpdateUserAsync(Usuario usuario, List<int> selectedRoles);
    Task<(bool Succeeded, string Message)> ToggleUserStatusAsync(Guid id);
    Task<(bool Succeeded, string Message)> ResetUserPasswordAsync(Guid id, string newPassword);

    // Gestión de Roles
    Task<IEnumerable<Rol>> GetRolesAsync();
    Task<Rol?> GetRolByIdAsync(int id);
    Task<RolPermissionsViewModel> GetRolPermissionsContextAsync(int rolId);
    Task<(bool Succeeded, string Message)> UpdateRolPermissionsAsync(int rolId, int[] selectedPermissions);
    Task<(bool Succeeded, string Message)> CreateRolAsync(Rol rol);
    Task<(bool Succeeded, string Message)> UpdateRolAsync(Rol rol);
    Task<(bool Succeeded, string Message)> DeleteRolAsync(int id);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Security.IEntidadProvider _entidadProvider;

    public AuthService(AppDbContext context, IHttpContextAccessor httpContextAccessor, Security.IEntidadProvider entidadProvider)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _entidadProvider = entidadProvider;
    }

    public async Task<IEnumerable<Usuario>> GetUsersAsync()
    {
        var isMaster = _entidadProvider.IsMaster;

        if (isMaster)
        {
            return await _context.Usuarios
                .IgnoreQueryFilters()
                .Include(u => u.Entidad)
                .Include(u => u.EsEmpleado)
                .Include(u => u.Sucursal)
                .ToListAsync();
        }

        return await _context.Usuarios
            .Include(u => u.EsEmpleado)
            .Include(u => u.Sucursal)
            .ToListAsync();
    }

    public async Task<Usuario?> GetUserByIdAsync(Guid id)
    {
        var isMaster = _entidadProvider.IsMaster;
        var query = _context.Usuarios.AsQueryable();

        if (isMaster) query = query.IgnoreQueryFilters();

        return await query
            .Include(u => u.Entidad)
            .Include(u => u.EsEmpleado)
            .Include(u => u.Sucursal)
            .Include(u => u.UsuarioRolUsuarios).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<UserFormViewModel> GetUserFormContextAsync(Usuario? existing = null)
    {
        var entidadId = _entidadProvider.CurrentEntidadId;
        var isMaster = _entidadProvider.IsMaster;

        var roles = await GetRolesAsync();

        var entidades = isMaster
            ? await _context.Entidads.IgnoreQueryFilters().OrderBy(e => e.NombreComercial).ToListAsync()
            : new List<Entidad>();

        return new UserFormViewModel
        {
            Usuario = existing ?? new Usuario { Activo = true },
            Empleados = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Empleados.Where(e => e.Estado == "ACTIVO").ToListAsync(), "Id", "NombreCompleto"),
            Sucursales = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Sucursals.Where(s => s.Activo).ToListAsync(), "Id", "Nombre"),
            Entidades = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(entidades, "Id", "NombreComercial"),
            RolesDisponibles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Disabled = r.EsSistema && !isMaster
            }),
            SelectedRoles = existing?.UsuarioRolUsuarios.Select(ur => ur.RolId).ToList() ?? new List<int>()
        };
    }

    public async Task<(bool Succeeded, string Message)> CreateUserAsync(Usuario usuario, string password, List<int> selectedRoles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            usuario.Id = Guid.NewGuid();

            // Si es Master, permite elegir entidad, si no, fuerza la suya
            if (!_entidadProvider.IsMaster || usuario.EntidadId == Guid.Empty)
            {
                usuario.EntidadId = _entidadProvider.CurrentEntidadId;
            }

            usuario.HashPassword = HashPassword(password);
            usuario.CreadoEn = DateTimeOffset.Now;
            usuario.ActualizadoEn = DateTimeOffset.Now;
            usuario.DebeCambiarPass = true;

            _context.Usuarios.Add(usuario);

            foreach (var roleId in selectedRoles)
            {
                var role = await _context.Rols.FindAsync(roleId);
                if (role == null) continue;

                // Bloqueo de seguridad: No Master no puede asignar roles de sistema
                if (role.EsSistema && !_entidadProvider.IsMaster) continue;

                _context.UsuarioRols.Add(new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = roleId,
                    SucursalId = usuario.SucursalId ?? Guid.Empty,
                    AsignadoEn = DateTimeOffset.Now,
                    AsignadoPor = _entidadProvider.CurrentUsuarioId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Usuario creado correctamente con sus roles.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al crear usuario: {ex.Message}");
        }
    }

    public async Task<(bool Succeeded, string Message)> UpdateUserAsync(Usuario usuario, List<int> selectedRoles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.Usuarios
                .Include(u => u.UsuarioRolUsuarios)
                .FirstOrDefaultAsync(u => u.Id == usuario.Id);

            if (existing == null) return (false, "Usuario no encontrado.");

            _context.Entry(existing).CurrentValues.SetValues(usuario);

            if (!_entidadProvider.IsMaster)
            {
                existing.EntidadId = _entidadProvider.CurrentEntidadId;
            }

            existing.ActualizadoEn = DateTimeOffset.Now;

            // Actualizar Roles
            _context.UsuarioRols.RemoveRange(existing.UsuarioRolUsuarios);
            foreach (var roleId in selectedRoles)
            {
                var role = await _context.Rols.FindAsync(roleId);
                if (role == null) continue;
                if (role.EsSistema && !_entidadProvider.IsMaster) continue;

                _context.UsuarioRols.Add(new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = roleId,
                    SucursalId = usuario.SucursalId ?? Guid.Empty,
                    AsignadoEn = DateTimeOffset.Now,
                    AsignadoPor = _entidadProvider.CurrentUsuarioId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Usuario y roles actualizados.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string Message)> ToggleUserStatusAsync(Guid id)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null) return (false, "No existe.");

        user.Activo = !user.Activo;
        await _context.SaveChangesAsync();
        return (true, $"Usuario {(user.Activo ? "activado" : "desactivado")}.");
    }

    public async Task<(bool Succeeded, string Message)> ResetUserPasswordAsync(Guid id, string newPassword)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null) return (false, "No existe.");

        user.HashPassword = HashPassword(newPassword);
        user.DebeCambiarPass = true;
        await _context.SaveChangesAsync();
        return (true, "Contraseña reiniciada.");
    }

    public async Task<IEnumerable<Rol>> GetRolesAsync()
    {
        var isMaster = _entidadProvider.IsMaster;
        var entidadId = _entidadProvider.CurrentEntidadId;

        if (isMaster)
            return await _context.Rols.OrderBy(r => r.Codigo).ToListAsync();

        // El Administrador local solo ve roles de sistema (lectura) y los de su propia entidad
        return await _context.Rols
            .Where(r => r.EsSistema || r.EntidadId == entidadId)
            .OrderBy(r => r.Codigo)
            .ToListAsync();
    }

    public async Task<Rol?> GetRolByIdAsync(int id)
    {
        return await _context.Rols.Include(r => r.Permisos).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<RolPermissionsViewModel> GetRolPermissionsContextAsync(int rolId)
    {
        var rol = await GetRolByIdAsync(rolId);
        if (rol == null) throw new Exception("Rol no encontrado.");

        var allPermissions = await _context.Permisos.ToListAsync();
        var selectedIds = rol.Permisos.Select(p => p.Id).ToList();

        var vm = new RolPermissionsViewModel
        {
            Rol = rol,
            Modules = allPermissions.GroupBy(p => p.Modulo)
                .Select(g => new PermissionGroup
                {
                    ModuleName = g.Key,
                    Permissions = g.Select(p => new PermissionItem
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Descripcion = p.Descripcion ?? p.Codigo,
                        IsSelected = selectedIds.Contains(p.Id)
                    }).ToList()
                }).ToList()
        };

        return vm;
    }

    public async Task<(bool Succeeded, string Message)> UpdateRolPermissionsAsync(int rolId, int[] selectedPermissions)
    {
        var rol = await _context.Rols.Include(r => r.Permisos).FirstOrDefaultAsync(r => r.Id == rolId);
        if (rol == null) return (false, "Rol no encontrado.");

        if (rol.EsSistema && !_entidadProvider.IsMaster)
            return (false, "No se pueden modificar los permisos de los roles de sistema.");

        rol.Permisos.Clear();
        foreach (var pId in selectedPermissions)
        {
            var p = await _context.Permisos.FindAsync(pId);
            if (p != null) rol.Permisos.Add(p);
        }

        await _context.SaveChangesAsync();
        return (true, "Permisos actualizados.");
    }

    public async Task<(bool Succeeded, string Message)> CreateRolAsync(Rol rol)
    {
        try
        {
            if (await _context.Rols.AnyAsync(r => r.Codigo == rol.Codigo && r.EntidadId == _entidadProvider.CurrentEntidadId))
                return (false, "Ya existe un rol con ese código en su entidad.");

            rol.Id = 0;
            rol.EntidadId = _entidadProvider.CurrentEntidadId;
            rol.EsSistema = false;
            rol.CreadoEn = DateTimeOffset.Now;

            _context.Rols.Add(rol);
            await _context.SaveChangesAsync();
            return (true, "Rol creado exitosamente.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> UpdateRolAsync(Rol rol)
    {
        try
        {
            var existing = await _context.Rols.FindAsync(rol.Id);
            if (existing == null) return (false, "Rol no encontrado.");

            if (existing.EsSistema && !_entidadProvider.IsMaster)
                return (false, "No se puede editar un rol de sistema.");

            if (!_entidadProvider.IsMaster && existing.EntidadId != _entidadProvider.CurrentEntidadId)
                return (false, "No tiene permisos para editar este rol.");

            _context.Entry(existing).CurrentValues.SetValues(rol);
            await _context.SaveChangesAsync();
            return (true, "Rol actualizado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message)> DeleteRolAsync(int id)
    {
        try
        {
            var rol = await _context.Rols.FindAsync(id);
            if (rol == null) return (false, "Rol no encontrado.");
            if (rol.EsSistema) return (false, "No se pueden eliminar roles de sistema.");

            if (!_entidadProvider.IsMaster && rol.EntidadId != _entidadProvider.CurrentEntidadId)
                return (false, "No tiene permisos para eliminar este rol.");

            _context.Rols.Remove(rol);
            await _context.SaveChangesAsync();
            return (true, "Rol eliminado.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Succeeded, string Message, bool MustChangePassword)> LoginAsync(string username, string password, bool rememberMe)
    {
        var usuario = await _context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Entidad)
            .Include(u => u.UsuarioRolUsuarios)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == username);

        if (usuario == null)
            return (false, "Usuario o contraseña incorrectos.", false);

        if (!usuario.Activo)
            return (false, "La cuenta de usuario está desactivada.", false);

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.Now)
            return (false, $"La cuenta está bloqueada hasta {usuario.BloqueadoHasta.Value:HH:mm:ss}.", false);

        if (!VerifyPassword(password, usuario.HashPassword))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= 5)
            {
                usuario.BloqueadoHasta = DateTime.Now.AddMinutes(15);
                usuario.IntentosFallidos = 0;
            }
            await _context.SaveChangesAsync();
            return (false, "Usuario o contraseña incorrectos.", false);
        }

        // Reset fail count
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoLogin = DateTime.Now;
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim("FullName", usuario.NombreCompleto),
            new Claim("EntidadId", usuario.EntidadId.ToString()),
            new Claim("EntidadNombre", usuario.Entidad.RazonSocial),
            new Claim("SucursalId", usuario.SucursalId?.ToString() ?? ""),
            new Claim("MustChangePassword", usuario.DebeCambiarPass.ToString())
        };

        foreach (var ur in usuario.UsuarioRolUsuarios)
        {
            claims.Add(new Claim(ClaimTypes.Role, ur.Rol.Codigo));

            // Cargar permisos del rol como claims para evitar consultas repetitivas a la DB
            var roleWithPerms = await _context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Id == ur.RolId);
            foreach (var p in roleWithPerms.Permisos)
            {
                if (!claims.Any(c => c.Type == "Permission" && c.Value == p.Codigo))
                {
                    claims.Add(new Claim("Permission", p.Codigo));
                }
            }
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = rememberMe };

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return (true, "Inicio de sesión exitoso.", usuario.DebeCambiarPass);
    }

    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public string HashPassword(string password)
    {
        return BC.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try { return BC.Verify(password, hashedPassword); }
        catch { return false; }
    }

    public async Task<(bool Succeeded, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Usuarios.FindAsync(userId);
        if (user == null) return (false, "Usuario no encontrado.");

        if (!VerifyPassword(currentPassword, user.HashPassword))
            return (false, "La contraseña actual es incorrecta.");

        user.HashPassword = HashPassword(newPassword);
        user.DebeCambiarPass = false;
        user.ActualizadoEn = DateTime.Now;

        await _context.SaveChangesAsync();

        // Tras cambiar la clave, se debe re-autenticar o actualizar el claim en la sesión actual
        // Para simplificar, pediremos login de nuevo o cerraremos sesión.
        await LogoutAsync();

        return (true, "Contraseña actualizada correctamente. Por favor, inicie sesión con su nueva clave.");
    }
}
