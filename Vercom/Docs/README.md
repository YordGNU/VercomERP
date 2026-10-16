# VERCOMERP – Documentación

ERP empresarial para **S.U.R.L.**  
Stack: **ASP.NET Core 10 + SQL Server 2022 + Docker + Nginx**

## 📚 Índice

| Documento | Contenido |
|---|---|
| [01 - Arquitectura](01-ARQUITECTURA.md) | Diseño del sistema, componentes, redes |
| [02 - Instalación](02-INSTALACION.md) | Setup desde cero en un servidor nuevo |
| [03 - Deploy](03-DEPLOY.md) | Procedimiento de despliegue automatizado |
| [04 - Operaciones](04-OPERACIONES.md) | Tareas del día a día |
| [05 - Backup y Restore](05-BACKUP-RESTORE.md) | Respaldos y recuperación |
| [06 - Troubleshooting](06-TROUBLESHOOTING.md) | Problemas comunes y soluciones |
| [07 - Mantenimiento](07-MANTENIMIENTO.md) | Tareas periódicas (semanal/mensual) |
| [08 - Seguridad](08-SEGURIDAD.md) | Hardening y buenas prácticas |
| [09 - Comandos](09-COMANDOS.md) | Chuleta de comandos rápidos |

## 🚀 Inicio rápido

```bash
# Deploy completo
cd /opt/VERCOM
./scripts/deploy.sh --init-db --skip-build

# Ver estado
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps

# Ver logs
docker logs VERCOM-app -f