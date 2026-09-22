-- ============================================================================
-- ESQUEMA COMPLETO SQL SERVER � ERP Sociedad Mercantil Tierra Prometida S.U.R.L.
-- CORREGIDO: �ndice filtrado, orden de GO y ejecuci�n de sp_addextendedproperty
-- ============================================================================

-- ============================================================================
-- M�DULO 0: N�CLEO, SEGURIDAD, ADMINISTRACI�N Y GESTI�N API/POS
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'nucleo')
BEGIN
    EXEC('CREATE SCHEMA nucleo');
END
GO

CREATE TABLE nucleo.entidad (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    razon_social        NVARCHAR(255) NOT NULL,
    nombre_comercial    NVARCHAR(255),
    nit                 NVARCHAR(20) NOT NULL UNIQUE,
    codigo_reeup        NVARCHAR(20) UNIQUE,
    forma_juridica      NVARCHAR(50) NOT NULL DEFAULT 'S.U.R.L.',
    direccion_legal     NVARCHAR(MAX) NOT NULL,
    municipio           NVARCHAR(100),
    provincia           NVARCHAR(100),
    telefono            NVARCHAR(30),
    email               NVARCHAR(150),
    fecha_constitucion  DATE,
    licencia_actividad  NVARCHAR(100),
    moneda_base         NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE nucleo.sucursal (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    tipo                NVARCHAR(30) NOT NULL DEFAULT 'ALMACEN'
                         CHECK (tipo IN ('OFICINA','ALMACEN','TIENDA','PRODUCCION','MIXTO')),
    direccion           NVARCHAR(MAX),
    municipio           NVARCHAR(100),
    provincia           NVARCHAR(100),
    telefono            NVARCHAR(30),
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE nucleo.rol (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    entidad_id          UNIQUEIDENTIFIER REFERENCES nucleo.entidad(id), -- NULL para roles globales/sistema
    codigo              NVARCHAR(30) NOT NULL,
    nombre              NVARCHAR(100) NOT NULL,
    descripcion         NVARCHAR(MAX),
    es_sistema          BIT NOT NULL DEFAULT 0,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT UQ_Rol_Entidad_Codigo UNIQUE (entidad_id, codigo)
);

CREATE TABLE nucleo.permiso (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(80) NOT NULL UNIQUE,
    modulo              NVARCHAR(50) NOT NULL,
    descripcion         NVARCHAR(MAX) NOT NULL
);

CREATE TABLE nucleo.rol_permiso (
    rol_id              INTEGER NOT NULL REFERENCES nucleo.rol(id) ON DELETE CASCADE,
    permiso_id          INTEGER NOT NULL REFERENCES nucleo.permiso(id) ON DELETE CASCADE,
    PRIMARY KEY (rol_id, permiso_id)
);

CREATE TABLE nucleo.usuario (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    nombre_usuario      NVARCHAR(50) NOT NULL UNIQUE,
    nombre_completo     NVARCHAR(150) NOT NULL,
    email               NVARCHAR(150) UNIQUE,
    hash_password       NVARCHAR(255) NOT NULL,
    debe_cambiar_pass   BIT NOT NULL DEFAULT 1,
    intentos_fallidos   SMALLINT NOT NULL DEFAULT 0,
    bloqueado_hasta     DATETIMEOFFSET,
    ultimo_login        DATETIMEOFFSET,
    activo              BIT NOT NULL DEFAULT 1,
    es_empleado_id      UNIQUEIDENTIFIER,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE nucleo.usuario_rol (
    usuario_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.usuario(id) ON DELETE CASCADE,
    rol_id              INTEGER NOT NULL REFERENCES nucleo.rol(id) ON DELETE CASCADE,
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    asignado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    asignado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    PRIMARY KEY (usuario_id, rol_id, sucursal_id)
);

CREATE TABLE nucleo.auditoria (
    id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
    usuario_id          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    nombre_usuario      NVARCHAR(50) NOT NULL,
    accion              NVARCHAR(20) NOT NULL CHECK (accion IN ('INSERT','UPDATE','DELETE','LOGIN','LOGIN_FALLIDO','LOGOUT','EXPORT','REVERSION')),
    esquema_tabla       NVARCHAR(100) NOT NULL,
    registro_id         NVARCHAR(100),
    valores_anteriores  NVARCHAR(MAX) CHECK (valores_anteriores IS NULL OR ISJSON(valores_anteriores) = 1),
    valores_nuevos      NVARCHAR(MAX) CHECK (valores_nuevos IS NULL OR ISJSON(valores_nuevos) = 1),
    ip_origen           NVARCHAR(45),
    canal               NVARCHAR(20) NOT NULL DEFAULT 'ERP' CHECK (canal IN ('ERP','API','POS')),
    dispositivo_id      UNIQUEIDENTIFIER,
    ocurrido_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_auditoria_tabla_registro ON nucleo.auditoria(esquema_tabla, registro_id);
CREATE INDEX idx_auditoria_usuario ON nucleo.auditoria(usuario_id);
CREATE INDEX idx_auditoria_fecha ON nucleo.auditoria(ocurrido_en);

CREATE TABLE nucleo.feedback (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    usuario_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.usuario(id),
    tipo                NVARCHAR(20) NOT NULL CHECK (tipo IN ('SUGERENCIA', 'ERROR', 'FELICITACION', 'SOPORTE')),
    mensaje             NVARCHAR(MAX) NOT NULL,
    metadata_tecnica    NVARCHAR(MAX),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'PENDIENTE' CHECK (estado IN ('PENDIENTE', 'REVISADO', 'RESUELTO', 'ARCHIVADO')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_feedback_entidad ON nucleo.feedback(entidad_id);
CREATE INDEX idx_feedback_fecha ON nucleo.feedback(creado_en);

CREATE TABLE nucleo.backup_log (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tipo                NVARCHAR(20) NOT NULL CHECK (tipo IN ('PROGRAMADO','MANUAL','PRE_CIERRE')),
    ruta_archivo        NVARCHAR(MAX) NOT NULL,
    tamano_bytes        BIGINT,
    estado              NVARCHAR(20) NOT NULL DEFAULT 'EN_PROGRESO' CHECK (estado IN ('EN_PROGRESO','COMPLETADO','FALLIDO')),
    mensaje_error       NVARCHAR(MAX),
    iniciado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    finalizado_en       DATETIMEOFFSET
);

CREATE TABLE nucleo.parametro_sistema (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(80) NOT NULL,
    valor               NVARCHAR(MAX) NOT NULL,
    tipo_dato           NVARCHAR(20) NOT NULL DEFAULT 'STRING' CHECK (tipo_dato IN ('STRING','NUMERIC','BIT','JSON','DATE')),
    descripcion         NVARCHAR(MAX),
    vigente_desde       DATE NOT NULL DEFAULT CAST(SYSDATETIMEOFFSET() AS DATE),
    vigente_hasta       DATE,
    UNIQUE (entidad_id, codigo, vigente_desde)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tasas fiscales, escalas salariales, % vacaciones, etc. Versionado por vigencia para resistir cambios normativos frecuentes del MFP/ONAT.', @level0type=N'SCHEMA', @level0name=N'nucleo', @level1type=N'TABLE', @level1name=N'parametro_sistema';
GO

CREATE TABLE nucleo.consecutivo (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    tipo_documento      NVARCHAR(40) NOT NULL,
    serie               NVARCHAR(10) NOT NULL DEFAULT 'A',
    ultimo_numero       BIGINT NOT NULL DEFAULT 0,
    longitud_padding    SMALLINT NOT NULL DEFAULT 8,
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, sucursal_id, tipo_documento, serie)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Actualizar con SELECT ... FOR UPDATE dentro de la transacci�n para evitar saltos/duplicados concurrentes (POS + ERP simult�neo).', @level0type=N'SCHEMA', @level0name=N'nucleo', @level1type=N'TABLE', @level1name=N'consecutivo';
GO

-- ============================================================================
-- M�DULO 1: CONTABILIDAD Y FINANZAS
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'contabilidad')
BEGIN
    EXEC('CREATE SCHEMA contabilidad');
END
GO

CREATE TABLE contabilidad.cuenta_contable (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(200) NOT NULL,
    cuenta_padre_id     UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    nivel               SMALLINT NOT NULL DEFAULT 1,
    clase               NVARCHAR(20) NOT NULL CHECK (clase IN ('ACTIVO','PASIVO','PATRIMONIO','INGRESO','GASTO','ORDEN')),
    naturaleza          NVARCHAR(10) NOT NULL CHECK (naturaleza IN ('DEUDORA','ACREEDORA')),
    acepta_movimiento   BIT NOT NULL DEFAULT 1,
    requiere_centro_costo BIT NOT NULL DEFAULT 0,
    requiere_tercero    BIT NOT NULL DEFAULT 0,
    moneda              NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, codigo)
);
CREATE INDEX idx_cuenta_padre ON contabilidad.cuenta_contable(cuenta_padre_id);

CREATE TABLE contabilidad.centro_costo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    activo              BIT NOT NULL DEFAULT 1,
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE contabilidad.periodo_contable (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    anio                SMALLINT NOT NULL,
    mes                 SMALLINT NOT NULL CHECK (mes BETWEEN 1 AND 12),
    fecha_inicio        DATE NOT NULL,
    fecha_fin           DATE NOT NULL,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'ABIERTO' CHECK (estado IN ('ABIERTO','CERRADO','BLOQUEADO')),
    cerrado_por         UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    cerrado_en          DATETIMEOFFSET,
    UNIQUE (entidad_id, anio, mes)
);

CREATE TABLE contabilidad.tipo_comprobante (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(10) NOT NULL UNIQUE,
    nombre              NVARCHAR(80) NOT NULL
);

CREATE TABLE contabilidad.asiento_contable (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    periodo_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.periodo_contable(id),
    tipo_comprobante_id INTEGER NOT NULL REFERENCES contabilidad.tipo_comprobante(id),
    numero_comprobante  BIGINT NOT NULL,
    fecha               DATE NOT NULL,
    concepto            NVARCHAR(MAX) NOT NULL,
    modulo_origen       NVARCHAR(30) NOT NULL DEFAULT 'CONTABILIDAD'
                         CHECK (modulo_origen IN ('CONTABILIDAD','NOMINA','INVENTARIO','PRODUCCION','COMPRAS','VENTAS','POS','ACTIVOS_FIJOS')),
    documento_origen_tipo NVARCHAR(50),
    documento_origen_id UNIQUEIDENTIFIER,
    total_debe          NUMERIC(18,2) NOT NULL,
    total_haber         NUMERIC(18,2) NOT NULL,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'CONTABILIZADO'
                         CHECK (estado IN ('BORRADOR','CONTABILIZADO','REVERTIDO')),
    asiento_reversion_id UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    creado_por          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT chk_cuadre CHECK (total_debe = total_haber),
    UNIQUE (entidad_id, tipo_comprobante_id, numero_comprobante)
);
CREATE INDEX idx_asiento_periodo ON contabilidad.asiento_contable(periodo_id);
CREATE INDEX idx_asiento_origen ON contabilidad.asiento_contable(modulo_origen, documento_origen_id);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RF-12: un asiento contabilizado jam�s se edita ni elimina; solo se revierte mediante un nuevo asiento de ajuste enlazado aqu�.', @level0type=N'SCHEMA', @level0name=N'contabilidad', @level1type=N'TABLE', @level1name=N'asiento_contable', @level2type=N'COLUMN', @level2name=N'asiento_reversion_id';
GO

CREATE TABLE contabilidad.asiento_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    asiento_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.asiento_contable(id) ON DELETE CASCADE,
    linea               SMALLINT NOT NULL,
    cuenta_id           UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    centro_costo_id     UNIQUEIDENTIFIER REFERENCES contabilidad.centro_costo(id),
    tercero_tipo        NVARCHAR(20) CHECK (tercero_tipo IN ('CLIENTE','PROVEEDOR','EMPLEADO')),
    tercero_id          UNIQUEIDENTIFIER,
    debe                NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (debe >= 0),
    haber               NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (haber >= 0),
    glosa               NVARCHAR(255),
    CONSTRAINT chk_debe_o_haber CHECK ((debe > 0 AND haber = 0) OR (haber > 0 AND debe = 0)),
    UNIQUE (asiento_id, linea)
);
CREATE INDEX idx_asiento_detalle_cuenta ON contabilidad.asiento_detalle(cuenta_id);
CREATE INDEX idx_asiento_detalle_tercero ON contabilidad.asiento_detalle(tercero_tipo, tercero_id);

CREATE TABLE contabilidad.activo_fijo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    codigo_inventario   NVARCHAR(30) NOT NULL,
    descripcion         NVARCHAR(255) NOT NULL,
    cuenta_activo_id    UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    cuenta_depreciacion_id UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    cuenta_gasto_dep_id UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    fecha_adquisicion   DATE NOT NULL,
    valor_adquisicion   NUMERIC(18,2) NOT NULL,
    valor_residual      NUMERIC(18,2) NOT NULL DEFAULT 0,
    vida_util_meses     INTEGER NOT NULL,
    tasa_depreciacion_anual NUMERIC(5,2),
    metodo_depreciacion NVARCHAR(20) NOT NULL DEFAULT 'LINEA_RECTA' CHECK (metodo_depreciacion IN ('LINEA_RECTA')),
    depreciacion_acumulada NUMERIC(18,2) NOT NULL DEFAULT 0,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'ACTIVO' CHECK (estado IN ('ACTIVO','BAJA','TRASLADADO')),
    fecha_baja          DATE,
    motivo_baja         NVARCHAR(MAX),
    UNIQUE (entidad_id, codigo_inventario)
);

CREATE TABLE contabilidad.activo_fijo_depreciacion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    activo_fijo_id      UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.activo_fijo(id),
    periodo_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.periodo_contable(id),
    monto               NUMERIC(18,2) NOT NULL,
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    calculado_en        DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (activo_fijo_id, periodo_id)
);

CREATE TABLE contabilidad.cuenta_por_cobrar (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    cliente_id          UNIQUEIDENTIFIER NOT NULL,
    documento_origen_tipo NVARCHAR(50) NOT NULL,
    documento_origen_id UNIQUEIDENTIFIER NOT NULL,
    asiento_origen_id   UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    fecha_emision       DATE NOT NULL,
    fecha_vencimiento   DATE NOT NULL,
    monto_original      NUMERIC(18,2) NOT NULL,
    saldo_pendiente     NUMERIC(18,2) NOT NULL,
    moneda              NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    estado              NVARCHAR(15) NOT NULL DEFAULT 'PENDIENTE' CHECK (estado IN ('PENDIENTE','PARCIAL','PAGADO','INCOBRABLE')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_cxc_cliente ON contabilidad.cuenta_por_cobrar(cliente_id);
CREATE INDEX idx_cxc_vencimiento ON contabilidad.cuenta_por_cobrar(fecha_vencimiento) WHERE estado IN ('PENDIENTE','PARCIAL');

CREATE TABLE contabilidad.cuenta_por_pagar (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    proveedor_id        UNIQUEIDENTIFIER NOT NULL,
    documento_origen_tipo NVARCHAR(50) NOT NULL,
    documento_origen_id UNIQUEIDENTIFIER NOT NULL,
    asiento_origen_id   UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    fecha_emision       DATE NOT NULL,
    fecha_vencimiento   DATE NOT NULL,
    monto_original      NUMERIC(18,2) NOT NULL,
    saldo_pendiente     NUMERIC(18,2) NOT NULL,
    moneda              NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    estado              NVARCHAR(15) NOT NULL DEFAULT 'PENDIENTE' CHECK (estado IN ('PENDIENTE','PARCIAL','PAGADO')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_cxp_proveedor ON contabilidad.cuenta_por_pagar(proveedor_id);
CREATE INDEX idx_cxp_vencimiento ON contabilidad.cuenta_por_pagar(fecha_vencimiento) WHERE estado IN ('PENDIENTE','PARCIAL');

CREATE TABLE contabilidad.pago_aplicado (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tipo                NVARCHAR(10) NOT NULL CHECK (tipo IN ('COBRO','PAGO')),
    cuenta_por_cobrar_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_por_cobrar(id),
    cuenta_por_pagar_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_por_pagar(id),
    fecha               DATE NOT NULL,
    monto               NUMERIC(18,2) NOT NULL,
    forma_pago          NVARCHAR(20) NOT NULL CHECK (forma_pago IN ('EFECTIVO','TRANSFERMOVIL','ENZONA','TRANSFERENCIA_BANCARIA','CHEQUE')),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    referencia_externa  NVARCHAR(100),
    CONSTRAINT chk_pago_destino CHECK (
        (tipo='COBRO' AND cuenta_por_cobrar_id IS NOT NULL AND cuenta_por_pagar_id IS NULL) OR
        (tipo='PAGO' AND cuenta_por_pagar_id IS NOT NULL AND cuenta_por_cobrar_id IS NULL)
    )
);

CREATE TABLE contabilidad.cuenta_bancaria (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    banco               NVARCHAR(100) NOT NULL,
    numero_cuenta       NVARCHAR(40) NOT NULL,
    tipo_cuenta         NVARCHAR(20) NOT NULL CHECK (tipo_cuenta IN ('CUP','MLC','AMBAS')),
    cuenta_contable_id  UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    saldo_actual        NUMERIC(18,2) NOT NULL DEFAULT 0,
    activa              BIT NOT NULL DEFAULT 1,
    UNIQUE (entidad_id, numero_cuenta)
);

CREATE TABLE contabilidad.movimiento_bancario (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    cuenta_bancaria_id  UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_bancaria(id),
    fecha               DATE NOT NULL,
    tipo                NVARCHAR(10) NOT NULL CHECK (tipo IN ('DEBITO','CREDITO')),
    monto               NUMERIC(18,2) NOT NULL,
    descripcion         NVARCHAR(MAX),
    referencia          NVARCHAR(100),
    conciliado          BIT NOT NULL DEFAULT 0,
    fecha_conciliacion  DATE,
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id)
);

CREATE TABLE contabilidad.caja (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.sucursal(id),
    nombre              NVARCHAR(100) NOT NULL,
    cuenta_contable_id  UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    limite_efectivo     NUMERIC(18,2),
    saldo_actual        NUMERIC(18,2) NOT NULL DEFAULT 0,
    activa              BIT NOT NULL DEFAULT 1
);

CREATE TABLE contabilidad.tipo_obligacion_fiscal (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(30) NOT NULL UNIQUE,
    nombre              NVARCHAR(150) NOT NULL,
    periodicidad        NVARCHAR(15) NOT NULL CHECK (periodicidad IN ('MENSUAL','TRIMESTRAL','ANUAL')),
    tasa_actual         NUMERIC(6,3),
    base_legal          NVARCHAR(150)
);

CREATE TABLE contabilidad.declaracion_jurada (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    tipo_obligacion_id  INTEGER NOT NULL REFERENCES contabilidad.tipo_obligacion_fiscal(id),
    periodo_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.periodo_contable(id),
    base_imponible      NUMERIC(18,2) NOT NULL,
    monto_calculado     NUMERIC(18,2) NOT NULL,
    monto_pagado        NUMERIC(18,2) NOT NULL DEFAULT 0,
    fecha_limite        DATE NOT NULL,
    fecha_presentacion  DATE,
    estado              NVARCHAR(20) NOT NULL DEFAULT 'PENDIENTE' CHECK (estado IN ('PENDIENTE','PRESENTADA','PAGADA','VENCIDA')),
    numero_dj           NVARCHAR(40),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    generado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, tipo_obligacion_id, periodo_id)
);

CREATE TABLE contabilidad.presupuesto (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    anio                SMALLINT NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'BORRADOR' CHECK (estado IN ('BORRADOR','APROBADO','CERRADO')),
    aprobado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE contabilidad.presupuesto_linea (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    presupuesto_id      UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.presupuesto(id) ON DELETE CASCADE,
    cuenta_id           UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.cuenta_contable(id),
    centro_costo_id     UNIQUEIDENTIFIER REFERENCES contabilidad.centro_costo(id),
    mes                 SMALLINT NOT NULL CHECK (mes BETWEEN 1 AND 12),
    monto_planificado   NUMERIC(18,2) NOT NULL DEFAULT 0,
    UNIQUE (presupuesto_id, cuenta_id, centro_costo_id, mes)
);

GO
CREATE VIEW contabilidad.v_ejecucion_presupuesto AS
SELECT
    pl.presupuesto_id,
    pl.cuenta_id,
    pl.centro_costo_id,
    pl.mes,
    pl.monto_planificado,
    COALESCE(SUM(ad.debe - ad.haber), 0) AS monto_real
FROM contabilidad.presupuesto_linea pl
JOIN contabilidad.presupuesto p ON p.id = pl.presupuesto_id
LEFT JOIN contabilidad.asiento_contable ac
    ON DATEPART(MONTH, ac.fecha) = pl.mes
    AND DATEPART(YEAR, ac.fecha) = p.anio
    AND ac.entidad_id = p.entidad_id
    AND ac.estado = 'CONTABILIZADO'
LEFT JOIN contabilidad.asiento_detalle ad
    ON ad.asiento_id = ac.id AND ad.cuenta_id = pl.cuenta_id
    AND (ad.centro_costo_id = pl.centro_costo_id OR pl.centro_costo_id IS NULL)
GROUP BY pl.presupuesto_id, pl.cuenta_id, pl.centro_costo_id, pl.mes, pl.monto_planificado;
GO

-- ============================================================================
-- M�DULO 2: RECURSOS HUMANOS Y N�MINA
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'rrhh')
BEGIN
    EXEC('CREATE SCHEMA rrhh');
END
GO

CREATE TABLE rrhh.cargo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    funciones           NVARCHAR(MAX), -- RF-20: Misión y funciones del cargo
    salario_escala_min  NUMERIC(12,2),
    salario_escala_max  NUMERIC(12,2),
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE rrhh.plantilla_aprobada (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    cargo_id            UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.cargo(id),
    plazas_aprobadas    INTEGER NOT NULL DEFAULT 1,
    vigente_desde       DATE NOT NULL,
    vigente_hasta       DATE
);

CREATE TABLE rrhh.empleado (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    carnet_identidad    NVARCHAR(11) NOT NULL UNIQUE,
    nombres             NVARCHAR(100) NOT NULL,
    apellidos           NVARCHAR(100) NOT NULL,
    fecha_nacimiento    DATE NOT NULL,
    sexo                NVARCHAR(1) CHECK (sexo IN ('M','F')),
    direccion           NVARCHAR(MAX),
    telefono            NVARCHAR(30),
    email               NVARCHAR(150),
    cargo_id            UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.cargo(id),
    calificacion        NVARCHAR(100),
    nivel_escolaridad   NVARCHAR(50),
    fecha_ingreso       DATE NOT NULL,
    fecha_baja          DATE,
    motivo_baja         NVARCHAR(MAX),
    cuenta_bancaria_pago NVARCHAR(40),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'ACTIVO' CHECK (estado IN ('ACTIVO','LICENCIA','BAJA')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

ALTER TABLE nucleo.usuario ADD CONSTRAINT fk_usuario_empleado FOREIGN KEY (es_empleado_id) REFERENCES rrhh.empleado(id);

CREATE TABLE rrhh.contrato_laboral (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    tipo_contrato       NVARCHAR(20) NOT NULL CHECK (tipo_contrato IN ('DETERMINADO','INDETERMINADO','PRUEBA')),
    fecha_inicio        DATE NOT NULL,
    fecha_fin           DATE,
    salario_pactado     NUMERIC(12,2) NOT NULL,
    jornada_horas_semana NUMERIC(4,1) NOT NULL DEFAULT 44,
    cargo_id            UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.cargo(id),
    documento_url       NVARCHAR(MAX),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'VIGENTE' CHECK (estado IN ('VIGENTE','FINALIZADO','RESCINDIDO')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE rrhh.tipo_ausencia (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(30) NOT NULL UNIQUE,
    nombre              NVARCHAR(100) NOT NULL,
    remunerada          BIT NOT NULL DEFAULT 1,
    afecta_vacaciones   BIT NOT NULL DEFAULT 0
);

CREATE TABLE rrhh.registro_asistencia (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    fecha               DATE NOT NULL,
    hora_entrada        TIME,
    hora_salida         TIME,
    horas_extra         NUMERIC(4,2) NOT NULL DEFAULT 0,
    tipo_ausencia_id    INTEGER REFERENCES rrhh.tipo_ausencia(id),
    observaciones       NVARCHAR(MAX),
    registrado_por      UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    UNIQUE (empleado_id, fecha)
);

CREATE TABLE rrhh.saldo_vacaciones (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    anio                SMALLINT NOT NULL,
    dias_acumulados     NUMERIC(6,2) NOT NULL DEFAULT 0,
    dias_disfrutados    NUMERIC(6,2) NOT NULL DEFAULT 0,
    dias_compensados    NUMERIC(6,2) NOT NULL DEFAULT 0,
    saldo_actual        AS (CAST(dias_acumulados - dias_disfrutados - dias_compensados AS NUMERIC(6,2))) PERSISTED,
    UNIQUE (empleado_id, anio)
);

CREATE TABLE rrhh.certificado_medico (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    fecha_inicio        DATE NOT NULL,
    fecha_fin           DATE NOT NULL,
    dias                AS (DATEDIFF(DAY, fecha_inicio, fecha_fin) + 1) PERSISTED,
    diagnostico_cie     NVARCHAR(20),
    porcentaje_subsidio NUMERIC(5,2) NOT NULL DEFAULT 100,
    numero_certificado  NVARCHAR(40),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-20: acceso restringido � datos de salud del trabajador, solo RR.HH. y direcci�n.', @level0type=N'SCHEMA', @level0name=N'rrhh', @level1type=N'TABLE', @level1name=N'certificado_medico';
GO

CREATE TABLE rrhh.concepto_nomina (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(30) NOT NULL UNIQUE,
    nombre              NVARCHAR(100) NOT NULL,
    tipo                NVARCHAR(15) NOT NULL CHECK (tipo IN ('DEVENGO','DEDUCCION','APORTE_PATRONAL')),
    cuenta_contable_id  UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    formula             NVARCHAR(MAX)
);

CREATE TABLE rrhh.periodo_nomina (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    anio                SMALLINT NOT NULL,
    mes                 SMALLINT NOT NULL CHECK (mes BETWEEN 1 AND 12),
    tipo                NVARCHAR(15) NOT NULL DEFAULT 'MENSUAL' CHECK (tipo IN ('MENSUAL','QUINCENAL','EXTRAORDINARIA')),
    estado              NVARCHAR(20) NOT NULL DEFAULT 'PRENOMINA'
                         CHECK (estado IN ('PRENOMINA','CALCULADA','APROBADA','CONTABILIZADA','PAGADA')),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    calculado_en        DATETIMEOFFSET,
    aprobado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    UNIQUE (entidad_id, anio, mes, tipo)
);

CREATE TABLE rrhh.nomina_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    periodo_nomina_id   UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.periodo_nomina(id) ON DELETE CASCADE,
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    dias_trabajados     NUMERIC(4,1) NOT NULL DEFAULT 0,
    horas_extra         NUMERIC(6,2) NOT NULL DEFAULT 0,
    salario_devengado   NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_deducciones   NUMERIC(12,2) NOT NULL DEFAULT 0,
    salario_neto        NUMERIC(12,2) NOT NULL DEFAULT 0,
    UNIQUE (periodo_nomina_id, empleado_id)
);

CREATE TABLE rrhh.nomina_detalle_concepto (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    nomina_detalle_id   UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.nomina_detalle(id) ON DELETE CASCADE,
    concepto_id         INTEGER NOT NULL REFERENCES rrhh.concepto_nomina(id),
    monto               NUMERIC(12,2) NOT NULL,
    UNIQUE (nomina_detalle_id, concepto_id)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-22: hist�rico salarial inalterable � no se actualiza tras CONTABILIZADA, solo se referencia para reportes probatorios.', @level0type=N'SCHEMA', @level0name=N'rrhh', @level1type=N'TABLE', @level1name=N'nomina_detalle_concepto';
GO

CREATE TABLE rrhh.registro_salario_tiempo_servicio (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    anio                SMALLINT NOT NULL,
    mes                 SMALLINT NOT NULL CHECK (mes BETWEEN 1 AND 12),
    dias_trabajados     NUMERIC(4,1) NOT NULL,
    salario_devengado   NUMERIC(12,2) NOT NULL,
    tiempo_servicio_acumulado_meses INTEGER,
    UNIQUE (empleado_id, anio, mes)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Equivalente a modelo SC-4-08 u oficial vigente MTSS.', @level0type=N'SCHEMA', @level0name=N'rrhh', @level1type=N'TABLE', @level1name=N'registro_salario_tiempo_servicio';

CREATE TABLE rrhh.utile_responsabilidad (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    empleado_id         UNIQUEIDENTIFIER NOT NULL REFERENCES rrhh.empleado(id),
    descripcion         NVARCHAR(200) NOT NULL, -- Ej: Laptop Dell Latitude 5420
    numero_serie        NVARCHAR(50),
    fecha_entrega       DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    fecha_devolucion    DATE,
    estado_entrega      NVARCHAR(50), -- Ej: NUEVO, USADO
    observaciones       NVARCHAR(MAX),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_utile_empleado ON rrhh.utile_responsabilidad(empleado_id);
GO

-- ============================================================================
-- M�DULO 3: INVENTARIO Y ALMAC�N
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'inventario')
BEGIN
    EXEC('CREATE SCHEMA inventario');
END
GO

CREATE TABLE inventario.unidad_medida (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(10) NOT NULL UNIQUE,
    nombre              NVARCHAR(50) NOT NULL,
    es_fraccionable     BIT NOT NULL DEFAULT 0
);

CREATE TABLE inventario.familia_producto (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(100) NOT NULL,
    familia_padre_id    UNIQUEIDENTIFIER REFERENCES inventario.familia_producto(id),
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE inventario.producto (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    codigo              NVARCHAR(30) NOT NULL,
    codigo_barras       NVARCHAR(50),
    nombre              NVARCHAR(200) NOT NULL,
    descripcion         NVARCHAR(MAX),
    familia_id          UNIQUEIDENTIFIER REFERENCES inventario.familia_producto(id),
    unidad_medida_id    INTEGER NOT NULL REFERENCES inventario.unidad_medida(id),
    tipo                NVARCHAR(20) NOT NULL DEFAULT 'TERMINADO'
                         CHECK (tipo IN ('MATERIA_PRIMA','EN_PROCESO','TERMINADO','SERVICIO','MERCANCIA')),
    cuenta_inventario_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    cuenta_costo_venta_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    cuenta_ingreso_id   UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    precio_venta_actual NUMERIC(14,2),
    aplica_impuesto_ventas BIT NOT NULL DEFAULT 1,
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, codigo)
);
CREATE INDEX idx_producto_nombre ON inventario.producto(nombre);
CREATE UNIQUE INDEX uq_producto_codigo_barras ON inventario.producto(entidad_id, codigo_barras)
    WHERE codigo_barras IS NOT NULL;
CREATE INDEX idx_producto_codigo_barras ON inventario.producto(codigo_barras);

CREATE TABLE inventario.lista_precio (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    nombre              NVARCHAR(100) NOT NULL,
    canal               NVARCHAR(15) NOT NULL DEFAULT 'GENERAL' CHECK (canal IN ('GENERAL','ERP','POS','API')),
    vigente_desde       DATE NOT NULL,
    vigente_hasta       DATE,
    activa              BIT NOT NULL DEFAULT 1
);

CREATE TABLE inventario.lista_precio_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    lista_precio_id     UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.lista_precio(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    precio              NUMERIC(14,2) NOT NULL,
    UNIQUE (lista_precio_id, producto_id)
);

CREATE TABLE inventario.almacen (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.sucursal(id),
    codigo              NVARCHAR(20) NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    es_punto_venta      BIT NOT NULL DEFAULT 0,
    activo              BIT NOT NULL DEFAULT 1,
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE inventario.existencia (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    almacen_id          UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad            NUMERIC(16,4) NOT NULL DEFAULT 0,
    costo_promedio      NUMERIC(14,4) NOT NULL DEFAULT 0,
    stock_minimo        NUMERIC(16,4) NOT NULL DEFAULT 0,
    stock_maximo        NUMERIC(16,4),
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (almacen_id, producto_id)
);
-- �ndice corregido: se elimin� la cl�usula WHERE con comparaci�n de dos columnas
CREATE INDEX idx_existencia_bajo_minimo ON inventario.existencia(almacen_id, cantidad, stock_minimo);

CREATE TABLE inventario.tipo_movimiento (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(30) NOT NULL UNIQUE,
    nombre              NVARCHAR(100) NOT NULL,
    naturaleza          NVARCHAR(10) NOT NULL CHECK (naturaleza IN ('ENTRADA','SALIDA')),
    afecta_costo        BIT NOT NULL DEFAULT 1
);

CREATE TABLE inventario.movimiento_inventario (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    tipo_movimiento_id  INTEGER NOT NULL REFERENCES inventario.tipo_movimiento(id),
    numero_documento    NVARCHAR(30) NOT NULL,
    almacen_origen_id   UNIQUEIDENTIFIER REFERENCES inventario.almacen(id),
    almacen_destino_id  UNIQUEIDENTIFIER REFERENCES inventario.almacen(id),
    fecha               DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    referencia_externa_tipo NVARCHAR(50),
    referencia_externa_id  UNIQUEIDENTIFIER,
    canal               NVARCHAR(15) NOT NULL DEFAULT 'ERP' CHECK (canal IN ('ERP','API','POS')),
    dispositivo_pos_id  UNIQUEIDENTIFIER,
    observaciones       NVARCHAR(MAX),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, numero_documento)
);
CREATE INDEX idx_mov_inv_referencia ON inventario.movimiento_inventario(referencia_externa_tipo, referencia_externa_id);
CREATE INDEX idx_mov_inv_fecha ON inventario.movimiento_inventario(fecha);

CREATE TABLE inventario.movimiento_inventario_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    movimiento_id       UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.movimiento_inventario(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad            NUMERIC(16,4) NOT NULL CHECK (cantidad > 0),
    costo_unitario      NUMERIC(14,4),
    lote                NVARCHAR(50),
    fecha_vencimiento   DATE,
    observaciones       NVARCHAR(MAX)
);
CREATE INDEX idx_mov_inv_det_producto ON inventario.movimiento_inventario_detalle(producto_id);

CREATE TABLE inventario.conteo_fisico (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    almacen_id          UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    fecha               DATE NOT NULL,
    tipo                NVARCHAR(15) NOT NULL DEFAULT 'PARCIAL' CHECK (tipo IN ('PARCIAL','TOTAL')),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'EN_PROCESO' CHECK (estado IN ('EN_PROCESO','CONCILIADO','CERRADO')),
    responsable_id      UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE inventario.conteo_fisico_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    conteo_id           UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.conteo_fisico(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad_sistema    NUMERIC(16,4) NOT NULL,
    cantidad_fisica     NUMERIC(16,4),
    diferencia          AS (CAST(cantidad_fisica - cantidad_sistema AS NUMERIC(16,4))) PERSISTED,
    justificacion       NVARCHAR(MAX),
    movimiento_ajuste_id UNIQUEIDENTIFIER REFERENCES inventario.movimiento_inventario(id)
);

-- ============================================================================
-- M�DULO 4: PRODUCCI�N Y MANUFACTURA
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'produccion')
BEGIN
    EXEC('CREATE SCHEMA produccion');
END
GO

CREATE TABLE produccion.ficha_costo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    version             INTEGER NOT NULL DEFAULT 1,
    vigente_desde       DATE NOT NULL,
    vigente_hasta       DATE,
    costo_materia_prima NUMERIC(14,4) NOT NULL DEFAULT 0,
    costo_mano_obra     NUMERIC(14,4) NOT NULL DEFAULT 0,
    gastos_indirectos   NUMERIC(14,4) NOT NULL DEFAULT 0,
    costo_total_unitario AS (CAST(costo_materia_prima + costo_mano_obra + gastos_indirectos AS NUMERIC(14,4))) PERSISTED,
    margen_porcentaje   NUMERIC(5,2),
    precio_sugerido     NUMERIC(14,2),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'VIGENTE' CHECK (estado IN ('BORRADOR','VIGENTE','OBSOLETA')),
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (producto_id, version)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-40: versionada para permitir redefinici�n �gil ante cambios de precios de insumos.', @level0type=N'SCHEMA', @level0name=N'produccion', @level1type=N'TABLE', @level1name=N'ficha_costo';
GO

CREATE TABLE produccion.lista_materiales (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    producto_terminado_id UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    version             INTEGER NOT NULL DEFAULT 1,
    activa              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (producto_terminado_id, version)
);

CREATE TABLE produccion.lista_materiales_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    lista_materiales_id UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.lista_materiales(id) ON DELETE CASCADE,
    producto_insumo_id  UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad_requerida  NUMERIC(14,4) NOT NULL CHECK (cantidad_requerida > 0),
    porcentaje_merma    NUMERIC(5,2) NOT NULL DEFAULT 0,
    UNIQUE (lista_materiales_id, producto_insumo_id)
);

CREATE TABLE produccion.plan_produccion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    anio                SMALLINT NOT NULL,
    mes                 SMALLINT CHECK (mes BETWEEN 1 AND 12),
    presupuesto_id      UNIQUEIDENTIFIER REFERENCES contabilidad.presupuesto(id),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'BORRADOR' CHECK (estado IN ('BORRADOR','APROBADO','EJECUTADO')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE produccion.plan_produccion_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    plan_id             UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.plan_produccion(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad_planificada NUMERIC(14,4) NOT NULL,
    cantidad_ejecutada  NUMERIC(14,4) NOT NULL DEFAULT 0,
    UNIQUE (plan_id, producto_id)
);

CREATE TABLE produccion.orden_produccion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    numero_orden        NVARCHAR(30) NOT NULL,
    producto_terminado_id UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    lista_materiales_id UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.lista_materiales(id),
    ficha_costo_id      UNIQUEIDENTIFIER REFERENCES produccion.ficha_costo(id),
    almacen_insumos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    almacen_producto_id UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    cantidad_planificada NUMERIC(14,4) NOT NULL,
    cantidad_producida  NUMERIC(14,4) NOT NULL DEFAULT 0,
    fecha_inicio_plan   DATE,
    fecha_fin_plan      DATE,
    fecha_inicio_real   DATETIMEOFFSET,
    fecha_fin_real      DATETIMEOFFSET,
    estado              NVARCHAR(20) NOT NULL DEFAULT 'PLANIFICADA'
                         CHECK (estado IN ('PLANIFICADA','EN_PROCESO','TERMINADA','CANCELADA')),
    costo_real_total    NUMERIC(16,4),
    asiento_consumo_id  UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    asiento_terminado_id UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, numero_orden)
);

CREATE TABLE produccion.orden_produccion_consumo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    orden_produccion_id UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.orden_produccion(id) ON DELETE CASCADE,
    producto_insumo_id  UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad_planificada NUMERIC(14,4) NOT NULL,
    cantidad_real       NUMERIC(14,4) NOT NULL DEFAULT 0,
    costo_unitario      NUMERIC(14,4),
    movimiento_inventario_id UNIQUEIDENTIFIER REFERENCES inventario.movimiento_inventario(id)
);

CREATE TABLE produccion.analisis_desviacion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    orden_produccion_id UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.orden_produccion(id),
    componente          NVARCHAR(20) NOT NULL CHECK (componente IN ('MATERIA_PRIMA','MANO_OBRA','GASTOS_INDIRECTOS')),
    costo_estandar      NUMERIC(16,4) NOT NULL,
    costo_real          NUMERIC(16,4) NOT NULL,
    desviacion          AS (CAST(costo_real - costo_estandar AS NUMERIC(16,4))) PERSISTED,
    analizado_en        DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE produccion.merma (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    orden_produccion_id UNIQUEIDENTIFIER REFERENCES produccion.orden_produccion(id),
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad            NUMERIC(14,4) NOT NULL,
    causa               NVARCHAR(30) NOT NULL CHECK (causa IN ('PROCESO_NORMAL','DANO','VENCIMIENTO','ERROR_OPERATIVO','OTRO')),
    valor_contable      NUMERIC(16,4),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    fecha               DATE NOT NULL DEFAULT CAST(SYSDATETIMEOFFSET() AS DATE),
    observaciones       NVARCHAR(MAX)
);

CREATE TABLE produccion.equipo (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER REFERENCES nucleo.sucursal(id),
    activo_fijo_id      UNIQUEIDENTIFIER REFERENCES contabilidad.activo_fijo(id),
    codigo              NVARCHAR(30) NOT NULL,
    nombre              NVARCHAR(150) NOT NULL,
    fecha_ultima_revision DATE,
    frecuencia_mantenimiento_dias INTEGER,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'OPERATIVO' CHECK (estado IN ('OPERATIVO','MANTENIMIENTO','FUERA_SERVICIO')),
    UNIQUE (entidad_id, codigo)
);

CREATE TABLE produccion.mantenimiento_programado (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    equipo_id           UNIQUEIDENTIFIER NOT NULL REFERENCES produccion.equipo(id),
    fecha_programada    DATE NOT NULL,
    fecha_ejecutada     DATE,
    tipo                NVARCHAR(15) NOT NULL DEFAULT 'PREVENTIVO' CHECK (tipo IN ('PREVENTIVO','CORRECTIVO')),
    descripcion         NVARCHAR(MAX),
    costo               NUMERIC(14,2),
    responsable_id      UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'PROGRAMADO' CHECK (estado IN ('PROGRAMADO','EJECUTADO','VENCIDO'))
);

-- ============================================================================
-- M�DULO 5: COMPRAS Y VENTAS
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'comercial')
BEGIN
    EXEC('CREATE SCHEMA comercial');
END
GO

CREATE TABLE comercial.proveedor (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    tipo_persona        NVARCHAR(15) NOT NULL DEFAULT 'JURIDICA' CHECK (tipo_persona IN ('JURIDICA','NATURAL')),
    nit                 NVARCHAR(20),
    razon_social        NVARCHAR(255) NOT NULL,
    direccion           NVARCHAR(MAX),
    telefono            NVARCHAR(30),
    email               NVARCHAR(150),
    cuenta_bancaria     NVARCHAR(40),
    cuenta_contable_id  UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE UNIQUE INDEX uq_proveedor_nit ON comercial.proveedor(entidad_id, nit) WHERE nit IS NOT NULL;

CREATE TABLE comercial.cliente (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    tipo_persona        NVARCHAR(15) NOT NULL DEFAULT 'JURIDICA' CHECK (tipo_persona IN ('JURIDICA','NATURAL')),
    nit_o_ci            NVARCHAR(20),
    nombre_razon_social NVARCHAR(255) NOT NULL,
    direccion           NVARCHAR(MAX),
    telefono            NVARCHAR(30),
    email               NVARCHAR(150),
    segmento            NVARCHAR(20) NOT NULL DEFAULT 'MINORISTA' CHECK (segmento IN ('MINORISTA','MAYORISTA')),
    lista_precio_id     UNIQUEIDENTIFIER REFERENCES inventario.lista_precio(id),
    limite_credito      NUMERIC(14,2) NOT NULL DEFAULT 0,
    cuenta_contable_id  UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_contable(id),
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE UNIQUE INDEX uq_cliente_nit_o_ci ON comercial.cliente(entidad_id, nit_o_ci) WHERE nit_o_ci IS NOT NULL;
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'El "cliente mostrador" del POS (venta an�nima) se modela como registro fijo con nit_o_ci=NULL, nombre_razon_social=''CONSUMIDOR FINAL''.', @level0type=N'SCHEMA', @level0name=N'comercial', @level1type=N'TABLE', @level1name=N'cliente';
GO

CREATE TABLE comercial.contrato_economico (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    tercero_tipo        NVARCHAR(15) NOT NULL CHECK (tercero_tipo IN ('CLIENTE','PROVEEDOR')),
    cliente_id          UNIQUEIDENTIFIER REFERENCES comercial.cliente(id),
    proveedor_id        UNIQUEIDENTIFIER REFERENCES comercial.proveedor(id),
    numero_contrato     NVARCHAR(40) NOT NULL,
    objeto              NVARCHAR(MAX) NOT NULL,
    fecha_firma         DATE NOT NULL,
    fecha_inicio        DATE NOT NULL,
    fecha_fin           DATE,
    monto_total         NUMERIC(16,2),
    documento_url       NVARCHAR(MAX),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'VIGENTE' CHECK (estado IN ('VIGENTE','VENCIDO','RESCINDIDO')),
    CONSTRAINT chk_contrato_tercero CHECK (
        (tercero_tipo='CLIENTE' AND cliente_id IS NOT NULL AND proveedor_id IS NULL) OR
        (tercero_tipo='PROVEEDOR' AND proveedor_id IS NOT NULL AND cliente_id IS NULL)
    ),
    UNIQUE (entidad_id, numero_contrato)
);

CREATE TABLE comercial.orden_compra (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    numero_orden        NVARCHAR(30) NOT NULL,
    proveedor_id        UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.proveedor(id),
    contrato_id         UNIQUEIDENTIFIER REFERENCES comercial.contrato_economico(id),
    almacen_destino_id  UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    fecha               DATE NOT NULL DEFAULT CAST(SYSDATETIMEOFFSET() AS DATE),
    fecha_entrega_esperada DATE,
    moneda              NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    subtotal            NUMERIC(16,2) NOT NULL DEFAULT 0,
    total               NUMERIC(16,2) NOT NULL DEFAULT 0,
    estado              NVARCHAR(20) NOT NULL DEFAULT 'BORRADOR'
                         CHECK (estado IN ('BORRADOR','APROBADA','RECIBIDA_PARCIAL','RECIBIDA_TOTAL','CANCELADA')),
    aprobado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, numero_orden)
);

CREATE TABLE comercial.orden_compra_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    orden_compra_id     UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.orden_compra(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad_solicitada NUMERIC(14,4) NOT NULL,
    cantidad_recibida   NUMERIC(14,4) NOT NULL DEFAULT 0,
    precio_unitario     NUMERIC(14,4) NOT NULL,
    subtotal_linea      AS (CAST(cantidad_solicitada * precio_unitario AS NUMERIC(16,2))) PERSISTED
);

CREATE TABLE comercial.recepcion_compra (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    orden_compra_id     UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.orden_compra(id),
    movimiento_inventario_id UNIQUEIDENTIFIER REFERENCES inventario.movimiento_inventario(id),
    cuenta_por_pagar_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_por_pagar(id),
    fecha               DATE NOT NULL DEFAULT CAST(SYSDATETIMEOFFSET() AS DATE),
    numero_informe_recepcion NVARCHAR(30),
    recibido_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id)
);

CREATE TABLE comercial.factura_venta (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.sucursal(id),
    numero_factura      NVARCHAR(30) NOT NULL,
    serie               NVARCHAR(10) NOT NULL DEFAULT 'A',
    cliente_id          UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.cliente(id),
    contrato_id         UNIQUEIDENTIFIER REFERENCES comercial.contrato_economico(id),
    almacen_id          UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    canal_venta         NVARCHAR(20) NOT NULL DEFAULT 'ERP' CHECK (canal_venta IN ('ERP','POS','API','MAYORISTA')),
    tipo_venta          NVARCHAR(15) NOT NULL DEFAULT 'MINORISTA' CHECK (tipo_venta IN ('MINORISTA','MAYORISTA')),
    dispositivo_pos_id  UNIQUEIDENTIFIER,
    sesion_caja_pos_id  UNIQUEIDENTIFIER,
    fecha               DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    subtotal            NUMERIC(16,2) NOT NULL DEFAULT 0,
    descuento_total     NUMERIC(16,2) NOT NULL DEFAULT 0,
    impuesto_ventas_total NUMERIC(16,2) NOT NULL DEFAULT 0,
    total               NUMERIC(16,2) NOT NULL DEFAULT 0,
    moneda              NVARCHAR(3) NOT NULL DEFAULT 'CUP',
    estado              NVARCHAR(15) NOT NULL DEFAULT 'EMITIDA'
                         CHECK (estado IN ('BORRADOR','EMITIDA','ANULADA','DEVUELTA_PARCIAL','DEVUELTA_TOTAL')),
    motivo_anulacion    NVARCHAR(MAX),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    cuenta_por_cobrar_id UNIQUEIDENTIFIER REFERENCES contabilidad.cuenta_por_cobrar(id),
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, sucursal_id, serie, numero_factura)
);
CREATE INDEX idx_factura_cliente ON comercial.factura_venta(cliente_id);
CREATE INDEX idx_factura_canal ON comercial.factura_venta(canal_venta, fecha);
CREATE INDEX idx_factura_pos_sesion ON comercial.factura_venta(sesion_caja_pos_id);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-51: asignado v�a nucleo.consecutivo con SELECT...FOR UPDATE; numeraci�n reservada por sesi�n offline (ver integracion.pos_venta_pendiente) para sobrevivir cortes de red del POS.', @level0type=N'SCHEMA', @level0name=N'comercial', @level1type=N'TABLE', @level1name=N'factura_venta', @level2type=N'COLUMN', @level2name=N'numero_factura';
GO

CREATE TABLE comercial.factura_venta_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    factura_id          UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.factura_venta(id) ON DELETE CASCADE,
    producto_id         UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.producto(id),
    cantidad            NUMERIC(14,4) NOT NULL CHECK (cantidad > 0),
    precio_unitario     NUMERIC(14,4) NOT NULL,
    descuento_porcentaje NUMERIC(5,2) NOT NULL DEFAULT 0,
    costo_unitario_venta NUMERIC(14,4),
    impuesto_porcentaje NUMERIC(5,2) NOT NULL DEFAULT 0,
    subtotal_linea      NUMERIC(16,2) NOT NULL,
    movimiento_inventario_id UNIQUEIDENTIFIER REFERENCES inventario.movimiento_inventario(id)
);

CREATE TABLE comercial.forma_pago_venta (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    factura_id          UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.factura_venta(id) ON DELETE CASCADE,
    forma_pago          NVARCHAR(20) NOT NULL CHECK (forma_pago IN ('EFECTIVO','TRANSFERMOVIL','ENZONA','TRANSFERENCIA_BANCARIA','CHEQUE','CREDITO')),
    monto               NUMERIC(16,2) NOT NULL,
    referencia_externa  NVARCHAR(100),
    vuelto_entregado    NUMERIC(16,2) DEFAULT 0
);

CREATE TABLE comercial.devolucion_venta (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    factura_id          UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.factura_venta(id),
    fecha               DATE NOT NULL DEFAULT CAST(SYSDATETIMEOFFSET() AS DATE),
    motivo              NVARCHAR(MAX) NOT NULL,
    total_devuelto      NUMERIC(16,2) NOT NULL,
    movimiento_inventario_id UNIQUEIDENTIFIER REFERENCES inventario.movimiento_inventario(id),
    asiento_id          UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    autorizado_por      UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id)
);

CREATE TABLE comercial.devolucion_venta_detalle (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    devolucion_id       UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.devolucion_venta(id) ON DELETE CASCADE,
    factura_detalle_id  UNIQUEIDENTIFIER NOT NULL REFERENCES comercial.factura_venta_detalle(id),
    cantidad_devuelta   NUMERIC(14,4) NOT NULL CHECK (cantidad_devuelta > 0)
);

CREATE TABLE comercial.tope_precio_mfp (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    producto_id         UNIQUEIDENTIFIER REFERENCES inventario.producto(id),
    familia_id          UNIQUEIDENTIFIER REFERENCES inventario.familia_producto(id),
    precio_maximo       NUMERIC(14,2) NOT NULL,
    vigente_desde       DATE NOT NULL,
    vigente_hasta       DATE,
    resolucion_referencia NVARCHAR(150),
    CONSTRAINT chk_tope_alcance CHECK (producto_id IS NOT NULL OR familia_id IS NOT NULL)
);

-- ============================================================================
-- M�DULO 6: REPORTES, INDICADORES Y CIERRE
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'reportes')
BEGIN
    EXEC('CREATE SCHEMA reportes');
END
GO

CREATE TABLE reportes.indicador (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    codigo              NVARCHAR(40) NOT NULL UNIQUE,
    nombre              NVARCHAR(150) NOT NULL,
    categoria           NVARCHAR(30) NOT NULL CHECK (categoria IN ('LIQUIDEZ','RENTABILIDAD','ACTIVIDAD','PRESUPUESTO','OPERATIVO')),
    formula_descripcion NVARCHAR(MAX) NOT NULL
);

CREATE TABLE reportes.indicador_valor (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    indicador_id        INTEGER NOT NULL REFERENCES reportes.indicador(id),
    periodo_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.periodo_contable(id),
    valor               NUMERIC(18,6) NOT NULL,
    calculado_en        DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, indicador_id, periodo_id)
);

CREATE TABLE reportes.paquete_informacion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    periodo_id          UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.periodo_contable(id),
    tipo                NVARCHAR(20) NOT NULL CHECK (tipo IN ('DIRECCION','ONAT','MFP')),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'GENERADO' CHECK (estado IN ('GENERADO','ENVIADO')),
    generado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    generado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE reportes.reporte_generado (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    nombre_reporte      NVARCHAR(150) NOT NULL,
    formato             NVARCHAR(10) NOT NULL CHECK (formato IN ('PDF','XLSX','CSV')),
    parametros_json     NVARCHAR(MAX) CHECK (parametros_json IS NULL OR ISJSON(parametros_json) = 1),
    ruta_archivo        NVARCHAR(MAX) NOT NULL,
    generado_por        UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    generado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

GO
CREATE VIEW reportes.v_auditoria_reversiones AS
SELECT a.id, a.usuario_id, a.nombre_usuario, a.esquema_tabla, a.registro_id,
       a.valores_anteriores, a.valores_nuevos, a.canal, a.ocurrido_en
FROM nucleo.auditoria a
WHERE a.accion = 'REVERSION';
GO

CREATE VIEW reportes.v_auditoria_accesos AS
SELECT a.id, a.usuario_id, a.nombre_usuario, a.accion, a.ip_origen, a.canal, a.ocurrido_en
FROM nucleo.auditoria a
WHERE a.accion IN ('LOGIN','LOGIN_FALLIDO','LOGOUT');
GO

-- ============================================================================
-- VISTA v_saldo_cuenta (corregida: GO antes de EXEC)
-- ============================================================================
GO
CREATE VIEW contabilidad.v_saldo_cuenta AS
SELECT
    ac.entidad_id,
    ad.cuenta_id,
    c.codigo AS codigo_cuenta,
    c.nombre AS nombre_cuenta,
    c.clase,
    c.naturaleza,
    pc.id AS periodo_id,
    pc.anio,
    pc.mes,
    SUM(ad.debe) AS total_debe,
    SUM(ad.haber) AS total_haber,
    CASE WHEN c.naturaleza = 'DEUDORA'
         THEN SUM(ad.debe) - SUM(ad.haber)
         ELSE SUM(ad.haber) - SUM(ad.debe)
    END AS saldo
FROM contabilidad.asiento_detalle ad
JOIN contabilidad.asiento_contable ac ON ac.id = ad.asiento_id
JOIN contabilidad.cuenta_contable c ON c.id = ad.cuenta_id
JOIN contabilidad.periodo_contable pc ON pc.id = ac.periodo_id
WHERE ac.estado = 'CONTABILIZADO'
GROUP BY ac.entidad_id, ad.cuenta_id, c.codigo, c.nombre, c.clase, c.naturaleza, pc.id, pc.anio, pc.mes;
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Base para balance general y estado de resultados. Los reportes NCC (RF-13) se construyen agregando esta vista por clase de cuenta.', @level0type=N'SCHEMA', @level0name=N'contabilidad', @level1type=N'VIEW', @level1name=N'v_saldo_cuenta';
GO

-- ============================================================================
-- M�DULO 7: INTEGRACI�N API Y APLICACI�N POS
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'integracion')
BEGIN
    EXEC('CREATE SCHEMA integracion');
END
GO

CREATE TABLE integracion.api_cliente (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    nombre              NVARCHAR(150) NOT NULL,
    tipo                NVARCHAR(20) NOT NULL DEFAULT 'POS' CHECK (tipo IN ('POS','TERCERO','INTEGRACION_INTERNA')),
    client_id           NVARCHAR(64) NOT NULL UNIQUE,
    client_secret_hash  NVARCHAR(255) NOT NULL,
    scopes              NVARCHAR(MAX) NOT NULL DEFAULT N'[]' CHECK (ISJSON(scopes) = 1),
    activo              BIT NOT NULL DEFAULT 1,
    creado_por          UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    revocado_en         DATETIMEOFFSET
);

CREATE TABLE integracion.api_token (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    api_cliente_id      UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.api_cliente(id),
    token_hash          NVARCHAR(255) NOT NULL UNIQUE,
    tipo                NVARCHAR(15) NOT NULL DEFAULT 'ACCESS' CHECK (tipo IN ('ACCESS','REFRESH')),
    emitido_en          DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    expira_en           DATETIMEOFFSET NOT NULL,
    revocado            BIT NOT NULL DEFAULT 0,
    ip_origen           NVARCHAR(45)
);
CREATE INDEX idx_api_token_cliente ON integracion.api_token(api_cliente_id);

CREATE TABLE integracion.api_rate_limit (
    api_cliente_id      UNIQUEIDENTIFIER PRIMARY KEY REFERENCES integracion.api_cliente(id),
    solicitudes_por_minuto INTEGER NOT NULL DEFAULT 120,
    actualizado_en      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE integracion.api_log (
    id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
    api_cliente_id      UNIQUEIDENTIFIER REFERENCES integracion.api_cliente(id),
    metodo_http         NVARCHAR(10) NOT NULL,
    endpoint            NVARCHAR(255) NOT NULL,
    codigo_respuesta    SMALLINT,
    duracion_ms         INTEGER,
    ip_origen           NVARCHAR(45),
    ocurrido_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_api_log_cliente_fecha ON integracion.api_log(api_cliente_id, ocurrido_en);

CREATE TABLE integracion.dispositivo_pos (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    entidad_id          UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.entidad(id),
    sucursal_id         UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.sucursal(id),
    almacen_id          UNIQUEIDENTIFIER NOT NULL REFERENCES inventario.almacen(id),
    api_cliente_id      UNIQUEIDENTIFIER REFERENCES integracion.api_cliente(id),
    codigo              NVARCHAR(30) NOT NULL,
    nombre              NVARCHAR(100) NOT NULL,
    identificador_hardware NVARCHAR(100),
    caja_id             UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.caja(id),
    ultima_sincronizacion DATETIMEOFFSET,
    version_app_pos     NVARCHAR(20),
    estado              NVARCHAR(15) NOT NULL DEFAULT 'ACTIVO' CHECK (estado IN ('ACTIVO','INACTIVO','MANTENIMIENTO')),
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UNIQUE (entidad_id, codigo)
);

ALTER TABLE nucleo.auditoria ADD CONSTRAINT fk_auditoria_dispositivo FOREIGN KEY (dispositivo_id) REFERENCES integracion.dispositivo_pos(id);
ALTER TABLE inventario.movimiento_inventario ADD CONSTRAINT fk_mov_inv_dispositivo FOREIGN KEY (dispositivo_pos_id) REFERENCES integracion.dispositivo_pos(id);
ALTER TABLE comercial.factura_venta ADD CONSTRAINT fk_factura_dispositivo FOREIGN KEY (dispositivo_pos_id) REFERENCES integracion.dispositivo_pos(id);

CREATE TABLE integracion.sesion_caja_pos (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    caja_id             UNIQUEIDENTIFIER NOT NULL REFERENCES contabilidad.caja(id),
    dispositivo_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.dispositivo_pos(id),
    cajero_id           UNIQUEIDENTIFIER NOT NULL REFERENCES nucleo.usuario(id),
    fecha_apertura      DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    monto_apertura      NUMERIC(14,2) NOT NULL DEFAULT 0,
    fecha_cierre        DATETIMEOFFSET,
    monto_cierre_declarado NUMERIC(14,2),
    monto_cierre_sistema NUMERIC(14,2),
    diferencia_arqueo   AS (CAST(monto_cierre_declarado - monto_cierre_sistema AS NUMERIC(14,2))) PERSISTED,
    total_ventas        NUMERIC(16,2) NOT NULL DEFAULT 0,
    total_efectivo      NUMERIC(16,2) NOT NULL DEFAULT 0,
    total_transfermovil NUMERIC(16,2) NOT NULL DEFAULT 0,
    total_enzona        NUMERIC(16,2) NOT NULL DEFAULT 0,
    total_otros_medios  NUMERIC(16,2) NOT NULL DEFAULT 0,
    cantidad_facturas   INTEGER NOT NULL DEFAULT 0,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'ABIERTA' CHECK (estado IN ('ABIERTA','CERRADA','CONCILIADA')),
    asiento_cierre_id   UNIQUEIDENTIFIER REFERENCES contabilidad.asiento_contable(id),
    observaciones_cierre NVARCHAR(MAX),
    supervisor_conciliacion_id UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id)
);
CREATE INDEX idx_sesion_caja_dispositivo ON integracion.sesion_caja_pos(dispositivo_pos_id, estado);
CREATE INDEX idx_sesion_caja_caja ON integracion.sesion_caja_pos(caja_id, estado);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Una caja puede tener una única sesión ABIERTA, compartida por todos sus dispositivos (UNIQUE(caja_id) WHERE estado=''ABIERTA''). dispositivo_pos_id es el dispositivo que la abrió.', @level0type=N'SCHEMA', @level0name=N'integracion', @level1type=N'TABLE', @level1name=N'sesion_caja_pos';
GO

CREATE UNIQUE INDEX uq_sesion_caja_abierta ON integracion.sesion_caja_pos(caja_id) WHERE estado = 'ABIERTA';

ALTER TABLE comercial.factura_venta ADD CONSTRAINT fk_factura_sesion_caja FOREIGN KEY (sesion_caja_pos_id) REFERENCES integracion.sesion_caja_pos(id);

CREATE TABLE integracion.movimiento_caja_pos (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    sesion_caja_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.sesion_caja_pos(id),
    tipo                NVARCHAR(20) NOT NULL CHECK (tipo IN ('VENTA','RETIRO_EFECTIVO','INGRESO_EFECTIVO','DEVOLUCION')),
    monto               NUMERIC(14,2) NOT NULL,
    factura_id          UNIQUEIDENTIFIER REFERENCES comercial.factura_venta(id),
    motivo              NVARCHAR(MAX),
    autorizado_por      UNIQUEIDENTIFIER REFERENCES nucleo.usuario(id),
    ocurrido_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE integracion.pos_venta_pendiente (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dispositivo_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.dispositivo_pos(id),
    sesion_caja_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.sesion_caja_pos(id),
    idempotency_key     NVARCHAR(80) NOT NULL,
    payload_json        NVARCHAR(MAX) NOT NULL CHECK (ISJSON(payload_json) = 1),
    fecha_venta_local   DATETIMEOFFSET NOT NULL,
    fecha_recibido_servidor DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    estado              NVARCHAR(20) NOT NULL DEFAULT 'PENDIENTE'
                         CHECK (estado IN ('PENDIENTE','PROCESADO','ERROR','DUPLICADO')),
    factura_id          UNIQUEIDENTIFIER REFERENCES comercial.factura_venta(id),
    mensaje_error       NVARCHAR(MAX),
    intentos_procesamiento SMALLINT NOT NULL DEFAULT 0,
    procesado_en        DATETIMEOFFSET,
    UNIQUE (dispositivo_pos_id, idempotency_key)
);
CREATE INDEX idx_pos_venta_pendiente_estado ON integracion.pos_venta_pendiente(estado) WHERE estado = 'PENDIENTE';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-02/RNF-50: la app POS crea el registro localmente con idempotency_key propio y hace upsert al reconectar. El worker de sincronizaci�n procesa PENDIENTE -> crea factura_venta -> marca PROCESADO. Reintentos seguros gracias a la clave �nica (dispositivo, idempotency_key).', @level0type=N'SCHEMA', @level0name=N'integracion', @level1type=N'TABLE', @level1name=N'pos_venta_pendiente';
GO

CREATE TABLE integracion.pos_rango_numeracion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dispositivo_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.dispositivo_pos(id),
    tipo_documento      NVARCHAR(40) NOT NULL DEFAULT 'FACTURA_VENTA',
    serie               NVARCHAR(10) NOT NULL,
    numero_desde        BIGINT NOT NULL,
    numero_hasta        BIGINT NOT NULL,
    numero_siguiente_local BIGINT NOT NULL,
    asignado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    agotado             BIT NOT NULL DEFAULT 0,
    CONSTRAINT chk_rango CHECK (numero_hasta > numero_desde AND numero_siguiente_local BETWEEN numero_desde AND numero_hasta + 1)
);
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Alternativa a reservar consecutivos: el servidor asigna bloques (p.ej. 1000 n�meros) a cada terminal al sincronizar. El terminal numera localmente dentro de su rango incluso sin conexi�n, preservando RNF-51 (sin duplicados ni saltos) sin depender de la red para cada venta.', @level0type=N'SCHEMA', @level0name=N'integracion', @level1type=N'TABLE', @level1name=N'pos_rango_numeracion';
GO

CREATE TABLE integracion.pos_sync_log (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dispositivo_pos_id  UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.dispositivo_pos(id),
    tipo_sync           NVARCHAR(20) NOT NULL CHECK (tipo_sync IN ('CATALOGO','PRECIOS','EXISTENCIAS','VENTAS_SUBIDA','COMPLETA')),
    direccion           NVARCHAR(10) NOT NULL CHECK (direccion IN ('SERVIDOR_POS','POS_SERVIDOR')),
    registros_procesados INTEGER NOT NULL DEFAULT 0,
    estado              NVARCHAR(15) NOT NULL DEFAULT 'EN_PROGRESO' CHECK (estado IN ('EN_PROGRESO','COMPLETADO','FALLIDO')),
    detalle_error       NVARCHAR(MAX),
    iniciado_en         DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    finalizado_en       DATETIMEOFFSET
);
CREATE INDEX idx_pos_sync_dispositivo ON integracion.pos_sync_log(dispositivo_pos_id, iniciado_en);

CREATE TABLE integracion.webhook_suscripcion (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    api_cliente_id      UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.api_cliente(id),
    evento              NVARCHAR(60) NOT NULL,
    url_destino         NVARCHAR(MAX) NOT NULL,
    secreto_firma_hash  NVARCHAR(255) NOT NULL,
    activo              BIT NOT NULL DEFAULT 1,
    creado_en           DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE integracion.webhook_entrega (
    id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    suscripcion_id      UNIQUEIDENTIFIER NOT NULL REFERENCES integracion.webhook_suscripcion(id),
    payload_json        NVARCHAR(MAX) NOT NULL CHECK (ISJSON(payload_json) = 1),
    intento_numero      SMALLINT NOT NULL DEFAULT 1,
    codigo_respuesta_http SMALLINT,
    exitoso             BIT NOT NULL DEFAULT 0,
    proximo_reintento_en DATETIMEOFFSET,
    enviado_en          DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX idx_webhook_entrega_pendiente ON integracion.webhook_entrega(proximo_reintento_en) WHERE exitoso = 0;

-- ============================================================================
-- DATOS BASE / SEED
-- ============================================================================

INSERT INTO nucleo.rol (codigo, nombre, es_sistema) VALUES
    ('ADMIN', 'Administrador del sistema', 1),
    ('CONTADOR', 'Contador', 1),
    ('ECONOMICO', 'Econ�mico', 1),
    ('JEFE_PRODUCCION', 'Jefe de Producci�n', 1),
    ('ALMACENERO', 'Almacenero', 1),
    ('RRHH', 'Recursos Humanos', 1),
    ('COMERCIAL', 'Comercial / Ventas', 1),
    ('CAJERO_POS', 'Cajero de punto de venta', 1),
    ('DIRECCION', 'Direcci�n', 1);

INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES
    ('ING', 'Comprobante de Ingreso'),
    ('EGR', 'Comprobante de Egreso'),
    ('DIA', 'Comprobante Diario / Operaciones'),
    ('AJU', 'Comprobante de Ajuste / Reversi�n');

INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES
    ('UN', 'Unidad', 0),
    ('KG', 'Kilogramo', 1),
    ('G', 'Gramo', 1),
    ('L', 'Litro', 1),
    ('ML', 'Mililitro', 1),
    ('M', 'Metro', 1),
    ('CAJ', 'Caja', 0),
    ('PAQ', 'Paquete', 0);

INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES
    ('RECEPCION', 'Informe de Recepci�n', 'ENTRADA', 1),
    ('VALE_ENTREGA', 'Vale de Entrega', 'SALIDA', 1),
    ('DEVOLUCION_ENTRADA', 'Devoluci�n de Cliente', 'ENTRADA', 1),
    ('DEVOLUCION_SALIDA', 'Devoluci�n a Proveedor', 'SALIDA', 1),
    ('TRANSFERENCIA_SALIDA', 'Transferencia - Salida', 'SALIDA', 0),
    ('TRANSFERENCIA_ENTRADA', 'Transferencia - Entrada', 'ENTRADA', 0),
    ('AJUSTE_POSITIVO', 'Ajuste por Sobrante', 'ENTRADA', 1),
    ('AJUSTE_NEGATIVO', 'Ajuste por Faltante', 'SALIDA', 1),
    ('CONSUMO_PRODUCCION', 'Consumo en Producci�n', 'SALIDA', 1),
    ('ENTRADA_PRODUCCION', 'Entrada de Producto Terminado', 'ENTRADA', 1),
    ('VENTA_POS', 'Venta en Punto de Venta', 'SALIDA', 1);

INSERT INTO rrhh.tipo_ausencia (codigo, nombre, remunerada, afecta_vacaciones) VALUES
    ('VACACIONES', 'Vacaciones', 1, 0),
    ('CERT_MEDICO', 'Certificado M�dico', 1, 0),
    ('LICENCIA_NO_RETRIBUIDA', 'Licencia no Retribuida', 0, 1),
    ('MATERNIDAD', 'Licencia de Maternidad', 1, 0),
    ('AUSENCIA_INJUSTIFICADA', 'Ausencia Injustificada', 0, 1);

INSERT INTO contabilidad.tipo_obligacion_fiscal (codigo, nombre, periodicidad, base_legal) VALUES
    ('IMP_UTILIDADES', 'Impuesto sobre Utilidades', 'ANUAL', 'Ley 113 del Sistema Tributario'),
    ('IMP_VENTAS', 'Impuesto sobre Ventas', 'MENSUAL', 'Ley 113 del Sistema Tributario'),
    ('IMP_SERVICIOS', 'Impuesto sobre Servicios', 'MENSUAL', 'Ley 113 del Sistema Tributario'),
    ('SEG_SOCIAL', 'Contribuci�n a la Seguridad Social', 'MENSUAL', 'Ley 113 del Sistema Tributario'),
    ('FUERZA_TRABAJO', 'Impuesto por Utilizaci�n de Fuerza de Trabajo', 'MENSUAL', 'Ley 113 del Sistema Tributario');

INSERT INTO rrhh.concepto_nomina (codigo, nombre, tipo) VALUES
    ('SAL_BASICO', 'Salario B�sico', 'DEVENGO'),
    ('PAGO_RESULTADOS', 'Pago por Resultados', 'DEVENGO'),
    ('HORA_EXTRA', 'Horas Extras', 'DEVENGO'),
    ('SUBSIDIO_SS', 'Subsidio Seguridad Social (corto plazo)', 'DEVENGO'),
    ('VACACIONES_PAGO', 'Pago de Vacaciones', 'DEVENGO'),
    ('CONT_SS_TRAB', 'Contribuci�n Especial Seg. Social (trabajador)', 'DEDUCCION'),
    ('IMP_INGRESOS_PERS', 'Impuesto sobre Ingresos Personales', 'DEDUCCION'),
    ('OTRAS_DEDUCCIONES', 'Otras Deducciones', 'DEDUCCION'),
    ('APORTE_SS_PATRONAL', 'Aporte Seg. Social (empleador)', 'APORTE_PATRONAL'),
    ('APORTE_FUERZA_TRABAJO', 'Impuesto Fuerza de Trabajo (empleador)', 'APORTE_PATRONAL');

GO

-- ============================================================================
-- SEGURIDAD A NIVEL DE BASE DE DATOS
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'erp_app')
BEGIN
    CREATE LOGIN erp_app WITH PASSWORD = 'CAMBIAR_EN_DESPLIEGUE!2026', CHECK_POLICY = ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'erp_readonly')
BEGIN
    CREATE LOGIN erp_readonly WITH PASSWORD = 'CAMBIAR_EN_DESPLIEGUE!2026', CHECK_POLICY = ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'erp_app')
BEGIN
    CREATE USER erp_app FOR LOGIN erp_app;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'erp_readonly')
