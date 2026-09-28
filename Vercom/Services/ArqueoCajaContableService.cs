using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

/// <summary>
/// Contabiliza la diferencia de arqueo de una sesión de caja POS (item P3-E2E 21):
/// sobrante (diferencia &gt; 0) → Debe caja / Haber 930.0020 (Ingresos por Sobrantes - Medios Monetarios);
/// faltante (diferencia &lt; 0) → Debe 850.0020 (Gastos por Faltante - Medios Monetarios) / Haber caja.
/// Las cuentas se resuelven por parámetro de sistema con fallback; el asiento queda CONTABILIZADO,
/// cuadra partida doble y se liga a la sesión vía AsientoCierreId. Sin diferencia no genera asiento.
/// </summary>
public sealed class ArqueoCajaContableService
{
    public const string DocumentoOrigenArqueo = "ARQUEO_SESION_POS";

    private readonly AppDbContext _db;
    private readonly IAccountingService _accounting;
    private readonly IParametroSistemaService _parametros;

    public ArqueoCajaContableService(AppDbContext db, IAccountingService accounting, IParametroSistemaService parametros)
    {
        _db = db;
        _accounting = accounting;
        _parametros = parametros;
    }

    public async Task<(bool Succeeded, string Message, AsientoContable? Entry)> ContabilizarAsync(
        Guid sesionId, Guid cajaId, Guid cajeroId, decimal diferencia, DateTimeOffset? fechaCierre, CancellationToken cancellationToken)
    {
        if (diferencia == 0) return (true, "Sin diferencia de arqueo.", null);

        var caja = await _db.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cajaId, cancellationToken);
        if (caja is null) return (false, "La caja de la sesión no existe.", null);

        var cuentaCaja = await _db.CuentaContables.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caja.CuentaContableId, cancellationToken);
        if (cuentaCaja is null)
            return (false, "Error de configuración: la caja no tiene una cuenta contable válida.", null);

        var esSobrante = diferencia > 0;
        var codigoDestino = esSobrante
            ? (await _parametros.ObtenerValorVigenteAsync(caja.EntidadId, "CTA_SOBRANTE_CAJA")) ?? "930.0020"
            : (await _parametros.ObtenerValorVigenteAsync(caja.EntidadId, "CTA_FALTANTE_CAJA")) ?? "850.0020";

        var cuentaDestino = await _db.CuentaContables.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == codigoDestino && c.EntidadId == caja.EntidadId, cancellationToken);
        if (cuentaDestino is null)
            return (false, $"Error de configuración: la cuenta {codigoDestino} no existe en el plan de cuentas.", null);

        var tipoDiario = await _db.TipoComprobantes.AsNoTracking().FirstOrDefaultAsync(t => t.Codigo == "DIA", cancellationToken);
        if (tipoDiario is null) return (false, "Error de configuración: el tipo de comprobante DIA no existe.", null);

        var magnitud = Math.Abs(diferencia);
        var entry = new AsientoContable
        {
            Id = Guid.NewGuid(),
            EntidadId = caja.EntidadId,
            SucursalId = caja.SucursalId,
            Fecha = DateOnly.FromDateTime((fechaCierre ?? DateTimeOffset.Now).LocalDateTime),
            Concepto = $"ARQUEO SESIÓN POS — {(esSobrante ? "SOBRANTE" : "FALTANTE")} {magnitud:N2}",
            ModuloOrigen = "POS",
            DocumentoOrigenTipo = DocumentoOrigenArqueo,
            DocumentoOrigenId = sesionId,
            TipoComprobanteId = tipoDiario.Id,
            CreadoPor = cajeroId,
            CreadoEn = DateTimeOffset.Now,
            Estado = "CONTABILIZADO"
        };

        if (esSobrante)
        {
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaCaja.Id, Debe = Math.Round(magnitud, 2), Glosa = "Sobrante de arqueo (efectivo en caja)" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaDestino.Id, Haber = Math.Round(magnitud, 2), Glosa = "Ingreso por sobrante - medios monetarios" });
        }
        else
        {
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaDestino.Id, Debe = Math.Round(magnitud, 2), Glosa = "Gasto por faltante - medios monetarios" });
            entry.AsientoDetalles.Add(new AsientoDetalle { Id = Guid.NewGuid(), CuentaId = cuentaCaja.Id, Haber = Math.Round(magnitud, 2), Glosa = "Faltante de arqueo (efectivo en caja)" });
        }

        return await _accounting.CreateEntryAsync(entry);
    }
}