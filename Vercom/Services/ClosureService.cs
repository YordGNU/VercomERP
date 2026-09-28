using Microsoft.EntityFrameworkCore;
using Vercom.Helpers;
using Vercom.Models;

namespace Vercom.Services;

public interface IClosureService
{
    Task<(bool Succeeded, string Message)> CloseFiscalYearAsync(Guid entidadId, short year, Guid userId);
}

public class ClosureService : IClosureService
{
    private readonly AppDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly IConsecutivoService _consecutivoService;

    public ClosureService(AppDbContext context, IAccountingService accountingService, IConsecutivoService consecutivoService)
    {
        _context = context;
        _accountingService = accountingService;
        _consecutivoService = consecutivoService;
    }

    public async Task<(bool Succeeded, string Message)> CloseFiscalYearAsync(Guid entidadId, short year, Guid userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Validar que todos los periodos del año estén cerrados
            var openPeriods = await _context.PeriodoContables
                .AnyAsync(p => p.EntidadId == entidadId && p.Anio == year && p.Estado == "ABIERTO");

            if (openPeriods) return (false, "Existen periodos mensuales abiertos en el ejercicio fiscal.");

            // 2. Cuenta de Resultado y periodo de destino del cierre (diciembre o el último del ejercicio)
            var ctaResultado = await _context.CuentaContables.FirstOrDefaultAsync(c => c.Codigo == "999" && c.EntidadId == entidadId);
            if (ctaResultado == null) return (false, "No existe la cuenta 999 (Resultado). No se puede cerrar el ejercicio.");

            var periodoCierre = await _context.PeriodoContables
                .Where(p => p.EntidadId == entidadId && p.Anio == year)
                .OrderByDescending(p => p.Mes)
                .FirstOrDefaultAsync();
            if (periodoCierre == null) return (false, "No existe un periodo contable para el ejercicio.");

            var tipoAJ = await _context.TipoComprobantes.FirstOrDefaultAsync(t => t.Codigo == "AJ");
            if (tipoAJ == null) return (false, "No existe el tipo de comprobante AJ (Ajustes).");

            // 3. Saldos anuales por cuenta de INGRESOS (naturaleza acreedora) y GASTOS (deudora)
            var balances = await _context.AsientoDetalles
                .Where(d => d.Asiento.EntidadId == entidadId
                         && d.Asiento.Periodo.Anio == year
                         && d.Asiento.Estado == "CONTABILIZADO")
                .GroupBy(d => new { d.CuentaId, d.Cuenta.Clase })
                .Select(g => new
                {
                    g.Key.CuentaId,
                    g.Key.Clase,
                    Saldo = g.Sum(d => (d.Haber - d.Debe))
                })
                .ToListAsync();

            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                PeriodoId = periodoCierre.Id,
                TipoComprobanteId = tipoAJ.Id,
                Fecha = new DateOnly(year, 12, 31),
                Concepto = $"CIERRE DEL EJERCICIO FISCAL {year}",
                ModuloOrigen = "CONTABILIDAD",
                DocumentoOrigenTipo = "CIERRE_ANUAL",
                Estado = "CONTABILIZADO",
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now
            };

            // 4. Cerrar cuentas de ingresos y gastos contra Resultado (partida doble)
            decimal totalContra = 0;
            foreach (var b in balances)
            {
                if (Math.Abs(b.Saldo) < 0.005m || b.CuentaId == ctaResultado.Id) continue;

                if (b.Clase == "INGRESOS" && b.Saldo > 0)
                {
                    entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = b.CuentaId, Debe = b.Saldo, Haber = 0, Glosa = $"Cierre de Ingresos {year}" });
                    entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ctaResultado.Id, Debe = 0, Haber = b.Saldo, Glosa = $"Resultado del Ejercicio {year}" });
                    totalContra += b.Saldo;
                }
                else if (b.Clase == "GASTOS" && b.Saldo < 0)
                {
                    var monto = -b.Saldo;
                    entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = ctaResultado.Id, Debe = monto, Haber = 0, Glosa = $"Resultado del Ejercicio {year}" });
                    entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = b.CuentaId, Debe = 0, Haber = monto, Glosa = $"Cierre de Gastos {year}" });
                    totalContra += monto;
                }
            }

            if (!entry.AsientoDetalles.Any())
            {
                await transaction.RollbackAsync();
                return (true, $"Ejercicio {year} sin resultados que cerrar.");
            }

            var totalDebe = entry.AsientoDetalles.Sum(d => d.Debe);
            var totalHaber = entry.AsientoDetalles.Sum(d => d.Haber);
            if (totalDebe != totalHaber)
            {
                await transaction.RollbackAsync();
                return (false, $"Asiento de cierre descuadrado (Deber {totalDebe} / Haber {totalHaber}).");
            }

            entry.TotalDebe = totalDebe;
            entry.TotalHaber = totalHaber;
            entry.NumeroComprobante = await _consecutivoService.ObtenerSiguienteNumeroLongAsync(
                entidadId, null, DocumentoTipo.AsientoContable, tipoAJ.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));

            short linea = 1;
            foreach (var det in entry.AsientoDetalles)
            {
                det.AsientoId = entry.Id;
                det.Linea = linea++;
            }

            _context.AsientoContables.Add(entry);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Ejercicio {year} cerrado correctamente. Resultado del ejercicio: {totalContra:C}.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error en cierre anual: {ex.Message}");
        }
    }
}
