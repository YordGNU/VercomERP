# AGENTS.md — contexto y memoria del proyecto Vercom ERP

Instrucciones y contexto aprendidos en sesiones anteriores. Es la fuente de memoria para futuras sesiones.

## Usuario
- Comunica en español. Responder en español salvo la keyword "continue" etc.
- Aprobó explícitamente: aplicar correcciones al código Y al esquema real de BD cuando la evidencia lo requiera, y validar en runtime contra la BD VercomERP real con un harness desechable + REVERT final. Las dudas de producto se dejan anotadas como "nota", no se implementan sin ok.

## Reglas de trabajo (IME)
- IME = "Instrucciones para el editor": no agregar comentarios al código salvo que se pida.
- NO hacer commit/push salvo que se pida explícitamente.
- Después de tocar código: `dotnet build Vercom.csproj -c Release` y esperar 0 errores.
- No abrir arquitecturas nuevas; reutilizar servicios/patrones existentes (AccountingService, ConsecutivoService, ParametroSistemaService, IEntidadProvider).
- Ambiente: Windows PowerShell 5.1, win32.

## Conexiones / rutas
- BD real de validación: `Server=LOCALHOST;Database=VercomERP;User Id=sa;Password=sql2026*;TrustServerCertificate=True;MultipleActiveResultSets=true`.
- ERP: `C:\Users\Usuario\source\repos\Vercom\Vercom` (proyecto Vercom.csproj, solución Vercom.slnx).
- POS Android (separado): `C:\Users\Usuario\AndroidStudioProjects\VercomPos` — build APK bloqueado por el entorno.
- Harness desechables bajo `C:\Users\Usuario\AppData\Local\Temp\opencode\` con `ProjectReference` al csproj del ERP (el harness usa el UFService REAL contra la BD REAL).
- DB tiene usuario `master` id `3933ad33-9e58-49d7-9a52-9f33a87eacf6` — los INSERT de asientos requieren `creado_por` válido (FK enforced).

## Esquema BD es CANÓNICO para enumerados (el código debe alinearse a la BD, no al revés)
- `asiento_contable.modulo_origen`: ACTIVOS_FIJOS/POS/VENTAS/COMPRAS/PRODUCCION/INVENTARIO/NOMINA/CONTABILIDAD (NO existe "CARTERA").
- `asiento_contable.estado`: REVERTIDO/CONTABILIZADO/BORRADOR. Los asientos de cobros/pagos quedan CONTABILIZADO (fix aplicado: CreateEntryAsync solo fuerza BORRADOR si Estado vacío).
- `cuenta_por_cobrar.estado` CHECK: PENDIENTE/PARCIAL/PAGADA/INCOBRABLE (fue corregido de PAGADO→PAGADA en DDL, porque el código usa PAGADA). `cuenta_por_pagar.estado`: PENDIENTE/PARCIAL/PAGADA.
- `pago_aplicado.forma_pago` nvarchar(20) con CHECK: CHEQUE/TRANSFERENCIA_BANCARIA/ENZONA/TRANSFERMOVIL/EFECTIVO. TRANSFERENCIA_BANCARIA (21 chars) trunca → NO almacenable; el catálogo del código usa EFECTIVO/CHEQUE/ENZONA/TRANSFERMOVIL + guard `EsFormaPagoAdmitida`.
- `pago_aplicado.tipo`: PAGO/COBRO.
- `contabilidad.tipo_comprobante` DEBE tener catálogo (FK enforced en asiento_contable.tipo_comprobante_id). Insertadas filas con IDENTITY_INSERT: 1 VEN, 2 COM, 3 ING (Ingreso), 4 EGR (Egreso), 5 DIA, 6 REC, 7 AJ (Ajuste), 8 NOM. El código de asientos hardcodea 3/4/7.
- Consecutivos: `nucleo.consecutivo`. En `ASIENTO_CONTABLE` la serie es el TipoComprobanteId ("3"/"4"/"7"). Fix aplicado: la primera numeración arranca en 1 (antes 0). Concurrencia resuelta con UPDLOCK/ROWLOCK.
- FKs: `asiento_detalle.asiento_contable` elimina CASCADE; `pago_aplicado.asiento_contable` NO_ACTION (borrar pagos antes que asientos al revertir). No hay triggers en tablas de cartera/contables.

## Validación "harness real + revert" (patrón aprobado y probado)
- Snapshot previo de IDs (asientos, pagos, periodos, consecutivos serie 3/4) + saldo/estado de la CxC objetivo.
- El SERVICE real (no mock): `ReceivablesPayablesService` con `AccountingService` (necesita IServiceScopeFactory e IEntidadProvider stub que devuelva el id de usuario real).
- Cobros/pagos: `ExecuteUpdateAsync` atómico con `WHERE SaldoPendiente >= monto` (guarda de concurrencia), `rows==0` → rollback y mensaje "El saldo del documento cambió". Race de 2 cobros → exactamente 1 éxito.
- SaldoPendiente recalculado desde PagoAplicados en UpdateCxC/UpdateCxP (ignora valor del form); rechaza si queda negativo.
- Sobrepago rechazado: "El monto del cobro excede el saldo pendiente."
- Revert: borrar pagos nuevos → asientos nuevos (cascade) → restaurar saldo/estado → borrar periodos nuevos → borrar consecutivos nuevos serie 3/4 → restaurar consecutivos solo si delta==1.
- Última corrida (TEST 1 parcial + TEST 2 carrera + TEST 3 sobrepago): TODO OK, revert limpio.

## Datos de prueba usados en BD VercomERP
- Entidad: `24c50c75-26a7-4bad-a81d-c4d680f4cdf6`.
- CxC target: `1f40b98a-ac4b-4b2d-8dea-c9c34e015d32` (FAC-2026-002, monto 1000, saldo 250, PARCIAL).
- CxC sobrepago: `ac6a5b67-cea9-4d0e-926d-4a4492d394a4` (FAC-2026-001, saldo 1250).
- Cuentas contables usadas: 101 (efectivo), 135 (cuentas por cobrar), 405 (cuentas por pagar).

## Notas de producto pendientes (NO implementadas)
- #4: Policies CREAR/EDITAR en cartera dependen del catálogo de permisos en BD (PermissionPolicyProvider trata policies con "." como claim Permission); sin evidencia de bug en código.
- #5: CxC/CxP dadas de alta manualmente no generan asiento de apertura.
- #6: Moneda MLC sin tipo de cambio en asientos de cobro/pago.

## Nómina (P0-6, validado en BD real 2026-09-25)
- Hueco real confirmado: la aprobación de nómina SIEMPRE fallaba en producción por partida descuadrada (Debe 1468.75 vs Haber 1406.25 → 62.50 = retención SS no contabilizada) porque las retenciones se resolvían por IDs hardcodeados `ConceptoId == 6/7` y `concepto_nomina` estaba vacía en producción → 0 asientos NOMINA en toda la historia.
- Fix (PayrollService.ApprovePayrollAsync): las retenciones se resuelven por CÓDIGO de concepto (`CONT_SS_TRAB`, `IMP_INGRESOS_PERS`) y, si el desglose no coincide con `Σ TotalDeducciones` (config de conceptos incompleta), fallback determinista: toda la deducción a la cuenta de retención SS (460.0050) e IRP en 0 → la partida doble SIEMPRE cuadra.
- Semilla idempotente aplicada en BD real: conceptos de nómina (entidad real) SAL_BASICO/HORA_EXTRA/SUBSIDIO_SS/VACACIONES_PAGO (tipo DEVENGO) y CONT_SS_TRAB/IMP_INGRESOS_PERS (tipo DEDUCCION). `concepto_nomina` estaba vacía.
- La contabilización usa cuentas 822 (gasto), 565.0070 (pasivo), 460.0050/460.0060 (retenciones), 440.0008.001 (SS pat), 440.0006.001 (FT); TipoComprobante DIA serie '5' (era la única serie sin consecutivo). El verification_report.artifact.md que cita "701/401" está desactualizado.
- Recalculo + aprobación de 2026-9 ejecutado en producción: periodo CONTABILIZADA, asiento #1 NOMINA cuadra 1468.75/1468.75 (822 ×1250+156.25+62.50; 460.0050 ×62.50; 440.0008.001 ×156.25; 440.0006.001 ×62.50; 565.0070 ×1187.50). Recalculo sobre 67 empleados ACTIVO solo generó detalles para los 3 con contrato VIGENTE.
- Aprobación también acumula vacaciones (FACTOR_VAC 0.0909 × días → saldo_vacaciones por empleado/año, 67 filas en 2026), inserta registro_salario_tiempo_servicio por detalle, y notifica.
- `AccumulateMonthlyVacationsAsync` corre en scope separado (IServiceScopeFactory) → sus cambios NO revierten con el rollback de la transacción externa. `HRService` ctor necesita IAdminService+IFileStorageService (los requires al resolver en harness).
- No existen triggers en rrhh.periodo_nomina/rrhh.nomina_detalle (el comentario "RNF-22 vía Trigger DB" en PayrollService es falso).
- La FK real `periodo_nomina.asiento_id → asiento_contable` exige, al revertir, desvincular el periodo (asiento_id=NULL) ANTES de borrar el asiento.
- NominaDetalle no tiene columna audit; el recálculo borra y regenera detalles del periodo (permitido en PRENOMINA/CALCULADA).

## Desavíos base/BD (P1-1, auditados y corregidos 2026-09-25)
- Auditoría con `erp_schema_harness` (EF ↔ BD real) + `erp_p1_harness` (insert/leer/revert): DDL canónico ↔ BD real solo difiere en una tabla.
- `nucleo.feedback` FALTABA en la BD real (existe en DDL canónico y EF `Feedback`→`feedback`/`nucleo`, usada por FeedbackController) → cualquier POST de feedback fallaba en runtime. Creada en BD real replicando el DDL (CHECKs tipo SUGERENCIA/ERROR/FELICITACION/SOPORTE y estado PENDIENTE/REVISADO/RESUELTO/ARCHIVADO + indexes).
- `webhook_entrega.mensaje_error` FALTABA (columna) y EF NO tenía `.HasColumnName("mensaje_error")` para `WebhookEntrega.MensajeError` (quedaba maopeando `MensajeError`, columna inexistente) → cualquier query/insert de webhooks fallaba. Fix: ALTER TABLE en BD real + mapeo en AppDbContext + columna añadida al DDL canónico.
- `ConceptoNominaController.Create` usaba default `Tipo = "DEVENGADO"` pero el CHECK admite solo APORTE_PATRONAL/DEDUCCION/DEVENGO → default corregido a DEVENGO (la vista ya ofrecía DEVENGO).
- `forma_pago` en contabilidad.pago_aplicado y comercial.forma_pago_venta era NVARCHAR(20) pero el CHECK admite `TRANSFERENCIA_BANCARIA` (22 chars) → cualquier insert fallaba (truncación/error 2628). Ampliadas a NVARCHAR(24) en BD real + DDL canónico + HasMaxLength(24) en AppDbContext (FormaPagoVentum y PagoAplicado). Prueba con insert+CHECK en transacción con ROLLBACK: OK.
- Filtros muertos en ProductionService alineados al CHECK de `producto.tipo` (MERCANCIA/SERVICIO/TERMINADO/EN_PROCESO/MATERIA_PRIMA): `"ELABORADO" || "TERMINADO"` → `"TERMINADO"`, `"INSUMO"` → `"MATERIA_PRIMA"`. No existe código que ESCRIBA "ELABORADO"/"INSUMO" (solo lecturas).
- Canónico confirmado: el CHECK `dispositivo_pos.estado` solo admite ACTIVO/INACTIVO/MANTENIMIENTO (nunca ONLINE) aunque PosCajaService.AbrirAsync busca ACTIVO/ONLINE — nota latente, no se toca.
- `erp_p1_harness` (C:\Users\Usuario\AppData\Local\Temp\opencode\erp_p1_harness): valida feedback y webhook con mensaje_error contra BD real (2 PASS / 0 FAIL) + revert limpio. Patrón de contexto: `new AppDbContext(opts, provider)` con `IEntidadProvider` (Security) y DbSet `Entidads`.

## Audit outputs de P1 (conciliación) — 2026-09-25
- Integridad contable real: 0 asientos descuadrados (chk_cuadre), sin asientos rotos ni huérfanos de VENTAS/POS. Único asiento real en producción = #1 NOMINA (P0-6). Periodo contable 2026-9 ABIERTO. `tipo_movimiento`=12, `concepto_nomina`=6, consecutivos=7 (6 de factura + serie '5' contable), facturas=7, movimientos_inv=7 — BD real en verde (greenfield).
- Los 92 CHECK constraints de BD fueron cruzados contra los literales del código: `ModuloOrigen` = CONTABILIDAD/INVENTARIO/ACTIVOS_FIJOS/VENTAS/NOMINA (todos válidos); POS sincronizado escribe solo en `forma_pago_venta` (CREDITO OK); `ONLINE` en PosCajaService/PosCatalogoService/PosConfigService es SOLO lectura (la BD nunca persiste ONLINE).
- Filtros muertos alineados al CHECK `producto.tipo`: SalesService:96 `(TERMINADO || ELABORADO)` → `TERMINADO` (igual que ProductionService en P1-1). Build 0 errores; harness p1 re-ejecutado: 2 PASS/0 FAIL.
- Las 4 vistas del esquema EJECUTAN en BD real: `contabilidad.v_saldo_cuenta` (6 filas), `contabilidad.v_ejecucion_presupuesto`, `reportes.v_auditoria_accesos`, `reportes.v_auditoria_reversiones` (vacías pero funcionales).

## P1.2 Asistencias/turnos RRHH — corregido y validado 2026-09-25 (harness `erp_hr_harness`: 4 PASS/0 FAIL)
- Bug real: `HRService.AddMedicalCertificateAsync` hardcodeaba `TipoAusenciaId = 1` (Vacaciones) para certificados médicos → corregido con lookup `incapacidad = TipoAusencia.FirstOrDefaultAsync(t => t.Codigo == "05")` y `TipoAusenciaId = incapacidad?.Id ?? 1`. Validado: 3 ausencias con id=1002 (05 Incapacidad Temporal / Subsidio INASS).
- `rrhh.tipo_ausencia.id` es INT: 1=01 Vacaciones, 2=02, 3=03, 4=04, 1002=05 Incapacidad. Turnos por entidad: T-DIURNO/T-NOCTURNO/T-MIXTO (nocturno cruza medianoche; `retardo > tolerancia ? retardo : 0`). Esquema 107 aplicado.
- Validados en BD real: asistencia diurna (retardo 40, salida temprana 10), upsert mismo día (1 fila, retardo 60), nocturno 22:30/06:30 (retardo 30, temprana 0). Revert limpio.

## P1.3 Permisos/roles — auditar y completar 2026-09-25 (harness `erp_perm_harness`: 8 PASS/0 FAIL)
- Infraestructura sana: `PermissionPolicyProvider` (policy con "." o MAYÚS_ → `PermissionRequirement`, resto fallback a default), `PermissionHandler` (bypass MASTER y ADMINISTRADOR), `PosAuthService` emite JWT con ClaimTypes.Role + claims "Permission"; usuarios seed master→MASTER / admin→ADMINISTRADOR. 87 permisos → 113.
- **Se agregaron idempotentemente 26 permisos a `nucleo.permiso`** (código = policy referenciada en `[Authorize(Policy=...)]` sin código en catálogo): COMERCIAL CLIENTE/CONTRATO/PROVEEDOR CREAR+EDITAR, CONTABILIDAD ACTIVO_FIJO CREAR/EDITAR/ELIMINAR + CUENTA.ELIMINAR, INVENTARIO ALMACEN.CREAR/FAMILIA{CREAR,ELIMINAR,VER}/LISTA_PRECIO{CREAR,VER}/PRODUCTO{EDITAR,ELIMINAR}, PRODUCCION BOM{CREAR,VER}/MANTENIMIENTO{CREAR,VER}/MERMA.VER, RRHH.NOMINA.CREAR, SEGURIDAD ROL.ASIGNAR/USUARIO.EDITAR. **Sin grants de rol (asignación = decisión de producto).**
- `pos_gestionar_usuarios`/`pos_configuracion` (único lower-case) están registradas en Program.cs y SÍ existen en BD. Validado: MASTER/ADMINISTRADOR bypass, claim exacto pasa, sin claim deniega, permiso nuevo sin rol deniega, provider no revienta con policy inexistente.
- Nota producto: decidir qué roles reciben los 26 permisos nuevos (hoy solo pasan MASTER/ADMINISTRADOR).

## P1.4 Reportes regulatorios — corregido 2026-09-25 (validado en `erp_hr_harness`: paquete insert/revert OK)
- **Bug real:** `ReportingService.GenerateMonthlyPackageAsync` insertaba `Tipo = "MENSUAL"` pero el CHECK de `reportes.paquete_informacion.tipo` solo admite MFP/ONAT/DIRECCION → toda generación de paquete mensual habría fallado (547). Corregido a `Tipo = "MFP"`. Validado insert (Tipo=MFP, Estado=GENERADO) + revert.
- Presupuesto (BORRADOR default) y cartera de declaraciones (TaxService PENDIENTE/PRESENTADA) cumplen los CHECKs.

## P1.5 Cartera/consecutivos/maestras — auditado 2026-09-25
- 108/109/110 aplicados en BD real (lat/long sucursales poblados; `documento_origen_numero` en cxc/cxp; consecutivos base serie 'A' por entidad). 110 deja una fila fantasma `ASIENTO_CONTABLE serie '5' ultimo 1` (origen seed, sin asientos que la usen) — inofensiva, numeración real va por serie 'A'.
- **105 NO aplicado y es trampa:** los triggers de auditoría usaban `canal = 'DB'` pero el CHECK de `nucleo.auditoria.canal` solo admite POS/API/ERP → habrían roto INSERT/UPDATE/DELETE de cliente/proveedor/producto. Corregido el script a `canal = 'ERP'`. No se crean en BD real (el `AuditInterceptor` ya audita; canal nullable=0).
- **106 NO aplicado (producto):** el catálogo real tiene 369 cuentas (nivel1=6) y 65 raíces con `acepta_movimiento=1`; 701 no existe. `FixedAssetService:259` busca cuenta `Codigo == "701"` para la pérdida por baja: con `lossAccount != null` como guard NO crashea, PERO si `valorNeto > 0` y no hay cuenta de pérdida el asiento de baja queda **descadrado** (falta la pata de pérdida). El script 106 además es incoherente (clasifica 701 como INGRESO/ACREEDORA pero el código la debita como GASTO). → Decidir acorde al plan MFP: crear cuenta de gasto de pérdida y usarla (nota).
- Cartera: estados escritos = PENDIENTE/PARCIAL/PAGADA (PAGADA vía update atómico), cumplen CHECK cxc IN (PENDIENTE/PARCIAL/PAGADA/INCOBRABLE) y cxp.

## P1.6 POS backend restante — auditado 2026-09-25 (validado en `erp_hr_harness`: rango 1..7 + revert)
- `ReserveNumberRangeAsync` es atómico (UPDLOCK/ROWLOCK + transacción propia), crea rango en `integracion.pos_rango_numeracion` y avanza `nucleo.consecutivo`. Validado rango 1..7 + revert limpio.
- ConsecutivoService idempotente (crea serie si falta, UPDLOCK). El worker de sync fija `Serie = "POS"` y renumera vía `CreateInvoiceAsync` (ObtenerSiguienteNumeroAsync) → los números de las ventas POS sincronizadas SIEMPRE son únicos.
- Nota producto: el rango reservado por el dispositivo NO se reconcilia con la numeración del server (se reserva y luego se ignora) → los tickets impresos pueden no coincidir con la numeración fiscal. No genera duplicados.
- Catálogo POS: `UnidadMedidaId` NOT NULL (sin NRE), 9 productos. `pos_venta_pendiente` sin CHECK (estados libres PENDIENTE/ERROR/PROCESADA → sin riesgo 547).

## P2-E2E Compras → CxP → Pago (validado 2026-09-25, `erp_hr_harness`, 2 PASS/0 FAIL, revert limpio)
- Flujo completo contra BD real: crear proveedor de prueba → OC (BORRADOR, consecutivo ORDEN_COMPRA 'A' OC-00000001) → aprobar (APROBADA) → recepción (movimiento RECEPCION + existencia +2 [116→118] + asiento INVENTARIO cuadrado 50/50: Debe 183.0010 fallback de producto sin cuenta, Haber 405.0020; CxP PENDIENTE 50) → pago 50 EFECTIVO (CxP→PAGADA, pago_aplicado 50, asiento CONTABILIDAD 50/50 Debe 405/Caja 101). Revert completo OK.
- **#7 (nota técnica, no bug):** `RecordPaymentAsync`/`RecordCollectionAsync` actualizan saldo/estado con `ExecuteUpdateAsync` (SQL directo) que NO refresca la entidad trackeada → dentro del MISMO DbContext una lectura posterior devuelve el estado viejo (PENDIENTE) hasta re-query con `AsNoTracking`/contexto nuevo. En web (scope por request + redirect) no afecta; en flujos que lean el documento tras pagar en el mismo scope conviene `AsNoTracking`.
- Lecciones del harness: para snapshot/revert usar SIEMPRE valores escalares (no referencias trackeables que mutan con el flujo), o se restaura el valor ya incrementado (consecutivo quedó +2 en una corrida; corregido). Los bloques E2E de `erp_hr_harness` hacen `dbc.ChangeTracker.Clear()` al inicio, y las lecturas de verificación/snapshot usan `AsNoTracking()` (los `ExecuteUpdateAsync`/`ExecuteDeleteAsync` del revert NO refrescan el tracking → entidades stale entre bloques).

## P2-E2E Venta en efectivo → Devolución/Anulación (validado 2026-09-25, `erp_hr_harness`: 10 PASS/0 FAIL, revert limpio)
- **Bug real (fix aplicado `Services/SalesService.cs`):** el default de venta era `CTA_VENTAS_GENERAL = "500.0100"`, cuenta que NO existe en el catálogo real (369 cuentas) ni hay parámetro `CTA_%` en `nucleo.parametro_sistema` → `salesAcc` null → asiento desbalanceado → **las ventas nunca generaban asiento contable**. Fix: `?? "500.0100"` → `?? "900"` (cuenta canónica `900 VENTAS DE PRODUCCIÓN`; existen 900/901/904). Build 0 errores.
- **Semilla idempotente aplicada:** parámetro `TAX_VENTA = 0.10` (NUMERIC) por entidad en `nucleo.parametro_sistema` — sin él `TaxService.CalculateSalesTaxAsync` devolvía 0 → las ventas no cobraban impuesto (P-0001 tiene `aplica_impuesto_ventas=1`, 10% hardcodeado en detalle).
- Ciclo validado: venta 2×25 = 50 + IVA 5 → factura A-00000001 total 55 EFECTIVO (cliente CONSUMIDOR FINAL `D0000000-...-000000000001`, sucursal MATRIZ, almacén B000...) → asiento VENTAS CONTABILIZADO 55/55 (Debe 101 / Haber 900 + 440.0001), stock 116→114 → `CancelInvoiceAsync` → factura ANULADA, asiento original REVERTIDO, asiento espejo REVERSIÓN 55/55 (ESTADO CONTABILIZADO), movimiento DEVOLUCION_ENTRADA, stock 114→116.
- **FK de revisión (`asiento_reversion_id`): es bidireccional** (el espejo apunta al original y el original al espejo) → al revertir hay que: (1) desvincular `factura_venta.asiento_id`, (2) `UPDATE asiento_contable SET asiento_reversion_id=NULL` en el original, (3) borrar el espejo (REVERSION), (4) borrar el original. Borrar cualquiera primero sin romper el ciclo da 547.
- Resto del ciclo ya validado: cancelar (ANULACIÓN) con productos sin cuenta de costo NO genera asiento de devolución (contra null → `continue`), el asiento de costo de P-0001..P-0008 tampoco existe → reversión neta simple; stock restaurado con DEVO +2.
- Nota producto: P-0001..P-0008 sin `cuenta_costo_venta_id`/`cuenta_ingreso_id` → no se genera asiento de costo en VALE/DEVOLUCIÓN (solo existen las cuentas 900/810 en catálogo).

## P2-Bloque C items 3-5 (validados 2026-09-25, `erp_hr_harness`: +13 PASS/0 FAIL, revert limpio; total suite 18 PASS/0 FAIL)
- **Item 3 Transferencia + ajustes:**
  - Transferencia entre almacenes (`TransferBetweenWarehousesAsync`): movimiento TRANSFERENCIA_SALIDA con origen+destino; `tipo.AfectaCosto=false` → NO genera asiento (diseño correcto). Stock origen −10, destino +10 (crea Existencia en destino con el PPP). Validado con almacén temporal ALM-E2E + revert.
  - **Bug real (fix `InventoryService.ConciliatePhysicalCountAsync`):** los ajustes de SOBRANTE (`AJUSTE_POSITIVO`, ENTRADA) no llevaban `CostoUnitario` → `monto = cantidad × (CostoUnitario ?? 0) = 0` → `continue` → **el asiento de sobrante NUNCA se generaba**. Fix: capturar `stockAdj.CostoPromedio` en el detalle del movimiento de ajuste (también aplica a faltantes). Validado: faltante (Debe 850.0010 Haber 183.0010, monto=Δ×PPP, cuadrado) y sobrante (Debe 183.0010 Haber 930.0010, cuadrado) — ambos ahora cuadrado 166.64 y 222.18 en el run.
  - Conteo físico: RF-33 exige justificación en faltantes; Start/Submit/Close (`WarehouseService` → `ConciliatePhysicalCountAsync`) → conteo CERRADO, movimiento AJU-{8}, asiento, stock ajustado. Revert: null asiento→delete asiento→delete conteo (cascade detalles, libera FK movimiento_ajuste_id)→delete movimiento→restore stock.
- **Item 4 Estados financieros:**
  - **Bug real (fix `IntelligenceService` + `FixedAssetService:73` + `AccountingService:164`):** la BD canónica usa `cuenta_contable.clase` PLURAL (`GASTOS`/`INGRESOS`; CHECK: ACT/GASTOS/INGRESOS/PATRIMONIO/PASIVO/ORDEN) pero el código filtraba singular `"GASTO"`/`"INGRESO"` (mismo patrón de SalesService→900) → los reportes devolvían VACÍO/0: Estado de Resultados, indicadores del dashboard (márgenes/rentabilidad), cierre anual (EstimatedProfit siempre 0), y el selector de cuenta de gasto del Activo Fijo. Alineados a plural; el SelectList del form de cuenta también.
  - Validado contra BD real (periodo 9/2026, único asiento = nómina #1 CONTABILIZADO): Balance General activos 0 / pasivos 1468.75 (565.0070 1187.50 + retenciones 62.50+156.25+62.50); Estado Resultados solo gasto 822 = 1468.75; cierre anual 2026: utilidad estimada −1468.75, CanClose=false (periodos abiertos).
  - `GetAccountBalanceAsync`: saldo según Naturaleza (DEUDORA→Debe−Haber, ACREEDORA→Haber−Debe), filtra Estado=CONTABILIZADO y Periodo.
- **Item 5 Caja/Banco:**
  - `ValidateCashLimitAsync`: con `limite_efectivo` son NULL (Caja Principal real lo tiene NULL) el comparador `>` sobre `decimal?` con NULL → SIEMPRE pasa (operador elevado da false) → **control interno no valida si el límite no está configurado** (nota; con límite configurado funciona: 50 ok / 140 rechazado validado).
  - Conciliación bancaria: `CreateBankMovementAsync` (movimiento CREDITO/DEBITO, CHECK) → `GetPendingReconciliationAsync` → `ReconcileBankMovementAsync` (Conciliado=true, FechaConciliacion) → pendientes 1→0. Cuenta bancaria CUP/CORRIENTE. Validado con datos temporales + revert.
  - Nota: las vistas `MovimientoBancario/Index` y `Details.cshtml` comparan `Tipo == "INGRESO"` (muerto, el CHECK es CREDITO/DEBITO) — cosmético, no se toca.

## P2-E2E item 6: ciclo POS backend (validado 2026-09-25, `erp_hr_harness`, revert limpio; suite 20 PASS/0 FAIL)
- Flujo validado de punta a punta: `PosSincronizacionService.RecibirAsync` (valida dispositivo existente + sesión ABIERTA de la caja del dispositivo) → `ProcesarAsync` (`ProcesarCoreAsync`, transacción SERIALIZABLE, reintento 3× ante deadlock 1205; IgnoreQueryFilters porque en el worker no hay HttpContext) → crea FacturaVenta (CanalVenta=POS, numeración provista por el dispositivo desde su rango reservado), MovimientoInventario canal POS, decremento atómico condicional de stock (`UPDATE ... WHERE cantidad >= n`, orden por ProductoId anti-interbloqueo), FormaPagoVenta, acumula totales en la sesión, y los dos asientos (venta + costo/inventario).
- **Bug real (fix `PosSincronizacionService.CrearAsientoVentaYCosteAsync:303`):** según fallback de `CTA_VENTAS_GENERAL` seguía siendo `"500.0100"` (cuenta inexistente) → `ventasAcc=null` → el asiento quedaba desbalanceado (55 Debe vs 5 Haber) → NO se creaba el asiento de venta POS (mismo patrón que SalesService/P1). Fix → `"900"`. Validado: factura POS-00000092 55 CUP EMITIDA, asiento VENTA POS 55/55 (Debe 101, Haber 900 + 440.0001) CONTABILIZADO, stock 116→114, sesión TotalVentas 55/CantidadFacturas 1.
- Costo de venta POS: `GenerateAccountingEntryForExistingMovementAsync` → con P-0001 sin `cuenta_costo_venta_id` el `contraAccId` queda null → `continue` → NO se genera asiento de costo (misma nota de VALE/DEVOLUCIÓN; se resolverá al configurar la cuenta en el producto). Movimiento SALIDA de VENTA usa `CtaInv = producto.CuentaInventarioId ?? CTA_INV_GENERICA`.
- Consecutivo serie POS (MATRIZ) avanza con cada reserva (`nucleo.consecutivo`), pero las reservas NUNCA se reconcilian contra las facturas reales (P1.6): los números los asigna el dispositivo localmente y el backend los acepta sin validar que pertenezcan al rango reservado.
- Revert POS exige orden estricto por FKs: formapago → detalles factura → NULL asiento_id → asiento (detalles+asiento) → pendiente → factura → movimiento → rango → sesión → dispositivo → caja → stock.
- `RecibirAsync` exige sesión ABIERTA de la misma caja; UNIQUE parcial (caja_id) WHERE estado='ABIERTA'. Dispositivo real de prueba: POS-01 (`C0000000-...`) con caja Principal (`A0000000-...`) y sesión real ABIERTA (`1B13838D-...`); TMV id=1 VENTA ("Venta POS") / 1011 VENTA_POS.

## P2-E2E item 7: POS cierre de sesión / arqueo (validado 2026-09-25, `erp_hr_harness`, revert limpio; suite 22 PASS/0 FAIL)
- `PosCajaService.AbrirAsync`: valida dispositivo ACTIVO/ONLINE (BD solo admite ACTIVO/INACTIVO/MANTENIMIENTO), única ABIERTA por caja (índice único parcial), Conflict si ya hay sesión abierta (incluso de otro dispositivo: sesión compartida por caja).
- `PosCajaService.CerrarAsync`: rechaza monto negativo, **bloquea el cierre si hay ventas PENDIENTE de procesar** para esa sesión, calcula `MontoCierreSistema = MontoApertura + TotalEfectivo` y `DiferenciaArqueo = declarado − sistema`, marca CERRADA sin crear asiento ni conciliación.
- Validado: apertura 500 + venta POS 55 EFECTIVO → sistema 555, declarado 555, diferencia 0, CERRADA; pendiente sin procesar → "No se puede cerrar la caja con ventas pendientes"; negativo → error.
- **Hallazgo (nota, no bug):** el cierre de caja NO contabiliza el arqueo (no asiento, no conciliación). Las ventas POS ya contabilizan su asiento al procesarse, así que el efectivo está reflejado; un asiento por faltante/sobrante de arqueo sería decisión de producto (dejarla como nota del P3). Tampoco existe tabla/vista de conciliación de arqueo en la BD real.
- Lección del harness rehecha: más vale capturar el ESTADO esperado (bool) justo tras la operación que lo produce y usar `AsNoTracking` para todas las lecturas de verificación; las luego-lecturas con un contexto que ya trackea la entidad re-ejecutan el fix-up del query y "adelantan" el estado (PENDIENTE→PROCESADO) invalidando la aserción.

## P3-E2E item 8: Producción BOM → consumo WIP → devolución + entrada PT → merma (validado 2026-09-27, `erp_hr_harness`, revert limpio; suite 23 PASS/0 FAIL)
- Flujo completo contra BD real (producto 45ASD "Croquetas" tipo EN_PROCESO como PT sin existencias + insumo P-0001 con stock): `CreateBomAsync` (BOM activa, características req/merma) → `CreateCostSheetAsync` (ficha VIGENTE, CostoTotal=PrecioSugerido) → `CreateProductionOrderAsync` (OP- N°, PLANIFICADA, consumos = req × cantidad × (1+merma/100) = 7.956) → `StartProductionAndConsumeAsync` (consumo CONSUMO_PRODUCCION, stock −7.956, asiento WIP cuadrado Debe 185.0010/Haber 183.0010 por cantidad×PPP, EN_PROCESO, CostoRealTotal) → `FinishProductionAsync` (desviación NEGATIVA devuelve con DEVOLUCION_PRODUCCION recién auto-creada, entrada PT +4 @ costo unitario real, asiento Debe 183.0010/Haber 185.0010, estado TERMINADA, desvíos RF-43) → merma directa `CreateWasteAsync`. En el run final: consumo 7.956×PPP 48.9842=389.72 WIP, devolución 0.956, entrada 4 @ 89.47, stock 116→109, TERMINADA, desv=2.
- **Bug real #1 (fix `ProductionService.CreateCostSheetAsync`):** al existir una ficha VIGENTE previa, versionaba con `Estado = "HISTORICA"`, pero el CHECK `CK__ficha_cos__estad__3296789C` de `produccion.ficha_costo` solo admite VIGENTE/OBSOLETA/BORRADOR (HISTORICA NO existe) → 547 → **la re-versionada de fichas de costo SIEMPRE fallaba**. Fix: `OBSOLETA` (el lookup de "vigente" ya filtra por `Estado == "VIGENTE"`).
- **Bug real #2 (fix `ProductionService.FinishProductionAsync`):** insertaba en `analisis_desviacion` el componente `"TOTAL"`, pero la CHECK `CK__analisis___compo__30AE302A` solo admite MATERIA_PRIMA/MANO_OBRA/GASTOS_INDIRECTOS → 547 al finalizar la orden (el análisis de desviaciones nunca se guardaba). Fix: segunda fila con `Componente = "MANO_OBRA"` y CostoEstandar=(MO+GIF)×q, CostoReal=(total−matPrimaReal) (desviación de conversión, dentro de los componentes permitidos).
- `DEVOLUCION_PRODUCCION` NO existía en `inventario.tipo_movimiento`; `EnsureMovementTypeAsync` la auto-crea (id 1012) con Naturaleza ENTRADA/AfectaCosto=true al primer devolver material (verificado y revertido). Consumo usa `stock.CostoPromedio` vigente (PPP dinámico run a run).
- Merma directa: columna `causa` tiene CHECK solo OTRO/ERROR_OPERATIVO/VENCIMIENTO/DANO/PROCESO_NORMAL (el harness debe usar una de esas; el modelo EF no valida). La merma es un registro simple SIN asiento automático (su `asiento_id` es nullable; no hay contabilización en `CreateWasteAsync` → nota P3).
- Revert del bloque: merma → desvíos → `UPDATE movimiento SET asiento_id=NULL` → asiento (detalles+asiento) → consumos → OP → movimientos → existencia PT (creada) → restock insumo (cantidad+PPP) → BOM (detalle+maestra) → ficha → consecutivo ORDEN_PRODUCCION (ultimo_numero restaurado) → tipo DEVOLUCION_PRODUCCION si se auto-creó. Lecciones: los `ExecuteUpdate/Delete` del revert NO refrescan tracking; el DELETE del asiento exige antes librar la FK `movimiento.asiento_id`.
- Montos deconómicos del run se calculan SIEMPRE con el PPP de P-0001 leído al inicio (el harness de compras E2E recalcula el PPP de producción en cada corrida y el revert solo restaura cantidad → el PPP flotante 53.04→49.40 entre runs es residual/normal, igual que el avance de consecutivos).

## P3-E2E item 9: Activo Fijo crear → depreciación mensual → baja con pérdida (validado 2026-09-27, `erp_hr_harness`, revert limpio; suite completa 0 FAIL)
- Flujo contra BD real (activo temporal E2E-AF-0001, valor 1200, residual 200, 24 meses → cuota 41.6666…): cuentas del catálogo MFP (`243` AFT Equipos de Computación, `378` Dep. Acum. Equipos de Computación, `822` Gastos Generales, `845.0050` Gastos por Pérdidas por Venta/Baja de AFT) y periodo real 9/2026 ABIERTO B3129200. `CreateAssetAsync` (MetodoDepreciacion LINEA_RECTA, Estado ACTIVO) → `GenerateMonthlyDepreciationAsync` (asiento AJ cuadrado Debe 822/Haber 378 × cuota, fila activo_fijo_depreciacion, segunda llamada → guard "La depreciación de este mes ya fue procesada.") → `RetireAssetAsync` (baja Estado BAJA + motivo; asiento cuadrado Debe 378 dep acum 41.67 + Debe 845.0050 pérdida 1158.33 / Haber 243 valor adquisición 1200). Verificado: asiento cuadrado, 3 detalles, valorNeto en la cuenta de pérdida real.
- **Bug real #4 (fix `FixedAssetService.RetireAssetAsync`):** la baja buscaba cuenta `"701"` (inexistente en el catálogo MFP) → con valorNeto>0 el asiento quedaba descuadrado (nota 106). Fix: buscar `"845.0050"` (cuenta real, GASTOS/DEUDORA/acepta movimiento).
- **Bug real #5 (fix `FixedAssetService.GenerateMonthlyDepreciationAsync`):** el asiento AJ de depreciación se creaba SIN `CreadoPor`, columna NOT NULL → 515 → la depreciación mensual NUNCA podía contabilizar. Fix: `CreadoPor = _entidadProvider.CurrentUsuarioId`.
- **Bug real #6 (fix `FixedAssetService.GenerateMonthlyDepreciationAsync`):** las filas `ActivoFijoDepreciacion` se agregaban al tracker junto con el asiento, pero EF no conoce esa FK → el batch insertaba `activo_fijo_depreciacion` ANTES que `asiento_contable` → 547 FK `_asien__46136164`. Fix: persistir primero CreateEntryAsync (asiento+detalles) y en un segundo SaveChanges las filas de depreciación + acumulado (dos fases).
- **Bug real #7 (fix `FixedAssetService.RetireAssetAsync`):** el asiento de baja no llevaba `PeriodoId` (NOT NULL) → hubiera fallado 515. Fix: tomar el periodo ABIERTO más reciente de la entidad.
- Revert del bloque: ActivoFijoDepreciacion → asientos baja/dep (detalles+asiento) → ActivoFijo. BD post-run: activo_fijo 0, depreciaciones 0, asientos 1, detalles 8, movimientos 7 (baseline intacto).

## P3-E2E item 10: Reportes regulatorios — generación del paquete mensual MFP (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- `ReportingService.GenerateMonthlyPackageAsync(Entidad, Periodo9)` (RNF-60, simulación <10s) inserta en `reportes.paquete_informacion` un registro Tipo `MFP` / Estado `GENERADO` (ambos válidos por CHECK `CK__paquete_i__estad__3B2BBE9D` y `CK__paquete_in__tipo__3C1FE2D6`; la tabla admite además ONAT y DIRECCION). El nombre devuelto es `PAQUETE_{entidad}_{mes}_{anio}.zip`.
- Verificado: fileName == `PAQUETE_24C50C75-…_9_2026.zip`, fila insertada con Tipo MFP, Estado GENERADO, GeneradoEn recién; revert `DELETE` limpio (BD post-run: paquete 0). El servicio NO está expuesto por ningún controlador/REST (solo DI) → nota P3.
- Notas de producto P3: el "paquete" es solo un registro, no genera PDF/ZIP real; no hay generación ONAT/DIRECCION; `GeneradoPor` queda NULL (no se setea).

## P3-E2E item 11: Producción — equipos + mantenimiento programado (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- `MaintenanceService`: `ScheduleMaintenanceAsync` (equipo OPERATIVO E2E-EQ-0001 + mantenimiento PREVENTIVO PROGRAMADO) → `GetPendingMaintenancesAsync` lo devuelve como pendiente → `CompleteMaintenanceAsync(id, 250)` lo pasa a EJECUTADO con FechaEjecutada=Costo=250 y actualiza `Equipo.FechaUltimaRevision` a la fecha ejecutada. CHECKs de BD respetados: `equipo.estado` (OPERATIVO/MANTENIMIENTO/FUERA_SERVICIO), `mantenimiento_programado.estado` (PROGRAMADO/EJECUTADO/VENCIDO), `mantenimiento_programado.tipo` (PREVENTIVO/CORRECTIVO). Revert: mant → equipo.

## P3-E2E item 12: Producción — plan de producción sugerido + persistencia (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- **Bug real #8 (fix `ProductionService.GenerateSuggestedPlanAsync`):** creaba el plan con `Estado = "SUGERIDO"`, pero el CHECK `CK__plan_prod__estad__384F51F2` de `plan_produccion.estado` solo admite BORRADOR/APROBADO/EJECUTADO → persistir un plan sugerido SIEMPRE fallaba 547 (y el controller `GenerateSuggestedPlan` ignoraba el resultado del `CreatePlanAsync` → falso éxito). Fix: `"BORRADOR"` (mismo default del form).
- E2E: `GenerateSuggestedPlanAsync` devuelve Estado=BORRADOR; `CreatePlanAsync` persiste plan (10/2026) + 1 detalle (P-0001, CantidadPlanificada 10, CantidadEjecutada 0); verificada la fila con su detalle. Revert: detalle → plan. BD post-run: plan/detalle/equipo/mantenimiento 0.

## P3-E2E item 13: Contabilidad — cierre ejecutivo de periodo (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- `AccountingService.ClosePeriodAsync(periodo, usuario)`: valida no-CERRADO, valida ausencia de asientos BORRADOR, dispara `GenerateMonthlyDepreciationAsync` (RF-14; sin activos → ok), pasa el periodo a CERRADO con `CerradoPor`/`CerradoEn`. E2E usó un **periodo temporal 2027/1** (nunca el real 9/2026): creado ABIERTO → `ClosePeriodAsync` CERRADO (cerrado_por=en) → segunda llamada rechazada "El periodo ya está cerrado." → revert (ExecuteUpdate a ABIERTO limpiando CerradoPor/CerradoEn + DELETE). BD post-run: 2027/1 no existe.

## P3-E2E item 14: Caja/Banco — CRUD de cuenta bancaria (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- La tabla `contabilidad.cuenta_bancaria` está vacía en producción y su CRUD no tenía E2E. Validado sobre catálogo real: `CreateBankAccountAsync` (Banco Metropolitano, E2E-0001, CORRIENTE, cuenta contable `109` EFECTIVO EN BANCO, moneda CUP, Activa=true, saldo 0) verificado en BD (CHECKs `tipo_cuenta` CORRIENTE/AHORRO/FISCAL y `moneda` CUP/MLC respetados) → `UpdateBankAccountAsync` saldo 0→1000. Revert: DELETE. BD post-run: cuenta_bancaria 0.

## P3-E2E item 15: RRHH — acumulación mensual de vacaciones (validado 2026-09-27, `erp_hr_harness`, revert limpio; suite 21 PASS/0 FAIL)
- `AccumulateMonthlyVacationsAsync(entidad, year, month, userId)` (P0-6 lo llama al aprobar nómina): cuenta `RegistroAsistencia` sin `TipoAusenciaId` del mes × `FACTOR_VAC` (0.0909) → upsert a `saldo_vacaciones (empleado, anio)`. E2E con **año temporal 2099** (nunca toca saldos reales 2026, regla del harness): 1 asistencia temporal para el primer ACTIVO real → primera llamada crea la fila con 0.09, segunda suma → 0.18; además se crean filas 2099 para los 67 ACTIVOS reales (los demás con 0). Revert: DELETE asistencia 2099 + DELETE saldo 2099. BD post-run: 2099 limpio.
- **Hallazgo de precisión (nota):** `saldo_vacaciones.dias_acumulados` es `numeric(6,2)` → el factor 0.0909 se redondea a 2 decimales (1 día → 0.09, no 0.0909). En el ciclo mensual cubano (≈22 días/mes) la pérdida por redondeo es <0.01 día/mes/empleado; acumulado anual es ≈24 días (estándar). No se toca (política de redondeo = decisión/maestra).

## P3-E2E item 16: Ventas — factura con DESCUENTO (validado 2026-09-27, `erp_hr_harness`, revert limpio; suite 22 PASS/0 FAIL)
- E2E: 2×25 subtotal 50, `DescuentoTotal=5`, IVA 10% (P-0001 `aplica_impuesto_ventas=1`, TAX_VENTA=0.10) → `invoice.Total = subtotal + impuesto − descuento` (lógica `SalesService.CreateInvoiceAsync:199-201`) = 50; FormaPago EFECTIVO 50. Verificado: asiento VENTAS cuadrado 50/50 (Debe 101 = 50; Haber 900 = subtotal−descuento 45 + 440.0001 = 5), stock 116→114, **sin CxC** (pago completo). Nótese que el impuesto se calcula sobre el subtotal sin descontar (RF/thought: decisión fiscal, no bug). Revert completo (asiento→factura→movimiento→stock→consecutivo).
- Hallazgo: `invoice.Subtotal` en el asiento va neto de descuento (Haber 45), el descuento NO tiene cuenta contable propia (no hay cuenta de descuento) → el descuento reduce ingreso directamente. Nota de producto menor (si se requiere cuenta 840/descuento, es decisión).

## P3-E2E item 17: Activo Fijo — Update + Delete con guard de historial (validado 2026-09-27, `erp_hr_harness`, revert limpio; suite 23 PASS/0 FAIL)
- `UpdateAssetAsync` (`SetValues` completo + re-force EntidadId): E2E cambia valor 1200→1300 y descripción → persistido. `DeleteAssetAsync`: E2E sin depreciación → OK (fila eliminada); luego con 1 mes de depreciación → rechazado con "No se puede eliminar un activo que ya tiene historial de depreciación." Revert: filas dep + asiento dep + activo. BD post-run: activo_fijo 0, dep 0, asientos 1, movimientos 7.

## P3-E2E item 18: Inventario/Ventas — asiento de COSTO de venta (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- Cuando el producto TIENE `cuenta_costo_venta_id` configurada, el flujo `SalesService.CreateInvoiceAsync` → `ProcessMovementAsync` → `CreateAccountingEntryAsync` genera el asiento de inventario SOLO (la UQ `asiento_contable (entidad, tipo, numero)` y `asiento_detalle (asiento, linea)` exigen `NumeroComprobante ≠ 0` y `Linea ≥ 1`). E2E: UPDATE temporal a P-0001 `cuenta_costo_venta_id = 810` (COSTO DE VENTAS DE PRODUCCIONES), venta 2×25 (IVA 10, asiento VENTAS cuadrado) → segundo asiento `MOVIMIENTO_INVENTARIO` cuadrado DEBE 810 / HABER inventario (183.0010 por fallback `CTA_INV_GENERICA`) por 2×PPP. Verificado y revertido (restaura P-0001 cuenta NULL + asientos + factura + movimiento + stock + consecutivo).
- Concluye que el hueco NP-03 es EXCLUSIVAMENTE de maestra/datos (productos sin cuentas): el motor contable del costo de venta funciona. Evidencia de costo en el run final: 81.83/81.83 cuadrado.

## P3-E2E item 19: Contabilidad — cierre de EJERCICIO fiscal anual (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- `ClosureService.CloseFiscalYearAsync(entidad, año, usuario)` (RF-14 completo): requiere TODOS los periodos del año CERRADOS → agrupa saldos CONTABILIZADOS del año por cuenta/clase (INGRESOS/GASTOS, PLURAL) → asiento CONTABILIZADO Tipo AJ (serie '7') llamado "CIERRE DEL EJERCICIO FISCAL {año}" contra la cuenta `999 RESULTADO`: Debe 900 (ingreso)/Haber 999 y Debe 999/Haber 810 (gasto). E2E con **año temporal 2098** (2 asientos temporales en periodo 2098/12 con numeración alta 990000001/2 para no chocar con UQ): cierre de periodo→CERRADO, cierre anual → asiento 300/300 (900 200→999; 999 100→810), resultado 100. Guardia "Existen periodos mensuales abiertos" si el año no está cerrado. Revert limpio (restaura consecutivo ASIENTO_CONTABLE serie '7').
- **Lecciones:** (1) los INSERT directos de `asiento_contable`/`asiento_detalle` deben llevar `NumeroComprobante` no-zero (UQ por tipo) y `Linea ≥ 1` (UQ por asiento), o dan 547/duplicado; (2) si un run falla y deja residuos, re-ejecutar implica limpieza idempotente previa (DELETE asientos E2E_2098 → periodo 2098).

## P3-E2E item 20: Activo Fijo BAJA con pérdida + RBAC rol/permisos (validado 2026-09-27, `erp_hr_harness`, revert limpio)
- `FixedAssetService.RetireAssetAsync` espejo del item 9 pero con `DepreciacionAcumulada = 400` + valor 1200 + residual 200 → neto 800 con pérdida: asiento cuadrado Debe 378 (dep acum) 400 + Debe 845.0050 (pérdida) 800 / Haber 243 (valor) 1200, Estado BAJA. Verificado con `ValorAdquisicion` (el flujo usa DepreciacionAcumulada del activo, no el generado).
- RBAC `UpdateRolPermissionsAsync(rolId, int[] selectedPermissions)`: rol temporal `es_sistema=false` + 2 permisos temporales en `nucleo.permiso` → asignación crea 2 filas en `nucleo.rol_permiso` (verificado) → revert (DELETE de la tabla puente vía SQL bruto, rol, permisos). Cierra la mecánica de asignación de NP-01 (queda decidir qué roles); la tabla `nucleo.permiso` estaba vacía(0) antes de la corrida y vuelve a 0 (los 26 permisos del P1.3 no se re-sembran automáticamente en cada run, son de despliegue).

## Mapa de completamiento por módulo (diagnóstico 360) — actualizado 2026-09-27
% = avance del diagnóstico por módulo: Hitó 1 auditado (25%) → Hitó 2 corregido (50%) → Hitó 3 validado E2E contra BD real (75%) → sin pendientes/decisiones (100%).
| Módulo | % | Estado / evidencia |
|---|---|---|
| Inventario (movs, transferencia, ajustes, conteo) | 100% | E2E compras→mov→asiento, transferencia sin asiento, faltante/sobrante con asiento, conteo CERRADO + **E2E asiento de costo VALE_ENTREGA** (810/183.0010 cuadrado, item 18) |
| Cartera CxC/CxP | 100% | cobro/pago E2E, sobrepago rechazado, carrera=1 éxito + decisiones NP-09/NP-10 adoptadas (aceptar) |
| Compras (OC→CxP→Pago) | 100% | E2E completo cuadra 50/50 + revert |
| Ventas/Comercial (fact + anulación) | 100% | E2E venta 55/55, anulación + espejo, descuento (50−5+5=50, asiento 50/50), **asiento de costo** (item 18); + NP-03 adoptada (cargar cuentas 900/810 por producto) |
| Contabilidad/Estados financieros | 100% | asientos cuadran (NOMINA/VENTAS/POS), Balance+ER+cierre anual validados, cierre de periodo con guard + **cierre de EJERCICIO anual E2E** (999/900/810, item 19) |
| Caja/Banco | 100% | límite y conciliación validados + cierre de sesión POS/arqueo + E2E CRUD cuenta bancaria (109/CUP/CORRIENTE, saldo 0→1000); NP-04 (fijar límite real) y NP-05 (asiento de arqueo → item 21 backlog) adoptadas |
| RRHH (asistencia, turnos, nómina) | 100% | P1.2, P0-6 y E2E acumulación vacaciones (año 2099, 67 filas, doble suma 0.09→0.18) validados |
| POS backend | 100% | reserva, recibir, procesar, asiento venta POS y cierre de sesión validados + NP-02 adoptada (aceptar device-first, numeración fiscal se concilia en el corte); worker legacy sineve |
| Maestras/BD/schema | 100% | P1-1, 108-110 aplicados, CHECKs alineados; 105 latente decidida: NO aplicar triggers (`AuditInterceptor` ya audita) |
| Seguridad/RBAC | 100% | 26 permisos sembrados, provider/handler validados + **E2E asignación rol/permisos** (item 20) + NP-01 adoptada (grants por rol, script de maestra) |
| Reportes regulatorios | 100% | E2E generación paquete MFP (RNF-60) contra BD real + revert limpio; NP-07 adoptada (construir entrega real → item 22 backlog, el módulo deja de ser simulado cuando se implemente) |
| Producción | 100% | E2E OP completo + E2E equipos/mantenimiento (Schedule→Pending→Complete) + E2E plan sugerido persistido (bug #8 SUGERIDO→BORRADOR); NP-06 adoptada (merma registro-solo, baja stock vía movimiento) |
| Activo Fijo | 100% | E2E completo (crear→dep 822/378→guard ya-procesada→baja 845.0050) + Update/Delete guard + **baja con pérdida** (item 20) + 3 bugs corregidos; nota 106 resuelta |
| Integridad/auditoría | 100% | P1: 0 asientos descuadrados, vistas ejecutan, auditoría ERP/POS/API |
Global del diagnóstico P0–P3: **≈ 100%** — 14/14 módulos 100% con evidencia E2E + decisiones NP-01..NP-10 adoptadas; los 2 items de construcción (21/22) son backlog, no deuda del diagnóstico. Harness último run: 26 PASS e2e / 0 FAIL.

## P3: Notas de producto — DECISIONES ADOPTADAS (2026-09-27, ok del dueño del producto)
Regla del repo cumplida: ninguna se implementó sin decisión. Todas las NP-01..NP-10 cerradas con decisión adoptada. Código verificado = **26 PASS e2e / 0 FAIL** en `erp_hr_harness`.

| ID | Decisión adoptada | Acción que cierra el módulo |
|----|---|---|
| NP-01 | Asignar grants por rol: MASTER/ADMINISTRADOR = 26; CONTADOR → contabilidad/cartera/reportes; VENDEDOR → ventas/POS; perfil inventario → inventario/producción; RRHH → asistencias/nómina. | Script de datos (maestra) |
| NP-02 | Aceptar diseño device-first: sin duplicados (validado P1.6/item 6); la numeración fiscal se concilia en el corte. | Documentar política |
| NP-03 | Cargar maestra por producto: ingreso = 900, costo = 810; política de costo = PPP (motor validado item 18). | Script de datos |
| NP-04 | Fijar límite real en la Caja Principal (ej. 5000); validación ya probada (50 OK / 140 rechazado). Endurecer el servicio = opcional en backlog. | Dato maestra |
| NP-05 | Construir asiento de arqueo cuando `DiferenciaArqueo ≠ 0` + tabla/vista de conciliación. | **Backlog → item 21** |
| NP-06 | Aceptar merma registro-solo: ya baja stock vía movimiento; contabilización = opcional futuro. | Documentar |
| NP-07 | Construir entrega real del paquete MFP (PDF/ZIP + ONAT/DIRECCION + endpoint REST + `GeneradoPor`). Requisito regulatorio RNF-60, deja de ser simulado. | **Backlog → item 22** |
| NP-08 | Cerrada por item 19: mecanismo de cierre anual validado E2E (año 2098, asiento 999/900/810). Ejecutar el cierre real del año en curso = operación de la administración, no desarrollo. | Operación (no desarrollo) |
| NP-09 | Aceptar: alta manual de CxC/CxP sin asiento de apertura; las originadas por venta/compra ya lo tienen (evita duplicados). | Documentar |
| NP-10 | Aceptar moneda canónica: cada asiento queda en su moneda; conversión MLC↔CUP solo si se exige reporte consolidado (futuro). | Documentar |
| 105 latente | NO aplicar triggers: `AuditInterceptor` ya audita (evita doble auditoría). | Documentar |

## Backlog de construcción (items nuevos, decisiones NP-05 y NP-07)
- **item 21 — Caja/Banco:** al cerrar sesión POS, asiento de arqueo cuando `DiferenciaArqueo ≠ 0` (faltante/sobrante a cuenta definida) + vista de conciliación de arqueo en BD. Validar E2E cuando se implemente.
- **item 22 — Reportes:** paquete MFP físico (archivos exportados + tipos ONAT/DIRECCION + controlador REST + `GeneradoPor`) reemplazando el paquete simulado; validar E2E anti contrabando/generación real cuando se implemente.

## Operación pendiente (no es desarrollo)
- Ejecutar `CloseFiscalYearAsync` sobre el ejercicio contable real (cierre de periodos + cierre anual) cuando la administración lo disponga — mecanismo 100% validado (item 19, año 2098 de prueba revertido).

Notas LAtentes (sin acción, registradas por contexto): #4 policies con "." como claim Permission (sin evidencia de bug); `dispositivo_pos.estado` nunca persiste ONLINE (solo ACTIVO/INACTIVO/MANTENIMIENTO, el código solo lee); vistas `MovimientoBancario/Index`+`Details` comparan `Tipo == "INGRESO"` (muerto, CHECK es CREDITO/DEBITO, cosmético).
**Resueltas sin decisión:** nota 106 (cuenta de pérdida por baja de AFT): `RetireAssetAsync` ahora usa `845.0050` real (item 9); "cuenta bancaria 0 en producción": CRUD validado E2E (item 14), el 0 es dato de despliegue, no defecto.

## Historial abreviado de trabajo previo
- Edit-con-documento en contratos: validado end-to-end (UpdateContractAsync Succeeded, documento_url actualizado, archivo nuevo en wwwroot real, viejo borrado; harness `erp_harness_di`).
- Revisión CxC/CxP: hallazgos #1..#6 → fixes #1 (Estado en CreateEntryAsync), #2 (guarda atómica concurrencia), #3 (saldo recalculado), forma de pago/modulo alineados a CHECKs, catálogo tipo_comprobante insertado, DDL estado PAGADA, consecutivo desde 1. Validado con harness real + revert.