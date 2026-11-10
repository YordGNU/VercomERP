-- 114: NP-01 - Grants por rol (26 permisos nuevos de P1.3)
-- Inserta permisos idempotentemente y asigna a roles según lógica de negocio

SET NOCOUNT ON;

-- 1) Insertar 26 permisos (idempotente: NOT EXISTS en codigo)
INSERT INTO nucleo.permiso (codigo, descripcion, modulo)
SELECT v.codigo, v.descripcion, v.modulo FROM (VALUES
    ('COMERCIAL.CLIENTE.CREAR', 'Crear clientes', 'COMERCIAL'),
    ('COMERCIAL.CLIENTE.EDITAR', 'Editar clientes', 'COMERCIAL'),
    ('COMERCIAL.CONTRATO.CREAR', 'Crear contratos', 'COMERCIAL'),
    ('COMERCIAL.CONTRATO.EDITAR', 'Editar contratos', 'COMERCIAL'),
    ('COMERCIAL.PROVEEDOR.CREAR', 'Crear proveedores', 'COMERCIAL'),
    ('COMERCIAL.PROVEEDOR.EDITAR', 'Editar proveedores', 'COMERCIAL'),
    ('CONTABILIDAD.ACTIVO_FIJO.CREAR', 'Crear activos fijos', 'CONTABILIDAD'),
    ('CONTABILIDAD.ACTIVO_FIJO.EDITAR', 'Editar activos fijos', 'CONTABILIDAD'),
    ('CONTABILIDAD.ACTIVO_FIJO.ELIMINAR', 'Eliminar activos fijos', 'CONTABILIDAD'),
    ('CONTABILIDAD.CUENTA.ELIMINAR', 'Eliminar cuentas contables', 'CONTABILIDAD'),
    ('INVENTARIO.ALMACEN.CREAR', 'Crear almacenes', 'INVENTARIO'),
    ('INVENTARIO.FAMILIA.CREAR', 'Crear familias de producto', 'INVENTARIO'),
    ('INVENTARIO.FAMILIA.EDITAR', 'Editar familias de producto', 'INVENTARIO'),
    ('INVENTARIO.FAMILIA.ELIMINAR', 'Eliminar familias de producto', 'INVENTARIO'),
    ('INVENTARIO.FAMILIA.VER', 'Ver familias de producto', 'INVENTARIO'),
    ('INVENTARIO.LISTA_PRECIO.CREAR', 'Crear listas de precio', 'INVENTARIO'),
    ('INVENTARIO.LISTA_PRECIO.EDITAR', 'Editar listas de precio', 'INVENTARIO'),
    ('INVENTARIO.LISTA_PRECIO.VER', 'Ver listas de precio', 'INVENTARIO'),
    ('INVENTARIO.PRODUCTO.CREAR', 'Crear productos', 'INVENTARIO'),
    ('INVENTARIO.PRODUCTO.EDITAR', 'Editar productos', 'INVENTARIO'),
    ('INVENTARIO.PRODUCTO.ELIMINAR', 'Eliminar productos', 'INVENTARIO'),
    ('PRODUCCION.BOM.CREAR', 'Crear BOMs', 'PRODUCCION'),
    ('PRODUCCION.BOM.VER', 'Ver BOMs', 'PRODUCCION'),
    ('PRODUCCION.MANTENIMIENTO.CREAR', 'Crear mantenimientos', 'PRODUCCION'),
    ('PRODUCCION.MANTENIMIENTO.VER', 'Ver mantenimientos', 'PRODUCCION'),
    ('PRODUCCION.MERMA.VER', 'Ver merma', 'PRODUCCION'),
    ('RRHH.NOMINA.CREAR', 'Crear nómina', 'RRHH'),
    ('SEGURIDAD.ROL.ASIGNAR', 'Asignar roles', 'SEGURIDAD'),
    ('SEGURIDAD.USUARIO.EDITAR', 'Editar usuarios', 'SEGURIDAD')
) AS v(codigo, descripcion, modulo)
WHERE NOT EXISTS (SELECT 1 FROM nucleo.permiso p WHERE p.codigo = v.codigo);

