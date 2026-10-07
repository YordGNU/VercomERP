-- 120_contrato_suplementos_produccion.sql
-- CONSOLIDADO 118 + 119 para produccion.
--
-- Ciclo de vida del suplemento de contrato economico:
--   * El suplemento es un documento propio que modifica terminos del contrato
--     (vigencia y/o monto pactado) sin sobrescribir los valores originales.
--   * fecha_fin_original / monto_total_original son respaldos inmutables.
--   * NULL en fecha_inicio / fecha_fin / monto_total_nuevo significa "no modifica ese termino".
--   * No se tipifica el suplemento: no existe la columna tipo.
--
-- IDEMPOTENTE. Puede ejecutarse en cualquiera de estos escenarios:
--   a) Produccion sin la 118  -> crea la tabla en su forma final.
--   b) Produccion con la 118  -> completa monto, fechas anulables y elimina tipo.
--   c) Produccion con 118+119 -> no hace nada (todo verificado).
--   d) Ejecucion repetida      -> no falla.
--
-- Equivale a ejecutar, EN ESTE ORDEN: 118_contrato_suplemento.sql y luego
-- 119_contrato_suplemento_monto.sql. Este archivo los absorbe.

SET NOCOUNT ON;
GO

-- ============================================================
-- 1) comercial.contrato_economico: respaldo de la vigencia original
-- ============================================================
IF COL_LENGTH('comercial.contrato_economico', 'fecha_fin_original') IS NULL
BEGIN
    ALTER TABLE comercial.contrato_economico ADD fecha_fin_original DATE NULL;
    PRINT 'Agregada columna comercial.contrato_economico.fecha_fin_original';
END
GO

UPDATE comercial.contrato_economico
SET fecha_fin_original = fecha_fin
WHERE fecha_fin_original IS NULL AND fecha_fin IS NOT NULL;
GO

-- ============================================================
-- 2) comercial.contrato_economico: respaldo del monto pactado original
-- ============================================================
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

-- ============================================================
-- 3) Tabla de suplementos (forma final: sin tipo, fechas y monto anulables)
-- ============================================================
IF OBJECT_ID('comercial.contrato_economico_suplemento', 'U') IS NULL
BEGIN
    CREATE TABLE comercial.contrato_economico_suplemento (
        id                 UNIQUEIDENTIFIER NOT NULL,
        entidad_id         UNIQUEIDENTIFIER NOT NULL,
        contrato_id        UNIQUEIDENTIFIER NOT NULL,
        numero_suplemento  INT              NOT NULL,
        fecha_firma        DATE             NOT NULL,
        fecha_inicio       DATE             NULL,
        fecha_fin          DATE             NULL,
        monto_total_nuevo  NUMERIC(16, 2)   NULL,
        concepto           NVARCHAR(MAX)    NULL,
        documento_url      NVARCHAR(MAX)    NULL,
        estado             NVARCHAR(15)     NOT NULL CONSTRAINT DF_suplemento_estado DEFAULT 'VIGENTE',
        motivo_anulacion   NVARCHAR(MAX)    NULL,
        creado_por         UNIQUEIDENTIFIER NULL,
        creado_en          DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_suplemento_creado DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT PK_contrato_economico_suplemento PRIMARY KEY (id),
        CONSTRAINT UQ_suplemento_numero UNIQUE (contrato_id, numero_suplemento),
        CONSTRAINT CK_suplemento_estado CHECK (estado IN ('VIGENTE', 'ANULADO')),
        CONSTRAINT CK_suplemento_rango CHECK (fecha_inicio IS NULL OR fecha_fin IS NULL OR fecha_fin > fecha_inicio),
        CONSTRAINT CK_suplemento_monto CHECK (monto_total_nuevo >= 0),
        CONSTRAINT FK_suplemento_contrato FOREIGN KEY (contrato_id)
            REFERENCES comercial.contrato_economico (id) ON DELETE CASCADE,
        CONSTRAINT FK_suplemento_entidad FOREIGN KEY (entidad_id)
            REFERENCES nucleo.entidad (id)
    );

    PRINT 'Creada tabla comercial.contrato_economico_suplemento (forma final)';
END
GO

-- ============================================================
-- 4) Si la tabla existe en la forma de la 118, se lleva a la forma final.
--    Cada bloque esta protegido: si la tabla ya esta actualizada, no hace nada.
-- ============================================================

-- 4.1 Monto que fija el suplemento (NULL = no lo modifica)
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

-- 4.2 Fechas opcionales: NULL = el suplemento no fija ese termino de vigencia
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

