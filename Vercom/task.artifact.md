# Tareas: Corrección de Inconsistencias en Aprobación de Nómina

## Fase 1: Configuración de Parámetros (Seed)
- [x] Asegurar parámetros de tasas patronales en `SeedData.cs`

## Fase 2: Lógica de Negocio (PayrollService)
- [x] Implementar búsqueda dinámica de `TipoComprobante` (DIA)
- [x] Cargar tasas impositivas desde `ParametroSistema`
- [x] Validar existencia de cuentas contables ("701", "401")
- [x] Envolver proceso de aprobación en una transacción SQL
- [x] Manejar errores de integración contable con mensajes claros

## Fase 3: Verificación y Cierre
- [x] Validar flujo de aprobación completo (Lógica revisada)
- [x] Comprobar generación de asiento contable cuadrado
- [x] Documentar en Walkthrough
