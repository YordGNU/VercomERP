/* ============================================================
   VercomERP – Datos Demo
   Ejecutar DESPUÉS de cuentas-mfp.sql
   Alineado 1:1 con el DDL real
   ============================================================ */
USE [VercomERP];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO


/* ============================================================
   1. PERMISOS (catálogo global)
   ============================================================ */
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'CONT.CUENTA.VER')
BEGIN
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('CONT.CUENTA.VER','CONTABILIDAD','Ver plan de cuentas'),
    ('CONT.CUENTA.EDIT','CONTABILIDAD','Crear/editar cuentas'),
    ('CONT.ASIENTO.VER','CONTABILIDAD','Ver asientos'),
    ('CONT.ASIENTO.CREAR','CONTABILIDAD','Registrar asientos'),
    ('CONT.PERIODO.CERRAR','CONTABILIDAD','Cerrar períodos'),
    ('CONT.REPORTE.VER','CONTABILIDAD','Ver reportes contables'),
    ('NUCLEO.USUARIO.ADMIN','NUCLEO','Administrar usuarios'),
    ('NUCLEO.PARAM.ADMIN','NUCLEO','Administrar parámetros'),
    ('NUCLEO.AUDIT.VER','NUCLEO','Ver log de auditoría'),
    ('COM.CLIENTE.ADMIN','COMERCIAL','Administrar clientes'),
    ('COM.FACTURA.CREAR','COMERCIAL','Emitir facturas'),
    ('INV.PRODUCTO.ADMIN','INVENTARIO','Administrar productos'),
    ('INV.MOVIMIENTO.CREAR','INVENTARIO','Registrar movimientos'),
    ('RRHH.TRABAJADOR.ADMIN','RRHH','Administrar trabajadores'),
    ('RRHH.NOMINA.CREAR','RRHH','Procesar nóminas');
    PRINT '✔ Permisos cargados.';
END
GO


