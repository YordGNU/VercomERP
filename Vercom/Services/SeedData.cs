using Microsoft.EntityFrameworkCore;
using VERCOM.Models;

namespace VERCOM.Services;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<AppDbContext>();
        var authService = serviceProvider.GetRequiredService<IAuthService>();

        // ======================================================================
        // GUARDIA: Si ya hay usuarios, el seed ya se ejecutÃ³
        // ======================================================================
        if (await context.Usuarios.IgnoreQueryFilters().AnyAsync())
            return;

        Console.WriteLine("ðŸŒ± Iniciando seed de datos base...");

        // ======================================================================
        // 1. ENTIDAD
        // ======================================================================
        var entidad = await context.Entidads.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (entidad == null)
        {
            entidad = new Entidad
            {
                Id = Guid.NewGuid(),
                RazonSocial = "S.U.R.L.",
                NombreComercial = "VERCOM ERP",
                Nit = "90000000000",
                CodigoReeup = "REEUP2026001",
                FormaJuridica = "S.U.R.L.",
                DireccionLegal = "Calle MartÃ­ #123, HolguÃ­n",
                Municipio = "HolguÃ­n",
                Provincia = "HolguÃ­n",
                Telefono = "+53 24 123456",
                Email = "info@email.cu",
                MonedaBase = "CUP",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
                ActualizadoEn = DateTimeOffset.UtcNow
            };
            context.Entidads.Add(entidad);
            await context.SaveChangesAsync();
        }
        var entidadId = entidad.Id;

        // ======================================================================
        // 2. SUCURSAL
        // ======================================================================
        var sucursal = await context.Sucursals.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.EntidadId == entidadId);
        if (sucursal == null)
        {
            sucursal = new Sucursal
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Codigo = "MATRIZ",
                Nombre = "Sucursal Central",
                Tipo = "MIXTO",
                Direccion = "Calle MartÃ­ #123, HolguÃ­n",
                Municipio = "HolguÃ­n",
                Provincia = "HolguÃ­n",
                Telefono = "+53 24 123456",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow
            };
            context.Sucursals.Add(sucursal);
            await context.SaveChangesAsync();
        }
        var sucursalId = sucursal.Id;

        // ======================================================================
        // 3. PERMISOS
        // ======================================================================
        await SeedPermisosAsync(context);

        // ======================================================================
        // 4. ROLES
        // ======================================================================
        await SeedRolesAsync(context, entidadId);

        // ======================================================================
        // 5. ASIGNACIÃ“N DE PERMISOS A ROLES
        // ======================================================================
        await SeedRolPermisosAsync(context);

        // ======================================================================
        // 6. USUARIOS
        // ======================================================================
        await SeedUsuariosAsync(context, authService, entidadId, sucursalId);

        // ======================================================================
        // 7. CARGOS
        // ======================================================================
        await SeedCargosAsync(context, entidadId);

        // ======================================================================
        // 8. UNIDADES DE MEDIDA
        // ======================================================================
        await SeedUnidadesMedidaAsync(context);

        // ======================================================================
        // 9. TIPOS DE MOVIMIENTO
        // ======================================================================
        await SeedTiposMovimientoAsync(context);

        // ======================================================================
        // 10. TIPOS DE AUSENCIA
        // ======================================================================
        await SeedTiposAusenciaAsync(context);

        // ======================================================================
        // 11. TIPOS DE COMPROBANTE
        // ======================================================================
        await SeedTiposComprobanteAsync(context);

        // ======================================================================
        // 12. CONCEPTOS DE NÃ“MINA
        // ======================================================================
        await SeedConceptosNominaAsync(context, entidadId);

        // ======================================================================
        // 13. INDICADORES BI
        // ======================================================================
        await SeedIndicadoresAsync(context);

        // ======================================================================
        // 14. PARÃMETROS FISCALES
        // ======================================================================
        await SeedParametrosFiscalesAsync(context, entidadId);

        // ======================================================================
        // 15. TIPOS DE OBLIGACIÃ“N FISCAL
        // ======================================================================
        await SeedTiposObligacionFiscalAsync(context);

        // ======================================================================
        // 16. ALMACENES
        // ======================================================================
        await SeedAlmacenesAsync(context, entidadId, sucursalId);

        // ======================================================================
        // 17. PLAN DE CUENTAS BÃSICO
        // ======================================================================
        await SeedPlanCuentasAsync(context, entidadId);

        // ======================================================================
        // 18. LISTAS DE PRECIO
        // ======================================================================
        await SeedListasPrecioAsync(context, entidadId);

        Console.WriteLine("âœ… Seed de datos base completado exitosamente");
        Console.WriteLine($"   Entidad: {entidad.RazonSocial} (NIT: {entidad.Nit})");
        Console.WriteLine($"   Sucursal: {sucursal.Nombre}");
        Console.WriteLine($"   Usuarios: master / admin");
    }

    // ==========================================================================
    // 3. PERMISOS
    // ==========================================================================
    private static async Task SeedPermisosAsync(AppDbContext context)
    {
        var permisos = new List<Permiso>
        {
            // ------------------------------------------------------------------
            // SEGURIDAD
            // ------------------------------------------------------------------
            new() { Codigo = "SEGURIDAD.USUARIO.VER", Modulo = "Seguridad", Descripcion = "Ver listado de usuarios" },
            new() { Codigo = "SEGURIDAD.USUARIO.CREAR", Modulo = "Seguridad", Descripcion = "Crear usuarios" },
            new() { Codigo = "SEGURIDAD.USUARIO.EDITAR", Modulo = "Seguridad", Descripcion = "Editar usuarios" },
            new() { Codigo = "SEGURIDAD.USUARIO.ELIMINAR", Modulo = "Seguridad", Descripcion = "Eliminar usuarios" },
            new() { Codigo = "SEGURIDAD.ROL.VER", Modulo = "Seguridad", Descripcion = "Ver roles y permisos" },
            new() { Codigo = "SEGURIDAD.ROL.CREAR", Modulo = "Seguridad", Descripcion = "Crear roles" },
            new() { Codigo = "SEGURIDAD.ROL.EDITAR", Modulo = "Seguridad", Descripcion = "Editar roles y asignar permisos" },
            new() { Codigo = "SEGURIDAD.ROL.ELIMINAR", Modulo = "Seguridad", Descripcion = "Eliminar roles" },
            new() { Codigo = "SEGURIDAD.AUDITORIA.VER", Modulo = "Seguridad", Descripcion = "Ver bitÃ¡cora de auditorÃ­a" },
            new() { Codigo = "SEGURIDAD.AUDITORIA.EXPORTAR", Modulo = "Seguridad", Descripcion = "Exportar bitÃ¡cora de auditorÃ­a" },

            // ------------------------------------------------------------------
            // ADMINISTRACIÃ“N
            // ------------------------------------------------------------------
            new() { Codigo = "ADMIN.ENTIDAD.VER", Modulo = "AdministraciÃ³n", Descripcion = "Ver datos de la entidad" },
            new() { Codigo = "ADMIN.ENTIDAD.EDITAR", Modulo = "AdministraciÃ³n", Descripcion = "Editar datos de la entidad" },
            new() { Codigo = "ADMIN.SUCURSAL.VER", Modulo = "AdministraciÃ³n", Descripcion = "Ver sucursales" },
            new() { Codigo = "ADMIN.SUCURSAL.CREAR", Modulo = "AdministraciÃ³n", Descripcion = "Crear sucursales" },
            new() { Codigo = "ADMIN.SUCURSAL.EDITAR", Modulo = "AdministraciÃ³n", Descripcion = "Editar sucursales" },
            new() { Codigo = "ADMIN.SUCURSAL.ELIMINAR", Modulo = "AdministraciÃ³n", Descripcion = "Eliminar sucursales" },
            new() { Codigo = "ADMIN.BACKUP.VER", Modulo = "AdministraciÃ³n", Descripcion = "Ver historial de respaldos" },
            new() { Codigo = "ADMIN.BACKUP.EJECUTAR", Modulo = "AdministraciÃ³n", Descripcion = "Ejecutar respaldos" },
            new() { Codigo = "ADMIN.BACKUP.RESTAURAR", Modulo = "AdministraciÃ³n", Descripcion = "Restaurar desde respaldo" },
            new() { Codigo = "ADMIN.PARAMETROS.VER", Modulo = "AdministraciÃ³n", Descripcion = "Ver parÃ¡metros del sistema" },
            new() { Codigo = "ADMIN.PARAMETROS.EDITAR", Modulo = "AdministraciÃ³n", Descripcion = "Editar parÃ¡metros del sistema" },

            // ------------------------------------------------------------------
            // CONTABILIDAD
            // ------------------------------------------------------------------
            new() { Codigo = "CONTABILIDAD.CUENTA.VER", Modulo = "Contabilidad", Descripcion = "Ver plan de cuentas" },
            new() { Codigo = "CONTABILIDAD.CUENTA.CREAR", Modulo = "Contabilidad", Descripcion = "Crear cuentas contables" },
            new() { Codigo = "CONTABILIDAD.CUENTA.EDITAR", Modulo = "Contabilidad", Descripcion = "Editar cuentas contables" },
            new() { Codigo = "CONTABILIDAD.CUENTA.ELIMINAR", Modulo = "Contabilidad", Descripcion = "Eliminar cuentas contables" },
            new() { Codigo = "CONTABILIDAD.ASIENTO.VER", Modulo = "Contabilidad", Descripcion = "Ver asientos contables" },
            new() { Codigo = "CONTABILIDAD.ASIENTO.CREAR", Modulo = "Contabilidad", Descripcion = "Crear asientos contables" },
            new() { Codigo = "CONTABILIDAD.ASIENTO.REVERTIR", Modulo = "Contabilidad", Descripcion = "Revertir asientos contabilizados" },
            new() { Codigo = "CONTABILIDAD.PERIODO.VER", Modulo = "Contabilidad", Descripcion = "Ver perÃ­odos contables" },
            new() { Codigo = "CONTABILIDAD.PERIODO.CERRAR", Modulo = "Contabilidad", Descripcion = "Cerrar perÃ­odos contables" },
            new() { Codigo = "CONTABILIDAD.PERIODO.REABRIR", Modulo = "Contabilidad", Descripcion = "Reabrir perÃ­odos cerrados" },
            new() { Codigo = "CONTABILIDAD.ACTIVOS.VER", Modulo = "Contabilidad", Descripcion = "Ver activos fijos" },
            new() { Codigo = "CONTABILIDAD.ACTIVOS.CREAR", Modulo = "Contabilidad", Descripcion = "Crear activos fijos" },
            new() { Codigo = "CONTABILIDAD.ACTIVOS.EDITAR", Modulo = "Contabilidad", Descripcion = "Editar activos fijos" },
            new() { Codigo = "CONTABILIDAD.ACTIVOS.BAJA", Modulo = "Contabilidad", Descripcion = "Dar de baja activos fijos" },
            new() { Codigo = "CONTABILIDAD.CXC.VER", Modulo = "Contabilidad", Descripcion = "Ver cuentas por cobrar" },
            new() { Codigo = "CONTABILIDAD.CXC.COBRAR", Modulo = "Contabilidad", Descripcion = "Registrar cobros" },
            new() { Codigo = "CONTABILIDAD.CXP.VER", Modulo = "Contabilidad", Descripcion = "Ver cuentas por pagar" },
            new() { Codigo = "CONTABILIDAD.CXP.PAGAR", Modulo = "Contabilidad", Descripcion = "Registrar pagos" },
            new() { Codigo = "CONTABILIDAD.CAJA.VER", Modulo = "Contabilidad", Descripcion = "Ver cajas" },
            new() { Codigo = "CONTABILIDAD.BANCO.VER", Modulo = "Contabilidad", Descripcion = "Ver cuentas bancarias" },
            new() { Codigo = "CONTABILIDAD.BANCO.CONCILIAR", Modulo = "Contabilidad", Descripcion = "Conciliar movimientos bancarios" },
            new() { Codigo = "CONTABILIDAD.ONAT.VER", Modulo = "Contabilidad", Descripcion = "Ver declaraciones ONAT" },
            new() { Codigo = "CONTABILIDAD.ONAT.GENERAR", Modulo = "Contabilidad", Descripcion = "Generar declaraciones juradas" },
            new() { Codigo = "CONTABILIDAD.CIERRE.VER", Modulo = "Contabilidad", Descripcion = "Ver cierre contable" },
            new() { Codigo = "CONTABILIDAD.CIERRE.EJECUTAR", Modulo = "Contabilidad", Descripcion = "Ejecutar cierre contable" },

            // ------------------------------------------------------------------
            // INVENTARIO
            // ------------------------------------------------------------------
            new() { Codigo = "INVENTARIO.PRODUCTO.VER", Modulo = "Inventario", Descripcion = "Ver productos" },
            new() { Codigo = "INVENTARIO.PRODUCTO.CREAR", Modulo = "Inventario", Descripcion = "Crear productos" },
            new() { Codigo = "INVENTARIO.PRODUCTO.EDITAR", Modulo = "Inventario", Descripcion = "Editar productos" },
            new() { Codigo = "INVENTARIO.PRODUCTO.ELIMINAR", Modulo = "Inventario", Descripcion = "Eliminar productos" },
            new() { Codigo = "INVENTARIO.EXISTENCIA.VER", Modulo = "Inventario", Descripcion = "Ver existencias" },
            new() { Codigo = "INVENTARIO.MOVIMIENTO.VER", Modulo = "Inventario", Descripcion = "Ver movimientos de inventario" },
            new() { Codigo = "INVENTARIO.MOVIMIENTO.CREAR", Modulo = "Inventario", Descripcion = "Registrar movimientos" },
            new() { Codigo = "INVENTARIO.CONTEO.VER", Modulo = "Inventario", Descripcion = "Ver conteos fÃ­sicos" },
            new() { Codigo = "INVENTARIO.CONTEO.CREAR", Modulo = "Inventario", Descripcion = "Crear conteos fÃ­sicos" },
            new() { Codigo = "INVENTARIO.ALMACEN.VER", Modulo = "Inventario", Descripcion = "Ver almacenes" },
            new() { Codigo = "INVENTARIO.ALMACEN.CREAR", Modulo = "Inventario", Descripcion = "Crear almacenes" },
            new() { Codigo = "INVENTARIO.ALMACEN.EDITAR", Modulo = "Inventario", Descripcion = "Editar almacenes" },
            new() { Codigo = "INVENTARIO.FAMILIA.VER", Modulo = "Inventario", Descripcion = "Ver familias de productos" },
            new() { Codigo = "INVENTARIO.FAMILIA.CREAR", Modulo = "Inventario", Descripcion = "Crear familias" },
            new() { Codigo = "INVENTARIO.FAMILIA.EDITAR", Modulo = "Inventario", Descripcion = "Editar familias" },
            new() { Codigo = "INVENTARIO.FAMILIA.ELIMINAR", Modulo = "Inventario", Descripcion = "Eliminar familias" },
            new() { Codigo = "INVENTARIO.LISTA_PRECIO.VER", Modulo = "Inventario", Descripcion = "Ver listas de precios" },
            new() { Codigo = "INVENTARIO.LISTA_PRECIO.EDITAR", Modulo = "Inventario", Descripcion = "Editar listas de precios" },
            new() { Codigo = "INVENTARIO.ALERTAS.VER", Modulo = "Inventario", Descripcion = "Ver alertas de stock bajo" },

            // ------------------------------------------------------------------
            // COMERCIAL
            // ------------------------------------------------------------------
            new() { Codigo = "COMERCIAL.FACTURA_VENTA.VER", Modulo = "Comercial", Descripcion = "Ver facturas de venta" },
            new() { Codigo = "COMERCIAL.FACTURA_VENTA.CREAR", Modulo = "Comercial", Descripcion = "Crear facturas de venta" },
            new() { Codigo = "COMERCIAL.FACTURA_VENTA.ANULAR", Modulo = "Comercial", Descripcion = "Anular facturas" },
            new() { Codigo = "COMERCIAL.FACTURA_VENTA.EXPORTAR", Modulo = "Comercial", Descripcion = "Exportar facturas" },
            new() { Codigo = "COMERCIAL.DEVOLUCION.VER", Modulo = "Comercial", Descripcion = "Ver devoluciones" },
            new() { Codigo = "COMERCIAL.DEVOLUCION.CREAR", Modulo = "Comercial", Descripcion = "Registrar devoluciones" },
            new() { Codigo = "COMERCIAL.DEVOLUCION.AUTORIZAR", Modulo = "Comercial", Descripcion = "Autorizar devoluciones" },
            new() { Codigo = "COMERCIAL.CLIENTE.VER", Modulo = "Comercial", Descripcion = "Ver clientes" },
            new() { Codigo = "COMERCIAL.CLIENTE.CREAR", Modulo = "Comercial", Descripcion = "Crear clientes" },
            new() { Codigo = "COMERCIAL.CLIENTE.EDITAR", Modulo = "Comercial", Descripcion = "Editar clientes" },
            new() { Codigo = "COMERCIAL.CLIENTE.ELIMINAR", Modulo = "Comercial", Descripcion = "Eliminar clientes" },
            new() { Codigo = "COMERCIAL.CLIENTE.CREDITO", Modulo = "Comercial", Descripcion = "Gestionar crÃ©ditos de clientes" },
            new() { Codigo = "COMERCIAL.PROVEEDOR.VER", Modulo = "Comercial", Descripcion = "Ver proveedores" },
            new() { Codigo = "COMERCIAL.PROVEEDOR.CREAR", Modulo = "Comercial", Descripcion = "Crear proveedores" },
            new() { Codigo = "COMERCIAL.PROVEEDOR.EDITAR", Modulo = "Comercial", Descripcion = "Editar proveedores" },
            new() { Codigo = "COMERCIAL.PROVEEDOR.ELIMINAR", Modulo = "Comercial", Descripcion = "Eliminar proveedores" },
            new() { Codigo = "COMERCIAL.PROVEEDOR.CREDITO", Modulo = "Comercial", Descripcion = "Gestionar crÃ©ditos de proveedores" },
            new() { Codigo = "COMERCIAL.ORDEN_COMPRA.VER", Modulo = "Comercial", Descripcion = "Ver Ã³rdenes de compra" },
            new() { Codigo = "COMERCIAL.ORDEN_COMPRA.CREAR", Modulo = "Comercial", Descripcion = "Crear Ã³rdenes de compra" },
            new() { Codigo = "COMERCIAL.ORDEN_COMPRA.APROBAR", Modulo = "Comercial", Descripcion = "Aprobar Ã³rdenes de compra" },
            new() { Codigo = "COMERCIAL.ORDEN_COMPRA.RECIBIR", Modulo = "Comercial", Descripcion = "Registrar recepciÃ³n" },
            new() { Codigo = "COMERCIAL.CONTRATO.VER", Modulo = "Comercial", Descripcion = "Ver contratos econÃ³micos" },
            new() { Codigo = "COMERCIAL.CONTRATO.CREAR", Modulo = "Comercial", Descripcion = "Crear contratos econÃ³micos" },
            new() { Codigo = "COMERCIAL.CONTRATO.EDITAR", Modulo = "Comercial", Descripcion = "Editar contratos econÃ³micos" },
            new() { Codigo = "COMERCIAL.TOPE_PRECIO.VER", Modulo = "Comercial", Descripcion = "Ver topes de precio MFP" },
            new() { Codigo = "COMERCIAL.TOPE_PRECIO.EDITAR", Modulo = "Comercial", Descripcion = "Gestionar topes de precio MFP" },

            // ------------------------------------------------------------------
            // RRHH
            // ------------------------------------------------------------------
            new() { Codigo = "RRHH.EMPLEADO.VER", Modulo = "RRHH", Descripcion = "Ver expedientes de empleados" },
            new() { Codigo = "RRHH.EMPLEADO.CREAR", Modulo = "RRHH", Descripcion = "Crear empleados" },
            new() { Codigo = "RRHH.EMPLEADO.EDITAR", Modulo = "RRHH", Descripcion = "Editar empleados" },
            new() { Codigo = "RRHH.EMPLEADO.ELIMINAR", Modulo = "RRHH", Descripcion = "Dar de baja empleados" },
            new() { Codigo = "RRHH.CONTRATO.VER", Modulo = "RRHH", Descripcion = "Ver contratos laborales" },
            new() { Codigo = "RRHH.CONTRATO.CREAR", Modulo = "RRHH", Descripcion = "Crear contratos laborales" },
            new() { Codigo = "RRHH.CONTRATO.EDITAR", Modulo = "RRHH", Descripcion = "Editar contratos laborales" },
            new() { Codigo = "RRHH.CONTRATO.FINALIZAR", Modulo = "RRHH", Descripcion = "Finalizar contratos" },
            new() { Codigo = "RRHH.ASISTENCIA.VER", Modulo = "RRHH", Descripcion = "Ver registros de asistencia" },
            new() { Codigo = "RRHH.ASISTENCIA.REGISTRAR", Modulo = "RRHH", Descripcion = "Registrar asistencia" },
            new() { Codigo = "RRHH.ASISTENCIA.EDITAR", Modulo = "RRHH", Descripcion = "Editar asistencia" },
            new() { Codigo = "RRHH.NOMINA.VER", Modulo = "RRHH", Descripcion = "Ver nÃ³minas" },
            new() { Codigo = "RRHH.NOMINA.CALCULAR", Modulo = "RRHH", Descripcion = "Calcular nÃ³minas" },
            new() { Codigo = "RRHH.NOMINA.APROBAR", Modulo = "RRHH", Descripcion = "Aprobar nÃ³minas" },
            new() { Codigo = "RRHH.NOMINA.CONTABILIZAR", Modulo = "RRHH", Descripcion = "Contabilizar nÃ³minas" },
            new() { Codigo = "RRHH.NOMINA.PAGAR", Modulo = "RRHH", Descripcion = "Marcar nÃ³minas como pagadas" },
            new() { Codigo = "RRHH.VACACIONES.VER", Modulo = "RRHH", Descripcion = "Ver saldos de vacaciones" },
            new() { Codigo = "RRHH.VACACIONES.REGISTRAR", Modulo = "RRHH", Descripcion = "Registrar disfrute de vacaciones" },
            new() { Codigo = "RRHH.VACACIONES.COMPENSAR", Modulo = "RRHH", Descripcion = "Compensar vacaciones" },
            new() { Codigo = "RRHH.SC408.VER", Modulo = "RRHH", Descripcion = "Ver modelo SC-4-08" },
            new() { Codigo = "RRHH.SC408.EXPORTAR", Modulo = "RRHH", Descripcion = "Exportar modelo SC-4-08" },
            new() { Codigo = "RRHH.CERTIFICADO.VER", Modulo = "RRHH", Descripcion = "Ver certificados mÃ©dicos" },
            new() { Codigo = "RRHH.CERTIFICADO.CREAR", Modulo = "RRHH", Descripcion = "Crear certificados mÃ©dicos" },
            new() { Codigo = "RRHH.PLANTILLA.VER", Modulo = "RRHH", Descripcion = "Ver plantilla aprobada" },
            new() { Codigo = "RRHH.PLANTILLA.CREAR", Modulo = "RRHH", Descripcion = "Crear plazas en plantilla" },
            new() { Codigo = "RRHH.PLANTILLA.EDITAR", Modulo = "RRHH", Descripcion = "Editar plazas" },
            new() { Codigo = "RRHH.CARGO.VER", Modulo = "RRHH", Descripcion = "Ver cargos" },
            new() { Codigo = "RRHH.CARGO.CREAR", Modulo = "RRHH", Descripcion = "Crear cargos" },
            new() { Codigo = "RRHH.CARGO.EDITAR", Modulo = "RRHH", Descripcion = "Editar cargos" },
            new() { Codigo = "RRHH.UTIL.VER", Modulo = "RRHH", Descripcion = "Ver medios y responsabilidades" },
            new() { Codigo = "RRHH.UTIL.ENTREGAR", Modulo = "RRHH", Descripcion = "Registrar entrega de medios" },
            new() { Codigo = "RRHH.UTIL.DEVOLVER", Modulo = "RRHH", Descripcion = "Registrar devoluciÃ³n de medios" },

            // ------------------------------------------------------------------
            // PRODUCCIÃ“N
            // ------------------------------------------------------------------
            new() { Codigo = "PRODUCCION.FICHA.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver fichas de costo" },
            new() { Codigo = "PRODUCCION.FICHA.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Crear fichas de costo" },
            new() { Codigo = "PRODUCCION.FICHA.EDITAR", Modulo = "ProducciÃ³n", Descripcion = "Editar fichas de costo" },
            new() { Codigo = "PRODUCCION.FICHA.APROBAR", Modulo = "ProducciÃ³n", Descripcion = "Aprobar fichas de costo" },
            new() { Codigo = "PRODUCCION.BOM.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver listas de materiales (BOM)" },
            new() { Codigo = "PRODUCCION.BOM.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Crear listas de materiales" },
            new() { Codigo = "PRODUCCION.BOM.EDITAR", Modulo = "ProducciÃ³n", Descripcion = "Editar listas de materiales" },
            new() { Codigo = "PRODUCCION.ORDEN.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver Ã³rdenes de producciÃ³n" },
            new() { Codigo = "PRODUCCION.ORDEN.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Crear Ã³rdenes de producciÃ³n" },
            new() { Codigo = "PRODUCCION.ORDEN.INICIAR", Modulo = "ProducciÃ³n", Descripcion = "Iniciar Ã³rdenes" },
            new() { Codigo = "PRODUCCION.ORDEN.TERMINAR", Modulo = "ProducciÃ³n", Descripcion = "Terminar Ã³rdenes" },
            new() { Codigo = "PRODUCCION.ORDEN.CANCELAR", Modulo = "ProducciÃ³n", Descripcion = "Cancelar Ã³rdenes" },
            new() { Codigo = "PRODUCCION.PLAN.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver planes de producciÃ³n" },
            new() { Codigo = "PRODUCCION.PLAN.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Crear planes" },
            new() { Codigo = "PRODUCCION.PLAN.APROBAR", Modulo = "ProducciÃ³n", Descripcion = "Aprobar planes" },
            new() { Codigo = "PRODUCCION.MERMA.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver mermas" },
            new() { Codigo = "PRODUCCION.MERMA.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Registrar mermas" },
            new() { Codigo = "PRODUCCION.EQUIPO.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver equipos" },
            new() { Codigo = "PRODUCCION.MANTENIMIENTO.VER", Modulo = "ProducciÃ³n", Descripcion = "Ver mantenimientos" },
            new() { Codigo = "PRODUCCION.MANTENIMIENTO.CREAR", Modulo = "ProducciÃ³n", Descripcion = "Programar mantenimiento" },
            new() { Codigo = "PRODUCCION.MANTENIMIENTO.EJECUTAR", Modulo = "ProducciÃ³n", Descripcion = "Ejecutar mantenimiento" },

            // ------------------------------------------------------------------
            // POS / INTEGRACIÃ“N
            // ------------------------------------------------------------------
            new() { Codigo = "POS.CAJA", Modulo = "POS", Descripcion = "Acceso a la pantalla de caja POS" },
            new() { Codigo = "POS.SESION.ABRIR", Modulo = "POS", Descripcion = "Abrir sesiÃ³n de caja" },
            new() { Codigo = "POS.SESION.CERRAR", Modulo = "POS", Descripcion = "Cerrar sesiÃ³n de caja" },
            new() { Codigo = "POS.SESION.CONCILIAR", Modulo = "POS", Descripcion = "Conciliar cierre de caja" },
            new() { Codigo = "POS.VENTA.ANULAR", Modulo = "POS", Descripcion = "Anular ventas del POS" },
            new() { Codigo = "POS.MOVIMIENTO.CAJA", Modulo = "POS", Descripcion = "Registrar movimientos de caja" },
            new() { Codigo = "INTEGRACION.POS.VER", Modulo = "IntegraciÃ³n", Descripcion = "Ver dispositivos POS" },
            new() { Codigo = "INTEGRACION.POS.CREAR", Modulo = "IntegraciÃ³n", Descripcion = "Registrar dispositivos" },
            new() { Codigo = "INTEGRACION.API.VER", Modulo = "IntegraciÃ³n", Descripcion = "Ver clientes API" },
            new() { Codigo = "INTEGRACION.API.CREAR", Modulo = "IntegraciÃ³n", Descripcion = "Crear clientes API" },
            new() { Codigo = "INTEGRACION.API.REVOCAR", Modulo = "IntegraciÃ³n", Descripcion = "Revocar clientes API" },
            new() { Codigo = "INTEGRACION.WEBHOOK.VER", Modulo = "IntegraciÃ³n", Descripcion = "Ver suscripciones webhook" },
            new() { Codigo = "INTEGRACION.RANGO.VER", Modulo = "IntegraciÃ³n", Descripcion = "Ver rangos de numeraciÃ³n POS" },
            new() { Codigo = "INTEGRACION.RANGO.ASIGNAR", Modulo = "IntegraciÃ³n", Descripcion = "Asignar rangos a dispositivos" },

            // ------------------------------------------------------------------
            // REPORTES
            // ------------------------------------------------------------------
            new() { Codigo = "REPORTES.INDICADORES.VER", Modulo = "Reportes", Descripcion = "Ver dashboard de indicadores" },
            new() { Codigo = "REPORTES.CONTABLES.VER", Modulo = "Reportes", Descripcion = "Ver reportes contables" },
            new() { Codigo = "REPORTES.CONTABLES.EXPORTAR", Modulo = "Reportes", Descripcion = "Exportar reportes contables" },
            new() { Codigo = "REPORTES.AUDITORIA.VER", Modulo = "Reportes", Descripcion = "Ver trazas de auditorÃ­a" },
            new() { Codigo = "REPORTES.AUDITORIA.EXPORTAR", Modulo = "Reportes", Descripcion = "Exportar trazas" },
            new() { Codigo = "REPORTES.FISCALES.VER", Modulo = "Reportes", Descripcion = "Ver reportes fiscales" },
            new() { Codigo = "REPORTES.FISCALES.EXPORTAR", Modulo = "Reportes", Descripcion = "Exportar reportes fiscales" },
            new() { Codigo = "REPORTES.PERSONALIZADO.VER", Modulo = "Reportes", Descripcion = "Generar reportes personalizados" }
        };

        foreach (var p in permisos)
        {
            if (!await context.Permisos.AnyAsync(x => x.Codigo == p.Codigo))
                context.Permisos.Add(p);
        }
        await context.SaveChangesAsync();

        Console.WriteLine($"   âœ… {permisos.Count} permisos procesados");
    }

    // ==========================================================================
    // 4. ROLES
    // ==========================================================================
    private static async Task SeedRolesAsync(AppDbContext context, Guid entidadId)
    {
        var roles = new List<Rol>
        {
            new() { Codigo = "MASTER", Nombre = "Administrador Global (Ecosistema)", Descripcion = "Acceso total a todas las entidades del ecosistema", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "ADMINISTRADOR", Nombre = "Administrador de Entidad", Descripcion = "AdministraciÃ³n total de la entidad", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "DIRECCION", Nombre = "DirecciÃ³n", Descripcion = "Acceso de solo lectura a toda la informaciÃ³n y aprobaciÃ³n de operaciones", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "CONTADOR", Nombre = "Contador", Descripcion = "GestiÃ³n contable, fiscal, presupuestaria y cierre de perÃ­odos", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "ECONOMICO", Nombre = "EconÃ³mico", Descripcion = "GestiÃ³n de tesorerÃ­a, cuentas por cobrar/pagar y conciliaciÃ³n bancaria", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "JEFE_PRODUCCION", Nombre = "Jefe de ProducciÃ³n", Descripcion = "GestiÃ³n de Ã³rdenes, fichas de costo y planificaciÃ³n", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "ALMACENERO", Nombre = "Almacenero", Descripcion = "GestiÃ³n de inventario, movimientos y conteos", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "RRHH", Nombre = "Recursos Humanos", Descripcion = "GestiÃ³n de empleados, nÃ³minas y asistencia", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "COMERCIAL", Nombre = "Comercial / Ventas", Descripcion = "GestiÃ³n de clientes, facturaciÃ³n y devoluciones", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "COMPRADOR", Nombre = "Comprador", Descripcion = "GestiÃ³n de proveedores y Ã³rdenes de compra", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "CAJERO_POS", Nombre = "Cajero de POS", Descripcion = "OperaciÃ³n de terminal POS y apertura/cierre de caja", EsSistema = true, EntidadId = entidadId },
            new() { Codigo = "SUPERVISOR_POS", Nombre = "Supervisor de POS", Descripcion = "SupervisiÃ³n de cajas y anulaciÃ³n de ventas", EsSistema = true, EntidadId = entidadId }
        };

        foreach (var r in roles)
        {
            if (!await context.Rols
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Codigo == r.Codigo))
            {
                context.Rols.Add(r);
            }
        }
        await context.SaveChangesAsync();

        Console.WriteLine($"   âœ… {roles.Count} roles procesados");
    }

    // ==========================================================================
    // 5. ASIGNACIÃ“N DE PERMISOS A ROLES
    // ==========================================================================
    private static async Task SeedRolPermisosAsync(AppDbContext context)
    {
        // Obtener todos los IDs
        var roles = await context.Rols
           .IgnoreQueryFilters()
           .ToDictionaryAsync(r => r.Codigo, r => r.Id);

        var permisos = await context.Permisos
            .IgnoreQueryFilters()
            .ToDictionaryAsync(p => p.Codigo, p => p.Id);

        // ------------------------------------------------------------------
        // MASTER y ADMINISTRADOR: TODOS los permisos
        // ------------------------------------------------------------------
        var permisosAll = permisos.Values.ToList();
        await AsignarPermisosAsync(context, roles["MASTER"], permisosAll);
        await AsignarPermisosAsync(context, roles["ADMINISTRADOR"], permisosAll);

        // ------------------------------------------------------------------
        // DIRECCION: solo lectura + aprobaciones
        // ------------------------------------------------------------------
        var permisosDireccion = permisos.Where(p => p.Key.EndsWith(".VER") || p.Key.Contains(".EXPORTAR"))
            .Select(p => p.Value).ToList();
        await AsignarPermisosAsync(context, roles["DIRECCION"], permisosDireccion);

        // ------------------------------------------------------------------
        // CONTADOR
        // ------------------------------------------------------------------
        var permisosContador = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "CONTABILIDAD.CUENTA.VER", "CONTABILIDAD.CUENTA.CREAR", "CONTABILIDAD.CUENTA.EDITAR",
            "CONTABILIDAD.ASIENTO.VER", "CONTABILIDAD.ASIENTO.CREAR", "CONTABILIDAD.ASIENTO.REVERTIR",
            "CONTABILIDAD.PERIODO.VER", "CONTABILIDAD.PERIODO.CERRAR", "CONTABILIDAD.PERIODO.REABRIR",
            "CONTABILIDAD.ACTIVOS.VER", "CONTABILIDAD.ACTIVOS.CREAR", "CONTABILIDAD.ACTIVOS.EDITAR", "CONTABILIDAD.ACTIVOS.BAJA",
            "CONTABILIDAD.CXC.VER", "CONTABILIDAD.CXC.COBRAR",
            "CONTABILIDAD.CXP.VER", "CONTABILIDAD.CXP.PAGAR",
            "CONTABILIDAD.CAJA.VER", "CONTABILIDAD.BANCO.VER", "CONTABILIDAD.BANCO.CONCILIAR",
            "CONTABILIDAD.ONAT.VER", "CONTABILIDAD.ONAT.GENERAR",
            "CONTABILIDAD.CIERRE.VER", "CONTABILIDAD.CIERRE.EJECUTAR",
            "REPORTES.INDICADORES.VER", "REPORTES.CONTABLES.VER", "REPORTES.CONTABLES.EXPORTAR",
            "REPORTES.FISCALES.VER", "REPORTES.FISCALES.EXPORTAR",
            "RRHH.NOMINA.VER", "RRHH.NOMINA.CONTABILIZAR"
        });
        await AsignarPermisosAsync(context, roles["CONTADOR"], permisosContador);

        // ------------------------------------------------------------------
        // ECONOMICO
        // ------------------------------------------------------------------
        var permisosEconomico = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "CONTABILIDAD.CXC.VER", "CONTABILIDAD.CXC.COBRAR",
            "CONTABILIDAD.CXP.VER", "CONTABILIDAD.CXP.PAGAR",
            "CONTABILIDAD.CAJA.VER", "CONTABILIDAD.BANCO.VER", "CONTABILIDAD.BANCO.CONCILIAR",
            "CONTABILIDAD.CUENTA.VER", "CONTABILIDAD.ASIENTO.VER",
            "COMERCIAL.CLIENTE.VER", "COMERCIAL.CLIENTE.CREDITO",
            "COMERCIAL.PROVEEDOR.VER", "COMERCIAL.PROVEEDOR.CREDITO",
            "REPORTES.INDICADORES.VER", "REPORTES.CONTABLES.VER"
        });
        await AsignarPermisosAsync(context, roles["ECONOMICO"], permisosEconomico);

        // ------------------------------------------------------------------
        // JEFE_PRODUCCION
        // ------------------------------------------------------------------
        var permisosProduccion = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "PRODUCCION.FICHA.VER", "PRODUCCION.FICHA.CREAR", "PRODUCCION.FICHA.EDITAR", "PRODUCCION.FICHA.APROBAR",
            "PRODUCCION.BOM.VER", "PRODUCCION.BOM.CREAR", "PRODUCCION.BOM.EDITAR",
            "PRODUCCION.ORDEN.VER", "PRODUCCION.ORDEN.CREAR", "PRODUCCION.ORDEN.INICIAR",
            "PRODUCCION.ORDEN.TERMINAR", "PRODUCCION.ORDEN.CANCELAR",
            "PRODUCCION.PLAN.VER", "PRODUCCION.PLAN.CREAR", "PRODUCCION.PLAN.APROBAR",
            "PRODUCCION.MERMA.VER", "PRODUCCION.MERMA.CREAR",
            "PRODUCCION.EQUIPO.VER", "PRODUCCION.MANTENIMIENTO.VER",
            "PRODUCCION.MANTENIMIENTO.CREAR", "PRODUCCION.MANTENIMIENTO.EJECUTAR",
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.EXISTENCIA.VER", "INVENTARIO.MOVIMIENTO.VER",
            "INVENTARIO.ALMACEN.VER"
        });
        await AsignarPermisosAsync(context, roles["JEFE_PRODUCCION"], permisosProduccion);

        // ------------------------------------------------------------------
        // ALMACENERO
        // ------------------------------------------------------------------
        var permisosAlmacenero = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.PRODUCTO.CREAR", "INVENTARIO.PRODUCTO.EDITAR",
            "INVENTARIO.EXISTENCIA.VER",
            "INVENTARIO.MOVIMIENTO.VER", "INVENTARIO.MOVIMIENTO.CREAR",
            "INVENTARIO.CONTEO.VER", "INVENTARIO.CONTEO.CREAR",
            "INVENTARIO.ALMACEN.VER",
            "INVENTARIO.FAMILIA.VER",
            "INVENTARIO.LISTA_PRECIO.VER",
            "INVENTARIO.ALERTAS.VER",
            "COMERCIAL.ORDEN_COMPRA.VER", "COMERCIAL.ORDEN_COMPRA.RECIBIR",
            "COMERCIAL.PROVEEDOR.VER"
        });
        await AsignarPermisosAsync(context, roles["ALMACENERO"], permisosAlmacenero);

        // ------------------------------------------------------------------
        // RRHH
        // ------------------------------------------------------------------
        var permisosRrhh = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "RRHH.EMPLEADO.VER", "RRHH.EMPLEADO.CREAR", "RRHH.EMPLEADO.EDITAR", "RRHH.EMPLEADO.ELIMINAR",
            "RRHH.CONTRATO.VER", "RRHH.CONTRATO.CREAR", "RRHH.CONTRATO.EDITAR", "RRHH.CONTRATO.FINALIZAR",
            "RRHH.ASISTENCIA.VER", "RRHH.ASISTENCIA.REGISTRAR", "RRHH.ASISTENCIA.EDITAR",
            "RRHH.NOMINA.VER", "RRHH.NOMINA.CALCULAR", "RRHH.NOMINA.APROBAR",
            "RRHH.VACACIONES.VER", "RRHH.VACACIONES.REGISTRAR", "RRHH.VACACIONES.COMPENSAR",
            "RRHH.SC408.VER", "RRHH.SC408.EXPORTAR",
            "RRHH.CERTIFICADO.VER", "RRHH.CERTIFICADO.CREAR",
            "RRHH.PLANTILLA.VER", "RRHH.PLANTILLA.CREAR", "RRHH.PLANTILLA.EDITAR",
            "RRHH.CARGO.VER", "RRHH.CARGO.CREAR", "RRHH.CARGO.EDITAR",
            "RRHH.UTIL.VER", "RRHH.UTIL.ENTREGAR", "RRHH.UTIL.DEVOLVER"
        });
        await AsignarPermisosAsync(context, roles["RRHH"], permisosRrhh);

        // ------------------------------------------------------------------
        // COMERCIAL
        // ------------------------------------------------------------------
        var permisosComercial = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "COMERCIAL.FACTURA_VENTA.VER", "COMERCIAL.FACTURA_VENTA.CREAR", "COMERCIAL.FACTURA_VENTA.EXPORTAR",
            "COMERCIAL.DEVOLUCION.VER", "COMERCIAL.DEVOLUCION.CREAR",
            "COMERCIAL.CLIENTE.VER", "COMERCIAL.CLIENTE.CREAR", "COMERCIAL.CLIENTE.EDITAR", "COMERCIAL.CLIENTE.CREDITO",
            "COMERCIAL.CONTRATO.VER", "COMERCIAL.CONTRATO.CREAR", "COMERCIAL.CONTRATO.EDITAR",
            "COMERCIAL.TOPE_PRECIO.VER",
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.EXISTENCIA.VER", "INVENTARIO.LISTA_PRECIO.VER",
            "REPORTES.INDICADORES.VER"
        });
        await AsignarPermisosAsync(context, roles["COMERCIAL"], permisosComercial);

        // ------------------------------------------------------------------
        // COMPRADOR
        // ------------------------------------------------------------------
        var permisosComprador = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "COMERCIAL.PROVEEDOR.VER", "COMERCIAL.PROVEEDOR.CREAR", "COMERCIAL.PROVEEDOR.EDITAR",
            "COMERCIAL.ORDEN_COMPRA.VER", "COMERCIAL.ORDEN_COMPRA.CREAR",
            "COMERCIAL.CONTRATO.VER", "COMERCIAL.CONTRATO.CREAR",
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.EXISTENCIA.VER", "INVENTARIO.ALERTAS.VER",
            "CONTABILIDAD.CXP.VER"
        });
        await AsignarPermisosAsync(context, roles["COMPRADOR"], permisosComprador);

        // ------------------------------------------------------------------
        // CAJERO_POS
        // ------------------------------------------------------------------
        var permisosCajeroPos = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "POS.CAJA", "POS.SESION.ABRIR", "POS.SESION.CERRAR", "POS.MOVIMIENTO.CAJA",
            "COMERCIAL.FACTURA_VENTA.VER", "COMERCIAL.FACTURA_VENTA.CREAR",
            "COMERCIAL.CLIENTE.VER",
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.EXISTENCIA.VER", "INVENTARIO.LISTA_PRECIO.VER"
        });
        await AsignarPermisosAsync(context, roles["CAJERO_POS"], permisosCajeroPos);

        // ------------------------------------------------------------------
        // SUPERVISOR_POS
        // ------------------------------------------------------------------
        var permisosSupervisorPos = ObtenerPermisosPorCodigos(permisos, new[]
        {
            "POS.CAJA", "POS.SESION.ABRIR", "POS.SESION.CERRAR", "POS.SESION.CONCILIAR",
            "POS.VENTA.ANULAR", "POS.MOVIMIENTO.CAJA",
            "INTEGRACION.POS.VER",
            "COMERCIAL.FACTURA_VENTA.VER", "COMERCIAL.FACTURA_VENTA.CREAR", "COMERCIAL.FACTURA_VENTA.ANULAR",
            "COMERCIAL.DEVOLUCION.VER", "COMERCIAL.DEVOLUCION.CREAR", "COMERCIAL.DEVOLUCION.AUTORIZAR",
            "COMERCIAL.CLIENTE.VER",
            "INVENTARIO.PRODUCTO.VER", "INVENTARIO.EXISTENCIA.VER",
            "REPORTES.INDICADORES.VER"
        });
        await AsignarPermisosAsync(context, roles["SUPERVISOR_POS"], permisosSupervisorPos);

        await context.SaveChangesAsync();
        Console.WriteLine("   âœ… AsignaciÃ³n de permisos a roles completada");
    }

    // Helper para asignar permisos evitando duplicados
    private static async Task AsignarPermisosAsync(AppDbContext context, int rolId, List<int> permisoIds)
    {
        var existentes = await context.Permisos
            .FromSqlRaw("SELECT p.* FROM nucleo.permiso p INNER JOIN nucleo.rol_permiso rp ON p.id=rp.permiso_id WHERE rp.rol_id={0}", rolId)
            .Select(p => p.Id)
            .ToListAsync();
        foreach (var permisoId in permisoIds.Distinct())
        {
            if (!existentes.Contains(permisoId))
            {
                await context.Database.ExecuteSqlRawAsync("INSERT INTO nucleo.rol_permiso (rol_id, permiso_id) VALUES ({0}, {1})", rolId, permisoId);
            }
        }
    }
    // Helper para obtener IDs de permisos por cÃ³digo
    private static List<int> ObtenerPermisosPorCodigos(Dictionary<string, int> permisos, string[] codigos)
    {
        return codigos
            .Where(c => permisos.ContainsKey(c))
            .Select(c => permisos[c])
            .ToList();
    }

    // ==========================================================================
    // 6. USUARIOS
    // ==========================================================================
    private static async Task SeedUsuariosAsync(AppDbContext context, IAuthService authService, Guid entidadId, Guid sucursalId)
    {
        var masterRoleId = await context.Rols
      .IgnoreQueryFilters()
      .Where(r => r.Codigo == "MASTER")
      .Select(r => r.Id)
      .FirstAsync();

        var adminRoleId = await context.Rols
            .IgnoreQueryFilters()
            .Where(r => r.Codigo == "ADMINISTRADOR")
            .Select(r => r.Id)
            .FirstAsync();

        var masterUser = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = sucursalId,
            NombreUsuario = "master",
            NombreCompleto = "Usuario Maestro",
            Email = "master@email.cu",
            HashPassword = authService.HashPassword("MasterVERCOM2026*"),
            Activo = true,
            DebeCambiarPass = false,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };

        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = sucursalId,
            NombreUsuario = "admin",
            NombreCompleto = "Administrador Local",
            Email = "admin@email.cu",
            HashPassword = authService.HashPassword("admin123*"),
            Activo = true,
            DebeCambiarPass = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };

        context.Usuarios.Add(masterUser);
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        context.UsuarioRols.AddRange(new List<UsuarioRol>
        {
            new() { UsuarioId = masterUser.Id, RolId = masterRoleId, SucursalId = sucursalId, AsignadoEn = DateTimeOffset.UtcNow },
            new() { UsuarioId = admin.Id, RolId = adminRoleId, SucursalId = sucursalId, AsignadoEn = DateTimeOffset.UtcNow }
        });
        await context.SaveChangesAsync();

        Console.WriteLine("   âœ… Usuarios master y admin creados");
    }

    // ==========================================================================
    // 7. CARGOS
    // ==========================================================================
    private static async Task SeedCargosAsync(AppDbContext context, Guid entidadId)
    {
        var cargos = new List<Cargo>
        {
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "DIR-01", Nombre = "Director General", CategoriaOcupacional = "DIRIGENTE", Funciones = "DirecciÃ³n y representaciÃ³n legal de la entidad.", SalarioEscalaMin = 8000, SalarioEscalaMax = 12000 },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "CONT-01", Nombre = "Especialista B en Contabilidad", CategoriaOcupacional = "TECNICO", Funciones = "Control econÃ³mico y financiero de la entidad.", SalarioEscalaMin = 6500, SalarioEscalaMax = 8500 },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "ADM-01", Nombre = "Especialista en RRHH", CategoriaOcupacional = "TECNICO", Funciones = "GestiÃ³n de personal y nÃ³mina.", SalarioEscalaMin = 5500, SalarioEscalaMax = 7500 },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "VEN-01", Nombre = "Vendedor", CategoriaOcupacional = "OPERARIO", Funciones = "AtenciÃ³n al cliente y ventas.", SalarioEscalaMin = 3500, SalarioEscalaMax = 5500 },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "ALM-01", Nombre = "Almacenero", CategoriaOcupacional = "OPERARIO", Funciones = "Control de inventario y almacÃ©n.", SalarioEscalaMin = 3500, SalarioEscalaMax = 5000 },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "OPER-01", Nombre = "Operario Integral", CategoriaOcupacional = "OPERARIO", Funciones = "Labores de producciÃ³n y servicios generales.", SalarioEscalaMin = 4500, SalarioEscalaMax = 6000 }
        };

        foreach (var c in cargos)
        {
            if (!await context.Cargos.IgnoreQueryFilters().AnyAsync(x => x.EntidadId == entidadId && x.Codigo == c.Codigo))
                context.Cargos.Add(c);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {cargos.Count} cargos procesados");
    }

    // ==========================================================================
    // 8. UNIDADES DE MEDIDA
    // ==========================================================================
    private static async Task SeedUnidadesMedidaAsync(AppDbContext context)
    {
        var unidades = new List<UnidadMedidum>
        {
            new() { Codigo = "UN", Nombre = "Unidad", EsFraccionable = false },
            new() { Codigo = "KG", Nombre = "Kilogramo", EsFraccionable = true },
            new() { Codigo = "G", Nombre = "Gramo", EsFraccionable = true },
            new() { Codigo = "L", Nombre = "Litro", EsFraccionable = true },
            new() { Codigo = "ML", Nombre = "Mililitro", EsFraccionable = true },
            new() { Codigo = "M", Nombre = "Metro", EsFraccionable = true },
            new() { Codigo = "M2", Nombre = "Metro Cuadrado", EsFraccionable = true },
            new() { Codigo = "M3", Nombre = "Metro CÃºbico", EsFraccionable = true },
            new() { Codigo = "CAJ", Nombre = "Caja", EsFraccionable = false },
            new() { Codigo = "PAQ", Nombre = "Paquete", EsFraccionable = false },
            new() { Codigo = "DOC", Nombre = "Docena", EsFraccionable = false },
            new() { Codigo = "TON", Nombre = "Tonelada", EsFraccionable = true }
        };

        foreach (var u in unidades)
        {
            if (!await context.UnidadMedida.AnyAsync(x => x.Codigo == u.Codigo))
                context.UnidadMedida.Add(u);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {unidades.Count} unidades de medida procesadas");
    }

    // ==========================================================================
    // 9. TIPOS DE MOVIMIENTO
    // ==========================================================================
    private static async Task SeedTiposMovimientoAsync(AppDbContext context)
    {
        var tipos = new List<TipoMovimiento>
        {
            new() { Codigo = "RECEPCION", Nombre = "RecepciÃ³n de MercancÃ­a", Naturaleza = "ENTRADA", AfectaCosto = true },
            new() { Codigo = "VALE_ENTREGA", Nombre = "Vale de Entrega", Naturaleza = "SALIDA", AfectaCosto = true },
            new() { Codigo = "DEVOLUCION_ENTRADA", Nombre = "DevoluciÃ³n de Cliente", Naturaleza = "ENTRADA", AfectaCosto = true },
            new() { Codigo = "DEVOLUCION_SALIDA", Nombre = "DevoluciÃ³n a Proveedor", Naturaleza = "SALIDA", AfectaCosto = true },
            new() { Codigo = "TRANSFERENCIA_SALIDA", Nombre = "Transferencia - Salida", Naturaleza = "SALIDA", AfectaCosto = false },
            new() { Codigo = "TRANSFERENCIA_ENTRADA", Nombre = "Transferencia - Entrada", Naturaleza = "ENTRADA", AfectaCosto = false },
            new() { Codigo = "AJUSTE_POSITIVO", Nombre = "Ajuste por Sobrante", Naturaleza = "ENTRADA", AfectaCosto = true },
            new() { Codigo = "AJUSTE_NEGATIVO", Nombre = "Ajuste por Faltante", Naturaleza = "SALIDA", AfectaCosto = true },
            new() { Codigo = "CONSUMO_PRODUCCION", Nombre = "Consumo en ProducciÃ³n", Naturaleza = "SALIDA", AfectaCosto = true },
            new() { Codigo = "ENTRADA_PRODUCCION", Nombre = "Entrada de Producto Terminado", Naturaleza = "ENTRADA", AfectaCosto = true },
            new() { Codigo = "VENTA_POS", Nombre = "Venta en Punto de Venta", Naturaleza = "SALIDA", AfectaCosto = true }
        };

        foreach (var t in tipos)
        {
            if (!await context.TipoMovimientos.AnyAsync(x => x.Codigo == t.Codigo))
                context.TipoMovimientos.Add(t);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {tipos.Count} tipos de movimiento procesados");
    }

    // ==========================================================================
    // 10. TIPOS DE AUSENCIA
    // ==========================================================================
    private static async Task SeedTiposAusenciaAsync(AppDbContext context)
    {
        var tipos = new List<TipoAusencium>
        {
            new() { Codigo = "VACACIONES", Nombre = "Vacaciones", Remunerada = true, AfectaVacaciones = false },
            new() { Codigo = "CERT_MEDICO", Nombre = "Certificado MÃ©dico", Remunerada = true, AfectaVacaciones = false },
            new() { Codigo = "LICENCIA_NO_RETRIBUIDA", Nombre = "Licencia no Retribuida", Remunerada = false, AfectaVacaciones = true },
            new() { Codigo = "MATERNIDAD", Nombre = "Licencia de Maternidad", Remunerada = true, AfectaVacaciones = false },
            new() { Codigo = "AUSENCIA_INJUSTIFICADA", Nombre = "Ausencia Injustificada", Remunerada = false, AfectaVacaciones = true }
        };

        foreach (var t in tipos)
        {
            if (!await context.TipoAusencia.AnyAsync(x => x.Codigo == t.Codigo))
                context.TipoAusencia.Add(t);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {tipos.Count} tipos de ausencia procesados");
    }

    // ==========================================================================
    // 11. TIPOS DE COMPROBANTE
    // ==========================================================================
    private static async Task SeedTiposComprobanteAsync(AppDbContext context)
    {
        var tipos = new List<TipoComprobante>
        {
            new() { Codigo = "ING", Nombre = "Comprobante de Ingreso" },
            new() { Codigo = "EGR", Nombre = "Comprobante de Egreso" },
            new() { Codigo = "DIA", Nombre = "Comprobante Diario / Operaciones" },
            new() { Codigo = "AJU", Nombre = "Comprobante de Ajuste / ReversiÃ³n" }
        };

        foreach (var t in tipos)
        {
            if (!await context.TipoComprobantes.AnyAsync(x => x.Codigo == t.Codigo))
                context.TipoComprobantes.Add(t);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {tipos.Count} tipos de comprobante procesados");
    }

    // ==========================================================================
    // 12. CONCEPTOS DE NÃ“MINA
    // ==========================================================================
    private static async Task SeedConceptosNominaAsync(AppDbContext context, Guid entidadId)
    {
        var conceptos = new List<ConceptoNomina>
        {
            new() { Codigo = "SAL_BASICO", Nombre = "Salario BÃ¡sico", Tipo = "DEVENGO", EntidadId = entidadId, Activo = true },
            new() { Codigo = "PAGO_RESULTADOS", Nombre = "Pago por Resultados", Tipo = "DEVENGO", EntidadId = entidadId, Activo = true },
            new() { Codigo = "HORA_EXTRA", Nombre = "Horas Extras", Tipo = "DEVENGO", EntidadId = entidadId, Activo = true },
            new() { Codigo = "SUBSIDIO_SS", Nombre = "Subsidio Seguridad Social (corto plazo)", Tipo = "DEVENGO", EntidadId = entidadId, Activo = true },
            new() { Codigo = "VACACIONES_PAGO", Nombre = "Pago de Vacaciones", Tipo = "DEVENGO", EntidadId = entidadId, Activo = true },
            new() { Codigo = "CONT_SS_TRAB", Nombre = "ContribuciÃ³n Especial Seguridad Social (trabajador)", Tipo = "DEDUCCION", EntidadId = entidadId, Activo = true },
            new() { Codigo = "IMP_INGRESOS_PERS", Nombre = "Impuesto sobre Ingresos Personales", Tipo = "DEDUCCION", EntidadId = entidadId, Activo = true },
            new() { Codigo = "OTRAS_DEDUCCIONES", Nombre = "Otras Deducciones", Tipo = "DEDUCCION", EntidadId = entidadId, Activo = true },
            new() { Codigo = "APORTE_SS_PATRONAL", Nombre = "Aporte Seguridad Social (empleador)", Tipo = "APORTE_PATRONAL", EntidadId = entidadId, Activo = true },
            new() { Codigo = "APORTE_FUERZA_TRABAJO", Nombre = "Impuesto Fuerza de Trabajo (empleador)", Tipo = "APORTE_PATRONAL", EntidadId = entidadId, Activo = true }
        };

        foreach (var c in conceptos)
        {
            if (!await context.ConceptoNominas.IgnoreQueryFilters().AnyAsync(x => x.Codigo == c.Codigo))
                context.ConceptoNominas.Add(c);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {conceptos.Count} conceptos de nÃ³mina procesados");
    }

    // ==========================================================================
    // 13. INDICADORES BI
    // ==========================================================================
    private static async Task SeedIndicadoresAsync(AppDbContext context)
    {
        var indicadores = new List<Indicador>
        {
            new() { Codigo = "LIQUIDEZ", Nombre = "Ãndice de Liquidez", Categoria = "LIQUIDEZ", FormulaDescripcion = "Activo Corriente / Pasivo Corriente" },
            new() { Codigo = "RENTABILIDAD", Nombre = "Margen de Utilidad Neta", Categoria = "RENTABILIDAD", FormulaDescripcion = "Utilidad Neta / Ventas Totales" },
            new() { Codigo = "ROT_INV", Nombre = "RotaciÃ³n de Inventario", Categoria = "OPERATIVO", FormulaDescripcion = "Costo de Ventas / Inventario Promedio" },
            new() { Codigo = "CICLO_COBRO", Nombre = "Ciclo de Cobro (DÃ­as)", Categoria = "OPERATIVO", FormulaDescripcion = "Promedio CxC / Ventas Diarias" },
            new() { Codigo = "CICLO_PAGO", Nombre = "Ciclo de Pago (DÃ­as)", Categoria = "OPERATIVO", FormulaDescripcion = "Promedio CxP / Compras Diarias" },
            new() { Codigo = "MARGEN_BRUTO", Nombre = "Margen Bruto", Categoria = "RENTABILIDAD", FormulaDescripcion = "(Ventas - Costo de Ventas) / Ventas" }
        };

        foreach (var i in indicadores)
        {
            if (!await context.Indicadors.AnyAsync(ind => ind.Codigo == i.Codigo))
                context.Indicadors.Add(i);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {indicadores.Count} indicadores BI procesados");
    }

    // ==========================================================================
    // 14. PARÃMETROS FISCALES
    // ==========================================================================
    private static async Task SeedParametrosFiscalesAsync(AppDbContext context, Guid entidadId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var parametros = new List<ParametroSistema>
        {
            new() { EntidadId = entidadId, Codigo = "TASA_SS_PATRONAL", Valor = "0.125", TipoDato = "NUMERIC", Descripcion = "Tasa de ContribuciÃ³n a la Seguridad Social (Entidad)", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "TASA_FUERZA_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "Impuesto por la UtilizaciÃ³n de la Fuerza de Trabajo", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "RET_SS_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "RetenciÃ³n Seguridad Social Trabajador", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "FACTOR_VAC", Valor = "0.0909", TipoDato = "NUMERIC", Descripcion = "Factor AcumulaciÃ³n Vacaciones (9.09%)", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "IRP_MIN_EXENTO", Valor = "3260", TipoDato = "NUMERIC", Descripcion = "MÃ­nimo Exento Impuesto sobre Ingresos Personales", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "IRP_TASA", Valor = "0.03", TipoDato = "NUMERIC", Descripcion = "Tasa General Impuesto sobre Ingresos Personales", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "TASA_IMP_VENTAS", Valor = "0.10", TipoDato = "NUMERIC", Descripcion = "Tasa del Impuesto sobre Ventas", VigenteDesde = hoy },
            new() { EntidadId = entidadId, Codigo = "TASA_IMP_SERVICIOS", Valor = "0.10", TipoDato = "NUMERIC", Descripcion = "Tasa del Impuesto sobre Servicios", VigenteDesde = hoy }
        };

        foreach (var p in parametros)
        {
            if (!await context.ParametroSistemas.IgnoreQueryFilters().AnyAsync(ps => ps.EntidadId == entidadId && ps.Codigo == p.Codigo))
                context.ParametroSistemas.Add(p);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {parametros.Count} parÃ¡metros fiscales procesados");
    }

    // ==========================================================================
    // 15. TIPOS DE OBLIGACIÃ“N FISCAL
    // ==========================================================================
    private static async Task SeedTiposObligacionFiscalAsync(AppDbContext context)
    {
        var obligaciones = new List<TipoObligacionFiscal>
        {
            new() { Codigo = "IMP_UTILIDADES", Nombre = "Impuesto sobre Utilidades", Periodicidad = "ANUAL", BaseLegal = "Ley 113 del Sistema Tributario" },
            new() { Codigo = "IMP_VENTAS", Nombre = "Impuesto sobre Ventas", Periodicidad = "MENSUAL", BaseLegal = "Ley 113 del Sistema Tributario" },
            new() { Codigo = "IMP_SERVICIOS", Nombre = "Impuesto sobre Servicios", Periodicidad = "MENSUAL", BaseLegal = "Ley 113 del Sistema Tributario" },
            new() { Codigo = "SEG_SOCIAL", Nombre = "ContribuciÃ³n a la Seguridad Social", Periodicidad = "MENSUAL", BaseLegal = "Ley 113 del Sistema Tributario" },
            new() { Codigo = "FUERZA_TRABAJO", Nombre = "Impuesto por UtilizaciÃ³n de Fuerza de Trabajo", Periodicidad = "MENSUAL", BaseLegal = "Ley 113 del Sistema Tributario" }
        };

        foreach (var o in obligaciones)
        {
            if (!await context.TipoObligacionFiscals.AnyAsync(x => x.Codigo == o.Codigo))
                context.TipoObligacionFiscals.Add(o);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {obligaciones.Count} tipos de obligaciÃ³n fiscal procesados");
    }

    // ==========================================================================
    // 16. ALMACENES
    // ==========================================================================
    private static async Task SeedAlmacenesAsync(AppDbContext context, Guid entidadId, Guid sucursalId)
    {
        var almacenes = new List<Almacen>
        {
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, SucursalId = sucursalId, Codigo = "ALM01", Nombre = "AlmacÃ©n Central", EsPuntoVenta = false, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, SucursalId = sucursalId, Codigo = "TIENDA01", Nombre = "Tienda Principal", EsPuntoVenta = true, Activo = true }
        };

        foreach (var a in almacenes)
        {
            if (!await context.Almacens.IgnoreQueryFilters().AnyAsync(x => x.EntidadId == entidadId && x.Codigo == a.Codigo))
                context.Almacens.Add(a);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {almacenes.Count} almacenes procesados");
    }

    // ==========================================================================
    // 17. PLAN DE CUENTAS BÃSICO
    // ==========================================================================
    private static async Task SeedPlanCuentasAsync(AppDbContext context, Guid entidadId)
    {
        var cuentas = new List<CuentaContable>
        {
            // ACTIVO
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "101", Nombre = "Efectivo en Caja", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "109", Nombre = "Efectivo en Banco", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "135", Nombre = "Cuentas por Cobrar a Corto Plazo", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, RequiereTercero = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "183", Nombre = "Materias Primas y Materiales", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "188", Nombre = "ProducciÃ³n Terminada", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "189", Nombre = "MercancÃ­as para la Venta", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "240", Nombre = "Activos Fijos Tangibles", Nivel = 3, Clase = "ACTIVO", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "300", Nombre = "DepreciaciÃ³n de Activos Fijos Tangibles", Nivel = 3, Clase = "ACTIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },

            // PASIVO
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "405", Nombre = "Cuentas por Pagar a Corto Plazo", Nivel = 3, Clase = "PASIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, RequiereTercero = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "440", Nombre = "Obligaciones con el Presupuesto del Estado", Nivel = 3, Clase = "PASIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "455", Nombre = "NÃ³minas por Pagar", Nivel = 3, Clase = "PASIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "460", Nombre = "Retenciones por Pagar", Nivel = 3, Clase = "PASIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "492", Nombre = "ProvisiÃ³n para Vacaciones", Nivel = 3, Clase = "PASIVO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },

            // PATRIMONIO
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "600", Nombre = "Capital Social", Nivel = 3, Clase = "PATRIMONIO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "630", Nombre = "Utilidades Retenidas", Nivel = 3, Clase = "PATRIMONIO", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },

            // INGRESOS
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "900", Nombre = "Ventas de ProducciÃ³n", Nivel = 3, Clase = "INGRESOS", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "901", Nombre = "Ventas de Servicios", Nivel = 3, Clase = "INGRESOS", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "904", Nombre = "Venta de MercancÃ­as", Nivel = 3, Clase = "INGRESOS", Naturaleza = "ACREEDORA", AceptaMovimiento = true, Activo = true },

            // GASTOS
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "810", Nombre = "Costo de Ventas de Producciones", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "814", Nombre = "Costo de Venta de MercancÃ­as", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "822", Nombre = "Gastos Generales y de AdministraciÃ³n", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "835", Nombre = "Gastos Financieros", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "855", Nombre = "Impuesto por la UtilizaciÃ³n de la Fuerza de Trabajo 5%", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "856", Nombre = "Seguridad Social a Largo Plazo 12.5%", Nivel = 3, Clase = "GASTOS", Naturaleza = "DEUDORA", AceptaMovimiento = true, Activo = true }
        };

        foreach (var c in cuentas)
        {
            if (!await context.CuentaContables.IgnoreQueryFilters().AnyAsync(x => x.EntidadId == entidadId && x.Codigo == c.Codigo))
                context.CuentaContables.Add(c);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {cuentas.Count} cuentas contables procesadas");
    }

    // ==========================================================================
    // 18. LISTAS DE PRECIO
    // ==========================================================================
    private static async Task SeedListasPrecioAsync(AppDbContext context, Guid entidadId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var listas = new List<ListaPrecio>
        {
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Nombre = "Precios Generales", Canal = "GENERAL", VigenteDesde = hoy, Activa = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Nombre = "Precios Mayoristas", Canal = "ERP", VigenteDesde = hoy, Activa = true },
            new() { Id = Guid.NewGuid(), EntidadId = entidadId, Nombre = "Precios Punto de Venta", Canal = "POS", VigenteDesde = hoy, Activa = true }
        };

        foreach (var l in listas)
        {
            if (!await context.ListaPrecios.IgnoreQueryFilters().AnyAsync(x => x.EntidadId == entidadId && x.Nombre == l.Nombre))
                context.ListaPrecios.Add(l);
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"   âœ… {listas.Count} listas de precio procesadas");
    }
}
