# Producción: pruebas y configuración

## Avance funcional estimado

**Estimación al 2026-11-03: 67% funcional.** Es una evaluación de requisitos y flujos implementados, no la cobertura de pruebas. Se ponderan los requisitos RF-40 a RF-43 del plan (80% del total) y los flujos complementarios del módulo (20%); el resultado se redondea al entero más cercano.

| Área evaluada | Peso | Cumplimiento | Evidencia y límite principal |
|---|---:|---:|---|
| RF-40: fichas de costo | 20% | 70% | Versionado y aprobación están implementados. Los componentes MP/MO/GIF se capturan manualmente; no se calculan integralmente desde BOM, nómina, tarifas y actividad fabril. |
| RF-41: BOM y órdenes | 25% | 78% | BOM versionada, aprobación de receta por activación, orden con explosión de consumos, inicio y ajuste real de materiales. Falta cerrar cancelación/reversión operativa y la validación con datos reales. |
| RF-42: integración contable | 25% | 55% | Consumo y entrada de producto terminado usan movimientos contabilizados y se registra la transferencia de conversión a WIP. MO/GIF exige parámetros por entidad (`CTA_MANO_OBRA_PRODUCCION`, `CTA_GIF_PRODUCCION`); el flujo no valida la política contable real de cada entidad y no se probó completo con fixtures SQL de Producción. |
| RF-43: desviaciones | 15% | 72% | Se registran desviaciones de MP/MO/GIF al finalizar. Falta probar reconciliación financiera completa y validar cómo se distribuye el costo de conversión contra valores reales, ya que hoy se aplica desde la ficha estándar. |
| Flujos complementarios (planes, mermas, mantenimiento) | 15% | 62% | Hay aprobación de planes, sugerencia por stock, merma con movimiento/asiento y ejecución de mantenimiento. El plan sugerido no crea órdenes automáticamente; mantenimiento no genera asiento de su costo, y esos flujos no tienen pruebas de integración dedicadas. |

La ponderación equivale a: $20\%\times70\% + 25\%\times78\% + 25\%\times55\% + 15\%\times72\% + 15\%\times62\% = 67.35\%$, redondeado a **67%**. Es una estimación revisable basada en alcance funcional visible, no una certificación de cumplimiento MFP.

### Pendientes para acercarse al 100%

- Probar en SQL Server el ciclo íntegro de una orden, con existencias iniciales, movimientos, asientos y reversión al fallar una etapa.
- Configurar y validar subcuentas imputables de conversión y comprobar que el asiento coincide con la política contable aprobada.
- Vincular el cálculo de costos estándar a fuentes de MP, nómina y GIF, en vez de depender solo de importes capturados en la ficha.
- Cerrar la gestión del ciclo de órdenes (cancelación segura/reversión) y comprobar el comportamiento de planes ejecutados parcialmente.
- Definir y probar la política contable del costo de mantenimiento y el tratamiento de mermas vinculadas a una OP.
- Añadir tests de integración propios de Producción y pruebas de autorización/model binding de sus controladores.

**Cobertura automatizada medida, métrica separada:** 13,19% de líneas en `ProductionService.cs` (89/675); 9,75% (89/913) sumando el servicio y controladores principales de Producción. La suite completa tiene 73/73 tests aprobados, pero la mayoría de los tests existentes cubren otros módulos. La cobertura no se usa como sustituto del porcentaje funcional anterior.

## Ejecutar pruebas

Desde la raíz del repositorio:

```powershell
dotnet test Vercom.Tests/Vercom.Tests.csproj
```

Para ejecutar solo las pruebas unitarias del servicio de producción, no se necesita SQL Server:

```powershell
dotnet test Vercom.Tests/Vercom.Tests.csproj --filter "FullyQualifiedName~ProductionServiceTests"
```

`ProductionServiceTests` cubre rechazos de reglas de negocio para BOM vacía o con cantidades inválidas, costos negativos, períodos inválidos, costos de mantenimiento negativos y aprobación de planes sin productos. Usa EF Core InMemory; no acredita el comportamiento de transacciones ni los asientos contra SQL Server.

