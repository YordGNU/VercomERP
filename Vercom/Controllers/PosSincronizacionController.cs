using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.DTOs;
using VERCOM.Services;

namespace VERCOM.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/pos/sincronizacion")]
public sealed class PosSincronizacionController : ControllerBase
{
    private readonly PosSincronizacionService _service;

    public PosSincronizacionController(PosSincronizacionService service) => _service = service;

    // Mismo criterio que PermissionHandler: MASTER y ADMINISTRADOR tienen acceso total.
    private bool Puede(string codigo) =>
        User.IsInRole("MASTER") || User.IsInRole("ADMINISTRADOR") || User.HasClaim("Permission", codigo);

    private static bool TieneDescuento(JsonElement venta)
    {
        if (venta.ValueKind != JsonValueKind.Object) return false;

        foreach (var prop in venta.EnumerateObject())
        {
            if (!string.Equals(prop.Name, "lineas", StringComparison.OrdinalIgnoreCase) ||
                prop.Value.ValueKind != JsonValueKind.Array) continue;

            foreach (var linea in prop.Value.EnumerateArray())
            {
                if (linea.ValueKind != JsonValueKind.Object) continue;
                foreach (var campo in linea.EnumerateObject())
                {
                    if (!string.Equals(campo.Name, "descuentoPorcentaje", StringComparison.OrdinalIgnoreCase)) continue;
                    if (campo.Value.ValueKind == JsonValueKind.Number && campo.Value.GetDecimal() > 0) return true;
                }
            }
        }
        return false;
    }

    [HttpGet("siguiente-numero")]
    public async Task<ActionResult> SiguienteNumero([FromQuery] Guid dispositivoPosId, [FromQuery] string serie = "A", CancellationToken cancellationToken = default)
    {
        var result = await _service.SiguienteNumeroAsync(dispositivoPosId, serie, cancellationToken);
        if (result.Error is not null) return BadRequest(new { message = result.Error });
        return Ok(new { numeroFactura = result.Numero, serie });
    }

    [HttpPost("ventas")]
    public async Task<ActionResult<VentaPosPendienteDto>> Recibir([FromBody] RecibirVentaPosRequest request, CancellationToken cancellationToken = default)
    {
        if (TieneDescuento(request.Venta) && !Puede("aplicar_descuento"))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tiene permiso para aplicar descuentos." });

        var result = await _service.RecibirAsync(request, cancellationToken);
        if (result.Error is not null) return result.Conflict ? Conflict(result.Error) : BadRequest(result.Error);
        return result.Accepted ? Accepted(result.Result) : Ok(result.Result);
    }

    [HttpGet("facturas")]
    public async Task<ActionResult<FacturaPosDto>> Factura([FromQuery] Guid dispositivoPosId, [FromQuery] string serie, [FromQuery] string numero, CancellationToken cancellationToken = default)
    {
        var result = await _service.ObtenerFacturaAsync(dispositivoPosId, serie, numero, cancellationToken);
        if (result.Error is not null) return BadRequest(new { message = result.Error });
        return Ok(result.Result);
    }

    [HttpPost("devoluciones")]
    public async Task<ActionResult<DevolucionPosResultDto>> Devolucion([FromBody] DevolucionPosRequest request, CancellationToken cancellationToken = default)
    {
        if (!Puede("procesar_devolucion"))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tiene permiso para procesar devoluciones." });

        var result = await _service.ProcesarDevolucionAsync(request, cancellationToken);
        if (result.Error is not null) return BadRequest(new { message = result.Error });
        return Ok(result.Result);
    }

    [HttpGet("ventas/{dispositivoPosId:guid}/{idempotencyKey}")]
    public async Task<ActionResult<VentaPosPendienteDto>> Estado(Guid dispositivoPosId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await _service.EstadoAsync(dispositivoPosId, idempotencyKey, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("ventas/{dispositivoPosId:guid}/{idempotencyKey}/procesar")]
    public async Task<ActionResult<VentaPosPendienteDto>> Procesar(Guid dispositivoPosId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await _service.ProcesarAsync(dispositivoPosId, idempotencyKey, cancellationToken);
        if (result.Error is not null) return result.Conflict ? Conflict(result.Error) : BadRequest(result.Error);
        if (result.Result is null) return NotFound();
        return result.Conflict ? Conflict(result.Result) : Ok(result.Result);
    }
}