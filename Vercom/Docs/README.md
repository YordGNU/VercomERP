# VERCOMERP – Documentación

ERP empresarial para **S.U.R.L.**  
Stack: **ASP.NET Core 10 + SQL Server 2022 + Docker + Nginx**

## 📚 Índice

| Documento | Contenido |
|---|---|
| [Levantar el stack en local](DIPLOY-LOCAL.md) | Stack completo en Windows con Docker Desktop |
| [Despliegue en VM](DIPLOY-VM.md) | Publicación en servidor Debian + Nginx |
| [README del proyecto](../../README.md) | Módulos, arquitectura, terminal POS web y pruebas |
| [Memoria de trabajo](../../AGENTS.md) | Decisiones, hallazgos y estado por módulo |

> Los documentos numerados `01-ARQUITECTURA` … `09-COMANDOS` que figuraban aquí
> nunca llegaron al repositorio; su contenido vive hoy en el README del proyecto,
> en `AGENTS.md` y en los dos procedimientos de despliegue de esta carpeta.

## 🚀 Inicio rápido

```bash
# Deploy completo
cd /opt/VERCOM
./scripts/deploy.sh --init-db --skip-build

# Ver estado
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps

# Ver logs
docker logs VERCOM-app -f