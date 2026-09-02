# Plan de Desarrollo ERP — "Tierra Prometida S.U.R.L." (Metodología XP)

## 1. Contexto y marco regulatorio cubano
Antes de definir módulos, el sistema debe alinearse con la normativa vigente:
- **Normas Cubanas de Contabilidad (NCC)** y Normas Cubanas de Información Financiera (NCIF), emitidas por el Ministerio de Finanzas y Precios (MFP).
- **Estados financieros obligatorios:** balance general, estado de resultados (pérdidas y ganancias), estado de flujos de efectivo y notas/informes complementarios.
- **Nomenclador de cuentas del MFP** (clasificador de cuentas para el sector empresarial y las mipymes/S.U.R.L.).
- **Obligaciones fiscales ante la ONAT:** Ley 113 del Sistema Tributario (impuesto sobre utilidades, sobre ventas/servicios, contribución a la seguridad social, impuesto por la utilización de fuerza de trabajo).
- **Código de Trabajo (Ley 116/2013)** y su reglamento para el módulo de RR.HH. y nóminas.
- **Resolución 60/2011 de la CGR** (Contraloría General de la República): sistema de control interno — trazabilidad, separación de funciones y auditoría.
- **Legislación contra el lavado de activos:** trazabilidad total e inmutabilidad de asientos.
- **Bancarización:** operaciones en CUP, conciliación con pasarelas nacionales (Transfermóvil, EnZona) y cuentas fiscales bancarias.

## 2. Adaptación de XP al proyecto
| Práctica XP | Aplicación en el proyecto |
| :--- | :--- |
| **Releases pequeños** | Un release funcional por módulo (cada 4–6 semanas). |
| **Cliente en sitio** | Contador y administrador de la S.U.R.L. validan historias de usuario semanalmente. |
| **TDD** | Pruebas unitarias obligatorias en lógica contable y de nómina. |
| **Refactorización** | Continua, protegida por la suite de pruebas. |

## 3. Módulos y Requisitos Críticos

### Módulo 0 — Núcleo, Seguridad y Administración
- **RF-03:** Registro de trazas de auditoría (quién, qué, cuándo) inmutable.
- **RF-05:** Copias de seguridad y restauración.

### Módulo 1 — Contabilidad y Finanzas
- **RF-11:** Registro de asientos por partida doble con validación de cuadre; **asientos automáticos desde los demás módulos**.
- **RF-12:** Comprobantes no editables tras contabilizarse (solo reversión).
- **RF-17:** Cálculo y liquidación de obligaciones fiscales ONAT; generación de DJ.

### Módulo 2 — Recursos Humanos y Nómina
- **RF-21:** Control de asistencia; vacaciones (acumulación del 9,09%).
- **RF-24:** Generación de **comprobante contable automático** hacia el Módulo 1.

### Módulo 3 — Inventario y Almacén
- **RF-32:** Métodos de valuación conformes a las NCC (**PPP**).
- **RF-35:** **Contabilización automática** de movimientos hacia el Módulo 1.

### Módulo 4 — Producción y Manufactura
- **RF-41:** Órdenes de producción con consumo de materiales (**BOM**) descontado del inventario.
- **RF-42:** Traslado contable automático de producción en proceso y terminada.

### Módulo 5 — Compras y Ventas
- **RF-51:** Órdenes de compra vinculadas al almacén y a **cuentas por pagar**.
- **RF-52:** Facturación de ventas con requisitos fiscales cubanos.
- **RF-55:** Control de precios según normativas del MFP y topes vigentes.

### Módulo 6 — Reportes e Inteligencia
- **RF-60:** Tablero de indicadores: liquidez, rentabilidad, rotación de inventarios.
