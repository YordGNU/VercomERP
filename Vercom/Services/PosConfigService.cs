using Microsoft.EntityFrameworkCore;
using Vercom.DTOs;
using Vercom.Models;

namespace Vercom.Services;

public sealed class PosConfigService
{
    private readonly AppDbContext _db;

    public PosConfigService(AppDbContext db) => _db = db;

    /// <summary>
    /// Devuelve la configuración completa de la terminal (entidad, sucursal, almacén, dispositivo,
    /// tipo de movimiento de venta y cliente mostrador) a partir del identificador de hardware (MAC WiFi)
    /// registrado en el sistema.
    /// </summary>
    public async Task<DispositivoConfigDto?> GetConfiguracionAsync(string identificadorHardware, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identificadorHardware)) return null;

        // El endpoint es anónimo (no hay token) => CurrentEntidadId = Guid.Empty.
        // IgnoreQueryFilters es obligatorio o la consulta no devolvería filas.
        var mac = NormalizeMac(identificadorHardware);
        var device = await _db.DispositivoPos.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => (x.Estado == "ACTIVO" || x.Estado == "ONLINE")
                && x.IdentificadorHardware != null
                && x.IdentificadorHardware.ToUpper().Replace(":", "").Replace("-", "").Replace(".", "").Replace(" ", "") == mac,
                cancellationToken);
        if (device is null) return null;

        var entidad = await _db.Entidads.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == device.EntidadId && x.Activo, cancellationToken);
        if (entidad is null) return null;

        var sucursal = await _db.Sucursals.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == device.SucursalId && x.Activo, cancellationToken);

        var almacen = await _db.Almacens.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == device.AlmacenId && x.Activo, cancellationToken);

        // Tipo de movimiento de venta: preferir el explícito (VENTA / VENTA_POS), con respaldo al primero.
        var tipoMovimiento = await _db.TipoMovimientos.AsNoTracking()
            .Where(x => x.Codigo == "VENTA" || x.Codigo == "VENTA_POS")
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _db.TipoMovimientos.AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var clienteCF = await _db.Clientes.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.EntidadId == device.EntidadId && x.Activo && x.NitOCi == null)
            .OrderBy(x => x.NombreRazonSocial)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _db.Clientes.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.EntidadId == device.EntidadId && x.Activo)
                .OrderBy(x => x.NombreRazonSocial)
                .FirstOrDefaultAsync(cancellationToken);

        return new DispositivoConfigDto
        {
            EntidadId = device.EntidadId,
            EntidadNombre = entidad.RazonSocial,
            EntidadNit = entidad.Nit,
            EntidadDireccion = entidad.DireccionLegal,
            EntidadTelefono = entidad.Telefono ?? string.Empty,
            EntidadEmail = entidad.Email,
            MonedaBase = string.IsNullOrWhiteSpace(entidad.MonedaBase) ? "CUP" : entidad.MonedaBase,
            SucursalId = sucursal?.Id ?? device.SucursalId,
            SucursalNombre = sucursal?.Nombre ?? string.Empty,
            AlmacenId = almacen?.Id ?? device.AlmacenId,
            AlmacenCodigo = almacen?.Codigo ?? string.Empty,
            AlmacenNombre = almacen?.Nombre ?? string.Empty,
            DispositivoPosId = device.Id,
            DispositivoCodigo = device.Codigo,
            DispositivoNombre = device.Nombre,
            TipoMovimientoVenta = tipoMovimiento?.Id ?? 1,
            ClienteConsumidorFinalId = clienteCF?.Id ?? Guid.Empty
        };
    }

    private static string NormalizeMac(string mac) =>
        new string(mac.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}