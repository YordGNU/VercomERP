# Plan: Integración de Iteración 3 — RRHH e Inmutabilidad

Este plan detalla la incorporación de las mejoras de la "Iteración 3" al módulo de Recursos Humanos, centrándose en la exactitud del cálculo salarial (horas extra e IRP), la inmutabilidad de los datos históricos (RNF-22) y el control de plazas (RF-26).

## User Review Required

> [!IMPORTANT]
> **Inmutabilidad de Nómina:** Se activarán disparadores (triggers) en la base de datos que impedirán CUALQUIER edición o eliminación de detalles de nómina una vez que el periodo esté en estado `CONTABILIZADA` o `PAGADA`.
> **Cambio en Cálculo de IRP:** El Impuesto sobre Ingresos Personales ahora se calculará sobre el excedente de un umbral exento (ej: 2,500 CUP), aplicando una tasa porcentual configurada.
> **Recargo de Horas Extra:** Se aplicará un recargo porcentual (ej: 25%) sobre el valor de la hora normal para el cálculo de horas extraordinarias.

## Cambios Propuestos

### 1. Base de Datos (Seguridad e Integridad)
#### [MODIFY] [rrhh.nomina_detalle / rrhh.nomina_detalle_concepto]
- Ejecutar script `103_iteracion3_rrhh.sql` para instalar los triggers de bloqueo.
- Provisionar los nuevos parámetros legales: `TASA_RECARGO_HORA_EXTRA`, `UMBRAL_EXENTO_IMP_INGRESOS_PERS`, `TASA_IMP_INGRESOS_PERS`.

### 2. Capa de Servicios (Lógica de Negocio)
#### [MODIFY] [PayrollService.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Services/PayrollService.cs)
- **`CalculatePayrollAsync`**:
    - Implementar el cálculo de Horas Extra con recargo dinámico.
    - Implementar el cálculo de IRP con umbral exento.
    - Registrar conceptos de aportes patronales (SS e Impuesto FT) para transparencia contable.
- **`ApprovePayrollAsync`**:
    - Ajustar el asiento contable para que el DEBE (Gasto Total) sea igual al HABER (Neto + Retenciones + Aportes), asegurando el cuadre automático.

#### [MODIFY] [HRService.cs](file:///C:/Users/Usuario/source/repos/Vercom/Vercom/Services/HRService.cs)
- **`AddContractAsync`**: Integrar `ValidarPlazaDisponibleAsync` para impedir contrataciones si no hay cupo en la plantilla aprobada (RF-26).
- **`GetExpedienteAsync`**: Implementar restricción de acceso a diagnósticos médicos basada en roles (RRHH/Dirección) (RNF-20).

### 3. Interfaz de Usuario (Feedback)
- Actualizar la visualización de la boleta de pago para mostrar el desglose de IRP (Base imponible vs. Impuesto).

## Plan de Verificación

1.  **Validación de Bloqueo:** Intentar editar una nómina ya contabilizada vía base de datos o aplicación. El sistema debe lanzar un error 51022.
2.  **Cálculo de IRP:** Verificar que un salario de 3,000 CUP con umbral de 2,500 genere un impuesto del 3% solo sobre los 500 CUP de exceso.
3.  **Control de Plazas:** Intentar dar un alta en un cargo/sucursal que tenga 0 plazas disponibles. El sistema debe bloquear la operación.

---

**¿Deseas que proceda con la integración de la Iteración 3?**
