-- 118: Suplementos de contrato economico (prorrogas encadenadas)
-- Un suplemento formaliza la prorroga como documento propio, sin sobrescribir la
-- fecha original del contrato. Principio analogo al Codigo de Trabajo (Ley 49/1984,
-- art. 39): toda modificacion del contrato requiere documento propio firmado.

SET NOCOUNT ON;
GO

-- 1) Respaldo inmutable de la vigencia original pactada
IF COL_LENGTH('comercial.contrato_economico', 'fecha_fin_original') IS NULL
BEGIN
    ALTER TABLE comercial.contrato_economico ADD fecha_fin_original DATE NULL;
    PRINT 'Agregada columna comercial.contrato_economico.fecha_fin_original';
END
GO

-- Backfill: en contratos existentes la vigencia original es la que hoy figura.
UPDATE comercial.contrato_economico
SET fecha_fin_original = fecha_fin
WHERE fecha_fin_original IS NULL AND fecha_fin IS NOT NULL;
GO

-- 2) Tabla de suplementos
IF OBJECT_ID('comercial.contrato_economico_suplemento', 'U') IS NULL
BEGIN
    CREATE TABLE comercial.contrato_economico_suplemento (
        id                 UNIQUEIDENTIFIER NOT NULL,
        entidad_id         UNIQUEIDENTIFIER NOT NULL,
        contrato_id        UNIQUEIDENTIFIER NOT NULL,
        numero_suplemento  INT              NOT NULL,
        tipo               NVARCHAR(15)     NOT NULL CONSTRAINT DF_suplemento_tipo DEFAULT 'PRORROGA',
        fecha_firma        DATE             NOT NULL,
        fecha_inicio       DATE             NOT NULL,
        fecha_fin          DATE             NOT NULL,
        concepto           NVARCHAR(MAX)    NULL,
        documento_url      NVARCHAR(MAX)    NULL,
        estado             NVARCHAR(15)     NOT NULL CONSTRAINT DF_suplemento_estado DEFAULT 'VIGENTE',
        motivo_anulacion   NVARCHAR(MAX)    NULL,
        creado_por         UNIQUEIDENTIFIER NULL,
        creado_en          DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_suplemento_creado DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT PK_contrato_economico_suplemento PRIMARY KEY (id),
        CONSTRAINT UQ_suplemento_numero UNIQUE (contrato_id, numero_suplemento),
        CONSTRAINT CK_suplemento_tipo CHECK (tipo IN ('PRORROGA')),
        CONSTRAINT CK_suplemento_estado CHECK (estado IN ('VIGENTE', 'ANULADO')),
        CONSTRAINT CK_suplemento_rango CHECK (fecha_fin > fecha_inicio),
        CONSTRAINT FK_suplemento_contrato FOREIGN KEY (contrato_id)
            REFERENCES comercial.contrato_economico (id) ON DELETE CASCADE,
        CONSTRAINT FK_suplemento_entidad FOREIGN KEY (entidad_id)
            REFERENCES nucleo.entidad (id)
    );

    PRINT 'Creada tabla comercial.contrato_economico_suplemento';
END
GO

-- Indice de apoyo para calcular el ultimo suplemento vigente de un contrato
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_suplemento_contrato_vigente' AND object_id = OBJECT_ID('comercial.contrato_economico_suplemento'))
BEGIN
    CREATE INDEX IX_suplemento_contrato_vigente
        ON comercial.contrato_economico_suplemento (contrato_id, estado, fecha_fin DESC);
    PRINT 'Creado indice IX_suplemento_contrato_vigente';
END
GO

-- 3) Permiso de gestion de suplementos (idempotente por codigo)
INSERT INTO nucleo.permiso (codigo, descripcion, modulo)
SELECT v.codigo, v.descripcion, v.modulo FROM (VALUES
    ('COMERCIAL.CONTRATO_SUPLEMENTO.VER',    'Ver suplementos de contrato',    'COMERCIAL'),
    ('COMERCIAL.CONTRATO_SUPLEMENTO.CREAR',  'Crear suplementos de contrato',  'COMERCIAL'),
    ('COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR', 'Anular suplementos de contrato', 'COMERCIAL')
) AS v(codigo, descripcion, modulo)
WHERE NOT EXISTS (SELECT 1 FROM nucleo.permiso p WHERE p.codigo = v.codigo);
GO

-- 4) Grants por rol (idempotente). El suplemento prorroga el contrato, por lo que
--    comparte roles con la edicion de contratos.
DECLARE @admin INT = (SELECT id FROM nucleo.rol WHERE codigo = 'ADMINISTRADOR');
DECLARE @vendedor INT = (SELECT id FROM nucleo.rol WHERE codigo = 'VENDEDOR');

INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.rol_id, p.id FROM (VALUES
    (@admin, 'COMERCIAL.CONTRATO_SUPLEMENTO.VER'),    (@vendedor, 'COMERCIAL.CONTRATO_SUPLEMENTO.VER'),
    (@admin, 'COMERCIAL.CONTRATO_SUPLEMENTO.CREAR'),  (@vendedor, 'COMERCIAL.CONTRATO_SUPLEMENTO.CREAR'),
    (@admin, 'COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR'), (@vendedor, 'COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR')
) AS r(rol_id, codigo)
INNER JOIN nucleo.permiso p ON p.codigo = r.codigo
WHERE NOT EXISTS (
    SELECT 1 FROM nucleo.rol_permiso rp
    WHERE rp.rol_id = r.rol_id AND rp.permiso_id = p.id
);
GO

PRINT 'Migracion 118 completada.'
GO