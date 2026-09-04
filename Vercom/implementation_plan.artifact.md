# Plan: Funcionalidad de Subida de Contratos (Word/PDF)

Este plan detalla la implementación para permitir que los usuarios suban el documento físico del contrato (Word o PDF) al registrar un nuevo contrato laboral, permitiendo su consulta posterior desde el expediente del empleado.

## User Review Required

> [!IMPORTANT]
> **Almacenamiento de Archivos:** Los contratos se guardarán en la carpeta `wwwroot/uploads/contracts/`. Es necesario asegurar que el servidor IIS tenga permisos de escritura en este directorio.
> **Formatos Soportados:** Se permitirá la subida de archivos `.doc`, `.docx` y `.pdf`.

## Cambios Propuestos

### 1. Capa de Servicios
#### [MODIFY] [HRService.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Services/HRService.cs)
- Inyectar `IWebHostEnvironment` para obtener la ruta física del servidor.
- Actualizar la firma de `AddContractAsync` para aceptar un objeto `IFormFile`.
- Implementar la lógica de guardado:
    - Validar que el archivo sea un documento válido.
    - Generar un nombre de archivo único (ej: `Contrato_[EmpleadoId]_[Timestamp].docx`).
    - Guardar en disco y registrar la ruta relativa en el campo `DocumentoUrl` del modelo.

### 2. Controlador de Empleados
#### [MODIFY] [EmpleadoController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/EmpleadoController.cs)
- Actualizar la acción `AddContract` (POST) para recibir el parámetro `IFormFile document`.
- Pasar el archivo al servicio de RRHH.

### 3. Vistas de Usuario (UI)
#### [MODIFY] `Views/Empleado/AddContract.cshtml`
- Modificar el `<form>` para soportar subida de archivos (`enctype="multipart/form-data"`).
- Reemplazar el campo de texto de "Referencia" por un input de tipo `file`.

#### [MODIFY] [File.cshtml](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Views/Empleado/File.cshtml)
- En la tabla de contratos, añadir una columna de "Acciones" o "Documento".
- Mostrar un icono de descarga (Word o PDF) si el contrato tiene un archivo adjunto.

## Plan de Verificación

1.  **Subida de Archivo:** Registrar un contrato subiendo un archivo `.docx`. Verificar que se guarda en la carpeta de uploads.
2.  **Consulta desde Expediente:** Entrar al expediente del empleado y comprobar que aparece el enlace de descarga.
3.  **Descarga/Apertura:** Hacer clic en el enlace y confirmar que el navegador abre o descarga el documento correctamente.

---

**¿Deseas que proceda con esta implementación?**
