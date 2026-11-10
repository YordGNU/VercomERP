-- 113: Añadir columnas onat y direccion a paquete_informacion para paquete MFP físico
-- Item 22 backlog: paquete MFP físico (PDF/ZIP + ONAT/DIRECCION + endpoint REST + GeneradoPor)
ALTER TABLE reportes.paquete_informacion ADD onat NVARCHAR(50) NULL;
ALTER TABLE reportes.paquete_informacion ADD direccion NVARCHAR(200) NULL;