BEGIN
    CREATE USER erp_readonly FOR LOGIN erp_readonly;
END
GO

GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::nucleo       TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::contabilidad TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::rrhh         TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::inventario   TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::produccion   TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::comercial    TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::reportes     TO erp_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::integracion  TO erp_app;

GRANT SELECT ON SCHEMA::nucleo       TO erp_readonly;
GRANT SELECT ON SCHEMA::contabilidad TO erp_readonly;
GRANT SELECT ON SCHEMA::rrhh         TO erp_readonly;
GRANT SELECT ON SCHEMA::inventario   TO erp_readonly;
GRANT SELECT ON SCHEMA::produccion   TO erp_readonly;
GRANT SELECT ON SCHEMA::comercial    TO erp_readonly;
GRANT SELECT ON SCHEMA::reportes     TO erp_readonly;
GRANT SELECT ON SCHEMA::integracion  TO erp_readonly;
GO

DENY UPDATE, DELETE ON nucleo.auditoria TO erp_app;
GRANT INSERT, SELECT ON nucleo.auditoria TO erp_app;
GO

-- ----------------------------------------------------------------------------
-- TRIGGERS DE SEGURIDAD (RF-12, RF-18, RF-03)
-- ----------------------------------------------------------------------------

CREATE TRIGGER contabilidad.trg_bloquear_edicion_asiento
ON contabilidad.asiento_contable
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN inserted i ON i.id = d.id
        WHERE d.estado = 'CONTABILIZADO' AND i.estado <> 'REVERTIDO'
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50012, 'RF-12: un asiento CONTABILIZADO no puede editarse. Genere un asiento de reversion.', 1;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM deleted d
        WHERE d.estado = 'CONTABILIZADO'
          AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.id = d.id)
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50013, 'RF-12: un asiento CONTABILIZADO no puede eliminarse. Genere un asiento de reversion.', 1;
        RETURN;
    END
