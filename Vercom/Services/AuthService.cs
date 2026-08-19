using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vercom.Models;
using BC = BCrypt.Net.BCrypt;

namespace Vercom.Services;

public interface IAuthService
{
    Task<(bool Succeeded, string Message, bool MustChangePassword)> LoginAsync(string username, string password, bool rememberMe);
    Task LogoutAsync();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    Task<(bool Succeeded, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool Succeeded, string Message, bool MustChangePassword)> LoginAsync(string username, string password, bool rememberMe)
    {
        var usuario = await _context.Usuarios
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
            new Claim("MustChangePassword", usuario.DebeCambiarPass.ToString())
        };

        foreach (var ur in usuario.UsuarioRolUsuarios)
        {
            claims.Add(new Claim(ClaimTypes.Role, ur.Rol.Codigo));
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
