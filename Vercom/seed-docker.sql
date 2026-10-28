-- ==============================================================================
-- SEED DE DATOS INICIALES - VERCOM ERP + POS (Docker / Linux / SQL Server)
-- Ejecutar DESPUÉS de init-docker.sql
-- Genera una demo completamente funcional (ERP + POS terminal + Inventarios + Caja)
-- ==============================================================================

USE [VERCOMERP]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION;

-- ==============================================================================
-- 1. ENTIDAD BASE Y SUCURSAL CENTRAL
-- ==============================================================================
DECLARE @EntidadId UNIQUEIDENTIFIER = '24C50C75-26A7-4BAD-A81D-C4D680F4CDF6';
DECLARE @SucursalId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM nucleo.entidad WHERE id = @EntidadId)
BEGIN
    INSERT INTO nucleo.entidad (id, razon_social, nombre_comercial, nit, codigo_reeup, forma_juridica, direccion_legal, municipio, provincia, telefono, email, moneda_base, activo, creado_en, actualizado_en)
    VALUES (@EntidadId, 'VERCOM ERP Demo S.U.R.L.', 'VERCOM ERP Demo', '12345678901', 'REEUP2026001', 'S.U.R.L.', 'Calle Martí #123', 'Holguín', 'Holguín', '+53 24 123456', 'demo@vercom.cu', 'CUP', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
END

IF NOT EXISTS (SELECT 1 FROM nucleo.sucursal WHERE id = @SucursalId)
BEGIN
    INSERT INTO nucleo.sucursal (id, entidad_id, codigo, nombre, tipo, direccion, municipio, provincia, telefono, activo, creado_en)
    VALUES (@SucursalId, @EntidadId, 'MATRIZ', 'Sucursal Central', 'MIXTO', 'Calle Martí #123', 'Holguín', 'Holguín', '+53 24 123456', 1, SYSDATETIMEOFFSET());
END

-- ==============================================================================
-- 2. PLAN DE CUENTAS CONTABLES BÁSICO
-- ==============================================================================
DECLARE @CtaCajaId UNIQUEIDENTIFIER = '10100000-0000-0000-0000-000000000001';
DECLARE @CtaBancoId UNIQUEIDENTIFIER = '10900000-0000-0000-0000-000000000001';
DECLARE @CtaCxcId UNIQUEIDENTIFIER = '13500000-0000-0000-0000-000000000001';
DECLARE @CtaInvId UNIQUEIDENTIFIER = '18300000-0000-0000-0000-000000000183';
DECLARE @CtaWipId UNIQUEIDENTIFIER = '18500000-0000-0000-0000-000000000185';
DECLARE @CtaCxpId UNIQUEIDENTIFIER = '40500000-0000-0000-0000-000000000405';
DECLARE @CtaImpId UNIQUEIDENTIFIER = '44000000-0000-0000-0000-000000000440';
DECLARE @CtaCostoId UNIQUEIDENTIFIER = '81000000-0000-0000-0000-000000000810';
DECLARE @CtaGastoId UNIQUEIDENTIFIER = '82200000-0000-0000-0000-000000000822';
DECLARE @CtaPerdidaId UNIQUEIDENTIFIER = '85000000-0000-0000-0000-000000000850';
DECLARE @CtaVentasId UNIQUEIDENTIFIER = '90000000-0000-0000-0000-000000000900';
DECLARE @CtaAjusteId UNIQUEIDENTIFIER = '93000000-0000-0000-0000-000000000930';

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '101.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaCajaId, @EntidadId, '101.0000', 'Efectivo en Caja', 'ACTIVO', 'DEUDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '109.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaBancoId, @EntidadId, '109.0000', 'Efectivo en Banco', 'ACTIVO', 'DEUDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '135.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaCxcId, @EntidadId, '135.0000', 'Cuentas por Cobrar Clientes', 'ACTIVO', 'DEUDORA', 1, 1, 0, 1, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '183.0010')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaInvId, @EntidadId, '183.0010', 'Inventario de Mercancías', 'ACTIVO', 'DEUDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '185.0010')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaWipId, @EntidadId, '185.0010', 'Productos en Proceso (WIP)', 'ACTIVO', 'DEUDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '405.0020')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaCxpId, @EntidadId, '405.0020', 'Cuentas por Pagar Proveedores', 'PASIVO', 'ACREEDORA', 1, 1, 0, 1, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '440.0001')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaImpId, @EntidadId, '440.0001', 'Impuesto sobre Ventas Por Pagar', 'PASIVO', 'ACREEDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '810.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaCostoId, @EntidadId, '810.0000', 'Costo de Ventas DE PRODUCCIONES', 'GASTOS', 'DEUDORA', 1, 1, 1, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '822.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaGastoId, @EntidadId, '822.0000', 'Gastos Generales y de Administración', 'GASTOS', 'DEUDORA', 1, 1, 1, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '850.0010')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaPerdidaId, @EntidadId, '850.0010', 'Pérdida por Faltantes e Inventario', 'GASTOS', 'DEUDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '900.0000')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaVentasId, @EntidadId, '900.0000', 'Ventas de Producción / Mercancías', 'INGRESOS', 'ACREEDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '930.0010')
    INSERT INTO contabilidad.cuenta_contable (id, entidad_id, codigo, nombre, clase, naturaleza, nivel, acepta_movimiento, requiere_centro_costo, requiere_tercero, moneda, activo, creado_en) VALUES (@CtaAjusteId, @EntidadId, '930.0010', 'Ingreso por Ajuste de Inventario', 'INGRESOS', 'ACREEDORA', 1, 1, 0, 0, 'CUP', 1, SYSDATETIMEOFFSET());

