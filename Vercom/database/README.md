# Base de Datos - Vercom ERP

Scripts SQL para la creación, mantenimiento y limpieza de la base de datos del ERP.

## Estructura

| Carpeta | Propósito |
| :--- | :--- |
| `scripts/` | Scripts de infraestructura (esquema, limpieza) |
| `seed/` | Scripts de datos iniciales (catálogos oficiales) |

## Orden de ejecución

```bash
# 1. Crear esquema completo
sqlcmd -S localhost -U sa -P 'tu_password' -i scripts/01-schema.sql

# 2. (Opcional) Cargar plan de cuentas MFP completo
sqlcmd -S localhost -U sa -P 'tu_password' -i seed/cuentas-mfp.sql