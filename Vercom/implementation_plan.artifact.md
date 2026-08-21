# Plan Maestro de Refactorización y Auditoría Final (ERP Completo)

Este plan aborda la refactorización de **TODOS** los controladores restantes en el sistema para asegurar que operen 100% bajo el patrón de **Servicios + ViewModels**, eliminando cualquier acceso directo al `AppDbContext` desde la capa de UI.

## Objetivo
Garantizar la consistencia arquitectónica en todos los módulos (0-7), estabilizar el Model Binding en los formularios y eliminar definitivamente el código scaffolded redundante.

## User Review Required

> [!IMPORTANT]
> **Consolidación de Servicios:** Se crearán métodos CRUD genéricos y especializados en los servicios existentes (`IInventoryService`, `IHRService`, `IAccountingService`, `IPurchaseService`) para absorber la lógica de los controladores de catálogos.

> [!WARNING]
> **Aislamiento Multi-inquilino:** Se verificará que el Filtro Global Dinámico de `AppDbContext` cubra el 100% de las entidades, incluyendo las de configuración técnica (Consecutivos, Parámetros).

## Propuesta de Cambios por Módulo

### 1. Módulo de Inventario y Almacén [REMAINING]
*   **Servicio:** Expandir `IInventoryService` para gestionar Almacenes, Familias, Unidades de Medida y Listas de Precio.
*   **Controllers:** Refactorizar `AlmacenController`, `FamiliaProductoController`, `UnidadMedidumController`, `TipoMovimientoController`, `ListaPrecioController`.
*   **ViewModels:** Crear modelos para cada formulario de configuración.

### 2. Módulo Comercial [REMAINING]
*   **Servicio:** Crear `ICommercialService` (o expandir `IPurchaseService`/`ISalesService`) para gestionar Clientes, Proveedores y Contratos Económicos.
*   **Controllers:** Refactorizar `ClienteController`, `ProveedorController`, `ContratoEconomicoController`.

### 3. Módulo Contabilidad y Finanzas [REMAINING]
*   **Servicio:** Expandir `ICashBankService` y `IAccountingService` para gestionar Cajas, Cuentas Bancarias, Centros de Costo y Tipos de Comprobante.
*   **Controllers:** Refactorizar `CajaController`, `CuentaBancariumController`, `CentroCostoController`, `TipoComprobanteController`.

### 4. Módulo de Producción y Mantenimiento [REMAINING]
*   **Servicio:** Expandir `IProductionService` para gestionar Equipos y Mantenimiento Programado.
*   **Controllers:** Refactorizar `EquipoController`, `MantenimientoProgramadoController`, `MermaController`.

### 5. Núcleo y Configuración Técnica [REMAINING]
*   **Servicio:** Expandir `IAdminService` para gestionar Consecutivos y Parámetros del Sistema.
*   **Controllers:** Refactorizar `ConsecutivoController`, `ParametroSistemaController`.

## Fase de Limpieza Definitiva [DELETE]
Se eliminarán los controladores de integración que no tengan una UI definida o que hayan sido absorbidos por servicios de fondo:
*   `ApiLogController.cs`, `ApiTokenController.cs`, `ApiRateLimitController.cs`.
*   Cualquier controlador de "Detalle" remanente.

## Plan de Verificación

### Auditoría de Código
*   Ejecutar búsqueda global de `_context` en la carpeta `Controllers`. El resultado debe ser 0 coincidencias al finalizar.
*   Verificar que todos los métodos `POST` utilicen `Bind(Prefix = "...")` o ViewModels directos para evitar el fallo de Model Binding.

### Manual
*   Navegación completa por el menú lateral (`_SideNav.cshtml`) para asegurar que no existan enlaces a controladores eliminados.

## Open Questions
1.  **Módulo POS:** ¿Los controladores `SesionCajaPo`, `DispositivoPo` y `PosVentaPendiente` se refactorizan ahora o se reservan para la fase de Integración POS Avanzada?
2.  **Reportes Dinámicos:** ¿Desea que el `ReportsController` centralice también las descargas de los "Paquetes Informativos"?
