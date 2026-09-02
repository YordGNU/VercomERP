# Tareas: Implementación de Filtros Avanzados en RRHH

## 1. Capa de Servicios (Business Logic)
- [x] Modificar `IHRService` para incluir parámetros de filtrado en `GetEmployeesAsync` y `GetAttendanceConsoleAsync`.
- [x] Implementar la lógica de filtrado en `HRService`.

## 2. Controladores (MVC)
- [x] Actualizar `EmpleadoController.Index` para recibir y pasar los filtros (inyección de `IAdminService`).
- [x] Actualizar `RegistroAsistenciaController.Console` para soportar filtrado por sucursal, cargo y búsqueda.

## 3. Interfaz de Usuario (Vistas)
- [x] Rediseñar `Views/Empleado/Index.cshtml` con la barra de búsqueda avanzada y segmentación.
- [x] Actualizar `Views/RegistroAsistencia/Console.cshtml` con los selectores de sucursal y cargo.

## 4. Verificación
- [x] Validar búsquedas combinadas en Empleados.
- [x] Validar segregación por sucursal en Asistencia.
- [x] Generar Walkthrough final de la optimización de búsqueda.