/* ============================================================
   2. ROLES – Entidad DEMO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM nucleo.rol WHERE entidad_id = @E)
BEGIN
    INSERT INTO nucleo.rol (codigo, nombre, descripcion, es_sistema, entidad_id, creado_en)
    VALUES
    ('ADMIN','Administrador','Acceso total al sistema',1,@E,SYSDATETIMEOFFSET()),
    ('CONT','Contador','Módulo contable y reportes',0,@E,SYSDATETIMEOFFSET()),
    ('CAJA','Cajero','Registro de movimientos de caja',0,@E,SYSDATETIMEOFFSET()),
    ('ALM','Almacenero','Inventario y almacenes',0,@E,SYSDATETIMEOFFSET()),
    ('RRHH','RRHH','Nóminas y trabajadores',0,@E,SYSDATETIMEOFFSET()),
    ('AUD','Auditor','Solo lectura + auditoría',0,@E,SYSDATETIMEOFFSET());
    PRINT '✔ Roles creados.';
END
GO


/* ============================================================
   3. ROL ↔ PERMISO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

-- Administrador: todos los permisos
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r CROSS JOIN nucleo.permiso p
WHERE r.entidad_id = @E AND r.codigo = 'ADMIN'
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp
                  WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- Contador
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r JOIN nucleo.permiso p ON p.codigo IN
    ('CONT.CUENTA.VER','CONT.CUENTA.EDIT','CONT.ASIENTO.VER','CONT.ASIENTO.CREAR',
     'CONT.PERIODO.CERRAR','CONT.REPORTE.VER')
WHERE r.entidad_id = @E AND r.codigo = 'CONT'
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp
                  WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);

-- Cajero
INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT r.id, p.id
FROM nucleo.rol r JOIN nucleo.permiso p ON p.codigo IN
    ('CONT.CUENTA.VER','CONT.ASIENTO.VER','CONT.ASIENTO.CREAR','COM.FACTURA.CREAR')
WHERE r.entidad_id = @E AND r.codigo = 'CAJA'
  AND NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp
                  WHERE rp.rol_id = r.id AND rp.permiso_id = p.id);
GO


/* ============================================================
   4. USUARIOS DEMO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM nucleo.usuario WHERE entidad_id = @E)
BEGIN
    INSERT INTO nucleo.usuario
        (id, entidad_id, sucursal_id, nombre_usuario, nombre_completo, email,
         hash_password, debe_cambiar_pass, intentos_fallidos,
         bloqueado_hasta, ultimo_login, activo, es_empleado_id,
         creado_en, actualizado_en)
    VALUES
    ('33333333-0000-0000-0000-000000000001', @E, @S, 'admin',
     'Administrador del Sistema','admin@tierraprometida.cu',
     'REEMPLAZAR_HASH_ADMIN', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),

    ('33333333-0000-0000-0000-000000000002', @E, @S, 'contador',
     'María Pérez Gómez','contador@tierraprometida.cu',
     'REEMPLAZAR_HASH_CONTADOR', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),

    ('33333333-0000-0000-0000-000000000003', @E, @S, 'cajero',
     'Juan Rodríguez López','cajero@tierraprometida.cu',
     'REEMPLAZAR_HASH_CAJERO', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),

    ('33333333-0000-0000-0000-000000000004', @E, @S, 'almacenero',
     'Carlos Fernández Ruiz','almacen@tierraprometida.cu',
     'REEMPLAZAR_HASH_ALM', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),

    ('33333333-0000-0000-0000-000000000005', @E, @S, 'rrhh',
     'Ana González Sánchez','rrhh@tierraprometida.cu',
     'REEMPLAZAR_HASH_RRHH', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),

    ('33333333-0000-0000-0000-000000000006', @E, @S, 'auditor',
     'Roberto Díaz Castro','auditor@tierraprometida.cu',
     'REEMPLAZAR_HASH_AUD', 1, 0, NULL, NULL, 1, NULL,
     SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
    PRINT '✔ Usuarios demo creados.';
END
GO


/* ============================================================
   5. USUARIO ↔ ROL
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @UAdm UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM nucleo.usuario_rol)
BEGIN
    INSERT INTO nucleo.usuario_rol (usuario_id, rol_id, sucursal_id, asignado_en, asignado_por)
    SELECT u.id, r.id, @S, SYSDATETIMEOFFSET(), @UAdm
    FROM nucleo.usuario u
    JOIN nucleo.rol r ON r.entidad_id = @E AND r.codigo = CASE u.nombre_usuario
        WHEN 'admin'      THEN 'ADMIN'
        WHEN 'contador'   THEN 'CONT'
        WHEN 'cajero'     THEN 'CAJA'
        WHEN 'almacenero' THEN 'ALM'
        WHEN 'rrhh'       THEN 'RRHH'
        WHEN 'auditor'    THEN 'AUD'
    END
    WHERE u.entidad_id = @E;
    PRINT '✔ Roles asignados a usuarios.';
END
GO


/* ============================================================
   6. PARÁMETROS DEL SISTEMA
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @E)
BEGIN
    INSERT INTO nucleo.parametro_sistema
        (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde, vigente_hasta)
    VALUES
    (@E,'empresa.nombre','Tierra Prometida S.U.R.L.','STRING','Razón social','2025-01-01',NULL),
    (@E,'empresa.nit','DEMO00000000','STRING','NIT','2025-01-01',NULL),
    (@E,'contabilidad.moneda_base','CUP','STRING','Moneda funcional','2025-01-01',NULL),
    (@E,'contabilidad.tasa_iva','0.18','NUMERIC','Tasa IVA','2025-01-01',NULL),
    (@E,'contabilidad.periodo_actual','2025-01','STRING','Período vigente','2025-01-01',NULL),
    (@E,'seguridad.max_intentos','5','NUMERIC','Intentos antes de bloquear','2025-01-01',NULL),
    (@E,'seguridad.minutos_bloqueo','15','NUMERIC','Minutos de bloqueo','2025-01-01',NULL),
    (@E,'pos.sincronizacion_offline','true','BIT','POS offline','2025-01-01',NULL);
    PRINT '✔ Parámetros cargados.';
END
GO


/* ============================================================
   7. CONSECUTIVOS (nucleo.consecutivo)
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM nucleo.consecutivo WHERE entidad_id = @E)
BEGIN
    INSERT INTO nucleo.consecutivo
        (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero,
         longitud_padding, actualizado_en)
    VALUES
    (@E, @S, 'ASIENTO_APE',  'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'ASIENTO_ING',  'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'ASIENTO_EGR',  'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'ASIENTO_DIAR', 'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'ASIENTO_AJU',  'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'ASIENTO_CIE',  'A', 0, 8, SYSDATETIMEOFFSET()),
    (@E, @S, 'FACTURA_VENTA','A', 0, 8, SYSDATETIMEOFFSET());
    PRINT '✔ Consecutivos creados.';
END
GO


/* ============================================================
   8. CLIENTES Y PROVEEDORES
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM comercial.cliente WHERE entidad_id = @E)
BEGIN
    INSERT INTO comercial.cliente
        (entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion,
         telefono, email, segmento, limite_credito, activo, creado_en)
    VALUES
    (@E,'JURIDICA','ABC123456789','Mayorista ABC S.A.','Centro Habana',
     '+53 7 800 0001','abc@mail.cu','MAYORISTA',100000,1,SYSDATETIMEOFFSET()),
    (@E,'JURIDICA','XYZ987654321','Comercial XYZ','Vedado',
     '+53 7 800 0002','xyz@mail.cu','MINORISTA',50000,1,SYSDATETIMEOFFSET()),
    (@E,'JURIDICA','DN000111222','Distribuidora Nacional','Miramar',
     '+53 7 800 0003','dist@mail.cu','MAYORISTA',200000,1,SYSDATETIMEOFFSET()),
    (@E,'NATURAL',NULL,'CONSUMIDOR FINAL',NULL,NULL,NULL,'MINORISTA',0,1,SYSDATETIMEOFFSET());
    PRINT '✔ Clientes creados.';
END
GO

DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM comercial.proveedor WHERE entidad_id = @E)
BEGIN
    INSERT INTO comercial.proveedor
        (entidad_id, tipo_persona, nit, razon_social, direccion, telefono,
         email, activo, creado_en)
    VALUES
    (@E,'JURIDICA','SUP111222333','Suministros Industriales','Habana Vieja',
     '+53 7 900 0001','prov1@mail.cu',1,SYSDATETIMEOFFSET()),
    (@E,'JURIDICA','MAT444555666','Materiales de Construcción','Cerro',
     '+53 7 900 0002','prov2@mail.cu',1,SYSDATETIMEOFFSET());
    PRINT '✔ Proveedores creados.';
END
GO


/* ============================================================
   9. UNIDADES DE MEDIDA
   ============================================================ */
