-- 111_permisos_pos.sql
-- RNF-POS: Completa la matriz de permisos del módulo POS/INTEGRACION.
--
-- Motivación (auditoría del panel POS):
--   1. Los controladores MVC (DispositivoPo, SesionCajaPo, PosVentaPendiente) protegen con las
--      políticas POS.CONFIGURACION.VER / .CREAR / .EDITAR, pero esas constantes NO existían como
--      permisos en nucleo.permiso -> el panel quedaba inaccesible para POS_SUPERVISOR/POS_ADMIN
--      (solo MASTER/ADMINISTRADOR pasaban por el PermissionHandler).
--   2. Los permisos POS.CAJA e INTEGRACION.POS.VER existían en la tabla, pero no estaban otorgados
--      a ningún rol POS.
--
-- Idempotente: solo inserta lo que no exista; puede ejecutarse repetidas veces.
SET NOCOUNT ON;

-- 1) Garantizar que existan los permisos del módulo (por codigo único).
INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
SELECT src.codigo, src.modulo, src.descripcion
FROM (VALUES
    ('POS.CONFIGURACION.VER',    'POS',         'Ver la configuración de terminales, sesiones de caja y ventas offline'),
    ('POS.CONFIGURACION.CREAR',  'POS',         'Crear/registrar terminales POS y procesar ventas pendientes'),
    ('POS.CONFIGURACION.EDITAR', 'POS',         'Editar la configuración de un terminal POS'),
    ('POS.CAJA',                 'INTEGRACION', 'Acceso a terminal de punto de venta'),
    ('INTEGRACION.POS.VER',      'INTEGRACION', 'Ver el módulo de integración POS')
) src(codigo, modulo, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM nucleo.permiso p WHERE p.codigo = src.codigo);

-- 2) Matriz rol -> permiso para los roles del POS (y el administrador de entidad).
--    El PermissionHandler da acceso total a MASTER y ADMINISTRADOR; aquí se reflejan las
--    asignaciones para que también apliquen si el handler cambia y queden consistente en BD.
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM (VALUES
    ('POS_ADMIN',       'POS.CONFIGURACION.VER'),
    ('POS_ADMIN',       'POS.CONFIGURACION.CREAR'),
    ('POS_ADMIN',       'POS.CONFIGURACION.EDITAR'),
    ('POS_ADMIN',       'POS.CAJA'),
    ('POS_ADMIN',       'INTEGRACION.POS.VER'),
    ('POS_SUPERVISOR',  'POS.CONFIGURACION.VER'),
    ('POS_SUPERVISOR',  'POS.CAJA'),
    ('POS_SUPERVISOR',  'INTEGRACION.POS.VER'),
    ('ADMINISTRADOR',   'POS.CONFIGURACION.VER'),
    ('ADMINISTRADOR',   'POS.CONFIGURACION.CREAR'),
    ('ADMINISTRADOR',   'POS.CONFIGURACION.EDITAR')
) m(rol_codigo, permiso_codigo)
JOIN nucleo.rol r     ON r.codigo = m.rol_codigo
JOIN nucleo.permiso p ON p.codigo = m.permiso_codigo
WHERE NOT EXISTS (
    SELECT 1 FROM nucleo.rol_permiso rp
    WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
);

-- 3) Verificación final.
SELECT p.codigo, p.modulo, COUNT(rp.permiso_id) AS roles_asignados
FROM nucleo.permiso p
LEFT JOIN nucleo.rol_permiso rp ON rp.permiso_id = p.id
WHERE p.codigo IN ('POS.CONFIGURACION.VER','POS.CONFIGURACION.CREAR','POS.CONFIGURACION.EDITAR','POS.CAJA','INTEGRACION.POS.VER')
GROUP BY p.codigo, p.modulo
ORDER BY p.codigo;

SELECT r.codigo AS rol, STRING_AGG(p.codigo, ', ') WITHIN GROUP (ORDER BY p.codigo) AS permisos_pos
FROM nucleo.rol_permiso rp
JOIN nucleo.rol r ON r.id = rp.rol_id
JOIN nucleo.permiso p ON p.id = rp.permiso_id
WHERE r.codigo IN ('POS_CAJERO','POS_SUPERVISOR','POS_ADMIN','ADMINISTRADOR')
  AND (p.codigo LIKE 'POS.%' OR p.codigo LIKE 'INTEGRACION.%' OR p.codigo LIKE 'POS%' OR p.codigo = 'configuracion' OR p.codigo = 'gestionar_usuarios')
GROUP BY r.codigo
ORDER BY r.codigo;