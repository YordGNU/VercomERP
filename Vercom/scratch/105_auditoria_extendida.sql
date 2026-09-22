USE [VercomERP];
GO

-- Auditoría para Clientes
CREATE OR ALTER TRIGGER comercial.trg_auditar_cliente
ON comercial.cliente
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, ocurrido_en, accion, esquema_tabla, registro_id, ip_origen, canal)
    SELECT NULL, 'SQL_TRIGGER', SYSDATETIMEOFFSET(),
           CASE WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 'UPDATE'
                WHEN EXISTS(SELECT 1 FROM inserted) THEN 'INSERT'
                ELSE 'DELETE' END,
           'comercial.cliente',
           COALESCE((SELECT CAST(id AS NVARCHAR(36)) FROM inserted), (SELECT CAST(id AS NVARCHAR(36)) FROM deleted)),
           '::1', 'DB'
END;
GO

-- Auditoría para Proveedores
CREATE OR ALTER TRIGGER comercial.trg_auditar_proveedor
ON comercial.proveedor
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, ocurrido_en, accion, esquema_tabla, registro_id, ip_origen, canal)
    SELECT NULL, 'SQL_TRIGGER', SYSDATETIMEOFFSET(),
           CASE WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 'UPDATE'
                WHEN EXISTS(SELECT 1 FROM inserted) THEN 'INSERT'
                ELSE 'DELETE' END,
           'comercial.proveedor',
           COALESCE((SELECT CAST(id AS NVARCHAR(36)) FROM inserted), (SELECT CAST(id AS NVARCHAR(36)) FROM deleted)),
           '::1', 'DB'
END;
GO

-- Auditoría para Productos
CREATE OR ALTER TRIGGER inventario.trg_auditar_producto
ON inventario.producto
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO nucleo.auditoria (usuario_id, nombre_usuario, ocurrido_en, accion, esquema_tabla, registro_id, ip_origen, canal)
    SELECT NULL, 'SQL_TRIGGER', SYSDATETIMEOFFSET(),
           CASE WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 'UPDATE'
                WHEN EXISTS(SELECT 1 FROM inserted) THEN 'INSERT'
                ELSE 'DELETE' END,
           'inventario.producto',
           COALESCE((SELECT CAST(id AS NVARCHAR(36)) FROM inserted), (SELECT CAST(id AS NVARCHAR(36)) FROM deleted)),
           '::1', 'DB'
END;
GO
