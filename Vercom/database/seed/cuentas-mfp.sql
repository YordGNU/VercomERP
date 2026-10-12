/* ============================================================
   VercomERP – Plan de Cuentas MFP
   Ejecutar DESPUÉS de 01-schema.sql
   Alineado 1:1 con el DDL real (VercomERP)
   ============================================================ */
USE [VercomERP];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   1. ENTIDAD DEMO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER;

SELECT @E = id FROM nucleo.entidad WHERE nit = 'DEMO00000000';

IF @E IS NULL
BEGIN
    SET @E = '11111111-1111-1111-1111-111111111111';
    INSERT INTO nucleo.entidad
        (id, razon_social, nombre_comercial, nit, codigo_reeup, forma_juridica,
         direccion_legal, municipio, provincia, telefono, email,
         moneda_base, activo, creado_en, actualizado_en)
    VALUES
        (@E, 'Tierra Prometida S.U.R.L.', 'Tierra Prometida', 'DEMO00000000',
         'REEUP-DEMO-001', 'S.U.R.L.',
         'Calle 23 #456, Vedado', 'Plaza de la Revolución', 'La Habana',
         '+53 7 832 0000', 'info@tierraprometida.cu',
         'CUP', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
    PRINT '✔ Entidad DEMO creada.';
END
ELSE
    PRINT 'ℹ️  Entidad DEMO ya existe.';
GO


/* ============================================================
   2. SUCURSAL PRINCIPAL
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @S UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM nucleo.sucursal WHERE id = @S)
BEGIN
    INSERT INTO nucleo.sucursal
        (id, entidad_id, codigo, nombre, tipo, direccion, municipio, provincia,
         telefono, activo, creado_en)
    VALUES
        (@S, @E, 'SUC-01', 'Casa Matriz – Vedado', 'OFICINA',
         'Calle 23 #456, Vedado', 'Plaza de la Revolución', 'La Habana',
         '+53 7 832 0000', 1, SYSDATETIMEOFFSET());
    PRINT '✔ Sucursal principal creada.';
END
GO


/* ============================================================
   3. TIPOS DE COMPROBANTE
   ============================================================ */
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'ING')
BEGIN
    INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES
    ('APE','Apertura de Ejercicio'),
    ('ING','Comprobante de Ingreso'),
    ('EGR','Comprobante de Egreso'),
    ('DIAR','Asiento de Diario'),
    ('AJU','Ajuste Contable'),
    ('CIE','Cierre de Ejercicio');
    PRINT '✔ Tipos de comprobante cargados.';
END
GO


/* ============================================================
   4. PERÍODOS CONTABLES 2025
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM contabilidad.periodo_contable WHERE entidad_id = @E)
BEGIN
    INSERT INTO contabilidad.periodo_contable
        (id, entidad_id, anio, mes, fecha_inicio, fecha_fin, estado, creado_en)
    VALUES
    (NEWID(), @E, 2025,  1,'2025-01-01','2025-01-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  2,'2025-02-01','2025-02-28','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  3,'2025-03-01','2025-03-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  4,'2025-04-01','2025-04-30','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  5,'2025-05-01','2025-05-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  6,'2025-06-01','2025-06-30','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  7,'2025-07-01','2025-07-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  8,'2025-08-01','2025-08-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025,  9,'2025-09-01','2025-09-30','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025, 10,'2025-10-01','2025-10-31','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025, 11,'2025-11-01','2025-11-30','ABIERTO',SYSDATETIMEOFFSET()),
    (NEWID(), @E, 2025, 12,'2025-12-01','2025-12-31','ABIERTO',SYSDATETIMEOFFSET());
    PRINT '✔ Períodos 2025 cargados.';
END
GO


/* ============================================================
   5. SP HELPER – insertar cuenta contable
   ============================================================ */
