# Protocolo de Pruebas de Aceptación (FAT) - ERP Vercom Elite

Este documento define el conjunto de pruebas finales para validar que el ERP cumple con los Requisitos Funcionales (RF) y No Funcionales (RNF) establecidos, integrando las normativas cubanas del MFP, ONAT y CGR.

## Escenario Maestro: "Ciclo Operativo de la Tierra Prometida S.U.R.L."

El objetivo es simular un mes completo de operación, desde la contratación del personal hasta la emisión de los estados financieros.

---

### 1. Infraestructura y Configuración (Módulo 0)
*   **Caso 1.1: Multi-inquilino y Seguridad.**
    *   *Acción:* Crear un nuevo Rol "ECONÓMICO" con permisos limitados a Contabilidad.
    *   *Resultado Esperado:* Un usuario con este rol no debe ver el menú de "Configuración de Seguridad" ni "RRHH".
*   **Caso 1.2: Parámetros Fiscales.**
    *   *Acción:* Configurar la tasa de Seguridad Social en 12.5% en `ParametroSistema`.
    *   *Resultado:* Los cálculos de nómina deben usar este valor dinámicamente.

### 2. Gestión de Capital Humano (Módulo 2)
*   **Caso 2.1: El Ciclo del Trabajador.**
    *   *Acción:* Registrar un empleado -> Crear contrato -> Marcar 20 días de asistencia.
    *   *Resultado:* El sistema debe acumular 1.82 días de vacaciones (9.09% de 20).
*   **Caso 2.2: Nómina y Contabilización.**
    *   *Acción:* Calcular nómina del mes -> Aprobar nómina.
    *   *Resultado:* Se debe generar un asiento contable cuadrado en el módulo de Contabilidad (Cuentas 701 y 401).

### 3. Cadena de Suministro y Producción (Módulos 3 y 4)
*   **Caso 3.1: Recepción de Materia Prima.**
    *   *Acción:* Crear Orden de Compra -> Registrar Recepción en Almacén.
    *   *Resultado:* El stock debe aumentar y el costo promedio (PPP) debe actualizarse. Se debe generar una Cuenta por Pagar automática.
*   **Caso 3.2: Transformación (BOM).**
    *   *Acción:* Crear Orden de Producción -> Consumir Insumos -> Finalizar Producto Terminado.
    *   *Resultado:* El sistema debe descontar materia prima y cargar el costo total al producto elaborado según la Ficha de Costo.

### 4. Ciclo Comercial y Facturación (Módulo 5)
*   **Caso 4.1: Venta con Contrato (B2B).**
    *   *Acción:* Emitir factura mayorista vinculada a un Contrato Económico vigente.
    *   *Resultado:* El sistema debe validar que el monto no exceda el límite del contrato y generar una Cuenta por Cobrar.
*   **Caso 4.2: Integración POS.**
    *   *Acción:* Simular llegada de venta desde la App Android (`integracion.pos_venta_pendiente`).
    *   *Resultado:* El `PosSyncWorker` debe procesar la venta, bajar el stock y generar el ingreso en Caja.

### 5. Inteligencia y Cumplimiento (Módulo 1 y 6)
*   **Caso 5.1: Integridad de la Partida Doble.**
    *   *Acción:* Intentar registrar un asiento manual descuadrado.
    *   *Resultado:* El sistema (y los triggers de DB) deben rechazar la operación (RF-11).
*   **Caso 5.2: Reportes Oficiales.**
    *   *Acción:* Generar Balance General al cierre del mes.
    *   *Resultado:* El reporte debe mostrar Activos = Pasivos + Patrimonio con exactitud decimal.

---

## Plan de Verificación Técnica

1.  **Auditoría (Res. 60/2011):** Tras cada caso, verificar que en `nucleo.auditoria` exista la traza de (Quién, Qué, Cuándo).
2.  **Inmutabilidad:** Intentar editar un asiento contable ya "Contabilizado". El sistema debe arrojar un error y exigir una Reversión.

## Preguntas para el Usuario

> [!IMPORTANT]
> **¿Desea que estas pruebas las realicemos juntos paso a paso a través de comandos SQL de verificación, o prefiere que yo ejecute el ciclo completo y le entregue el informe final de consistencia?**

> [!CAUTION]
> Estas pruebas implican la inserción de datos "limpios". Se recomienda realizar un respaldo previo (`BackupController`) antes de iniciar.
