-- ==============================================================================
-- SEED DEMO PARA PRESENTACION - VERCOM ERP + POS
-- Ejecutar DESPUES de seed-docker.sql (usa la misma entidad/sucursal/almacenes).
-- Anade catalogo ampliado, stock, clientes, proveedores, produccion y activos.
-- ASCII only. Idempotente (IF NOT EXISTS).
-- ==============================================================================

USE [VERCOMERP]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DECLARE @EntidadId UNIQUEIDENTIFIER = '24C50C75-26A7-4BAD-A81D-C4D680F4CDF6';
DECLARE @SucursalId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @AlmacenCentralId UNIQUEIDENTIFIER = 'E4397C1D-0000-0000-0000-000000000001';
DECLARE @AlmacenTiendaId UNIQUEIDENTIFIER = 'E4397C1D-0000-0000-0000-000000000002';
DECLARE @CtaInvId UNIQUEIDENTIFIER = '18300000-0000-0000-0000-000000000183';
DECLARE @CtaCostoId UNIQUEIDENTIFIER = '81000000-0000-0000-0000-000000000810';
DECLARE @CtaVentasId UNIQUEIDENTIFIER = '90000000-0000-0000-0000-000000000900';
DECLARE @CtaWipId UNIQUEIDENTIFIER = '18500000-0000-0000-0000-000000000185';
DECLARE @ListaPrecioId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @FamAlmId UNIQUEIDENTIFIER = 'F1111111-0000-0000-0000-000000000001';
DECLARE @FamBebId UNIQUEIDENTIFIER = 'F2222222-0000-0000-0000-000000000002';
DECLARE @UnidadUNId INT = (SELECT id FROM inventario.unidad_medida WHERE codigo = 'UN');
DECLARE @UnidadKGId INT = (SELECT id FROM inventario.unidad_medida WHERE codigo = 'KG');
DECLARE @MasterUserId UNIQUEIDENTIFIER = '3933AD33-9E58-49D7-9A52-9F33A87EACF6';

-- ==============================================================================
-- FAMILIAS
-- ==============================================================================
DECLARE @FamLac UNIQUEIDENTIFIER = 'F3333333-0000-0000-0000-000000000003';
DECLARE @FamCar UNIQUEIDENTIFIER = 'F4444444-0000-0000-0000-000000000004';
DECLARE @FamFrut UNIQUEIDENTIFIER = 'F5555555-0000-0000-0000-000000000005';
DECLARE @FamLimp UNIQUEIDENTIFIER = 'F6666666-0000-0000-0000-000000000006';
DECLARE @FamFerr UNIQUEIDENTIFIER = 'F7777777-0000-0000-0000-000000000007';
DECLARE @FamMPrima UNIQUEIDENTIFIER = 'F8888888-0000-0000-0000-000000000008';

IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamLac) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamLac, @EntidadId, 'LAC', 'Lacteos');
IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamCar) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamCar, @EntidadId, 'CAR', 'Carnes y Embutidos');
IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamFrut) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamFrut, @EntidadId, 'FRU', 'Frutas y Vegetales');
IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamLimp) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamLimp, @EntidadId, 'LIM', 'Limpieza e Higiene');
IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamFerr) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamFerr, @EntidadId, 'FER', 'Ferreteria');
IF NOT EXISTS (SELECT 1 FROM inventario.familia_producto WHERE id = @FamMPrima) INSERT INTO inventario.familia_producto (id, entidad_id, codigo, nombre) VALUES (@FamMPrima, @EntidadId, 'MP', 'Materias Primas');