-- ==============================================================================
-- 3. TIPOS DE COMPROBANTE Y PERIODO CONTABLE ABIERTO
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'VEN') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('VEN', 'Comprobante de Ventas');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'COM') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('COM', 'Comprobante de Compras');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'ING') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('ING', 'Comprobante de Ingreso');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'EGR') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('EGR', 'Comprobante de Egreso');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'DIA') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('DIA', 'Comprobante Diario');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'REC') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('REC', 'Recepción de Mercancía');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'AJU') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('AJU', 'Ajuste Contable');
IF NOT EXISTS (SELECT 1 FROM contabilidad.tipo_comprobante WHERE codigo = 'NOM') INSERT INTO contabilidad.tipo_comprobante (codigo, nombre) VALUES ('NOM', 'Nómina');

DECLARE @PeriodoId UNIQUEIDENTIFIER = 'B3129200-0000-0000-0000-000000002026';
DECLARE @AnioActual SMALLINT = YEAR(GETDATE());
DECLARE @MesActual SMALLINT = MONTH(GETDATE());

IF NOT EXISTS (SELECT 1 FROM contabilidad.periodo_contable WHERE entidad_id = @EntidadId AND anio = @AnioActual AND mes = @MesActual)
BEGIN
    INSERT INTO contabilidad.periodo_contable (id, entidad_id, anio, mes, fecha_inicio, fecha_fin, estado, creado_en, actualizado_en)
    VALUES (@PeriodoId, @EntidadId, @AnioActual, @MesActual, DATEFROMPARTS(@AnioActual, @MesActual, 1), EOMONTH(DATEFROMPARTS(@AnioActual, @MesActual, 1)), 'ABIERTO', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
END

-- ==============================================================================
-- 4. CAJAS Y CUENTAS BANCARIAS
-- ==============================================================================
DECLARE @CajaId UNIQUEIDENTIFIER = '1A58673B-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM contabilidad.caja WHERE id = @CajaId)
BEGIN
    INSERT INTO contabilidad.caja (id, entidad_id, sucursal_id, nombre, cuenta_contable_id, limite_efectivo, saldo_actual, activa)
    VALUES (@CajaId, @EntidadId, @SucursalId, 'CAJA POS DEV / PRINCIPAL', @CtaCajaId, 50000.00, 1000.00, 1);
END

IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_bancaria WHERE entidad_id = @EntidadId AND numero_cuenta = '0684123456789012')
BEGIN
    INSERT INTO contabilidad.cuenta_bancaria (id, entidad_id, banco, numero_cuenta, tipo_cuenta, cuenta_contable_id, moneda, saldo_actual, activa)
    VALUES (NEWID(), @EntidadId, 'Banco Metropolitano', '0684123456789012', 'CORRIENTE', @CtaBancoId, 'CUP', 100000.00, 1);
END

-- ==============================================================================
-- 5. ALMACENES Y UNIDADES DE MEDIDA
-- ==============================================================================
DECLARE @AlmacenCentralId UNIQUEIDENTIFIER = 'E4397C1D-0000-0000-0000-000000000001';
DECLARE @AlmacenTiendaId UNIQUEIDENTIFIER = 'E4397C1D-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM inventario.unidad_medida WHERE codigo = 'UN') INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES ('UN', 'Unidad', 0);
IF NOT EXISTS (SELECT 1 FROM inventario.unidad_medida WHERE codigo = 'KG') INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES ('KG', 'Kilogramo', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.unidad_medida WHERE codigo = 'L') INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES ('L', 'Litro', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.unidad_medida WHERE codigo = 'PQT') INSERT INTO inventario.unidad_medida (codigo, nombre, es_fraccionable) VALUES ('PQT', 'Paquete', 0);

