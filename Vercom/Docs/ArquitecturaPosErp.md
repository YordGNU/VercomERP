# POS ↔ ERP — Arquitectura de la Sincronización de Ventas

> Estado: **auditoría completada** — refleja el código real (posiciones verificadas con `file:line`).
> Ubicación de los archivos clave:
> - Controlador API: `Controllers/PosSincronizacionController.cs`
> - Servicio: `Services/PosSincronizacionService.cs`
> - Worker legacy: `Services/PosSyncBackgroundWorker.cs`
> - Modelo: `Models/PosVentaPendiente.cs`, DTOs: `DTOs/PosApiDtos.cs`

---

## 1. Modelo de datos (tabla núcleo)

**`pos.pos_venta_pendiente`** — fila por venta no confirmada, dirigida por `(DispositivoPosId, IdempotencyKey)`:

| Campo | Tipo | Uso |
|---|---|---|
| `Id` | Guid (PK) | |
| `DispositivoPosId` | Guid, FK | Terminal que emite la venta |
| `SesionCajaPosId` | Guid, FK | Sesión de caja ABIERTA contra la que se factura |
| `IdempotencyKey` | string(≤80) | Clave externa del dispositivo (offline) |
| `PayloadJson` | nvarchar(max) | Venta cruda en JSON (payload opaco) |
| `FechaVentaLocal`, `FechaRecibidoServidor` | datetimeoffset | Referencia temporal |
| `Estado` | string: `PENDIENTE`/`PROCESADO` | Máquina de estados |
| `FacturaId`, `MensajeError` | | Resultado del procesamiento |
| `IntentosProcesamiento`, `ProcesadoEn` | | Reintentos / diagnóstico |

**Restricción únca:** `(DispositivoPosId, IdempotencyKey)` — repetir el mismo `POST` con la misma clave no duplica la venta (idempotencia).

---

## 2. Flujo de ingesta (recibir venta)

```
Dispositivo POS ──(POST /api/pos/sincronizacion/ventas)──▶ PosSincronizacionController.Recibir()
```

`PosSincronizacionService.RecibirAsync` (PosSincronizacionService.cs:21):
1. **Valida** referencias obligatorias: `DispositivoPosId`, `SesionCajaPosId`, `IdempotencyKey` **(no vacías, ≤80)** y `Venta` debe ser objeto JSON (`:23-25`).
2. **Devuelve 401/404** si el dispositivo no existe (`:29`).
3. **Acepta solo sesión `ABIERTA` de la caja del dispositivo** (sesión compartida: cualquier terminal de la misma caja puede operar contra la sesión abierta, la abra quien la abra, `:33-35`).
4. **Idempotencia por lectura previa** (`FindAsync`, `:37-38`): si ya existe `(device, key)`, devuelve la fila existente **sin insertar** → el reintento no genera ventas duplicadas.
5. Inserta `PENDIENTE` con el payload crudo y `IdempotencyKey` (`:40-50`).
6. **Race-safe en inserción**: si dos terminales mandan la misma clave a la vez, la PK única dispara `DbUpdateException`; el catch (`:52-58`) re-lee el registro concurrente y lo devuelve (se trata como "ya existente"). Idempotencia de primer nivel.

---

## 3. Flujo de procesamiento (facturación + inventario)

Hay **dos caminos** (hallazgo principal de la auditoría):

### 3.1 Ruta EN LÍNEA (JWT / API) — `PosSincronizacionController.Procesar`
```
POST /api/pos/sincronizacion/ventas/{device}/{key}/procesar
  └▶ PosSincronizacionService.ProcesarAsync (PosSincronizacionService.cs:65)
      └▶ ProcesarCoreAsync (:87)
```
- **Transacción `SERIALIZABLE`** con reintento acotado (3 intentos) ante interbloqueo **SQL 1205** (`:68-84`).
- Valida una a una las referencias de la venta contra la entidad del dispositivo: entidad/sucursal/cliente/almacén/tipo-movimiento activos (`:106-112`).
- **Stock por SUMA por producto** (no línea a línea) → evita dejar existencias negativas (`:120-125`).
- Crea `FacturaVenta` + líneas + `MovimientoInventario` + detalles + pagos (`:143-206`).
- **Decremento atómico condicional** por producto, `WHERE ... AND cantidad >= X`; si afecta 0 filas → venta revierte: "existencia insuficiente" (`:211-217`). Orden por `ProductoId` evita interbloqueos entre terminales con el mismo set de ítems.
- Actualiza totales de la sesión (por medio de pago) y cantidad de facturas (`:232-237`).
- Marca `PROCESADO`, guarda, commitea, notifica a la entidad (`:239-248`).

