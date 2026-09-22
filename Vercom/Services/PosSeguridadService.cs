using Microsoft.EntityFrameworkCore;
using Vercom.DTOs;
using Vercom.Models;
using BC = BCrypt.Net.BCrypt;

namespace Vercom.Services;

public sealed class PosSeguridadService
{
    private readonly AppDbContext _db;

    public PosSeguridadService(AppDbContext db) => _db = db;

    public async Task<PosRbacSnapshotDto> SnapshotAsync(Guid entidadId, Guid? sucursalId, CancellationToken cancellationToken)
    {
        // Roles del POS: solo la familia POS_*
        var posRolIds = await _db.Rols.AsNoTracking().IgnoreQueryFilters()
            .Where(r => r.Codigo.StartsWith("POS_") && (r.EntidadId == null || r.EntidadId == entidadId))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
        var posRolIdSet = posRolIds.ToHashSet();

        // Asignaciones (usuario-rol) de la sucursal sobre roles POS. Son las que definen
        // los "usuarios vinculados a la sucursal" con acceso al POS.
        var usuarioRoles = await _db.UsuarioRols.AsNoTracking().IgnoreQueryFilters()
            .Where(x => posRolIdSet.Contains(x.RolId))
            .Where(x => sucursalId == null || x.SucursalId == sucursalId)
            .ToListAsync(cancellationToken);
        var usuarioIdsEnAlcance = usuarioRoles.Select(x => x.UsuarioId).Distinct().ToHashSet();

        // Usuarios de la entidad que tienen rol POS en la sucursal del dispositivo.
        var usuarios = await _db.Usuarios.AsNoTracking().IgnoreQueryFilters()
            .Where(x => x.EntidadId == entidadId && usuarioIdsEnAlcance.Contains(x.Id))
            .OrderBy(x => x.NombreUsuario)
            .ToListAsync(cancellationToken);
        var usuarioIds = usuarios.Select(x => x.Id).ToHashSet();
        usuarioRoles = usuarioRoles.Where(x => usuarioIds.Contains(x.UsuarioId)).ToList();

        var roles = await _db.Rols.AsNoTracking()
            .Include(r => r.Permisos)
            .Where(x => x.Codigo.StartsWith("POS_"))
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);
        var permisos = await _db.Permisos.AsNoTracking()
            .Where(x => x.Modulo == "POS")
            .OrderBy(x => x.Modulo).ThenBy(x => x.Codigo)
            .ToListAsync(cancellationToken);
        var permisosIds = permisos.Select(x => x.Id).ToHashSet();

