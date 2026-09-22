USE [VercomERP];
GO

-- ============================================================
-- 109. Referencia legible del documento origen en la cartera
--   El campo permite registrar documentos manuales/saldos
--   extraordinarios con su numero de referencia (FAC-001, etc.).
-- ============================================================

IF COL_LENGTH('contabilidad.cuenta_por_cobrar', 'documento_origen_numero') IS NULL
    ALTER TABLE contabilidad.cuenta_por_cobrar ADD documento_origen_numero nvarchar(100) NULL;

IF COL_LENGTH('contabilidad.cuenta_por_pagar', 'documento_origen_numero') IS NULL
    ALTER TABLE contabilidad.cuenta_por_pagar ADD documento_origen_numero nvarchar(100) NULL;
GO