CREATE OR ALTER PROCEDURE contabilidad.sp_ins_cuenta
    @EntidadId  UNIQUEIDENTIFIER,
    @Codigo     NVARCHAR(20),
    @Nombre     NVARCHAR(200),
    @Clase      NVARCHAR(20),
    @Nat        CHAR(1),              -- 'D' o 'A'
    @Nivel      SMALLINT,
    @PadreCod   NVARCHAR(20) = NULL,
    @Acepta     BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PadreId UNIQUEIDENTIFIER = NULL;
    DECLARE @Naturaleza NVARCHAR(10) =
        CASE @Nat WHEN 'D' THEN 'DEUDORA' WHEN 'A' THEN 'ACREEDORA' ELSE @Nat END;

    IF @PadreCod IS NOT NULL
    BEGIN
        SELECT @PadreId = id
        FROM contabilidad.cuenta_contable
        WHERE entidad_id = @EntidadId AND codigo = @PadreCod;

        IF @PadreId IS NULL
        BEGIN
            RAISERROR('Cuenta padre %s no existe para la entidad', 16, 1, @PadreCod);
            RETURN;
        END
    END

    IF EXISTS (SELECT 1 FROM contabilidad.cuenta_contable
               WHERE entidad_id = @EntidadId AND codigo = @Codigo)
        RETURN;

    INSERT INTO contabilidad.cuenta_contable
        (id, entidad_id, codigo, nombre, cuenta_padre_id, nivel, clase,
         naturaleza, acepta_movimiento, requiere_centro_costo, requiere_tercero,
         moneda, activo, creado_en)
    VALUES
        (NEWID(), @EntidadId, @Codigo, @Nombre, @PadreId, @Nivel, @Clase,
         @Naturaleza, @Acepta, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());
END
GO


