using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<AppDbContext>();

        var authService = serviceProvider.GetRequiredService<IAuthService>();

        // Si ya hay usuarios, asumimos que el seed ya se ejecutó
        if (await context.Usuarios.IgnoreQueryFilters().AnyAsync())
            return;
        // ======================================================================
        // 1. ENTIDAD (verificar existencia para evitar duplicados)
        // ======================================================================
        var entidad = await context.Entidads.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (entidad == null)
        {
            entidad = new Entidad
            {
                Id = Guid.NewGuid(),
                RazonSocial = "Sociedad Mercantil Tierra Prometida S.U.R.L.",
                Nit = "90000000000",
                FormaJuridica = "S.U.R.L.",
                DireccionLegal = "Calle Central #101, Matanzas",
                MonedaBase = "CUP",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow
            };
            context.Entidads.Add(entidad);
            await context.SaveChangesAsync();
        }
        var entidadId = entidad.Id;

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
        // 5. ROLES DE SISTEMA
        // ======================================================================
        var roles = new List<Rol>
        {
            new Rol { Codigo = "MASTER", Nombre = "Administrador Global (Ecosistema)", EsSistema = true },
            new Rol { Codigo = "ADMINISTRADOR", Nombre = "Administrador de Entidad", EsSistema = true },
            new Rol { Codigo = "CONTADOR", Nombre = "Contador de Entidad", EsSistema = true },
            new Rol { Codigo = "VENDEDOR", Nombre = "Vendedor", EsSistema = true }
        };

        foreach (var r in roles)
        {
            if (!await context.Rols.AnyAsync(x => x.Codigo == r.Codigo))
                context.Rols.Add(r);
        }
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
        context.Usuarios.Add(masterUser);
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        // Asignar roles a los usuarios
        var masterRoleId = await context.Rols.Where(r => r.Codigo == "MASTER").Select(r => r.Id).FirstAsync();
        var adminRoleId = await context.Rols.Where(r => r.Codigo == "ADMINISTRADOR").Select(r => r.Id).FirstAsync();

        context.UsuarioRols.AddRange(new List<UsuarioRol>
        {
            new UsuarioRol { UsuarioId = masterUser.Id, RolId = masterRoleId, SucursalId = sucursalId, AsignadoEn = DateTimeOffset.UtcNow },
            new UsuarioRol { UsuarioId = admin.Id, RolId = adminRoleId, SucursalId = sucursalId, AsignadoEn = DateTimeOffset.UtcNow }
        });

        // ======================================================================
        // 7. CARGOS POR DEFECTO
        // ======================================================================
        var cargos = new List<Cargo>
        {
            new Cargo { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "DIR-01", Nombre = "Director General", CategoriaOcupacional = "DIRIGENTE", Funciones = "Dirección y representación legal de la entidad.", SalarioEscalaMin = 8000, SalarioEscalaMax = 12000 },
            new Cargo { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "CONT-01", Nombre = "Especialista B en Contabilidad", CategoriaOcupacional = "TECNICO", Funciones = "Control económico y financiero de la entidad.", SalarioEscalaMin = 6500, SalarioEscalaMax = 8500 },
            new Cargo { Id = Guid.NewGuid(), EntidadId = entidadId, Codigo = "OPER-01", Nombre = "Operario Integral", CategoriaOcupacional = "OPERARIO", Funciones = "Labores de producción y servicios generales.", SalarioEscalaMin = 4500, SalarioEscalaMax = 6000 }
        };

        foreach (var c in cargos)
        {
            if (!await context.Cargos.AnyAsync(x => x.EntidadId == entidadId && x.Codigo == c.Codigo))
                context.Cargos.Add(c);
        }
        await context.SaveChangesAsync();


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
        // 14. PARÁMETROS FISCALES NÓMINA (NUEVO)
        // ======================================================================
        var parametros = new List<ParametroSistema>
        {
            new ParametroSistema { EntidadId = entidadId, Codigo = "TASA_SS_PATRONAL", Valor = "0.125", TipoDato = "NUMERIC", Descripcion = "Tasa de Contribución a la Seguridad Social (Entidad)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "TASA_FUERZA_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "Impuesto por la Utilización de la Fuerza de Trabajo", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "RET_SS_TRAB", Valor = "0.05", TipoDato = "NUMERIC", Descripcion = "Retención Seguridad Social Trabajador", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "FACTOR_VAC", Valor = "0.0909", TipoDato = "NUMERIC", Descripcion = "Factor Acumulación Vacaciones (9.09%)", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "IRP_MIN_EXENTO", Valor = "3260", TipoDato = "NUMERIC", Descripcion = "Mínimo Exento Impuesto sobre Ingresos Personales", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) },
            new ParametroSistema { EntidadId = entidadId, Codigo = "IRP_TASA", Valor = "0.03", TipoDato = "NUMERIC", Descripcion = "Tasa General Impuesto sobre Ingresos Personales", VigenteDesde = DateOnly.FromDateTime(DateTime.Now) }
        };

        foreach (var p in parametros)
        {
            if (!await context.ParametroSistemas.AnyAsync(ps => ps.EntidadId == entidadId && ps.Codigo == p.Codigo))
                context.ParametroSistemas.Add(p);
        }

        // ======================================================================
        // GUARDAR TODOS LOS CAMBIOS
        // ======================================================================
        await context.SaveChangesAsync();
    }
}