IF NOT EXISTS (SELECT 1 FROM inventario.unidad_medida WHERE codigo = 'UND')
BEGIN
    INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES
    ('UND','Unidad',0),
    ('KG','Kilogramo',1),
    ('LT','Litro',1),
    ('MT','Metro',1),
    ('KIT','Kit',0),
    ('CAJ','Caja',0);
    PRINT '✔ Unidades de medida cargadas.';
END
GO


/* ============================================================
   10. ALMACENES, FAMILIAS, PRODUCTOS
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM inventario.almacen WHERE entidad_id = @E)
BEGIN
    INSERT INTO inventario.almacen
        (entidad_id, sucursal_id, codigo, nombre, es_punto_venta, activo)
    VALUES
    (@E, @S,'ALM-01','Almacén Central',0,1),
    (@E, @S,'ALM-02','Almacén Materias Primas',0,1),
    (@E, @S,'ALM-03','Punto de Venta Vedado',1,1);
    PRINT '✔ Almacenes creados.';
END
GO

DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE entidad_id = @E)
BEGIN
    INSERT INTO inventario.familia_producto (entidad_id, codigo, nombre, familia_padre_id)
    VALUES
    (@E,'FAM-01','Productos Terminados',NULL),
    (@E,'FAM-02','Materias Primas',NULL),
    (@E,'FAM-03','Suministros',NULL);
    PRINT '✔ Familias creadas.';
END
GO

DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE entidad_id = @E)
BEGIN
    DECLARE @FamProd UNIQUEIDENTIFIER, @FamMP UNIQUEIDENTIFIER, @FamSum UNIQUEIDENTIFIER;
    DECLARE @UM_UND INT, @UM_KG INT, @UM_KIT INT;

    SELECT @FamProd = id FROM inventario.familia_producto WHERE entidad_id=@E AND codigo='FAM-01';
    SELECT @FamMP   = id FROM inventario.familia_producto WHERE entidad_id=@E AND codigo='FAM-02';
    SELECT @FamSum  = id FROM inventario.familia_producto WHERE entidad_id=@E AND codigo='FAM-03';
    SELECT @UM_UND  = id FROM inventario.unidad_medida WHERE codigo='UND';
    SELECT @UM_KG   = id FROM inventario.unidad_medida WHERE codigo='KG';
    SELECT @UM_KIT  = id FROM inventario.unidad_medida WHERE codigo='KIT';

    INSERT INTO inventario.producto
        (entidad_id, codigo, nombre, familia_id, unidad_medida_id, tipo,
         precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
    VALUES
    (@E,'PROD-001','Producto A',@FamProd,@UM_UND,'TERMINADO',118.00,1,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET()),
    (@E,'PROD-002','Producto B',@FamProd,@UM_UND,'TERMINADO',177.00,1,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET()),
    (@E,'MP-001','Materia Prima X',@FamMP,@UM_KG,'MATERIA_PRIMA',0.00,0,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET()),
    (@E,'SUM-001','Kit Útiles Oficina',@FamSum,@UM_KIT,'MERCANCIA',29.50,1,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET());
    PRINT '✔ Productos creados.';
END
GO


/* ============================================================
   11. SP HELPER – crear asiento contable
   ============================================================ */
