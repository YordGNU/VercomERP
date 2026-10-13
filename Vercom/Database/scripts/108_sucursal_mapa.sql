USE [VercomERP];
GO

-- ============================================================
-- 108. Vista de mapa para sucursales
--   Columnas opcionales de geolocalizacion (latitud/longitud)
--   y un seed aproximado (centroides por municipio/provincia).
-- ============================================================

IF COL_LENGTH('nucleo.sucursal', 'latitud') IS NULL
    ALTER TABLE nucleo.sucursal ADD latitud decimal(10,6) NULL;
IF COL_LENGTH('nucleo.sucursal', 'longitud') IS NULL
    ALTER TABLE nucleo.sucursal ADD longitud decimal(10,6) NULL;
GO

-- Coordenadas aproximadas (centroides) para registros sin valores.
-- Municipio Gibara, provincia Holguin: 21.1083, -76.1317
-- Municipio Frei (Freyre):            21.0500, -76.2700
-- Resto (default, Holguin centro):     20.8889, -76.2572
UPDATE nucleo.sucursal
SET latitud  = CASE
                   WHEN LTRIM(RTRIM(ISNULL(municipio, ''))) = 'Gibara' THEN 21.1083
                   WHEN municipio IS NOT NULL AND municipio LIKE '%Freyre%' THEN 21.0500
                   ELSE 20.8889
               END,
    longitud = CASE
                   WHEN LTRIM(RTRIM(ISNULL(municipio, ''))) = 'Gibara' THEN -76.1317
                   WHEN municipio IS NOT NULL AND municipio LIKE '%Freyre%' THEN -76.2700
                   ELSE -76.2572
               END
WHERE latitud IS NULL OR longitud IS NULL;
GO