-- ==============================================================================
-- PRODUCTOS (16 nuevos)
-- ==============================================================================
-- (id, familia, unidad, tipo, nombre, precio, costo)
DECLARE @P5 UNIQUEIDENTIFIER = 'A0000005-0000-0000-0000-000000000005';
DECLARE @P6 UNIQUEIDENTIFIER = 'A0000006-0000-0000-0000-000000000006';
DECLARE @P7 UNIQUEIDENTIFIER = 'A0000007-0000-0000-0000-000000000007';
DECLARE @P8 UNIQUEIDENTIFIER = 'A0000008-0000-0000-0000-000000000008';
DECLARE @P9 UNIQUEIDENTIFIER = 'A0000009-0000-0000-0000-000000000009';
DECLARE @P10 UNIQUEIDENTIFIER = 'A0000010-0000-0000-0000-000000000010';
DECLARE @P11 UNIQUEIDENTIFIER = 'A0000011-0000-0000-0000-000000000011';
DECLARE @P12 UNIQUEIDENTIFIER = 'A0000012-0000-0000-0000-000000000012';
DECLARE @P13 UNIQUEIDENTIFIER = 'A0000013-0000-0000-0000-000000000013';
DECLARE @P14 UNIQUEIDENTIFIER = 'A0000014-0000-0000-0000-000000000014';
DECLARE @P15 UNIQUEIDENTIFIER = 'A0000015-0000-0000-0000-000000000015';
DECLARE @P16 UNIQUEIDENTIFIER = 'A0000016-0000-0000-0000-000000000016';
DECLARE @P17 UNIQUEIDENTIFIER = 'A0000017-0000-0000-0000-000000000017';
DECLARE @P18 UNIQUEIDENTIFIER = 'A0000018-0000-0000-0000-000000000018';
DECLARE @P19 UNIQUEIDENTIFIER = 'A0000019-0000-0000-0000-000000000019';
DECLARE @P20 UNIQUEIDENTIFIER = 'A0000020-0000-0000-0000-000000000020';

IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P5) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P5, @EntidadId, 'P-005', '7891234567894', 'Leche Entera 1L', 'Leche UHT entera 1 litro', @FamLac, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 180.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P6) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P6, @EntidadId, 'P-006', '7891234567895', 'Yogurt Natural 200g', 'Yogurt natural tarrina 200g', @FamLac, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 120.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P7) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P7, @EntidadId, 'P-007', '7891234567896', 'Queso Gouda 500g', 'Queso gouda pieza 500g', @FamLac, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 650.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P8) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P8, @EntidadId, 'P-008', '7891234567897', 'Jamón de Cerdo 1kg', 'Jamón de cerdo rebanado 1kg', @FamCar, @UnidadKGId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 850.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P9) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P9, @EntidadId, 'P-009', '7891234567898', 'Salchicha Frankfurt 500g', 'Salchicha frankfurt paquete 500g', @FamCar, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 320.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P10) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P10, @EntidadId, 'P-010', '7891234567899', 'Tomate Fresco 1kg', 'Tomate fresco de temporada', @FamFrut, @UnidadKGId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 90.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P11) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P11, @EntidadId, 'P-011', '7891234567900', 'Plátano Macho 1kg', 'Plátano macho verde', @FamFrut, @UnidadKGId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 60.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P12) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P12, @EntidadId, 'P-012', '7891234567901', 'Detergente Líquido 1L', 'Detergente para ropa 1 litro', @FamLimp, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 280.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P13) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P13, @EntidadId, 'P-013', '7891234567902', 'Jabón de Tocador', 'Jabón de baño 125g', @FamLimp, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 75.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P14) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P14, @EntidadId, 'P-014', '7891234567903', 'Clavo 2"', 'Clavo de acero 2 pulgadas 1kg', @FamFerr, @UnidadKGId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 420.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P15) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P15, @EntidadId, 'P-015', '7891234567904', 'Tornillo M6 x 20mm', 'Tornillo acero inox M6', @FamFerr, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 15.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P16) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P16, @EntidadId, 'P-016', '7891234567905', 'Harina de Trigo 1kg', 'Harina de trigo todo uso', @FamMPrima, @UnidadKGId, 'MATERIA_PRIMA', @CtaInvId, NULL, NULL, NULL, 0, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P17) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P17, @EntidadId, 'P-017', '7891234567906', 'Azucar Blanca 1kg', 'Azucar refinada blanca', @FamMPrima, @UnidadKGId, 'MATERIA_PRIMA', @CtaInvId, NULL, NULL, NULL, 0, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P18) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P18, @EntidadId, 'P-018', '7891234567907', 'Pan de Molde', 'Pan de molde tajado 500g', @FamAlmId, @UnidadUNId, 'TERMINADO', @CtaInvId, @CtaCostoId, @CtaVentasId, 220.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P19) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P19, @EntidadId, 'P-019', '7891234567908', 'Galleta Maria 200g', 'Galleta maria paquete 200g', @FamAlmId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 95.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
IF NOT EXISTS (SELECT 1 FROM inventario.producto WHERE id = @P20) INSERT INTO inventario.producto (id, entidad_id, codigo, codigo_barras, nombre, descripcion, familia_id, unidad_medida_id, tipo, cuenta_inventario_id, cuenta_costo_venta_id, cuenta_ingreso_id, precio_venta_actual, aplica_impuesto_ventas, activo, creado_en, actualizado_en) VALUES (@P20, @EntidadId, 'P-020', '7891234567909', 'Refresco de Cola 2L', 'Refresco cola botella 2L', @FamBebId, @UnidadUNId, 'MERCANCIA', @CtaInvId, @CtaCostoId, @CtaVentasId, 210.00, 1, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());