/* ============================================================
   6. NIVEL 1 – CLASES (7 registros: 6 clases + 6 = GASTOS)
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable
               WHERE entidad_id = @E AND nivel = 1)
BEGIN
    INSERT INTO contabilidad.cuenta_contable
        (id, entidad_id, codigo, nombre, cuenta_padre_id, nivel, clase,
         naturaleza, acepta_movimiento, requiere_centro_costo, requiere_tercero,
         moneda, activo, creado_en)
    VALUES
    (NEWID(), @E,'1','ACTIVO','ACTIVO','DEUDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'2','PASIVO','PASIVO','ACREEDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'3','PATRIMONIO','PATRIMONIO','ACREEDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'4','INGRESOS','INGRESOS','ACREEDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'5','GASTOS','GASTOS','DEUDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'6','COSTOS DE PRODUCCIÓN','GASTOS','DEUDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET()),
    (NEWID(), @E,'7','CUENTAS DE ORDEN','ORDEN','DEUDORA',NULL,1,0,0,0,'CUP',1,SYSDATETIMEOFFSET());
    PRINT '✔ Nivel 1 cargado.';
END
GO


/* ============================================================
   7. CLASE 1 – ACTIVO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

-- Nivel 2
EXEC contabilidad.sp_ins_cuenta @E,'11','ACTIVO CIRCULANTE','ACTIVO','D',2,'1';
EXEC contabilidad.sp_ins_cuenta @E,'12','ACTIVO NO CIRCULANTE','ACTIVO','D',2,'1';

-- Nivel 3
EXEC contabilidad.sp_ins_cuenta @E,'110','Efectivo en Caja y Banco','ACTIVO','D',3,'11';
EXEC contabilidad.sp_ins_cuenta @E,'111','Cuentas por Cobrar','ACTIVO','D',3,'11';
EXEC contabilidad.sp_ins_cuenta @E,'112','Inventarios','ACTIVO','D',3,'11';
EXEC contabilidad.sp_ins_cuenta @E,'113','Inversiones Financieras','ACTIVO','D',3,'11';
EXEC contabilidad.sp_ins_cuenta @E,'114','Gastos Pagados por Anticipado','ACTIVO','D',3,'11';
EXEC contabilidad.sp_ins_cuenta @E,'120','Inversiones Financieras L/P','ACTIVO','D',3,'12';
EXEC contabilidad.sp_ins_cuenta @E,'121','Propiedad, Planta y Equipo','ACTIVO','D',3,'12';
EXEC contabilidad.sp_ins_cuenta @E,'122','Activos Intangibles','ACTIVO','D',3,'12';
EXEC contabilidad.sp_ins_cuenta @E,'123','Activos Biológicos','ACTIVO','D',3,'12';

-- Nivel 4 (imputables)
EXEC contabilidad.sp_ins_cuenta @E,'11001','Caja M/N','ACTIVO','D',4,'110',1;
EXEC contabilidad.sp_ins_cuenta @E,'11002','Caja M/E','ACTIVO','D',4,'110',1;
EXEC contabilidad.sp_ins_cuenta @E,'11003','Banco M/N','ACTIVO','D',4,'110',1;
EXEC contabilidad.sp_ins_cuenta @E,'11004','Banco M/E','ACTIVO','D',4,'110',1;
EXEC contabilidad.sp_ins_cuenta @E,'11101','Cuentas por Cobrar Comerciales','ACTIVO','D',4,'111',1;
EXEC contabilidad.sp_ins_cuenta @E,'11102','Cuentas por Cobrar Diversas','ACTIVO','D',4,'111',1;
EXEC contabilidad.sp_ins_cuenta @E,'11103','Cuentas por Cobrar al Personal','ACTIVO','D',4,'111',1;
EXEC contabilidad.sp_ins_cuenta @E,'11104','Provisión Cuentas Incobrables','ACTIVO','A',4,'111',1;
EXEC contabilidad.sp_ins_cuenta @E,'11201','Mercancías para la Venta','ACTIVO','D',4,'112',1;
EXEC contabilidad.sp_ins_cuenta @E,'11202','Materias Primas','ACTIVO','D',4,'112',1;
EXEC contabilidad.sp_ins_cuenta @E,'11203','Producción Terminada','ACTIVO','D',4,'112',1;
EXEC contabilidad.sp_ins_cuenta @E,'11204','Materiales y Suministros','ACTIVO','D',4,'112',1;
EXEC contabilidad.sp_ins_cuenta @E,'11205','Producción en Proceso','ACTIVO','D',4,'112',1;
EXEC contabilidad.sp_ins_cuenta @E,'12101','Terrenos','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12102','Edificaciones','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12103','Maquinaria y Equipo','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12104','Muebles y Enseres','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12105','Equipos de Cómputo','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12106','Vehículos','ACTIVO','D',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12107','Depreciación Acumulada','ACTIVO','A',4,'121',1;
EXEC contabilidad.sp_ins_cuenta @E,'12201','Software','ACTIVO','D',4,'122',1;
EXEC contabilidad.sp_ins_cuenta @E,'12202','Licencias','ACTIVO','D',4,'122',1;
EXEC contabilidad.sp_ins_cuenta @E,'12203','Amortización Acumulada','ACTIVO','A',4,'122',1;
GO


/* ============================================================
   8. CLASE 2 – PASIVO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'21','PASIVO CIRCULANTE','PASIVO','A',2,'2';
EXEC contabilidad.sp_ins_cuenta @E,'22','PASIVO NO CIRCULANTE','PASIVO','A',2,'2';
EXEC contabilidad.sp_ins_cuenta @E,'210','Cuentas por Pagar','PASIVO','A',3,'21';
EXEC contabilidad.sp_ins_cuenta @E,'211','Obligaciones Fiscales','PASIVO','A',3,'21';
EXEC contabilidad.sp_ins_cuenta @E,'212','Remuneraciones por Pagar','PASIVO','A',3,'21';
EXEC contabilidad.sp_ins_cuenta @E,'213','Obligaciones Previsionales','PASIVO','A',3,'21';
EXEC contabilidad.sp_ins_cuenta @E,'220','Deudas a Largo Plazo','PASIVO','A',3,'22';

EXEC contabilidad.sp_ins_cuenta @E,'21001','Facturas por Pagar','PASIVO','A',4,'210',1;
EXEC contabilidad.sp_ins_cuenta @E,'21002','Cuentas por Pagar Diversas','PASIVO','A',4,'210',1;
EXEC contabilidad.sp_ins_cuenta @E,'21101','IVA por Pagar','PASIVO','A',4,'211',1;
EXEC contabilidad.sp_ins_cuenta @E,'21102','Impuesto sobre Utilidades','PASIVO','A',4,'211',1;
EXEC contabilidad.sp_ins_cuenta @E,'21103','Retenciones por Pagar','PASIVO','A',4,'211',1;
EXEC contabilidad.sp_ins_cuenta @E,'21104','Contribución Territorial','PASIVO','A',4,'211',1;
EXEC contabilidad.sp_ins_cuenta @E,'21201','Sueldos por Pagar','PASIVO','A',4,'212',1;
EXEC contabilidad.sp_ins_cuenta @E,'21202','Vacaciones por Pagar','PASIVO','A',4,'212',1;
EXEC contabilidad.sp_ins_cuenta @E,'21301','Seguridad Social por Pagar','PASIVO','A',4,'213',1;
EXEC contabilidad.sp_ins_cuenta @E,'21302','Contribución Especial por Pagar','PASIVO','A',4,'213',1;
EXEC contabilidad.sp_ins_cuenta @E,'22001','Préstamos Bancarios L/P','PASIVO','A',4,'220',1;
EXEC contabilidad.sp_ins_cuenta @E,'22002','Empréstitos','PASIVO','A',4,'220',1;
GO


/* ============================================================
   9. CLASE 3 – PATRIMONIO
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'31','Capital','PATRIMONIO','A',2,'3';
EXEC contabilidad.sp_ins_cuenta @E,'32','Reservas','PATRIMONIO','A',2,'3';
EXEC contabilidad.sp_ins_cuenta @E,'33','Resultados Acumulados','PATRIMONIO','A',2,'3';
EXEC contabilidad.sp_ins_cuenta @E,'34','Resultados del Ejercicio','PATRIMONIO','A',2,'3';
EXEC contabilidad.sp_ins_cuenta @E,'310','Capital Social','PATRIMONIO','A',3,'31';
EXEC contabilidad.sp_ins_cuenta @E,'320','Reserva Legal','PATRIMONIO','A',3,'32';
EXEC contabilidad.sp_ins_cuenta @E,'330','Utilidades No Distribuidas','PATRIMONIO','A',3,'33';
EXEC contabilidad.sp_ins_cuenta @E,'331','Pérdidas Acumuladas','PATRIMONIO','D',3,'33';
EXEC contabilidad.sp_ins_cuenta @E,'340','Utilidad del Ejercicio','PATRIMONIO','A',3,'34';
EXEC contabilidad.sp_ins_cuenta @E,'341','Pérdida del Ejercicio','PATRIMONIO','D',3,'34';

EXEC contabilidad.sp_ins_cuenta @E,'31001','Aportes de Capital','PATRIMONIO','A',4,'310',1;
EXEC contabilidad.sp_ins_cuenta @E,'31002','Capital Estatal','PATRIMONIO','A',4,'310',1;
EXEC contabilidad.sp_ins_cuenta @E,'32001','Reserva Legal 10%','PATRIMONIO','A',4,'320',1;
EXEC contabilidad.sp_ins_cuenta @E,'33001','Utilidades Retenidas','PATRIMONIO','A',4,'330',1;
EXEC contabilidad.sp_ins_cuenta @E,'33101','Pérdidas de Ejercicios Anteriores','PATRIMONIO','D',4,'331',1;
GO


/* ============================================================
   10. CLASE 4 – INGRESOS
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'41','Ingresos Operacionales','INGRESOS','A',2,'4';
EXEC contabilidad.sp_ins_cuenta @E,'42','Ingresos No Operacionales','INGRESOS','A',2,'4';
EXEC contabilidad.sp_ins_cuenta @E,'43','Transferencias','INGRESOS','A',2,'4';
EXEC contabilidad.sp_ins_cuenta @E,'410','Venta de Bienes','INGRESOS','A',3,'41';
EXEC contabilidad.sp_ins_cuenta @E,'411','Prestación de Servicios','INGRESOS','A',3,'41';
EXEC contabilidad.sp_ins_cuenta @E,'412','Tasas y Derechos','INGRESOS','A',3,'41';
EXEC contabilidad.sp_ins_cuenta @E,'420','Ingresos Financieros','INGRESOS','A',3,'42';
EXEC contabilidad.sp_ins_cuenta @E,'421','Ingresos Extraordinarios','INGRESOS','A',3,'42';

EXEC contabilidad.sp_ins_cuenta @E,'41001','Venta de Productos Terminados','INGRESOS','A',4,'410',1;
EXEC contabilidad.sp_ins_cuenta @E,'41002','Venta de Mercancías','INGRESOS','A',4,'410',1;
EXEC contabilidad.sp_ins_cuenta @E,'41101','Servicios Administrativos','INGRESOS','A',4,'411',1;
EXEC contabilidad.sp_ins_cuenta @E,'41102','Servicios Técnicos','INGRESOS','A',4,'411',1;
EXEC contabilidad.sp_ins_cuenta @E,'41103','Servicios de Transporte','INGRESOS','A',4,'411',1;
EXEC contabilidad.sp_ins_cuenta @E,'41201','Licencias de Funcionamiento','INGRESOS','A',4,'412',1;
EXEC contabilidad.sp_ins_cuenta @E,'42001','Intereses Ganados','INGRESOS','A',4,'420',1;
EXEC contabilidad.sp_ins_cuenta @E,'42101','Venta de Activos Fijos','INGRESOS','A',4,'421',1;
GO


/* ============================================================
   11. CLASE 5 – GASTOS
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'51','Gastos Operacionales','GASTOS','D',2,'5';
EXEC contabilidad.sp_ins_cuenta @E,'52','Gastos Administrativos','GASTOS','D',2,'5';
EXEC contabilidad.sp_ins_cuenta @E,'53','Gastos Financieros','GASTOS','D',2,'5';
EXEC contabilidad.sp_ins_cuenta @E,'54','Gastos No Operacionales','GASTOS','D',2,'5';
EXEC contabilidad.sp_ins_cuenta @E,'510','Gastos de Personal','GASTOS','D',3,'51';
EXEC contabilidad.sp_ins_cuenta @E,'511','Servicios de Terceros','GASTOS','D',3,'51';
EXEC contabilidad.sp_ins_cuenta @E,'512','Materiales y Suministros','GASTOS','D',3,'51';
EXEC contabilidad.sp_ins_cuenta @E,'520','Gastos Administrativos Diversos','GASTOS','D',3,'52';
EXEC contabilidad.sp_ins_cuenta @E,'521','Depreciaciones y Amortizaciones','GASTOS','D',3,'52';
EXEC contabilidad.sp_ins_cuenta @E,'530','Intereses y Gastos Financieros','GASTOS','D',3,'53';
EXEC contabilidad.sp_ins_cuenta @E,'540','Gastos Extraordinarios','GASTOS','D',3,'54';

EXEC contabilidad.sp_ins_cuenta @E,'51001','Sueldos y Salarios','GASTOS','D',4,'510',1;
EXEC contabilidad.sp_ins_cuenta @E,'51002','Gratificaciones','GASTOS','D',4,'510',1;
EXEC contabilidad.sp_ins_cuenta @E,'51003','Bonificaciones','GASTOS','D',4,'510',1;
EXEC contabilidad.sp_ins_cuenta @E,'51004','Aportes y Contribuciones','GASTOS','D',4,'510',1;
EXEC contabilidad.sp_ins_cuenta @E,'51005','Vacaciones','GASTOS','D',4,'510',1;
EXEC contabilidad.sp_ins_cuenta @E,'51101','Servicios de Consultoría','GASTOS','D',4,'511',1;
EXEC contabilidad.sp_ins_cuenta @E,'51102','Servicios de Mantenimiento','GASTOS','D',4,'511',1;
EXEC contabilidad.sp_ins_cuenta @E,'51103','Servicios Públicos','GASTOS','D',4,'511',1;
EXEC contabilidad.sp_ins_cuenta @E,'51104','Alquileres','GASTOS','D',4,'511',1;
EXEC contabilidad.sp_ins_cuenta @E,'51105','Servicios de Transporte','GASTOS','D',4,'511',1;
EXEC contabilidad.sp_ins_cuenta @E,'51201','Útiles de Oficina','GASTOS','D',4,'512',1;
EXEC contabilidad.sp_ins_cuenta @E,'51202','Combustibles y Lubricantes','GASTOS','D',4,'512',1;
EXEC contabilidad.sp_ins_cuenta @E,'51203','Vestuario y Calzado','GASTOS','D',4,'512',1;
EXEC contabilidad.sp_ins_cuenta @E,'51204','Materiales de Construcción','GASTOS','D',4,'512',1;
EXEC contabilidad.sp_ins_cuenta @E,'52001','Gastos Administrativos Menores','GASTOS','D',4,'520',1;
EXEC contabilidad.sp_ins_cuenta @E,'52002','Publicidad y Propaganda','GASTOS','D',4,'520',1;
EXEC contabilidad.sp_ins_cuenta @E,'52003','Capacitación','GASTOS','D',4,'520',1;
EXEC contabilidad.sp_ins_cuenta @E,'52101','Depreciación de Inmuebles','GASTOS','D',4,'521',1;
EXEC contabilidad.sp_ins_cuenta @E,'52102','Depreciación de Maquinaria','GASTOS','D',4,'521',1;
EXEC contabilidad.sp_ins_cuenta @E,'52103','Amortización de Intangibles','GASTOS','D',4,'521',1;
EXEC contabilidad.sp_ins_cuenta @E,'53001','Intereses Bancarios','GASTOS','D',4,'530',1;
EXEC contabilidad.sp_ins_cuenta @E,'53002','Diferencial Cambiario','GASTOS','D',4,'530',1;
EXEC contabilidad.sp_ins_cuenta @E,'54001','Pérdidas por Siniestros','GASTOS','D',4,'540',1;
EXEC contabilidad.sp_ins_cuenta @E,'54002','Pérdidas por Robo','GASTOS','D',4,'540',1;
GO


/* ============================================================
   12. CLASE 6 – COSTOS DE PRODUCCIÓN (clase = 'GASTOS')
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'61','Costo de Producción','GASTOS','D',2,'6';
EXEC contabilidad.sp_ins_cuenta @E,'62','Costo de Distribución','GASTOS','D',2,'6';
EXEC contabilidad.sp_ins_cuenta @E,'610','Materia Prima Directa','GASTOS','D',3,'61';
EXEC contabilidad.sp_ins_cuenta @E,'611','Mano de Obra Directa','GASTOS','D',3,'61';
EXEC contabilidad.sp_ins_cuenta @E,'612','Costos Indirectos de Fabricación','GASTOS','D',3,'61';
EXEC contabilidad.sp_ins_cuenta @E,'620','Gastos de Distribución','GASTOS','D',3,'62';

EXEC contabilidad.sp_ins_cuenta @E,'61001','Materias Primas','GASTOS','D',4,'610',1;
EXEC contabilidad.sp_ins_cuenta @E,'61002','Materiales Auxiliares','GASTOS','D',4,'610',1;
EXEC contabilidad.sp_ins_cuenta @E,'61101','Sueldos Directos','GASTOS','D',4,'611',1;
EXEC contabilidad.sp_ins_cuenta @E,'61102','Seguridad Social Directa','GASTOS','D',4,'611',1;
EXEC contabilidad.sp_ins_cuenta @E,'61201','Energía Eléctrica','GASTOS','D',4,'612',1;
EXEC contabilidad.sp_ins_cuenta @E,'61202','Depreciación Fabril','GASTOS','D',4,'612',1;
EXEC contabilidad.sp_ins_cuenta @E,'62001','Fletes y Transporte','GASTOS','D',4,'620',1;
EXEC contabilidad.sp_ins_cuenta @E,'62002','Comisiones de Venta','GASTOS','D',4,'620',1;
GO


/* ============================================================
   13. CLASE 7 – CUENTAS DE ORDEN
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

EXEC contabilidad.sp_ins_cuenta @E,'71','Cuentas de Orden Deudoras','ORDEN','D',2,'7';
EXEC contabilidad.sp_ins_cuenta @E,'72','Cuentas de Orden Acreedoras','ORDEN','A',2,'7';
EXEC contabilidad.sp_ins_cuenta @E,'710','Bienes en Custodia','ORDEN','D',3,'71';
EXEC contabilidad.sp_ins_cuenta @E,'711','Garantías Recibidas','ORDEN','D',3,'71';
EXEC contabilidad.sp_ins_cuenta @E,'720','Contingencias','ORDEN','A',3,'72';
EXEC contabilidad.sp_ins_cuenta @E,'721','Compromisos','ORDEN','A',3,'72';

EXEC contabilidad.sp_ins_cuenta @E,'71001','Bienes Recibidos en Custodia','ORDEN','D',4,'710',1;
EXEC contabilidad.sp_ins_cuenta @E,'71101','Cartas Fianza Recibidas','ORDEN','D',4,'711',1;
EXEC contabilidad.sp_ins_cuenta @E,'72001','Contingencias Laborales','ORDEN','A',4,'720',1;
EXEC contabilidad.sp_ins_cuenta @E,'72002','Contingencias Tributarias','ORDEN','A',4,'720',1;
EXEC contabilidad.sp_ins_cuenta @E,'72101','Compromisos Contractuales','ORDEN','A',4,'721',1;
GO


/* ============================================================
   14. VERIFICACIÓN
   ============================================================ */
DECLARE @E UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

PRINT '=================================================';
PRINT ' Plan de cuentas MFP cargado';
PRINT '=================================================';

SELECT clase, nivel, COUNT(*) AS total
FROM contabilidad.cuenta_contable
WHERE entidad_id = @E
GROUP BY clase, nivel
ORDER BY clase, nivel;

SELECT COUNT(*) AS total_imputables
FROM contabilidad.cuenta_contable
WHERE entidad_id = @E AND acepta_movimiento = 1;
GO

PRINT '✔ Script finalizado.';
GO