IF NOT EXISTS (SELECT 1 FROM inventario.almacen WHERE id = @AlmacenCentralId)
BEGIN
    INSERT INTO inventario.almacen (id, entidad_id, sucursal_id, codigo, nombre, es_punto_venta, activo)
    VALUES (@AlmacenCentralId, @EntidadId, @SucursalId, 'ALM01', 'ALMACEN CENTRAL', 0, 1);
END

IF NOT EXISTS (SELECT 1 FROM inventario.almacen WHERE id = @AlmacenTiendaId)
BEGIN
    INSERT INTO inventario.almacen (id, entidad_id, sucursal_id, codigo, nombre, es_punto_venta, activo)
    VALUES (@AlmacenTiendaId, @EntidadId, @SucursalId, 'TIENDA01', 'Tienda Principal', 1, 1);
END

-- Tipos de Movimiento de Inventario
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'RECEPCION') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('RECEPCION', 'Recepción de Compra', 'ENTRADA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'VALE_ENTREGA') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('VALE_ENTREGA', 'Vale de Entrega', 'SALIDA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'CONSUMO_PRODUCCION') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('CONSUMO_PRODUCCION', 'Consumo en Producción', 'SALIDA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'ENTRADA_PRODUCCION') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('ENTRADA_PRODUCCION', 'Entrada PT', 'ENTRADA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'DEVOLUCION_PRODUCCION') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('DEVOLUCION_PRODUCCION', 'Devolución MP', 'ENTRADA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'VENTA_POS') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('VENTA_POS', 'Venta POS', 'SALIDA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'AJUSTE_POSITIVO') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('AJUSTE_POSITIVO', 'Ajuste por Sobrante', 'ENTRADA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'AJUSTE_NEGATIVO') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('AJUSTE_NEGATIVO', 'Ajuste por Faltante', 'SALIDA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'DEVOLUCION_SALIDA') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('DEVOLUCION_SALIDA', 'Devolución a Proveedor', 'SALIDA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'DEVOLUCION_ENTRADA') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('DEVOLUCION_ENTRADA', 'Devolución de Cliente', 'ENTRADA', 1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'TRANSFERENCIA_SALIDA') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('TRANSFERENCIA_SALIDA', 'Transferencia - Salida', 'SALIDA', 0);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo = 'TRANSFERENCIA_ENTRADA') INSERT INTO inventario.tipo_movimiento (codigo, nombre, naturaleza, afecta_costo) VALUES ('TRANSFERENCIA_ENTRADA', 'Transferencia - Entrada', 'ENTRADA', 0);

-- ==============================================================================
-- 6. PERMISOS Y ROLES DE SEGURIDAD (ERP + POS)
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM nucleo.permiso WHERE codigo = 'SEGURIDAD.USUARIO.VER')
BEGIN
    INSERT INTO nucleo.permiso (codigo, modulo, descripcion) VALUES
    ('SEGURIDAD.USUARIO.VER', 'Seguridad', 'Ver usuarios'),
    ('SEGURIDAD.USUARIO.CREAR', 'Seguridad', 'Crear usuarios'),
    ('SEGURIDAD.USUARIO.EDITAR', 'Seguridad', 'Editar usuarios'),
    ('SEGURIDAD.ROL.VER', 'Seguridad', 'Ver roles'),
    ('SEGURIDAD.ROL.EDITAR', 'Seguridad', 'Editar roles'),
    ('SEGURIDAD.AUDITORIA.VER', 'Seguridad', 'Ver auditoría'),
    ('ADMIN.ENTIDAD.VER', 'Administración', 'Ver entidad'),
    ('ADMIN.ENTIDAD.EDITAR', 'Administración', 'Editar entidad'),
    ('ADMIN.PARAMETROS.VER', 'Administración', 'Ver parámetros'),
    ('ADMIN.PARAMETROS.EDITAR', 'Administración', 'Editar parámetros'),
    ('CONTABILIDAD.CUENTA.VER', 'Contabilidad', 'Ver cuentas'),
    ('CONTABILIDAD.ASIENTO.VER', 'Contabilidad', 'Ver asientos'),
    ('CONTABILIDAD.ASIENTO.CREAR', 'Contabilidad', 'Crear asientos'),
    ('CONTABILIDAD.ACTIVO_FIJO.CREAR', 'Contabilidad', 'Crear activos fijos'),
    ('CONTABILIDAD.ACTIVO_FIJO.EDITAR', 'Contabilidad', 'Editar activos fijos'),
    ('CONTABILIDAD.ACTIVO_FIJO.ELIMINAR', 'Contabilidad', 'Eliminar activos fijos'),
    ('CONTABILIDAD.CUENTA.ELIMINAR', 'Contabilidad', 'Eliminar cuentas contables'),
    ('INVENTARIO.PRODUCTO.VER', 'Inventario', 'Ver productos'),
    ('INVENTARIO.PRODUCTO.CREAR', 'Inventario', 'Crear productos'),
    ('INVENTARIO.PRODUCTO.EDITAR', 'Inventario', 'Editar productos'),
    ('INVENTARIO.MOVIMIENTO.VER', 'Inventario', 'Ver movimientos'),
    ('INVENTARIO.MOVIMIENTO.CREAR', 'Inventario', 'Crear movimientos'),
    ('INVENTARIO.EXISTENCIA.VER', 'Inventario', 'Ver existencias'),
    ('INVENTARIO.FAMILIA.CREAR', 'Inventario', 'Crear familias'),
    ('INVENTARIO.FAMILIA.EDITAR', 'Inventario', 'Editar familias'),
    ('INVENTARIO.FAMILIA.ELIMINAR', 'Inventario', 'Eliminar familias'),
    ('INVENTARIO.FAMILIA.VER', 'Inventario', 'Ver familias'),
    ('INVENTARIO.LISTA_PRECIO.CREAR', 'Inventario', 'Crear listas de precio'),
    ('INVENTARIO.LISTA_PRECIO.EDITAR', 'Inventario', 'Editar listas de precio'),
    ('INVENTARIO.LISTA_PRECIO.VER', 'Inventario', 'Ver listas de precio'),
    ('INVENTARIO.ALMACEN.CREAR', 'Inventario', 'Crear almacenes'),
    ('INVENTARIO.ALMACEN.VER', 'Inventario', 'Ver almacenes'),
    ('COMERCIAL.CLIENTE.VER', 'Comercial', 'Ver clientes'),
    ('COMERCIAL.CLIENTE.CREAR', 'Comercial', 'Crear clientes'),
    ('COMERCIAL.CLIENTE.EDITAR', 'Comercial', 'Editar clientes'),
    ('COMERCIAL.FACTURA_VENTA.VER', 'Comercial', 'Ver facturas'),
    ('COMERCIAL.FACTURA_VENTA.CREAR', 'Comercial', 'Crear facturas'),
    ('COMERCIAL.CONTRATO.CREAR', 'Comercial', 'Crear contratos'),
    ('COMERCIAL.CONTRATO.EDITAR', 'Comercial', 'Editar contratos'),
    ('COMERCIAL.PROVEEDOR.CREAR', 'Comercial', 'Crear proveedores'),
    ('COMERCIAL.PROVEEDOR.EDITAR', 'Comercial', 'Editar proveedores'),
    ('POS.CAJA', 'POS', 'Acceso POS'),
    ('POS.SESION.ABRIR', 'POS', 'Abrir sesión'),
    ('POS.SESION.CERRAR', 'POS', 'Cerrar sesión'),
    ('vender', 'POS', 'Realizar ventas en el terminal POS'),
    ('consultar_productos', 'POS', 'Consultar catálogo'),
    ('ver_inventario', 'POS', 'Ver existencias'),
    ('gestionar_inventario', 'POS', 'Gestionar inventario'),
    ('cerrar_caja', 'POS', 'Cerrar caja'),
    ('aplicar_descuento', 'POS', 'Aplicar descuentos'),
    ('editar_producto', 'POS', 'Editar productos'),
    ('config_general', 'POS', 'Config general'),
    ('config_identidad', 'POS', 'Config identidad'),
    ('config_pagos', 'POS', 'Config pagos'),
    ('procesar_devolucion', 'POS', 'Procesar devoluciones'),
    ('gestionar_usuarios', 'POS', 'Gestionar usuarios'),
    ('configuracion', 'POS', 'Configuración completa'),
    ('ver_reportes', 'POS', 'Ver reportes'),
    ('REPORTES.INDICADORES.VER', 'Reportes', 'Ver dashboard');
