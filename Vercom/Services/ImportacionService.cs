using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public interface IImportacionService
{
    Task<ImportacionResumen> ObtenerResumenAsync(ConexionRequest conexion);
    Task<ImportacionResultado> ImportarAsync(Guid entidadId, Guid? sucursalId, ConexionRequest conexion, CancellationToken cancellationToken = default);
    Task<int> ReasignarCargosEmpleadosAsync(Guid entidadId, ConexionRequest conexion, CancellationToken cancellationToken = default);
    Task<TestConexionResultado> TestConexionAsync(ConexionRequest conexion);
    Task<List<ProductoVersatDto>> ObtenerProductosAsync(ConexionRequest conexion, CancellationToken cancellationToken = default);
    Task<List<ExistenciaVersatDto>> ObtenerExistenciasAsync(ConexionRequest conexion, CancellationToken cancellationToken = default);
    Task<List<AlmacenVersatDto>> ObtenerAlmacenesAsync(ConexionRequest conexion, CancellationToken cancellationToken = default);
}

public class ImportacionService : IImportacionService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public ImportacionService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<TestConexionResultado> TestConexionAsync(ConexionRequest conexion)
    {
        var connectionString = BuildConnectionString(conexion);
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return new TestConexionResultado { Exitoso = true, Mensaje = "Conexión exitosa" };
        }
        catch (Exception ex)
        {
            return new TestConexionResultado { Exitoso = false, Mensaje = ex.Message };
        }
    }

    public async Task<ImportacionResumen> ObtenerResumenAsync(ConexionRequest conexion)
    {
        var connectionString = BuildConnectionString(conexion);
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var resumen = new ImportacionResumen();

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM gen_producto WHERE activo = 1", connection))
            resumen.Productos = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand(@"
            SELECT COUNT(DISTINCT RTRIM(LTRIM(p.codigo)) + '|' + ISNULL(RTRIM(LTRIM(ga.codigo)), '@'))
            FROM inv_existencia e
            INNER JOIN gen_producto p ON p.idproducto = e.idproducto
            LEFT JOIN inv_existenciaalm ea ON ea.idexistencia = e.idexistencia
            LEFT JOIN gen_almacen ga ON ga.idalmacen = ea.idalmacen AND ga.activo = 1
            WHERE e.cantidad > 0", connection))
            resumen.Existencias = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM gen_almacen WHERE activo = 1", connection))
            resumen.Almacenes = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM gen_medida", connection))
            resumen.UnidadesMedida = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM gen_nivelclasprod WHERE activo = 1", connection))
            resumen.Categorias = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM inv_concepto WHERE activo = 1", connection))
            resumen.Conceptos = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM cos_centro WHERE activo = 1", connection))
            resumen.CentrosCosto = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(DISTINCT CodigoBarras) FROM pdv_Precios WHERE activo = 1", connection))
            resumen.Precios = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM gen_entidad WHERE activo = 1", connection))
            resumen.Clientes = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM nom_puestos_trb WHERE b_activo = 1", connection))
            resumen.Cargos = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY g.numident ORDER BY n.fecha DESC, n.idpuestotrabajo DESC) rn
                FROM gen_trabajador g
                INNER JOIN nom_trabajadores n ON n.idtrabajador = g.idtrabajador AND n.fecha IS NOT NULL
                WHERE g.activo = 1 AND g.numident IS NOT NULL
            ) x
            WHERE x.rn = 1", connection))
            resumen.Empleados = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM pdv_Registradoras", connection))
            resumen.DispositivosPos = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        return resumen;
    }

    public async Task<int> ReasignarCargosEmpleadosAsync(Guid entidadId, ConexionRequest conexion, CancellationToken cancellationToken = default)
    {
        var cadenaOrigen = BuildConnectionString(conexion);
        var puestosVersat = await LeerPuestosAsync(cadenaOrigen, cancellationToken);
        var empleadosVersat = await LeerEmpleadosAsync(cadenaOrigen, cancellationToken);

        using var destino = new SqlConnection(_db.Database.GetConnectionString());
        await destino.OpenAsync(cancellationToken);

        using var tx = destino.BeginTransaction();
        var (_, mapaPuestoCargo) = await ResolverCargosAsync(destino, tx, entidadId, puestosVersat, cancellationToken);

        var corregidos = 0;
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("UPDATE rrhh.empleado SET cargo_id = @CargoId WHERE carnet_identidad = @Carnet AND cargo_id <> @CargoId", destino, tx))
        {
            cmd.Parameters.Add("@CargoId", SqlDbType.UniqueIdentifier);
            cmd.Parameters.Add("@Carnet", SqlDbType.NVarChar, 60);
            foreach (var e in empleadosVersat)
            {
                if (string.IsNullOrEmpty(e.Carnet) || !vistos.Add(e.Carnet)) continue;
                if (!mapaPuestoCargo.TryGetValue(e.PuestoId, out var cargoId)) continue;
                cmd.Parameters["@CargoId"].Value = cargoId;
                cmd.Parameters["@Carnet"].Value = e.Carnet;
                corregidos += await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        tx.Commit();
        return corregidos;
    }

    public async Task<List<ProductoVersatDto>> ObtenerProductosAsync(ConexionRequest conexion, CancellationToken cancellationToken = default)
    {
        var connectionString = BuildConnectionString(conexion);
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT TOP 100 p.idproducto, p.codigo, p.descripcion, p.idmedida, p.activo, p.precio,
                   m.clave as unidadClave, m.descripcion as unidadNombre
            FROM gen_producto p
            LEFT JOIN gen_medida m ON m.idmedida = p.idmedida
            WHERE p.activo = 1
            ORDER BY p.codigo";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var productos = new List<ProductoVersatDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            productos.Add(new ProductoVersatDto
            {
                IdProducto = reader.GetInt32(0),
                Codigo = reader.GetString(1),
                Descripcion = reader.GetString(2),
                IdMedida = reader.GetInt32(3),
                Activo = reader.GetBoolean(4),
                Precio = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                UnidadClave = reader.IsDBNull(6) ? null : reader.GetString(6),
                UnidadNombre = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        return productos;
    }

    public async Task<List<ExistenciaVersatDto>> ObtenerExistenciasAsync(ConexionRequest conexion, CancellationToken cancellationToken = default)
    {
        var connectionString = BuildConnectionString(conexion);
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT TOP 100 e.idproducto, e.cantidad, e.preciocostoi, e.minimo, e.maximo,
                   p.codigo, p.descripcion, a.idalmacen, a.nombre as almacenNombre
            FROM inv_existencia e
            INNER JOIN gen_producto p ON p.idproducto = e.idproducto
            LEFT JOIN inv_existenciaalm ea ON ea.idexistencia = e.idexistencia
            LEFT JOIN gen_almacen a ON a.idalmacen = ea.idalmacen
            WHERE e.cantidad > 0
            ORDER BY p.codigo";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var existencias = new List<ExistenciaVersatDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            existencias.Add(new ExistenciaVersatDto
            {
                IdProducto = reader.GetInt32(0),
                Cantidad = reader.GetDecimal(1),
                PrecioCosto = reader.GetDecimal(2),
                Minimo = reader.GetDecimal(3),
                Maximo = reader.GetDecimal(4),
                Codigo = reader.GetString(5),
                Descripcion = reader.GetString(6),
                IdAlmacen = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                AlmacenNombre = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }

        return existencias;
    }

    public async Task<List<AlmacenVersatDto>> ObtenerAlmacenesAsync(ConexionRequest conexion, CancellationToken cancellationToken = default)
    {
        var connectionString = BuildConnectionString(conexion);
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT idalmacen, codigo, nombre, activo
            FROM gen_almacen
            WHERE activo = 1
            ORDER BY codigo";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var almacenes = new List<AlmacenVersatDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            almacenes.Add(new AlmacenVersatDto
            {
                IdAlmacen = reader.GetInt32(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Activo = reader.GetBoolean(3)
            });
        }

        return almacenes;
    }

    public async Task<ImportacionResultado> ImportarAsync(Guid entidadId, Guid? sucursalId, ConexionRequest conexion, CancellationToken cancellationToken = default)
    {
        var resultado = new ImportacionResultado();
        var cadenaOrigen = BuildConnectionString(conexion);

        var almacenesVersat = await LeerAlmacenesAsync(cadenaOrigen, cancellationToken);
        var unidadesVersat = await LeerUnidadesAsync(cadenaOrigen, cancellationToken);
        var familiasVersat = await LeerFamiliasAsync(cadenaOrigen, cancellationToken);
        var productosVersat = await LeerProductosAsync(cadenaOrigen, cancellationToken);
        var tiposProductoVersat = await LeerTiposProductoAsync(cadenaOrigen, cancellationToken);
        var existenciasVersat = await LeerExistenciasAsync(cadenaOrigen, cancellationToken);
        var centrosCostoVersat = await LeerCentrosCostoAsync(cadenaOrigen, cancellationToken);
        var conceptosVersat = await LeerConceptosAsync(cadenaOrigen, cancellationToken);
        var puestosVersat = await LeerPuestosAsync(cadenaOrigen, cancellationToken);
        var clientesVersat = await LeerClientesAsync(cadenaOrigen, cancellationToken);
        var empleadosVersat = await LeerEmpleadosAsync(cadenaOrigen, cancellationToken);
        var preciosVersat = await LeerPreciosAsync(cadenaOrigen, cancellationToken);
        var registradorasVersat = await LeerRegistradorasAsync(cadenaOrigen, cancellationToken);

        using var destino = new SqlConnection(_db.Database.GetConnectionString());
        await destino.OpenAsync(cancellationToken);

        var sucursalDestino = sucursalId ?? await ObtenerPrimeraSucursalAsync(destino, entidadId, cancellationToken);
        if (sucursalDestino == null)
            throw new InvalidOperationException("No se encontró una sucursal para la entidad. La columna sucursal_id es obligatoria.");

        using var tx = destino.BeginTransaction();
        try
        {
            var sel = conexion.Secciones;
            var on = (string seccion) => sel is null || sel.Count == 0 || sel.Contains(seccion, StringComparer.OrdinalIgnoreCase);

            if (on("almacenes") || on("existencias"))
                resultado.Almacenes = await InsertarAlmacenesAsync(destino, tx, entidadId, sucursalDestino.Value, almacenesVersat, cancellationToken);

            var mapaUnidades = new Dictionary<int, int>();
            if (on("unidades") || on("productos"))
            {
                var (unidadesInsertadas, mapa) = await InsertarUnidadesAsync(destino, tx, unidadesVersat, cancellationToken);
                resultado.UnidadesMedida = unidadesInsertadas;
                mapaUnidades = mapa;
            }

            var mapaFamilias = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var mapaIdFamilia = new Dictionary<int, Guid>();
            if (on("categorias") || on("productos"))
            {
                var (familiasInsertadas, mapa) = await InsertarFamiliasAsync(destino, tx, entidadId, familiasVersat, cancellationToken);
                resultado.Categorias = familiasInsertadas;
                mapaFamilias = mapa;
                foreach (var f in familiasVersat)
                    mapaIdFamilia[f.IdNivelClas] = mapa[f.IdNivelClas.ToString()];
            }

            if (on("productos"))
            {
                resultado.Productos = await InsertarProductosAsync(destino, tx, entidadId, productosVersat, mapaUnidades, mapaIdFamilia, tiposProductoVersat, cancellationToken);
                resultado.CodigosBarras = await ActualizarCodigosBarrasAsync(destino, tx, entidadId, cancellationToken);
                resultado.ProductoFamilias = await ActualizarFamiliasProductosAsync(destino, tx, entidadId, productosVersat, mapaIdFamilia, tiposProductoVersat, cancellationToken);
            }

            if (on("existencias"))
                resultado.Existencias = await InsertarExistenciasAsync(destino, tx, entidadId, existenciasVersat, cancellationToken);

            if (on("centrosCosto"))
                resultado.CentrosCosto = await InsertarCentrosCostoAsync(destino, tx, entidadId, sucursalDestino.Value, centrosCostoVersat, cancellationToken);

            if (on("conceptos"))
                resultado.Conceptos = await InsertarConceptosAsync(destino, tx, conceptosVersat, cancellationToken);

            var mapaCargos = new Dictionary<int, Guid>();
            if (on("cargos") || on("empleados"))
            {
                var (cargosInsertados, mapa) = await InsertarCargosAsync(destino, tx, entidadId, puestosVersat, cancellationToken);
                resultado.Cargos = cargosInsertados;
                mapaCargos = mapa;
            }

            if (on("clientes"))
                resultado.Clientes = await InsertarClientesAsync(destino, tx, entidadId, clientesVersat, cancellationToken);

            if (on("empleados"))
                resultado.Empleados = await InsertarEmpleadosAsync(destino, tx, entidadId, sucursalDestino.Value, empleadosVersat, mapaCargos, cancellationToken);

            if (on("precios"))
                resultado.Precios = await InsertarPreciosAsync(destino, tx, entidadId, preciosVersat, cancellationToken);

            if (on("registradoras"))
                resultado.DispositivosPos = await InsertarDispositivosPosAsync(destino, tx, entidadId, sucursalDestino.Value, registradorasVersat, cancellationToken);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        return resultado;
    }

    private string BuildConnectionString(ConexionRequest conexion)
    {
        return $"Server={conexion.Servidor};Database={conexion.BaseDatos};User Id={conexion.Usuario};Password={conexion.Password};TrustServerCertificate=True;";
    }

    private static async Task<List<(string Codigo, string Nombre, bool Activo)>> LeerAlmacenesAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT codigo, nombre, activo FROM gen_almacen WHERE activo = 1 ORDER BY codigo", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string, bool)>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0).Trim(), reader.GetString(1), reader.GetBoolean(2)));
        return result;
    }

    private static async Task<List<(int Id, string Clave, string Nombre)>> LeerUnidadesAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT idmedida, clave, descripcion FROM gen_medida ORDER BY idmedida", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(int, string, string)>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
        return result;
    }

    private static async Task<List<(int IdNivelClas, string ClaveNivel, string Nombre)>> LeerFamiliasAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT idnivelclas, clavenivel, descripcion FROM gen_nivelclasprod ORDER BY idnivelclas", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(int, string, string)>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetInt32(0), reader.GetString(1).Trim(), reader.GetString(2).Trim()));
        return result;
    }

    private static async Task<Dictionary<int, string>> LeerTiposProductoAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT e.idproducto,
                CASE c.nombre
                    WHEN 'Insumo' THEN 'MATERIA_PRIMA'
                    WHEN 'Mercancía para la venta' THEN 'MERCANCIA'
                    WHEN 'Producción Terminada' THEN 'TERMINADO'
                    WHEN 'Consignación' THEN 'MERCANCIA'
                    ELSE 'MERCANCIA'
                END AS tipo
            FROM inv_existencia e
            INNER JOIN inv_categoria c ON c.idcategoria = e.idcategoria", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new Dictionary<int, string>();
        while (await reader.ReadAsync(cancellationToken))
            result[reader.GetInt32(0)] = reader.GetString(1);
        return result;
    }

    private static async Task<List<(int IdProducto, string Codigo, string Descripcion, int IdMedida, bool Activo, decimal? Precio, int? IdNivelClas)>> LeerProductosAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT idproducto, codigo, descripcion, idmedida, activo, precio, idnivelclas FROM gen_producto WHERE activo = 1 ORDER BY codigo", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(int, string, string, int, bool, decimal?, int?)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add((
                reader.GetInt32(0),
                reader.GetString(1).Trim(),
                reader.GetString(2).Trim(),
                reader.GetInt32(3),
                reader.GetBoolean(4),
                reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                reader.IsDBNull(6) ? null : reader.GetInt32(6)
            ));
        }
        return result;
    }

    private static async Task<List<(string ProductoCodigo, string? AlmacenCodigo, decimal Cantidad, decimal Costo, decimal Minimo, decimal? Maximo)>> LeerExistenciasAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        const string sql = @"
            SELECT e.cantidad, e.preciocostoi, e.minimo, e.maximo, gp.codigo, ga.codigo
            FROM inv_existencia e
            INNER JOIN gen_producto gp ON gp.idproducto = e.idproducto
            LEFT JOIN inv_existenciaalm ea ON ea.idexistencia = e.idexistencia
            LEFT JOIN gen_almacen ga ON ga.idalmacen = ea.idalmacen AND ga.activo = 1
            WHERE e.cantidad > 0
            ORDER BY gp.codigo, ga.codigo";
        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string?, decimal, decimal, decimal, decimal?)>();
        var vistos = new HashSet<(string, string?)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var productoCodigo = reader.GetString(4).Trim();
            var almacenCodigo = reader.IsDBNull(5) ? null : reader.GetString(5).Trim();
            if (!vistos.Add((productoCodigo, almacenCodigo))) continue;
            result.Add((
                productoCodigo,
                almacenCodigo,
                reader.GetDecimal(0),
                reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                reader.IsDBNull(3) ? null : reader.GetDecimal(3)));
        }
        return result;
    }

    private static async Task<List<(string Codigo, string Nombre)>> LeerCentrosCostoAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT clave, descripcion FROM cos_centro WHERE activo = 1 ORDER BY clave", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string)>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0).Trim(), reader.GetString(1)));
        return result;
    }

    private static async Task<List<(string Codigo, string Nombre)>> LeerConceptosAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand("SELECT CAST(idconcepto AS NVARCHAR(20)), descripcion FROM inv_concepto WHERE activo = 1 ORDER BY idconcepto", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string)>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0).Trim(), reader.GetString(1)));
        return result;
    }

    private static async Task<List<(int PuestoId, string Codigo, string Nombre, decimal? Salario)>> LeerPuestosAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT idpuestotrabajo, codigo, str_descripcion, n_salario
            FROM nom_puestos_trb
            WHERE b_activo = 1
            ORDER BY codigo", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(int, string, string, decimal?)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add((
                reader.GetInt32(0),
                reader.GetString(1).Trim(),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetDecimal(3)));
        }
        return result;
    }

    private static async Task<List<(string Codigo, string Nombre, string? Nit, string? Direccion, string? Telefono, string? Email, bool Activo)>> LeerClientesAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT codigo, nombre, NIT, direccion, telefono, email, activo
            FROM gen_entidad
            WHERE activo = 1
            ORDER BY codigo", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string, string?, string?, string?, string?, bool)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add((
                reader.GetString(0).Trim(),
                reader.GetString(1).Trim(),
                reader.IsDBNull(2) ? null : reader.GetString(2).Trim(),
                reader.IsDBNull(3) ? null : reader.GetString(3).Trim(),
                reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                reader.IsDBNull(5) ? null : reader.GetString(5).Trim(),
                reader.GetBoolean(6)));
        }
        return result;
    }

    private static async Task<List<(string Carnet, string Nombres, string Apellidos, string? Direccion, string? Email, DateTime FechaIngreso, int PuestoId)>> LeerEmpleadosAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT nt.numident, nt.nombres, nt.Apellido1, nt.Apellido2, nt.direccion, nt.correo, nt.fecha, nt.idpuestotrabajo
            FROM (
                SELECT g.numident, g.nombres, g.Apellido1, g.Apellido2, g.direccion, g.correo,
                       n.fecha, n.idpuestotrabajo,
                       ROW_NUMBER() OVER (PARTITION BY g.numident ORDER BY n.fecha DESC, n.idpuestotrabajo DESC) rn
                FROM gen_trabajador g
                INNER JOIN nom_trabajadores n ON n.idtrabajador = g.idtrabajador AND n.fecha IS NOT NULL
                WHERE g.activo = 1 AND g.numident IS NOT NULL
            ) nt
            WHERE nt.rn = 1
            ORDER BY nt.numident", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string, string, string?, string?, DateTime, int)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var apellido1 = reader.GetString(2).Trim();
            var apellido2 = reader.IsDBNull(3) ? string.Empty : reader.GetString(3).Trim();
            result.Add((
                reader.GetString(0).Trim(),
                reader.GetString(1).Trim(),
                string.IsNullOrEmpty(apellido2) ? apellido1 : $"{apellido1} {apellido2}",
                reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                reader.IsDBNull(5) ? null : reader.GetString(5).Trim(),
                reader.GetDateTime(6),
                reader.GetInt32(7)));
        }
        return result;
    }

    private static async Task<List<(string Codigo, decimal Precio)>> LeerPreciosAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT CodigoBarras, Precio
            FROM pdv_Precios
            WHERE activo = 1
            ORDER BY CodigoBarras, Fecha DESC", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, decimal)>();
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            var codigo = reader.GetString(0).Trim();
            if (!vistos.Add(codigo)) continue;
            result.Add((codigo, reader.GetDecimal(1)));
        }
        return result;
    }

    private static async Task<List<(string Codigo, string Nombre, bool Activa)>> LeerRegistradorasAsync(string cadenaOrigen, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(cadenaOrigen);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(@"
            SELECT CAST(Id AS NVARCHAR(20)), Descripcion, Activa
            FROM pdv_Registradoras
            ORDER BY Id", conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<(string, string, bool)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add((
                reader.GetString(0).Trim(),
                reader.GetString(1).Trim(),
                reader.GetBoolean(2)));
        }
        return result;
    }

    private static async Task<Guid?> ObtenerPrimeraSucursalAsync(SqlConnection destino, Guid entidadId, CancellationToken cancellationToken)
    {
        using var cmd = new SqlCommand("SELECT TOP 1 id FROM nucleo.sucursal WHERE entidad_id = @EntidadId ORDER BY codigo", destino);
        cmd.Parameters.AddWithValue("@EntidadId", entidadId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is Guid guid ? guid : null;
    }

    private static async Task<int> InsertarAlmacenesAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, Guid sucursalId, List<(string Codigo, string Nombre, bool Activo)> almacenes, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM inventario.almacen WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                existentes.Add(reader.GetString(0));
        }

        const string sql = @"
            INSERT INTO inventario.almacen (id, entidad_id, sucursal_id, codigo, nombre, es_punto_venta, activo)
            VALUES (@Id, @EntidadId, @SucursalId, @Codigo, @Nombre, 0, @Activo)";

        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@SucursalId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300);
        cmd2.Parameters.Add("@Activo", SqlDbType.Bit);

        var insertados = 0;
        foreach (var a in almacenes)
        {
            var codigo = a.Codigo.Trim();
            if (existentes.Contains(codigo)) continue;

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@SucursalId"].Value = sucursalId;
            cmd2.Parameters["@Codigo"].Value = codigo;
            cmd2.Parameters["@Nombre"].Value = a.Nombre;
            cmd2.Parameters["@Activo"].Value = a.Activo;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(codigo);
            insertados++;
        }
        return insertados;
    }

    private static async Task<(int Insertados, Dictionary<int, int> MapaIdMedida)> InsertarUnidadesAsync(SqlConnection destino, SqlTransaction tx, List<(int Id, string Clave, string Nombre)> unidades, CancellationToken cancellationToken)
    {
        var preExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM inventario.unidad_medida", destino, tx))
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                preExistentes.Add(reader.GetString(0));
        }

        var usadasOrigen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codigosUnidad = new Dictionary<int, string>();
        foreach (var u in unidades)
        {
            var codigo = u.Clave.Trim();
            if (codigo.Length == 0) codigo = u.Id.ToString();
            if (!usadasOrigen.Add(codigo))
                codigo = string.Concat(codigo, "-", u.Id.ToString());
            codigosUnidad[u.Id] = codigo;
        }

        const string sql = "INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES (@Codigo, @Nombre, 0)";
        var insertados = 0;
        using (var cmd2 = new SqlCommand(sql, destino, tx))
        {
            cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 20);
            cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100);

            foreach (var u in unidades)
            {
                var codigo = codigosUnidad[u.Id];
                if (preExistentes.Contains(codigo)) continue;

                cmd2.Parameters["@Codigo"].Value = codigo;
                cmd2.Parameters["@Nombre"].Value = u.Nombre;
                await cmd2.ExecuteNonQueryAsync(cancellationToken);
                insertados++;
            }
        }

        var idPorCodigo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT id, codigo FROM inventario.unidad_medida", destino, tx))
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                idPorCodigo[reader.GetString(1)] = reader.GetInt32(0);
        }

        var mapa = new Dictionary<int, int>();
        foreach (var u in unidades)
        {
            if (idPorCodigo.TryGetValue(codigosUnidad[u.Id], out var idUnidad))
                mapa[u.Id] = idUnidad;
        }

        return (insertados, mapa);
    }

    private static async Task<(int Insertados, Dictionary<string, Guid> MapaClaveFamilia)> InsertarFamiliasAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(int IdNivelClas, string ClaveNivel, string Nombre)> familias, CancellationToken cancellationToken)
    {
        var mapa = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo, id FROM inventario.familia_producto WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                mapa[reader.GetString(0)] = reader.GetGuid(1);
        }

        const string sql = @"
            INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre, familia_padre_id)
            VALUES (@Id, @EntidadId, @Codigo, @Nombre, @PadreId)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200);
        cmd2.Parameters.Add("@PadreId", SqlDbType.UniqueIdentifier);

        var insertados = 0;
        var raices = familias.Where(f => f.ClaveNivel.Length == 3).OrderBy(f => f.ClaveNivel).ToList();
        var hojas = familias.Where(f => f.ClaveNivel.Length == 6).OrderBy(f => f.ClaveNivel).ToList();

        foreach (var f in raices)
        {
            var codigo = f.IdNivelClas.ToString();
            if (mapa.ContainsKey(codigo)) continue;

            var id = Guid.NewGuid();
            cmd2.Parameters["@Id"].Value = id;
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@Codigo"].Value = codigo;
            cmd2.Parameters["@Nombre"].Value = f.Nombre;
            cmd2.Parameters["@PadreId"].Value = DBNull.Value;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            mapa[codigo] = id;
            insertados++;
        }

        foreach (var f in hojas)
        {
            var codigo = f.IdNivelClas.ToString();
            if (mapa.ContainsKey(codigo)) continue;

            var claveRaiz = f.ClaveNivel.Substring(0, 3);
            Guid? padre = null;
            foreach (var r in raices)
            {
                if (r.ClaveNivel == claveRaiz && mapa.TryGetValue(r.IdNivelClas.ToString(), out var idRaiz))
                {
                    padre = idRaiz;
                    break;
                }
            }

            var id = Guid.NewGuid();
            cmd2.Parameters["@Id"].Value = id;
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@Codigo"].Value = codigo;
            cmd2.Parameters["@Nombre"].Value = f.Nombre;
            cmd2.Parameters["@PadreId"].Value = padre ?? (object)DBNull.Value;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            mapa[codigo] = id;
            insertados++;
        }

        return (insertados, mapa);
    }

    private static readonly HashSet<string> TiposProductoValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "MERCANCIA", "SERVICIO", "TERMINADO", "EN_PROCESO", "MATERIA_PRIMA"
    };

    private static string NormalizarTipoProducto(string tipo) =>
        TiposProductoValidos.Contains(tipo) ? tipo : "MERCANCIA";

    private static async Task<int> InsertarProductosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(int IdProducto, string Codigo, string Descripcion, int IdMedida, bool Activo, decimal? Precio, int? IdNivelClas)> productos, Dictionary<int, int> mapaUnidades, Dictionary<int, Guid> mapaIdFamilia, Dictionary<int, string> tiposProducto, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM inventario.producto WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                existentes.Add(reader.GetString(0));
        }

        const string sql = @"
            INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
            VALUES (@Id, @EntidadId, @Codigo, @Codigo, @Nombre, @Descripcion, @FamiliaId, @UnidadMedidaId, @Tipo, @Precio, 0, @Activo, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 60);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 400);
        cmd2.Parameters.Add("@Descripcion", SqlDbType.NVarChar, -1);
        cmd2.Parameters.Add("@FamiliaId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@UnidadMedidaId", SqlDbType.Int);
        cmd2.Parameters.Add("@Tipo", SqlDbType.NVarChar, 50);
        cmd2.Parameters.Add("@Precio", SqlDbType.Decimal);
        cmd2.Parameters.Add("@Activo", SqlDbType.Bit);

        var insertados = 0;
        foreach (var p in productos)
        {
            if (existentes.Contains(p.Codigo)) continue;
            if (!mapaUnidades.TryGetValue(p.IdMedida, out var unidadMedidaId))
                throw new InvalidOperationException($"El producto '{p.Codigo}' referencia la unidad de medida {p.IdMedida}, que no existe en inventario.unidad_medida.");

            Guid? familiaId = null;
            if (p.IdNivelClas.HasValue && mapaIdFamilia.TryGetValue(p.IdNivelClas.Value, out var fid))
                familiaId = fid;

            var tipo = NormalizarTipoProducto(tiposProducto.TryGetValue(p.IdProducto, out var t) ? t : "MERCANCIA");

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@Codigo"].Value = p.Codigo;
            cmd2.Parameters["@Nombre"].Value = p.Descripcion;
            cmd2.Parameters["@Descripcion"].Value = p.Descripcion;
            cmd2.Parameters["@FamiliaId"].Value = familiaId ?? (object)DBNull.Value;
            cmd2.Parameters["@UnidadMedidaId"].Value = unidadMedidaId;
            cmd2.Parameters["@Tipo"].Value = tipo;
            cmd2.Parameters["@Precio"].Value = p.Precio ?? 0m;
            cmd2.Parameters["@Activo"].Value = p.Activo;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(p.Codigo);
            insertados++;
        }
        return insertados;
    }

    private static async Task<int> ActualizarCodigosBarrasAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, CancellationToken cancellationToken)
    {
        using var cmd = new SqlCommand(@"
            UPDATE inventario.producto
            SET codigo_barras = codigo
            WHERE entidad_id = @EntidadId AND codigo_barras IS NULL", destino, tx);
        cmd.Parameters.AddWithValue("@EntidadId", entidadId);
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> ActualizarFamiliasProductosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(int IdProducto, string Codigo, string Descripcion, int IdMedida, bool Activo, decimal? Precio, int? IdNivelClas)> productos, Dictionary<int, Guid> mapaIdFamilia, Dictionary<int, string> tiposProducto, CancellationToken cancellationToken)
    {
        using var cmd = new SqlCommand(@"
            UPDATE inventario.producto
            SET familia_id = @FamiliaId, tipo = @Tipo
            WHERE entidad_id = @EntidadId AND codigo = @Codigo AND familia_id IS NULL", destino, tx);
        cmd.Parameters.Add("@FamiliaId", SqlDbType.UniqueIdentifier);
        cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, 50);
        cmd.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd.Parameters.Add("@Codigo", SqlDbType.NVarChar, 60);
        cmd.Parameters["@EntidadId"].Value = entidadId;

        var actualizados = 0;
        foreach (var p in productos)
        {
            if (!p.IdNivelClas.HasValue) continue;
            if (!mapaIdFamilia.TryGetValue(p.IdNivelClas.Value, out var familiaId)) continue;

            var tipo = NormalizarTipoProducto(tiposProducto.TryGetValue(p.IdProducto, out var t) ? t : "MERCANCIA");

            cmd.Parameters["@FamiliaId"].Value = familiaId;
            cmd.Parameters["@Tipo"].Value = tipo;
            cmd.Parameters["@Codigo"].Value = p.Codigo;
            actualizados += await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        return actualizados;
    }

    private static async Task<int> InsertarExistenciasAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(string ProductoCodigo, string? AlmacenCodigo, decimal Cantidad, decimal Costo, decimal Minimo, decimal? Maximo)> existencias, CancellationToken cancellationToken)
    {
        using (var cmdDel = new SqlCommand(@"
            DELETE ee FROM inventario.existencia ee
            INNER JOIN inventario.producto p ON ee.producto_id = p.id
            WHERE p.entidad_id = @EntidadId", destino, tx))
        {
            cmdDel.Parameters.AddWithValue("@EntidadId", entidadId);
            await cmdDel.ExecuteNonQueryAsync(cancellationToken);
        }

        var productosPorCodigo = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo, id FROM inventario.producto WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                productosPorCodigo[reader.GetString(0)] = reader.GetGuid(1);
        }

        var almacenPorCodigo = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        Guid? almacenFallback = null;
        using (var cmd = new SqlCommand("SELECT codigo, id FROM inventario.almacen WHERE entidad_id = @EntidadId ORDER BY codigo", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var codigo = reader.GetString(0);
                var id = reader.GetGuid(1);
                almacenPorCodigo[codigo] = id;
                almacenFallback ??= id;
            }
        }

        const string sql = @"
            INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, stock_minimo, stock_maximo, actualizado_en)
            VALUES (@Id, @AlmacenId, @ProductoId, @Cantidad, @Costo, @Minimo, @Maximo, SYSDATETIMEOFFSET())";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@AlmacenId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@ProductoId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Cantidad", SqlDbType.Decimal);
        cmd2.Parameters.Add("@Costo", SqlDbType.Decimal);
        cmd2.Parameters.Add("@Minimo", SqlDbType.Decimal);
        cmd2.Parameters.Add("@Maximo", SqlDbType.Decimal);

        var insertados = 0;
        var porProducto = existencias
            .GroupBy(e => e.ProductoCodigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var grupo in porProducto)
        {
            if (!productosPorCodigo.TryGetValue(grupo.Key, out var productoId)) continue;

            var acumulado = new Dictionary<Guid, (decimal Cantidad, decimal Costo, decimal Minimo, decimal? Maximo)>();
            var sinAlmacen = new List<(decimal Cantidad, decimal Costo, decimal Minimo, decimal? Maximo)>();

            foreach (var e in grupo.Value)
            {
                if (string.IsNullOrEmpty(e.AlmacenCodigo) || !almacenPorCodigo.TryGetValue(e.AlmacenCodigo, out var almacenId))
                {
                    sinAlmacen.Add((e.Cantidad, e.Costo, e.Minimo, e.Maximo));
                    continue;
                }

                if (acumulado.TryGetValue(almacenId, out var prev))
                    acumulado[almacenId] = (prev.Cantidad + e.Cantidad, prev.Costo, Math.Max(prev.Minimo, e.Minimo), prev.Maximo ?? e.Maximo);
                else
                    acumulado[almacenId] = (e.Cantidad, e.Costo, e.Minimo, e.Maximo);
            }

            if (sinAlmacen.Count > 0 && almacenFallback.HasValue)
            {
                var total = sinAlmacen.Sum(r => r.Cantidad);
                var primero = sinAlmacen[0];
                if (acumulado.TryGetValue(almacenFallback.Value, out var prev))
                    acumulado[almacenFallback.Value] = (prev.Cantidad + total, prev.Costo, Math.Max(prev.Minimo, primero.Minimo), prev.Maximo ?? primero.Maximo);
                else
                    acumulado[almacenFallback.Value] = (total, primero.Costo, primero.Minimo, primero.Maximo);
            }

            foreach (var (almacenId, v) in acumulado)
            {
                cmd2.Parameters["@Id"].Value = Guid.NewGuid();
                cmd2.Parameters["@AlmacenId"].Value = almacenId;
                cmd2.Parameters["@ProductoId"].Value = productoId;
                cmd2.Parameters["@Cantidad"].Value = v.Cantidad;
                cmd2.Parameters["@Costo"].Value = v.Costo;
                cmd2.Parameters["@Minimo"].Value = v.Minimo;
                cmd2.Parameters["@Maximo"].Value = v.Maximo.HasValue ? v.Maximo.Value : DBNull.Value;
                await cmd2.ExecuteNonQueryAsync(cancellationToken);
                insertados++;
            }
        }
        return insertados;
    }

    private static async Task<int> InsertarCentrosCostoAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, Guid sucursalId, List<(string Codigo, string Nombre)> centrosCosto, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM contabilidad.centro_costo WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                existentes.Add(reader.GetString(0));
        }

        const string sql = @"
            INSERT INTO contabilidad.centro_costo (id, entidad_id, codigo, nombre, sucursal_id, activo)
            VALUES (@Id, @EntidadId, @Codigo, @Nombre, @SucursalId, 1)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300);
        cmd2.Parameters.Add("@SucursalId", SqlDbType.UniqueIdentifier);

        var insertados = 0;
        foreach (var c in centrosCosto)
        {
            if (existentes.Contains(c.Codigo)) continue;

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@Codigo"].Value = c.Codigo;
            cmd2.Parameters["@Nombre"].Value = c.Nombre;
            cmd2.Parameters["@SucursalId"].Value = sucursalId;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(c.Codigo);
            insertados++;
        }
        return insertados;
    }

    private static async Task<int> InsertarConceptosAsync(SqlConnection destino, SqlTransaction tx, List<(string Codigo, string Nombre)> conceptos, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM inventario.tipo_movimiento", destino, tx))
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                existentes.Add(reader.GetString(0));
        }

        const string sql = @"
            INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo)
            VALUES (@Codigo, @Nombre, @Naturaleza, 1)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300);
        cmd2.Parameters.Add("@Naturaleza", SqlDbType.NVarChar, 20);

        var insertados = 0;
        foreach (var c in conceptos)
        {
            if (existentes.Contains(c.Codigo)) continue;

            cmd2.Parameters["@Codigo"].Value = c.Codigo;
            cmd2.Parameters["@Nombre"].Value = c.Nombre;
            cmd2.Parameters["@Naturaleza"].Value = InferirNaturaleza(c.Nombre);
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(c.Codigo);
            insertados++;
        }
        return insertados;
    }

    private static string InferirNaturaleza(string nombre)
    {
        var salida = new[] { "salida", "baja", "descuento", "egreso", "consumo", "venta", "comercializ" };
        if (salida.Any(k => nombre.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
            return "SALIDA";
        return "ENTRADA";
    }

    private static async Task<(int Insertados, Dictionary<int, Guid> MapaPuestoCargo)> InsertarCargosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(int PuestoId, string Codigo, string Nombre, decimal? Salario)> puestos, CancellationToken cancellationToken)
        => await ResolverCargosAsync(destino, tx, entidadId, puestos, cancellationToken);

    private static async Task<(int Insertados, Dictionary<int, Guid> MapaPuestoCargo)> ResolverCargosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(int PuestoId, string Codigo, string Nombre, decimal? Salario)> puestos, CancellationToken cancellationToken)
    {
        var cargos = new List<(Guid Id, string Codigo, string NombreNormalizado)>();
        using (var cmd = new SqlCommand("SELECT id, codigo, nombre FROM rrhh.cargo WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                cargos.Add((reader.GetGuid(0), reader.GetString(1), NormalizarNombre(reader.GetString(2))));
        }

        const string sql = @"
            INSERT INTO rrhh.cargo (id, entidad_id, codigo, nombre, salario_escala_min, salario_escala_max, funciones, categoria_ocupacional)
            VALUES (@Id, @EntidadId, @Codigo, @Nombre, @SalarioMin, @SalarioMax, NULL, NULL)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300);
        cmd2.Parameters.Add("@SalarioMin", SqlDbType.Decimal);
        cmd2.Parameters.Add("@SalarioMax", SqlDbType.Decimal);

        var insertados = 0;
        var mapa = new Dictionary<int, Guid>();
        foreach (var p in puestos)
        {
            var nombreNorm = NormalizarNombre(p.Nombre);
            var codigoNorm = NormalizarCodigo(p.Codigo);

            var idCargo = EncontrarCargoPorNombre(cargos, nombreNorm, codigoNorm);
            if (idCargo == null)
            {
                var sufijo = 0;
                var codigoNuevo = codigoNorm;
                while (cargos.Any(c => c.Codigo.Equals(codigoNuevo, StringComparison.OrdinalIgnoreCase)))
                    codigoNuevo = codigoNorm + "-V" + (++sufijo);

                var salario = p.Salario ?? 0m;
                var nuevoId = Guid.NewGuid();
                cmd2.Parameters["@Id"].Value = nuevoId;
                cmd2.Parameters["@EntidadId"].Value = entidadId;
                cmd2.Parameters["@Codigo"].Value = codigoNuevo;
                cmd2.Parameters["@Nombre"].Value = p.Nombre;
                cmd2.Parameters["@SalarioMin"].Value = salario;
                cmd2.Parameters["@SalarioMax"].Value = salario;
                await cmd2.ExecuteNonQueryAsync(cancellationToken);

                cargos.Add((nuevoId, codigoNuevo, nombreNorm));
                idCargo = nuevoId;
                insertados++;
            }

            mapa[p.PuestoId] = idCargo.Value;
        }

        return (insertados, mapa);
    }

    private static Guid? EncontrarCargoPorNombre(List<(Guid Id, string Codigo, string NombreNormalizado)> cargos, string nombreNorm, string codigoNorm)
    {
        var exactos = cargos.Where(c => c.NombreNormalizado == nombreNorm).ToList();
        Guid? primero = null;
        foreach (var c in exactos)
        {
            primero ??= c.Id;
            if (c.Codigo.Equals(codigoNorm, StringComparison.OrdinalIgnoreCase))
                return c.Id;
        }
        if (primero.HasValue) return primero;

        var contencion = cargos.Where(c => nombreNorm.Length >= 3 && c.NombreNormalizado.Length >= 3 &&
            (c.NombreNormalizado.Contains(nombreNorm) || nombreNorm.Contains(c.NombreNormalizado))).ToList();
        return contencion.Count == 1 ? contencion[0].Id : null;
    }

    private static string NormalizarCodigo(string codigo)
    {
        var texto = codigo.Trim();
        return int.TryParse(texto, out var numero) ? numero.ToString() : texto;
    }

    private static string NormalizarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return string.Empty;
        var normalizado = nombre.Normalize(System.Text.NormalizationForm.FormD);
        var sinAcentos = new System.Text.StringBuilder(normalizado.Length);
        foreach (var ch in normalizado)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sinAcentos.Append(ch);
        }
        var texto = sinAcentos.ToString().Normalize(System.Text.NormalizationForm.FormC);
        return System.Text.RegularExpressions.Regex.Replace(texto.Trim(), @"\s+", " ").ToUpperInvariant();
    }

    private static DateTime? ExtraerFechaNacimiento(string carnet)
    {
        if (string.IsNullOrWhiteSpace(carnet) || carnet.Length < 6)
            return null;
        var primitivos = new string(carnet.Trim().TakeWhile(char.IsDigit).ToArray());
        if (primitivos.Length < 6)
            return null;
        var prefijo = primitivos.Substring(0, 6);
        return DateTime.TryParseExact(prefijo, "yyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeLocal,
            out var fecha) ? fecha.Date : (DateTime?)null;
    }

    private static async Task<int> InsertarClientesAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(string Codigo, string Nombre, string? Nit, string? Direccion, string? Telefono, string? Email, bool Activo)> clientes, CancellationToken cancellationToken)
    {
        var nitExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nombreExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT nit_o_ci, nombre_razon_social FROM comercial.cliente WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (!reader.IsDBNull(0) && !string.IsNullOrWhiteSpace(reader.GetString(0)))
                    nitExistentes.Add(reader.GetString(0).Trim());
                if (!reader.IsDBNull(1))
                    nombreExistentes.Add(reader.GetString(1).Trim());
            }
        }

        const string sql = @"
            INSERT INTO comercial.cliente (id, entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion, telefono, email, segmento, lista_precio_id, limite_credito, cuenta_contable_id, activo, creado_en)
            VALUES (@Id, @EntidadId, 'JURIDICA', @Nit, @Nombre, @Direccion, @Telefono, @Email, 'MINORISTA', NULL, 0, NULL, @Activo, SYSDATETIMEOFFSET())";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Nit", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Nombre", SqlDbType.NVarChar, 400);
        cmd2.Parameters.Add("@Direccion", SqlDbType.NVarChar, 500);
        cmd2.Parameters.Add("@Telefono", SqlDbType.NVarChar, 40);
        cmd2.Parameters.Add("@Email", SqlDbType.NVarChar, 200);
        cmd2.Parameters.Add("@Activo", SqlDbType.Bit);

        var insertados = 0;
        foreach (var c in clientes)
        {
            var nombre = c.Nombre;
            var nit = c.Nit;
            if ((!string.IsNullOrEmpty(nit) && nitExistentes.Contains(nit)) || nombreExistentes.Contains(nombre)) continue;

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@Nit"].Value = string.IsNullOrEmpty(nit) ? DBNull.Value : nit;
            cmd2.Parameters["@Nombre"].Value = nombre;
            cmd2.Parameters["@Direccion"].Value = string.IsNullOrEmpty(c.Direccion) ? DBNull.Value : c.Direccion;
            cmd2.Parameters["@Telefono"].Value = string.IsNullOrEmpty(c.Telefono) ? DBNull.Value : c.Telefono;
            cmd2.Parameters["@Email"].Value = string.IsNullOrEmpty(c.Email) ? DBNull.Value : c.Email;
            cmd2.Parameters["@Activo"].Value = c.Activo;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            if (!string.IsNullOrEmpty(nit)) nitExistentes.Add(nit);
            nombreExistentes.Add(nombre);
            insertados++;
        }
        return insertados;
    }

    private static async Task<int> InsertarEmpleadosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, Guid sucursalId, List<(string Carnet, string Nombres, string Apellidos, string? Direccion, string? Email, DateTime FechaIngreso, int PuestoId)> empleados, Dictionary<int, Guid> mapaCargos, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT carnet_identidad FROM rrhh.empleado", destino, tx))
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                if (!reader.IsDBNull(0))
                    existentes.Add(reader.GetString(0).Trim());
        }

        const string sql = @"
            INSERT INTO rrhh.empleado (id, entidad_id, sucursal_id, carnet_identidad, nombres, apellidos, fecha_nacimiento, sexo, direccion, telefono, email, cargo_id, calificacion, nivel_escolaridad, fecha_ingreso, fecha_baja, motivo_baja, cuenta_bancaria_pago, estado, creado_en, turno_trabajo_id)
            VALUES (@Id, @EntidadId, @SucursalId, @Carnet, @Nombres, @Apellidos, @FechaNacimiento, NULL, @Direccion, NULL, @Email, @CargoId, NULL, NULL, @FechaIngreso, NULL, NULL, NULL, 'ACTIVO', SYSDATETIMEOFFSET(), NULL)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@SucursalId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Carnet", SqlDbType.NVarChar, 60);
        cmd2.Parameters.Add("@Nombres", SqlDbType.NVarChar, 200);
        cmd2.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 200);
        cmd2.Parameters.Add("@FechaNacimiento", SqlDbType.DateTime2);
        cmd2.Parameters.Add("@Direccion", SqlDbType.NVarChar, 500);
        cmd2.Parameters.Add("@Email", SqlDbType.NVarChar, 200);
        cmd2.Parameters.Add("@CargoId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@FechaIngreso", SqlDbType.DateTime2);

        var insertados = 0;
        foreach (var e in empleados)
        {
            if (string.IsNullOrEmpty(e.Carnet)) continue;
            if (existentes.Contains(e.Carnet)) continue;
            if (!mapaCargos.TryGetValue(e.PuestoId, out var cargoId)) continue;

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@EntidadId"].Value = entidadId;
            cmd2.Parameters["@SucursalId"].Value = sucursalId;
            cmd2.Parameters["@Carnet"].Value = e.Carnet;
            cmd2.Parameters["@Nombres"].Value = e.Nombres;
            cmd2.Parameters["@Apellidos"].Value = e.Apellidos;
            cmd2.Parameters["@FechaNacimiento"].Value = ExtraerFechaNacimiento(e.Carnet) ?? e.FechaIngreso;
            cmd2.Parameters["@Direccion"].Value = string.IsNullOrEmpty(e.Direccion) ? DBNull.Value : e.Direccion;
            cmd2.Parameters["@Email"].Value = string.IsNullOrEmpty(e.Email) ? DBNull.Value : e.Email;
            cmd2.Parameters["@CargoId"].Value = cargoId;
            cmd2.Parameters["@FechaIngreso"].Value = e.FechaIngreso;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(e.Carnet);
            insertados++;
        }
        return insertados;
    }

    private static async Task<int> InsertarPreciosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, List<(string Codigo, decimal Precio)> precios, CancellationToken cancellationToken)
    {
        var nombreLista = "Precios PDV Versat";
        Guid listaPrecioId;
        using (var cmd = new SqlCommand("SELECT id FROM inventario.lista_precio WHERE entidad_id = @EntidadId AND nombre = @Nombre", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            cmd.Parameters.AddWithValue("@Nombre", nombreLista);
            var valor = await cmd.ExecuteScalarAsync(cancellationToken);
            listaPrecioId = valor is Guid guid ? guid : Guid.Empty;
        }

        if (listaPrecioId == Guid.Empty)
        {
            listaPrecioId = Guid.NewGuid();
            using var cmd = new SqlCommand(@"
                INSERT INTO inventario.lista_precio (id, entidad_id, nombre, canal, vigente_desde, vigente_hasta, activa)
                VALUES (@Id, @EntidadId, @Nombre, 'POS', CAST(SYSDATETIME() AS date), NULL, 1)", destino, tx);
            cmd.Parameters.AddWithValue("@Id", listaPrecioId);
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            cmd.Parameters.AddWithValue("@Nombre", nombreLista);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        using (var cmdDel = new SqlCommand("DELETE FROM inventario.lista_precio_detalle WHERE lista_precio_id = @ListaPrecioId", destino, tx))
        {
            cmdDel.Parameters.AddWithValue("@ListaPrecioId", listaPrecioId);
            await cmdDel.ExecuteNonQueryAsync(cancellationToken);
        }

        var productosPorCodigo = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo, id FROM inventario.producto WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                productosPorCodigo[reader.GetString(0)] = reader.GetGuid(1);
        }

        const string sql = @"
            INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio)
            VALUES (@Id, @ListaPrecioId, @ProductoId, @Precio)";
        using var cmd2 = new SqlCommand(sql, destino, tx);
        cmd2.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@ListaPrecioId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@ProductoId", SqlDbType.UniqueIdentifier);
        cmd2.Parameters.Add("@Precio", SqlDbType.Decimal);

        var insertados = 0;
        foreach (var p in precios)
        {
            if (!productosPorCodigo.TryGetValue(p.Codigo, out var productoId)) continue;

            cmd2.Parameters["@Id"].Value = Guid.NewGuid();
            cmd2.Parameters["@ListaPrecioId"].Value = listaPrecioId;
            cmd2.Parameters["@ProductoId"].Value = productoId;
            cmd2.Parameters["@Precio"].Value = p.Precio;
            await cmd2.ExecuteNonQueryAsync(cancellationToken);
            insertados++;
        }
        return insertados;
    }

    private static async Task<int> InsertarDispositivosPosAsync(SqlConnection destino, SqlTransaction tx, Guid entidadId, Guid sucursalId, List<(string Codigo, string Nombre, bool Activa)> registradoras, CancellationToken cancellationToken)
    {
        var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqlCommand("SELECT codigo FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                existentes.Add(reader.GetString(0));
        }

        Guid? cuentaContableId = null;
        using (var cmd = new SqlCommand("SELECT TOP 1 cuenta_contable_id FROM contabilidad.caja WHERE entidad_id = @EntidadId ORDER BY nombre", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            var valor = await cmd.ExecuteScalarAsync(cancellationToken);
            if (valor is Guid guid) cuentaContableId = guid;
        }

        Guid? almacenFallback = null;
        using (var cmd = new SqlCommand("SELECT TOP 1 id FROM inventario.almacen WHERE entidad_id = @EntidadId ORDER BY codigo", destino, tx))
        {
            cmd.Parameters.AddWithValue("@EntidadId", entidadId);
            var valor = await cmd.ExecuteScalarAsync(cancellationToken);
            if (valor is Guid guid) almacenFallback = guid;
        }

        const string sqlCaja = @"
            INSERT INTO contabilidad.caja (id, entidad_id, sucursal_id, nombre, cuenta_contable_id, limite_efectivo, saldo_actual, activa)
            VALUES (@Id, @EntidadId, @SucursalId, @Nombre, @CuentaContableId, NULL, 0, @Activa)";
        using var cmdCaja = new SqlCommand(sqlCaja, destino, tx);
        cmdCaja.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmdCaja.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmdCaja.Parameters.Add("@SucursalId", SqlDbType.UniqueIdentifier);
        cmdCaja.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200);
        cmdCaja.Parameters.Add("@CuentaContableId", SqlDbType.UniqueIdentifier);
        cmdCaja.Parameters.Add("@Activa", SqlDbType.Bit);

        const string sqlPos = @"
            INSERT INTO integracion.dispositivo_pos (id, entidad_id, sucursal_id, almacen_id, api_cliente_id, codigo, nombre, identificador_hardware, caja_id, ultima_sincronizacion, version_app_pos, estado, creado_en)
            VALUES (@Id, @EntidadId, @SucursalId, @AlmacenId, NULL, @Codigo, @Nombre, NULL, @CajaId, NULL, NULL, @Estado, SYSDATETIMEOFFSET())";
        using var cmdPos = new SqlCommand(sqlPos, destino, tx);
        cmdPos.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
        cmdPos.Parameters.Add("@EntidadId", SqlDbType.UniqueIdentifier);
        cmdPos.Parameters.Add("@SucursalId", SqlDbType.UniqueIdentifier);
        cmdPos.Parameters.Add("@AlmacenId", SqlDbType.UniqueIdentifier);
        cmdPos.Parameters.Add("@Codigo", SqlDbType.NVarChar, 40);
        cmdPos.Parameters.Add("@Nombre", SqlDbType.NVarChar, 300);
        cmdPos.Parameters.Add("@CajaId", SqlDbType.UniqueIdentifier);
        cmdPos.Parameters.Add("@Estado", SqlDbType.NVarChar, 20);

        var insertados = 0;
        foreach (var r in registradoras)
        {
            if (existentes.Contains(r.Codigo)) continue;
            if (!almacenFallback.HasValue || !cuentaContableId.HasValue) break;

            var cajaId = Guid.NewGuid();
            cmdCaja.Parameters["@Id"].Value = cajaId;
            cmdCaja.Parameters["@EntidadId"].Value = entidadId;
            cmdCaja.Parameters["@SucursalId"].Value = sucursalId;
            cmdCaja.Parameters["@Nombre"].Value = "Caja POS " + r.Nombre;
            cmdCaja.Parameters["@CuentaContableId"].Value = cuentaContableId.Value;
            cmdCaja.Parameters["@Activa"].Value = r.Activa;
            await cmdCaja.ExecuteNonQueryAsync(cancellationToken);

            cmdPos.Parameters["@Id"].Value = Guid.NewGuid();
            cmdPos.Parameters["@EntidadId"].Value = entidadId;
            cmdPos.Parameters["@SucursalId"].Value = sucursalId;
            cmdPos.Parameters["@AlmacenId"].Value = almacenFallback.Value;
            cmdPos.Parameters["@Codigo"].Value = r.Codigo;
            cmdPos.Parameters["@Nombre"].Value = r.Nombre;
            cmdPos.Parameters["@CajaId"].Value = cajaId;
            cmdPos.Parameters["@Estado"].Value = r.Activa ? "ACTIVO" : "INACTIVO";
            await cmdPos.ExecuteNonQueryAsync(cancellationToken);

            existentes.Add(r.Codigo);
            insertados++;
        }

        return insertados;
    }
}

