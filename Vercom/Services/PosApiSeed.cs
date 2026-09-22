using Microsoft.EntityFrameworkCore;
using Vercom.Models;

namespace Vercom.Services;

public static class PosApiSeed
{
    private const string Modulo = "POS";

    private static readonly (string Codigo, string Descripcion)[] Permisos =
    {
        ("vender", "Realizar ventas en el terminal POS"),
        ("consultar_productos", "Consultar el catálogo de productos"),
        ("ver_inventario", "Ver existencias e inventario"),
        ("gestionar_inventario", "Gestionar inventario desde el POS"),
        ("cerrar_caja", "Cerrar sesión de caja POS"),
        ("aplicar_descuento", "Aplicar descuentos a las ventas"),
        ("editar_producto", "Editar productos desde el POS"),
        ("config_general", "Configurar opciones generales del POS"),
        ("config_identidad", "Configurar identidad del negocio"),
        ("config_pagos", "Configurar medios de pago"),
        ("procesar_devolucion", "Procesar devoluciones"),
        ("gestionar_usuarios", "Administrar usuarios, roles y permisos"),
        ("configuracion", "Acceder a la configuración completa"),
        ("ver_reportes", "Ver reportes y estadísticas")
    };

    private static readonly (string Codigo, string Nombre, string Descripcion, string[] Permisos)[] Roles =
    {
        ("POS_CAJERO", "Cajero de Punto de Venta", "Rol estándar para operar el terminal POS",
            new[] { "vender", "consultar_productos", "ver_inventario", "cerrar_caja", "aplicar_descuento", "config_general", "config_pagos" }),
        ("POS_SUPERVISOR", "Supervisor de Punto de Venta", "Rol con control de inventario, devoluciones y reportes",
            new[] { "vender", "consultar_productos", "ver_inventario", "gestionar_inventario", "cerrar_caja", "aplicar_descuento", "editar_producto", "procesar_devolucion", "ver_reportes", "config_general", "config_pagos", "config_identidad" }),
        ("POS_ADMIN", "Administrador POS", "Rol con gestión de usuarios y configuración completa",
            new[] { "vender", "consultar_productos", "ver_inventario", "gestionar_inventario", "cerrar_caja", "aplicar_descuento", "editar_producto", "procesar_devolucion", "ver_reportes", "config_general", "config_identidad", "config_pagos", "gestionar_usuarios", "configuracion" })
    };

    public static async Task Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await context.Permisos.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.Modulo == Modulo)
            .Select(p => p.Codigo)
            .ToListAsync();
        var toAdd = Permisos.Where(p => !existing.Contains(p.Codigo)).ToList();
        if (toAdd.Count > 0)
        {
            context.Permisos.AddRange(toAdd.Select(p => new Permiso { Codigo = p.Codigo, Modulo = Modulo, Descripcion = p.Descripcion }));
            await context.SaveChangesAsync();
        }

        foreach (var (codigo, nombre, descripcion, permisos) in Roles)
        {
            var rol = await context.Rols.IgnoreQueryFilters()
                .Include(r => r.Permisos)
                .FirstOrDefaultAsync(r => r.Codigo == codigo);
            if (rol is null)
            {
                rol = new Rol { Codigo = codigo, Nombre = nombre, Descripcion = descripcion, EsSistema = false, EntidadId = null, CreadoEn = DateTimeOffset.UtcNow };
                context.Rols.Add(rol);
                await context.SaveChangesAsync();
            }
            var permisoEnts = await context.Permisos.IgnoreQueryFilters()
                .Where(p => permisos.Contains(p.Codigo))
                .ToListAsync();
            var existentesPermisoIds = rol.Permisos.Select(p => p.Id).ToHashSet();
            foreach (var permiso in permisoEnts)
            {
                if (!existentesPermisoIds.Contains(permiso.Id))
                    rol.Permisos.Add(permiso);
            }
            await context.SaveChangesAsync();
        }
    }
}