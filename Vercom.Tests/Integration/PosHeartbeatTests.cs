using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Vercom.Tests.Infrastructure;
using VERCOM.Models;
using VERCOM.Security;
using VERCOM.Services;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// El heartbeat de una entidad no puede alcanzar el dispositivo de otra: lo impide el filtro
/// global multi-entidad (<c>IsMaster || EntidadId == CurrentEntidadId</c>) sobre
/// <c>integracion.dispositivo_pos</c>, el mismo que protege al resto de las consultas del POS.
/// </summary>
public class PosHeartbeatTests
{
    [DbFact(RequiereDatosBase = true)]
    public async Task ElHeartbeatDeOtraEntidadNoEncuentraElDispositivo()
    {
        await using var db = TestDb.Create();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sesion = await PosFixture.CrearSesionAsync(db);

        var entidadAjena = Guid.NewGuid();
        await using var dbAjeno = CompartiendoTransaccion(db, new TestEntidadProvider(entidadAjena, TestDb.UsuarioIdSeed));
        var (okAjeno, mensajeAjeno) = await ServicioDe(dbAjeno).HeartbeatAsync(sesion.DispositivoPosId);

        Assert.False(okAjeno);
        Assert.Equal("Dispositivo no encontrado o inactivo.", mensajeAjeno);

        // Control: con su propia entidad —y sin ser master— el mismo dispositivo sí responde.
        await using var dbPropio = CompartiendoTransaccion(db, new TestEntidadProvider(sesion.EntidadId, TestDb.UsuarioIdSeed));
        var (okPropio, _) = await ServicioDe(dbPropio).HeartbeatAsync(sesion.DispositivoPosId);
        Assert.True(okPropio);

        await tx.RollbackAsync();
    }

    // Segundo contexto (con otra entidad) sobre la MISMA conexión y transacción del test, para
    // que alcance el fixture todavía no confirmado.
    private static AppDbContext CompartiendoTransaccion(AppDbContext db, IEntidadProvider provider)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(db.Database.GetDbConnection())
            .Options;

        var contexto = new AppDbContext(options, provider);
        contexto.Database.UseTransaction(db.Database.CurrentTransaction!.GetDbTransaction());
        return contexto;
    }

    // Solo se ejercita HeartbeatAsync: no hacen falta scope factory, arqueo ni precios.
    private static PosService ServicioDe(AppDbContext db) =>
        new(db, null!, new ConsecutivoService(db), null!, new NotificacionNula(), null!, null!);
}