-- Precios en lista POS
INSERT INTO inventario.lista_precio_detalle (id, lista_precio_id, producto_id, precio)
SELECT NEWID(), @ListaPrecioId, p.id, p.precio_venta_actual
FROM inventario.producto p
WHERE p.entidad_id = @EntidadId AND p.activo = 1 AND p.precio_venta_actual IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM inventario.lista_precio_detalle d WHERE d.lista_precio_id = @ListaPrecioId AND d.producto_id = p.id);

-- Existencias (central + tienda)
INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en)
SELECT NEWID(), @AlmacenCentralId, p.id, 60.00, p.precio_venta_actual * 0.6, SYSDATETIMEOFFSET()
FROM inventario.producto p
WHERE p.entidad_id = @EntidadId AND p.activo = 1 AND p.tipo = 'MERCANCIA'
  AND NOT EXISTS (SELECT 1 FROM inventario.existencia e WHERE e.almacen_id = @AlmacenCentralId AND e.producto_id = p.id);

INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en)
SELECT NEWID(), @AlmacenTiendaId, p.id, 20.00, p.precio_venta_actual * 0.6, SYSDATETIMEOFFSET()
FROM inventario.producto p
WHERE p.entidad_id = @EntidadId AND p.activo = 1 AND p.tipo = 'MERCANCIA'
  AND NOT EXISTS (SELECT 1 FROM inventario.existencia e WHERE e.almacen_id = @AlmacenTiendaId AND e.producto_id = p.id);

-- Materias primas en central
INSERT INTO inventario.existencia (id, almacen_id, producto_id, cantidad, costo_promedio, actualizado_en)
SELECT NEWID(), @AlmacenCentralId, p.id, 50.00, 40.00, SYSDATETIMEOFFSET()
FROM inventario.producto p
WHERE p.entidad_id = @EntidadId AND p.tipo = 'MATERIA_PRIMA'
  AND NOT EXISTS (SELECT 1 FROM inventario.existencia e WHERE e.almacen_id = @AlmacenCentralId AND e.producto_id = p.id);

