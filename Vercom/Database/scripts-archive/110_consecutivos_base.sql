-- 110_consecutivos_base.sql
-- RNF-51: Inicializa los consecutivos base (Serie A) para cada entidad y su sucursal matriz,
-- replicando lo que hace AdminService.InitializeEntidadDefaultsAsync al aprobar una entidad.
-- Idempotente: solo inserta las combinaciones que no existan.
SET NOCOUNT ON;

;WITH matriz AS (
    SELECT e.id AS entidad_id,
           (SELECT TOP 1 s.id FROM nucleo.sucursal s
            WHERE s.entidad_id = e.id
            ORDER BY CASE WHEN s.codigo = 'MATRIZ' THEN 0 ELSE 1 END, s.creado_en) AS sucursal_id
    FROM nucleo.entidad e
),
tipos AS (
    SELECT codigo FROM (VALUES
        ('FACTURA_VENTA'),
        ('ORDEN_COMPRA'),
        ('ORDEN_PRODUCCION'),
        ('ASIENTO_CONTABLE'),
        ('VALE_ENTRADA'),
        ('VALE_SALIDA')
    ) t(codigo)
)
INSERT INTO nucleo.consecutivo (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero, longitud_padding, actualizado_en)
SELECT m.entidad_id,
       CASE WHEN t.codigo = 'FACTURA_VENTA' THEN m.sucursal_id ELSE NULL END, -- factura por sucursal; resto central
       t.codigo, 'A', 0, 8, SYSDATETIMEOFFSET()
FROM matriz m
CROSS JOIN tipos t
WHERE NOT EXISTS (
    SELECT 1 FROM nucleo.consecutivo c
    WHERE c.entidad_id = m.entidad_id
      AND ((c.sucursal_id = m.sucursal_id) OR (c.sucursal_id IS NULL AND m.sucursal_id IS NULL))
      AND c.tipo_documento = t.codigo
      AND c.serie = 'A'
);

SELECT COUNT(*) AS consecutivos_totales FROM nucleo.consecutivo;