-- 2) Asignar permisos a roles (idempotente)
-- Obtener IDs de roles (INT)
DECLARE @master INT = (SELECT id FROM nucleo.rol WHERE codigo = 'MASTER');
DECLARE @admin INT = (SELECT id FROM nucleo.rol WHERE codigo = 'ADMINISTRADOR');
DECLARE @contador INT = (SELECT id FROM nucleo.rol WHERE codigo = 'CONTADOR');
DECLARE @vendedor INT = (SELECT id FROM nucleo.rol WHERE codigo = 'VENDEDOR');
DECLARE @comprador INT = (SELECT id FROM nucleo.rol WHERE codigo = 'COMPRADOR');
DECLARE @gestorInv INT = (SELECT id FROM nucleo.rol WHERE codigo = 'GESTOR_INVENTARIO');
DECLARE @gestorProd INT = (SELECT id FROM nucleo.rol WHERE codigo = 'GESTOR_PRODUCCION');
DECLARE @gestorRRHH INT = (SELECT id FROM nucleo.rol WHERE codigo = 'GESTOR_RRHH');
DECLARE @posAdmin INT = (SELECT id FROM nucleo.rol WHERE codigo = 'POS_ADMIN');
DECLARE @posSuper INT = (SELECT id FROM nucleo.rol WHERE codigo = 'POS_SUPERVISOR');
DECLARE @posCajero INT = (SELECT id FROM nucleo.rol WHERE codigo = 'POS_CAJERO');

