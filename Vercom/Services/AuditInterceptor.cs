using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using Vercom.Models;

namespace Vercom.Services;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            OnBeforeSaveChanges(context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void OnBeforeSaveChanges(AppDbContext context)
    {
        context.ChangeTracker.DetectChanges();
        var auditEntries = new List<Auditorium>();

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is Auditorium || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new Auditorium
            {
                UsuarioId = userId,
                NombreUsuario = userName,
                OcurridoEn = DateTime.Now,
                EsquemaTabla = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                IpOrigen = ipAddress,
                Canal = "ERP"
            };

            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey != null)
            {
                var keyValues = primaryKey.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToList();
                auditEntry.RegistroId = string.Join(",", keyValues);
            }

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.Accion = "INSERT";
                    foreach (var property in entry.Properties)
                    {
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    break;
                case EntityState.Deleted:
                    auditEntry.Accion = "DELETE";
                    foreach (var property in entry.Properties)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                    }
                    break;
                case EntityState.Modified:
                    auditEntry.Accion = "UPDATE";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            oldValues[property.Metadata.Name] = property.OriginalValue;
                            newValues[property.Metadata.Name] = property.CurrentValue;
                        }
                    }
                    break;
            }

            auditEntry.ValoresAnteriores = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            auditEntry.ValoresNuevos = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Count > 0)
        {
            context.Auditoria.AddRange(auditEntries);
        }
    }

    private Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idClaim, out var guid)) return guid;
        }
        return null;
    }

    private string GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "SISTEMA";
    }
}
