# Digitalización de Contratos Laborales (Word/PDF) - Walkthrough

Se ha implementado la funcionalidad para adjuntar y consultar documentos físicos de contratos laborales directamente desde el expediente del empleado, profesionalizando la gestión documental del ERP Vercom Elite.

## Funcionalidades Implementadas

### 1. Subida Segura de Documentos
El formulario de "Agregar Contrato" ha sido potenciado para permitir la carga de archivos:
- **Formatos Permitidos:** Soporte para documentos de Microsoft Word (`.doc`, `.docx`) y archivos `.pdf`.
- **Almacenamiento Organizado:** Los archivos se guardan en el servidor (`wwwroot/uploads/contracts/`) con nombres únicos que incluyen el ID del empleado y una marca de tiempo para evitar colisiones.
- **Validación:** Control de tamaño (máximo 10MB) y validación de extensiones permitidas.

### 2. Acceso Centralizado desde el Expediente
Se añadió una columna de acceso rápido en la pestaña de **Contratos** del expediente digital:
- **Iconografía Dinámica:** El sistema detecta el tipo de archivo y muestra un icono diferenciado para Word o PDF.
- **Descarga Directa:** Enlace seguro para abrir o descargar el documento oficial con un solo clic.
- **Transparencia:** Los contratos registrados anteriormente que no poseen documento digital muestran un indicador claro de ausencia.

### 3. Lógica de Servicio Blindada
- **Integración Atómica:** El proceso de guardado del archivo y el registro en la base de datos se realiza de forma coordinada. Si el guardado del archivo falla, no se registra el contrato.
- **Aislamiento Multi-tenancy:** Las rutas de los archivos están vinculadas a la identidad del contrato, respetando los filtros de seguridad de cada empresa.

## Resultados de Gestión

> [!SUCCESS]
> **Expediente 360°:** El personal de RRHH ahora puede consultar el contrato legal firmado sin necesidad de recurrir al archivo físico en papel.

> [!TIP]
> **Cumplimiento de Auditoría:** Esta mejora facilita los procesos de auditoría de la ONAT y el MTSS al proporcionar evidencia documental inmediata.

## Próximos Pasos
- Implementar la previsualización de documentos Word directamente en el navegador sin necesidad de descarga.
- Habilitar la subida de múltiples anexos (ej. Documento de Confidencialidad, Recibo de Útiles).
