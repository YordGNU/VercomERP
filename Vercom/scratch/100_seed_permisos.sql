-- ================================================================
-- SEED DE PERMISOS, ROLES Y ASIGNACIONES
-- ERP Vercom - Tierra Prometida S.U.R.L.
-- ================================================================
-- INSTRUCCIONES:
--   1. Reemplazar @EntidadId con el ID real de la entidad
--   2. Ejecutar el script completo
--   3. Verificar con la consulta de verificación al final
-- ================================================================

DECLARE @EntidadId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM nucleo.entidad ORDER BY creado_en);

PRINT '=== Entidad ID: ' + CAST(@EntidadId AS NVARCHAR(50)) + ' ===';

-- ================================================================
-- 1. INSERTAR PERMISOS (son globales, no dependen de entidad)
-- ================================================================

-- ----------------------------------------------------------------
-- 1.1 NÚCLEO / SEGURIDAD
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('SEGURIDAD.USUARIO.VER',       'Seguridad', 'Ver listado de usuarios'),
    ('SEGURIDAD.USUARIO.CREAR',     'Seguridad', 'Crear nuevos usuarios'),
    ('SEGURIDAD.USUARIO.EDITAR',    'Seguridad', 'Editar usuarios existentes'),
    ('SEGURIDAD.USUARIO.ELIMINAR',  'Seguridad', 'Eliminar usuarios'),
    ('SEGURIDAD.ROL.VER',           'Seguridad', 'Ver roles y permisos'),
    ('SEGURIDAD.ROL.CREAR',         'Seguridad', 'Crear roles'),
    ('SEGURIDAD.ROL.EDITAR',        'Seguridad', 'Editar roles y asignar permisos'),
    ('SEGURIDAD.ROL.ELIMINAR',      'Seguridad', 'Eliminar roles'),
    ('SEGURIDAD.AUDITORIA.VER',     'Seguridad', 'Ver bitácora de auditoría'),
    ('SEGURIDAD.AUDITORIA.EXPORTAR','Seguridad', 'Exportar bitácora de auditoría');

-- ----------------------------------------------------------------
-- 1.2 ADMINISTRACIÓN
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'ADMIN.ENTIDAD.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('ADMIN.ENTIDAD.VER',      'Administración', 'Ver datos de la entidad'),
    ('ADMIN.ENTIDAD.EDITAR',   'Administración', 'Editar datos legales de la entidad'),
    ('ADMIN.SUCURSAL.VER',     'Administración', 'Ver sucursales'),
    ('ADMIN.SUCURSAL.CREAR',   'Administración', 'Crear sucursales'),
    ('ADMIN.SUCURSAL.EDITAR',  'Administración', 'Editar sucursales'),
    ('ADMIN.SUCURSAL.ELIMINAR','Administración', 'Eliminar sucursales'),
    ('ADMIN.BACKUP.VER',       'Administración', 'Ver historial de respaldos'),
    ('ADMIN.BACKUP.EJECUTAR',  'Administración', 'Ejecutar respaldo de base de datos'),
    ('ADMIN.BACKUP.RESTAURAR', 'Administración', 'Restaurar base de datos desde respaldo'),
    ('ADMIN.PARAMETROS.VER',   'Administración', 'Ver parámetros del sistema'),
    ('ADMIN.PARAMETROS.EDITAR','Administración', 'Editar parámetros del sistema');

