-- ============================================================================
-- 112_v_arqueo_caja.sql — Vista de conciliación de arqueo de caja POS
-- Item P3-E2E 21: vista que expone la diferencia declarada vs sistema de cada
-- sesión de caja y su estado contable (PENDIENTE / CONTABILIZADO / SIN_DIFERENCIA).
-- Aplicada sobre BD real VercomERP (2026-09-27) y registrada en schema_completo_corregido.sql.
-- ============================================================================

IF OBJECT_ID('contabilidad.v_arqueo_caja', 'V') IS NOT NULL
    DROP VIEW contabilidad.v_arqueo_caja;
GO

CREATE VIEW contabilidad.v_arqueo_caja AS
SELECT
    s.id AS sesion_id,
    s.caja_id,
    c.nombre AS caja_nombre,
    s.dispositivo_pos_id,
    dp.nombre AS dispositivo_nombre,
    s.cajero_id,
    u.nombre_completo AS cajero_nombre,
    s.fecha_apertura,
    s.fecha_cierre,
    s.monto_apertura,
    s.total_efectivo,
    s.monto_cierre_sistema,
    s.monto_cierre_declarado,
    s.diferencia_arqueo,
    s.observaciones_cierre,
    s.supervisor_conciliacion_id,
    s.estado,
    s.asiento_cierre_id,
    a.numero_comprobante AS asiento_numero,
    CASE
        WHEN s.diferencia_arqueo IS NOT NULL AND s.diferencia_arqueo <> 0 AND s.asiento_cierre_id IS NULL THEN 'PENDIENTE'
        WHEN s.asiento_cierre_id IS NOT NULL THEN 'CONTABILIZADO'
        ELSE 'SIN_DIFERENCIA'
    END AS estado_conciliacion
FROM integracion.sesion_caja_pos s
LEFT JOIN contabilidad.caja c ON c.id = s.caja_id
LEFT JOIN integracion.dispositivo_pos dp ON dp.id = s.dispositivo_pos_id
LEFT JOIN nucleo.usuario u ON u.id = s.cajero_id
LEFT JOIN contabilidad.asiento_contable a ON a.id = s.asiento_cierre_id;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID('contabilidad.v_arqueo_caja') AND name = N'MS_Description'
)
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Conciliación de arqueo de caja POS: diferencia declarada vs sistema y su estado contable (item 21).', @level0type=N'SCHEMA', @level0name=N'contabilidad', @level1type=N'VIEW', @level1name=N'v_arqueo_caja';
GO