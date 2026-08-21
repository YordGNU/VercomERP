using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext();

        var authService = serviceProvider.GetRequiredService<IAuthService>();

        // Si ya hay usuarios, asumimos que el seed ya se ejecutó
        if (await context.Usuarios.AnyAsync())
            return;
        // ======================================================================
        // 1. ENTIDAD (verificar existencia para evitar duplicados)
        // ======================================================================
        var entidadId = Guid.NewGuid(); // valor por defecto
        var entidad = await context.Entidads.FirstOrDefaultAsync();
        entidadId = entidad?.Id ?? Guid.NewGuid();

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
        var adminRoleId = await context.Rols.Where(r => r.Codigo == "ADMIN").Select(r => r.Id).FirstAsync();
        context.UsuarioRols.AddRange(new List<UsuarioRol>
        {
            new UsuarioRol { UsuarioId = masterUser.Id, RolId = adminRoleId, AsignadoEn = DateTimeOffset.UtcNow },
            new UsuarioRol { UsuarioId = admin.Id, RolId = adminRoleId, AsignadoEn = DateTimeOffset.UtcNow }
        });      
      
        
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