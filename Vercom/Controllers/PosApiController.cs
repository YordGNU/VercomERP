using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.DTOs;
using Vercom.Services;
using System.Text.Json;

namespace Vercom.Controllers;

[Route("api")]
[ApiController]
public class PosApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly ISalesService _salesService;
    private readonly IConsecutivoService _consecutivoService;

    public PosApiController(AppDbContext context, IAuthService authService, ISalesService salesService, IConsecutivoService consecutivoService)
    {
        _context = context;
        _authService = authService;
        _salesService = salesService;
        _consecutivoService = consecutivoService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Dictionary<string, string> credentials)
    {
        if (!credentials.TryGetValue("username", out var username) ||
            !credentials.TryGetValue("password", out var password))
        {
            return BadRequest(new { message = "Credenciales requeridas" });
        }

        var result = await _authService.LoginAsync(username, password, false);
        if (result.Succeeded)
        {
            return Ok(new { token = "auth-active", message = result.Message });
        }

        return Unauthorized(new { message = result.Message });
    }

    [HttpGet("pos/reserva-rango")]
    public async Task<IActionResult> ReservarRango([FromQuery] Guid dispositivoId, [FromQuery] string serie = "POS", [FromQuery] int cantidad = 100)
    {
        var dispositivo = await _context.DispositivoPos.FindAsync(dispositivoId);
        if (dispositivo == null) return NotFound("Dispositivo no registrado");

        // RNF-51: Reservar bloque de números para operación offline
        var primerNumero = await _consecutivoService.ObtenerSiguienteNumeroAsync(
            dispositivo.EntidadId, dispositivo.SucursalId, "FACTURA_VENTA", serie);

        long start = long.Parse(primerNumero);
        long end = start + cantidad - 1;

        // Actualizar el contador del sistema para saltar el bloque reservado
        var consecutivo = await _context.Consecutivos.FirstAsync(c =>
            c.EntidadId == dispositivo.EntidadId && c.Serie == serie && c.TipoDocumento == "FACTURA_VENTA");

        consecutivo.UltimoNumero = end;
        await _context.SaveChangesAsync();

        var reserva = new PosRangoNumeracion
        {
            Id = Guid.NewGuid(),
            DispositivoPosId = dispositivoId,
            Serie = serie,
            NumeroDesde = (int)start,
            NumeroHasta = (int)end,
            NumeroSiguienteLocal = (int)start,
            AsignadoEn = DateTimeOffset.Now,
            Agotado = false
        };

        _context.PosRangoNumeracions.Add(reserva);
        await _context.SaveChangesAsync();

        return Ok(new { serie, desde = start, hasta = end });
    }

    [HttpPost("ventas/sync")]
    public async Task<IActionResult> SyncVentas([FromBody] List<OperacionRequestDto> operaciones)
    {
        // RNF-02: Procesamiento Idempotente Asíncrono
        foreach (var op in operaciones)
        {
            // 1. Guardar como Pendiente (Exactly-Once)
            var existe = await _context.PosVentaPendientes
                .AnyAsync(p => p.IdempotencyKey == op.LocalId.ToString() && p.DispositivoPosId == Guid.Parse(op.TerminalId ?? Guid.Empty.ToString()));

            if (existe) continue;

            var pendiente = new PosVentaPendiente
            {
                Id = Guid.NewGuid(),
                DispositivoPosId = Guid.Parse(op.TerminalId ?? Guid.Empty.ToString()),
                IdempotencyKey = op.LocalId.ToString(),
                PayloadJson = JsonSerializer.Serialize(op),
                Estado = "PENDIENTE",
                FechaRecibidoServidor = DateTimeOffset.Now,
                FechaVentaLocal = op.Fecha ?? DateTimeOffset.Now
            };

            _context.PosVentaPendientes.Add(pendiente);
        }

        await _context.SaveChangesAsync();

        // En una implementación productiva, un BackgroundJob dispararía el procesamiento real aquí.
        // Para esta fase de estabilización, retornamos OK confirmando la recepción segura.
        return Ok(new { status = "RECEIVED", count = operaciones.Count });
    }

    [HttpGet("pos/productos")]
    public async Task<IActionResult> GetProductos([FromQuery] string? updatedSince = null)
    {
        var query = _context.Productos.Include(p => p.UnidadMedida).AsQueryable();

        if (!string.IsNullOrEmpty(updatedSince) && DateTimeOffset.TryParse(updatedSince, out var date))
        {
            query = query.Where(p => p.ActualizadoEn > date);
        }

        var res = await query.Select(p => new {
            serverId = p.Id,
            cod = p.Codigo,
            nombre = p.Nombre,
            precio = p.PrecioVentaActual,
            unidad = p.UnidadMedida.Codigo,
            existencias = _context.Existencia.Where(e => e.ProductoId == p.Id).Sum(e => e.Cantidad)
        }).ToListAsync();

        return Ok(res);
    }
}
