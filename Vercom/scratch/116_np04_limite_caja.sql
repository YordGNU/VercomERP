-- 116: NP-04 - Fijar límite real en caja
-- Establece limite_efectivo en la Caja Principal (actual NULL -> 10000)

SET NOCOUNT ON;

DECLARE @Entidad UNIQUEIDENTIFIER = CAST('24C50C75-26A7-4BAD-A81D-C4D680F4CDF6' AS UNIQUEIDENTIFIER);
DECLARE @cajaId UNIQUEIDENTIFIER = CAST('A0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);

UPDATE contabilidad.caja
SET limite_efectivo = 10000
WHERE id = @cajaId AND entidad_id = @Entidad AND limite_efectivo IS NULL;

PRINT 'NP-04 completado: límite de efectivo fijado en Caja Principal = 10000.';