-- ==============================================================================
-- CLIENTES Y PROVEEDORES
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM comercial.cliente WHERE entidad_id = @EntidadId AND nit_o_ci = '987654321')
    INSERT INTO comercial.cliente (id, entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion, telefono, email, segmento, limite_credito, cuenta_contable_id, activo, creado_en)
    VALUES (NEWID(), @EntidadId, 'JURIDICA', '987654321', 'Mercado Municipal Demo', 'Av. Central #12', '+53 24 111222', 'mercado@demo.cu', 'MAYORISTA', 50000.00, (SELECT id FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '135.0000'), 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM comercial.cliente WHERE entidad_id = @EntidadId AND nit_o_ci = '555666777')
    INSERT INTO comercial.cliente (id, entidad_id, tipo_persona, nit_o_ci, nombre_razon_social, direccion, telefono, email, segmento, limite_credito, cuenta_contable_id, activo, creado_en)
    VALUES (NEWID(), @EntidadId, 'NATURAL', '555666777', 'Maria Perez', 'Calle 2 #45', '+53 24 333444', 'maria@demo.cu', 'MINORISTA', 0.00, (SELECT id FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '135.0000'), 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM comercial.proveedor WHERE entidad_id = @EntidadId AND razon_social = 'Proveedor Alimentos Demo')
    INSERT INTO comercial.proveedor (id, entidad_id, tipo_persona, nit, razon_social, direccion, telefono, email, cuenta_contable_id, activo, creado_en)
    VALUES (NEWID(), @EntidadId, 'JURIDICA', '111222333', 'Proveedor Alimentos Demo', 'Zona Industrial #8', '+53 24 555666', 'prov@demo.cu', (SELECT id FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '405.0020'), 1, SYSDATETIMEOFFSET());

IF NOT EXISTS (SELECT 1 FROM comercial.proveedor WHERE entidad_id = @EntidadId AND razon_social = 'Proveedor Ferreteria Demo')
    INSERT INTO comercial.proveedor (id, entidad_id, tipo_persona, nit, razon_social, direccion, telefono, email, cuenta_contable_id, activo, creado_en)
    VALUES (NEWID(), @EntidadId, 'JURIDICA', '444555666', 'Proveedor Ferreteria Demo', 'Calle Herramientas #3', '+53 24 777888', 'ferre@demo.cu', (SELECT id FROM contabilidad.cuenta_contable WHERE entidad_id = @EntidadId AND codigo = '405.0020'), 1, SYSDATETIMEOFFSET());

-- (bloque de produccion completo)
DECLARE @ProdTerminadoIdP UNIQUEIDENTIFIER = (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId AND codigo = 'P-018');
DECLARE @MatHarinaIdP UNIQUEIDENTIFIER = (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId AND codigo = 'P-016');
DECLARE @MatAzucarIdP UNIQUEIDENTIFIER = (SELECT id FROM inventario.producto WHERE entidad_id = @EntidadId AND codigo = 'P-017');
DECLARE @BomIdP UNIQUEIDENTIFIER = 'B1000001-0000-0000-0000-000000000001';
DECLARE @FichaIdP UNIQUEIDENTIFIER = 'F1000001-0000-0000-0000-000000000001';
DECLARE @PlanIdP UNIQUEIDENTIFIER = 'D1000001-0000-0000-0000-000000000001';
DECLARE @EquipoIdP UNIQUEIDENTIFIER = 'E1000001-0000-0000-0000-000000000001';