-- ----------------------------------------------------------------
-- 1.3 CONTABILIDAD
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CUENTA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('CONTABILIDAD.CUENTA.VER',      'Contabilidad', 'Ver plan de cuentas'),
    ('CONTABILIDAD.CUENTA.CREAR',    'Contabilidad', 'Crear cuentas contables'),
    ('CONTABILIDAD.CUENTA.EDITAR',   'Contabilidad', 'Editar cuentas contables'),
    ('CONTABILIDAD.CUENTA.ELIMINAR', 'Contabilidad', 'Eliminar cuentas contables'),
    ('CONTABILIDAD.ASIENTO.VER',     'Contabilidad', 'Ver asientos contables'),
    ('CONTABILIDAD.ASIENTO.CREAR',   'Contabilidad', 'Crear asientos contables'),
    ('CONTABILIDAD.ASIENTO.REVERTIR','Contabilidad', 'Revertir asientos contabilizados'),
    ('CONTABILIDAD.PERIODO.VER',     'Contabilidad', 'Ver períodos contables'),
    ('CONTABILIDAD.PERIODO.CERRAR',  'Contabilidad', 'Cerrar períodos contables'),
    ('CONTABILIDAD.PERIODO.REABRIR', 'Contabilidad', 'Reabrir períodos cerrados'),
    ('CONTABILIDAD.ACTIVOS.VER',     'Contabilidad', 'Ver activos fijos'),
    ('CONTABILIDAD.ACTIVOS.CREAR',   'Contabilidad', 'Crear activos fijos'),
    ('CONTABILIDAD.ACTIVOS.EDITAR',  'Contabilidad', 'Editar activos fijos'),
    ('CONTABILIDAD.ACTIVOS.BAJA',    'Contabilidad', 'Dar de baja activos fijos'),
    ('CONTABILIDAD.ACTIVOS.DEPRECIAR','Contabilidad', 'Calcular y contabilizar depreciación'),
    ('CONTABILIDAD.CXC.VER',         'Contabilidad', 'Ver cuentas por cobrar'),
    ('CONTABILIDAD.CXC.COBRAR',      'Contabilidad', 'Registrar cobros'),
    ('CONTABILIDAD.CXP.VER',         'Contabilidad', 'Ver cuentas por pagar'),
    ('CONTABILIDAD.CXP.PAGAR',       'Contabilidad', 'Registrar pagos'),
    ('CONTABILIDAD.CAJA.VER',        'Contabilidad', 'Ver cajas'),
    ('CONTABILIDAD.CAJA.CREAR',      'Contabilidad', 'Crear cajas'),
    ('CONTABILIDAD.CAJA.EDITAR',     'Contabilidad', 'Editar cajas'),
    ('CONTABILIDAD.BANCO.VER',       'Contabilidad', 'Ver cuentas bancarias'),
    ('CONTABILIDAD.BANCO.CONCILIAR', 'Contabilidad', 'Conciliar movimientos bancarios'),
    ('CONTABILIDAD.ONAT.VER',        'Contabilidad', 'Ver declaraciones juradas ONAT'),
    ('CONTABILIDAD.ONAT.GENERAR',    'Contabilidad', 'Generar declaraciones juradas'),
    ('CONTABILIDAD.ONAT.PRESENTAR',  'Contabilidad', 'Marcar declaraciones como presentadas'),
    ('CONTABILIDAD.CIERRE.VER',      'Contabilidad', 'Ver cierre contable'),
    ('CONTABILIDAD.CIERRE.EJECUTAR', 'Contabilidad', 'Ejecutar cierre contable'),
    ('CONTABILIDAD.PRESUPUESTO.VER', 'Contabilidad', 'Ver presupuestos'),
    ('CONTABILIDAD.PRESUPUESTO.CREAR','Contabilidad','Crear presupuestos'),
    ('CONTABILIDAD.PRESUPUESTO.APROBAR','Contabilidad','Aprobar presupuestos');

-- ----------------------------------------------------------------
-- 1.4 INVENTARIO
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.PRODUCTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('INVENTARIO.PRODUCTO.VER',       'Inventario', 'Ver productos'),
    ('INVENTARIO.PRODUCTO.CREAR',     'Inventario', 'Crear productos'),
    ('INVENTARIO.PRODUCTO.EDITAR',    'Inventario', 'Editar productos'),
    ('INVENTARIO.PRODUCTO.ELIMINAR',  'Inventario', 'Eliminar productos'),
    ('INVENTARIO.EXISTENCIA.VER',     'Inventario', 'Ver existencias'),
    ('INVENTARIO.MOVIMIENTO.VER',     'Inventario', 'Ver movimientos de inventario'),
    ('INVENTARIO.MOVIMIENTO.CREAR',   'Inventario', 'Registrar movimientos de inventario'),
    ('INVENTARIO.CONTEO.VER',         'Inventario', 'Ver conteos físicos'),
    ('INVENTARIO.CONTEO.CREAR',       'Inventario', 'Crear conteos físicos'),
    ('INVENTARIO.CONTEO.CERRAR',      'Inventario', 'Cerrar y conciliar conteos físicos'),
    ('INVENTARIO.ALMACEN.VER',        'Inventario', 'Ver almacenes'),
    ('INVENTARIO.ALMACEN.CREAR',      'Inventario', 'Crear almacenes'),
    ('INVENTARIO.ALMACEN.EDITAR',     'Inventario', 'Editar almacenes'),
    ('INVENTARIO.ALERTAS.VER',        'Inventario', 'Ver alertas de stock bajo'),
    ('INVENTARIO.FAMILIA.VER',        'Inventario', 'Ver familias de productos'),
    ('INVENTARIO.FAMILIA.CREAR',      'Inventario', 'Crear familias de productos'),
    ('INVENTARIO.FAMILIA.EDITAR',     'Inventario', 'Editar familias de productos'),
    ('INVENTARIO.FAMILIA.ELIMINAR',   'Inventario', 'Eliminar familias de productos'),
    ('INVENTARIO.LISTA_PRECIO.VER',   'Inventario', 'Ver listas de precios'),
    ('INVENTARIO.LISTA_PRECIO.EDITAR','Inventario', 'Editar listas de precios');

