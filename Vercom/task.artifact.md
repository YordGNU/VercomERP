# Tareas: Integración de Iteración 3 — RRHH e Inmutabilidad

## 1. Base de Datos (Seguridad y Parámetros)
- [x] Ejecutar script `103_iteracion3_rrhh.sql` para triggers de inmutabilidad.
- [x] Insertar nuevos parámetros legales:
    - [x] `TASA_RECARGO_HORA_EXTRA` (25%)
    - [x] `UMBRAL_EXENTO_IMP_INGRESOS_PERS` (2500)
    - [x] `TASA_IMP_INGRESOS_PERS` (3%)

## 2. Lógica de Nómina (PayrollService)
- [x] Refactorizar `CalculatePayrollAsync`:
    - [x] Implementar cálculo de horas extra con recargo.
    - [x] Implementar cálculo de IRP con umbral exento.
    - [x] Registrar aportes patronales como conceptos de nómina.
- [x] Ajustar `ApprovePayrollAsync` para asegurar cuadre contable (DEBE = Gasto + Aportes).

## 3. Gestión de Plantilla y Privacidad (HRService)
- [x] Integrar validación de plazas disponibles en `AddContractAsync`.
- [x] Implementar restricción de acceso a diagnósticos en `GetExpedienteAsync`.

## 4. Verificación
- [x] Validar inmutabilidad (intentar editar nómina aprobada).
- [x] Probar cálculo de IRP sobre excedente.
- [x] Validar bloqueo de contrato por falta de plazas.
- [x] Generar Walkthrough de la Iteración 3 finalizado.
