-- ============================================================
-- LIMPIEZA DE DATOS DEMO - Vercom ERP
-- Solo elimina datos de entidad NIT = 'DEMO00000000'
-- ============================================================

USE [VercomERP]
GO

DECLARE @DemoEntidadId UNIQUEIDENTIFIER = (
    SELECT id FROM nucleo.entidad WHERE nit = 'DEMO00000000'
);

IF @DemoEntidadId IS NULL
BEGIN
    PRINT 'ℹ️  No hay datos DEMO que eliminar';
    RETURN;
END

PRINT '🗑️  Eliminando datos DEMO...';

-- Deshabilitar triggers solo para esta limpieza controlada
DISABLE TRIGGER ALL ON contabilidad.asiento_contable;
DISABLE TRIGGER ALL ON contabilidad.asiento_detalle;
DISABLE TRIGGER ALL ON comercial.factura_venta;
DISABLE TRIGGER ALL ON rrhh.nomina_detalle;

-- Eliminar en orden de dependencia
DELETE FROM contabilidad.asiento_detalle
WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @DemoEntidadId);

DELETE FROM contabilidad.asiento_contable WHERE entidad_id = @DemoEntidadId;
DELETE FROM contabilidad.cuenta_por_cobrar WHERE entidad_id = @DemoEntidadId;
DELETE FROM contabilidad.cuenta_por_pagar WHERE entidad_id = @DemoEntidadId;
DELETE FROM contabilidad.periodo_contable WHERE entidad_id = @DemoEntidadId;
DELETE FROM contabilidad.cuenta_contable WHERE entidad_id = @DemoEntidadId;
DELETE FROM contabilidad.caja WHERE entidad_id = @DemoEntidadId;

DELETE FROM comercial.factura_venta_detalle
WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @DemoEntidadId);
DELETE FROM comercial.factura_venta WHERE entidad_id = @DemoEntidadId;
DELETE FROM comercial.cliente WHERE entidad_id = @DemoEntidadId;
DELETE FROM comercial.proveedor WHERE entidad_id = @DemoEntidadId;

DELETE FROM inventario.existencia
WHERE almacen_id IN (SELECT id FROM inventario.almacen WHERE entidad_id = @DemoEntidadId);
DELETE FROM inventario.producto WHERE entidad_id = @DemoEntidadId;
DELETE FROM inventario.familia_producto WHERE entidad_id = @DemoEntidadId;
DELETE FROM inventario.almacen WHERE entidad_id = @DemoEntidadId;
DELETE FROM inventario.lista_precio WHERE entidad_id = @DemoEntidadId;

DELETE FROM nucleo.usuario_rol
WHERE usuario_id IN (SELECT id FROM nucleo.usuario WHERE entidad_id = @DemoEntidadId);
DELETE FROM nucleo.usuario WHERE entidad_id = @DemoEntidadId;
DELETE FROM nucleo.rol WHERE entidad_id = @DemoEntidadId;

DELETE FROM nucleo.sucursal WHERE entidad_id = @DemoEntidadId;
DELETE FROM nucleo.entidad WHERE id = @DemoEntidadId;

-- Rehabilitar triggers
ENABLE TRIGGER ALL ON contabilidad.asiento_contable;
ENABLE TRIGGER ALL ON contabilidad.asiento_detalle;
ENABLE TRIGGER ALL ON comercial.factura_venta;
ENABLE TRIGGER ALL ON rrhh.nomina_detalle;

PRINT '✅ Datos DEMO eliminados correctamente';
GO