-- ----------------------------------------------------------------
-- 1.5 COMERCIAL (Compras y Ventas)
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.FACTURA_VENTA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('COMERCIAL.FACTURA_VENTA.VER',      'Comercial', 'Ver facturas de venta'),
    ('COMERCIAL.FACTURA_VENTA.CREAR',    'Comercial', 'Crear facturas de venta'),
    ('COMERCIAL.FACTURA_VENTA.ANULAR',   'Comercial', 'Anular facturas de venta'),
    ('COMERCIAL.FACTURA_VENTA.EXPORTAR', 'Comercial', 'Exportar facturas de venta'),
    ('COMERCIAL.DEVOLUCION.VER',         'Comercial', 'Ver devoluciones de ventas'),
    ('COMERCIAL.DEVOLUCION.CREAR',       'Comercial', 'Registrar devoluciones de ventas'),
    ('COMERCIAL.DEVOLUCION.AUTORIZAR',   'Comercial', 'Autorizar devoluciones de ventas'),
    ('COMERCIAL.CLIENTE.VER',            'Comercial', 'Ver clientes'),
    ('COMERCIAL.CLIENTE.CREAR',          'Comercial', 'Crear clientes'),
    ('COMERCIAL.CLIENTE.EDITAR',         'Comercial', 'Editar clientes'),
    ('COMERCIAL.CLIENTE.ELIMINAR',       'Comercial', 'Eliminar clientes'),
    ('COMERCIAL.CLIENTE.CREDITO',        'Comercial', 'Gestionar créditos de clientes'),
    ('COMERCIAL.PROVEEDOR.VER',          'Comercial', 'Ver proveedores'),
    ('COMERCIAL.PROVEEDOR.CREAR',        'Comercial', 'Crear proveedores'),
    ('COMERCIAL.PROVEEDOR.EDITAR',       'Comercial', 'Editar proveedores'),
    ('COMERCIAL.PROVEEDOR.ELIMINAR',     'Comercial', 'Eliminar proveedores'),
    ('COMERCIAL.PROVEEDOR.CREDITO',      'Comercial', 'Gestionar créditos de proveedores'),
    ('COMERCIAL.ORDEN_COMPRA.VER',       'Comercial', 'Ver órdenes de compra'),
    ('COMERCIAL.ORDEN_COMPRA.CREAR',     'Comercial', 'Crear órdenes de compra'),
    ('COMERCIAL.ORDEN_COMPRA.APROBAR',   'Comercial', 'Aprobar órdenes de compra'),
    ('COMERCIAL.ORDEN_COMPRA.RECIBIR',   'Comercial', 'Registrar recepción de órdenes de compra'),
    ('COMERCIAL.CONTRATO.VER',           'Comercial', 'Ver contratos económicos'),
    ('COMERCIAL.CONTRATO.CREAR',         'Comercial', 'Crear contratos económicos'),
    ('COMERCIAL.CONTRATO.EDITAR',        'Comercial', 'Editar contratos económicos'),
    ('COMERCIAL.TOPE_PRECIO.VER',        'Comercial', 'Ver topes de precio MFP'),
    ('COMERCIAL.TOPE_PRECIO.EDITAR',     'Comercial', 'Gestionar topes de precio MFP');

