-- ============================================================================
-- SCRIPT DE PERMISOS Y ASIGNACIÓN A ROLES
-- ERP Sociedad Mercantil Tierra Prometida S.U.R.L.
-- Ejecutar después de tener las tablas nucleo.permiso y nucleo.rol_permiso
-- ============================================================================

USE [VercomERP]; -- Cambia por tu base de datos
GO

-- ============================================================================
-- 1. INSERCIÓN DE PERMISOS (solo los que no existan)
-- ============================================================================
-- Módulo: SEGURIDAD (Núcleo)
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.USUARIO.VER', 'SEGURIDAD', 'Ver listado de usuarios');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.USUARIO.CREAR', 'SEGURIDAD', 'Crear nuevos usuarios');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.USUARIO.EDITAR', 'SEGURIDAD', 'Editar usuarios existentes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.ELIMINAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.USUARIO.ELIMINAR', 'SEGURIDAD', 'Eliminar usuarios (baja lógica)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.ROL.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.ROL.VER', 'SEGURIDAD', 'Ver roles del sistema');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.ROL.ASIGNAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.ROL.ASIGNAR', 'SEGURIDAD', 'Asignar roles a usuarios');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.AUDITORIA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.AUDITORIA.VER', 'SEGURIDAD', 'Consultar bitácora de auditoría');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.PARAMETRO.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.PARAMETRO.EDITAR', 'SEGURIDAD', 'Editar parámetros del sistema');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.CONSECUTIVO.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('SEGURIDAD.CONSECUTIVO.EDITAR', 'SEGURIDAD', 'Gestionar numeradores consecutivos');

-- Módulo: CONTABILIDAD
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CUENTA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CUENTA.VER', 'CONTABILIDAD', 'Ver plan de cuentas');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CUENTA.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CUENTA.CREAR', 'CONTABILIDAD', 'Crear cuentas contables');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CUENTA.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CUENTA.EDITAR', 'CONTABILIDAD', 'Editar cuentas contables');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CUENTA.ELIMINAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CUENTA.ELIMINAR', 'CONTABILIDAD', 'Eliminar cuentas contables');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ASIENTO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ASIENTO.CREAR', 'CONTABILIDAD', 'Crear asientos contables');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ASIENTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ASIENTO.VER', 'CONTABILIDAD', 'Ver asientos contables');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ASIENTO.REVERTIR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ASIENTO.REVERTIR', 'CONTABILIDAD', 'Reversión de asientos (RF-12)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.PERIODO.ABRIR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.PERIODO.ABRIR', 'CONTABILIDAD', 'Abrir período contable');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.PERIODO.CERRAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.PERIODO.CERRAR', 'CONTABILIDAD', 'Cerrar período contable');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.BALANCE.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.BALANCE.VER', 'CONTABILIDAD', 'Consultar balances y estados financieros');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CXC.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CXC.VER', 'CONTABILIDAD', 'Ver cuentas por cobrar');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.CXP.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.CXP.VER', 'CONTABILIDAD', 'Ver cuentas por pagar');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.PAGO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.PAGO.CREAR', 'CONTABILIDAD', 'Registrar cobros/pagos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.PRESUPUESTO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.PRESUPUESTO.CREAR', 'CONTABILIDAD', 'Crear presupuestos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.PRESUPUESTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.PRESUPUESTO.VER', 'CONTABILIDAD', 'Ver presupuestos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.DECLARACION.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.DECLARACION.CREAR', 'CONTABILIDAD', 'Crear declaraciones juradas (ONAT)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.DECLARACION.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.DECLARACION.VER', 'CONTABILIDAD', 'Ver declaraciones juradas');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ACTIVO_FIJO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ACTIVO_FIJO.CREAR', 'CONTABILIDAD', 'Crear activos fijos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ACTIVO_FIJO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ACTIVO_FIJO.VER', 'CONTABILIDAD', 'Ver activos fijos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONTABILIDAD.ACTIVO_FIJO.DEPRECIAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('CONTABILIDAD.ACTIVO_FIJO.DEPRECIAR', 'CONTABILIDAD', 'Calcular y contabilizar depreciación');

