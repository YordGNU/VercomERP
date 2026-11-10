using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VERCOM.DTOs;
using VERCOM.Models;
using BC = BCrypt.Net.BCrypt;

namespace VERCOM.Services;

public sealed class PosAuthService
{
    private const string MensajeCredenciales = "Credenciales inválidas.";
    private const string MensajeBloqueo = "La cuenta está bloqueada temporalmente por intentos fallidos. Reintente más tarde.";

    // Hash de descarte: se verifica cuando el usuario no existe para que el tiempo de
    // respuesta no delate qué nombres de usuario son válidos.
    private static readonly string HashDeDescarte = BC.HashPassword(Guid.NewGuid().ToString("N"));

    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public PosAuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    private int MaxIntentos => Math.Max(1, _configuration.GetValue("Pos:MaxLoginAttempts", 5));
    private int MinutosBloqueo => Math.Max(1, _configuration.GetValue("Pos:LockoutMinutes", 15));

    public async Task<(PosLoginResponse? Response, string? Error)> LoginAsync(PosLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return (null, "Usuario y contraseña son obligatorios.");

        var user = await _db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(x =>
            x.Activo && (x.NombreUsuario == request.Username || x.Email == request.Username), cancellationToken);

        if (user is null)
        {
            BC.Verify(request.Password, HashDeDescarte);
            return (null, MensajeCredenciales);
        }

        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTimeOffset.UtcNow)
            return (null, MensajeBloqueo);

        bool passwordValid;
        try
        {
            passwordValid = BC.Verify(request.Password, user.HashPassword);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            passwordValid = false;
        }

        if (!passwordValid)
        {
            user.IntentosFallidos++;
            if (user.IntentosFallidos >= MaxIntentos)
            {
                user.BloqueadoHasta = DateTimeOffset.UtcNow.AddMinutes(MinutosBloqueo);
                user.IntentosFallidos = 0;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return (null, MensajeCredenciales);
        }

        // Login válido: se limpian bloqueo e intentos y se registra la última conexión.
        user.IntentosFallidos = 0;
        user.BloqueadoHasta = null;
        user.UltimoLogin = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return await IssueAsync(user, request.SucursalId, cancellationToken);
    }

    /// <summary>
    /// Emite un token POS para un usuario ya autenticado por otro medio (p. ej. la sesión
    /// de cookie del terminal web del ERP), sin volver a pedir la contraseña.
    /// </summary>
    public async Task<(PosLoginResponse? Response, string? Error)> IssueForUserAsync(Guid userId, Guid? sucursalId, CancellationToken cancellationToken)
    {
        var user = await _db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken);
        return user is null
            ? (null, "El usuario no está activo.")
            : await IssueAsync(user, sucursalId, cancellationToken);
    }

    private async Task<(PosLoginResponse? Response, string? Error)> IssueAsync(Usuario user, Guid? sucursalSolicitada, CancellationToken cancellationToken)
    {
        var assignments = await _db.UsuarioRols.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.UsuarioId == user.Id)
            .Where(x => !sucursalSolicitada.HasValue || x.SucursalId == sucursalSolicitada.Value)
            .Include(x => x.Rol)
            .ToListAsync(cancellationToken);
        if (sucursalSolicitada.HasValue && assignments.Count == 0)
            return (null, "El usuario no tiene acceso a la sucursal solicitada.");

        var sucursalId = sucursalSolicitada ?? user.SucursalId ?? assignments.Select(x => (Guid?)x.SucursalId).FirstOrDefault();
        var roles = assignments.Select(x => string.IsNullOrWhiteSpace(x.Rol.Codigo) ? x.Rol.Nombre : x.Rol.Codigo)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var rolIds = assignments.Select(x => x.RolId).Distinct().ToList();
        var permisos = await _db.Rols.AsNoTracking()
            .Where(r => rolIds.Contains(r.Id))
            .SelectMany(r => r.Permisos)
            .Select(p => p.Codigo)
            .Distinct().OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpirationMinutes", 60));
        var token = CreateToken(user, sucursalId, roles, permisos, expiresAt);

        return (new PosLoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Username = user.NombreUsuario,
            NombreCompleto = user.NombreCompleto,
            EntidadId = user.EntidadId,
            SucursalId = sucursalId,
            Roles = roles,
            Permisos = permisos
        }, null);
    }

    private string CreateToken(Usuario user, Guid? sucursalId, IReadOnlyList<string> roles, IReadOnlyList<string> permisos, DateTimeOffset expiresAt)
    {
        var secret = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Jwt:SecretKey debe tener al menos 32 bytes.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreUsuario),
            new("FullName", user.NombreCompleto),
            new("EntidadId", user.EntidadId.ToString()),
            // Huella de la contraseña vigente: permite invalidar tokens ya emitidos
            // cuando el usuario cambia su clave (ver PosTokenValidator).
            new(PosTokenValidator.PasswordStampClaim, PosTokenValidator.Stamp(user.HashPassword))
        };
        if (sucursalId.HasValue) claims.Add(new Claim("SucursalId", sucursalId.Value.ToString()));
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var permiso in permisos) claims.Add(new Claim("Permission", permiso));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}