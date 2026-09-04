# Tareas: Implementación de Subida de Contratos (Word/PDF)

- [x] Modificar `IHRService` y `HRService` para soportar subida de archivos.
    - [x] Inyectar `IWebHostEnvironment`.
    - [x] Actualizar firma de `AddContractAsync`.
    - [x] Implementar lógica de guardado físico de archivos (con fallback de ruta).
- [x] Actualizar `EmpleadoController.AddContract` (POST) para recibir el archivo de forma explícita.
- [x] Rediseñar vista `Views/Empleado/AddContract.cshtml` con campo de tipo `file` y limpieza de nombres.
- [x] Actualizar vista `Views/Empleado/File.cshtml` para mostrar enlaces de descarga.
- [x] Verificar creación de carpeta de destino y permisos.
- [x] Habilitar `UseStaticFiles()` en `Program.cs` para servir los documentos.