IF @ProdTerminadoIdP IS NOT NULL AND @MatHarinaIdP IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM produccion.lista_materiales WHERE id = @BomIdP)
    BEGIN
        INSERT INTO produccion.lista_materiales (id, producto_terminado_id, version, activa, creado_en)
        VALUES (@BomIdP, @ProdTerminadoIdP, 1, 1, SYSDATETIMEOFFSET());
    END
    IF NOT EXISTS (SELECT 1 FROM produccion.lista_materiales_detalle WHERE lista_materiales_id = @BomIdP AND producto_insumo_id = @MatHarinaIdP)
    BEGIN
        INSERT INTO produccion.lista_materiales_detalle (id, lista_materiales_id, producto_insumo_id, cantidad_requerida, porcentaje_merma)
        VALUES (NEWID(), @BomIdP, @MatHarinaIdP, 0.30, 2.00);
    END
    IF @MatAzucarIdP IS NOT NULL AND NOT EXISTS (SELECT 1 FROM produccion.lista_materiales_detalle WHERE lista_materiales_id = @BomIdP AND producto_insumo_id = @MatAzucarIdP)
    BEGIN
        INSERT INTO produccion.lista_materiales_detalle (id, lista_materiales_id, producto_insumo_id, cantidad_requerida, porcentaje_merma)
        VALUES (NEWID(), @BomIdP, @MatAzucarIdP, 0.10, 1.00);
    END

    IF NOT EXISTS (SELECT 1 FROM produccion.ficha_costo WHERE id = @FichaIdP)
    BEGIN
        INSERT INTO produccion.ficha_costo (id, entidad_id, producto_id, version, vigente_desde, costo_materia_prima, costo_mano_obra, gastos_indirectos, estado, creado_por, creado_en)
        VALUES (@FichaIdP, @EntidadId, @ProdTerminadoIdP, 1, CAST(GETDATE() AS DATE), 45.00, 25.00, 10.00, 'VIGENTE', @MasterUserId, SYSDATETIMEOFFSET());
    END

    IF NOT EXISTS (SELECT 1 FROM produccion.plan_produccion WHERE id = @PlanIdP)
    BEGIN
        INSERT INTO produccion.plan_produccion (id, entidad_id, anio, mes, estado, creado_en)
        VALUES (@PlanIdP, @EntidadId, YEAR(GETDATE()), MONTH(GETDATE()), 'APROBADO', SYSDATETIMEOFFSET());
    END
    IF NOT EXISTS (SELECT 1 FROM produccion.plan_produccion_detalle WHERE plan_id = @PlanIdP AND producto_id = @ProdTerminadoIdP)
    BEGIN
        INSERT INTO produccion.plan_produccion_detalle (id, plan_id, producto_id, cantidad_planificada, cantidad_ejecutada)
        VALUES (NEWID(), @PlanIdP, @ProdTerminadoIdP, 200.00, 0.00);
    END

    IF NOT EXISTS (SELECT 1 FROM produccion.orden_produccion WHERE numero_orden = 'OP-DEMO-001')
    BEGIN
        INSERT INTO produccion.orden_produccion (id, entidad_id, numero_orden, producto_terminado_id, lista_materiales_id, ficha_costo_id, almacen_insumos_id, almacen_producto_id, cantidad_planificada, cantidad_producida, fecha_inicio_plan, fecha_fin_plan, estado, creado_por, creado_en)
        VALUES (NEWID(), @EntidadId, 'OP-DEMO-001', @ProdTerminadoIdP, @BomIdP, @FichaIdP, @AlmacenCentralId, @AlmacenTiendaId, 50.00, 0.00, CAST(GETDATE() AS DATE), DATEADD(DAY, 7, CAST(GETDATE() AS DATE)), 'PLANIFICADA', @MasterUserId, SYSDATETIMEOFFSET());
    END
END

-- Activos
IF NOT EXISTS (SELECT 1 FROM produccion.equipo WHERE codigo = 'EQ-HORNO-01')
BEGIN
    INSERT INTO produccion.equipo (id, entidad_id, sucursal_id, codigo, nombre, estado, frecuencia_mantenimiento_dias)
    VALUES (@EquipoIdP, @EntidadId, @SucursalId, 'EQ-HORNO-01', 'Horno Industrial Principal', 'OPERATIVO', 30);

    INSERT INTO produccion.mantenimiento_programado (id, equipo_id, fecha_programada, tipo, descripcion, estado)
    VALUES (NEWID(), @EquipoIdP, DATEADD(DAY, 5, CAST(GETDATE() AS DATE)), 'PREVENTIVO', 'Revision mensual del horno', 'PROGRAMADO');
END

-- Merma demo (requiere stock; se omite si no hay existencia)
-- (sin insertar para evitar FK/stock inconsistencias)

-- fin (sin COMMIT global)

PRINT '================================================================';
PRINT 'SEED DEMO (PRESENTACION) COMPLETADO';
PRINT '================================================================';
GO