-- Módulo: RRHH
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.EMPLEADO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.EMPLEADO.VER', 'RRHH', 'Ver expedientes de empleados');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.EMPLEADO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.EMPLEADO.CREAR', 'RRHH', 'Crear empleados');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.EMPLEADO.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.EMPLEADO.EDITAR', 'RRHH', 'Editar empleados');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.EMPLEADO.ELIMINAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.EMPLEADO.ELIMINAR', 'RRHH', 'Eliminar empleados (baja lógica)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.CARGO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.CARGO.VER', 'RRHH', 'Ver catálogo de cargos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.CARGO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.CARGO.CREAR', 'RRHH', 'Crear cargos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.CONTRATO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.CONTRATO.VER', 'RRHH', 'Ver contratos laborales');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.ASISTENCIA.REGISTRAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.ASISTENCIA.REGISTRAR', 'RRHH', 'Registrar asistencia diaria');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.ASISTENCIA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.ASISTENCIA.VER', 'RRHH', 'Ver registros de asistencia');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.NOMINA.CALCULAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.NOMINA.CALCULAR', 'RRHH', 'Calcular nómina');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.NOMINA.APROBAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.NOMINA.APROBAR', 'RRHH', 'Aprobar nómina');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.NOMINA.CONTABILIZAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.NOMINA.CONTABILIZAR', 'RRHH', 'Contabilizar nómina (generar asiento)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.NOMINA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.NOMINA.VER', 'RRHH', 'Ver nóminas calculadas');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.VACACIONES.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.VACACIONES.VER', 'RRHH', 'Ver saldos de vacaciones');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'RRHH.REPORTE.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('RRHH.REPORTE.VER', 'RRHH', 'Ver reportes de RRHH');

-- Módulo: INVENTARIO
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.PRODUCTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.PRODUCTO.VER', 'INVENTARIO', 'Ver productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.PRODUCTO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.PRODUCTO.CREAR', 'INVENTARIO', 'Crear productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.PRODUCTO.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.PRODUCTO.EDITAR', 'INVENTARIO', 'Editar productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.PRODUCTO.ELIMINAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.PRODUCTO.ELIMINAR', 'INVENTARIO', 'Eliminar productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.FAMILIA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.FAMILIA.VER', 'INVENTARIO', 'Ver familias de productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.FAMILIA.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.FAMILIA.CREAR', 'INVENTARIO', 'Crear familias de productos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.ALMACEN.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.ALMACEN.VER', 'INVENTARIO', 'Ver almacenes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.ALMACEN.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.ALMACEN.CREAR', 'INVENTARIO', 'Crear almacenes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.EXISTENCIA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.EXISTENCIA.VER', 'INVENTARIO', 'Consultar existencias');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.MOVIMIENTO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.MOVIMIENTO.CREAR', 'INVENTARIO', 'Crear movimientos de inventario');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.MOVIMIENTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.MOVIMIENTO.VER', 'INVENTARIO', 'Ver movimientos de inventario');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.CONTEO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.CONTEO.CREAR', 'INVENTARIO', 'Realizar conteos físicos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.CONTEO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.CONTEO.VER', 'INVENTARIO', 'Ver conteos físicos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.LISTA_PRECIO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.LISTA_PRECIO.CREAR', 'INVENTARIO', 'Crear listas de precios');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INVENTARIO.LISTA_PRECIO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INVENTARIO.LISTA_PRECIO.VER', 'INVENTARIO', 'Ver listas de precios');

-- Módulo: PRODUCCIÓN
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.FICHA.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.FICHA.CREAR', 'PRODUCCION', 'Crear fichas de costo');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.FICHA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.FICHA.VER', 'PRODUCCION', 'Ver fichas de costo');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.BOM.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.BOM.CREAR', 'PRODUCCION', 'Crear lista de materiales (BOM)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.BOM.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.BOM.VER', 'PRODUCCION', 'Ver lista de materiales');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.ORDEN.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.ORDEN.CREAR', 'PRODUCCION', 'Crear órdenes de producción');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.ORDEN.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.ORDEN.VER', 'PRODUCCION', 'Ver órdenes de producción');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.ORDEN.EJECUTAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.ORDEN.EJECUTAR', 'PRODUCCION', 'Ejecutar órdenes de producción (avanzar estado)');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.PLAN.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.PLAN.CREAR', 'PRODUCCION', 'Crear planes de producción');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.PLAN.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.PLAN.VER', 'PRODUCCION', 'Ver planes de producción');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.MERMA.REGISTRAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.MERMA.REGISTRAR', 'PRODUCCION', 'Registrar mermas y desperdicios');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.MANTENIMIENTO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.MANTENIMIENTO.CREAR', 'PRODUCCION', 'Crear mantenimiento de equipos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'PRODUCCION.MANTENIMIENTO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('PRODUCCION.MANTENIMIENTO.VER', 'PRODUCCION', 'Ver mantenimientos programados');

