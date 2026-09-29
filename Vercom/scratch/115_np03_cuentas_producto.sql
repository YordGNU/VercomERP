-- 115: NP-03 - Cargar cuentas 900/810 por producto
-- Asigna cuenta_ingreso_id = 900 (VENTAS DE PRODUCCION) y cuenta_costo_venta_id = 810 (COSTO DE VENTAS DE PRODUCCIONES)
-- a todos los productos activos que no las tengan configuradas

SET NOCOUNT ON;

DECLARE @Entidad UNIQUEIDENTIFIER = CAST('24C50C75-26A7-4BAD-A81D-C4D680F4CDF6' AS UNIQUEIDENTIFIER);
DECLARE @cta900 UNIQUEIDENTIFIER = (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '900' AND entidad_id = @Entidad);
DECLARE @cta810 UNIQUEIDENTIFIER = (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '810' AND entidad_id = @Entidad);

IF @cta900 IS NULL OR @cta810 IS NULL
BEGIN
    RAISERROR('Cuentas 900 o 810 no encontradas', 16, 1);
    RETURN;
END

UPDATE inventario.producto
SET
    cuenta_ingreso_id = CASE WHEN cuenta_ingreso_id IS NULL THEN @cta900 ELSE cuenta_ingreso_id END,
    cuenta_costo_venta_id = CASE WHEN cuenta_costo_venta_id IS NULL THEN @cta810 ELSE cuenta_costo_venta_id END
WHERE entidad_id = @Entidad
  AND (cuenta_ingreso_id IS NULL OR cuenta_costo_venta_id IS NULL);

PRINT 'NP-03 completado: cuentas 900/810 asignadas a productos sin configurar.';