-- ----------------------------------------------------------------
-- 1.6 RRHH
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.EMPLEADO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('RRHH.EMPLEADO.VER',         'RRHH', 'Ver expedientes de empleados'),
    ('RRHH.EMPLEADO.CREAR',       'RRHH', 'Crear empleados'),
    ('RRHH.EMPLEADO.EDITAR',      'RRHH', 'Editar empleados'),
    ('RRHH.EMPLEADO.ELIMINAR',    'RRHH', 'Dar de baja empleados'),
    ('RRHH.CONTRATO.VER',         'RRHH', 'Ver contratos laborales'),
    ('RRHH.CONTRATO.CREAR',       'RRHH', 'Crear contratos laborales'),
    ('RRHH.CONTRATO.EDITAR',      'RRHH', 'Editar contratos laborales'),
    ('RRHH.CONTRATO.FINALIZAR',   'RRHH', 'Finalizar contratos laborales'),
    ('RRHH.ASISTENCIA.VER',       'RRHH', 'Ver registros de asistencia'),
    ('RRHH.ASISTENCIA.REGISTRAR', 'RRHH', 'Registrar asistencia'),
    ('RRHH.ASISTENCIA.EDITAR',    'RRHH', 'Editar registros de asistencia'),
    ('RRHH.NOMINA.VER',           'RRHH', 'Ver nóminas'),
    ('RRHH.NOMINA.CALCULAR',      'RRHH', 'Calcular nóminas'),
    ('RRHH.NOMINA.APROBAR',       'RRHH', 'Aprobar nóminas'),
    ('RRHH.NOMINA.CONTABILIZAR',  'RRHH', 'Contabilizar nóminas'),
    ('RRHH.NOMINA.PAGAR',         'RRHH', 'Marcar nóminas como pagadas'),
    ('RRHH.VACACIONES.VER',       'RRHH', 'Ver saldos de vacaciones'),
    ('RRHH.VACACIONES.REGISTRAR', 'RRHH', 'Registrar disfrute de vacaciones'),
    ('RRHH.VACACIONES.COMPENSAR', 'RRHH', 'Compensar vacaciones'),
    ('RRHH.SC408.VER',            'RRHH', 'Ver modelo SC-4-08'),
    ('RRHH.SC408.EXPORTAR',       'RRHH', 'Exportar modelo SC-4-08'),
    ('RRHH.CERTIFICADO.VER',      'RRHH', 'Ver certificados médicos'),
    ('RRHH.CERTIFICADO.CREAR',    'RRHH', 'Crear certificados médicos'),
    ('RRHH.PLANTILLA.VER',        'RRHH', 'Ver plantilla aprobada'),
    ('RRHH.PLANTILLA.CREAR',      'RRHH', 'Crear plazas en plantilla'),
    ('RRHH.PLANTILLA.EDITAR',     'RRHH', 'Editar plazas en plantilla'),
    ('RRHH.PLANTILLA.ELIMINAR',   'RRHH', 'Eliminar plazas en plantilla'),
    ('RRHH.CARGO.VER',            'RRHH', 'Ver cargos'),
    ('RRHH.CARGO.CREAR',          'RRHH', 'Crear cargos'),
    ('RRHH.CARGO.EDITAR',         'RRHH', 'Editar cargos'),
    ('RRHH.UTIL.VER',             'RRHH', 'Ver medios y responsabilidades'),
    ('RRHH.UTIL.ENTREGAR',        'RRHH', 'Registrar entrega de medios'),
    ('RRHH.UTIL.DEVOLVER',        'RRHH', 'Registrar devolución de medios');

-- ----------------------------------------------------------------
-- 1.7 PRODUCCIÓN
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.FICHA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('PRODUCCION.FICHA.VER',         'Producción', 'Ver fichas de costo'),
    ('PRODUCCION.FICHA.CREAR',       'Producción', 'Crear fichas de costo'),
    ('PRODUCCION.FICHA.EDITAR',      'Producción', 'Editar fichas de costo'),
    ('PRODUCCION.FICHA.APROBAR',     'Producción', 'Aprobar fichas de costo'),
    ('PRODUCCION.BOM.VER',           'Producción', 'Ver listas de materiales (BOM)'),
    ('PRODUCCION.BOM.CREAR',         'Producción', 'Crear listas de materiales'),
    ('PRODUCCION.BOM.EDITAR',        'Producción', 'Editar listas de materiales'),
    ('PRODUCCION.ORDEN.VER',         'Producción', 'Ver órdenes de producción'),
    ('PRODUCCION.ORDEN.CREAR',       'Producción', 'Crear órdenes de producción'),
    ('PRODUCCION.ORDEN.INICIAR',     'Producción', 'Iniciar órdenes de producción'),
    ('PRODUCCION.ORDEN.TERMINAR',    'Producción', 'Terminar órdenes de producción'),
    ('PRODUCCION.ORDEN.CANCELAR',    'Producción', 'Cancelar órdenes de producción'),
    ('PRODUCCION.PLAN.VER',          'Producción', 'Ver planes de producción'),
    ('PRODUCCION.PLAN.CREAR',        'Producción', 'Crear planes de producción'),
    ('PRODUCCION.PLAN.APROBAR',      'Producción', 'Aprobar planes de producción'),
    ('PRODUCCION.MERMA.VER',         'Producción', 'Ver mermas y desperdicios'),
    ('PRODUCCION.MERMA.CREAR',       'Producción', 'Registrar mermas'),
    ('PRODUCCION.EQUIPO.VER',        'Producción', 'Ver equipos'),
    ('PRODUCCION.MANTENIMIENTO.VER', 'Producción', 'Ver mantenimientos programados'),
    ('PRODUCCION.MANTENIMIENTO.CREAR','Producción','Programar mantenimiento'),
    ('PRODUCCION.MANTENIMIENTO.EJECUTAR','Producción','Ejecutar mantenimiento');

