using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace VERCOM.Services;

/// <summary>
/// Valida en cada petición que un token POS siga correspondiendo a una cuenta utilizable:
/// el usuario existe, sigue activo y su contraseña no cambió desde que se emitió el token.
/// Sin esto un token de 60 minutos sobrevive a la desactivación del usuario o al cambio de clave.
/// </summary>
public sealed class PosTokenValidator
{
    public const string PasswordStampClaim = "pwdstamp";

    // 16 caracteres hexadecimales de SHA-256: suficiente como huella no reversible
    // del hash BCrypt y barato de recalcular en cada emisión.
    private const int LongitudHuella = 16;

    private readonly AppDbContext _db;

    public PosTokenValidator(AppDbContext db) => _db = db;

    public static string Stamp(string? hashPassword)
    {
        if (string.IsNullOrEmpty(hashPassword)) return string.Empty;
        var sha = SHA256.HashData(Encoding.UTF8.GetBytes(hashPassword));
        return Convert.ToHexString(sha)[..LongitudHuella];
    }

    public async Task<bool> IsValidAsync(Guid usuarioId, string? huella, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty || string.IsNullOrWhiteSpace(huella)) return false;

        var cuenta = await _db.Usuarios.IgnoreQueryFilters().AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => new { u.Activo, u.HashPassword })
            .FirstOrDefaultAsync(cancellationToken);

        if (cuenta is null || !cuenta.Activo) return false;

        var esperada = Stamp(cuenta.HashPassword);
        return esperada.Length > 0 && string.Equals(esperada, huella, StringComparison.OrdinalIgnoreCase);
    }
}
