-- SEED DE DATOS INICIALES - VERCOM ERP (Docker/Linux)
-- Ejecutar DESPUÉS de init-docker.sql
-- Versión genérica actualizada

USE [VERCOMERP]
GO

-- 1. ENTIDAD GENÉRICA
DECLARE @EntidadId UNIQUEIDENTIFIER = NEWID();
INSERT INTO nucleo.entidad (id, razon_social, nombre_comercial, nit, codigo_reeup, forma_juridica, direccion_legal, municipio, provincia, telefono, email, moneda_base, activo, creado_en, actualizado_en)
VALUES (@EntidadId,'VERCOM ERP Demo S.U.R.L.','VERCOM ERP Demo','12345678901','REEUP000000','S.U.R.L.','Dirección Genérica #123','Ciudad','Provincia','+00 00000000','demo@VERCOMerp.local','CUP',1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET());
GO

-- 2. SUCURSAL
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
DECLARE @SucursalId UNIQUEIDENTIFIER=NEWID();
INSERT INTO nucleo.sucursal (id,entidad_id,codigo,nombre,tipo,direccion,municipio,provincia,telefono,activo,creado_en)
VALUES (@SucursalId,@EntidadId,'MATRIZ','Sucursal Central','MIXTO','Dirección Genérica #123','Ciudad','Provincia','+00 00000000',1,SYSDATETIMEOFFSET());
GO