-- ----------------------------------------------------------------
-- 1.8 POS / INTEGRACIÓN
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'POS.CAJA')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('POS.CAJA',                  'POS', 'Acceso a la pantalla de caja POS'),
    ('POS.SESION.ABRIR',          'POS', 'Abrir sesión de caja'),
    ('POS.SESION.CERRAR',         'POS', 'Cerrar sesión de caja'),
    ('POS.SESION.CONCILIAR',      'POS', 'Conciliar cierre de caja'),
    ('POS.VENTA.ANULAR',          'POS', 'Anular ventas del POS'),
    ('POS.MOVIMIENTO.CAJA',       'POS', 'Registrar movimientos de caja (retiros/ingresos)'),
    ('INTEGRACION.POS.VER',       'Integración', 'Ver dispositivos, sesiones y ventas offline'),
    ('INTEGRACION.POS.CREAR',     'Integración', 'Registrar dispositivos POS'),
    ('INTEGRACION.POS.EDITAR',    'Integración', 'Editar dispositivos POS'),
    ('INTEGRACION.API.VER',       'Integración', 'Ver clientes API'),
    ('INTEGRACION.API.CREAR',     'Integración', 'Crear clientes API'),
    ('INTEGRACION.API.REVOCAR',   'Integración', 'Revocar clientes API'),
    ('INTEGRACION.WEBHOOK.VER',   'Integración', 'Ver suscripciones webhook'),
    ('INTEGRACION.WEBHOOK.CREAR', 'Integración', 'Crear suscripciones webhook'),
    ('INTEGRACION.RANGO.VER',     'Integración', 'Ver rangos de numeración POS'),
    ('INTEGRACION.RANGO.ASIGNAR', 'Integración', 'Asignar rangos de numeración a dispositivos');

-- ----------------------------------------------------------------
-- 1.9 REPORTES
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.INDICADORES.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('REPORTES.INDICADORES.VER',   'Reportes', 'Ver dashboard de indicadores'),
    ('REPORTES.CONTABLES.VER',     'Reportes', 'Ver reportes contables (Balance, Resultados)'),
    ('REPORTES.CONTABLES.EXPORTAR','Reportes', 'Exportar reportes contables'),
    ('REPORTES.AUDITORIA.VER',     'Reportes', 'Ver trazas de auditoría'),
    ('REPORTES.AUDITORIA.EXPORTAR','Reportes', 'Exportar trazas de auditoría'),
    ('REPORTES.FISCALES.VER',      'Reportes', 'Ver reportes fiscales ONAT/MFP'),
    ('REPORTES.FISCALES.EXPORTAR', 'Reportes', 'Exportar reportes fiscales'),
    ('REPORTES.PERSONALIZADO.VER', 'Reportes', 'Generar reportes personalizados');

-- ================================================================
-- 2. INSERTAR ROLES (por entidad)
-- ================================================================