END
GO

CREATE TRIGGER contabilidad.trg_bloquear_edicion_asiento_detalle
ON contabilidad.asiento_detalle
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN contabilidad.asiento_contable ac ON ac.id = d.asiento_id
        WHERE ac.estado = 'CONTABILIZADO'
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50014, 'RF-12: no se pueden alterar lineas de un asiento CONTABILIZADO.', 1;
        RETURN;
    END
END
GO

CREATE TRIGGER contabilidad.trg_validar_periodo_abierto
ON contabilidad.asiento_contable
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN contabilidad.periodo_contable p ON p.id = i.periodo_id
        WHERE p.estado IN ('CERRADO', 'BLOQUEADO')
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50018, 'No se pueden crear asientos en un periodo CERRADO o BLOQUEADO.', 1;
        RETURN;
    END
END
GO

CREATE TRIGGER contabilidad.trg_auditar_asiento_contable
ON contabilidad.asiento_contable
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsuarioId UNIQUEIDENTIFIER = TRY_CAST(SESSION_CONTEXT(N'current_user_id') AS UNIQUEIDENTIFIER);
    DECLARE @Canal NVARCHAR(20) = COALESCE(TRY_CAST(SESSION_CONTEXT(N'current_channel') AS NVARCHAR(20)), 'ERP');
    DECLARE @NombreUsuario NVARCHAR(50) = COALESCE((SELECT nombre_usuario FROM nucleo.usuario WHERE id = @UsuarioId), 'SISTEMA');

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'INSERT', 'contabilidad.asiento_contable', CAST(i.id AS NVARCHAR(100)), j.valores_nuevos, @Canal
    FROM inserted i
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_nuevos)
    WHERE NOT EXISTS (SELECT 1 FROM deleted d WHERE d.id = i.id);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'UPDATE', 'contabilidad.asiento_contable', CAST(i.id AS NVARCHAR(100)), jo.valores_anteriores, jn.valores_nuevos, @Canal
    FROM inserted i
    JOIN deleted d ON d.id = i.id
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jo(valores_anteriores)
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jn(valores_nuevos);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, canal)
    SELECT @UsuarioId, @NombreUsuario, 'DELETE', 'contabilidad.asiento_contable', CAST(d.id AS NVARCHAR(100)), j.valores_anteriores, @Canal
    FROM deleted d
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_anteriores)
    WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.id = d.id);