END

-- Roles
IF NOT EXISTS (SELECT 1 FROM nucleo.rol WHERE codigo = 'MASTER')
    INSERT INTO nucleo.rol (codigo, nombre, descripcion, es_sistema, entidad_id, creado_en) VALUES ('MASTER', 'Administrador Global', 'Acceso total al ecosistema', 1, @EntidadId, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM nucleo.rol WHERE codigo = 'ADMINISTRADOR')
    INSERT INTO nucleo.rol (codigo, nombre, descripcion, es_sistema, entidad_id, creado_en) VALUES ('ADMINISTRADOR', 'Administrador de Entidad', 'Acceso total a la entidad', 1, @EntidadId, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM nucleo.rol WHERE codigo = 'CAJERO_POS')
    INSERT INTO nucleo.rol (codigo, nombre, descripcion, es_sistema, entidad_id, creado_en) VALUES ('CAJERO_POS', 'Cajero POS', 'Operación de caja y ventas', 1, @EntidadId, SYSDATETIMEOFFSET());

-- Permisos a MASTER y ADMINISTRADOR
DECLARE @MasterRolId INT = (SELECT id FROM nucleo.rol WHERE codigo = 'MASTER');
DECLARE @AdminRolId INT = (SELECT id FROM nucleo.rol WHERE codigo = 'ADMINISTRADOR');

INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT @MasterRolId, p.id FROM nucleo.permiso p
WHERE NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = @MasterRolId AND rp.permiso_id = p.id);

INSERT INTO nucleo.rol_permiso (rol_id, permiso_id)
SELECT @AdminRolId, p.id FROM nucleo.permiso p
WHERE NOT EXISTS (SELECT 1 FROM nucleo.rol_permiso rp WHERE rp.rol_id = @AdminRolId AND rp.permiso_id = p.id);

-- ==============================================================================
-- 7. USUARIOS (master / MasterVercom2026*, admin / admin123*, cajero / cajero123*)
-- ==============================================================================
DECLARE @MasterUserId UNIQUEIDENTIFIER = '3933AD33-9E58-49D7-9A52-9F33A87EACF6';
DECLARE @AdminUserId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @CajeroUserId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

-- Hash de BCrypt para "MasterVercom2026*"
IF NOT EXISTS (SELECT 1 FROM nucleo.usuario WHERE id = @MasterUserId OR nombre_usuario = 'master')
BEGIN
    INSERT INTO nucleo.usuario (id, entidad_id, sucursal_id, nombre_usuario, nombre_completo, email, hash_password, debe_cambiar_pass, intentos_fallidos, activo, creado_en, actualizado_en)
    VALUES (@MasterUserId, @EntidadId, @SucursalId, 'master', 'Usuario Maestro', 'master@vercom.cu', '$2a$11$OkFgxLuZFfzuxCBDWXguAuTHKCX1UrNFRxmuoMJha8Y6W1g49aVEW', 0, 0, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

    INSERT INTO nucleo.usuario_rol (usuario_id, rol_id, sucursal_id, asignado_en)
    VALUES (@MasterUserId, @MasterRolId, @SucursalId, SYSDATETIMEOFFSET());
END

-- Hash de BCrypt para "admin123*"
IF NOT EXISTS (SELECT 1 FROM nucleo.usuario WHERE id = @AdminUserId OR nombre_usuario = 'admin')
BEGIN
    INSERT INTO nucleo.usuario (id, entidad_id, sucursal_id, nombre_usuario, nombre_completo, email, hash_password, debe_cambiar_pass, intentos_fallidos, activo, creado_en, actualizado_en)
    VALUES (@AdminUserId, @EntidadId, @SucursalId, 'admin', 'Administrador Local', 'admin@vercom.cu', '$2a$11$GHQ/Qsp4E7xGddksPCn6d.0FL8Rr79aaQhTOvP75iIL3xQEcyS3tC', 0, 0, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

    INSERT INTO nucleo.usuario_rol (usuario_id, rol_id, sucursal_id, asignado_en)
    VALUES (@AdminUserId, @AdminRolId, @SucursalId, SYSDATETIMEOFFSET());
END

-- Hash de BCrypt para "cajero123*"
IF NOT EXISTS (SELECT 1 FROM nucleo.usuario WHERE id = @CajeroUserId OR nombre_usuario = 'cajero')
BEGIN
    INSERT INTO nucleo.usuario (id, entidad_id, sucursal_id, nombre_usuario, nombre_completo, email, hash_password, debe_cambiar_pass, intentos_fallidos, activo, creado_en, actualizado_en)
    VALUES (@CajeroUserId, @EntidadId, @SucursalId, 'cajero', 'Cajero Demostración', 'cajero@vercom.cu', '$2a$11$.7Cj8zB.1sJ.zewIJBbQBONoM7d8goFFGqmW1K2GifkfmP6iq.Oda', 0, 0, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

    DECLARE @CajeroRolId INT = (SELECT id FROM nucleo.rol WHERE codigo = 'CAJERO_POS');
    IF (@CajeroRolId IS NOT NULL)
    BEGIN
        INSERT INTO nucleo.usuario_rol (usuario_id, rol_id, sucursal_id, asignado_en)
        VALUES (@CajeroUserId, @CajeroRolId, @SucursalId, SYSDATETIMEOFFSET());
    END
END

-- ==============================================================================
-- 8. CLIENTES Y CONSUMIDOR FINAL POS
-- ==============================================================================
DECLARE @ClienteConsumidorId UNIQUEIDENTIFIER = 'D0000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM comercial.cliente WHERE id = @ClienteConsumidorId)
BEGIN
    INSERT INTO comercial.cliente (id, entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion, telefono, email, segmento, limite_credito, cuenta_contable_id, activo, creado_en)
    VALUES (@ClienteConsumidorId, @EntidadId, 'NATURAL', NULL, 'CONSUMIDOR FINAL', 'Mostrador POS', NULL, NULL, 'MINORISTA', 0.00, @CtaCxcId, 1, SYSDATETIMEOFFSET());
END

IF NOT EXISTS (SELECT 1 FROM comercial.cliente WHERE entidad_id = @EntidadId AND nit_o_ci = '123456789')
BEGIN
    INSERT INTO comercial.cliente (id, entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion, telefono, email, segmento, limite_credito, cuenta_contable_id, activo, creado_en)
    VALUES (NEWID(), @EntidadId, 'JURIDICA', '123456789', 'Empresa Cliente Demo S.A.', 'Calle Mayor #45', '+53 24 987654', 'cliente@demoweb.cu', 'MAYORISTA', 100000.00, @CtaCxcId, 1, SYSDATETIMEOFFSET());
END

-- ==============================================================================
-- 9. FAMILIAS, PRODUCTOS, PRECIOS Y EXISTENCIAS (INVENTARIO COMPLETO DEMO)
-- ==============================================================================
DECLARE @FamiliaAlmId UNIQUEIDENTIFIER = 'F1111111-0000-0000-0000-000000000001';
DECLARE @FamiliaBebId UNIQUEIDENTIFIER = 'F2222222-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamiliaAlmId)
    INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamiliaAlmId, @EntidadId, 'ALM', 'Alimentos');

IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamiliaBebId)
    INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamiliaBebId, @EntidadId, 'BEB', 'Bebidas');