IF NOT EXISTS (SELECT 1 FROM nucleo.rol WHERE entidad_id = @EntidadId AND codigo = 'ADMIN')
    INSERT INTO nucleo.rol (codigo, nombre, descripcion, es_sistema, entidad_id) VALUES
    ('ADMIN',              'Administrador del Sistema',   'Acceso total al sistema. Gestiona usuarios, roles, parámetros y respaldos.', 1, @EntidadId),
    ('DIRECCION',          'Dirección',                    'Acceso de solo lectura a toda la información y aprobación de operaciones clave.', 1, @EntidadId),
    ('CONTADOR',           'Contador',                     'Gestión contable, fiscal, presupuestaria y cierre de períodos.', 1, @EntidadId),
    ('ECONOMICO',          'Económico',                    'Gestión de tesorería, cuentas por cobrar/pagar y conciliación bancaria.', 1, @EntidadId),
    ('JEFE_PRODUCCION',    'Jefe de Producción',           'Gestión de órdenes, fichas de costo, BOM y planificación de producción.', 1, @EntidadId),
    ('ALMACENERO',         'Almacenero',                   'Gestión de inventario, movimientos, conteos y recepción de compras.', 1, @EntidadId),
    ('RRHH',               'Recursos Humanos',             'Gestión de empleados, nóminas, asistencia, vacaciones y contratos.', 1, @EntidadId),
    ('COMERCIAL',          'Comercial / Ventas',           'Gestión de clientes, facturación, devoluciones y contratos económicos.', 1, @EntidadId),
    ('COMPRADOR',          'Comprador',                    'Gestión de proveedores y órdenes de compra.', 1, @EntidadId),
    ('CAJERO_POS',         'Cajero de Punto de Venta',     'Operación de terminal POS, apertura/cierre de caja y registro de ventas.', 1, @EntidadId),
    ('SUPERVISOR_POS',     'Supervisor de POS',            'Supervisión de cajas, anulación de ventas y conciliación de sesiones.', 1, @EntidadId),
    ('AUDITOR',            'Auditor',                      'Acceso de solo lectura a todas las trazas de auditoría y reportes.', 1, @EntidadId);

-- ================================================================
-- 3. ASIGNAR PERMISOS A ROLES
-- ================================================================

