# Plan: Construcción y Estilizado de Vistas de Integración POS

Este plan detalla las acciones para completar las interfaces de gestión de terminales móviles (POS) en el ERP, asegurando que sean funcionales y sigan el sistema de diseño Vercom Elite.

## User Review Required

> [!IMPORTANT]
> **Acciones en Controladores:** Se añadirán métodos `Details` y `Edit` en los controladores de Integración POS para permitir la visualización profunda de datos (como el JSON de ventas offline) y la edición de configuraciones de hardware.
> **Visibilidad Fiscal:** Se habilitará la vista detallada de Arqueos de Caja POS para auditoría de diferencias.

## Cambios Propuestos

### 1. Refactorización de Controladores
#### [MODIFY] [SesionCajaPoController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/SesionCajaPoController.cs)
- Añadir acción `Details` para ver el arqueo y movimientos de una sesión específica.

#### [MODIFY] [PosVentaPendienteController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/PosVentaPendienteController.cs)
- Añadir acción `Details` para inspeccionar el `PayloadJson` de operaciones enviadas desde Android.

#### [MODIFY] [DispositivoPoController.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Controllers/DispositivoPoController.cs)
- Añadir acciones `Details` y `Edit` para la gestión completa del hardware móvil.

### 2. Construcción de Vistas (UI)
#### [NEW] [SesionCajaPo/Details.cshtml](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Views/SesionCajaPo/Details.cshtml)
- Visualización de montos iniciales, declarados y diferencias de arqueo.

#### [NEW] [PosVentaPendiente/Details.cshtml](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Views/PosVentaPendiente/Details.cshtml)
- Visualizador de código formateado para el JSON de la operación.

#### [MODIFY] [PosRangoNumeracion/Index.cshtml](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Views/PosRangoNumeracion/Index.cshtml)
- Stylize con el sistema de diseño (DataTable, Badges de "Agotado").

### 3. Sincronización con la Aplicación Móvil
- Asegurar que los endpoints de la API en el nuevo proyecto `api` coincidan con la estructura que las vistas esperan gestionar.

## Plan de Verificación

1.  **Monitor de Ventas:** Navegar a "Ventas Offline", entrar a los detalles de una venta y verificar que el JSON se lee correctamente.
2.  **Arqueo de Caja:** Verificar que en el listado de sesiones, al entrar a "Detalles", se muestre el desglose de ingresos/egresos de la terminal.
3.  **Gestión de Rangos:** Validar que el listado de numeración offline indique claramente qué rangos están agotados.

---

**¿Deseas que proceda con la construcción de estas vistas de integración?**
