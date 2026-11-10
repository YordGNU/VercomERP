-- 121_numero_contrato_por_tercero.sql
-- El numero de contrato economico es unico por ENTIDAD y por TERCERO.
--
-- Regla anterior:  UNIQUE (entidad_id, numero_contrato)
--   -> en una misma entidad solo podia existir UN contrato con cada numero,
--      sin importar a quien fuera el contrato.
-- Regla nueva:     UNIQUE (entidad_id, numero_contrato, cliente_id, proveedor_id)
--   -> se admite el mismo numero en la misma entidad si el tercero difiere
--      (otro proveedor en la compra, otro cliente en la venta).
--   -> sigue prohibido repetir el numero con el MISMO tercero.
--
-- Es correcto gracias a la restriccion chk_contrato_tercero, que garantiza que
-- cliente_id y proveedor_id sean mutuamente excluyentes (el no utilizado es NULL).
-- SQL Server trata dos NULL como iguales al evaluar unicidad, de modo que la fila
-- solo entra en conflicto cuando el tercero activo coincide.
--
-- El indice nuevo es MAS permisivo que el antiguo, por lo que la migracion es
-- segura sin limpiar datos: todo lo valido antes sigue valido.
--
-- IDEMPOTENTE: puede ejecutarse repetidas veces.

SET NOCOUNT ON;
GO

-- ============================================================
-- 1) Eliminar el indice unico antiguo sobre (entidad_id, numero_contrato).
--    El nombre lo genera SQL Server y puede diferir entre instalaciones,
--    por eso se localiza por sus columnas.
-- ============================================================
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql +
    CASE WHEN kc.name IS NOT NULL
         THEN N'ALTER TABLE comercial.contrato_economico DROP CONSTRAINT ' + QUOTENAME(i.name) + N';'
         ELSE N'DROP INDEX ' + QUOTENAME(i.name) + N' ON comercial.contrato_economico;'
    END
FROM sys.indexes i
LEFT JOIN sys.key_constraints kc
       ON kc.parent_object_id = i.object_id
      AND kc.unique_index_id = i.index_id
WHERE i.object_id = OBJECT_ID('comercial.contrato_economico')
  AND i.is_unique = 1
  AND i.is_primary_key = 0
  AND (SELECT COUNT(*) FROM sys.index_columns ic
       WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
         AND ic.is_included_column = 0
         AND COL_NAME(ic.object_id, ic.column_id) IN ('entidad_id', 'numero_contrato')) = 2
  AND (SELECT COUNT(*) FROM sys.index_columns ic
       WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
         AND ic.is_included_column = 0) = 2;

IF @sql <> N''
BEGIN
    EXEC sp_executesql @sql;
    PRINT 'Eliminado el indice unico antiguo (entidad_id, numero_contrato)';
END
GO

-- ============================================================
-- 2) Indice unico nuevo: numero por entidad y por tercero
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID('comercial.contrato_economico')
                 AND name = 'UQ_contrato_numero_tercero')
BEGIN
    CREATE UNIQUE INDEX UQ_contrato_numero_tercero
        ON comercial.contrato_economico (entidad_id, numero_contrato, cliente_id, proveedor_id);
    PRINT 'Creado indice unico UQ_contrato_numero_tercero (entidad_id, numero_contrato, cliente_id, proveedor_id)';
END
GO

PRINT 'Migracion 121 completada.'
GO

-- ============================================================
-- 3) VERIFICACION
-- ============================================================

-- 3.1 Indices unicos de contrato_economico: solo deben existir el PK y el nuevo
SELECT i.name AS indice, i.is_unique, ic.key_ordinal, c.name AS columna
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c        ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('comercial.contrato_economico')
  AND i.is_unique = 1
ORDER BY i.name, ic.key_ordinal;

-- 3.2 La restriccion que sostiene el indice nuevo debe seguir presente
SELECT dc.name AS restriccion, dc.definition
FROM sys.check_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('comercial.contrato_economico')
  AND dc.name = 'chk_contrato_tercero';

-- 3.3 Estados posibles: el indice nuevo no debe haber fallado por datos existentes
SELECT tercero_tipo,
       COUNT(*) AS contratos,
       COUNT(DISTINCT numero_contrato) AS numeros_distintos
FROM comercial.contrato_economico
GROUP BY tercero_tipo;
