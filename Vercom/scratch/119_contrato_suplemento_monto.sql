SET NOCOUNT ON;
GO

-- 1) Respaldo inmutable del monto pactado original
IF COL_LENGTH('comercial.contrato_economico', 'monto_total_original') IS NULL
BEGIN
    ALTER TABLE comercial.contrato_economico ADD monto_total_original NUMERIC(16, 2) NULL;
    PRINT 'Agregada columna comercial.contrato_economico.monto_total_original';
END
GO

UPDATE comercial.contrato_economico
SET monto_total_original = monto_total
WHERE monto_total_original IS NULL AND monto_total IS NOT NULL;
GO

-- 2) Monto nuevo que fija el suplemento (NULL = no lo modifica)
IF COL_LENGTH('comercial.contrato_economico_suplemento', 'monto_total_nuevo') IS NULL
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento ADD monto_total_nuevo NUMERIC(16, 2) NULL;
    PRINT 'Agregada columna comercial.contrato_economico_suplemento.monto_total_nuevo';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_suplemento_monto'
                 AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento
        WITH CHECK ADD CONSTRAINT CK_suplemento_monto CHECK (monto_total_nuevo >= 0);
    PRINT 'Creada restriccion CK_suplemento_monto';
END
GO

-- 3) Fechas opcionales: NULL = el suplemento no fija vigencia
DECLARE @nullable VARCHAR(3);

SELECT @nullable = is_nullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'comercial'
  AND TABLE_NAME = 'contrato_economico_suplemento'
  AND COLUMN_NAME = 'fecha_inicio';

IF @nullable = 'NO'
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento ALTER COLUMN fecha_inicio DATE NULL;
    PRINT 'fecha_inicio ahora admite NULL';
END

SELECT @nullable = is_nullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'comercial'
  AND TABLE_NAME = 'contrato_economico_suplemento'
  AND COLUMN_NAME = 'fecha_fin';

IF @nullable = 'NO'
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento ALTER COLUMN fecha_fin DATE NULL;
    PRINT 'fecha_fin ahora admite NULL';
END
GO

-- El rango solo aplica cuando el suplemento fija vigencia completa
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = 'CK_suplemento_rango'
             AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento DROP CONSTRAINT CK_suplemento_rango;
    PRINT 'Eliminada restriccion CK_suplemento_rango';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = 'CK_suplemento_rango'
                 AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento
        WITH CHECK ADD CONSTRAINT CK_suplemento_rango
        CHECK (fecha_inicio IS NULL OR fecha_fin IS NULL OR fecha_fin > fecha_inicio);
    PRINT 'Creada restriccion CK_suplemento_rango';
END
GO

-- 4) Se elimina la tipificacion del suplemento
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = 'CK_suplemento_tipo'
             AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento DROP CONSTRAINT CK_suplemento_tipo;
    PRINT 'Eliminada restriccion CK_suplemento_tipo';
END
GO

IF EXISTS (SELECT 1 FROM sys.default_constraints
           WHERE name = 'DF_suplemento_tipo'
             AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento DROP CONSTRAINT DF_suplemento_tipo;
    PRINT 'Eliminado default DF_suplemento_tipo';
END
GO

IF COL_LENGTH('comercial.contrato_economico_suplemento', 'tipo') IS NOT NULL
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento DROP COLUMN tipo;
    PRINT 'Eliminada columna comercial.contrato_economico_suplemento.tipo';
END
GO

PRINT 'Migracion 119 completada.'
GO