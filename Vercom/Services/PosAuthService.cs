using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vercom.DTOs;
using Vercom.Models;
using BC = BCrypt.Net.BCrypt;

namespace Vercom.Services;

public sealed class PosAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public PosAuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<(PosLoginResponse? Response, string? Error)> LoginAsync(PosLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return (null, "Usuario y contraseña son obligatorios.");

        var user = await _db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(x =>
            x.Activo && (x.NombreUsuario == request.Username || x.Email == request.Username), cancellationToken);
        if (user is null) return (null, "Credenciales inválidas.");

        bool passwordValid;
        try
        {
            passwordValid = BC.Verify(request.Password, user.HashPassword);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            passwordValid = false;
        }

        if (!passwordValid) return (null, "Credenciales inválidas.");

        var assignments = await _db.UsuarioRols.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.UsuarioId == user.Id)
            .Where(x => !request.SucursalId.HasValue || x.SucursalId == request.SucursalId.Value)
            .Include(x => x.Rol)
            .ToListAsync(cancellationToken);
        if (request.SucursalId.HasValue && assignments.Count == 0)
            return (null, "El usuario no tiene acceso a la sucursal solicitada.");

        var sucursalId = request.SucursalId ?? user.SucursalId ?? assignments.Select(x => (Guid?)x.SucursalId).FirstOrDefault();
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
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreUsuario),
            new("FullName", user.NombreCompleto),
            new("EntidadId", user.EntidadId.ToString())
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