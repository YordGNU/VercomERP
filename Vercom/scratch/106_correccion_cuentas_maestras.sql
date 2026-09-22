USE [VercomERP];
GO

-- 1. Corregir clasificación de la cuenta 701 (Ingresos de Actividad Propia)
UPDATE contabilidad.cuenta_contable
SET clase = 'INGRESO', naturaleza = 'ACREEDORA', nombre = 'Ingresos por Ventas de Producción Propia'
WHERE codigo = '701';

-- 2. Asegurar que las cuentas raíz (Nivel 1) no acepten movimientos directos
UPDATE contabilidad.cuenta_contable
SET acepta_movimiento = 0
WHERE LEN(codigo) <= 3 OR nivel = 1;

-- 3. Asegurar que las cuentas de detalle tengan acepta_movimiento = 1 (si tienen subcuentas es 0)
UPDATE cc
SET acepta_movimiento = 1
FROM contabilidad.cuenta_contable cc
WHERE NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable sub WHERE sub.cuenta_padre_id = cc.id)
AND (LEN(codigo) > 3 OR nivel > 1);

-- 4. Crear cuentas faltantes para normalización si no existen (ejemplo 810 para Costo de Ventas)
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo = '810')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, acepta_movimiento, nivel, moneda)
    SELECT NEWID(), id, '810', 'Costo de Ventas', 'GASTO', 'DEUDORA', 1, 1, 'CUP'
    FROM nucleo.entidad;

GO
