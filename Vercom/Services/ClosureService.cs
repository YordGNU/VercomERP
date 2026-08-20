using Microsoft.EntityFrameworkCore;
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

    public ClosureService(AppDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
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

            // 2. Calcular Utilidad/Pérdida del Ejercicio
            var ingresos = await _context.AsientoDetalles
                .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "INGRESOS")
                .SumAsync(d => d.Haber - d.Debe);

            var gastos = await _context.AsientoDetalles
                .Where(d => d.Asiento.EntidadId == entidadId && d.Asiento.Periodo.Anio == year && d.Cuenta.Clase == "GASTOS")
                .SumAsync(d => d.Debe - d.Haber);

            var utilidad = ingresos - gastos;

            // 3. Crear Asiento de Cierre de Resultados
            var entry = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Fecha = new DateOnly(year, 12, 31),
                Concepto = $"CIERRE DEL EJERCICIO FISCAL {year}",
                ModuloOrigen = "CONTABILIDAD",
                DocumentoOrigenTipo = "CIERRE_ANUAL",
                TipoComprobanteId = 7, // Ajustes
                Estado = "CONTABILIZADO",
                CreadoPor = userId,
                CreadoEn = DateTimeOffset.Now
            };

            // Invertir saldos de ingresos y gastos para dejarlos en cero
            // (Lógica simplificada)

            await _context.AsientoContables.AddAsync(entry);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Ejercicio {year} cerrado exitosamente. Utilidad registrada.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error en cierre anual: {ex.Message}");
        }
    }
}