-- Mapeo permiso -> roles (INSERT idempotente via NOT EXISTS)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.rol_id, p.id FROM (VALUES
    -- COMERCIAL.CLIENTE -> ADMIN, VENDEDOR
    (@admin, 'COMERCIAL.CLIENTE.CREAR'), (@vendedor, 'COMERCIAL.CLIENTE.CREAR'),
    (@admin, 'COMERCIAL.CLIENTE.EDITAR'), (@vendedor, 'COMERCIAL.CLIENTE.EDITAR'),
    -- COMERCIAL.CONTRATO -> ADMIN, VENDEDOR
    (@admin, 'COMERCIAL.CONTRATO.CREAR'), (@vendedor, 'COMERCIAL.CONTRATO.CREAR'),
    (@admin, 'COMERCIAL.CONTRATO.EDITAR'), (@vendedor, 'COMERCIAL.CONTRATO.EDITAR'),
    -- COMERCIAL.PROVEEDOR -> ADMIN, COMPRADOR
    (@admin, 'COMERCIAL.PROVEEDOR.CREAR'), (@comprador, 'COMERCIAL.PROVEEDOR.CREAR'),
    (@admin, 'COMERCIAL.PROVEEDOR.EDITAR'), (@comprador, 'COMERCIAL.PROVEEDOR.EDITAR'),
    -- CONTABILIDAD.ACTIVO_FIJO -> ADMIN, CONTADOR
    (@admin, 'CONTABILIDAD.ACTIVO_FIJO.CREAR'), (@contador, 'CONTABILIDAD.ACTIVO_FIJO.CREAR'),
    (@admin, 'CONTABILIDAD.ACTIVO_FIJO.EDITAR'), (@contador, 'CONTABILIDAD.ACTIVO_FIJO.EDITAR'),
    (@admin, 'CONTABILIDAD.ACTIVO_FIJO.ELIMINAR'), (@contador, 'CONTABILIDAD.ACTIVO_FIJO.ELIMINAR'),
    -- CONTABILIDAD.CUENTA.ELIMINAR -> ADMIN, CONTADOR
    (@admin, 'CONTABILIDAD.CUENTA.ELIMINAR'), (@contador, 'CONTABILIDAD.CUENTA.ELIMINAR'),
    -- INVENTARIO.ALMACEN.CREAR -> ADMIN, GESTOR_INVENTARIO
    (@admin, 'INVENTARIO.ALMACEN.CREAR'), (@gestorInv, 'INVENTARIO.ALMACEN.CREAR'),
    -- INVENTARIO.FAMILIA.* -> ADMIN, GESTOR_INVENTARIO
    (@admin, 'INVENTARIO.FAMILIA.CREAR'), (@gestorInv, 'INVENTARIO.FAMILIA.CREAR'),
    (@admin, 'INVENTARIO.FAMILIA.EDITAR'), (@gestorInv, 'INVENTARIO.FAMILIA.EDITAR'),
    (@admin, 'INVENTARIO.FAMILIA.ELIMINAR'), (@gestorInv, 'INVENTARIO.FAMILIA.ELIMINAR'),
    (@admin, 'INVENTARIO.FAMILIA.VER'), (@gestorInv, 'INVENTARIO.FAMILIA.VER'),
    -- INVENTARIO.LISTA_PRECIO.* -> ADMIN, GESTOR_INVENTARIO, VENDEDOR
    (@admin, 'INVENTARIO.LISTA_PRECIO.CREAR'), (@gestorInv, 'INVENTARIO.LISTA_PRECIO.CREAR'), (@vendedor, 'INVENTARIO.LISTA_PRECIO.CREAR'),
    (@admin, 'INVENTARIO.LISTA_PRECIO.EDITAR'), (@gestorInv, 'INVENTARIO.LISTA_PRECIO.EDITAR'), (@vendedor, 'INVENTARIO.LISTA_PRECIO.EDITAR'),
    (@admin, 'INVENTARIO.LISTA_PRECIO.VER'), (@gestorInv, 'INVENTARIO.LISTA_PRECIO.VER'), (@vendedor, 'INVENTARIO.LISTA_PRECIO.VER'),
    -- INVENTARIO.PRODUCTO.* -> ADMIN, GESTOR_INVENTARIO
    (@admin, 'INVENTARIO.PRODUCTO.CREAR'), (@gestorInv, 'INVENTARIO.PRODUCTO.CREAR'),
    (@admin, 'INVENTARIO.PRODUCTO.EDITAR'), (@gestorInv, 'INVENTARIO.PRODUCTO.EDITAR'),
    (@admin, 'INVENTARIO.PRODUCTO.ELIMINAR'), (@gestorInv, 'INVENTARIO.PRODUCTO.ELIMINAR'),
    -- PRODUCCION.BOM.* -> ADMIN, GESTOR_PRODUCCION
    (@admin, 'PRODUCCION.BOM.CREAR'), (@gestorProd, 'PRODUCCION.BOM.CREAR'),
    (@admin, 'PRODUCCION.BOM.VER'), (@gestorProd, 'PRODUCCION.BOM.VER'),
    -- PRODUCCION.MANTENIMIENTO.* -> ADMIN, GESTOR_PRODUCCION
    (@admin, 'PRODUCCION.MANTENIMIENTO.CREAR'), (@gestorProd, 'PRODUCCION.MANTENIMIENTO.CREAR'),
    (@admin, 'PRODUCCION.MANTENIMIENTO.VER'), (@gestorProd, 'PRODUCCION.MANTENIMIENTO.VER'),
    -- PRODUCCION.MERMA.VER -> ADMIN, GESTOR_PRODUCCION, GESTOR_INVENTARIO
    (@admin, 'PRODUCCION.MERMA.VER'), (@gestorProd, 'PRODUCCION.MERMA.VER'), (@gestorInv, 'PRODUCCION.MERMA.VER'),
    -- RRHH.NOMINA.CREAR -> ADMIN, GESTOR_RRHH
    (@admin, 'RRHH.NOMINA.CREAR'), (@gestorRRHH, 'RRHH.NOMINA.CREAR'),
    -- SEGURIDAD -> ADMIN only
    (@admin, 'SEGURIDAD.ROL.ASIGNAR'),
    (@admin, 'SEGURIDAD.USUARIO.EDITAR')
) AS r(rol_id, permiso_codigo)
JOIN nucleo.permiso p ON p.codigo = r.permiso_codigo
WHERE NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.rol_id AND rp.permiso_id = p.id);

PRINT 'NP-01 completado: permisos insertados y asignados a roles.';