-- Lista de Precios
DECLARE @ListaPrecioId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
IF NOT EXISTS (SELECT 1 FROM inventario.lista_precio WHERE id = @ListaPrecioId)
BEGIN
    INSERT INTO inventario.lista_precio (id, entidad_id, nombre, canal, vigente_desde, activa)
    VALUES (@ListaPrecioId, @EntidadId, 'Precios PDV Versat', 'POS', CAST(GETDATE() AS DATE), 1);
END

-- Obtener Unidad de Medida UN ID
DECLARE @UnidadUNId INT = (SELECT id FROM inventario.unidad_medida WHERE codigo = 'UN');

-- Productos Demo
DECLARE @Prod1Id UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Prod2Id UNIQUEIDENTIFIER = 'A0000002-0000-0000-0000-000000000002';
DECLARE @Prod3Id UNIQUEIDENTIFIER = 'A0000003-0000-0000-0000-000000000003';
DECLARE @Prod4Id UNIQUEIDENTIFIER = 'A0000004-0000-0000-0000-000000000004';

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @Prod1Id)
    INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
    VALUES (@Prod1Id, @EntidadId, 'P-001', '7891234567890', 'Refresco Ciego Montero 330ml', 'Sabor Naranja lata 330ml', @FamiliaBebId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 150.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @Prod2Id)
    INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
    VALUES (@Prod2Id, @EntidadId, 'P-002', '7891234567891', 'Cerveza Cristal 355ml', 'Cerveza clara lata 355ml', @FamiliaBebId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 250.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @Prod3Id)
    INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
    VALUES (@Prod3Id, @EntidadId, 'P-003', '7891234567892', 'Café Cubita Molido 250g', 'Paquete café molido tostado 250g', @FamiliaAlmId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 450.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @Prod4Id)
    INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en)
    VALUES (@Prod4Id, @EntidadId, 'P-004', '7891234567893', 'Agua Mineral Ciego Montero 1.5L', 'Botella de agua natural 1.5L', @FamiliaBebId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 100.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

-- Precios en Lista de Precios
IF NOT EXISTS (SELECT 1 FROM inventario.lista_precio_detalle WHERE lista_precio_id = @ListaPrecioId AND producto_id = @Prod1Id)
    INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio) VALUES (NEWID(), @ListaPrecioId, @Prod1Id, 150.00);

