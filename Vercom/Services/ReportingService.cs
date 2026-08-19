using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IReportingService
{
    Task<List<Auditorium>> GetUserAuditReportAsync(Guid? userId, DateTime start, DateTime end);
    Task<List<VAuditoriaReversione>> GetReversionReportAsync(DateTime start, DateTime end);
    Task<string> GenerateMonthlyPackageAsync(Guid entidadId, Guid periodId);
}

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Auditorium>> GetUserAuditReportAsync(Guid? userId, DateTime start, DateTime end)
    {
        var query = _context.Auditoria.Where(a => a.OcurridoEn >= start && a.OcurridoEn <= end);
        if (userId.HasValue) query = query.Where(a => a.UsuarioId == userId);
        return await query.OrderByDescending(a => a.OcurridoEn).ToListAsync();
    }

    public async Task<List<VAuditoriaReversione>> GetReversionReportAsync(DateTime start, DateTime end)
    {
        return await _context.VAuditoriaReversiones
            .Where(r => r.OcurridoEn >= start && r.OcurridoEn <= end)
            .OrderByDescending(r => r.OcurridoEn)
            .ToListAsync();
    }

    public async Task<string> GenerateMonthlyPackageAsync(Guid entidadId, Guid periodId)
    {
        // RNF-60: Simulación de generación de paquete mensual (< 10s)
        var period = await _context.PeriodoContables.FindAsync(periodId);
        var fileName = $"PAQUETE_{entidadId}_{period?.Mes}_{period?.Anio}.zip";

        // Aquí iría la lógica real de exportación a PDF usando iTextSharp o QuestPDF
        // y consolidación en un ZIP.

        var record = new PaqueteInformacion
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            PeriodoId = periodId,
            Tipo = "MENSUAL",
            Estado = "GENERADO",
            GeneradoEn = DateTimeOffset.Now
        };
        _context.PaqueteInformacions.Add(record);
        await _context.SaveChangesAsync();

        return fileName;
    }
}