-- Módulo: COMERCIAL
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.PROVEEDOR.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.PROVEEDOR.CREAR', 'COMERCIAL', 'Crear proveedores');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.PROVEEDOR.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.PROVEEDOR.EDITAR', 'COMERCIAL', 'Editar proveedores');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.PROVEEDOR.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.PROVEEDOR.VER', 'COMERCIAL', 'Ver proveedores');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.CLIENTE.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.CLIENTE.CREAR', 'COMERCIAL', 'Crear clientes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.CLIENTE.EDITAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.CLIENTE.EDITAR', 'COMERCIAL', 'Editar clientes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.CLIENTE.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.CLIENTE.VER', 'COMERCIAL', 'Ver clientes');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.CONTRATO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.CONTRATO.CREAR', 'COMERCIAL', 'Crear contratos económicos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.CONTRATO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.CONTRATO.VER', 'COMERCIAL', 'Ver contratos económicos');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.ORDEN_COMPRA.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.ORDEN_COMPRA.CREAR', 'COMERCIAL', 'Crear órdenes de compra');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.ORDEN_COMPRA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.ORDEN_COMPRA.VER', 'COMERCIAL', 'Ver órdenes de compra');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.ORDEN_COMPRA.APROBAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.ORDEN_COMPRA.APROBAR', 'COMERCIAL', 'Aprobar órdenes de compra');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.FACTURA_VENTA.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.FACTURA_VENTA.CREAR', 'COMERCIAL', 'Crear facturas de venta');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.FACTURA_VENTA.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.FACTURA_VENTA.VER', 'COMERCIAL', 'Ver facturas de venta');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.FACTURA_VENTA.ANULAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.FACTURA_VENTA.ANULAR', 'COMERCIAL', 'Anular facturas de venta');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.DEVOLUCION.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.DEVOLUCION.CREAR', 'COMERCIAL', 'Crear devoluciones de venta');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'COMERCIAL.TOPE_PRECIO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('COMERCIAL.TOPE_PRECIO.VER', 'COMERCIAL', 'Ver topes de precio MFP');

-- Módulo: REPORTES
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.INDICADOR.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('REPORTES.INDICADOR.VER', 'REPORTES', 'Ver indicadores de gestión');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.PAQUETE.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('REPORTES.PAQUETE.CREAR', 'REPORTES', 'Generar paquetes de información');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.PAQUETE.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('REPORTES.PAQUETE.VER', 'REPORTES', 'Ver paquetes de información');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.GENERAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('REPORTES.GENERAR', 'REPORTES', 'Generar reportes personalizados');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'REPORTES.EXPORTAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('REPORTES.EXPORTAR', 'REPORTES', 'Exportar reportes (PDF, Excel, CSV)');

-- Módulo: INTEGRACIÓN
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.API.CLIENTE.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.API.CLIENTE.CREAR', 'INTEGRACION', 'Crear clientes API');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.API.CLIENTE.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.API.CLIENTE.VER', 'INTEGRACION', 'Ver clientes API');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.API.CLIENTE.REVOCAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.API.CLIENTE.REVOCAR', 'INTEGRACION', 'Revocar clientes API');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.POS.DISPOSITIVO.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.POS.DISPOSITIVO.CREAR', 'INTEGRACION', 'Crear dispositivos POS');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.POS.DISPOSITIVO.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.POS.DISPOSITIVO.VER', 'INTEGRACION', 'Ver dispositivos POS');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.POS.SESION.CERRAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.POS.SESION.CERRAR', 'INTEGRACION', 'Cerrar sesiones de caja POS');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.POS.SESION.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.POS.SESION.VER', 'INTEGRACION', 'Ver sesiones de caja POS');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.WEBHOOK.CREAR')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.WEBHOOK.CREAR', 'INTEGRACION', 'Crear suscripciones de webhook');

IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'INTEGRACION.WEBHOOK.VER')
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion)
    VALUES ('INTEGRACION.WEBHOOK.VER', 'INTEGRACION', 'Ver suscripciones de webhook');

-- ============================================================================
-- 2. ASIGNACIÓN DE PERMISOS A ROLES
-- ============================================================================

