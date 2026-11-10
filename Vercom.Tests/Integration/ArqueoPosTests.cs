using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using VERCOM.DTOs;
using VERCOM.Models;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Arqueo y cierre de la sesión de caja del POS.
/// Nota sobre <c>diferencia_arqueo</c>: es una columna calculada en BD
/// (<c>monto_cierre_declarado - monto_cierre_sistema</c>), por eso el código no
/// la asigna y sólo se puede comprobar después de recargar la entidad.
/// El camino con descuadre abre una transacción propia (no anidable dentro de
/// la del test), así que aquí se cubren las guardas y el cierre cuadrado.
/// </summary>
public class ArqueoPosTests
{
    private static PosCajaService CrearServicio(AppDbContext db)
        => new(db, null!, new ArqueoCajaContableService(db, null!, new ParametroSistemaService(db)));

    [DbFact]
    public async Task CerrarConMontoNegativoSeRechazaSinTocarLaSesion()
    {
        await using var db = TestDb.Create();
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.CerrarAsync(
            Guid.NewGuid(),
            new CerrarSesionCajaPosRequest { MontoCierreDeclarado = -1m },
            CancellationToken.None);

        Assert.Null(sesion);
        Assert.NotNull(error);
        Assert.False(conflict);
    }

    [DbFact]
    public async Task CerrarUnaSesionInexistenteDevuelveVacioSinError()
    {
        await using var db = TestDb.Create();
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.CerrarAsync(
            Guid.NewGuid(),
            new CerrarSesionCajaPosRequest { MontoCierreDeclarado = 0m },
            CancellationToken.None);

        Assert.Null(sesion);
        Assert.Null(error);
        Assert.False(conflict);
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task NoSePuedeCerrarLaCajaConVentasPendientes()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var fixture = await PosFixture.CrearSesionAsync(db, montoApertura: 500m, totalEfectivo: 120m);
        await PosFixture.AgregarVentaPendienteAsync(db, fixture);
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.CerrarAsync(
            fixture.SesionId,
            new CerrarSesionCajaPosRequest { MontoCierreDeclarado = 620m },
            CancellationToken.None);

        Assert.Null(sesion);
        Assert.True(conflict);
        Assert.Contains("pendientes", error, StringComparison.OrdinalIgnoreCase);

        var estado = await db.SesionCajaPos.AsNoTracking()
            .Where(s => s.Id == fixture.SesionId)
            .Select(s => s.Estado)
            .FirstAsync();
        Assert.Equal("ABIERTA", estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task CierreCuadradoMarcaLaSesionCerradaYElSistemaCalculaElEfectivoEsperado()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var fixture = await PosFixture.CrearSesionAsync(db, montoApertura: 100m, totalEfectivo: 250m);
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.CerrarAsync(
            fixture.SesionId,
            new CerrarSesionCajaPosRequest { MontoCierreDeclarado = 350m },
            CancellationToken.None);

        Assert.Null(error);
        Assert.False(conflict);
        Assert.NotNull(sesion);
        Assert.Equal("CERRADA", sesion!.Estado);
        Assert.Equal(350m, sesion.MontoCierreSistema);
        Assert.NotNull(sesion.FechaCierre);

        var entidad = await db.SesionCajaPos.FirstAsync(s => s.Id == fixture.SesionId);
        await db.Entry(entidad).ReloadAsync();

        Assert.Equal("CERRADA", entidad.Estado);
        Assert.Equal(350m, entidad.MontoCierreSistema);
        // Columna calculada: 350 declarado - 350 del sistema = 0 de descuadre.
        Assert.Equal(0m, entidad.DiferenciaArqueo);
        // Sin descuadre no debe generarse asiento de arqueo.
        Assert.Null(entidad.AsientoCierreId);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task CerrarUnaSesionYaCerradaEsConflictoYNoUnError()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var fixture = await PosFixture.CrearSesionAsync(db, estado: "CERRADA", montoApertura: 100m);
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.CerrarAsync(
            fixture.SesionId,
            new CerrarSesionCajaPosRequest { MontoCierreDeclarado = 100m },
            CancellationToken.None);

        Assert.Null(error);
        Assert.True(conflict);
        Assert.NotNull(sesion);
        Assert.Equal("CERRADA", sesion!.Estado);

        await tx.RollbackAsync();
    }

    [DbFact(RequiereDatosBase = true)]
    public async Task AbrirCuandoLaCajaYaTieneSesionAbiertaDevuelveLaMismaSesion()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var fixture = await PosFixture.CrearSesionAsync(db, montoApertura: 300m);
        var servicio = CrearServicio(db);

        var (sesion, error, conflict) = await servicio.AbrirAsync(
            new AbrirSesionCajaPosRequest
            {
                DispositivoPosId = fixture.DispositivoPosId,
                CajeroId = fixture.CajeroId,
                MontoApertura = 999m
            },
            CancellationToken.None);

        Assert.Null(error);
        Assert.True(conflict);
        Assert.NotNull(sesion);
        // La sesión es de la caja, no del dispositivo: se reutiliza la existente.
        Assert.Equal(fixture.SesionId, sesion!.Id);
        Assert.Equal(300m, sesion.MontoApertura);

        var cantidad = await db.SesionCajaPos.CountAsync(s => s.CajaId == fixture.CajaId && s.Estado == "ABIERTA");
        Assert.Equal(1, cantidad);

        await tx.RollbackAsync();
    }
}
