# VercomERP – Levantar stack en local (Windows)

**Última actualización:** 2026-10-15  
**Sistema:** Windows 10/11 + Docker Desktop  
**Duración estimada:** 2-3 minutos

---

## 📑 Índice

1. [Prerrequisitos](#1-prerrequisitos)
2. [Verificar archivos](#2-verificar-archivos)
3. [Limpiar stack anterior](#3-limpiar-stack-anterior)
4. [Levantar stack](#4-levantar-stack)
5. [Verificar](#5-verificar)
6. [Acceso a servicios](#6-acceso-a-servicios)
7. [Comandos frecuentes](#7-comandos-frecuentes)
8. [Reset completo](#8-reset-completo)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Prerrequisitos

### Software requerido

| Herramienta | Versión mínima | Verificar |
|---|---|---|
| Windows | 10/11 | `winver` |
| Docker Desktop | 4.x | `docker version` |
| PowerShell | 5.1+ | `$PSVersionTable` |
| SSH client | OpenSSH | `ssh -V` |

### Verificación rápida

```powershell
# Docker funcionando
docker version

# Docker Compose disponible
docker compose version

# Ubicación correcta
cd C:\Users\Usuario\source\repos\Vercom\Vercom
pwd
```

**Esperado:**
- Docker version 27.x o superior
- Docker Compose version v2.x
- Ubicación: `C:\Users\Usuario\source\repos\Vercom\Vercom`

---

## 2. Verificar archivos

Los siguientes archivos son **obligatorios**:

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

Write-Host "===== ARCHIVOS CLAVE =====" -ForegroundColor Cyan

$archivos = @(
    "docker-compose.yml",
    "docker-compose.prod.yml",
    "docker-compose.monitoring.yml",
    ".env",
    "secrets\jwt_key.txt",
    "Database\scripts\01-schema.sql",
    "Dockerfile",
    "Vercom.csproj"
)

foreach ($a in $archivos) {
    if (Test-Path $a) {
        Write-Host "  OK   $a" -ForegroundColor Green
    } else {
        Write-Host "  FALTA  $a" -ForegroundColor Red
    }
}
```

### Verificar `.env`

```powershell
Get-Content .env
```

**Contenido esperado:**

```env
MSSQL_SA_PASSWORD=Vercom#2026!StrongPass
MSSQL_PID=Developer
APP_VERSION=latest
ASPNETCORE_ENVIRONMENT=Production
JWT_ISSUER=VercomERP
JWT_AUDIENCE=VercomERPClientes
GRAFANA_PASSWORD=admin
```

### Verificar secreto JWT

```powershell
Get-Item secrets\jwt_key.txt | Select-Object Name, Length
```

**Esperado:** ~65 bytes.

---

## 3. Limpiar stack anterior

Si había un stack corriendo previamente:

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

# Bajar stack (conserva volúmenes)
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down

# Borrar contenedores huérfanos de vercom
$containers = docker ps -aq --filter "name=vercom"
if ($containers) { docker rm -f $containers }

# Verificar
docker ps -a --filter "name=vercom"
```

**Esperado:** sin contenedores vercom.

---

## 4. Levantar stack

### Opción A — Stack completo con monitoring (recomendado)

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d
```

**Levanta 8 servicios:**
- vercom-app (ASP.NET Core)
- vercom-db (SQL Server)
- vercom-nginx (HTTPS proxy)
- vercom-prometheus (métricas)
- vercom-grafana (dashboards)
- vercom-loki (logs)
- vercom-promtail (colector)
- vercom-node-exporter (métricas host)

### Opción B — Solo app + db (más ligero)

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

docker compose up -d
```

**Levanta solo:** app + db (2 servicios).

---

## 5. Verificar

### Estado del stack

```powershell
# Esperar arranque
Start-Sleep -Seconds 60

# Ver estado
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps
```

**Esperado:**

```
NAME                    STATUS
vercom-app              Up (healthy)
vercom-db               Up (healthy)
vercom-nginx            Up (healthy)
vercom-grafana          Up
vercom-loki             Up
vercom-node-exporter    Up
vercom-prometheus       Up
vercom-promtail         Up
```

### Health de la app

```powershell
curl.exe -k https://localhost/health
```

**Esperado:** `Healthy`

### Datos en BD

```powershell
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Vercom#2026!StrongPass" -C -h -1 -d VercomERP `
    -Q "SET NOCOUNT ON;
        SELECT 'Tablas:   ' + CAST(COUNT(*) AS VARCHAR(10)) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'
        UNION ALL SELECT 'Roles:    ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.rol
        UNION ALL SELECT 'Permisos: ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.permiso
        UNION ALL SELECT 'Usuarios: ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.usuario"
```

**Esperado:**
```
Tablas:   99
Roles:    15
Permisos: 180
Usuarios: 2
```

### Logs de la app (SeedData)

```powershell
docker logs vercom-app 2>&1 | Select-String "Seed|roles|usuarios|completado" | Select-Object -First 10
```

**Esperado:**
```
🌱 Iniciando seed de datos base...
   ✅ 166 permisos procesados
   ✅ 12 roles procesados
   ✅ Usuarios master y admin creados
   ✅ Seed de datos base completado exitosamente
```

---

## 6. Acceso a servicios

### URLs

| Servicio | URL | Credenciales |
|---|---|---|
| **App** | `https://localhost` | master / `MasterVercom2026*` |
| **Health** | `https://localhost/health` | — |
| **Prometheus** | `http://localhost:9090` | — |
| **Grafana** | `http://localhost:3000` | admin / admin |
| **Loki API** | `http://localhost:3100` | — |

### Aceptar certificado autofirmado

**Al abrir `https://localhost`:**

1. Chrome/Edge mostrará advertencia "Tu conexión no es privada"
2. Clic en **"Avanzado"**
3. Clic en **"Continuar a localhost (no seguro)"**

**Alternativa rápida (Chrome/Edge):**
- En la pantalla de advertencia, escribir: `thisisunsafe`

### DNS local (opcional)

Para usar `vercom.local` en lugar de `localhost`:

```powershell
# Como Admin
Start-Process notepad -Verb RunAs "C:\Windows\System32\drivers\etc\hosts"
```

Añadir al final:
```
127.0.0.1    vercom.local
```

Guardar y acceder a `https://vercom.local`.

---

## 7. Comandos frecuentes

### Levantar / bajar

```powershell
# Levantar stack completo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d

# Bajar (conserva volúmenes)
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down

# Reiniciar todo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml restart
```

### Estado

```powershell
# Ver contenedores
docker compose ps

# Ver recursos
docker stats

# Ver uso de disco
docker system df
```

### Logs

```powershell
# App
docker logs vercom-app --tail 50
docker logs vercom-app -f

# BD
docker logs vercom-db --tail 30

# Nginx
docker logs vercom-nginx --tail 30

# Todos
docker compose logs -f
```

### Reiniciar un servicio

```powershell
docker compose restart vercom-app
docker compose restart vercom-db
docker compose restart vercom-nginx
```

### Shell en contenedor

```powershell
# App (Alpine)
docker exec -it vercom-app sh

# BD (SQL Server CLI)
docker exec -it vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Vercom#2026!StrongPass" -C
```

### Consultas rápidas a BD

```powershell
# Contar tablas
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Vercom#2026!StrongPass" -C -h -1 -d VercomERP `
    -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
```

---

## 8. Reset completo

⚠️ **Borra todos los datos.** Solo usar cuando algo falla sin arreglo.

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

# 1. Bajar todo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down

# 2. Borrar contenedores
docker rm -f $(docker ps -aq --filter "name=vercom") 2>$null

# 3. Borrar volúmenes
docker volume rm `
    vercom-mssql-data `
    vercom_uploads-data `
    vercom_grafana-data `
    vercom_loki-data `
    vercom_prometheus-data `
    vercom_nginx-logs 2>$null

# 4. Borrar red
docker network rm vercom-network 2>$null

# 5. Levantar de nuevo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d

# 6. Esperar
Start-Sleep -Seconds 90

# 7. Verificar
docker compose ps
```

---

## 9. Troubleshooting

### ❌ `port is already allocated`

**Causa:** Otro servicio usa el puerto 80/443/1433.

**Solución:**

```powershell
# Ver qué usa el puerto
netstat -ano | Select-String ":80 |:443 |:1433 "

# Matar el proceso
Stop-Process -Id <PID> -Force
```

O cambiar el puerto en `docker-compose.prod.yml`.

### ❌ `external volume "vercom-mssql-data" not found`

**Causa:** Volumen marcado como `external: true` pero no existe.

**Solución:**

```powershell
foreach ($vol in @(
    "vercom-mssql-data",
    "vercom_uploads-data",
    "vercom_grafana-data",
    "vercom_loki-data",
    "vercom_prometheus-data",
    "vercom_nginx-logs"
)) {
    docker volume create $vol
}
```

### ❌ `Login failed for user 'sa'`

**Causa:** Contraseña del volumen ≠ del `.env`.

**Solución:** reset completo (sección 8).

### ❌ `Access to the path '/run/secrets/jwt_key' is denied`

**Causa:** Permisos del secreto.

**Solución:**

```powershell
# El contenedor corre como UID 1654
# En Windows, Docker Desktop traduce permisos — suele funcionar
# Si falla, verificar el archivo existe y tiene contenido
Get-Content secrets\jwt_key.txt
```

### ❌ `vercom-app is unhealthy`

**Causa:** App no puede conectar a BD o falla en seed.

**Solución:**

```powershell
# Ver logs
docker logs vercom-app --tail 50
docker logs vercom-app 2>&1 | Select-String "error|exception" -CaseSensitive:$false
```

### ❌ `vercom-nginx` en estado `Created`

**Causa:** Nginx espera a que la app esté healthy.

**Solución:**

```powershell
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d vercom-nginx
```

### ❌ Docker tarda en arrancar

**Causa:** Docker Desktop con poca RAM asignada.

**Solución:** Docker Desktop → Settings → Resources → subir RAM a 8 GB.

### ❌ SSL certificate problem

**Causa:** Certificado autofirmado.

**Solución:**
1. En el navegador: Avanzado → Continuar
2. O `thisisunsafe` en Chrome/Edge
3. O instalar cert como confiable (admin):
   ```powershell
   Import-Certificate -FilePath "nginx\certs\vercom.crt" -CertStoreLocation Cert:\LocalMachine\Root
   ```

### ❌ `Cannot open database "VercomERP"`

**Causa:** La BD no existe.

**Solución:**

```powershell
# Ejecutar init-db manualmente
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Vercom#2026!StrongPass" -C `
    -Q "CREATE DATABASE VercomERP"

# Copiar y aplicar schema
docker cp Database\scripts\01-schema.sql vercom-db:/tmp/schema.sql
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Vercom#2026!StrongPass" -C -d VercomERP `
    -i /tmp/schema.sql
```

### ❌ Error 502 Bad Gateway al hacer login

**Síntoma:**

En el navegador aparece "502 Bad Gateway" al enviar las credenciales de login.

En los logs de Nginx:

```
[error] upstream sent too big header while reading response header from upstream
```

**Causa:**

Los headers de respuesta de ASP.NET Core Identity son **más grandes que los buffers por defecto de Nginx** (4 KB). Cada login genera múltiples cookies (autenticación, antiforgery, sesión) que suman 5-8 KB.

**Solución:**

Añadir a `nginx/conf.d/proxy_params.conf`:

```nginx
proxy_buffer_size       16k;
proxy_buffers           4 16k;
proxy_busy_buffers_size 32k;
proxy_buffering         on;
```

Reiniciar Nginx:

```bash
docker compose restart vercom-nginx
```

**Verificación:**

```bash
curl -k https://localhost/health
# Esperado: Healthy
```

Luego reintentar el login en el navegador.
---

## 📋 Checklist de arranque

```
[ ] Docker Desktop corriendo
[ ] Carpeta correcta (Vercom/Vercom)
[ ] Archivos obligatorios presentes
[ ] .env con MSSQL_SA_PASSWORD
[ ] secrets/jwt_key.txt existe
[ ] Stack anterior bajado
[ ] docker compose up -d ejecutado
[ ] 8 servicios corriendo
[ ] vercom-app healthy
[ ] vercom-db healthy
[ ] vercom-nginx healthy
[ ] https://localhost/health responde
[ ] Datos en BD (99 tablas)
```

---

## 🎯 Resumen — Comandos esenciales

```powershell
# Ir al proyecto
cd C:\Users\Usuario\source\repos\Vercom\Vercom

# Levantar todo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d

# Esperar y verificar
Start-Sleep -Seconds 60
docker compose ps

# Ver logs
docker logs vercom-app -f

# Bajar
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down
```

---

**Fin del documento.**