-- 3. PERMISOS
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad); DECLARE @SucursalId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.sucursal);
INSERT INTO nucleo.permiso (codigo,modulo,descripcion) VALUES
('SEGURIDAD.USUARIO.VER','Seguridad','Ver usuarios'),
('SEGURIDAD.USUARIO.CREAR','Seguridad','Crear usuarios'),
('SEGURIDAD.USUARIO.EDITAR','Seguridad','Editar usuarios'),
('SEGURIDAD.ROL.VER','Seguridad','Ver roles'),
('SEGURIDAD.ROL.EDITAR','Seguridad','Editar roles'),
('SEGURIDAD.AUDITORIA.VER','Seguridad','Ver auditoría'),
('ADMIN.ENTIDAD.VER','Administración','Ver entidad'),
('ADMIN.ENTIDAD.EDITAR','Administración','Editar entidad'),
('ADMIN.PARAMETROS.VER','Administración','Ver parámetros'),
('ADMIN.PARAMETROS.EDITAR','Administración','Editar parámetros'),
('CONTABILIDAD.CUENTA.VER','Contabilidad','Ver cuentas'),
('CONTABILIDAD.ASIENTO.VER','Contabilidad','Ver asientos'),
('CONTABILIDAD.ASIENTO.CREAR','Contabilidad','Crear asientos'),('CONTABILIDAD.ACTIVO_FIJO.CREAR','CONTABILIDAD','Crear activos fijos'),
('CONTABILIDAD.ACTIVO_FIJO.EDITAR','CONTABILIDAD','Editar activos fijos'),
('CONTABILIDAD.ACTIVO_FIJO.ELIMINAR','CONTABILIDAD','Eliminar activos fijos'),
('CONTABILIDAD.CUENTA.ELIMINAR','CONTABILIDAD','Eliminar cuentas contables'),
('INVENTARIO.PRODUCTO.VER','Inventario','Ver productos'),
('INVENTARIO.PRODUCTO.CREAR','Inventario','Crear productos'),
('INVENTARIO.PRODUCTO.EDITAR','Inventario','Editar productos'),
('INVENTARIO.MOVIMIENTO.VER','Inventario','Ver movimientos'),
('INVENTARIO.MOVIMIENTO.CREAR','Inventario','Crear movimientos'),
('INVENTARIO.EXISTENCIA.VER','Inventario','Ver existencias'),
('INVENTARIO.FAMILIA.CREAR','Inventario','Crear familias'),
('INVENTARIO.FAMILIA.EDITAR','Inventario','Editar familias'),
('INVENTARIO.FAMILIA.ELIMINAR','Inventario','Eliminar familias'),
('INVENTARIO.FAMILIA.VER','Inventario','Ver familias'),
('INVENTARIO.LISTA_PRECIO.CREAR','Inventario','Crear listas de precio'),
('INVENTARIO.LISTA_PRECIO.EDITAR','Inventario','Editar listas de precio'),
('INVENTARIO.LISTA_PRECIO.VER','Inventario','Ver listas de precio'),
('INVENTARIO.ALMACEN.CREAR','Inventario','Crear almacenes'),('INVENTARIO.ALMACEN.VER','Inventario','Ver almacenes'),
('COMERCIAL.CLIENTE.VER','Comercial','Ver clientes'),
('COMERCIAL.CLIENTE.CREAR','Comercial','Crear clientes'),
('COMERCIAL.FACTURA_VENTA.VER','Comercial','Ver facturas'),
('COMERCIAL.FACTURA_VENTA.CREAR','Comercial','Crear facturas'),('COMERCIAL.CLIENTE.EDITAR','COMERCIAL','Editar clientes'),
('COMERCIAL.CONTRATO.CREAR','COMERCIAL','Crear contratos'),
('COMERCIAL.CONTRATO.EDITAR','COMERCIAL','Editar contratos'),
('COMERCIAL.PROVEEDOR.CREAR','COMERCIAL','Crear proveedores'),
('COMERCIAL.PROVEEDOR.EDITAR','COMERCIAL','Editar proveedores'),
('COMERCIAL.CONTRATO_SUPLEMENTO.VER','COMERCIAL','Ver suplementos de contrato'),
('COMERCIAL.CONTRATO_SUPLEMENTO.CREAR','COMERCIAL','Crear suplementos de contrato'),
('COMERCIAL.CONTRATO_SUPLEMENTO.ANULAR','COMERCIAL','Anular suplementos de contrato'),
('POS.CAJA','POS','Acceso POS'),
('POS.SESION.ABRIR','POS','Abrir sesión'),
('POS.SESION.CERRAR','POS','Cerrar sesión'),
('SEGURIDAD.ROL.ASIGNAR','SEGURIDAD','Asignar roles'),('SEGURIDAD.USUARIO.EDITAR','SEGURIDAD','Editar usuarios'),('vender','POS','Realizar ventas en el terminal POS'),('consultar_productos','POS','Consultar catálogo'),('ver_inventario','POS','Ver existencias'),('gestionar_inventario','POS','Gestionar inventario'),('cerrar_caja','POS','Cerrar caja'),('aplicar_descuento','POS','Aplicar descuentos'),('editar_producto','POS','Editar productos'),('config_general','POS','Config general'),('config_identidad','POS','Config identidad'),('config_pagos','POS','Config pagos'),('procesar_devolucion','POS','Procesar devoluciones'),('gestionar_usuarios','POS','Gestionar usuarios'),('configuracion','POS','Configuración completa'),('ver_reportes','POS','Ver reportes'),('REPORTES.INDICADORES.VER','Reportes','Ver dashboard'),
('PRODUCCION.ORDEN.VER','Producción','Ver órdenes'),
('PRODUCCION.ORDEN.CREAR','Producción','Crear órdenes'),
('PRODUCCION.ORDEN.EJECUTAR','Producción','Ejecutar órdenes'),
('RRHH.NOMINA.CREAR','RRHH','Crear nómina'),('PRODUCCION.PLAN.VER','Producción','Ver planes'),
('PRODUCCION.PLAN.CREAR','Producción','Crear planes'),('PRODUCCION.PLAN.EDITAR','Producción','Editar planes'),
('PRODUCCION.BOM.VER','Producción','Ver BOM'),
('PRODUCCION.BOM.CREAR','Producción','Crear BOM'),
('PRODUCCION.FICHA.VER','Producción','Ver fichas'),
('PRODUCCION.FICHA.CREAR','Producción','Crear fichas'),
('PRODUCCION.EQUIPO.VER','Producción','Ver equipos'),('PRODUCCION.EQUIPO.CREAR','Producción','Crear equipos'),('PRODUCCION.MANTENIMIENTO.VER','Producción','Ver mantenimientos'),('PRODUCCION.MANTENIMIENTO.CREAR','Producción','Crear mantenimientos'),('PRODUCCION.DEVIACION.VER','Producción','Ver desviaciones'),
('PRODUCCION.MERMA.VER','Producción','Ver mermas'),
('PRODUCCION.MERMA.CREAR','Producción','Crear mermas'),('PRODUCCION.MERMA.REGISTRAR','Producción','Registrar mermas');
GO

