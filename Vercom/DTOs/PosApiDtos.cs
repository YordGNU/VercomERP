using System.Text.Json;

namespace Vercom.DTOs;

public sealed class PosLoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid? SucursalId { get; init; }
}

public sealed class PosLoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public Guid EntidadId { get; init; }
    public Guid? SucursalId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Permisos { get; init; } = Array.Empty<string>();
}

public sealed class PosProductoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string? CodigoBarras { get; init; }
    public string Nombre { get; init; } = null!;
    public string? Descripcion { get; init; }
    public Guid? FamiliaId { get; init; }
    public int UnidadMedidaId { get; init; }
    public decimal? PrecioVentaActual { get; init; }
    public bool AplicaImpuestoVentas { get; init; }
    public decimal Stock { get; init; }
    public decimal StockMinimo { get; init; }
    public DateTimeOffset ActualizadoEn { get; init; }
}

public sealed class PosDispositivoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string Nombre { get; init; } = null!;
    public Guid SucursalId { get; init; }
    public Guid AlmacenId { get; init; }
    public string? AlmacenNombre { get; init; }
}

public sealed class AbrirSesionCajaPosRequest
{
    public Guid DispositivoPosId { get; init; }
    public Guid CajeroId { get; init; }
    public decimal MontoApertura { get; init; }
}

public sealed class CerrarSesionCajaPosRequest
{
    public decimal MontoCierreDeclarado { get; init; }
}

public sealed class SesionCajaPosDto
{
    public Guid Id { get; init; }
    public Guid CajaId { get; init; }
    public Guid DispositivoPosId { get; init; }
    public Guid CajeroId { get; init; }
    public DateTimeOffset FechaApertura { get; init; }
    public DateTimeOffset? FechaCierre { get; init; }
    public decimal MontoApertura { get; init; }
    public decimal? MontoCierreDeclarado { get; init; }
    public decimal? MontoCierreSistema { get; init; }
    public decimal TotalVentas { get; init; }
    public decimal TotalEfectivo { get; init; }
    public int CantidadFacturas { get; init; }
    public string Estado { get; init; } = string.Empty;
}

public sealed class RecibirVentaPosRequest
{
    public Guid DispositivoPosId { get; init; }
    public Guid SesionCajaPosId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTimeOffset FechaVentaLocal { get; init; }
    public JsonElement Venta { get; init; }
}

