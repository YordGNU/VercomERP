/* ============================================================
   VercomERP – Limpieza de datos DEMO
   Elimina todos los datos de la entidad NIT = 'DEMO00000000'
   - NO toca catálogos globales (tipo_comprobante, unidad_medida,
     tipo_movimiento, permiso, indicador, tipo_ausencia,
     tipo_obligacion_fiscal)
   - NO desactiva triggers (integridad de auditoría ONAT)
   - Usa transacción con ROLLBACK automático ante error
   ============================================================ */
USE [VercomERP];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   0. DETECTAR ENTIDAD DEMO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER;

SELECT @E = id FROM nucleo.entidad WHERE nit = 'DEMO00000000';

IF @E IS NULL
BEGIN
    PRINT 'ℹ️  No hay datos DEMO que eliminar (NIT DEMO00000000 no encontrado).';
    RETURN;
END

PRINT '=================================================';
PRINT ' Limpieza DEMO – Entidad: ' + CAST(@E AS NVARCHAR(50));
PRINT '=================================================';
GO

/* ============================================================
   1. EJECUCIÓN PRINCIPAL
   ============================================================ */
BEGIN TRY
    BEGIN TRAN;

    DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

    /* -------- Contadores para reporte final -------- */
    DECLARE @c_audit       INT = 0,
            @c_asientos    INT = 0,
            @c_detalles    INT = 0,
            @c_facturas    INT = 0,
            @c_clientes    INT = 0,
            @c_proveedores INT = 0,
            @c_productos   INT = 0,
            @c_empleados   INT = 0,
            @c_usuarios    INT = 0,
            @c_roles       INT = 0,
            @c_cuentas     INT = 0,
            @c_total       INT = 0;

    /* ============================================================
       FASE 1 – INTEGRACIÓN / POS
       ============================================================ */

    DELETE FROM integracion.webhook_entrega
    WHERE suscripcion_id IN (
        SELECT ws.id FROM integracion.webhook_suscripcion ws
        JOIN integracion.api_cliente ac ON ac.id = ws.api_cliente_id
        WHERE ac.entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.webhook_suscripcion
    WHERE api_cliente_id IN (
        SELECT id FROM integracion.api_cliente WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.api_token
    WHERE api_cliente_id IN (
        SELECT id FROM integracion.api_cliente WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.api_rate_limit
    WHERE api_cliente_id IN (
        SELECT id FROM integracion.api_cliente WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.api_log
    WHERE api_cliente_id IN (
        SELECT id FROM integracion.api_cliente WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.pos_venta_pendiente
    WHERE dispositivo_pos_id IN (
        SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.pos_sync_log
    WHERE dispositivo_pos_id IN (
        SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.pos_rango_numeracion
    WHERE dispositivo_pos_id IN (
        SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.movimiento_caja_pos
    WHERE sesion_caja_pos_id IN (
        SELECT s.id FROM integracion.sesion_caja_pos s
        JOIN integracion.dispositivo_pos dp ON dp.id = s.dispositivo_pos_id
        WHERE dp.entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.sesion_caja_pos
    WHERE dispositivo_pos_id IN (
        SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.dispositivo_pos WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM integracion.api_cliente WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 1 – Integración/POS';


    /* ============================================================
       FASE 2 – COMERCIAL
       ============================================================ */

    -- 2.1 Devoluciones (dependen de factura_detalle)
    DELETE FROM comercial.devolucion_venta_detalle
    WHERE devolucion_id IN (
        SELECT dv.id FROM comercial.devolucion_venta dv
        JOIN comercial.factura_venta fv ON fv.id = dv.factura_id
        WHERE fv.entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM comercial.devolucion_venta
    WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    -- 2.2 Formas de pago y detalles de factura
    DELETE FROM comercial.forma_pago_venta
    WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM comercial.factura_venta_detalle
    WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    -- 2.3 Facturas
    DELETE FROM comercial.factura_venta WHERE entidad_id = @E;
    SET @c_facturas = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    -- 2.4 Recepciones de compra (antes que orden_compra)
    DELETE FROM comercial.recepcion_compra
    WHERE orden_compra_id IN (
        SELECT id FROM comercial.orden_compra WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    -- 2.5 Órdenes de compra
    DELETE FROM comercial.orden_compra_detalle
    WHERE orden_compra_id IN (
        SELECT id FROM comercial.orden_compra WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM comercial.orden_compra WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    -- 2.6 Topes de precio MFP
    DELETE FROM comercial.tope_precio_mfp
    WHERE producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @E)
       OR familia_id  IN (SELECT id FROM inventario.familia_producto WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    -- 2.7 Contratos (suplementos primero)
    DELETE FROM comercial.contrato_economico_suplemento WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM comercial.contrato_economico WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    -- 2.8 Clientes y proveedores
    DELETE FROM comercial.cliente WHERE entidad_id = @E;
    SET @c_clientes = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM comercial.proveedor WHERE entidad_id = @E;
    SET @c_proveedores = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 2 – Comercial';


    /* ============================================================
       FASE 3 – PRODUCCIÓN
       ============================================================ */

    DELETE FROM produccion.analisis_desviacion
    WHERE orden_produccion_id IN (
        SELECT id FROM produccion.orden_produccion WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.orden_produccion_consumo
    WHERE orden_produccion_id IN (
        SELECT id FROM produccion.orden_produccion WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.merma
    WHERE orden_produccion_id IN (
        SELECT id FROM produccion.orden_produccion WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.orden_produccion WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.mantenimiento_programado
    WHERE equipo_id IN (
        SELECT id FROM produccion.equipo WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.equipo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.ficha_costo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.lista_materiales_detalle
    WHERE lista_materiales_id IN (
        SELECT id FROM produccion.lista_materiales lm
        WHERE lm.producto_terminado_id IN (
            SELECT id FROM inventario.producto WHERE entidad_id = @E));
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.lista_materiales
    WHERE producto_terminado_id IN (
        SELECT id FROM inventario.producto WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.plan_produccion_detalle
    WHERE plan_id IN (
        SELECT id FROM produccion.plan_produccion WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM produccion.plan_produccion WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 3 – Producción';


    /* ============================================================
       FASE 4 – INVENTARIO
       ============================================================ */

    DELETE FROM inventario.conteo_fisico_detalle
    WHERE conteo_id IN (
        SELECT cf.id FROM inventario.conteo_fisico cf
        JOIN inventario.almacen a ON a.id = cf.almacen_id
        WHERE a.entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.conteo_fisico
    WHERE almacen_id IN (SELECT id FROM inventario.almacen WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.existencia_lote
    WHERE almacen_id  IN (SELECT id FROM inventario.almacen  WHERE entidad_id = @E)
       OR producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.existencia
    WHERE almacen_id  IN (SELECT id FROM inventario.almacen  WHERE entidad_id = @E)
       OR producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.movimiento_inventario_detalle
    WHERE movimiento_id IN (
        SELECT id FROM inventario.movimiento_inventario WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.movimiento_inventario WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.lista_precio_detalle
    WHERE lista_precio_id IN (
        SELECT id FROM inventario.lista_precio WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.lista_precio WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.producto WHERE entidad_id = @E;
    SET @c_productos = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.familia_producto WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM inventario.almacen WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 4 – Inventario';


    /* ============================================================
       FASE 5 – RRHH
       ============================================================ */

    DELETE FROM rrhh.nomina_detalle_concepto
    WHERE nomina_detalle_id IN (
        SELECT nd.id FROM rrhh.nomina_detalle nd
        JOIN rrhh.periodo_nomina pn ON pn.id = nd.periodo_nomina_id
        WHERE pn.entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.nomina_detalle
    WHERE periodo_nomina_id IN (
        SELECT id FROM rrhh.periodo_nomina WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.certificado_medico
    WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.contrato_laboral
    WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.registro_asistencia
    WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.registro_salario_tiempo_servicio
    WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.saldo_vacaciones
    WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.utile_responsabilidad WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.periodo_nomina WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.empleado WHERE entidad_id = @E;
    SET @c_empleados = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.plantilla_aprobada WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.concepto_nomina WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.cargo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM rrhh.turno_trabajo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 5 – RRHH';


    /* ============================================================
       FASE 6 – CONTABILIDAD (asientos y derivados)
       ============================================================ */

    -- 6.1 Derivados de asiento_contable
    DELETE FROM contabilidad.activo_fijo_depreciacion
    WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @E)
       OR activo_fijo_id IN (SELECT id FROM contabilidad.activo_fijo WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.pago_aplicado
    WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.movimiento_bancario
    WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.declaracion_jurada WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.presupuesto_linea
    WHERE presupuesto_id IN (
        SELECT id FROM contabilidad.presupuesto WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.presupuesto WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    -- 6.2 CxC / CxP (antes de asiento, referencian asiento_origen_id)
    DELETE FROM contabilidad.cuenta_por_cobrar WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.cuenta_por_pagar WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    -- 6.3 Detalle y cabecera de asiento
    DELETE FROM contabilidad.asiento_detalle
    WHERE asiento_id IN (
        SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @E);
    SET @c_detalles = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    -- 6.4 Romper auto-referencia (asiento_reversion_id) por si hay reversiones
    UPDATE contabilidad.asiento_contable
       SET asiento_reversion_id = NULL
     WHERE entidad_id = @E AND asiento_reversion_id IS NOT NULL;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.asiento_contable WHERE entidad_id = @E;
    SET @c_asientos = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    -- 6.5 Activos fijos y auxiliares
    DELETE FROM contabilidad.activo_fijo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.caja WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.cuenta_bancaria WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.centro_costo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    -- 6.6 Períodos y cuentas (último, porque muchos FK apuntan aquí)
    DELETE FROM contabilidad.periodo_contable WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM contabilidad.cuenta_contable WHERE entidad_id = @E;
    SET @c_cuentas = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 6 – Contabilidad';


    /* ============================================================
       FASE 7 – REPORTES
       ============================================================ */

    DELETE FROM reportes.indicador_valor WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM reportes.paquete_informacion WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM reportes.reporte_generado WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 7 – Reportes';


    /* ============================================================
       FASE 8 – NÚCLEO (usuarios, roles, notificaciones)
       ============================================================ */

    DELETE FROM nucleo.feedback WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.notificacion WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.usuario_rol
    WHERE usuario_id IN (SELECT id FROM nucleo.usuario WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.usuario WHERE entidad_id = @E;
    SET @c_usuarios = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    -- Roles: primero romper rol_permiso
    DELETE FROM nucleo.rol_permiso
    WHERE rol_id IN (SELECT id FROM nucleo.rol WHERE entidad_id = @E);
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.rol WHERE entidad_id = @E;
    SET @c_roles = @@ROWCOUNT;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.consecutivo WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.parametro_sistema WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    DELETE FROM nucleo.sucursal WHERE entidad_id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 8 – Núcleo';


    /* ============================================================
       FASE 9 – ENTIDAD (raíz)
       ============================================================ */

    DELETE FROM nucleo.entidad WHERE id = @E;
    SET @c_total += @@ROWCOUNT;

    PRINT '  ✔ Fase 9 – Entidad';


    /* ============================================================
       COMMIT
       ============================================================ */
    COMMIT;

    PRINT '=================================================';
    PRINT ' ✅ Limpieza DEMO completada';
    PRINT '=================================================';
    PRINT '   Cuentas contables:       ' + CAST(@c_cuentas     AS NVARCHAR(10));
    PRINT '   Asientos eliminados:     ' + CAST(@c_asientos    AS NVARCHAR(10));
    PRINT '   Líneas de asiento:       ' + CAST(@c_detalles    AS NVARCHAR(10));
    PRINT '   Facturas eliminadas:     ' + CAST(@c_facturas    AS NVARCHAR(10));
    PRINT '   Clientes eliminados:     ' + CAST(@c_clientes    AS NVARCHAR(10));
    PRINT '   Proveedores eliminados:  ' + CAST(@c_proveedores AS NVARCHAR(10));
    PRINT '   Productos eliminados:    ' + CAST(@c_productos   AS NVARCHAR(10));
    PRINT '   Empleados eliminados:    ' + CAST(@c_empleados   AS NVARCHAR(10));
    PRINT '   Usuarios eliminados:     ' + CAST(@c_usuarios    AS NVARCHAR(10));
    PRINT '   Roles eliminados:        ' + CAST(@c_roles       AS NVARCHAR(10));
    PRINT '   ─────────────────────────────────────';
    PRINT '   TOTAL registros borrados:' + CAST(@c_total       AS NVARCHAR(10));
    PRINT '=================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;

    PRINT '❌ ERROR durante la limpieza. Se hizo ROLLBACK.';
    PRINT '   Mensaje:    ' + ERROR_MESSAGE();
    PRINT '   Línea:      ' + CAST(ERROR_LINE()     AS NVARCHAR(10));
    PRINT '   Severidad:  ' + CAST(ERROR_SEVERITY() AS NVARCHAR(10));
    PRINT '   Estado:     ' + CAST(ERROR_STATE()    AS NVARCHAR(10));

    THROW;
END CATCH
GO


/* ============================================================
   2. VERIFICACIÓN POST-LIMPIEZA
   ============================================================ */
PRINT '--- Verificación post-limpieza ---';

IF EXISTS (SELECT 1 FROM nucleo.entidad WHERE nit = 'DEMO00000000')
    PRINT '⚠️  La entidad DEMO todavía existe';
ELSE
    PRINT '✅ Entidad DEMO eliminada correctamente';

-- Conteos residuales por esquema (debería ser 0 para tenant DEMO)
SELECT 'comercial'   AS esquema, COUNT(*) AS n FROM comercial.cliente      WHERE entidad_id = '11111111-1111-1111-1111-111111111111'
UNION ALL SELECT 'inventario',  COUNT(*) FROM inventario.producto          WHERE entidad_id = '11111111-1111-1111-1111-111111111111'
UNION ALL SELECT 'contabilidad',COUNT(*) FROM contabilidad.cuenta_contable WHERE entidad_id = '11111111-1111-1111-1111-111111111111'
UNION ALL SELECT 'rrhh',        COUNT(*) FROM rrhh.empleado                WHERE entidad_id = '11111111-1111-1111-1111-111111111111'
UNION ALL SELECT 'nucleo',      COUNT(*) FROM nucleo.usuario               WHERE entidad_id = '11111111-1111-1111-1111-111111111111';
GO