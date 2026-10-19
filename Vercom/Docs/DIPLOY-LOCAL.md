# VERCOMERP – Levantar stack en local (Windows)

**Versión:** 3.0.0  
**Última actualización:** 2026-10-16  
**Sistema:** Windows 10/11 + Docker Desktop  
**Duración estimada:** 5 minutos (primera vez) / 1 minuto (subsecuentes)

---

## 📑 Índice

1. [Prerrequisitos](#1-prerrequisitos)
2. [Verificar archivos](#2-verificar-archivos)
3. [⚠️ Fixes críticos de código (primera vez)](#3-️-fixes-críticos-de-código-primera-vez)
4. [Reset total (opcional)](#4-reset-total-opcional)
5. [Levantar stack por fases](#5-levantar-stack-por-fases)
6. [Verificar](#6-verificar)
7. [Acceso a servicios](#7-acceso-a-servicios)
8. [Comandos frecuentes](#8-comandos-frecuentes)
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
docker version
docker compose version
cd C:\Users\Usuario\source\repos\Vercom\Vercom
pwd
```

**Esperado:**
- Docker version 27.x o superior
- Docker Compose version v2.x
- Ubicación: `C:\Users\Usuario\source\repos\Vercom\Vercom`

---

## 2. Verificar archivos

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
    ".dockerignore",
    "Vercom.csproj",
    "Services\SeedData.cs",
    "Program.cs",
    "nginx\nginx.conf",
    "nginx\conf.d\proxy_params.conf"
)

foreach ($a in $archivos) {
    if (Test-Path $a) {
        Write-Host "  OK   $a" -ForegroundColor Green
    } else {
        Write-Host "  FALTA  $a" -ForegroundColor Red
    }
}
```

### Verificar `.env` — ⚠️ PASSWORD case-sensitive

```powershell
Get-Content .env
```

**Contenido esperado:**

```env
MSSQL_SA_PASSWORD=Vercom#2026!StrongPass
MSSQL_PID=Developer
APP_VERSION=2.0.0
JWT_ISSUER=VERCOMERP
JWT_AUDIENCE=VERCOMERPClientes
GRAFANA_PASSWORD=admin
```

**⚠️ CRÍTICO:** la password es **`VERCOM`** (mayúscula), no `Vercom`. SQL Server es case-sensitive. Usar exactamente el mismo valor en TODOS los comandos.

### Verificar secreto JWT

```powershell
Get-Item secrets\jwt_key.txt | Select-Object Name, Length
```

**Esperado:** ~65 bytes.

---

## 3. ⚠️ Fixes críticos de código (primera vez)

**Aplicar SOLO la primera vez, ANTES del primer deploy.**

### Fix 1 – `SeedData.cs` con `IgnoreQueryFilters()`

**Problema:** Error 2627 (UNIQUE violation) al reiniciar la app.

**Script:**

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

$file = "Services\SeedData.cs"
$content = Get-Content $file -Raw

$content = $content -replace 'var sucursal = await context\.Sucursals\.FirstOrDefaultAsync', 'var sucursal = await context.Sucursals.IgnoreQueryFilters().FirstOrDefaultAsync'
$content = $content -replace 'if \(!await context\.Cargos\.AnyAsync', 'if (!await context.Cargos.IgnoreQueryFilters().AnyAsync'
$content = $content -replace 'if \(!await context\.Almacens\.AnyAsync', 'if (!await context.Almacens.IgnoreQueryFilters().AnyAsync'
$content = $content -replace 'if \(!await context\.CuentaContables\.AnyAsync', 'if (!await context.CuentaContables.IgnoreQueryFilters().AnyAsync'
$content = $content -replace 'if \(!await context\.ListaPrecios\.AnyAsync', 'if (!await context.ListaPrecios.IgnoreQueryFilters().AnyAsync'
$content = $content -replace 'if \(!await context\.ParametroSistemas\.AnyAsync', 'if (!await context.ParametroSistemas.IgnoreQueryFilters().AnyAsync'
$content = $content -replace 'if \(!await context\.ConceptoNominas\.AnyAsync', 'if (!await context.ConceptoNominas.IgnoreQueryFilters().AnyAsync'

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText("$PWD\$file", $content, $utf8NoBom)

Write-Host "Verificar cambios:" -ForegroundColor Cyan
(Select-String -Path $file -Pattern "IgnoreQueryFilters" | Measure-Object).Count
```

**Esperado:** 7+ resultados con `IgnoreQueryFilters`.

### Fix 2 – `Program.cs` con `KnownNetworks`

**Problema:** Error 400 (Antiforgery) porque .NET no confía en el proxy Nginx.

**En `Program.cs`**, reemplazar:

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
```

**Por:**

```csharp
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                     | ForwardedHeaders.XForwardedProto
                     | ForwardedHeaders.XForwardedHost
};

forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();

forwardedOptions.KnownNetworks.Add(
    new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
        System.Net.IPAddress.Parse("172.16.0.0"), 12));
forwardedOptions.KnownNetworks.Add(
    new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
        System.Net.IPAddress.Parse("10.0.0.0"), 8));
forwardedOptions.KnownNetworks.Add(
    new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
        System.Net.IPAddress.Parse("192.168.0.0"), 16));

app.UseForwardedHeaders(forwardedOptions);
```

### Fix 3 – `Dockerfile` con case correcto

**Problema:** Linux es case-sensitive. `vercom.csproj` ≠ `Vercom.csproj`.

**Verificar:**

```powershell
Select-String -Path Dockerfile -Pattern "csproj"
```

**Esperado:** `Vercom.csproj` (V mayúscula) en TODAS las líneas.

**Si hay minúsculas, corregir:**

```powershell
$content = Get-Content Dockerfile -Raw
$content = $content -replace 'vercom\.csproj', 'Vercom.csproj'
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText("$PWD\Dockerfile", $content, $utf8NoBom)
```

### Fix 4 – `nginx/nginx.conf` con buffers del cliente

**Problema:** Error 400 (Request Header Or Cookie Too Large).

**Añadir dentro del bloque `http`:**

```nginx
client_header_buffer_size 16k;
large_client_header_buffers 4 32k;
client_max_body_size 50M;
client_body_buffer_size 256k;
```

### Fix 5 – `nginx/conf.d/proxy_params.conf` con buffers del upstream

**Problema:** Error 502 (upstream sent too big header).

**Verificar que tenga:**

```nginx
proxy_buffer_size       16k;
proxy_buffers           4 16k;
proxy_busy_buffers_size 32k;
proxy_buffering         on;
```

### Fix 6 – `docker-compose.yml` con password variable

**Problema:** Password hardcodeada en connection string.

**Verificar que la connection string use `${MSSQL_SA_PASSWORD}`:**

```yaml
ConnectionStrings__DefaultConnection: "Server=vercom-db,1433;Database=VercomERP;UID=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true;Language=us_english"
```

### Fix 7 – Rebuild de la imagen después de los fixes

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

Write-Host "===== REBUILD =====" -ForegroundColor Cyan
docker build -t vercom-erp:2.0.0 -t vercom-erp:latest .
```

**Esperado:** `transferred context: ~5 MB` (con `.dockerignore`).

---

## 4. Reset total (opcional)

**Solo si algo falla sin arreglo** (borra todos los datos):

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

Write-Host "===== RESET TOTAL =====" -ForegroundColor Cyan

# 1. Bajar todo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down

# 2. Borrar contenedores
$containers = docker ps -aq --filter "name=vercom"
if ($containers) { docker rm -f $containers 2>$null }

# 3. Borrar volúmenes
docker volume rm vercom-mssql-data vercom_uploads-data vercom_grafana-data `
    vercom_loki-data vercom_prometheus-data vercom_nginx-logs 2>$null

# 4. Borrar red
docker network rm vercom-network 2>$null

# 5. Recrear volúmenes
foreach ($vol in @(
    "vercom-mssql-data",
    "vercom_uploads-data",
    "vercom_grafana-data",
    "vercom_loki-data",
    "vercom_prometheus-data",
    "vercom_nginx-logs"
)) {
    docker volume create $vol | Out-Null
}

Write-Host ""
Write-Host "===== VOLÚMENES =====" -ForegroundColor Cyan
docker volume ls --filter "name=vercom" --format "  {{.Name}}"

Write-Host ""
Write-Host "RESET COMPLETADO" -ForegroundColor Green
```

---

## 5. Levantar stack por fases

**⚠️ IMPORTANTE:** El stack debe levantarse **por fases**. Intentar levantar todo de golpe falla porque Nginx espera a la app, y la app espera a la BD.

### Fase 5.1 – Levantar SQL Server primero

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

Write-Host "Levantando SQL Server..." -ForegroundColor Yellow
docker compose -f docker-compose.yml up -d vercom-db

Write-Host ""
Write-Host "Esperando healthy (max 60s)..." -ForegroundColor Yellow
$waited = 0
while ($waited -lt 60) {
    $status = docker inspect vercom-db --format '{{.State.Health.Status}}' 2>$null
    if ($status -eq "healthy") {
        Write-Host ""
        Write-Host "OK SQL Server healthy (${waited}s)" -ForegroundColor Green
        break
    }
    Start-Sleep -Seconds 5
    $waited += 5
    Write-Host "." -NoNewline
}

if ($waited -ge 60) {
    Write-Host ""
    Write-Host "XX SQL Server no llegó a healthy" -ForegroundColor Red
    docker logs vercom-db --tail 20
    exit 1
}
```

### Fase 5.2 – Crear BD + aplicar schema

**⚠️ Usar COMILLAS SIMPLES para la password** (evita que `!` se interprete en PowerShell):

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

$SA_PASS = 'Vercom#2026!StrongPass'   # ⚠️ COMILLAS SIMPLES

Write-Host "[1/3] Creando BD VercomERP..." -ForegroundColor Yellow
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C `
    -Q "IF DB_ID('VercomERP') IS NULL CREATE DATABASE VercomERP"

Write-Host "[2/3] Copiando schema..." -ForegroundColor Yellow
docker cp Database\scripts\01-schema.sql vercom-db:/tmp/_schema.sql

Write-Host "[3/3] Aplicando schema..." -ForegroundColor Yellow
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C -b -d VercomERP `
    -i /tmp/_schema.sql

if ($LASTEXITCODE -ne 0) {
    Write-Host "XX Error aplicando schema" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "===== VERIFICAR =====" -ForegroundColor Cyan
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C -h -1 -d VercomERP `
    -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"

Write-Host "OK Schema aplicado" -ForegroundColor Green
```

**Esperado:** `99` tablas.

### Fase 5.3 – Levantar stack completo

```powershell
cd C:\Users\Usuario\source\repos\Vercom\Vercom

Write-Host "Levantando stack completo..." -ForegroundColor Yellow
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d

Write-Host ""
Write-Host "Esperando 60s..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

Write-Host ""
Write-Host "===== ESTADO =====" -ForegroundColor Cyan
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps
```

**Si `vercom-nginx` queda en `Created`:**

```powershell
# ⚠️ El SERVICIO se llama "nginx" (no "vercom-nginx")
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --force-recreate nginx
Start-Sleep -Seconds 15
docker compose ps
```

---

## 6. Verificar

### Estado del stack

```powershell
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps
```

**Esperado:** 8 servicios.

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

### Datos en BD (⚠️ comillas simples)

```powershell
$SA_PASS = 'Vercom#2026!StrongPass'

docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C -h -1 -d VercomERP `
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

### Login

```powershell
Start-Process "msedge.exe" --inprivate "https://localhost"
```

**Credenciales:**
- `master` / `MasterVercom2026*`

---

## 7. Acceso a servicios

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

1. Clic en **"Avanzado"**
2. Clic en **"Continuar a localhost (no seguro)"**
3. O escribir: `thisisunsafe` en Chrome/Edge

### DNS local (opcional)

Para usar `vercom.local`:

```powershell
# Como Admin
Start-Process notepad -Verb RunAs "C:\Windows\System32\drivers\etc\hosts"
```

Añadir:
```
127.0.0.1    vercom.local
```

---

## 8. Comandos frecuentes

### Levantar / bajar

```powershell
# Levantar stack completo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d

# Bajar (conserva volúmenes)
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml down

# Reiniciar un SERVICIO específico (usar nombre del SERVICIO, no del contenedor)
docker compose -f docker-compose.yml -f docker-compose.prod.yml restart vercom-app
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --force-recreate nginx
```

### ⚠️ Nombres correctos

| Servicio (compose) | Contenedor |
|---|---|
| `vercom-app` | `vercom-app` |
| `vercom-db` | `vercom-db` |
| **`nginx`** | `vercom-nginx` |
| `prometheus` | `vercom-prometheus` |
| `grafana` | `vercom-grafana` |
| `loki` | `vercom-loki` |
| `promtail` | `vercom-promtail` |
| `node-exporter` | `vercom-node-exporter` |

### Estado

```powershell
docker compose ps
docker stats --no-stream
docker system df
```

### Logs

```powershell
docker logs vercom-app --tail 50
docker logs vercom-app -f
docker logs vercom-nginx --tail 50
docker logs vercom-db --tail 30
```

### Shell en contenedor

```powershell
# App (Alpine)
docker exec -it vercom-app sh

# BD (SQL Server CLI) - ⚠️ comillas simples
docker exec -it vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P 'Vercom#2026!StrongPass' -C
```

### Consultas rápidas a BD (⚠️ comillas simples)

```powershell
$SA_PASS = 'Vercom#2026!StrongPass'

# Contar tablas
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C -h -1 -d VercomERP `
    -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"

# Ver usuarios
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASS -C -h -1 -d VercomERP `
    -Q "SET NOCOUNT ON; SELECT nombre_usuario, nombre_completo FROM nucleo.usuario"
```

---

## 9. Troubleshooting

### ❌ Error 18456: Login failed for user 'sa'

**Causa:** Password case-sensitive. `.env` dice `VERCOM`, comandos dicen `Vercom`.

**Solución:**

1. Verificar password real:
   ```powershell
   Get-Content .env | Select-String "MSSQL_SA_PASSWORD"
   ```
2. Usar **comillas simples** en PowerShell:
   ```powershell
   $SA_PASS = 'Vercom#2026!StrongPass'
   docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASS -C -Q "SELECT 1"
   ```

### ❌ Error 2627: Violación de UNIQUE KEY

**Síntoma:**
```
Msg 2627, Level 14, State 1
Violation of UNIQUE KEY constraint
   at Vercom.Services.SeedData.Initialize(...) in /src/Services/SeedData.cs:line 71
```

**Causa:** `SeedData.cs` no usa `IgnoreQueryFilters()`.

**Solución:** Aplicar el [Fix 1](#fix-1--seeddatacs-con-ignorequeryfilters) + rebuild + reset BD.

### ❌ Error 4060: Cannot open database "VercomERP"

**Causa:** La BD no existe.

**Solución:** Ejecutar la [Fase 5.2](#fase-52--crear-bd--aplicar-schema).

### ❌ Error 400: Bad Request (Antiforgery)

**Causa:** Nginx en `172.19.0.x`, .NET solo confía en `127.0.0.1`.

**Solución:** Aplicar el [Fix 2](#fix-2--programcs-con-knownnetworks).

### ❌ Error 502: Bad Gateway

**Causa:** Headers de Identity > 4 KB.

**Solución:** Aplicar el [Fix 5](#fix-5--nginxconfdproxy_paramsconf-con-buffers-del-upstream).

### ❌ Error 400: Request Header Or Cookie Too Large

**Causa 1:** Buffers del cliente en Nginx pequeños.
**Causa 2:** Cookies acumuladas en el navegador.

**Solución:**

1. Aplicar el [Fix 4](#fix-4--nginxnginxconf-con-buffers-del-cliente)
2. **Limpiar cookies del navegador:**
   - `edge://settings/content/all` → buscar `localhost` → eliminar todo
   - O `Ctrl+Shift+Supr` → "Todos los períodos" → Cookies → Borrar

### ❌ `no such service: vercom-nginx`

**Causa:** El SERVICIO en compose se llama `nginx`.

**Solución:**

```powershell
# ❌ Mal
docker compose restart vercom-nginx

# ✅ Bien
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --force-recreate nginx

# O por contenedor
docker restart vercom-nginx
```

### ❌ Dockerfile: `COPY ["vercom.csproj", ...] not found`

**Causa:** Linux es case-sensitive.

**Solución:** Aplicar el [Fix 3](#fix-3--dockerfile-con-case-correcto).

### ❌ `external volume "vercom-mssql-data" not found`

**Causa:** Volumen marcado como `external: true` pero no existe.

**Solución:**

```powershell
foreach ($vol in @(
    "vercom-mssql-data", "vercom_uploads-data", "vercom_grafana-data",
    "vercom_loki-data", "vercom_prometheus-data", "vercom_nginx-logs"
)) { docker volume create $vol }
```

### ❌ `vercom-nginx` en estado `Created`

**Causa:** Nginx espera a la app healthy.

**Solución:**

```powershell
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --force-recreate nginx
```

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

### ❌ `Access to the path '/run/secrets/jwt_key' is denied`

**Causa:** Permisos del secreto.

**Solución:** En Windows, Docker Desktop traduce permisos — suele funcionar. Verificar:

```powershell
Get-Content secrets\jwt_key.txt
```

### ❌ `vercom-app is unhealthy`

**Causa:** App no puede conectar a BD o falla en seed.

**Solución:**

```powershell
docker logs vercom-app --tail 50
docker logs vercom-app 2>&1 | Select-String "error|exception" -CaseSensitive:$false
```

### ❌ SSL certificate problem

**Solución:**

1. Avanzado → Continuar en el navegador
2. O `thisisunsafe` en Chrome/Edge
3. O instalar cert (admin):
   ```powershell
   Import-Certificate -FilePath "nginx\certs\vercom.crt" -CertStoreLocation Cert:\LocalMachine\Root
   ```

### ❌ Docker tarda en arrancar

**Causa:** Docker Desktop con poca RAM.

**Solución:** Docker Desktop → Settings → Resources → subir RAM a 8 GB.

---

## 📋 Checklist de arranque

```
[ ] Docker Desktop corriendo
[ ] Carpeta correcta (Vercom/Vercom)
[ ] Fixes 1-7 aplicados (primera vez)
[ ] Imagen reconstruida (docker build)
[ ] 6 volúmenes creados
[ ] SQL Server healthy
[ ] BD VercomERP creada + schema (99 tablas)
[ ] App healthy
[ ] Nginx healthy (usar "nginx", no "vercom-nginx")
[ ] https://localhost/health → Healthy
[ ] Login con master funciona
[ ] Prometheus targets UP
[ ] Grafana accesible

```

---

## 🎯 Resumen — Comandos esenciales

```powershell
# Ir al proyecto
cd C:\Users\Usuario\source\repos\Vercom\Vercom

# 1. Levantar SQL Server
docker compose -f docker-compose.yml up -d vercom-db
Start-Sleep -Seconds 30

# 2. Crear BD + schema (⚠️ comillas simples para la password)
$SA_PASS = 'Vercom#2026!StrongPass'
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASS -C -Q "IF DB_ID('VercomERP') IS NULL CREATE DATABASE VercomERP"
docker cp Database\scripts\01-schema.sql vercom-db:/tmp/_schema.sql
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASS -C -b -d VercomERP -i /tmp/_schema.sql

# 3. Levantar stack completo
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d
Start-Sleep -Seconds 60

# 4. Verificar
docker compose ps
curl.exe -k https://localhost/health

# 5. Abrir en navegador (incógnito)
Start-Process "msedge.exe" --inprivate "https://localhost"
```

---
**Fin del documento.**