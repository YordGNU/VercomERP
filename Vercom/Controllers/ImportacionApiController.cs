using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Security;
using Vercom.Services;

namespace Vercom.Controllers;

[ApiController]
[Route("api/importacion")]
[Authorize(Policy = "INVENTARIO.PRODUCTO.VER")]
public sealed class ImportacionApiController : ControllerBase
{
    private readonly IImportacionService _service;
    private readonly IEntidadProvider _entidadProvider;

    public ImportacionApiController(IImportacionService service, IEntidadProvider entidadProvider)
    {
        _service = service;
        _entidadProvider = entidadProvider;
    }

    [HttpPost("test-conexion")]
    public async Task<ActionResult> TestConexion([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _service.TestConexionAsync(request);
        return Ok(resultado);
    }

    [HttpPost("resumen")]
    public async Task<ActionResult<ImportacionResumen>> ObtenerResumen([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        var resumen = await _service.ObtenerResumenAsync(request);
        return Ok(resumen);
    }

    [HttpPost("productos")]
    public async Task<ActionResult<List<ProductoVersatDto>>> ObtenerProductos([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        var productos = await _service.ObtenerProductosAsync(request, cancellationToken);
        return Ok(productos);
    }

    [HttpPost("existencias")]
    public async Task<ActionResult<List<ExistenciaVersatDto>>> ObtenerExistencias([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        var existencias = await _service.ObtenerExistenciasAsync(request, cancellationToken);
        return Ok(existencias);
    }

    [HttpPost("almacenes")]
    public async Task<ActionResult<List<AlmacenVersatDto>>> ObtenerAlmacenes([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        var almacenes = await _service.ObtenerAlmacenesAsync(request, cancellationToken);
        return Ok(almacenes);
    }

    [HttpPost("ejecutar")]
    public async Task<ActionResult<ImportacionResultado>> Importar([FromBody] ConexionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entidadId = _entidadProvider.CurrentEntidadId;
            var sucursalId = _entidadProvider.CurrentSucursalId;
            var resultado = await _service.ImportarAsync(entidadId, sucursalId, request, cancellationToken);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
