# Plan: Modernización y Estandarización de Vistas (Index & Create)

Este plan tiene como objetivo elevar la calidad de la interfaz de usuario del ERP Vercom Elite, asegurando que todos los listados (`Index`) y formularios (`Create`) sigan un patrón de diseño profesional, coherente y altamente funcional basado en el framework **Tabler**.

## User Review Required

> [!IMPORTANT]
> **Consistencia de DataTables:** Se estandarizará el uso de DataTables en todos los `Index` con soporte para idioma español y ordenamiento por defecto. ¿Desea que se habiliten botones de exportación (PDF/Excel) en todos los listados?

> [!TIP]
> **UX de Formularios:** Los formularios de creación se organizarán en secciones lógicas usando "Cards" y cuadrículas responsivas para mejorar la velocidad de entrada de datos, vital para un sistema POS/ERP.

## Estrategia de Mejora

### 1. Vistas de Listado (Index)
- **Cabeceras Dinámicas:** Inclusión de títulos claros, descripciones breves y breadcrumbs.
- **Acciones Prominentes:** Botones de "Crear Nuevo" destacados en la esquina superior derecha.
- **DataTables Profesional:**
  - Inicialización corregida (jQuery `$(document).ready`).
  - Localización al español.
  - Diseño `table-hover` y `align-middle`.
- **Badges de Estado:** Uso de colores "soft" (badge-soft-success, etc.) para mayor legibilidad.

### 2. Vistas de Creación (Create)
- **Agrupación Lógica:** Uso de tarjetas (`card`) para separar datos personales, técnicos y contables.
- **Layout Responsivo:** Implementación de `row g-3` y columnas balanceadas.
- **Limpieza de Inputs:** Ocultar campos técnicos (`EntidadId`, `SucursalId`) que el sistema gestiona internamente.
- **Validación en Tiempo Real:** Integración consistente de `_ValidationScriptsPartial`.
- **Barra de Acciones:** Botones "Guardar" y "Cancelar" estandarizados al final del formulario.

## Módulos Prioritarios

Se aplicarán las mejoras de forma iterativa por módulos:

### [Módulo 1] Contabilidad y Finanzas
- `ActivoFijo`, `CuentaContable`, `CentroCosto`, `AsientoContable`.

### [Módulo 2] Recursos Humanos
- `Empleado`, `Cargo`, `ConceptoNomina`.

### [Módulo 3] Comercial y Abastecimiento
- `Cliente`, `Proveedor`, `ContratoEconomico`, `Purchase`.

### [Módulo 4] Inventario y Producción
- `Producto`, `Almacen`, `ListaMateriale`.

## Plan de Verificación

1.  **Integridad Visual:** Verificar que todas las páginas se vean uniformes y profesionales.
2.  **Funcionalidad de Tablas:** Comprobar que el buscador y el ordenamiento de DataTables funcionen en cada listado.
3.  **Flujo de Datos:** Asegurar que los formularios sigan enviando los datos correctamente a los controladores tras el cambio de layout.