-- 4. ROLES
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
INSERT INTO nucleo.rol (codigo,nombre,descripcion,es_sistema,entidad_id,creado_en) VALUES
('ADMINISTRADOR','Administrador de Entidad','Acceso total',1,@EntidadId,SYSDATETIMEOFFSET()),
('CONTADOR','Contador','Contabilidad',1,@EntidadId,SYSDATETIMEOFFSET()),
('ALMACENERO','Almacenero','Inventario',1,@EntidadId,SYSDATETIMEOFFSET()),
('COMERCIAL','Comercial','Ventas',1,@EntidadId,SYSDATETIMEOFFSET()),
('CAJERO_POS','Cajero POS','POS',1,@EntidadId,SYSDATETIMEOFFSET()),
('MASTER','Administrador Global','Master',1,@EntidadId,SYSDATETIMEOFFSET());
GO

-- 5. ASIGNAR PERMISOS A ADMINISTRADOR Y MASTER
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
DECLARE @AdminId INT=(SELECT id FROM nucleo.rol WHERE codigo='ADMINISTRADOR');
DECLARE @MasterId INT=(SELECT id FROM nucleo.rol WHERE codigo='MASTER');
INSERT INTO nucleo.rol_permiso (rol_id,permiso_id) SELECT @AdminId,p.id FROM nucleo.permiso p;
INSERT INTO nucleo.rol_permiso (rol_id,permiso_id) SELECT @MasterId,p.id FROM nucleo.permiso p;
GO

-- 6. USUARIOS
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
DECLARE @SucursalId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.sucursal);
DECLARE @AdminId INT=(SELECT id FROM nucleo.rol WHERE codigo='ADMINISTRADOR');
DECLARE @MasterId INT=(SELECT id FROM nucleo.rol WHERE codigo='MASTER');
DECLARE @AdminUserId UNIQUEIDENTIFIER=NEWID();
DECLARE @MasterUserId UNIQUEIDENTIFIER=NEWID();
INSERT INTO nucleo.usuario (id,entidad_id,sucursal_id,nombre_usuario,nombre_completo,email,hash_password,debe_cambiar_pass,intentos_fallidos,activo,creado_en,actualizado_en)
VALUES (@AdminUserId,@EntidadId,@SucursalId,'admin','Administrador Local','admin@VERCOMerp.local','',1,0,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET());
INSERT INTO nucleo.usuario_rol (usuario_id,rol_id,sucursal_id,asignado_en) VALUES (@AdminUserId,@AdminId,@SucursalId,SYSDATETIMEOFFSET());
INSERT INTO nucleo.usuario (id,entidad_id,sucursal_id,nombre_usuario,nombre_completo,email,hash_password,debe_cambiar_pass,intentos_fallidos,activo,creado_en,actualizado_en)
VALUES (@MasterUserId,@EntidadId,@SucursalId,'master','Usuario Maestro','master@VERCOMerp.local','',0,0,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET());
INSERT INTO nucleo.usuario_rol (usuario_id,rol_id,sucursal_id,asignado_en) VALUES (@MasterUserId,@MasterId,@SucursalId,SYSDATETIMEOFFSET());
GO

-- 7. TIPOS BÁSICOS
INSERT INTO contabilidad.tipo_comprobante (codigo,nombre) VALUES ('DIA','Comprobante Diario'),('ING','Ingreso'),('EGR','Egreso'),('AJU','Ajuste');
INSERT INTO inventario.unidad_medida (codigo,nombre,es_fraccionable) VALUES ('UN','Unidad',0),('KG','Kilogramo',1),('L','Litro',1);
INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('RECEPCION','Recepción','ENTRADA',1),('VALE_ENTREGA','Vale de Entrega','SALIDA',1),('CONSUMO_PRODUCCION','Consumo en Producción','SALIDA',1),('ENTRADA_PRODUCCION','Entrada PT','ENTRADA',1),('DEVOLUCION_PRODUCCION','Devolución MP','ENTRADA',1),('VENTA_POS','Venta POS','SALIDA',1);
GO