public sealed class VentaPosPendienteDto
{
    public Guid Id { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public Guid? FacturaId { get; init; }
    public string? MensajeError { get; init; }
}

public sealed class VentaPosPayload
{
    public Guid EntidadId { get; init; }
    public Guid SucursalId { get; init; }
    public Guid ClienteId { get; init; }
    public Guid AlmacenId { get; init; }
    public int TipoMovimientoId { get; init; }
    public string NumeroFactura { get; init; } = string.Empty;
    public string Serie { get; init; } = "A";
    public DateTimeOffset Fecha { get; init; }
    public string Moneda { get; init; } = "CUP";
    public List<VentaPosLineaPayload> Lineas { get; init; } = new();
    public List<VentaPosPagoPayload> Pagos { get; init; } = new();
}

public sealed class VentaPosLineaPayload
{
    public Guid ProductoId { get; init; }
    public decimal Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal DescuentoPorcentaje { get; init; }
    public decimal ImpuestoPorcentaje { get; init; }
}

public sealed class VentaPosPagoPayload
{
    public string FormaPago { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string? ReferenciaExterna { get; init; }
    public decimal? VueltoEntregado { get; init; }
}

public sealed class PosPermisoDto
{
    public int Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string Modulo { get; init; } = null!;
    public string Descripcion { get; init; } = null!;
}

public sealed class PosRolDto
{
    public int Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string Nombre { get; init; } = null!;
    public string? Descripcion { get; init; }
    public bool EsSistema { get; init; }
    public IReadOnlyList<int> PermisoIds { get; init; } = Array.Empty<int>();
}

public sealed class PosUsuarioDto
{
    public Guid Id { get; init; }
    public Guid EntidadId { get; init; }
    public Guid? SucursalId { get; init; }
    public string NombreUsuario { get; init; } = null!;
    public string NombreCompleto { get; init; } = null!;
    public string? Email { get; init; }
    public bool Activo { get; init; }
    public IReadOnlyList<int> RolIds { get; init; } = Array.Empty<int>();
}

public sealed class PosUsuarioRolDto
{
    public Guid UsuarioId { get; init; }
    public int RolId { get; init; }
    public Guid SucursalId { get; init; }
}

public sealed class PosRolPermisoDto
{
    public int RolId { get; init; }
    public int PermisoId { get; init; }
}

public sealed class PosRbacSnapshotDto
{
    public IReadOnlyList<PosRolDto> Roles { get; init; } = Array.Empty<PosRolDto>();
    public IReadOnlyList<PosPermisoDto> Permisos { get; init; } = Array.Empty<PosPermisoDto>();
    public IReadOnlyList<PosUsuarioDto> Usuarios { get; init; } = Array.Empty<PosUsuarioDto>();
    public IReadOnlyList<PosUsuarioRolDto> UsuarioRoles { get; init; } = Array.Empty<PosUsuarioRolDto>();
    public IReadOnlyList<PosRolPermisoDto> RolPermisos { get; init; } = Array.Empty<PosRolPermisoDto>();
}

public sealed class DispositivoConfigRequest
{
    public string IdentificadorHardware { get; init; } = string.Empty;
}

public sealed class DispositivoConfigDto
{
    public Guid EntidadId { get; init; }
    public string EntidadNombre { get; init; } = string.Empty;
    public string EntidadNit { get; init; } = string.Empty;
    public string EntidadDireccion { get; init; } = string.Empty;
    public string EntidadTelefono { get; init; } = string.Empty;
    public string? EntidadEmail { get; init; }
    public string MonedaBase { get; init; } = "CUP";
    public Guid SucursalId { get; init; }
    public string SucursalNombre { get; init; } = string.Empty;
    public Guid AlmacenId { get; init; }
    public string AlmacenCodigo { get; init; } = string.Empty;
    public string AlmacenNombre { get; init; } = string.Empty;
    public Guid DispositivoPosId { get; init; }
    public string DispositivoCodigo { get; init; } = string.Empty;
    public string DispositivoNombre { get; init; } = string.Empty;
    public int TipoMovimientoVenta { get; init; }
    public Guid ClienteConsumidorFinalId { get; init; }
}

public sealed class CrearUsuarioPosRequest
{
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Password { get; init; } = string.Empty;
    public Guid SucursalId { get; init; }
    public IReadOnlyList<int> RolIds { get; init; } = Array.Empty<int>();
}

public sealed class ActualizarUsuarioPosRequest
{
    public string NombreCompleto { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool Activo { get; init; }
    public Guid? SucursalId { get; init; }
}

public sealed class AsignarRolesPosRequest
{
    public Guid SucursalId { get; init; }
    public IReadOnlyList<int> RolIds { get; init; } = Array.Empty<int>();
}

public sealed class CambiarPasswordPosRequest
{
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class CrearRolPosRequest
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public IReadOnlyList<int> PermisoIds { get; init; } = Array.Empty<int>();
}

public sealed class ActualizarRolPosRequest
{
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}

public sealed class AsignarPermisosPosRequest
{
public IReadOnlyList<int> PermisoIds { get; init; } = Array.Empty<int>();
}

public sealed class PosCatalogoProductoDto
{
    public Guid serverId { get; init; }
    public string cod { get; init; } = string.Empty;
    public string nombre { get; init; } = string.Empty;
    public decimal precio { get; init; }
    public string origenPrecio { get; init; } = "SIN_PRECIO";
    public string unidad { get; init; } = string.Empty;
    public decimal existencias { get; init; }
}