-- ADMIN: TODOS los permisos
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'ADMIN'
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- CONTADOR: Contabilidad + Seguridad (solo vista) + Reportes
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'CONTADOR'
  AND (
      p.modulo = 'CONTABILIDAD'
      OR (p.modulo = 'SEGURIDAD' AND p.codigo LIKE 'SEGURIDAD.%VER')
      OR p.modulo = 'REPORTES'
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- ECONOMICO: Contabilidad (vista y presupuestos) + Reportes + Seguridad (vista)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'ECONOMICO'
  AND (
      (p.modulo = 'CONTABILIDAD' AND (p.codigo LIKE 'CONTABILIDAD.%VER' OR p.codigo LIKE 'CONTABILIDAD.PRESUPUESTO%'))
      OR p.modulo = 'REPORTES'
      OR (p.modulo = 'SEGURIDAD' AND p.codigo LIKE 'SEGURIDAD.%VER')
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- JEFE_PRODUCCION: Producción + Inventario (vista de existencias y movimientos)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'JEFE_PRODUCCION'
  AND (
      p.modulo = 'PRODUCCION'
      OR (p.modulo = 'INVENTARIO' AND (p.codigo LIKE 'INVENTARIO.%VER' OR p.codigo LIKE 'INVENTARIO.EXISTENCIA%'))
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- ALMACENERO: Inventario completo + Comercial (vista de proveedores y productos)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'ALMACENERO'
  AND (
      p.modulo = 'INVENTARIO'
      OR (p.modulo = 'COMERCIAL' AND p.codigo LIKE 'COMERCIAL.%VER')
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- RRHH: RRHH completo + Reportes (generar/exportar)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'RRHH'
  AND (
      p.modulo = 'RRHH'
      OR (p.modulo = 'REPORTES' AND (p.codigo LIKE 'REPORTES.GENERAR' OR p.codigo LIKE 'REPORTES.EXPORTAR'))
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- COMERCIAL: Comercial completo + Inventario (productos, precios, existencias)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'COMERCIAL'
  AND (
      p.modulo = 'COMERCIAL'
      OR (p.modulo = 'INVENTARIO' AND (p.codigo LIKE 'INVENTARIO.PRODUCTO.%' OR p.codigo LIKE 'INVENTARIO.LISTA_PRECIO.%' OR p.codigo LIKE 'INVENTARIO.EXISTENCIA.%'))
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- CAJERO_POS: permisos específicos (ventas, clientes, devoluciones, inventario vista, sesiones POS)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'CAJERO_POS'
  AND p.codigo IN (
      'COMERCIAL.FACTURA_VENTA.CREAR',
      'COMERCIAL.FACTURA_VENTA.VER',
      'COMERCIAL.CLIENTE.CREAR',
      'COMERCIAL.CLIENTE.VER',
      'COMERCIAL.DEVOLUCION.CREAR',
      'INVENTARIO.EXISTENCIA.VER',
      'INVENTARIO.LISTA_PRECIO.VER',
      'INVENTARIO.ALMACEN.VER',
      'INTEGRACION.POS.SESION.VER',
      'INTEGRACION.POS.SESION.CERRAR'
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

-- DIRECCION: Reportes, balances, presupuestos, auditoría, parámetros, existencias, nómina (vista)
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r
CROSS JOIN nucleo.permiso p
WHERE r.codigo = 'DIRECCION'
  AND (
      p.modulo = 'REPORTES'
      OR (p.modulo = 'CONTABILIDAD' AND (p.codigo LIKE 'CONTABILIDAD.BALANCE%' OR p.codigo LIKE 'CONTABILIDAD.PRESUPUESTO.%VER' OR p.codigo LIKE 'CONTABILIDAD.CXC.VER' OR p.codigo LIKE 'CONTABILIDAD.CXP.VER'))
      OR (p.modulo = 'SEGURIDAD' AND p.codigo LIKE 'SEGURIDAD.%VER')
      OR (p.modulo = 'INVENTARIO' AND p.codigo LIKE 'INVENTARIO.EXISTENCIA.VER')
      OR (p.modulo = 'RRHH' AND p.codigo LIKE 'RRHH.NOMINA.VER')
      OR p.codigo = 'SEGURIDAD.PARAMETRO.EDITAR'
  )
  AND NOT EXISTS (
      SELECT 1 FROM nucleo.rol_permiso rp
      WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
  );

GO

PRINT 'Permisos insertados y asignados correctamente.';
GO