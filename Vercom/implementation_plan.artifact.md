# Plan: Implementación de Filtros Avanzados en RRHH

Este plan expande las capacidades de búsqueda y segmentación en los expedientes de empleados y la consola de asistencia para mejorar la eficiencia operativa.

## User Review Required

> [!IMPORTANT]
> **Nuevos Filtros:** Además de nombre y sucursal, se añadirán filtros por **Cargo**, **Estado Laboral** (Activo/Baja) y **Rango de Fecha de Ingreso**.
> **Modelo de Vista:** Se creará un `EmployeeIndexViewModel` para manejar los parámetros de búsqueda de forma limpia entre el controlador y la vista.

## Cambios Propuestos

### 1. Capa de Servicios (Business Logic)
#### [MODIFY] [IHRService.cs / HRService.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Services/HRService.cs)
- **`GetEmployeesAsync`**: Soportar parámetros: `string? search`, `Guid? cargoId`, `Guid? sucursalId`, `string? estado`, `DateOnly? desde`, `DateOnly? hasta`.
- **`GetAttendanceConsoleAsync`**: Soportar: `Guid? sucursalId`, `Guid? cargoId`, `string? search`.

### 2. Controladores (MVC)
#### [MODIFY] [EmpleadoController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/EmpleadoController.cs)
- Actualizar `Index` para procesar el nuevo set de filtros.
- Cargar listas de Cargos y Sucursales en `ViewBag` para poblar los dropdowns de filtrado.
#### [MODIFY] [RegistroAsistenciaController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/RegistroAsistenciaController.cs)
- Extender la acción `Console` con filtros por Cargo y Búsqueda textual.

### 3. Vistas (UI)
#### [MODIFY] `Views/Empleado/Index.cshtml`
- Rediseñar la cabecera para incluir una sección colapsable de "Búsqueda Avanzada".
- Incluir selectores para Cargo, Sucursal y Estado.
#### [MODIFY] `Views/RegistroAsistencia/Console.cshtml`
- Añadir selector de Cargo y campo de búsqueda por Nombre/CI al lado del selector de Sucursal.

## Plan de Verificación

1.  **Segmentación por Cargo:** Filtrar por "Operario" y verificar que solo aparezcan los trabajadores de ese grupo.
2.  **Filtrado por Antigüedad:** Buscar empleados que ingresaron en un rango de fechas específico.
3.  **Consola Segmentada:** Probar la carga de la consola de asistencia filtrando simultáneamente por Sucursal y Cargo (ej. "Sucursal Central" + "Vendedores").

---

**¿Deseas que proceda con este set extendido de filtros?**

---

**¿Deseas que proceda con estas mejoras de usabilidad?**