IF NOT EXISTS (SELECT 1 FROM inventario.lista_precio_detalle WHERE lista_precio_id = @ListaPrecioId AND producto_id = @Prod2Id)
    INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio) VALUES (NEWID(), @ListaPrecioId, @Prod2Id, 250.00);

IF NOT EXISTS (SELECT 1 FROM inventario.lista_precio_detalle WHERE lista_precio_id = @ListaPrecioId AND producto_id = @Prod3Id)
    INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio) VALUES (NEWID(), @ListaPrecioId, @Prod3Id, 450.00);

IF NOT EXISTS (SELECT 1 FROM inventario.lista_precio_detalle WHERE lista_precio_id = @ListaPrecioId AND producto_id = @Prod4Id)
    INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio) VALUES (NEWID(), @ListaPrecioId, @Prod4Id, 100.00);

-- Existencias de Inventario (Almacén Central)
IF NOT EXISTS (SELECT 1 FROM inventario.existencia WHERE almacen_id = @AlmacenCentralId AND producto_id = @Prod1Id)
    INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en) VALUES (NEWID(), @AlmacenCentralId, @Prod1Id, 100.00, 90.00, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.existencia WHERE almacen_id = @AlmacenCentralId AND producto_id = @Prod2Id)
    INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en) VALUES (NEWID(), @AlmacenCentralId, @Prod2Id, 100.00, 150.00, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.existencia WHERE almacen_id = @AlmacenCentralId AND producto_id = @Prod3Id)
    INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en) VALUES (NEWID(), @AlmacenCentralId, @Prod3Id, 100.00, 270.00, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM inventario.existencia WHERE almacen_id = @AlmacenCentralId AND producto_id = @Prod4Id)
    INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en) VALUES (NEWID(), @AlmacenCentralId, @Prod4Id, 100.00, 60.00, SYSDATETIMEOFFSET());

-- ==============================================================================
-- 10. DISPOSITIVO POS, RANGOS DE NUMERACIÓN Y SESIÓN DE CAJA ABIERTA
-- ==============================================================================
DECLARE @DispositivoPosId UNIQUEIDENTIFIER = '08F5A4F6-AF55-49A7-A3F5-870187B436A4';
DECLARE @SesionPosId UNIQUEIDENTIFIER = '1B13838D-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM integracion.dispositivo_pos WHERE id = @DispositivoPosId)
BEGIN
    INSERT INTO integracion.dispositivo_pos (id, entidad_id, sucursal_id, almacen_id, codigo, nombre, identificador_hardware, caja_id, estado, creado_en)
    VALUES (@DispositivoPosId, @EntidadId, @SucursalId, @AlmacenCentralId, 'POS-01', 'Terminal Principal POS', 'POS-01', @CajaId, 'ACTIVO', SYSDATETIMEOFFSET());
END

-- Rango de Numeración para el Dispositivo POS
IF NOT EXISTS (SELECT 1 FROM integracion.pos_rango_numeracion WHERE dispositivo_pos_id = @DispositivoPosId)
BEGIN
    INSERT INTO integracion.pos_rango_numeracion (id, dispositivo_pos_id, tipo_documento, serie, numero_desde, numero_hasta, numero_siguiente_local, asignado_en, agotado)
    VALUES (NEWID(), @DispositivoPosId, 'FACTURA_VENTA', 'A', 1, 1000, 1, SYSDATETIMEOFFSET(), 0);
END

-- Sesión de Caja Activa para que el POS pueda operar inmediatamente
IF NOT EXISTS (SELECT 1 FROM integracion.sesion_caja_pos WHERE id = @SesionPosId OR (caja_id = @CajaId AND estado = 'ABIERTA'))
BEGIN
    INSERT INTO integracion.sesion_caja_pos (id, caja_id, dispositivo_pos_id, cajero_id, fecha_apertura, monto_apertura, total_ventas, total_efectivo, total_transfermovil, total_enzona, total_otros_medios, cantidad_facturas, estado)
    VALUES (@SesionPosId, @CajaId, @DispositivoPosId, @MasterUserId, SYSDATETIMEOFFSET(), 1000.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0, 'ABIERTA');
END