-- 4.3 El rango estricto solo aplica cuando el suplemento fija ambas fechas.
--     Definicion antigua (118): [fecha_fin]>[fecha_inicio]  -> sin "IS NULL"
--     Definicion final:         ... IS NULL OR ... IS NULL OR [fecha_fin]>[fecha_inicio]
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = 'CK_suplemento_rango'
             AND parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento')
             AND definition NOT LIKE '%IS NULL%')
BEGIN
    ALTER TABLE comercial.contrato_economico_suplemento DROP CONSTRAINT CK_suplemento_rango;
    PRINT 'Eliminada restriccion CK_suplemento_rango (definicion antigua)';
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

-- 4.4 Se elimina la tipificacion del suplemento
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

-- ============================================================
-- 5) Indice de apoyo: ultimo suplemento vigente de un contrato
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_suplemento_contrato_vigente'
                 AND object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    CREATE INDEX IX_suplemento_contrato_vigente
        ON comercial.contrato_economico_suplemento (contrato_id, estado, fecha_fin DESC);
    PRINT 'Creado indice IX_suplemento_contrato_vigente';
END
GO

-- ============================================================
-- 6) Permisos (idempotente por codigo)
-- ============================================================
INSERT INTO nucleo.permiso (codigo, descripcion, modulo)
SELECT v.codigo, v.descripcion, v.modulo FROM (VALUES
    ('COMERCIAL.CONTRATO_SUPLEMENTO.VER',    'Ver suplementos de contrato',    'COMERCIAL'),
    ('COMERCIAL.CONTRATO_SUPLEMENTO.CREAR',  'Crear suplementos de contrato',  'COMERCIAL'),
    ('COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR', 'Anular suplementos de contrato', 'COMERCIAL')
) AS v(codigo, descripcion, modulo)
WHERE NOT EXISTS (SELECT 1 FROM nucleo.permiso p WHERE p.codigo = v.codigo);
GO

-- ============================================================
-- 7) Grants por rol (idempotente). El suplemento modifica el contrato, por lo
--    que comparte roles con la edicion de contratos. Si un rol no existe, el
--    INNER JOIN lo omite sin abortar.
-- ============================================================
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id FROM (VALUES
    ('ADMINISTRADOR', 'COMERCIAL.CONTRATO_SUPLEMENTO.VER'),
    ('VENDEDOR',      'COMERCIAL.CONTRATO_SUPLEMENTO.VER'),
    ('ADMINISTRADOR', 'COMERCIAL.CONTRATO_SUPLEMENTO.CREAR'),
    ('VENDEDOR',      'COMERCIAL.CONTRATO_SUPLEMENTO.CREAR'),
    ('ADMINISTRADOR', 'COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR'),
    ('VENDEDOR',      'COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR')
) AS g(rol_codigo, permiso_codigo)
INNER JOIN nucleo.rol r     ON r.codigo = g.rol_codigo
INNER JOIN nucleo.permiso p ON p.codigo = g.permiso_codigo
WHERE NOT EXISTS (
    SELECT 1 FROM nucleo.rol_permiso rp
    WHERE rp.rol_id = r.id AND rp.permiso_id = p.id
);
GO

PRINT 'Migracion 120 (consolidado 118+119) completada.'
GO

-- ============================================================
-- 8) VERIFICACION
-- ============================================================

-- 8.1 Respaldos originales en contrato_economico
SELECT c.name AS columna, t.name AS tipo, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('comercial.contrato_economico')
  AND c.name IN ('fecha_fin_original', 'monto_total_original')
ORDER BY c.name;

-- 8.2 Columnas de la tabla de suplementos (no debe aparecer 'tipo')
SELECT c.name AS columna, t.name AS tipo, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('comercial.contrato_economico_suplemento')
ORDER BY c.column_id;

-- 8.3 Restricciones de la tabla de suplementos
SELECT dc.name AS restriccion, dc.type_desc, dc.definition
FROM sys.check_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento')
UNION ALL
SELECT con.name, 'DEFAULT', con.definition
FROM sys.default_constraints con
WHERE con.parent_object_id = OBJECT_ID('comercial.contrato_economico_suplemento')
ORDER BY restriccion;

-- 8.4 Unicidad de numero_contrato: debe ser COMPUESTA por entidad
--     (varias entidades pueden tener el mismo numero de contrato).
SELECT i.name AS indice, i.is_unique, c.name AS columna
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c        ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('comercial.contrato_economico')
  AND i.is_unique = 1
ORDER BY i.name, ic.key_ordinal;

-- 8.5 Permisos de suplemento y sus roles
SELECT p.codigo, COUNT(rp.permiso_id) AS roles_asignados
FROM nucleo.permiso p
LEFT JOIN nucleo.rol_permiso rp ON rp.permiso_id = p.id
WHERE p.codigo LIKE 'COMERCIAL.CONTRATO_SUPLEMENTO.%'
GROUP BY p.codigo
ORDER BY p.codigo;