public class ImportacionResumen
{
    public int Almacenes { get; set; }
    public int UnidadesMedida { get; set; }
    public int Categorias { get; set; }
    public int Productos { get; set; }
    public int Existencias { get; set; }
    public int Conceptos { get; set; }
    public int CentrosCosto { get; set; }
    public int Precios { get; set; }
    public int Clientes { get; set; }
    public int Cargos { get; set; }
    public int Empleados { get; set; }
    public int DispositivosPos { get; set; }
}

public class ImportacionResultado
{
    public int Almacenes { get; set; }
    public int UnidadesMedida { get; set; }
    public int Categorias { get; set; }
    public int Productos { get; set; }
    public int CodigosBarras { get; set; }
    public int ProductoFamilias { get; set; }
    public int Existencias { get; set; }
    public int Conceptos { get; set; }
    public int CentrosCosto { get; set; }
    public int Precios { get; set; }
    public int Clientes { get; set; }
    public int Cargos { get; set; }
    public int Empleados { get; set; }
    public int DispositivosPos { get; set; }
    public bool Exito => true;
    public string Mensaje => "Importación completada correctamente";
}

public class TestConexionResultado
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class ProductoVersatDto
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int IdMedida { get; set; }
    public bool Activo { get; set; }
    public decimal? Precio { get; set; }
    public string? UnidadClave { get; set; }
    public string? UnidadNombre { get; set; }
}

public class ExistenciaVersatDto
{
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal Minimo { get; set; }
    public decimal Maximo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? IdAlmacen { get; set; }
    public string? AlmacenNombre { get; set; }
}

public class AlmacenVersatDto
{
    public int IdAlmacen { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
