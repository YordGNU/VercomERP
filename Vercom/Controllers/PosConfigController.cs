using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.DTOs;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Route("api/pos/config")]
public sealed class PosConfigController : ControllerBase
{
    private readonly PosConfigService _service;

    public PosConfigController(PosConfigService service) => _service = service;

    /// <summary>
    /// Auto-configuración de terminal POS: dado el identificador de hardware (MAC WiFi) registrado,
    /// devuelve entidad, sucursal, almacén, dispositivo, tipo de movimiento de venta y cliente mostrador.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("dispositivo")]
    public async Task<ActionResult<DispositivoConfigDto>> GetConfiguracion([FromBody] DispositivoConfigRequest request, CancellationToken cancellationToken = default)
    {
        var config = await _service.GetConfiguracionAsync(request.IdentificadorHardware, cancellationToken);
        return config is null
            ? NotFound(new { message = "El dispositivo POS no está registrado en el sistema o se encuentra inactivo." })
            : Ok(config);
    }
}