END
GO

CREATE TRIGGER comercial.trg_auditar_factura_venta
ON comercial.factura_venta
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsuarioId UNIQUEIDENTIFIER = TRY_CAST(SESSION_CONTEXT(N'current_user_id') AS UNIQUEIDENTIFIER);
    DECLARE @Canal NVARCHAR(20) = COALESCE(TRY_CAST(SESSION_CONTEXT(N'current_channel') AS NVARCHAR(20)), 'ERP');
    DECLARE @NombreUsuario NVARCHAR(50) = COALESCE((SELECT nombre_usuario FROM nucleo.usuario WHERE id = @UsuarioId), 'SISTEMA');

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'INSERT', 'comercial.factura_venta', CAST(i.id AS NVARCHAR(100)), j.valores_nuevos, @Canal
    FROM inserted i
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_nuevos)
    WHERE NOT EXISTS (SELECT 1 FROM deleted d WHERE d.id = i.id);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'UPDATE', 'comercial.factura_venta', CAST(i.id AS NVARCHAR(100)), jo.valores_anteriores, jn.valores_nuevos, @Canal
    FROM inserted i
    JOIN deleted d ON d.id = i.id
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jo(valores_anteriores)
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jn(valores_nuevos);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, canal)
    SELECT @UsuarioId, @NombreUsuario, 'DELETE', 'comercial.factura_venta', CAST(d.id AS NVARCHAR(100)), j.valores_anteriores, @Canal
    FROM deleted d
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_anteriores)
    WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.id = d.id);