        var usuarioRolIds = usuarioRoles.GroupBy(x => x.UsuarioId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RolId).Distinct().OrderBy(i => i).ToList());

        return new PosRbacSnapshotDto
        {
            Roles = roles.Select(r => new PosRolDto
            {
                Id = r.Id,
                Codigo = r.Codigo,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                EsSistema = r.EsSistema,
                PermisoIds = r.Permisos.Where(p => permisosIds.Contains(p.Id)).Select(p => p.Id).Distinct().OrderBy(i => i).ToList()
            }).ToList(),
            Permisos = permisos.Select(p => new PosPermisoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Modulo = p.Modulo,
                Descripcion = p.Descripcion
            }).ToList(),
            Usuarios = usuarios.Select(u => new PosUsuarioDto
            {
                Id = u.Id,
                EntidadId = u.EntidadId,
                SucursalId = u.SucursalId,
                NombreUsuario = u.NombreUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                Activo = u.Activo,
                RolIds = usuarioRolIds.TryGetValue(u.Id, out var ids2) ? ids2 : Array.Empty<int>()
            }).ToList(),
            UsuarioRoles = usuarioRoles.Select(ur => new PosUsuarioRolDto
            {
                UsuarioId = ur.UsuarioId,
                RolId = ur.RolId,
                SucursalId = ur.SucursalId
            }).ToList(),
            RolPermisos = roles.SelectMany(r => r.Permisos
                    .Where(p => permisosIds.Contains(p.Id))
                    .Select(p => new PosRolPermisoDto { RolId = r.Id, PermisoId = p.Id }))
                .DistinctBy(x => (x.RolId, x.PermisoId))
                .OrderBy(x => x.RolId).ThenBy(x => x.PermisoId)
                .ToList()
        };
    }

    public async Task<IReadOnlyList<PosUsuarioDto>> GetUsuariosAsync(Guid entidadId, Guid? sucursalId, CancellationToken cancellationToken)
    {
        var posRolIds = await _db.Rols.AsNoTracking().IgnoreQueryFilters()
            .Where(r => r.Codigo.StartsWith("POS_") && (r.EntidadId == null || r.EntidadId == entidadId))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
        var posRolIdSet = posRolIds.ToHashSet();

        var usuarioRoles = await _db.UsuarioRols.AsNoTracking().IgnoreQueryFilters()
            .Where(x => posRolIdSet.Contains(x.RolId))
            .Where(x => sucursalId == null || x.SucursalId == sucursalId)
            .ToListAsync(cancellationToken);
        var usuarioIdsEnAlcance = usuarioRoles.Select(x => x.UsuarioId).Distinct().ToHashSet();

        var usuarios = await _db.Usuarios.AsNoTracking().IgnoreQueryFilters()
            .Where(x => x.EntidadId == entidadId && usuarioIdsEnAlcance.Contains(x.Id))
            .OrderBy(x => x.NombreUsuario)
            .ToListAsync(cancellationToken);
        var usuarioIds = usuarios.Select(x => x.Id).ToHashSet();

        var rolesPorUsuario = usuarioRoles
            .Where(x => usuarioIds.Contains(x.UsuarioId))
            .GroupBy(x => x.UsuarioId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RolId).Distinct().OrderBy(i => i).ToList());

        return usuarios.Select(u => new PosUsuarioDto
        {
            Id = u.Id,
            EntidadId = u.EntidadId,
            SucursalId = u.SucursalId,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            Activo = u.Activo,
            RolIds = rolesPorUsuario.TryGetValue(u.Id, out var ids) ? ids : Array.Empty<int>()
        }).ToList();
    }

    public async Task<PosUsuarioDto?> GetUsuarioAsync(Guid entidadId, Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (usuario is null) return null;
        var rolIds = await _db.UsuarioRols.AsNoTracking().Where(x => x.UsuarioId == id)
            .Select(x => x.RolId).Distinct().OrderBy(i => i).ToListAsync(cancellationToken);
        return new PosUsuarioDto
        {
            Id = usuario.Id,
            EntidadId = usuario.EntidadId,
            SucursalId = usuario.SucursalId,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            Activo = usuario.Activo,
            RolIds = rolIds
        };
    }

    public async Task<(PosUsuarioDto? Usuario, string? Error)> CrearUsuarioAsync(Guid entidadId, CrearUsuarioPosRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.NombreCompleto) || string.IsNullOrEmpty(request.Password))
            return (null, "Nombre de usuario, nombre completo y contraseña son obligatorios.");
        if (request.Password.Length < 6) return (null, "La contraseña debe tener al menos 6 caracteres.");
        if (request.SucursalId == Guid.Empty) return (null, "La sucursal es obligatoria.");
        if (await _db.Usuarios.AnyAsync(x => x.NombreUsuario == request.NombreUsuario, cancellationToken)) return (null, "El nombre de usuario ya existe.");

        var sucursalValida = await _db.Sucursals.AnyAsync(x => x.Id == request.SucursalId && x.EntidadId == entidadId && x.Activo, cancellationToken);
        if (!sucursalValida) return (null, "La sucursal no existe o no pertenece a la entidad.");
        var rolIdsValidos = await ValidarRolIdsAsync(entidadId, request.RolIds, cancellationToken);
        if (rolIdsValidos is not null) return (null, rolIdsValidos);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = request.SucursalId,
            NombreUsuario = request.NombreUsuario.Trim(),
            NombreCompleto = request.NombreCompleto.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            HashPassword = BC.HashPassword(request.Password),
            DebeCambiarPass = false,
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        _db.Usuarios.Add(usuario);
        foreach (var rolId in request.RolIds.Distinct())
            _db.UsuarioRols.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rolId, SucursalId = request.SucursalId, AsignadoEn = DateTimeOffset.UtcNow });
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await _db.Usuarios.AnyAsync(x => x.NombreUsuario == request.NombreUsuario, cancellationToken)) return (null, "El nombre de usuario ya existe.");
            throw;
        }
        return (await GetUsuarioAsync(entidadId, usuario.Id, cancellationToken), null);
    }

    public async Task<(PosUsuarioDto? Usuario, string? Error)> ActualizarUsuarioAsync(Guid entidadId, Guid id, ActualizarUsuarioPosRequest request, Guid? actorId, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (usuario is null) return (null, "not_found");
        if (string.IsNullOrWhiteSpace(request.NombreCompleto)) return (null, "El nombre completo es obligatorio.");
        if (!request.Activo && actorId == id) return (null, "No puedes desactivar tu propio usuario.");
        if (request.SucursalId.HasValue && request.SucursalId != Guid.Empty &&
            !await _db.Sucursals.AnyAsync(x => x.Id == request.SucursalId.Value && x.EntidadId == entidadId, cancellationToken))
            return (null, "La sucursal no existe o no pertenece a la entidad.");

        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        usuario.Activo = request.Activo;
        usuario.ActualizadoEn = DateTimeOffset.UtcNow;
        if (request.SucursalId.HasValue && request.SucursalId != Guid.Empty) usuario.SucursalId = request.SucursalId;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetUsuarioAsync(entidadId, id, cancellationToken), null);
    }

    public async Task<(PosUsuarioDto? Usuario, string? Error)> EliminarUsuarioAsync(Guid entidadId, Guid id, Guid? actorId, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (usuario is null) return (null, "not_found");
        if (actorId == id) return (null, "No puedes eliminar tu propio usuario.");
        usuario.Activo = false;
        usuario.ActualizadoEn = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetUsuarioAsync(entidadId, id, cancellationToken), null);
    }

    public async Task<(bool Ok, string? Error)> AsignarRolesAsync(Guid entidadId, Guid id, AsignarRolesPosRequest request, Guid? actorId, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.AnyAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (!usuario) return (false, "not_found");
        if (request.SucursalId == Guid.Empty) return (false, "La sucursal es obligatoria.");
        var sucursalValida = await _db.Sucursals.AnyAsync(x => x.Id == request.SucursalId && x.EntidadId == entidadId, cancellationToken);
        if (!sucursalValida) return (false, "La sucursal no existe o no pertenece a la entidad.");
        var error = await ValidarRolIdsAsync(entidadId, request.RolIds, cancellationToken);
        if (error is not null) return (false, error);

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var existentes = await _db.UsuarioRols.Where(x => x.UsuarioId == id).ToListAsync(cancellationToken);
            _db.UsuarioRols.RemoveRange(existentes);
            foreach (var rolId in request.RolIds.Distinct())
                _db.UsuarioRols.Add(new UsuarioRol { UsuarioId = id, RolId = rolId, SucursalId = request.SucursalId, AsignadoEn = DateTimeOffset.UtcNow });
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> CambiarPasswordAsync(Guid entidadId, Guid id, CambiarPasswordPosRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (usuario is null) return (false, "not_found");
        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6) return (false, "La contraseña debe tener al menos 6 caracteres.");
        usuario.HashPassword = BC.HashPassword(request.NewPassword);
        usuario.DebeCambiarPass = false;
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<IReadOnlyList<PosRolDto>> GetRolesAsync(Guid entidadId, CancellationToken cancellationToken)
    {
        var roles = await _db.Rols.AsNoTracking()
            .Include(r => r.Permisos)
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);
        return roles.Select(r => new PosRolDto
        {
            Id = r.Id,
            Codigo = r.Codigo,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            EsSistema = r.EsSistema,
            PermisoIds = r.Permisos.Select(p => p.Id).Distinct().OrderBy(i => i).ToList()
        }).ToList();
    }

    public async Task<PosRolDto?> GetRolAsync(Guid entidadId, int id, CancellationToken cancellationToken)
    {
        var rol = await _db.Rols.AsNoTracking()
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rol is null) return null;
        return new PosRolDto
        {
            Id = rol.Id,
            Codigo = rol.Codigo,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            EsSistema = rol.EsSistema,
            PermisoIds = rol.Permisos.Select(p => p.Id).Distinct().OrderBy(i => i).ToList()
        };
    }

    public async Task<(PosRolDto? Rol, string? Error)> CrearRolAsync(Guid entidadId, CrearRolPosRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo) || string.IsNullOrWhiteSpace(request.Nombre))
            return (null, "Código y nombre del rol son obligatorios.");
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _db.Rols.AnyAsync(x => x.Codigo == codigo, cancellationToken)) return (null, "El código del rol ya existe.");
        var error = await ValidarPermisoIdsAsync(request.PermisoIds, cancellationToken);
        if (error is not null) return (null, error);

        var rol = new Rol
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            EsSistema = false,
            CreadoEn = DateTimeOffset.UtcNow,
            EntidadId = entidadId
        };
        _db.Rols.Add(rol);
        await _db.SaveChangesAsync(cancellationToken);
        var permisos = await _db.Permisos.Where(p => request.PermisoIds.Distinct().Contains(p.Id)).ToListAsync(cancellationToken);
        rol.Permisos = permisos;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetRolAsync(entidadId, rol.Id, cancellationToken), null);
    }

    public async Task<(PosRolDto? Rol, string? Error)> ActualizarRolAsync(Guid entidadId, int id, ActualizarRolPosRequest request, CancellationToken cancellationToken)
    {
        var rol = await _db.Rols.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (rol is null) return (null, "not_found");
        if (string.IsNullOrWhiteSpace(request.Nombre)) return (null, "El nombre del rol es obligatorio.");
        rol.Nombre = request.Nombre.Trim();
        rol.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetRolAsync(entidadId, id, cancellationToken), null);
    }

    public async Task<(bool Ok, string? Error)> EliminarRolAsync(Guid entidadId, int id, CancellationToken cancellationToken)
    {
        var rol = await _db.Rols.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (rol is null) return (false, "not_found");
        if (rol.EsSistema) return (false, "No se puede eliminar un rol de sistema.");
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [nucleo].[rol_permiso] WHERE [rol_id] = {0}", new object[] { id }, cancellationToken);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [nucleo].[usuario_rol] WHERE [rol_id] = {0}", new object[] { id }, cancellationToken);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [nucleo].[rol] WHERE [id] = {0}", new object[] { id }, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
        return (true, null);
    }

    public async Task<(PosRolDto? Rol, string? Error)> AsignarPermisosAsync(Guid entidadId, int id, AsignarPermisosPosRequest request, CancellationToken cancellationToken)
    {
        var rol = await _db.Rols.FirstOrDefaultAsync(x => x.Id == id && x.EntidadId == entidadId, cancellationToken);
        if (rol is null) return (null, "not_found");
        if (rol.EsSistema) return (null, "No se pueden modificar los permisos de un rol de sistema.");
        var error = await ValidarPermisoIdsAsync(request.PermisoIds, cancellationToken);
        if (error is not null) return (null, error);

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var permisos = await _db.Permisos.Where(p => request.PermisoIds.Distinct().Contains(p.Id)).ToListAsync(cancellationToken);
            rol.Permisos = permisos;
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
        return (await GetRolAsync(entidadId, id, cancellationToken), null);
    }

    private async Task<string?> ValidarRolIdsAsync(Guid entidadId, IReadOnlyList<int> rolIds, CancellationToken cancellationToken)
    {
        var distinct = rolIds.Distinct().ToList();
        var existentes = await _db.Rols.AsNoTracking()
            .Where(x => distinct.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        return distinct.Count == existentes.Count ? null : "Uno o más roles no existen o no pertenecen a la entidad.";
    }

    private async Task<string?> ValidarPermisoIdsAsync(IReadOnlyList<int> permisoIds, CancellationToken cancellationToken)
    {
        var distinct = permisoIds.Distinct().ToList();
        var existentes = await _db.Permisos.AsNoTracking().Where(x => distinct.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        return distinct.Count == existentes.Count ? null : "Uno o más permisos no existen.";
    }
}