## Pruebas de integración

Los tests decorados con `DbFact` usan SQL Server. La conexión se obtiene en este orden:

1. Variable de entorno `VERCOM_TEST_CONNECTION`.
2. `ConnectionStrings:DefaultConnection` de `Vercom/appsettings.json`.

Ejemplo en PowerShell para la sesión actual:

```powershell
$env:VERCOM_TEST_CONNECTION = "Server=localhost;Database=VERCOMERP;User Id=sa;Password=<password>;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet test Vercom.Tests/Vercom.Tests.csproj
```

No guardes credenciales de prueba en el repositorio. Los tests de integración hacen rollback de sus transacciones, pero usan una base con el esquema y datos semilla requeridos por los fixtures. Ejecútalos contra una base dedicada de pruebas, no contra producción.

La CI ejecuta `dotnet test` sin SQL Server; por ello ejecuta las pruebas unitarias y muestra como omitidas las pruebas `DbFact` que necesitan el motor. La ejecución local verificada en esta sesión fue 73/73 con SQL Server configurado.

## Preparación contable de Producción

El costo de conversión al terminar una orden se transfiere de las cuentas de gasto a WIP en el mismo cierre transaccional:

- WIP: parámetro vigente `CTA_WIP`, por defecto `185.0010`.
- Mano de obra: parámetro `CTA_MANO_OBRA_PRODUCCION` con una subcuenta imputable activa, deudora y de clase `GASTOS`.
- Gastos indirectos: parámetro `CTA_GIF_PRODUCCION` con una subcuenta imputable activa, deudora y de clase `GASTOS`.

En el catálogo MFP, `611` y `612` son cuentas agrupadoras. No se deben configurar como cuentas de movimiento; utiliza las subcuentas imputables definidas para cada entidad. Si falta una de las cuentas necesarias, el cierre se rechaza y revierte sus cambios para evitar una contabilización parcial.

Las pruebas unitarias no verifican que estos parámetros existan en una entidad real ni que sus importes/tasas representen la política contable aprobada. Antes de liberar, valida la configuración con el responsable contable y ejecuta una orden de prueba en una base SQL Server dedicada.

## Verificación manual previa a liberar

1. Crear y aprobar una ficha de costo; comprobar que solo haya una versión vigente por producto.
2. Crear una BOM con varios insumos, crear una versión nueva y confirmar que órdenes antiguas conserven su BOM original.
3. Crear una orden, iniciarla y comprobar decremento de existencias y asiento de consumo a WIP.
4. Intentar finalizar con cantidades de consumo omitidas, repetidas o ajenas a la BOM; todas deben rechazarse sin cambios en inventario.
5. Finalizar una orden válida y comprobar ajustes de consumo, costo de producto terminado, asiento de conversión, asiento de terminación y análisis de desviaciones.
6. Registrar una merma y comprobar existencias, valor contable y asiento; repetir con stock insuficiente y confirmar que no se guarde nada.
7. Crear, aprobar y ejecutar un plan; verificar que la producción terminada actualice la cantidad ejecutada.
8. Programar y ejecutar mantenimiento; comprobar el estado, fecha de ejecución y fecha de última revisión del equipo.

## Alcance no cubierto por tests automatizados

- La contabilización transaccional completa y el aislamiento entre entidades requieren SQL Server.
- Las subcuentas de mano de obra/GIF deben configurarse por entidad; el repositorio no puede inferir la política contable particular.
- El costo de mantenimiento se registra operativamente, pero el esquema actual no define una cuenta contable para contabilizarlo.
- El porcentaje funcional es una estimación ponderada; revisarla cuando se cierren o cambien los requisitos y después de validar los flujos con el usuario contable/operativo.
- Las cifras de cobertura son línea base; ampliar tests de flujos de Producción contra SQL Server antes de afirmar completamiento o certificar cobertura de requisitos.
