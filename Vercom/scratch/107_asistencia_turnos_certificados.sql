USE [VercomERP];
GO

-- ============================================================
-- 107. Mejoras al registro de asistencia (RRHH)
--   B. Turnos de trabajo + deteccion de retardos/salidas tempranas
--   C. Incapacidad temporal (subsidio INASS) enlazada a certificados medicos
-- ============================================================

-- 1. Catalogo de turnos de trabajo
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'rrhh' AND TABLE_NAME = 'turno_trabajo')
BEGIN
    CREATE TABLE rrhh.turno_trabajo (
        id                 uniqueidentifier NOT NULL CONSTRAINT PK_turno_trabajo PRIMARY KEY DEFAULT (newid()),
        entidad_id         uniqueidentifier NOT NULL,
        codigo             nvarchar(20)  NOT NULL,
        nombre             nvarchar(100) NOT NULL,
        hora_entrada       time          NOT NULL,
        hora_salida        time          NOT NULL,
        tolerancia_minutos int           NOT NULL CONSTRAINT DF_turno_tolerancia DEFAULT (15),
        es_nocturno        bit           NOT NULL CONSTRAINT DF_turno_nocturno   DEFAULT (0),
        activo             bit           NOT NULL CONSTRAINT DF_turno_activo     DEFAULT (1),
        creado_en          datetimeoffset NOT NULL CONSTRAINT DF_turno_creado    DEFAULT (sysdatetimeoffset()),
        CONSTRAINT UQ_turno_trabajo_entidad_codigo UNIQUE (entidad_id, codigo),
        CONSTRAINT FK_turno_trabajo_entidad FOREIGN KEY (entidad_id) REFERENCES nucleo.entidad (id)
    );
END
GO

-- 2. Turnos por defecto (jornadas tipicas cubanas)
INSERT INTO rrhh.turno_trabajo (entidad_id, codigo, nombre, hora_entrada, hora_salida, tolerancia_minutos, es_nocturno)
SELECT e.id, v.codigo, v.nombre, v.hora_entrada, v.hora_salida, v.tolerancia, v.nocturno
FROM nucleo.entidad e
CROSS JOIN (VALUES
    ('T-DIURNO',   'Turno Diurno',   CAST('08:00' AS time), CAST('17:00' AS time), 15, CAST(0 AS bit)),
    ('T-MIXTO',    'Turno Mixto',    CAST('14:00' AS time), CAST('22:00' AS time), 15, CAST(0 AS bit)),
    ('T-NOCTURNO', 'Turno Nocturno', CAST('22:00' AS time), CAST('06:00' AS time), 15, CAST(1 AS bit))
) AS v (codigo, nombre, hora_entrada, hora_salida, tolerancia, nocturno)
WHERE NOT EXISTS (
    SELECT 1 FROM rrhh.turno_trabajo t WHERE t.entidad_id = e.id AND t.codigo = v.codigo
);
GO

-- 3. Asignacion del turno por defecto al empleado
IF COL_LENGTH('rrhh.empleado', 'turno_trabajo_id') IS NULL
BEGIN
    ALTER TABLE rrhh.empleado ADD turno_trabajo_id uniqueidentifier NULL;
    ALTER TABLE rrhh.empleado ADD CONSTRAINT FK_empleado_turno_trabajo
        FOREIGN KEY (turno_trabajo_id) REFERENCES rrhh.turno_trabajo (id);
END
GO

-- 4. Snapshot de jornada en la captura de asistencia (auditoria Res. 60/2011)
IF COL_LENGTH('rrhh.registro_asistencia', 'turno_trabajo_id') IS NULL
    ALTER TABLE rrhh.registro_asistencia ADD turno_trabajo_id uniqueidentifier NULL;
IF COL_LENGTH('rrhh.registro_asistencia', 'retardo_minutos') IS NULL
    ALTER TABLE rrhh.registro_asistencia ADD retardo_minutos int NULL;
IF COL_LENGTH('rrhh.registro_asistencia', 'salida_temprana_minutos') IS NULL
    ALTER TABLE rrhh.registro_asistencia ADD salida_temprana_minutos int NULL;
GO

-- 5. Tipo de ausencia: Incapacidad Temporal (Subsidio INASS)
IF NOT EXISTS (SELECT 1 FROM rrhh.tipo_ausencia WHERE codigo = '05')
    INSERT INTO rrhh.tipo_ausencia (codigo, nombre, remunerada, afecta_vacaciones)
    VALUES ('05', 'Incapacidad Temporal (Subsidio INASS)', 1, 0);
GO
