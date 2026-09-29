using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO.Compression;
using Vercom.Models;
using Vercom.Security;

namespace Vercom.Services;

public interface IReportingService
{
    Task<List<Auditorium>> GetUserAuditReportAsync(Guid? userId, DateTime start, DateTime end);
    Task<List<VAuditoriaReversione>> GetReversionReportAsync(DateTime start, DateTime end);
    Task<Guid> GenerateMonthlyPackageAsync(Guid entidadId, Guid periodId);
}

public class ReportingService : IReportingService
{
    private readonly AppDbContext _db;
    private readonly IEntidadProvider _entidadProvider;
    private readonly IWebHostEnvironment _env;

    public ReportingService(AppDbContext context, IEntidadProvider entidadProvider, IWebHostEnvironment env)
    {
        _db = context;
        _entidadProvider = entidadProvider;
        _env = env;
    }

    public async Task<List<Auditorium>> GetUserAuditReportAsync(Guid? userId, DateTime start, DateTime end)
    {
        var query = _db.Auditoria.Where(a => a.OcurridoEn >= start && a.OcurridoEn <= end);
        if (userId.HasValue) query = query.Where(a => a.UsuarioId == userId);
        return await query.OrderByDescending(a => a.OcurridoEn).ToListAsync();
    }

    public async Task<List<VAuditoriaReversione>> GetReversionReportAsync(DateTime start, DateTime end)
    {
        return await _db.VAuditoriaReversiones
            .Where(r => r.OcurridoEn >= start && r.OcurridoEn <= end)
            .OrderByDescending(r => r.OcurridoEn)
            .ToListAsync();
    }

    public async Task<Guid> GenerateMonthlyPackageAsync(Guid entidadId, Guid periodId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var periodo = await _db.PeriodoContables.FindAsync(periodId);
        var entidad = await _db.Entidads.FindAsync(entidadId)
            ?? throw new ArgumentException("Entidad no encontrada.");

        var pdfOnat = BuildPdf(entidad, periodo, "ONAT");
        var pdfDireccion = BuildPdf(entidad, periodo, "DIRECCION");

        byte[] zipBytes;
        using (var ms = new MemoryStream())
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            AddZipEntry(archive, "datos_onat.pdf", pdfOnat);
            AddZipEntry(archive, "datos_direccion.pdf", pdfDireccion);
            zipBytes = ms.ToArray();
        }

        var packageDir = Path.Combine(_env.WebRootPath ?? "", "uploads", "paquetes");
        Directory.CreateDirectory(packageDir);
        var packageId = Guid.NewGuid();
        var filePath = Path.Combine(packageDir, $"{packageId}.zip");
        await File.WriteAllBytesAsync(filePath, zipBytes);

        var record = new PaqueteInformacion
        {
            Id = packageId,
            EntidadId = entidadId,
            PeriodoId = periodId,
            Tipo = "MFP",
            Estado = "GENERADO",
            GeneradoPor = _entidadProvider.CurrentUsuarioId,
            Onat = entidad.Nit,
            Direccion = entidad.DireccionLegal,
            GeneradoEn = DateTimeOffset.Now
        };
        _db.PaqueteInformacions.Add(record);
        await _db.SaveChangesAsync();

        return packageId;
    }

    private static byte[] BuildPdf(Entidad entidad, PeriodoContable? periodo, string subtipo)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(2, Unit.Centimetre);
                page.Header().AlignCenter().Text("Paquete MFP").FontSize(18).Bold().FontColor(Colors.Grey.Darken2);
                page.Content().Column(col =>
                {
                    col.Item().Text(subtipo).FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                    col.Item().PaddingTop(6);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Razón Social:").Bold();
                        row.RelativeItem().Text(entidad.RazonSocial);
                    });
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("NIT / ONAT:").Bold();
                        row.RelativeItem().Text(entidad.Nit);
                    });
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Dirección:").Bold();
                        row.RelativeItem().Text(entidad.DireccionLegal);
                    });
                    if (!string.IsNullOrEmpty(entidad.Municipio))
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Municipio:").Bold();
                            row.RelativeItem().Text(entidad.Municipio);
                        });
                    if (!string.IsNullOrEmpty(entidad.Provincia))
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Provincia:").Bold();
                            row.RelativeItem().Text(entidad.Provincia);
                        });
                    col.Item().PaddingTop(6);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Período:").Bold();
                        row.RelativeItem().Text($"{periodo?.Mes}/{periodo?.Anio}");
                    });
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static void AddZipEntry(ZipArchive archive, string name, byte[] content)
    {
        var entry = archive.CreateEntry(name);
        using var es = entry.Open();
        es.Write(content, 0, content.Length);
    }
}