-- ==============================================================================
-- 11. PARÁMETROS DEL SISTEMA (CONFIGURACIÓN POS Y REGLAS DE NEGOCIO)
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_VENTAS_GENERAL')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_VENTAS_GENERAL', '900.0000', 'STRING', 'Cuenta de ventas general', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_INGRESO_GENERAL')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_INGRESO_GENERAL', '900.0000', 'STRING', 'Cuenta de ingresos general', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_COSTO_VENTA_GENERAL')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_COSTO_VENTA_GENERAL', '810.0000', 'STRING', 'Cuenta de costo de ventas general', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_INV_GENERICA')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_INV_GENERICA', '183.0010', 'STRING', 'Cuenta genérica de inventario', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_WIP')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_WIP', '185.0010', 'STRING', 'Cuenta WIP productos en proceso', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_CAJA_MN')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_CAJA_MN', '101.0000', 'STRING', 'Cuenta contable de caja MN', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_BANCO_MN')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_BANCO_MN', '109.0000', 'STRING', 'Cuenta contable de banco MN', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_CXP_PROVEEDORES')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_CXP_PROVEEDORES', '405.0020', 'STRING', 'Cuenta de CXP proveedores', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_CXC_CLIENTES')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_CXC_CLIENTES', '135.0000', 'STRING', 'Cuenta de CXC clientes', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_IMP_VENTAS')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_IMP_VENTAS', '440.0001', 'STRING', 'Cuenta del impuesto de ventas', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_PERDIDA_INVENTARIO')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_PERDIDA_INVENTARIO', '850.0010', 'STRING', 'Cuenta de pérdidas de inventario', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'CTA_INGRESO_AJUSTE_INV')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'CTA_INGRESO_AJUSTE_INV', '930.0010', 'STRING', 'Cuenta de ingresos por ajustes', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'POS_SERIE')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'POS_SERIE', 'A', 'STRING', 'Serie por defecto del POS', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'TAX_VENTA')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'TAX_VENTA', '0.10', 'NUMERIC', 'Porcentaje de impuesto sobre venta', CAST(GETDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM nucleo.parametro_sistema WHERE entidad_id = @EntidadId AND codigo = 'META_VENTAS_MENSUAL')
    INSERT INTO nucleo.parametro_sistema (entidad_id, codigo, valor, tipo_dato, descripcion, vigente_desde) VALUES (@EntidadId, 'META_VENTAS_MENSUAL', '100000.00', 'NUMERIC', 'Meta mensual de ventas', CAST(GETDATE() AS DATE));

-- ==============================================================================
-- 12. CONSECUTIVOS DEL SISTEMA
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM nucleo.consecutivo WHERE entidad_id = @EntidadId AND tipo_documento = 'FACTURA_VENTA' AND serie = 'A')
    INSERT INTO nucleo.consecutivo (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero, actualizado_en)
    VALUES (@EntidadId, @SucursalId, 'FACTURA_VENTA', 'A', 0, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM nucleo.consecutivo WHERE entidad_id = @EntidadId AND tipo_documento = 'ORDEN_COMPRA' AND serie = 'A')
    INSERT INTO nucleo.consecutivo (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero, actualizado_en)
    VALUES (@EntidadId, @SucursalId, 'ORDEN_COMPRA', 'A', 0, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM nucleo.consecutivo WHERE entidad_id = @EntidadId AND tipo_documento = 'MOVIMIENTO_INVENTARIO' AND serie = 'A')
    INSERT INTO nucleo.consecutivo (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero, actualizado_en)
    VALUES (@EntidadId, @SucursalId, 'MOVIMIENTO_INVENTARIO', 'A', 0, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM nucleo.consecutivo WHERE entidad_id = @EntidadId AND tipo_documento = 'ASIENTO_CONTABLE' AND serie = 'A')
    INSERT INTO nucleo.consecutivo (entidad_id, sucursal_id, tipo_documento, serie, ultimo_numero, actualizado_en)
    VALUES (@EntidadId, @SucursalId, 'ASIENTO_CONTABLE', 'A', 0, SYSDATETIMEOFFSET());

COMMIT TRANSACTION;

PRINT '================================================================';
PRINT 'SEED DOCKER COMPLETADO CORRECTAMENTE - DEMO VERCOM ERP + POS';
PRINT '================================================================';
PRINT '  Entidad: VERCOM ERP Demo S.U.R.L.';
PRINT '  Sucursal: Sucursal Central (MATRIZ)';
PRINT '  Almacén: ALMACEN CENTRAL (100 u. stock por producto)';
PRINT '  Caja: CAJA POS DEV / PRINCIPAL (Sesión ABIERTA)';
PRINT '  Terminal POS: POS-01 (Hardware ID: POS-01)';
PRINT '  Usuarios de inicio:';
PRINT '    - master  / MasterVercom2026*';
PRINT '    - admin   / admin123*';
PRINT '    - cajero  / cajero123*';
PRINT '================================================================';
GO