### 3.2 Ruta OFFLINE (worker legacy) — `PosSyncBackgroundWorker`
```
Cada 30 s: lote de PENDIENTE (≤20), en orden de FechaRecibidoServidor
  └▶ ISalesService (facturación legacy) vía ISalesService.CreateInvoiceAsync
```
- Procesa los pendientes del **formato legacy** (sin `TipoMovimientoId`); los del formato nuevo los **salta** (`continue`, PosSyncBackgroundWorker.cs:58) porque esos van por JWT.
- Usa el flujo de ventas **legacy** de InventaReporte (no el servicio nuevo) → **duplicación funcional** (ver §6).

---

## 4. Concurrencia / transacciones — hallazgos verificados

| Área | Estado | Detalle |
|---|---|---|
| Aislamiento procesamiento | ✅ SERIALIZABLE | `ProcesarCoreAsync` (`:89`) usa `BeginTransactionAsync(IsolationLevel.Serializable)` |
| Interbloqueo 1205 | ✅ Reintento 3x + backoff | `:68-84` reintenta 100ms×n |
| Stock negativo | ✅ Atómico condicional | `UPDATE ... cantidad>=X` + validación por suma (`:211-217, :123-125`) |
| Caja/almacén | ✅ Validaciones de referencia | `:106-117` |
| **Doble implementación** | ⚠️ **DUPLICADO** | Worker legacy (`ISalesService`) vs servicio nuevo — **unificar (ver §6)** |
| Sesión compartida | ✅ Correcto | Cualquier dispositivo de la caja usa la sesión ABIERTA (`:33-35`) |

**Punto débil concurrente:** los pendientes con formato nuevo (JWT) que el worker salta **quedan en `PENDIENTE` para siempre** si el terminal no vuelve a llamar `/procesar` (offline o por caída). El worker no es su red de seguridad.

---

## 5. Validaciones e idempotencia — estado real

- ✅ Referencias vacías / clave vacía >80 → rechazo temprano.
- ✅ Payload debe ser objeto JSON → rechazo.
- ✅ Clave idempotente única `(DispositivoPosId, IdempotencyKey)` → PK no duplicada.
- ✅ Inserción concurrente protegida por catch `DbUpdateException` → devuelve registro existente.
- ✅ Sesión solo `ABIERTA`; sesión `INACTIVA` → rechazo con `Forbid`.
- ⚠️ **Falta**: reintento acotado ante `DbUpdateException` genérica (no 1205) en `RecibirAsync`; hoy solo se trata el duplicado de PK (`:52-58`).
- ⚠️ **Falta**: `Estado = "ERROR"` no tiene reintento automático tras corregirse el dato (política de poison-message no definida).

---

## 6. Recomendación de unificación (ACCIONABLE)

**Problema:** dos implementaciones de facturación (worker legacy `ISalesService` vs servicio nuevo). El worker hoy **salta** el formato nuevo, dejando hueco de recovery si el terminal queda offline tras enqueue.

**Plan recomendado (sin romper legacy):**
1. **Unificar rutas**: que `PosSyncBackgroundWorker` **delegue** el procesamiento al mismo `PosSincronizacionService.ProcesarAsync` (la ruta JWT) para los pendientes del formato nuevo — eliminando el `continue` y usando el servicio como única implementación de facturación POS. El worker se convierte en la **red de seguridad** (sigue procesando legacy + ahora también re-procesa nuevos PENDIENTE huérfanos).
2. Auditar de nuevo (y documentar) tras el cambio.
3. Validaciones e idempotencia: añadir reintento genérico acotado + política explícita de estados `ERROR`.

> ⚠️ **IMPORTANTE:** esto modifica un ERP en producción. Antes de tocar `PosSyncBackgroundWorker.cs:58` se debe confirmar que el worker está registrado como `AddHostedService` en `Program.cs` y que la ruta JWT `ProcesarAsync` acepta el mismo `(deviceId, key)` que consume el worker.

---
*Documento de arquitectura generado por auditoría de código. No incluye cambios de esquema pendientes.*