-- 8. ALMACENES
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad); DECLARE @SucursalId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.sucursal);
INSERT INTO inventario.almacen (id,entidad_id,sucursal_id,codigo,nombre,es_punto_venta,activo) VALUES (NEWID(),@EntidadId,@SucursalId,'ALM01','Almacén Central',0,1),(NEWID(),@EntidadId,@SucursalId,'TIENDA01','Tienda Principal',1,1);
GO

-- 9. PARÁMETROS DEL SISTEMA
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
INSERT INTO nucleo.parametro_sistema (entidad_id,codigo,valor,tipo_dato,descripcion,vigente_desde) VALUES
(@EntidadId,'META_VENTAS_MENSUAL','100000.00','NUMERIC','Meta de ventas mensual',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_INV_GENERICA','183.0010','NUMERIC','Cuenta genérica de inventario',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_WIP','185.0010','NUMERIC','Cuenta WIP',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_CXP_PROVEEDORES','405.0020','NUMERIC','CXP Proveedores',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_INGRESO_AJUSTE_INV','930.0010','NUMERIC','Ingreso por ajuste de inventario',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_PERDIDA_INVENTARIO','850.0010','NUMERIC','Pérdida de inventario',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_COSTO_VENTA_GENERAL','810.0000','NUMERIC','Costo de venta general',CAST(GETDATE() AS DATE)),
(@EntidadId,'CTA_INGRESO_GENERAL','900.0000','NUMERIC','Ingreso general',CAST(GETDATE() AS DATE));
GO

PRINT 'SEED GENÉRICO CARGADO CORRECTAMENTE';
GO
-- 10. PLAN DE CUENTAS MÍNIMO
DECLARE @EntidadId UNIQUEIDENTIFIER=(SELECT TOP 1 id FROM nucleo.entidad);
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='183.0010') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'183.0010','Inventario de Mercancías','ACTIVO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='185.0010') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'185.0010','Productos en Proceso','ACTIVO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='405.0020') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'405.0020','Cuentas por Pagar - Proveedores','PASIVO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='810.0000') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'810.0000','Costo de Ventas','GASTO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='850.0010') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'850.0010','Pérdida por Faltantes','GASTO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='900.0000') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'900.0000','Ventas','INGRESO',1,1,SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT 1 FROM contabilidad.cuenta_contable WHERE codigo='930.0010') INSERT INTO contabilidad.cuenta_contable (id,entidad_id,codigo,nombre,tipo,nivel,activo,creado_en) SELECT NEWID(),@EntidadId,'930.0010','Ingreso por Ajuste de Inventario','INGRESO',1,1,SYSDATETIMEOFFSET();
GO

-- 11. TIPOS DE MOVIMIENTO COMPLEMENTARIOS
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='AJUSTE_POSITIVO') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('AJUSTE_POSITIVO','Ajuste por Sobrante','ENTRADA',1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='AJUSTE_NEGATIVO') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('AJUSTE_NEGATIVO','Ajuste por Faltante','SALIDA',1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='DEVOLUCION_SALIDA') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('DEVOLUCION_SALIDA','Devolución a Proveedor','SALIDA',1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='DEVOLUCION_ENTRADA') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('DEVOLUCION_ENTRADA','Devolución de Cliente','ENTRADA',1);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='TRANSFERENCIA_SALIDA') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('TRANSFERENCIA_SALIDA','Transferencia - Salida','SALIDA',0);
IF NOT EXISTS (SELECT 1 FROM inventario.tipo_movimiento WHERE codigo='TRANSFERENCIA_ENTRADA') INSERT INTO inventario.tipo_movimiento (codigo,nombre,naturaleza,afecta_costo) VALUES ('TRANSFERENCIA_ENTRADA','Transferencia - Entrada','ENTRADA',0);
GO