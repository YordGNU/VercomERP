using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        var authService = serviceProvider.GetRequiredService<IAuthService>();

        // Si ya hay usuarios, asumimos que el seed ya se ejecutó
        if (await context.Usuarios.AnyAsync())
            return;
        // ======================================================================
        // 1. ENTIDAD (verificar existencia para evitar duplicados)
        // ======================================================================
        var entidadId = Guid.NewGuid(); // valor por defecto
        var entidad = await context.Entidads.FirstOrDefaultAsync(e => e.Nit == "12345678901");
        if (entidad == null)
        {
            // No existe, crear nueva
            entidad = new Entidad
            {
                Id = Guid.NewGuid(),
                RazonSocial = "Tierra Prometida S.U.R.L.",
                Nit = "12345678901",
                FormaJuridica = "S.U.R.L.",
                DireccionLegal = "Cuba",
                MonedaBase = "CUP",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
                ActualizadoEn = DateTimeOffset.UtcNow,
                CodigoReeup = Guid.NewGuid().ToString()  // Asignar un valor único para evitar NULL duplicado
            };
            context.Entidads.Add(entidad);
            await context.SaveChangesAsync();
        }
        entidadId = entidad.Id;

        // ======================================================================
        // 2. SUCURSAL (siempre asegurar que exista para esta entidad)
        // ======================================================================
        var sucursal = await context.Sucursals.FirstOrDefaultAsync(s => s.EntidadId == entidadId);
        if (sucursal == null)
        {
            sucursal = new Sucursal
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Codigo = "SUC_01",
                Nombre = "Sucursal Central",
                Tipo = "OFICINA",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow
            };
            context.Sucursals.Add(sucursal);
            await context.SaveChangesAsync();
        }
        var sucursalId = sucursal.Id;
        // ======================================================================
        // 3. PERMISOS (lista completa: 98 códigos)
        // ======================================================================
        var todosLosPermisos = new List<Permiso>
        {
            // === SEGURIDAD ===
            new Permiso { Codigo = "SEC_VIEW_USERS", Modulo = "SEGURIDAD", Descripcion = "Ver listado de usuarios" },
            new Permiso { Codigo = "SEC_EDIT_USERS", Modulo = "SEGURIDAD", Descripcion = "Crear y editar usuarios" },
            new Permiso { Codigo = "SEC_VIEW_ROLES", Modulo = "SEGURIDAD", Descripcion = "Ver y gestionar roles/permisos" },
            new Permiso { Codigo = "SEC_VIEW_AUDIT", Modulo = "SEGURIDAD", Descripcion = "Consultar bitácora de auditoría" },

            // === CONTABILIDAD ===
            new Permiso { Codigo = "ACC_VIEW_PLAN", Modulo = "CONTABILIDAD", Descripcion = "Ver plan de cuentas" },
            new Permiso { Codigo = "ACC_EDIT_PLAN", Modulo = "CONTABILIDAD", Descripcion = "Modificar cuentas contables" },
            new Permiso { Codigo = "ACC_CREATE_ENTRY", Modulo = "CONTABILIDAD", Descripcion = "Crear asientos contables" },
            new Permiso { Codigo = "ACC_POST_ENTRY", Modulo = "CONTABILIDAD", Descripcion = "Contabilizar asientos (Firme)" },
            new Permiso { Codigo = "ACC_CLOSE_PERIOD", Modulo = "CONTABILIDAD", Descripcion = "Cerrar periodos contables" },

            // === INVENTARIO ===
            new Permiso { Codigo = "INVENTARIO.FAMILIA.VER", Modulo = "INVENTARIO", Descripcion = "Ver familias" },
            new Permiso { Codigo = "INVENTARIO.ALMACEN.CREAR", Modulo = "INVENTARIO", Descripcion = "Crear almacenes" },
            new Permiso { Codigo = "INVENTARIO.ALMACEN.VER", Modulo = "INVENTARIO", Descripcion = "Ver almacenes" },
            new Permiso { Codigo = "INVENTARIO.EXISTENCIA.VER", Modulo = "INVENTARIO", Descripcion = "Consultar existencias" },
            new Permiso { Codigo = "INVENTARIO.MOVIMIENTO.CREAR", Modulo = "INVENTARIO", Descripcion = "Crear movimientos de inventario" },
            new Permiso { Codigo = "INVENTARIO.MOVIMIENTO.VER", Modulo = "INVENTARIO", Descripcion = "Ver movimientos de inventario" },
            new Permiso { Codigo = "INVENTARIO.CONTEO.CREAR", Modulo = "INVENTARIO", Descripcion = "Realizar conteos físicos" },
            new Permiso { Codigo = "INVENTARIO.CONTEO.VER", Modulo = "INVENTARIO", Descripcion = "Ver conteos físicos" },
            new Permiso { Codigo = "INVENTARIO.LISTA_PRECIO.CREAR", Modulo = "INVENTARIO", Descripcion = "Crear listas de precios" },
            new Permiso { Codigo = "INVENTARIO.LISTA_PRECIO.VER", Modulo = "INVENTARIO", Descripcion = "Ver listas de precios" },

            // === PRODUCCIÓN ===
            new Permiso { Codigo = "PRODUCCION.FICHA.CREAR", Modulo = "PRODUCCION", Descripcion = "Crear fichas de costo" },
            new Permiso { Codigo = "PRODUCCION.FICHA.VER", Modulo = "PRODUCCION", Descripcion = "Ver fichas de costo" },
            new Permiso { Codigo = "PRODUCCION.BOM.CREAR", Modulo = "PRODUCCION", Descripcion = "Crear lista de materiales (BOM)" },
            new Permiso { Codigo = "PRODUCCION.BOM.VER", Modulo = "PRODUCCION", Descripcion = "Ver lista de materiales" },
            new Permiso { Codigo = "PRODUCCION.ORDEN.CREAR", Modulo = "PRODUCCION", Descripcion = "Crear órdenes de producción" },
            new Permiso { Codigo = "PRODUCCION.ORDEN.VER", Modulo = "PRODUCCION", Descripcion = "Ver órdenes de producción" },
            new Permiso { Codigo = "PRODUCCION.ORDEN.EJECUTAR", Modulo = "PRODUCCION", Descripcion = "Ejecutar órdenes de producción (avanzar estado)" },
            new Permiso { Codigo = "PRODUCCION.PLAN.CREAR", Modulo = "PRODUCCION", Descripcion = "Crear planes de producción" },
            new Permiso { Codigo = "PRODUCCION.PLAN.VER", Modulo = "PRODUCCION", Descripcion = "Ver planes de producción" },
            new Permiso { Codigo = "PRODUCCION.MERMA.REGISTRAR", Modulo = "PRODUCCION", Descripcion = "Registrar mermas" },
            new Permiso { Codigo = "PRODUCCION.MANTENIMIENTO.CREAR", Modulo = "PRODUCCION", Descripcion = "Crear mantenimiento de equipos" },
            new Permiso { Codigo = "PRODUCCION.MANTENIMIENTO.VER", Modulo = "PRODUCCION", Descripcion = "Ver mantenimientos" },

            // === COMERCIAL ===
            new Permiso { Codigo = "COMERCIAL.PROVEEDOR.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear proveedores" },
            new Permiso { Codigo = "COMERCIAL.PROVEEDOR.EDITAR", Modulo = "COMERCIAL", Descripcion = "Editar proveedores" },
            new Permiso { Codigo = "COMERCIAL.PROVEEDOR.VER", Modulo = "COMERCIAL", Descripcion = "Ver proveedores" },
            new Permiso { Codigo = "COMERCIAL.CLIENTE.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear clientes" },
            new Permiso { Codigo = "COMERCIAL.CLIENTE.EDITAR", Modulo = "COMERCIAL", Descripcion = "Editar clientes" },
            new Permiso { Codigo = "COMERCIAL.CLIENTE.VER", Modulo = "COMERCIAL", Descripcion = "Ver clientes" },
            new Permiso { Codigo = "COMERCIAL.CONTRATO.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear contratos económicos" },
            new Permiso { Codigo = "COMERCIAL.CONTRATO.VER", Modulo = "COMERCIAL", Descripcion = "Ver contratos económicos" },
            new Permiso { Codigo = "COMERCIAL.ORDEN_COMPRA.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear órdenes de compra" },
            new Permiso { Codigo = "COMERCIAL.ORDEN_COMPRA.VER", Modulo = "COMERCIAL", Descripcion = "Ver órdenes de compra" },
            new Permiso { Codigo = "COMERCIAL.ORDEN_COMPRA.APROBAR", Modulo = "COMERCIAL", Descripcion = "Aprobar órdenes de compra" },
            new Permiso { Codigo = "COMERCIAL.FACTURA_VENTA.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear facturas de venta" },
            new Permiso { Codigo = "COMERCIAL.FACTURA_VENTA.VER", Modulo = "COMERCIAL", Descripcion = "Ver facturas de venta" },
            new Permiso { Codigo = "COMERCIAL.FACTURA_VENTA.ANULAR", Modulo = "COMERCIAL", Descripcion = "Anular facturas de venta" },
            new Permiso { Codigo = "COMERCIAL.DEVOLUCION.CREAR", Modulo = "COMERCIAL", Descripcion = "Crear devoluciones de venta" },
            new Permiso { Codigo = "COMERCIAL.TOPE_PRECIO.VER", Modulo = "COMERCIAL", Descripcion = "Ver topes de precio MFP" },

            // === RRHH ===
            new Permiso { Codigo = "HR_VIEW_STAFF", Modulo = "RRHH", Descripcion = "Ver expedientes de empleados" },
            new Permiso { Codigo = "HR_PAYROLL_RUN", Modulo = "RRHH", Descripcion = "Procesar y aprobar nómina" },

            // === REPORTES ===
            new Permiso { Codigo = "REPORTES.INDICADOR.VER", Modulo = "REPORTES", Descripcion = "Ver indicadores de gestión" },
            new Permiso { Codigo = "REPORTES.PAQUETE.CREAR", Modulo = "REPORTES", Descripcion = "Generar paquetes de información" },
            new Permiso { Codigo = "REPORTES.PAQUETE.VER", Modulo = "REPORTES", Descripcion = "Ver paquetes de información" },
            new Permiso { Codigo = "REPORTES.GENERAR", Modulo = "REPORTES", Descripcion = "Generar reportes personalizados" },
            new Permiso { Codigo = "REPORTES.EXPORTAR", Modulo = "REPORTES", Descripcion = "Exportar reportes (PDF, Excel, CSV)" },

            // === INTEGRACIÓN ===
            new Permiso { Codigo = "INTEGRACION.API.CLIENTE.CREAR", Modulo = "INTEGRACION", Descripcion = "Crear clientes API" },
            new Permiso { Codigo = "INTEGRACION.API.CLIENTE.VER", Modulo = "INTEGRACION", Descripcion = "Ver clientes API" },
            new Permiso { Codigo = "INTEGRACION.API.CLIENTE.REVOCAR", Modulo = "INTEGRACION", Descripcion = "Revocar clientes API" },
            new Permiso { Codigo = "INTEGRACION.POS.DISPOSITIVO.CREAR", Modulo = "INTEGRACION", Descripcion = "Crear dispositivos POS" },
            new Permiso { Codigo = "INTEGRACION.POS.DISPOSITIVO.VER", Modulo = "INTEGRACION", Descripcion = "Ver dispositivos POS" },
            new Permiso { Codigo = "INTEGRACION.POS.SESION.CERRAR", Modulo = "INTEGRACION", Descripcion = "Cerrar sesiones de caja POS" },
            new Permiso { Codigo = "INTEGRACION.POS.SESION.VER", Modulo = "INTEGRACION", Descripcion = "Ver sesiones de caja POS" },
            new Permiso { Codigo = "INTEGRACION.WEBHOOK.CREAR", Modulo = "INTEGRACION", Descripcion = "Crear suscripciones de webhook" },
            new Permiso { Codigo = "INTEGRACION.WEBHOOK.VER", Modulo = "INTEGRACION", Descripcion = "Ver suscripciones de webhook" }
        };

        // Insertar permisos que no existan
        foreach (var permiso in todosLosPermisos)
        {
            if (!await context.Permisos.AnyAsync(p => p.Codigo == permiso.Codigo))
                context.Permisos.Add(permiso);
        }
        await context.SaveChangesAsync();

        // ======================================================================
        // 4. ROLES
        // ======================================================================
        if (!await context.Rols.AnyAsync())
        {
            var roles = new List<Rol>
            {
                new Rol { Codigo = "ADMINISTRADOR", Nombre = "Administrador Total", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "CONTADOR", Nombre = "Contabilidad General", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "ECONOMICO", Nombre = "Económico", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "JEFE_PRODUCCION", Nombre = "Jefe de Producción", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "ALMACENERO", Nombre = "Almacenero", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "RRHH", Nombre = "Recursos Humanos", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "COMERCIAL", Nombre = "Comercial / Ventas", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "CAJERO_POS", Nombre = "Cajero de Punto de Venta", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow },
                new Rol { Codigo = "DIRECCION", Nombre = "Dirección", EsSistema = true, CreadoEn = DateTimeOffset.UtcNow }
            };
            context.Rols.AddRange(roles);
            await context.SaveChangesAsync();
        }

        // ======================================================================
        // 5. ASIGNACIÓN DE PERMISOS A ROLES
        // ======================================================================
        var todos = await context.Permisos.ToListAsync();

        // ADMINISTRADOR → todos
        var adminRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "ADMINISTRADOR");
        adminRole.Permisos = todos;

        // CONTADOR → contabilidad + seguridad (ver) + reportes financieros
        var contadorRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "CONTADOR");
        contadorRole.Permisos = todos
            .Where(p => p.Modulo == "CONTABILIDAD" ||
                        (p.Modulo == "SEGURIDAD" && p.Codigo.Contains("VIEW")) ||
                        (p.Modulo == "REPORTES" && (p.Codigo.Contains("INDICADOR") || p.Codigo.Contains("PAQUETE") || p.Codigo == "REPORTES.GENERAR" || p.Codigo == "REPORTES.EXPORTAR")))
            .ToList();

        // ECONOMICO → finanzas, presupuesto, parámetros, contabilidad (ver), reportes
        var economicoRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "ECONOMICO");
        economicoRole.Permisos = todos
            .Where(p => p.Modulo == "CONTABILIDAD" ||
                        p.Modulo == "REPORTES" ||
                        p.Codigo == "SEC_VIEW_USERS" ||
                        p.Codigo == "SEC_VIEW_AUDIT" ||
                        p.Codigo == "SEC_VIEW_ROLES")
            .ToList();

        // JEFE_PRODUCCION → producción + inventario (ver) + mantenimiento + mermas
        var produccionRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "JEFE_PRODUCCION");
        produccionRole.Permisos = todos
            .Where(p => p.Modulo == "PRODUCCION" ||
                        (p.Modulo == "INVENTARIO" && (p.Codigo.Contains("VER") || p.Codigo.Contains("MOVIMIENTO.VER") || p.Codigo.Contains("EXISTENCIA.VER"))))
            .ToList();

        // ALMACENERO → inventario (crear, ver, movimientos, conteo, listas) + algunas vistas de compras/ventas
        var almaceneroRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "ALMACENERO");
        almaceneroRole.Permisos = todos
            .Where(p => p.Modulo == "INVENTARIO" ||
                        (p.Modulo == "COMERCIAL" && (p.Codigo.Contains("ORDEN_COMPRA.VER") || p.Codigo.Contains("PROVEEDOR.VER") || p.Codigo.Contains("FACTURA_VENTA.VER"))))
            .ToList();

        // RRHH → empleados, cargos, contratos, asistencia, nómina, vacaciones, reportes RRHH
        var rrhhRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "RRHH");
        rrhhRole.Permisos = todos
            .Where(p => p.Modulo == "RRHH" ||
                        (p.Modulo == "REPORTES" && (p.Codigo.Contains("GENERAR") || p.Codigo.Contains("EXPORTAR"))))
            .ToList();

        // COMERCIAL → clientes, proveedores, contratos, órdenes compra, facturas venta, devoluciones, topes precio + inventario (vista)
        var comercialRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "COMERCIAL");
        comercialRole.Permisos = todos
            .Where(p => p.Modulo == "COMERCIAL" ||
                        (p.Modulo == "INVENTARIO" && (p.Codigo.Contains("PRODUCTO.VER") || p.Codigo.Contains("EXISTENCIA.VER") || p.Codigo.Contains("LISTA_PRECIO.VER"))))
            .ToList();

        // CAJERO_POS → facturas venta (crear/ver), clientes (crear/ver), devoluciones, inventario (vista), sesiones POS
        var cajeroRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "CAJERO_POS");
        cajeroRole.Permisos = todos
            .Where(p => p.Codigo == "COMERCIAL.FACTURA_VENTA.CREAR" ||
                        p.Codigo == "COMERCIAL.FACTURA_VENTA.VER" ||
                        p.Codigo == "COMERCIAL.CLIENTE.CREAR" ||
                        p.Codigo == "COMERCIAL.CLIENTE.VER" ||
                        p.Codigo == "COMERCIAL.DEVOLUCION.CREAR" ||
                        p.Codigo == "INVENTARIO.EXISTENCIA.VER" ||
                        p.Codigo == "INVENTARIO.LISTA_PRECIO.VER" ||
                        p.Codigo == "INVENTARIO.ALMACEN.VER" ||
                        p.Codigo == "INTEGRACION.POS.SESION.VER" ||
                        p.Codigo == "INTEGRACION.POS.SESION.CERRAR")
            .ToList();

        // DIRECCION → reportes, indicadores, paquetes, balances, presupuestos (ver), auditoría, parámetros, existencias, nómina (ver)
        var direccionRole = await context.Rols.Include(r => r.Permisos).FirstAsync(r => r.Codigo == "DIRECCION");
        direccionRole.Permisos = todos
            .Where(p => p.Modulo == "REPORTES" ||
                        (p.Modulo == "CONTABILIDAD" && (p.Codigo.Contains("BALANCE") || p.Codigo.Contains("PRESUPUESTO.VER") || p.Codigo.Contains("CXC.VER") || p.Codigo.Contains("CXP.VER"))) ||
                        (p.Modulo == "SEGURIDAD" && (p.Codigo.Contains("VIEW") || p.Codigo == "SEC_EDIT_USERS")) ||
                        (p.Modulo == "INVENTARIO" && p.Codigo.Contains("EXISTENCIA.VER")) ||
                        (p.Modulo == "RRHH" && p.Codigo.Contains("NOMINA.VER")))
            .ToList();

        await context.SaveChangesAsync();

        // ======================================================================
        // 6. USUARIOS
        // ======================================================================
        var masterUser = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            SucursalId = sucursalId,
            NombreUsuario = "master",
            NombreCompleto = "Usuario Maestro",
            Email = "master@tierraprometida.cu",
            HashPassword = authService.HashPassword("MasterVercom2026*"),
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
            Email = "admin@tierraprometida.cu",
            HashPassword = authService.HashPassword("admin123*"),
            Activo = true,
            DebeCambiarPass = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        // Asignar roles a los usuarios
        var adminRoleId = await context.Rols.Where(r => r.Codigo == "ADMINISTRADOR").Select(r => r.Id).FirstAsync();
        context.UsuarioRols.AddRange(new List<UsuarioRol>
        {
            new UsuarioRol { UsuarioId = masterUser.Id, RolId = adminRoleId, AsignadoEn = DateTimeOffset.UtcNow },
            new UsuarioRol { UsuarioId = admin.Id, RolId = adminRoleId, AsignadoEn = DateTimeOffset.UtcNow }
        });

        // ======================================================================
        // 7. TIPOS DE COMPROBANTE
        // ======================================================================
        var tiposComprobante = new List<TipoComprobante>
        {
            new TipoComprobante { Codigo = "AP", Nombre = "Apertura" },
            new TipoComprobante { Codigo = "DI", Nombre = "Diario" },
            new TipoComprobante { Codigo = "CI", Nombre = "Comprobante de Ingreso" },
            new TipoComprobante { Codigo = "CE", Nombre = "Comprobante de Egreso" },
            new TipoComprobante { Codigo = "VE", Nombre = "Ventas" },
            new TipoComprobante { Codigo = "CO", Nombre = "Compras" },
            new TipoComprobante { Codigo = "AJ", Nombre = "Ajustes" },
            new TipoComprobante { Codigo = "NO", Nombre = "Nómina" }
        };
        foreach (var tc in tiposComprobante)
        {
            if (!await context.TipoComprobantes.AnyAsync(t => t.Codigo == tc.Codigo))
                context.TipoComprobantes.Add(tc);
        }

        // ======================================================================
        // 8. CUENTAS CONTABLES
        // ======================================================================
        var cuentas = new List<CuentaContable>
        {
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "101", Nombre = "Efectivo en Caja", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "103", Nombre = "Efectivo en Banco", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "201", Nombre = "Cuentas por Cobrar", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "301", Nombre = "Inventario de Mercancías", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "401", Nombre = "Cuentas por Pagar", Clase = "PASIVO", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "501", Nombre = "Capital Social", Clase = "PATRIMONIO", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "601", Nombre = "Ingresos por Ventas", Clase = "INGRESO", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
            new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "701", Nombre = "Gastos de Operación", Clase = "GASTO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow }
        };
        foreach (var c in cuentas)
        {
            if (!await context.CuentaContables.AnyAsync(cc => cc.EntidadId == entidadId && cc.Codigo == c.Codigo))
                context.CuentaContables.Add(c);
        }

        // ======================================================================
        // 9. CONCEPTOS DE NÓMINA (CORREGIDO: "DEVENGO")
        // ======================================================================
        var conceptos = new List<ConceptoNomina>
        {
            new ConceptoNomina { Codigo = "SAL_BASE", Nombre = "Salario Básico", Tipo = "DEVENGO" },
            new ConceptoNomina { Codigo = "ESTIMULACION", Nombre = "Estimulación por Resultados", Tipo = "DEVENGO" },
            new ConceptoNomina { Codigo = "HORAS_EXTRA", Nombre = "Horas Extras", Tipo = "DEVENGO" },
            new ConceptoNomina { Codigo = "RET_SS_5", Nombre = "Contribución Especial Seg. Social (5%)", Tipo = "DEDUCCION", Formula = "DEVENGADO * 0.05" },
            new ConceptoNomina { Codigo = "SUBSIDIO_ENF", Nombre = "Subsidio Certificado Médico", Tipo = "DEVENGO" },
            new ConceptoNomina { Codigo = "VACACIONES", Nombre = "Vacaciones Pagadas", Tipo = "DEVENGO" }
        };
        foreach (var c in conceptos)
        {
            if (!await context.ConceptoNominas.AnyAsync(cn => cn.Codigo == c.Codigo))
                context.ConceptoNominas.Add(c);
        }

        // ======================================================================
        // 10. TIPOS DE MOVIMIENTO
        // ======================================================================
        var movimientos = new List<TipoMovimiento>
        {
            new TipoMovimiento { Codigo = "REC", Nombre = "Informe de Recepción", Naturaleza = "ENTRADA", AfectaCosto = true },
            new TipoMovimiento { Codigo = "TRA", Nombre = "Transferencia entre Almacenes", Naturaleza = "MIXTO", AfectaCosto = false },
            new TipoMovimiento { Codigo = "VEN", Nombre = "Salida por Venta", Naturaleza = "SALIDA", AfectaCosto = false },
            new TipoMovimiento { Codigo = "AJU", Nombre = "Ajuste de Inventario", Naturaleza = "MIXTO", AfectaCosto = true },
            new TipoMovimiento { Codigo = "DEV", Nombre = "Devolución de Compra", Naturaleza = "SALIDA", AfectaCosto = true },
            new TipoMovimiento { Codigo = "PRO", Nombre = "Consumo para Producción", Naturaleza = "SALIDA", AfectaCosto = false }
        };
        foreach (var m in movimientos)
        {
            if (!await context.TipoMovimientos.AnyAsync(tm => tm.Codigo == m.Codigo))
                context.TipoMovimientos.Add(m);
        }

        // ======================================================================
        // 11. ALMACÉN CENTRAL (CON SucursalId)
        // ======================================================================
        if (!await context.Almacens.AnyAsync(a => a.EntidadId == entidadId && a.Codigo == "ALM_01"))
        {
            context.Almacens.Add(new Almacen
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                SucursalId = sucursalId,
                Codigo = "ALM_01",
                Nombre = "Almacén Central",
                EsPuntoVenta = false,
                Activo = true
            });
        }

        // ======================================================================
        // 12. PARÁMETROS DEL SISTEMA (CORREGIDO: "NUMERIC")
        // ======================================================================
        var parametros = new List<ParametroSistema>
        {
            new ParametroSistema { EntidadId = entidadId, Codigo = "TAX_VENTA", Valor = "0.10", Descripcion = "Impuesto sobre ventas (10%)", TipoDato = "NUMERIC", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "GATEWAY_TRANSFERMOVIL", Valor = "ACTIVO", Descripcion = "Pasarela Transfermóvil habilitada", TipoDato = "BOOLEAN", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "GATEWAY_ENZONA", Valor = "ACTIVO", Descripcion = "Pasarela EnZona habilitada", TipoDato = "BOOLEAN", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) }
        };
        foreach (var p in parametros)
        {
            if (!await context.ParametroSistemas.AnyAsync(ps => ps.EntidadId == entidadId && ps.Codigo == p.Codigo))
                context.ParametroSistemas.Add(p);
        }

        // ======================================================================
        // 13. INDICADORES BI (CORREGIDO: CATEGORÍA)
        // ======================================================================
        var indicadores = new List<Indicador>
        {
            new Indicador { Codigo = "LIQUIDEZ", Nombre = "Índice de Liquidez", Categoria = "LIQUIDEZ", FormulaDescripcion = "Activo Corriente / Pasivo Corriente" },
            new Indicador { Codigo = "RENTABILIDAD", Nombre = "Margen de Utilidad Neta", Categoria = "RENTABILIDAD", FormulaDescripcion = "Utilidad Neta / Ventas Totales" },
            new Indicador { Codigo = "ROT_INV", Nombre = "Rotación de Inventario", Categoria = "OPERATIVO", FormulaDescripcion = "Costo de Ventas / Inventario Promedio" },
            new Indicador { Codigo = "CICLO_COBRO", Nombre = "Ciclo de Cobro (Días)", Categoria = "OPERATIVO", FormulaDescripcion = "Promedio CxC / Ventas Diarias" }
        };
        foreach (var i in indicadores)
        {
            if (!await context.Indicadors.AnyAsync(ind => ind.Codigo == i.Codigo))
                context.Indicadors.Add(i);
        }

        // ======================================================================
        // GUARDAR TODOS LOS CAMBIOS
        // ======================================================================
        await context.SaveChangesAsync();
    }
}