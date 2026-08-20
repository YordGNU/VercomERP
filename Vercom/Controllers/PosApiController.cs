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
   private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
    private readonly IAuthService _authService;
    private readonly ISalesService _salesService;

    public PosApiController(AppDbContext context, IAuthService authService, ISalesService salesService)
    {
        _context = context;
        _authService = authService;
        _salesService = salesService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Dictionary<string, string> credentials)
    {
        if (!credentials.TryGetValue("username", out var username) ||
            !credentials.TryGetValue("password", out var password))
        {
            return BadRequest(new { message = "Username and password required" });
        }

        var result = await _authService.LoginAsync(username, password, false);
        if (result.Succeeded)
        {
            return Ok(new { token = "session-cookie-active", message = result.Message });
        }

        return Unauthorized(new { message = result.Message });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "OK", timestamp = DateTime.Now });
    }

    [HttpGet("pos/productos")]
    public async Task<IActionResult> GetProductos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? updatedSince = null,
        [FromQuery] Guid? puntoVentaId = null)
    {
        var query = _context.Productos.AsQueryable();

        if (!string.IsNullOrEmpty(updatedSince) && DateTime.TryParse(updatedSince, out var date))
        {
            query = query.Where(p => p.ActualizadoEn > date);
        }

        var productos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new {
                serverId = p.Id.ToString(), // Enviar como string (Guid)
                cod = p.Codigo,
                nombre = p.Nombre,
                precio = p.PrecioVentaActual,
                activo = p.Activo,
                existencias = _context.Existencia
                    .Where(e => e.ProductoId == p.Id && (puntoVentaId == null || e.AlmacenId == puntoVentaId))
                    .Sum(e => (float)e.Cantidad),
                stockMinimo = _context.Existencia
                    .Where(e => e.ProductoId == p.Id && (puntoVentaId == null || e.AlmacenId == puntoVentaId))
                    .Max(e => (float)e.StockMinimo),
                categoriaid = p.FamiliaId.ToString(),
                unidadid = p.UnidadMedidaId,
                unidadMedida = p.UnidadMedida.Codigo
            })
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("pos/categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var familias = await _context.FamiliaProductos
            .Select(f => new { id = f.Id.ToString(), clave = f.Codigo, nombre = f.Nombre })
            .ToListAsync();
        return Ok(familias);
    }

    [HttpGet("pos/unidades")]
    public async Task<IActionResult> GetUnidades()
    {
        var unidades = await _context.UnidadMedida
            .Select(u => new { id = u.Id, clave = u.Codigo, unidad_name = u.Nombre })
            .ToListAsync();
        return Ok(unidades);
    }

    [HttpGet("pos/clientes")]
    public async Task<IActionResult> GetClientes()
    {
        var clientes = await _context.Clientes
            .Select(c => new { id = c.Id.ToString(), nombre = c.NombreRazonSocial, rfc = c.NitOCi })
            .ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("pos/areas")]
    public async Task<IActionResult> GetAreas()
    {
        var sucursales = await _context.Sucursals
            .Select(s => new { id = s.Id.ToString(), nombre = s.Nombre })
            .ToListAsync();
        return Ok(sucursales);
    }

    [HttpPost("ventas/sync")]
    public async Task<IActionResult> SyncVentas([FromBody] List<OperacionRequestDto> operaciones)
    {
        var entidad = await _context.Entidads.FirstOrDefaultAsync();
        if (entidad == null) return BadRequest("No entity configured");

        var sucursalId = await _context.Sucursals.Where(s => s.EntidadId == entidad.Id).Select(s => s.Id).FirstOrDefaultAsync();
        var defaultClient = await _context.Clientes.Where(c => c.EntidadId == entidad.Id).FirstOrDefaultAsync();
        var defaultWarehouse = await _context.Almacens.Where(a => a.EntidadId == entidad.Id).FirstOrDefaultAsync();

        foreach (var op in operaciones)
        {
            var invoice = new FacturaVentum
            {
                EntidadId = entidad.Id,
                SucursalId = sucursalId,
                Serie = "POS",
                ClienteId = op.ClienteId ?? defaultClient?.Id ?? Guid.Empty,
                AlmacenId = op.PuntoVentaid ?? defaultWarehouse?.Id ?? Guid.Empty,
                CanalVenta = "POS",
                TipoVenta = "MINORISTA",
                Fecha = op.Fecha ?? DateTimeOffset.Now,
                Moneda = op.Moneda ?? "CUP",
                Total = op.Importe ?? 0,
                CreadoPor = Guid.Empty
            };

            if (op.Productoid.HasValue)
            {
                invoice.FacturaVentaDetalles.Add(new FacturaVentaDetalle
                {
                    ProductoId = op.Productoid.Value,
                    Cantidad = op.Cantidad ?? 0,
                    PrecioUnitario = (op.Importe / op.Cantidad) ?? 0,
                    DescuentoPorcentaje = 0,
                    ImpuestoPorcentaje = 10
                });
            }

            await _salesService.CreateInvoiceAsync(invoice);
        }

        return Ok();
    }

    [HttpPost("auditoria/sync")]
    public async Task<IActionResult> SyncAuditoria([FromBody] JsonElement auditorias)
    {
        if (auditorias.ValueKind == JsonValueKind.Array)
        {
            foreach (var a in auditorias.EnumerateArray())
            {
                var entry = new Auditorium
                {
                    Accion = a.GetProperty("Accion").GetString() ?? "SYNC",
                    EsquemaTabla = a.GetProperty("Tabla").GetString() ?? "POS",
                    RegistroId = a.GetProperty("ReferenciaId").ToString(),
                    NombreUsuario = a.TryGetProperty("Usuario", out var u) ? u.GetString() ?? "POS-USER" : "POS-USER",
                    Canal = "POS",
                    OcurridoEn = a.TryGetProperty("Fecha", out var f) ? f.GetDateTime() : DateTime.Now
                };
                _context.Auditoria.Add(entry);
            }
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("pos/cierre-caja")]
    public async Task<IActionResult> RegistrarCierre([FromBody] Dictionary<string, object> data)
    {
        return Ok(new { id = Guid.NewGuid().ToString() });
    }
}