CREATE OR ALTER PROCEDURE contabilidad.sp_crear_asiento
    @EntidadId      UNIQUEIDENTIFIER,
    @SucursalId     UNIQUEIDENTIFIER,
    @TipoCodigo     NVARCHAR(10),
    @Fecha          DATE,
    @Concepto       NVARCHAR(MAX),
    @ModuloOrigen   NVARCHAR(30),
    @UsuarioId      UNIQUEIDENTIFIER,
    @Lineas         NVARCHAR(MAX)   -- JSON [{cuenta, debe, haber, glosa}]
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) Tipo comprobante
    DECLARE @TipoId INT;
    SELECT @TipoId = id FROM contabilidad.tipo_comprobante WHERE codigo = @TipoCodigo;
    IF @TipoId IS NULL
    BEGIN
        RAISERROR('Tipo comprobante %s no existe', 16, 1, @TipoCodigo);
        RETURN;
    END

    -- 2) Período
    DECLARE @PeriodoId UNIQUEIDENTIFIER;
    SELECT @PeriodoId = id
    FROM contabilidad.periodo_contable
    WHERE entidad_id = @EntidadId
      AND anio = YEAR(@Fecha) AND mes = MONTH(@Fecha);
    IF @PeriodoId IS NULL
    BEGIN
        RAISERROR('No hay período contable para %s', 16, 1, CONVERT(VARCHAR(10),@Fecha,120));
        RETURN;
    END

    -- 3) Consecutivo
    DECLARE @Num BIGINT;
    DECLARE @TipoDoc NVARCHAR(40) = 'ASIENTO_' + @TipoCodigo;

    UPDATE nucleo.consecutivo
       SET @Num = ultimo_numero = ultimo_numero + 1,
           actualizado_en = SYSDATETIMEOFFSET()
     WHERE entidad_id = @EntidadId
       AND sucursal_id = @SucursalId
       AND tipo_documento = @TipoDoc
       AND serie = 'A';

    IF @Num IS NULL
    BEGIN
        SET @Num = 1;
        INSERT INTO nucleo.consecutivo
            (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero,
             longitud_padding, actualizado_en)
        VALUES (@EntidadId, @SucursalId, @TipoDoc, 'A', 1, 8, SYSDATETIMEOFFSET());
    END

    -- 4) Calcular totales y validar cuadre ANTES de insertar
    DECLARE @TD DECIMAL(18,2) = 0, @TH DECIMAL(18,2) = 0;
    SELECT  @TD = ISNULL(SUM(CAST(JSON_VALUE(j.value,'$.debe')  AS DECIMAL(18,2))),0),
            @TH = ISNULL(SUM(CAST(JSON_VALUE(j.value,'$.haber') AS DECIMAL(18,2))),0)
    FROM OPENJSON(@Lineas) j;

    IF @TD <> @TH
    BEGIN
        RAISERROR('El asiento no cuadra: D=%.2f H=%.2f', 16, 1, @TD, @TH);
        RETURN;
    END

    -- 5) Insertar asiento
    DECLARE @Aid UNIQUEIDENTIFIER = NEWID();

    INSERT INTO contabilidad.asiento_contable
        (id, entidad_id, sucursal_id, periodo_id, tipo_comprobante_id,
         numero_comprobante, fecha, concepto, modulo_origen,
         total_debe, total_haber, estado, creado_por, creado_en)
    VALUES
        (@Aid, @EntidadId, @SucursalId, @PeriodoId, @TipoId,
         @Num, @Fecha, @Concepto, @ModuloOrigen,
         @TD, @TH, 'CONTABILIZADO', @UsuarioId, SYSDATETIMEOFFSET());

    -- 6) Insertar líneas
    INSERT INTO contabilidad.asiento_detalle
        (id, asiento_id, linea, cuenta_id, debe, haber, glosa)
    SELECT  NEWID(), @Aid,
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
            c.id,
            CAST(JSON_VALUE(j.value,'$.debe')  AS DECIMAL(18,2)),
            CAST(JSON_VALUE(j.value,'$.haber') AS DECIMAL(18,2)),
            JSON_VALUE(j.value,'$.glosa')
    FROM OPENJSON(@Lineas) j
    JOIN contabilidad.cuenta_contable c
      ON c.entidad_id = @EntidadId
     AND c.codigo     = JSON_VALUE(j.value,'$.cuenta')
     AND c.acepta_movimiento = 1;
