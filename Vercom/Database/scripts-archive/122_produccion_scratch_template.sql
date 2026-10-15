-- 122: plantila segura para cambios de BD de producción
-- NO ejecutar en producción sin revisión explícita y validación previa.
-- Si no hay ajuste real requerido, este script queda como plantilla inactiva.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Ejemplo de patrón seguro:
    -- USE [VercomERP];
    -- IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'tabla_objetivo' AND type = 'U')
    -- BEGIN
    --     CREATE TABLE ...;
    -- END

    -- Aquí va el cambio real, si aplica.
    PRINT 'Scratch listo: no hay cambio de producción requerido por esta corrección.';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
