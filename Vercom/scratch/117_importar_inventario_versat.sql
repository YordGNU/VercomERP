-- ============================================================================
-- Script de importación de inventario desde TIERRA_PROMETIDA (Versat) a VercomERP
-- ============================================================================
-- Requisito: OPENROWSET habilitado (sp_configure 'Ad Hoc Distributed Queries', 1)
-- El script es idempotente: no duplica datos existentes.
-- ============================================================================

SET NOCOUNT ON;
GO

DECLARE @EntidadId UNIQUEIDENTIFIER = '24C50C75-26A7-4BAD-A81D-C4D680F4CDF6';

PRINT '=== Importación de inventario desde TIERRA_PROMETIDA ===';
PRINT '';

-- ============================================================================
-- 1. ALMACENES
-- ============================================================================
PRINT 'Importando almacenes...';

SELECT codigo, nombre, activo
INTO #Almacenes
FROM OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT codigo, nombre, activo FROM gen_almacen WHERE activo = 1') AS t;

INSERT INTO inventario.almacen (id, entidad_id, sucursal_id, codigo, nombre, es_punto_venta, activo)
SELECT NEWID(), @EntidadId, NULL, a.codigo, a.nombre, 0, a.activo
FROM #Almacenes a
LEFT JOIN inventario.almacen ea ON ea.codigo = a.codigo AND ea.entidad_id = @EntidadId
WHERE ea.id IS NULL;

PRINT '  Almacenes importados: ' + CAST(@@ROWCOUNT AS VARCHAR);
DROP TABLE #Almacenes;

-- ============================================================================
-- 2. UNIDADES DE MEDIDA
-- ============================================================================
PRINT 'Importando unidades de medida...';

SELECT abreviatura, nombre
INTO #Unidades
FROM OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT abreviatura, nombre FROM gen_unidad') AS t;

INSERT INTO inventario.unidad_medida (id, codigo, nombre)
SELECT NEWID(), u.abreviatura, u.nombre
FROM #Unidades u
LEFT JOIN inventario.unidad_medida eu ON eu.codigo = u.abreviatura
WHERE eu.id IS NULL;

PRINT '  Unidades de medida importadas: ' + CAST(@@ROWCOUNT AS VARCHAR);
DROP TABLE #Unidades;

-- ============================================================================
-- 3. CATEGORÍAS
-- ============================================================================
PRINT 'Importando categorías...';

SELECT nombre
INTO #Categorias
FROM OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT nombre FROM inv_categoria') AS t;

INSERT INTO inventario.categoria (id, nombre, activo)
SELECT NEWID(), c.nombre, 1
FROM #Categorias c
LEFT JOIN inventario.categoria ec ON ec.nombre = c.nombre
WHERE ec.id IS NULL;

PRINT '  Categorías importadas: ' + CAST(@@ROWCOUNT AS VARCHAR);
DROP TABLE #Categorias;

-- ============================================================================
-- 4. PRODUCTOS
-- ============================================================================
PRINT 'Importando productos...';

SELECT codigo, descripcion, idmedida, activo, precio
INTO #Productos
FROM OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT codigo, descripcion, idmedida, activo, precio FROM gen_producto WHERE activo = 1') AS t;

INSERT INTO inventario.producto (
    id, entidad_id, codigo, nombre, descripcion, unidad_medida_id,
    tipo, precio_venta_actual, aplica_impuesto_ventas, activo,
    creado_en, actualizado_en
)
SELECT
    NEWID(), @EntidadId, p.codigo, p.descripcion, p.descripcion, p.idmedida,
    'MERCANCIA', ISNULL(p.precio, 0), 0, p.activo,
    SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM #Productos p
LEFT JOIN inventario.producto ep ON ep.codigo = p.codigo AND ep.entidad_id = @EntidadId
WHERE ep.id IS NULL;

PRINT '  Productos importados: ' + CAST(@@ROWCOUNT AS VARCHAR);
DROP TABLE #Productos;

-- ============================================================================
-- 5. EXISTENCIAS
-- ============================================================================
PRINT 'Importando existencias...';

SELECT t.idproducto, t.cantidad, t.preciomn, t.minimo, t.maximo, gp.codigo
INTO #Existencias
FROM OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT idproducto, cantidad, preciomn, minimo, maximo FROM inv_existencia WHERE cantidad > 0') AS t
INNER JOIN OPENROWSET('SQLNCLI', 'Server=localhost;Database=TIERRA_PROMETIDA;UID=sa;PWD=sql2026*;',
    'SELECT idproducto, codigo FROM gen_producto') AS gp ON gp.idproducto = t.idproducto;

INSERT INTO inventario.existencia (
    id, almacen_id, producto_id, cantidad, costo_promedio,
    stock_minimo, stock_maximo, actualizado_en
)
SELECT
    NEWID(), a.id, p.id, e.cantidad, e.preciomn,
    e.minimo, e.maximo, SYSDATETIMEOFFSET()
FROM #Existencias e
INNER JOIN inventario.producto p ON p.codigo = e.codigo AND p.entidad_id = @EntidadId
INNER JOIN inventario.almacen a ON a.entidad_id = @EntidadId
LEFT JOIN inventario.existencia ee ON ee.producto_id = p.id
WHERE ee.id IS NULL;

PRINT '  Existencias importadas: ' + CAST(@@ROWCOUNT AS VARCHAR);
DROP TABLE #Existencias;

-- ============================================================================
-- RESUMEN
-- ============================================================================
PRINT '';
PRINT '=== Importación completada ===';
PRINT '  Almacenes: ' + CAST((SELECT COUNT(*) FROM inventario.almacen WHERE entidad_id = @EntidadId) AS VARCHAR);
PRINT '  Unidades de medida: ' + CAST((SELECT COUNT(*) FROM inventario.unidad_medida) AS VARCHAR);
PRINT '  Categorías: ' + CAST((SELECT COUNT(*) FROM inventario.categoria) AS VARCHAR);
PRINT '  Productos: ' + CAST((SELECT COUNT(*) FROM inventario.producto WHERE entidad_id = @EntidadId) AS VARCHAR);
PRINT '  Existencias: ' + CAST((SELECT COUNT(*) FROM inventario.existencia) AS VARCHAR);
GO