END
GO


/* ============================================================
   12. ASIENTOS DEMO
   ============================================================ */
DECLARE @E   UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S   UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @UAdm UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000001';
DECLARE @UCon UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000002';
DECLARE @UCaj UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000003';

-- APERTURA 2025
EXEC contabilidad.sp_crear_asiento @E, @S, 'APE', '2025-01-02',
     'Apertura de ejercicio 2025', 'CONTABILIDAD', @UAdm,
     N'[{"cuenta":"11001","debe":500000,"haber":0,"glosa":"Caja M/N"},
        {"cuenta":"31001","debe":0,"haber":500000,"glosa":"Aporte capital"}]';

-- VENTA CONTADO
EXEC contabilidad.sp_crear_asiento @E, @S, 'ING', '2025-01-05',
     'Venta de mercancías al contado', 'VENTAS', @UCaj,
     N'[{"cuenta":"11001","debe":23600,"haber":0,"glosa":"Cobro caja"},
        {"cuenta":"41002","debe":0,"haber":20000,"glosa":"Venta"},
        {"cuenta":"21101","debe":0,"haber":3600,"glosa":"IVA 18%"}]';

-- COMPRA A CRÉDITO
EXEC contabilidad.sp_crear_asiento @E, @S, 'EGR', '2025-01-08',
     'Compra de materiales al crédito', 'COMPRAS', @UCaj,
     N'[{"cuenta":"11204","debe":15000,"haber":0,"glosa":"Materiales"},
        {"cuenta":"21101","debe":2700,"haber":0,"glosa":"IVA crédito"},
        {"cuenta":"21001","debe":0,"haber":17700,"glosa":"Factura F-001"}]';

-- SERVICIOS PÚBLICOS
EXEC contabilidad.sp_crear_asiento @E, @S, 'EGR', '2025-01-15',
     'Pago de servicios públicos', 'CONTABILIDAD', @UCaj,
     N'[{"cuenta":"51103","debe":3500,"haber":0,"glosa":"Electricidad/agua"},
        {"cuenta":"11003","debe":0,"haber":3500,"glosa":"Transferencia"}]';

-- SERVICIOS AL CRÉDITO
EXEC contabilidad.sp_crear_asiento @E, @S, 'ING', '2025-01-20',
     'Servicios técnicos al crédito', 'VENTAS', @UCaj,
     N'[{"cuenta":"11101","debe":35400,"haber":0,"glosa":"CxC cliente"},
        {"cuenta":"41102","debe":0,"haber":30000,"glosa":"Servicios"},
        {"cuenta":"21101","debe":0,"haber":5400,"glosa":"IVA 18%"}]';

-- PLANILLA ENERO
EXEC contabilidad.sp_crear_asiento @E, @S, 'DIAR', '2025-01-31',
     'Planilla enero 2025', 'NOMINA', @UCon,
     N'[{"cuenta":"51001","debe":80000,"haber":0,"glosa":"Sueldos"},
        {"cuenta":"51002","debe":5000,"haber":0,"glosa":"Gratificaciones"},
        {"cuenta":"51004","debe":10400,"haber":0,"glosa":"Seg. Social"},
        {"cuenta":"21201","debe":0,"haber":70000,"glosa":"Sueldos netos"},
        {"cuenta":"21301","debe":0,"haber":10400,"glosa":"Seg. Social"},
        {"cuenta":"21103","debe":0,"haber":15000,"glosa":"Retenciones"}]';
GO

DECLARE @E   UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S   UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @UCon UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000002';
DECLARE @UCaj UNIQUEIDENTIFIER = '33333333-0000-0000-0000-000000000003';

-- COBRO FACTURA
EXEC contabilidad.sp_crear_asiento @E, @S, 'ING', '2025-02-03',
     'Cobro de factura cliente', 'VENTAS', @UCaj,
     N'[{"cuenta":"11003","debe":35400,"haber":0,"glosa":"Abono bancario"},
        {"cuenta":"11101","debe":0,"haber":35400,"glosa":"CxC cancelada"}]';

