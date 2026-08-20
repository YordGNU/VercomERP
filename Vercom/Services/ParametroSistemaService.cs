using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IParametroSistemaService
{
    Task<string?> ObtenerValorVigenteAsync(Guid entidadId, string codigo, DateOnly? fecha = null);
    Task<decimal> ObtenerValorNumericoVigenteAsync(Guid entidadId, string codigo, DateOnly? fecha = null);
}

public class ParametroSistemaService : IParametroSistemaService
{
    private readonly AppDbContext _context;

    public ParametroSistemaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ObtenerValorVigenteAsync(Guid entidadId, string codigo, DateOnly? fecha = null)
    {
        var targetDate = fecha ?? DateOnly.FromDateTime(DateTime.Today);

        var parametro = await _context.ParametroSistemas
            .Where(p => p.EntidadId == entidadId && p.Codigo == codigo)
            .Where(p => p.VigenteDesde <= targetDate && (p.VigenteHasta == null || p.VigenteHasta >= targetDate))
            .OrderByDescending(p => p.VigenteDesde)
            .Select(p => p.Valor)
            .FirstOrDefaultAsync();

        return parametro;
    }

    public async Task<decimal> ObtenerValorNumericoVigenteAsync(Guid entidadId, string codigo, DateOnly? fecha = null)
    {
        var valorStr = await ObtenerValorVigenteAsync(entidadId, codigo, fecha);
        if (string.IsNullOrEmpty(valorStr)) return 0;

        if (decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var valor))
        {
            return valor;
        }

        return 0;
    }
}