END
GO

CREATE TRIGGER rrhh.trg_auditar_nomina_detalle
ON rrhh.nomina_detalle
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsuarioId UNIQUEIDENTIFIER = TRY_CAST(SESSION_CONTEXT(N'current_user_id') AS UNIQUEIDENTIFIER);
    DECLARE @Canal NVARCHAR(20) = COALESCE(TRY_CAST(SESSION_CONTEXT(N'current_channel') AS NVARCHAR(20)), 'ERP');
    DECLARE @NombreUsuario NVARCHAR(50) = COALESCE((SELECT nombre_usuario FROM nucleo.usuario WHERE id = @UsuarioId), 'SISTEMA');

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'INSERT', 'rrhh.nomina_detalle', CAST(i.id AS NVARCHAR(100)), j.valores_nuevos, @Canal
    FROM inserted i
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_nuevos)
    WHERE NOT EXISTS (SELECT 1 FROM deleted d WHERE d.id = i.id);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, valores_nuevos, canal)
    SELECT @UsuarioId, @NombreUsuario, 'UPDATE', 'rrhh.nomina_detalle', CAST(i.id AS NVARCHAR(100)), jo.valores_anteriores, jn.valores_nuevos, @Canal
    FROM inserted i
    JOIN deleted d ON d.id = i.id
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jo(valores_anteriores)
    CROSS APPLY (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) jn(valores_nuevos);

    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, accion, esquema_tabla, registro_id, valores_anteriores, canal)
    SELECT @UsuarioId, @NombreUsuario, 'DELETE', 'rrhh.nomina_detalle', CAST(d.id AS NVARCHAR(100)), j.valores_anteriores, @Canal
    FROM deleted d
    CROSS APPLY (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES) j(valores_anteriores)
    WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.id = d.id);
END
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description',
    @value=N'La aplicacion debe ejecutar sp_set_session_context para current_user_id y current_channel al inicio de cada conexion/transaccion, para que la auditoria capture usuario y canal (ERP/API/POS) reales.',
    @level0type=N'SCHEMA', @level0name=N'contabilidad',
    @level1type=N'TABLE', @level1name=N'asiento_contable',
    @level2type=N'TRIGGER', @level2name=N'trg_auditar_asiento_contable';
GO