-- VENTA CONTADO
EXEC contabilidad.sp_crear_asiento @E, @S, 'ING', '2025-02-07',
     'Venta al contado', 'VENTAS', @UCaj,
     N'[{"cuenta":"11001","debe":47200,"haber":0,"glosa":"Cobro caja"},
        {"cuenta":"41001","debe":0,"haber":40000,"glosa":"Venta productos"},
        {"cuenta":"21101","debe":0,"haber":7200,"glosa":"IVA 18%"}]';

-- PAGO PROVEEDOR
EXEC contabilidad.sp_crear_asiento @E, @S, 'EGR', '2025-02-12',
     'Pago a proveedores', 'COMPRAS', @UCaj,
     N'[{"cuenta":"21001","debe":17700,"haber":0,"glosa":"Pago factura"},
        {"cuenta":"11003","debe":0,"haber":17700,"glosa":"Transferencia"}]';

-- DEPRECIACIÓN
EXEC contabilidad.sp_crear_asiento @E, @S, 'AJU', '2025-02-28',
     'Depreciación mensual', 'CONTABILIDAD', @UCon,
     N'[{"cuenta":"52101","debe":2500,"haber":0,"glosa":"Dep. edificios"},
        {"cuenta":"52102","debe":1800,"haber":0,"glosa":"Dep. maquinaria"},
        {"cuenta":"12107","debe":0,"haber":4300,"glosa":"Dep. acumulada"}]';

-- PLANILLA FEBRERO
EXEC contabilidad.sp_crear_asiento @E, @S, 'DIAR', '2025-02-28',
     'Planilla febrero 2025', 'NOMINA', @UCon,
     N'[{"cuenta":"51001","debe":80000,"haber":0,"glosa":"Sueldos"},
        {"cuenta":"51004","debe":10400,"haber":0,"glosa":"Seg. Social"},
        {"cuenta":"21201","debe":0,"haber":75400,"glosa":"Sueldos netos"},
        {"cuenta":"21301","debe":0,"haber":10400,"glosa":"Seg. Social"},
        {"cuenta":"21103","debe":0,"haber":4600,"glosa":"Retenciones"}]';
GO


/* ============================================================
   13. VERIFICACIÓN
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

PRINT '=================================================';
PRINT ' Datos demo cargados';
PRINT '=================================================';

SELECT 'roles'       AS entidad, COUNT(*) AS n FROM nucleo.rol               WHERE entidad_id = @E
UNION ALL SELECT 'usuarios',    COUNT(*) FROM nucleo.usuario                WHERE entidad_id = @E
UNION ALL SELECT 'parametros',  COUNT(*) FROM nucleo.parametro_sistema      WHERE entidad_id = @E
UNION ALL SELECT 'consecutivos',COUNT(*) FROM nucleo.consecutivo            WHERE entidad_id = @E
UNION ALL SELECT 'clientes',    COUNT(*) FROM comercial.cliente             WHERE entidad_id = @E
UNION ALL SELECT 'proveedores', COUNT(*) FROM comercial.proveedor           WHERE entidad_id = @E
UNION ALL SELECT 'almacenes',   COUNT(*) FROM inventario.almacen            WHERE entidad_id = @E
UNION ALL SELECT 'productos',   COUNT(*) FROM inventario.producto           WHERE entidad_id = @E
UNION ALL SELECT 'cuentas',     COUNT(*) FROM contabilidad.cuenta_contable  WHERE entidad_id = @E
UNION ALL SELECT 'periodos',    COUNT(*) FROM contabilidad.periodo_contable WHERE entidad_id = @E
UNION ALL SELECT 'asientos',    COUNT(*) FROM contabilidad.asiento_contable WHERE entidad_id = @E
UNION ALL SELECT 'lineas',      COUNT(*) FROM contabilidad.asiento_detalle;
GO

DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
SELECT a.numero_comprobante, a.fecha, a.tipo_comprobante_id, a.concepto,
       a.total_debe, a.total_haber,
       CASE WHEN a.total_debe = a.total_haber THEN 'OK' ELSE 'DESCUADRADO' END AS cuadre
FROM contabilidad.asiento_contable a
WHERE a.entidad_id = @E
ORDER BY a.fecha, a.numero_comprobante;
GO

PRINT '✔ Script demo finalizado.';
GO