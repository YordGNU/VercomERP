/* ============================================================
   VERCOMERP – SP reutilizable de purga por entidad
   Uso: EXEC nucleo.sp_purgar_entidad @EntidadId = '...', @DryRun = 1;
   ============================================================ */
USE [VERCOMERP];
GO

CREATE OR ALTER PROCEDURE nucleo.sp_purgar_entidad
    @EntidadId  UNIQUEIDENTIFIER,
    @DryRun     BIT = 1,              -- 1 = solo reporta, 0 = borra
    @BorrarEntidad BIT = 0            -- 1 = también borra nucleo.entidad
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM nucleo.entidad WHERE id = @EntidadId)
    BEGIN
        RAISERROR('La entidad %s no existe', 16, 1, @EntidadId);
        RETURN;
    END

    -- Tabla temporal para reporte
    IF OBJECT_ID('tempdb..#purge_report') IS NOT NULL
        DROP TABLE #purge_report;

    CREATE TABLE #purge_report (
        orden       INT IDENTITY(1,1),
        fase        NVARCHAR(30),
        tabla       NVARCHAR(100),
        filas       INT
    );

    DECLARE @r INT;

    BEGIN TRY
        BEGIN TRAN;

        /* ============================================================
           FASE 1 – INTEGRACIÓN
           ============================================================ */
        DELETE FROM integracion.webhook_entrega
        WHERE suscripcion_id IN (
            SELECT ws.id FROM integracion.webhook_suscripcion ws
            JOIN integracion.api_cliente ac ON ac.id = ws.api_cliente_id
            WHERE ac.entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','webhook_entrega',@r);

        DELETE FROM integracion.webhook_suscripcion
        WHERE api_cliente_id IN (SELECT id FROM integracion.api_cliente WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','webhook_suscripcion',@r);

        DELETE FROM integracion.api_token
        WHERE api_cliente_id IN (SELECT id FROM integracion.api_cliente WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','api_token',@r);

        DELETE FROM integracion.api_rate_limit
        WHERE api_cliente_id IN (SELECT id FROM integracion.api_cliente WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','api_rate_limit',@r);

        DELETE FROM integracion.api_log
        WHERE api_cliente_id IN (SELECT id FROM integracion.api_cliente WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','api_log',@r);

        DELETE FROM integracion.pos_venta_pendiente
        WHERE dispositivo_pos_id IN (SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','pos_venta_pendiente',@r);

        DELETE FROM integracion.pos_sync_log
        WHERE dispositivo_pos_id IN (SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','pos_sync_log',@r);

        DELETE FROM integracion.pos_rango_numeracion
        WHERE dispositivo_pos_id IN (SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','pos_rango_numeracion',@r);

        DELETE FROM integracion.movimiento_caja_pos
        WHERE sesion_caja_pos_id IN (
            SELECT s.id FROM integracion.sesion_caja_pos s
            JOIN integracion.dispositivo_pos dp ON dp.id = s.dispositivo_pos_id
            WHERE dp.entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','movimiento_caja_pos',@r);

        DELETE FROM integracion.sesion_caja_pos
        WHERE dispositivo_pos_id IN (SELECT id FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','sesion_caja_pos',@r);

        DELETE FROM integracion.dispositivo_pos WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','dispositivo_pos',@r);

        DELETE FROM integracion.api_cliente WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INTEGRACION','api_cliente',@r);

        /* ============================================================
           FASE 2 – COMERCIAL
           ============================================================ */
        DELETE FROM comercial.devolucion_venta_detalle
        WHERE devolucion_id IN (
            SELECT dv.id FROM comercial.devolucion_venta dv
            JOIN comercial.factura_venta fv ON fv.id = dv.factura_id
            WHERE fv.entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','devolucion_venta_detalle',@r);

        DELETE FROM comercial.devolucion_venta
        WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','devolucion_venta',@r);

        DELETE FROM comercial.forma_pago_venta
        WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','forma_pago_venta',@r);

        DELETE FROM comercial.factura_venta_detalle
        WHERE factura_id IN (SELECT id FROM comercial.factura_venta WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','factura_venta_detalle',@r);

        DELETE FROM comercial.factura_venta WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','factura_venta',@r);

        DELETE FROM comercial.recepcion_compra
        WHERE orden_compra_id IN (SELECT id FROM comercial.orden_compra WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','recepcion_compra',@r);

        DELETE FROM comercial.orden_compra_detalle
        WHERE orden_compra_id IN (SELECT id FROM comercial.orden_compra WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','orden_compra_detalle',@r);

        DELETE FROM comercial.orden_compra WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','orden_compra',@r);

        DELETE FROM comercial.tope_precio_mfp
        WHERE producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId)
           OR familia_id  IN (SELECT id FROM inventario.familia_producto WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','tope_precio_mfp',@r);

        DELETE FROM comercial.contrato_economico_suplemento WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','contrato_economico_suplemento',@r);

        DELETE FROM comercial.contrato_economico WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','contrato_economico',@r);

        DELETE FROM comercial.cliente WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','cliente',@r);

        DELETE FROM comercial.proveedor WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('COMERCIAL','proveedor',@r);

        /* ============================================================
           FASE 3 – PRODUCCIÓN
           ============================================================ */
        DELETE FROM produccion.analisis_desviacion
        WHERE orden_produccion_id IN (SELECT id FROM produccion.orden_produccion WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','analisis_desviacion',@r);

        DELETE FROM produccion.orden_produccion_consumo
        WHERE orden_produccion_id IN (SELECT id FROM produccion.orden_produccion WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','orden_produccion_consumo',@r);

        DELETE FROM produccion.merma
        WHERE orden_produccion_id IN (SELECT id FROM produccion.orden_produccion WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','merma',@r);

        DELETE FROM produccion.orden_produccion WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','orden_produccion',@r);

        DELETE FROM produccion.mantenimiento_programado
        WHERE equipo_id IN (SELECT id FROM produccion.equipo WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','mantenimiento_programado',@r);

        DELETE FROM produccion.equipo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','equipo',@r);

        DELETE FROM produccion.ficha_costo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','ficha_costo',@r);

        DELETE FROM produccion.lista_materiales_detalle
        WHERE lista_materiales_id IN (
            SELECT lm.id FROM produccion.lista_materiales lm
            WHERE lm.producto_terminado_id IN (
                SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId));
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','lista_materiales_detalle',@r);

        DELETE FROM produccion.lista_materiales
        WHERE producto_terminado_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','lista_materiales',@r);

        DELETE FROM produccion.plan_produccion_detalle
        WHERE plan_id IN (SELECT id FROM produccion.plan_produccion WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','plan_produccion_detalle',@r);

        DELETE FROM produccion.plan_produccion WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('PRODUCCION','plan_produccion',@r);

        /* ============================================================
           FASE 4 – INVENTARIO
           ============================================================ */
        DELETE FROM inventario.conteo_fisico_detalle
        WHERE conteo_id IN (
            SELECT cf.id FROM inventario.conteo_fisico cf
            JOIN inventario.almacen a ON a.id = cf.almacen_id
            WHERE a.entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','conteo_fisico_detalle',@r);

        DELETE FROM inventario.conteo_fisico
        WHERE almacen_id IN (SELECT id FROM inventario.almacen WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','conteo_fisico',@r);

        DELETE FROM inventario.existencia_lote
        WHERE almacen_id IN (SELECT id FROM inventario.almacen  WHERE entidad_id = @EntidadId)
           OR producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','existencia_lote',@r);

        DELETE FROM inventario.existencia
        WHERE almacen_id IN (SELECT id FROM inventario.almacen  WHERE entidad_id = @EntidadId)
           OR producto_id IN (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','existencia',@r);

        DELETE FROM inventario.movimiento_inventario_detalle
        WHERE movimiento_id IN (SELECT id FROM inventario.movimiento_inventario WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','movimiento_inventario_detalle',@r);

        DELETE FROM inventario.movimiento_inventario WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','movimiento_inventario',@r);

        DELETE FROM inventario.lista_precio_detalle
        WHERE lista_precio_id IN (SELECT id FROM inventario.lista_precio WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','lista_precio_detalle',@r);

        DELETE FROM inventario.lista_precio WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','lista_precio',@r);

        DELETE FROM inventario.producto WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','producto',@r);

        DELETE FROM inventario.familia_producto WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','familia_producto',@r);

        DELETE FROM inventario.almacen WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('INVENTARIO','almacen',@r);

        /* ============================================================
           FASE 5 – RRHH
           ============================================================ */
        DELETE FROM rrhh.nomina_detalle_concepto
        WHERE nomina_detalle_id IN (
            SELECT nd.id FROM rrhh.nomina_detalle nd
            JOIN rrhh.periodo_nomina pn ON pn.id = nd.periodo_nomina_id
            WHERE pn.entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','nomina_detalle_concepto',@r);

        DELETE FROM rrhh.nomina_detalle
        WHERE periodo_nomina_id IN (SELECT id FROM rrhh.periodo_nomina WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','nomina_detalle',@r);

        DELETE FROM rrhh.certificado_medico
        WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','certificado_medico',@r);

        DELETE FROM rrhh.contrato_laboral
        WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','contrato_laboral',@r);

        DELETE FROM rrhh.registro_asistencia
        WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','registro_asistencia',@r);

        DELETE FROM rrhh.registro_salario_tiempo_servicio
        WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','registro_salario_tiempo_servicio',@r);

        DELETE FROM rrhh.saldo_vacaciones
        WHERE empleado_id IN (SELECT id FROM rrhh.empleado WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','saldo_vacaciones',@r);

        DELETE FROM rrhh.utile_responsabilidad WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','utile_responsabilidad',@r);

        DELETE FROM rrhh.periodo_nomina WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','periodo_nomina',@r);

        DELETE FROM rrhh.empleado WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','empleado',@r);

        DELETE FROM rrhh.plantilla_aprobada WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','plantilla_aprobada',@r);

        DELETE FROM rrhh.concepto_nomina WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','concepto_nomina',@r);

        DELETE FROM rrhh.cargo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','cargo',@r);

        DELETE FROM rrhh.turno_trabajo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('RRHH','turno_trabajo',@r);

        /* ============================================================
           FASE 6 – CONTABILIDAD
           ============================================================ */
        DELETE FROM contabilidad.activo_fijo_depreciacion
        WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @EntidadId)
           OR activo_fijo_id IN (SELECT id FROM contabilidad.activo_fijo WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','activo_fijo_depreciacion',@r);

        DELETE FROM contabilidad.pago_aplicado
        WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','pago_aplicado',@r);

        DELETE FROM contabilidad.movimiento_bancario
        WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','movimiento_bancario',@r);

        DELETE FROM contabilidad.declaracion_jurada WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','declaracion_jurada',@r);

        DELETE FROM contabilidad.presupuesto_linea
        WHERE presupuesto_id IN (SELECT id FROM contabilidad.presupuesto WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','presupuesto_linea',@r);

        DELETE FROM contabilidad.presupuesto WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','presupuesto',@r);

        DELETE FROM contabilidad.cuenta_por_cobrar WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','cuenta_por_cobrar',@r);

        DELETE FROM contabilidad.cuenta_por_pagar WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','cuenta_por_pagar',@r);

        DELETE FROM contabilidad.asiento_detalle
        WHERE asiento_id IN (SELECT id FROM contabilidad.asiento_contable WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','asiento_detalle',@r);

        UPDATE contabilidad.asiento_contable SET asiento_reversion_id = NULL
        WHERE entidad_id = @EntidadId AND asiento_reversion_id IS NOT NULL;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','asiento_contable (unlink reversion)',@r);

        DELETE FROM contabilidad.asiento_contable WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','asiento_contable',@r);

        DELETE FROM contabilidad.activo_fijo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','activo_fijo',@r);

        DELETE FROM contabilidad.caja WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','caja',@r);

        DELETE FROM contabilidad.cuenta_bancaria WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','cuenta_bancaria',@r);

        DELETE FROM contabilidad.centro_costo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','centro_costo',@r);

        DELETE FROM contabilidad.periodo_contable WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','periodo_contable',@r);

        DELETE FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('CONTABILIDAD','cuenta_contable',@r);

        /* ============================================================
           FASE 7 – REPORTES
           ============================================================ */
        DELETE FROM reportes.indicador_valor WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('REPORTES','indicador_valor',@r);

        DELETE FROM reportes.paquete_informacion WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('REPORTES','paquete_informacion',@r);

        DELETE FROM reportes.reporte_generado WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('REPORTES','reporte_generado',@r);

        /* ============================================================
           FASE 8 – NÚCLEO
           ============================================================ */
        DELETE FROM nucleo.feedback WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','feedback',@r);

        DELETE FROM nucleo.notificacion WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','notificacion',@r);

        DELETE FROM nucleo.usuario_rol
        WHERE usuario_id IN (SELECT id FROM nucleo.usuario WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','usuario_rol',@r);

        DELETE FROM nucleo.usuario WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','usuario',@r);

        DELETE FROM nucleo.rol_permiso
        WHERE rol_id IN (SELECT id FROM nucleo.rol WHERE entidad_id = @EntidadId);
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','rol_permiso',@r);

        DELETE FROM nucleo.rol WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','rol',@r);

        DELETE FROM nucleo.consecutivo WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','consecutivo',@r);

        DELETE FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','parametro_sistema',@r);

        DELETE FROM nucleo.sucursal WHERE entidad_id = @EntidadId;
        SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','sucursal',@r);

        IF @BorrarEntidad = 1
        BEGIN
            DELETE FROM nucleo.entidad WHERE id = @EntidadId;
            SET @r=@@ROWCOUNT; INSERT INTO #purge_report VALUES('NUCLEO','entidad',@r);
        END

        /* ============================================================
           DRY RUN vs COMMIT
           ============================================================ */
        IF @DryRun = 1
        BEGIN
            ROLLBACK;
            PRINT '🔍 DRY RUN – no se eliminó nada (rollback).';
        END
        ELSE
        BEGIN
            COMMIT;
            PRINT '✅ Purga completada.';
        END

        /* Reporte */
        SELECT fase, tabla, filas,
               SUM(filas) OVER (PARTITION BY fase) AS subtotal_fase,
               SUM(filas) OVER () AS total_general
        FROM #purge_report
        WHERE filas > 0
        ORDER BY orden;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        PRINT '❌ ERROR en sp_purgar_entidad: ' + ERROR_MESSAGE();
        PRINT '   Línea: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
        THROW;
    END CATCH
END
GO

PRINT '✔ SP nucleo.sp_purgar_entidad creado.';
GO