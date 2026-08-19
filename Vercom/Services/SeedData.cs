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

        // Evitar ejecutar si ya hay usuarios
        if (context.Usuarios.Any())
            return;

        // 1. Crear Entidad inicial
        var entidadId = Guid.NewGuid();
        var entidad = new Entidad
        {
            Id = entidadId,
            RazonSocial = "Tierra Prometida S.U.R.L.",
            Nit = "12345678901",
            FormaJuridica = "S.U.R.L.",
            DireccionLegal = "Cuba",
            MonedaBase = "CUP",
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        context.Entidads.Add(entidad);

        // 2. Crear Roles y Permisos
        if (!await context.Permisos.AnyAsync())
        {
            var permisos = new List<Permiso>
            {
                // Núcleo y Seguridad
                new Permiso { Codigo = "SEC_VIEW_USERS", Modulo = "SEGURIDAD", Descripcion = "Ver listado de usuarios" },
                new Permiso { Codigo = "SEC_EDIT_USERS", Modulo = "SEGURIDAD", Descripcion = "Crear y editar usuarios" },
                new Permiso { Codigo = "SEC_VIEW_ROLES", Modulo = "SEGURIDAD", Descripcion = "Ver y gestionar roles/permisos" },
                new Permiso { Codigo = "SEC_VIEW_AUDIT", Modulo = "SEGURIDAD", Descripcion = "Consultar bitácora de auditoría" },
                
                // Contabilidad
                new Permiso { Codigo = "ACC_VIEW_PLAN", Modulo = "CONTABILIDAD", Descripcion = "Ver plan de cuentas" },
                new Permiso { Codigo = "ACC_EDIT_PLAN", Modulo = "CONTABILIDAD", Descripcion = "Modificar cuentas contables" },
                new Permiso { Codigo = "ACC_CREATE_ENTRY", Modulo = "CONTABILIDAD", Descripcion = "Crear asientos contables" },
                new Permiso { Codigo = "ACC_POST_ENTRY", Modulo = "CONTABILIDAD", Descripcion = "Contabilizar asientos (Firme)" },
                new Permiso { Codigo = "ACC_CLOSE_PERIOD", Modulo = "CONTABILIDAD", Descripcion = "Cerrar periodos contables" },
                
                // Inventario
                new Permiso { Codigo = "INV_VIEW_STOCK", Modulo = "INVENTARIO", Descripcion = "Ver existencias y almacenes" },
                new Permiso { Codigo = "INV_EDIT_PROD", Modulo = "INVENTARIO", Descripcion = "Gestionar catálogo de productos" },
                new Permiso { Codigo = "INV_MOVEMENT", Modulo = "INVENTARIO", Descripcion = "Registrar movimientos de almacén" },
                
                // Comercial (Ventas/Compras)
                new Permiso { Codigo = "COM_SALE_FISCAL", Modulo = "COMERCIAL", Descripcion = "Emitir facturas de venta" },
                new Permiso { Codigo = "COM_PURCHASE_ORDER", Modulo = "COMERCIAL", Descripcion = "Gestionar órdenes de compra" },
                
                // RRHH
                new Permiso { Codigo = "HR_VIEW_STAFF", Modulo = "RRHH", Descripcion = "Ver expedientes de empleados" },
                new Permiso { Codigo = "HR_PAYROLL_RUN", Modulo = "RRHH", Descripcion = "Procesar y aprobar nómina" }
            };
            context.Permisos.AddRange(permisos);
            await context.SaveChangesAsync();
        }

        if (!await context.Rols.AnyAsync())
        {
            var adminRole = new Rol { Codigo = "ADMINISTRADOR", Nombre = "Administrador Total", EsSistema = true, CreadoEn = DateTimeOffset.Now };
            var accountantRole = new Rol { Codigo = "CONTADOR", Nombre = "Contabilidad General", EsSistema = true, CreadoEn = DateTimeOffset.Now };

            context.Rols.AddRange(adminRole, accountantRole);
            await context.SaveChangesAsync();

            // Asignar TODOS los permisos al administrador
            var todosLosPermisos = await context.Permisos.ToListAsync();
            foreach (var p in todosLosPermisos) { adminRole.Permisos.Add(p); }

            // Asignar permisos contables al contador
            var permisosContables = todosLosPermisos.Where(p => p.Modulo == "CONTABILIDAD" || p.Modulo == "SEGURIDAD" && p.Codigo.Contains("VIEW")).ToList();
            foreach (var p in permisosContables) { accountantRole.Permisos.Add(p); }

            await context.SaveChangesAsync();
        }

        // 3. Crear Usuarios Iniciales
        var masterUser = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            NombreUsuario = "master",
            NombreCompleto = "Usuario Maestro",
            HashPassword = authService.HashPassword("MasterVercom2026*"),
            Activo = true,
            DebeCambiarPass = false,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        context.Usuarios.Add(masterUser);

        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            EntidadId = entidadId,
            NombreUsuario = "admin",
            NombreCompleto = "Administrador Local",
            HashPassword = authService.HashPassword("admin123*"),
            Activo = true,
            DebeCambiarPass = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow
        };
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        // 4. Asignar Roles a Usuarios (usar el rol "ADMINISTRADOR" recién creado)
        var adminRole = await context.Rols.FirstAsync(r => r.Codigo == "ADMINISTRADOR");
        context.UsuarioRols.AddRange(new List<UsuarioRol>
        {
            new UsuarioRol { UsuarioId = masterUser.Id, RolId = adminRole.Id, AsignadoEn = DateTimeOffset.UtcNow },
            new UsuarioRol { UsuarioId = admin.Id, RolId = adminRole.Id, AsignadoEn = DateTimeOffset.UtcNow }
        });

        // 5. Tipos de Comprobante Contable (sin Id)
        if (!context.TipoComprobantes.Any())
        {
            context.TipoComprobantes.AddRange(new List<TipoComprobante>
            {
                new TipoComprobante { Codigo = "AP", Nombre = "Apertura" },
                new TipoComprobante { Codigo = "DI", Nombre = "Diario" },
                new TipoComprobante { Codigo = "CI", Nombre = "Comprobante de Ingreso" },
                new TipoComprobante { Codigo = "CE", Nombre = "Comprobante de Egreso" },
                new TipoComprobante { Codigo = "VE", Nombre = "Ventas" },
                new TipoComprobante { Codigo = "CO", Nombre = "Compras" },
                new TipoComprobante { Codigo = "AJ", Nombre = "Ajustes" },
                new TipoComprobante { Codigo = "NO", Nombre = "Nómina" }
            });
        }

        // 6. Cuentas Contables
        if (!context.CuentaContables.Any())
        {
            var cuentas = new List<CuentaContable>
            {
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "101", Nombre = "Efectivo en Caja", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "103", Nombre = "Efectivo en Banco", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "201", Nombre = "Cuentas por Cobrar", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "301", Nombre = "Inventario de Mercancías", Clase = "ACTIVO", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "401", Nombre = "Cuentas por Pagar", Clase = "PASIVO", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "501", Nombre = "Capital Social", Clase = "PATRIMONIO", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "601", Nombre = "Ingresos por Ventas", Clase = "INGRESOS", Naturaleza = "ACREEDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow },
                new CuentaContable { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "701", Nombre = "Gastos de Operación", Clase = "GASTOS", Naturaleza = "DEUDORA", Nivel = 1, AceptaMovimiento = true, Moneda = "CUP", Activo = true, CreadoEn = DateTimeOffset.UtcNow }
            };
            context.CuentaContables.AddRange(cuentas);
        }

        // 7. Conceptos de Nómina (sin Id)
        if (!context.ConceptoNominas.Any())
        {
            context.ConceptoNominas.AddRange(new List<ConceptoNomina>
            {
                new ConceptoNomina { Codigo = "SAL_BASE", Nombre = "Salario Básico", Tipo = "DEVENGADO" },
                new ConceptoNomina { Codigo = "ESTIMULACION", Nombre = "Estimulación por Resultados", Tipo = "DEVENGADO" },
                new ConceptoNomina { Codigo = "HORAS_EXTRA", Nombre = "Horas Extras", Tipo = "DEVENGADO" },
                new ConceptoNomina { Codigo = "RET_SS_5", Nombre = "Contribución Especial Seg. Social (5%)", Tipo = "DEDUCCION", Formula = "DEVENGADO * 0.05" },
                new ConceptoNomina { Codigo = "SUBSIDIO_ENF", Nombre = "Subsidio Certificado Médico", Tipo = "DEVENGADO" },
                new ConceptoNomina { Codigo = "VACACIONES", Nombre = "Vacaciones Pagadas", Tipo = "DEVENGADO" }
            });
        }

        // 8. Tipos de Movimiento de Inventario (sin Id)
        if (!context.TipoMovimientos.Any())
        {
            context.TipoMovimientos.AddRange(new List<TipoMovimiento>
            {
                new TipoMovimiento { Codigo = "REC", Nombre = "Informe de Recepción", Naturaleza = "ENTRADA", AfectaCosto = true },
                new TipoMovimiento { Codigo = "TRA", Nombre = "Transferencia entre Almacenes", Naturaleza = "MIXTO", AfectaCosto = false },
                new TipoMovimiento { Codigo = "VEN", Nombre = "Salida por Venta", Naturaleza = "SALIDA", AfectaCosto = false },
                new TipoMovimiento { Codigo = "AJU", Nombre = "Ajuste de Inventario", Naturaleza = "MIXTO", AfectaCosto = true },
                new TipoMovimiento { Codigo = "DEV", Nombre = "Devolución de Compra", Naturaleza = "SALIDA", AfectaCosto = true },
                new TipoMovimiento { Codigo = "PRO", Nombre = "Consumo para Producción", Naturaleza = "SALIDA", AfectaCosto = false }
            });
        }

        // 9. Almacén Central
        if (!context.Almacens.Any())
        {
            context.Almacens.Add(new Almacen
            {
                Id = Guid.NewGuid(),
                EntidadId = entidadId,
                Codigo = "ALM_01",
                Nombre = "Almacén Central",
                EsPuntoVenta = false,
                Activo = true
            });
        }

        // 10. Parámetros del Sistema (sin Id)
        if (!context.ParametroSistemas.Any())
        {
            context.ParametroSistemas.AddRange(new List<ParametroSistema>
            {
                new ParametroSistema { EntidadId = entidadId, Codigo = "TAX_VENTA", Valor = "0.10", Descripcion = "Impuesto sobre ventas (10%)", TipoDato = "DECIMAL", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) },
                new ParametroSistema { EntidadId = entidadId, Codigo = "GATEWAY_TRANSFERMOVIL", Valor = "ACTIVO", Descripcion = "Pasarela Transfermóvil habilitada", TipoDato = "BOOLEAN", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) },
                new ParametroSistema { EntidadId = entidadId, Codigo = "GATEWAY_ENZONA", Valor = "ACTIVO", Descripcion = "Pasarela EnZona habilitada", TipoDato = "BOOLEAN", VigenteDesde = DateOnly.FromDateTime(DateTime.UtcNow) }
            });
        }

        // 11. Indicadores BI (sin Id)
        if (!context.Indicadors.Any())
        {
            context.Indicadors.AddRange(new List<Indicador>
            {
                new Indicador { Codigo = "LIQUIDEZ", Nombre = "Índice de Liquidez", Categoria = "FINANCIERO", FormulaDescripcion = "Activo Corriente / Pasivo Corriente" },
                new Indicador { Codigo = "RENTABILIDAD", Nombre = "Margen de Utilidad Neta", Categoria = "FINANCIERO", FormulaDescripcion = "Utilidad Neta / Ventas Totales" },
                new Indicador { Codigo = "ROT_INV", Nombre = "Rotación de Inventario", Categoria = "OPERATIVO", FormulaDescripcion = "Costo de Ventas / Inventario Promedio" },
                new Indicador { Codigo = "CICLO_COBRO", Nombre = "Ciclo de Cobro (Días)", Categoria = "OPERATIVO", FormulaDescripcion = "Promedio CxC / Ventas Diarias" }
            });
        }

        await context.SaveChangesAsync();
    }
}