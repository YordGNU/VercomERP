using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VERCOM.Models;
using VERCOM.Security;
using Xunit;

namespace Vercom.Tests.Infrastructure;

/// <summary>
/// Implementación mínima de <see cref="IEntidadProvider"/> para tests.
/// Con <c>isMaster=true</c> el filtro global multi-entidad deja de filtrar
/// (AppDbContext aplica <c>IsMaster || EntidadId == CurrentEntidadId</c>).
/// </summary>
public sealed class TestEntidadProvider : IEntidadProvider
{
    public TestEntidadProvider(Guid entidadId, Guid usuarioId, bool isMaster = false)
    {
        CurrentEntidadId = entidadId;
        CurrentUsuarioId = usuarioId;
        IsMaster = isMaster;
    }

    public Guid CurrentEntidadId { get; }
    public Guid CurrentUsuarioId { get; }
    public Guid? CurrentSucursalId => null;
    public bool IsMaster { get; }
}

/// <summary>
/// Fábrica de <see cref="AppDbContext"/> contra la BD de desarrollo.
/// La cadena de conexión se toma de la variable de entorno
/// <c>VERCOM_TEST_CONNECTION</c>; si no existe, se lee
/// <c>ConnectionStrings:DefaultConnection</c> del appsettings.json del proyecto
/// (evita duplicar credenciales en el código de tests).
/// Si no hay BD disponible, los tests marcados con <see cref="DbFactAttribute"/>
/// se saltan en lugar de fallar.
/// </summary>
public static class TestDb
{
    public static readonly Guid EntidadIdSeed = Guid.Parse("24c50c75-26a7-4bad-a81d-c4d680f4cdf6");
    public static readonly Guid UsuarioIdSeed = Guid.Parse("3933ad33-9e58-49d7-9a52-9f33a87eacf6");

    private static readonly Lazy<string?> ConnectionStringLazy = new(ResolveConnectionString);
    private static readonly Lazy<bool> AvailableLazy = new(Probe);

    public static string? ConnectionString => ConnectionStringLazy.Value;

    public static bool IsAvailable => AvailableLazy.Value;

    public static AppDbContext Create(bool isMaster = true, Guid? entidadId = null, Guid? usuarioId = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var provider = new TestEntidadProvider(
            entidadId ?? EntidadIdSeed,
            usuarioId ?? UsuarioIdSeed,
            isMaster);

        return new AppDbContext(options, provider);
    }

    /// <summary>Ejecuta un bloque dentro de una transacción y siempre revierte.</summary>
    public static async Task EnTransaccionAsync(Func<AppDbContext, Task> cuerpo)
    {
        await using var db = Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await cuerpo(db);
        }
        finally
        {
            await tx.RollbackAsync();
        }
    }

    /// <summary>
    /// Comprueba que la BD sembrada tenga los datos base que necesitan los
    /// fixtures que insertan filas con FK reales (sucursal, almacén, cuenta
    /// contable y usuario).
    /// </summary>
    public static bool HayDatosBase()
    {
        if (!IsAvailable) return false;

        try
        {
            using var db = Create();
            return db.Sucursals.Any() && db.Almacens.Any() && db.CuentaContables.Any() && db.Usuarios.Any();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string? ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("VERCOM_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        var appSettings = FindAppSettings();
        if (appSettings is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(appSettings));
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                cs.TryGetProperty("DefaultConnection", out var def))
            {
                return def.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? FindAppSettings()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            foreach (var candidate in new[]
                     {
                         Path.Combine(dir.FullName, "appsettings.json"),
                         Path.Combine(dir.FullName, "Vercom", "appsettings.json")
                     })
            {
                if (File.Exists(candidate))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(candidate));
                    if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                        cs.TryGetProperty("DefaultConnection", out _))
                    {
                        return candidate;
                    }
                }
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static bool Probe()
    {
        var cs = ConnectionString;
        if (string.IsNullOrWhiteSpace(cs)) return false;

        var builder = new SqlConnectionStringBuilder(cs) { ConnectTimeout = 5 };

        try
        {
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.ExecuteScalar();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

/// <summary>
/// <c>[Fact]</c> que se marca como omitido cuando no hay SQL Server accesible,
/// para que la suite siga siendo ejecutable sin BD (p. ej. en CI sin motor).
/// </summary>
public sealed class DbFactAttribute : FactAttribute
{
    public DbFactAttribute()
    {
        if (!TestDb.IsAvailable)
        {
            Skip = "SQL Server no disponible. Defina VERCOM_TEST_CONNECTION o levante la BD de desarrollo.";
        }
    }

    /// <summary>
    /// Marque el test con <c>true</c> cuando además necesite los datos base
    /// sembrados (sucursal/almacén/cuenta contable/usuario).
    /// </summary>
    public bool RequiereDatosBase
    {
        get => _requiereDatosBase;
        set
        {
            _requiereDatosBase = value;
            if (value && Skip is null && !TestDb.HayDatosBase())
            {
                Skip = "La BD no tiene datos base (sucursal/almacén/cuenta contable/usuario) para el fixture.";
            }
        }
    }

    private bool _requiereDatosBase;
}