-- ----------------------------------------------------------------
-- 3.1 ADMIN: TODOS los permisos
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'ADMIN'
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.2 DIRECCION: Solo lectura en todo
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'DIRECCION'
  AND (p.codigo LIKE '%.VER' OR p.codigo IN ('POS.CAJA'))
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.3 CONTADOR
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'CONTADOR'
  AND p.codigo IN (
    -- Contabilidad
    'CONTABILIDAD.CUENTA.VER', 'CONTABILIDAD.CUENTA.CREAR', 'CONTABILIDAD.CUENTA.EDITAR',
    'CONTABILIDAD.ASIENTO.VER', 'CONTABILIDAD.ASIENTO.CREAR', 'CONTABILIDAD.ASIENTO.REVERTIR',
    'CONTABILIDAD.PERIODO.VER', 'CONTABILIDAD.PERIODO.CERRAR', 'CONTABILIDAD.PERIODO.REABRIR',
    'CONTABILIDAD.ACTIVOS.VER', 'CONTABILIDAD.ACTIVOS.CREAR', 'CONTABILIDAD.ACTIVOS.EDITAR',
    'CONTABILIDAD.ACTIVOS.BAJA', 'CONTABILIDAD.ACTIVOS.DEPRECIAR',
    'CONTABILIDAD.CXC.VER', 'CONTABILIDAD.CXC.COBRAR',
    'CONTABILIDAD.CXP.VER', 'CONTABILIDAD.CXP.PAGAR',
    'CONTABILIDAD.CAJA.VER', 'CONTABILIDAD.BANCO.VER', 'CONTABILIDAD.BANCO.CONCILIAR',
    'CONTABILIDAD.ONAT.VER', 'CONTABILIDAD.ONAT.GENERAR', 'CONTABILIDAD.ONAT.PRESENTAR',
    'CONTABILIDAD.CIERRE.VER', 'CONTABILIDAD.CIERRE.EJECUTAR',
    'CONTABILIDAD.PRESUPUESTO.VER', 'CONTABILIDAD.PRESUPUESTO.CREAR', 'CONTABILIDAD.PRESUPUESTO.APROBAR',
    -- Reportes
    'REPORTES.INDICADORES.VER', 'REPORTES.CONTABLES.VER', 'REPORTES.CONTABLES.EXPORTAR',
    'REPORTES.FISCALES.VER', 'REPORTES.FISCALES.EXPORTAR',
    'REPORTES.AUDITORIA.VER',
    -- Nómina (para revisar asientos)
    'RRHH.NOMINA.VER', 'RRHH.NOMINA.CONTABILIZAR'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.4 ECONOMICO
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'ECONOMICO'
  AND p.codigo IN (
    'CONTABILIDAD.CXC.VER', 'CONTABILIDAD.CXC.COBRAR',
    'CONTABILIDAD.CXP.VER', 'CONTABILIDAD.CXP.PAGAR',
    'CONTABILIDAD.CAJA.VER', 'CONTABILIDAD.CAJA.CREAR', 'CONTABILIDAD.CAJA.EDITAR',
    'CONTABILIDAD.BANCO.VER', 'CONTABILIDAD.BANCO.CONCILIAR',
    'CONTABILIDAD.CUENTA.VER',
    'CONTABILIDAD.ASIENTO.VER',
    'COMERCIAL.CLIENTE.VER', 'COMERCIAL.CLIENTE.CREDITO',
    'COMERCIAL.PROVEEDOR.VER', 'COMERCIAL.PROVEEDOR.CREDITO',
    'REPORTES.INDICADORES.VER', 'REPORTES.CONTABLES.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.5 JEFE_PRODUCCION
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'JEFE_PRODUCCION'
  AND p.codigo IN (
    'PRODUCCION.FICHA.VER', 'PRODUCCION.FICHA.CREAR', 'PRODUCCION.FICHA.EDITAR', 'PRODUCCION.FICHA.APROBAR',
    'PRODUCCION.BOM.VER', 'PRODUCCION.BOM.CREAR', 'PRODUCCION.BOM.EDITAR',
    'PRODUCCION.ORDEN.VER', 'PRODUCCION.ORDEN.CREAR', 'PRODUCCION.ORDEN.INICIAR',
    'PRODUCCION.ORDEN.TERMINAR', 'PRODUCCION.ORDEN.CANCELAR',
    'PRODUCCION.PLAN.VER', 'PRODUCCION.PLAN.CREAR', 'PRODUCCION.PLAN.APROBAR',
    'PRODUCCION.MERMA.VER', 'PRODUCCION.MERMA.CREAR',
    'PRODUCCION.EQUIPO.VER', 'PRODUCCION.MANTENIMIENTO.VER',
    'PRODUCCION.MANTENIMIENTO.CREAR', 'PRODUCCION.MANTENIMIENTO.EJECUTAR',
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.EXISTENCIA.VER', 'INVENTARIO.MOVIMIENTO.VER',
    'INVENTARIO.ALMACEN.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.6 ALMACENERO
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'ALMACENERO'
  AND p.codigo IN (
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.PRODUCTO.CREAR', 'INVENTARIO.PRODUCTO.EDITAR',
    'INVENTARIO.EXISTENCIA.VER',
    'INVENTARIO.MOVIMIENTO.VER', 'INVENTARIO.MOVIMIENTO.CREAR',
    'INVENTARIO.CONTEO.VER', 'INVENTARIO.CONTEO.CREAR', 'INVENTARIO.CONTEO.CERRAR',
    'INVENTARIO.ALMACEN.VER',
    'INVENTARIO.ALERTAS.VER',
    'INVENTARIO.FAMILIA.VER',
    'INVENTARIO.LISTA_PRECIO.VER',
    'COMERCIAL.ORDEN_COMPRA.VER', 'COMERCIAL.ORDEN_COMPRA.RECIBIR',
    'COMERCIAL.PROVEEDOR.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.7 RRHH
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'RRHH'
  AND p.codigo IN (
    'RRHH.EMPLEADO.VER', 'RRHH.EMPLEADO.CREAR', 'RRHH.EMPLEADO.EDITAR', 'RRHH.EMPLEADO.ELIMINAR',
    'RRHH.CONTRATO.VER', 'RRHH.CONTRATO.CREAR', 'RRHH.CONTRATO.EDITAR', 'RRHH.CONTRATO.FINALIZAR',
    'RRHH.ASISTENCIA.VER', 'RRHH.ASISTENCIA.REGISTRAR', 'RRHH.ASISTENCIA.EDITAR',
    'RRHH.NOMINA.VER', 'RRHH.NOMINA.CALCULAR', 'RRHH.NOMINA.APROBAR',
    'RRHH.VACACIONES.VER', 'RRHH.VACACIONES.REGISTRAR', 'RRHH.VACACIONES.COMPENSAR',
    'RRHH.SC408.VER', 'RRHH.SC408.EXPORTAR',
    'RRHH.CERTIFICADO.VER', 'RRHH.CERTIFICADO.CREAR',
    'RRHH.PLANTILLA.VER', 'RRHH.PLANTILLA.CREAR', 'RRHH.PLANTILLA.EDITAR', 'RRHH.PLANTILLA.ELIMINAR',
    'RRHH.CARGO.VER', 'RRHH.CARGO.CREAR', 'RRHH.CARGO.EDITAR',
    'RRHH.UTIL.VER', 'RRHH.UTIL.ENTREGAR', 'RRHH.UTIL.DEVOLVER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.8 COMERCIAL
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'COMERCIAL'
  AND p.codigo IN (
    'COMERCIAL.FACTURA_VENTA.VER', 'COMERCIAL.FACTURA_VENTA.CREAR', 'COMERCIAL.FACTURA_VENTA.EXPORTAR',
    'COMERCIAL.DEVOLUCION.VER', 'COMERCIAL.DEVOLUCION.CREAR',
    'COMERCIAL.CLIENTE.VER', 'COMERCIAL.CLIENTE.CREAR', 'COMERCIAL.CLIENTE.EDITAR',
    'COMERCIAL.CLIENTE.CREDITO',
    'COMERCIAL.CONTRATO.VER', 'COMERCIAL.CONTRATO.CREAR', 'COMERCIAL.CONTRATO.EDITAR',
    'COMERCIAL.TOPE_PRECIO.VER',
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.EXISTENCIA.VER', 'INVENTARIO.LISTA_PRECIO.VER',
    'REPORTES.INDICADORES.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.9 COMPRADOR
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'COMPRADOR'
  AND p.codigo IN (
    'COMERCIAL.PROVEEDOR.VER', 'COMERCIAL.PROVEEDOR.CREAR', 'COMERCIAL.PROVEEDOR.EDITAR',
    'COMERCIAL.ORDEN_COMPRA.VER', 'COMERCIAL.ORDEN_COMPRA.CREAR',
    'COMERCIAL.CONTRATO.VER', 'COMERCIAL.CONTRATO.CREAR',
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.EXISTENCIA.VER', 'INVENTARIO.ALERTAS.VER',
    'CONTABILIDAD.CXP.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.10 CAJERO_POS
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'CAJERO_POS'
  AND p.codigo IN (
    'POS.CAJA', 'POS.SESION.ABRIR', 'POS.SESION.CERRAR',
    'POS.MOVIMIENTO.CAJA',
    'COMERCIAL.FACTURA_VENTA.VER', 'COMERCIAL.FACTURA_VENTA.CREAR',
    'COMERCIAL.CLIENTE.VER',
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.EXISTENCIA.VER', 'INVENTARIO.LISTA_PRECIO.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.11 SUPERVISOR_POS
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'SUPERVISOR_POS'
  AND p.codigo IN (
    'POS.CAJA', 'POS.SESION.ABRIR', 'POS.SESION.CERRAR', 'POS.SESION.CONCILIAR',
    'POS.VENTA.ANULAR', 'POS.MOVIMIENTO.CAJA',
    'INTEGRACION.POS.VER',
    'COMERCIAL.FACTURA_VENTA.VER', 'COMERCIAL.FACTURA_VENTA.CREAR', 'COMERCIAL.FACTURA_VENTA.ANULAR',
    'COMERCIAL.DEVOLUCION.VER', 'COMERCIAL.DEVOLUCION.CREAR', 'COMERCIAL.DEVOLUCION.AUTORIZAR',
    'COMERCIAL.CLIENTE.VER',
    'INVENTARIO.PRODUCTO.VER', 'INVENTARIO.EXISTENCIA.VER',
    'REPORTES.INDICADORES.VER'
  )
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ----------------------------------------------------------------
-- 3.12 AUDITOR: Solo lectura de todo + auditoría
-- ----------------------------------------------------------------
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @EntidadId
  AND r.codigo = 'AUDITOR'
  AND (p.codigo LIKE '%.VER'
       OR p.codigo LIKE '%.EXPORTAR'
       OR p.codigo = 'SEGURIDAD.AUDITORIA.VER'
       OR p.codigo = 'REPORTES.AUDITORIA.VER')
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- ================================================================
-- 4. VERIFICACIÓN
-- ================================================================

PRINT '';
PRINT '=== RESUMEN DE PERMISOS INSERTADOS ===';
SELECT 
    modulo AS [Módulo],
    COUNT(*) AS [Total Permisos]
FROM nucleo.permiso
GROUP BY modulo
ORDER BY modulo;

PRINT '';
PRINT '=== RESUMEN DE ROLES CREADOS ===';
SELECT 
    codigo AS [Código],
    nombre AS [Rol],
    (SELECT COUNT(*) FROM nucleo.rol_permiso rp WHERE rp.rol_id = r.id) AS [Permisos Asignados]
FROM nucleo.rol r
WHERE r.entidad_id = @EntidadId
ORDER BY codigo;

PRINT '';
PRINT '=== PERMISOS SIN ASIGNAR A NINGÚN ROL ===';
SELECT p.codigo, p.modulo, p.descripcion
FROM nucleo.permiso p
WHERE NOT EXISTS (
    SELECT 1 FROM nucleo.rol_permiso rp
    JOIN nucleo.rol r ON r.id = rp.rol_id
    WHERE rp.permiso_id = p.id AND r.entidad_id = @EntidadId
)
ORDER BY p.modulo, p.codigo;