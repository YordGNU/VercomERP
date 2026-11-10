using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vercom.Tests.Infrastructure;
using Xunit;

namespace Vercom.Tests.Integration;

/// <summary>
/// Detecta "schema drift": entidades que el modelo EF (AppDbContext) mapea a una
/// tabla/vista que no existe en la base de datos real.
///
/// Nace de un fallo real documentado en AGENTS.md: `nucleo.feedback` existía en
/// el DDL y en EF pero faltaba en la BD productiva, por lo que cualquier POST de
/// feedback reventaba en runtime. Este test lo habría detectado de inmediato.
/// </summary>
public class SchemaDriftTests
{
    [DbFact]
    public async Task TodaTablaOVistaMapeadaPorEfExisteEnLaBaseDeDatos()
    {
        await using var db = TestDb.Create();

        var esperadas = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entityType in db.Model.GetEntityTypes())
        {
            var tabla = entityType.GetTableName();
            var nombre = tabla ?? entityType.GetViewName();
            if (string.IsNullOrEmpty(nombre)) continue;

            // GetSchema() solo refleja el schema de las tablas; el de las vistas
            // vive en la anotación Relational:ViewSchema.
            var esquema = entityType.GetSchema()
                ?? entityType.FindAnnotation("Relational:ViewSchema")?.Value as string
                ?? "dbo";

            esperadas.Add($"{esquema}.{nombre}");
        }

        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var conexion = new SqlConnection(TestDb.ConnectionString))
        {
            await conexion.OpenAsync();
            await using var cmd = conexion.CreateCommand();
            cmd.CommandText =
                "SELECT s.name + '.' + o.name FROM sys.objects o " +
                "JOIN sys.schemas s ON s.schema_id = o.schema_id " +
                "WHERE o.type IN ('U', 'V')";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) existentes.Add(reader.GetString(0));
        }

        var faltantes = esperadas.Where(e => !existentes.Contains(e)).ToList();

        Assert.True(
            faltantes.Count == 0,
            $"El modelo EF mapea {faltantes.Count} tabla(s)/vista(s) inexistente(s) en la BD real:\n  - " +
            string.Join("\n  - ", faltantes));
    }
}
