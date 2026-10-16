# VERCOMERP – Guía Completa de Instalación y Operación

**Versión:** 2.0.0  
**Última actualización:** 2026-10-15  
**Autor:** YordGNU  

---

## 📑 Tabla de contenidos

1. [Visión general](#1-visión-general)
2. [Arquitectura](#2-arquitectura)
3. [Requisitos previos](#3-requisitos-previos)
4. [Instalación desde cero](#4-instalación-desde-cero)
5. [Configuración inicial](#5-configuración-inicial)
6. [Deploy automatizado](#6-deploy-automatizado)
7. [Verificación](#7-verificación)
8. [Operaciones diarias](#8-operaciones-diarias)
9. [Backup y restore](#9-backup-y-restore)
10. [Troubleshooting](#10-troubleshooting)
11. [Mantenimiento](#11-mantenimiento)
12. [Seguridad](#12-seguridad)
13. [Chuleta de comandos](#13-chuleta-de-comandos)

---

## 1. Visión general

**VERCOMERP** es un sistema ERP empresarial para **S.U.R.L.**, diseñado bajo normativa cubana (MFP/ONAT/CGR).

### Stack tecnológico

| Componente | Tecnología |
|---|---|
| Backend | ASP.NET Core 10 (C#) |
| Base de datos | SQL Server 2022 |
| Frontend | Razor Views + Tabler UI |
| Reverse proxy | Nginx 1.27 |
| Contenedores | Docker + Docker Compose |
| Hypervisor | Proxmox VE 8.x |
| Monitoring | Prometheus + Grafana + Loki |

### URLs de acceso

| Servicio | URL |
|---|---|
| Aplicación | `https://VERCOM-erp.lan` |
| Health check | `https://VERCOM-erp.lan/health` |
| Prometheus | `http://192.x.x.220:9090` |
| Grafana | `http://192.x.x.220:3000` |
| Loki (API) | `http://192.x.x.220:3100` |

### Credenciales por defecto

| Servicio | Usuario | Password |
|---|---|---|
| App (master) | `master` | `MasterVERCOM2026*` |
| App (admin) | `admin` | `admin123*` |
| SQL Server | `sa` | Según `.env` |
| Grafana | `admin` | Según `.env` (por defecto `admin`) |

---

## 2. Arquitectura

### Diagrama de componentes

```
┌─────────────────────────────────────────────────────────────┐
│  Dell PowerEdge T620 – Proxmox VE                          │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │  OPNsense    │  │  Pi-hole     │                        │
│  │  192.x.x.1 │  │  192.x.x.2 │                        │
│  └──────────────┘  └──────────────┘                        │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  VM VERCOM-erp  (Debian 13)  – 192.x.x.220         │  │
│  │                                                      │  │
│  │  Red Docker: VERCOM-network (bridge)                 │  │
│  │                                                      │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────┐       │  │
│  │  │ nginx    │─▶│ app      │─▶│ db           │       │  │
│  │  │ :443     │  │ :8080    │  │ :1433        │       │  │
│  │  └──────────┘  └──────────┘  └──────────────┘       │  │
│  │                                                      │  │
│  │  ┌──────────┐ ┌───────┐ ┌─────────┐                 │  │
│  │  │prometheus│ │loki   │ │grafana  │                 │  │
│  │  │ :9090    │ │ :3100 │ │ :3000   │                 │  │
│  │  └──────────┘ └───────┘ └─────────┘                 │  │
│  │  ┌──────────┐ ┌─────────────┐                       │  │
│  │  │promtail  │ │node-exporter│                       │  │
│  │  │          │ │ :9100       │                       │  │
│  │  └──────────┘ └─────────────┘                       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Componentes Docker

| Servicio | Imagen | Puerto | Propósito |
|---|---|---|---|
| VERCOM-nginx | nginx:1.27-alpine | 80, 443 | Reverse proxy + HTTPS |
| VERCOM-app | VERCOM-erp:latest | 8080 | API + UI (ASP.NET Core 10) |
| VERCOM-db | mssql/server:2022-latest | 1433 | Base de datos |
| VERCOM-prometheus | prom/prometheus:v2.55.0 | 9090 | Métricas |
| VERCOM-grafana | grafana/grafana:11.3.0 | 3000 | Dashboards |
| VERCOM-loki | grafana/loki:3.2.1 | 3100 | Almacenamiento de logs |
| VERCOM-promtail | grafana/promtail:3.2.1 | — | Colector de logs |
| VERCOM-node-exporter | prom/node-exporter:v1.8.2 | 9100 | Métricas del host |

### Volúmenes persistentes

| Volumen | Contenido |
|---|---|
| `VERCOM-mssql-data` | Datos de SQL Server |
| `VERCOM_uploads-data` | Archivos subidos por usuarios |
| `VERCOM_grafana-data` | Configuración de Grafana |
| `VERCOM_loki-data` | Índice y datos de logs |
| `VERCOM_prometheus-data` | Series temporales de Prometheus |
| `VERCOM_nginx-logs` | Access/error logs de Nginx |

---

## 3. Requisitos previos

### Hardware (VM)

| Recurso | Mínimo | Recomendado |
|---|---|---|
| vCPU | 4 | 4 |
| RAM | 8 GB | 8 GB |
| Disco | 100 GB | 200 GB |

### Software

- Proxmox VE 8.x (o cualquier hypervisor)
- Debian 13 (Trixie) en la VM
- Windows 10/11 en la máquina de desarrollo
- Docker Desktop en Windows (para build)
- SSH con llaves configurado

### Red

| Elemento | Valor |
|---|---|
| Subred LAN | `192.x.x.0/24` |
| Gateway | `192.x.x.1` |
| DNS (Pi-hole) | `192.x.x.2` |
| VM VERCOM-erp | `192.x.x.220` |

---

## 4. Instalación desde cero

### Paso 4.1 – Crear la VM en Proxmox

```bash
# Descargar ISO de Debian 13
cd /var/lib/vz/template/iso/
wget https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-13.6.0-amd64-netinst.iso

# Crear VM (ID 104)
qm create 104 \
  --name VERCOM-erp \
  --memory 8192 \
  --cores 4 \
  --sockets 1 \
  --cpu host \
  --ostype l26 \
  --net0 virtio,bridge=vmbr0 \
  --scsihw virtio-scsi-single \
  --scsi0 local-lvm:200,discard=on,ssd=1 \
  --ide2 local:iso/debian-13.6.0-amd64-netinst.iso,media=cdrom \
  --boot order=ide2 \
  --bootdisk scsi0 \
  --agent enabled=1 \
  --onboot 1

qm start 104
```

### Paso 4.2 – Instalar Debian 13

En la consola de la VM:

1. **Install** → Language English
2. **Hostname:** `VERCOM-erp`
3. **Domain:** `lan`
4. **Root password:** vacío (usar sudo)
5. **User:** `administrador` + password
6. **Timezone:** `America/Havana`
7. **Partitioning:** Guided - use entire disk
8. **Software:** marcar solo `SSH server`
9. **GRUB:** sí, en `/dev/sda`

Después del reboot, quitar la ISO:

```bash
qm stop 104
qm set 104 --ide2 none,media=cdrom
qm start 104
```

### Paso 4.3 – Configurar red

```bash
ssh administrador@192.x.x.220

sudo nano /etc/network/interfaces
```

```ini
auto lo
iface lo inet loopback

auto ens18
iface ens18 inet static
    address 192.x.x.220/24
    gateway 192.x.x.1
    dns-nameservers 192.x.x.2 1.1.1.1
    dns-search lan
```

```bash
sudo systemctl restart networking
ip -4 addr show ens18 | grep inet
```

### Paso 4.4 – Configurar hostname y hosts

```bash
sudo hostnamectl set-hostname VERCOM-erp.lan

sudo nano /etc/hosts
```

```
127.0.0.1       localhost
127.0.1.1       VERCOM-erp.lan VERCOM-erp
192.x.x.220   VERCOM-erp.lan VERCOM-erp
```

### Paso 4.5 – Actualizar sistema

```bash
sudo apt update
sudo apt upgrade -y
sudo apt install -y curl wget vim htop net-tools ca-certificates gnupg lsb-release openssl qemu-guest-agent
sudo systemctl enable --now qemu-guest-agent
```

### Paso 4.6 – SSH hardening

**En Windows:**

```powershell
# Generar llave si no existe
ssh-keygen -t ed25519 -C "admin@VERCOM-erp"

# Copiar al servidor
type $HOME\.ssh\id_ed25519.pub | ssh administrador@192.x.x.220 "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 700 ~/.ssh && chmod 600 ~/.ssh/authorized_keys"
```

**En la VM:**

```bash
sudo nano /etc/ssh/sshd_config.d/99-hardening.conf
```

```
PasswordAuthentication no
PubkeyAuthentication yes
PermitRootLogin no
MaxAuthTries 3
ClientAliveInterval 300
ClientAliveCountMax 2
```

```bash
sudo systemctl restart ssh
```

### Paso 4.7 – Firewall UFW

```bash
sudo apt install -y ufw

# SSH primero (crítico)
sudo ufw allow 22/tcp

# HTTP/HTTPS desde LAN
sudo ufw allow from 192.x.x.0/24 to any port 80 proto tcp
sudo ufw allow from 192.x.x.0/24 to any port 443 proto tcp

# Activar
sudo ufw --force enable
sudo ufw status verbose
```

### Paso 4.8 – Fail2ban

```bash
sudo apt install -y fail2ban

sudo nano /etc/fail2ban/jail.local
```

```ini
[DEFAULT]
bantime  = 3600
findtime = 600
maxretry = 3

[sshd]
enabled = true
port    = 22
logpath = %(sshd_log)s
maxretry = 3
bantime = 86400
```

```bash
sudo systemctl enable --now fail2ban
sudo fail2ban-client status sshd
```

### Paso 4.9 – Instalar Docker

```bash
# Repositorio oficial
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
     https://download.docker.com/linux/debian $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
     | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Usuario en grupo docker
sudo usermod -aG docker $USER
newgrp docker

docker --version
docker compose version
```

### Paso 4.10 – Estructura de carpetas

```bash
sudo mkdir -p /opt/VERCOM
sudo chown $USER:$USER /opt/VERCOM
cd /opt/VERCOM

mkdir -p secrets nginx/certs nginx/conf.d
mkdir -p Database/scripts Database/seed
mkdir -p monitoring/prometheus monitoring/loki monitoring/promtail
mkdir -p monitoring/grafana/provisioning/datasources
mkdir -p monitoring/grafana/provisioning/dashboards
mkdir -p monitoring/grafana/dashboards
mkdir -p scripts backups docs
```

---

## 5. Configuración inicial

### Paso 5.1 – Transferir proyecto desde Windows

**En Windows:**

```powershell
cd C:\Users\Usuario\source\repos\VERCOM\VERCOM

tar -czf VERCOM-src.tar.gz `
    --exclude="*/bin" `
    --exclude="*/obj" `
    --exclude="*/.vs" `
    --exclude="*/secrets" `
    --exclude="*.env" `
    VERCOM.csproj Program.cs appsettings*.json `
    Dockerfile .dockerignore `
    Controllers Models Views Services Data Helpers Security `
    Filters Hubs Middleware DTOs ViewModels wwwroot Properties `
    Database/scripts/01-schema.sql `
    Database/seed/demo-data.sql `
    docker-compose.yml docker-compose.prod.yml docker-compose.monitoring.yml `
    nginx monitoring

scp VERCOM-src.tar.gz administrador@192.x.x.220:/tmp/
```

**En la VM:**

```bash
cd /opt/VERCOM
tar -xzf /tmp/VERCOM-src.tar.gz
rm /tmp/VERCOM-src.tar.gz
```

### Paso 5.2 – Transferir scripts

**Desde Windows:**

```powershell
scp scripts\deploy-remote.sh scripts\init-db.sh administrador@192.x.x.220:/tmp/
```

**En la VM:**

```bash
mv /tmp/deploy-remote.sh /tmp/init-db.sh /opt/VERCOM/scripts/
chmod +x /opt/VERCOM/scripts/*.sh
```

### Paso 5.3 – Crear `.env`

```bash
cat > /opt/VERCOM/.env << 'EOF'
# ============================================================
# VERCOMERP – Variables de entorno
# ============================================================

MSSQL_SA_PASSWORD=VERCOM#2026!StrongPass
MSSQL_PID=Developer

APP_VERSION=latest
ASPNETCORE_ENVIRONMENT=Production

JWT_ISSUER=VERCOMERP
JWT_AUDIENCE=VERCOMERPClientes

GRAFANA_PASSWORD=admin
EOF

chmod 600 /opt/VERCOM/.env
```

### Paso 5.4 – Crear secreto JWT

```bash
openssl rand -base64 48 > /opt/VERCOM/secrets/jwt_key.txt
chmod 600 /opt/VERCOM/secrets/jwt_key.txt
sudo chown 1654:1654 /opt/VERCOM/secrets/jwt_key.txt
chmod 400 /opt/VERCOM/secrets/jwt_key.txt

ls -la /opt/VERCOM/secrets/
```

### Paso 5.5 – Generar certificados SSL

```bash
cd /opt/VERCOM/nginx/certs

openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout VERCOM.key -out VERCOM.crt \
    -subj "/C=CU/ST=La Habana/L=Vedado/O=Yord GNU/CN=VERCOM-erp.lan" \
    -addext "subjectAltName=DNS:VERCOM-erp.lan,DNS:VERCOM-erp,DNS:localhost,IP:192.x.x.220,IP:127.0.0.1"

chmod 600 VERCOM.key
```

---

## 6. Deploy automatizado

### Desde Windows (recomendado)

```powershell
cd C:\Users\Usuario\source\repos\VERCOM\VERCOM

# Deploy completo con reset de BD
.\scripts\deploy.ps1 -SkipBuild -InitDb -ForceDb

# Deploy normal (rebuild + sin reset)
.\scripts\deploy.ps1

# Deploy rápido (sin rebuild)
.\scripts\deploy.ps1 -SkipBuild
```

### Directo en la VM

```bash
cd /opt/VERCOM

./scripts/deploy.sh                         # deploy completo
./scripts/deploy.sh --init-db               # con init BD
./scripts/deploy.sh --init-db --force-db    # reset BD
./scripts/deploy.sh --skip-build            # sin rebuild
./scripts/deploy.sh --no-monitoring         # sin monitoring
```

---

## 7. Verificación

### Estado del stack

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps
```

Esperado: 8 servicios corriendo, 3 healthy (app, db, nginx).

### Health de la app

```bash
curl -k https://localhost/health
# Esperado: Healthy
```

### Datos en BD

```bash
source /opt/VERCOM/.env

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -d VERCOMERP \
    -Q "SET NOCOUNT ON;
        SELECT 'Tablas:   ' + CAST(COUNT(*) AS VARCHAR(10)) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'
        UNION ALL SELECT 'Roles:    ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.rol
        UNION ALL SELECT 'Permisos: ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.permiso
        UNION ALL SELECT 'Usuarios: ' + CAST(COUNT(*) AS VARCHAR(10)) FROM nucleo.usuario"
```

Esperado: Tablas: 99, Roles: 15, Permisos: 180, Usuarios: 2.

### DNS en Windows (opcional)

```
C:\Windows\System32\drivers\etc\hosts
```

Añadir:
```
192.x.x.220    VERCOM-erp.lan
```

### Acceso HTTPS

```
https://VERCOM-erp.lan
```

Aceptar el certificado autofirmado (Avanzado → Continuar).

---

## 8. Operaciones diarias

### Ver estado

```bash
docker compose ps
docker stats --no-stream
```

### Ver logs

```bash
docker logs VERCOM-app --tail 50
docker logs VERCOM-app -f
docker logs VERCOM-nginx --tail 50
docker logs VERCOM-db --tail 50
docker compose logs -f
```

### Reiniciar

```bash
docker compose restart VERCOM-app
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml restart
```

### Parar / arrancar

```bash
docker compose stop
docker compose down
docker compose up -d
```

### Shell en contenedor

```bash
docker exec -it VERCOM-app sh
docker exec -it VERCOM-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C
```

---

## 9. Backup y restore

### Backup manual

```bash
source /opt/VERCOM/.env
TIMESTAMP=$(date +%Y%m%d-%H%M%S)

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "BACKUP DATABASE [VERCOMERP] TO DISK = '/var/opt/mssql/backup/VERCOM-$TIMESTAMP.bak' WITH FORMAT, INIT"

docker cp VERCOM-db:/var/opt/mssql/backup/VERCOM-$TIMESTAMP.bak /opt/VERCOM/backups/
```

### Backup automático (cron)

```bash
cat > /opt/VERCOM/scripts/backup.sh << 'EOF'
#!/usr/bin/env bash
set -e
cd /opt/VERCOM
source .env

TIMESTAMP=$(date +%Y%m%d-%H%M%S)
BK_DIR="/opt/VERCOM/backups"
mkdir -p "$BK_DIR"

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "BACKUP DATABASE [VERCOMERP] TO DISK = '/var/opt/mssql/backup/VERCOM-$TIMESTAMP.bak' WITH FORMAT, INIT"

docker cp "VERCOM-db:/var/opt/mssql/backup/VERCOM-$TIMESTAMP.bak" "$BK_DIR/"

find "$BK_DIR" -name "VERCOM-*.bak" -mtime +30 -delete
echo "$(date +%Y-%m-%d\ %H:%M:%S) - Backup: VERCOM-$TIMESTAMP.bak" >> "$BK_DIR/backup.log"
EOF

chmod +x /opt/VERCOM/scripts/backup.sh
echo "0 3 * * * /opt/VERCOM/scripts/backup.sh" | crontab -
```

### Restore

```bash
source /opt/VERCOM/.env

docker cp /opt/VERCOM/backups/VERCOM-20251015-030000.bak VERCOM-db:/var/opt/mssql/backup/

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "ALTER DATABASE [VERCOMERP] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "RESTORE DATABASE [VERCOMERP] FROM DISK = '/var/opt/mssql/backup/VERCOM-20251015-030000.bak' WITH REPLACE, RECOVERY;"

docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "ALTER DATABASE [VERCOMERP] SET MULTI_USER;"

docker compose restart VERCOM-app
```

---

## 10. Troubleshooting

### `Login failed for user 'sa'`

Reset completo:

```bash
docker compose down
docker volume rm VERCOM-mssql-data
docker volume create VERCOM-mssql-data
./scripts/deploy.sh --init-db --force-db
```

### `Access to the path '/run/secrets/jwt_key' is denied`

```bash
sudo chown 1654:1654 /opt/VERCOM/secrets/jwt_key.txt
chmod 400 /opt/VERCOM/secrets/jwt_key.txt
docker compose restart VERCOM-app
```

### `external volume "VERCOM-mssql-data" not found`

```bash
for vol in VERCOM-mssql-data VERCOM_uploads-data VERCOM_grafana-data \
           VERCOM_loki-data VERCOM_prometheus-data VERCOM_nginx-logs; do
    docker volume create "$vol"
done
```

### Nginx en estado `Created` pero no arranca

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d VERCOM-nginx
```

### `Cannot open database "VERCOMERP"`

```bash
cd /opt/VERCOM
./scripts/init-db.sh
```

### `Container VERCOM-app is unhealthy`

```bash
docker logs VERCOM-app --tail 50
docker logs VERCOM-app 2>&1 | grep -i "error\|exception"
```

### SSL certificate problem

1. En navegador: Avanzado → Continuar
2. O `thisisunsafe` en Chrome/Edge
3. O instalar cert en Windows (admin):
   ```powershell
   Import-Certificate -FilePath "nginx\certs\VERCOM.crt" -CertStoreLocation Cert:\LocalMachine\Root
   ```

---

## 11. Mantenimiento

### Diario

- Verificar stack: `docker compose ps`
- Revisar logs: `docker logs VERCOM-app 2>&1 | grep -i error`
- Confirmar backups automáticos

### Semanal

- Uso de disco: `df -h && docker system df`
- Logs de auditoría en Grafana
- Alertas en Prometheus
- `sudo apt update && sudo apt upgrade`

### Mensual

- Prueba de restore de backup
- Revisar certificados SSL
- Limpiar logs antiguos
- Actualizar imágenes Docker

### Limpieza

```bash
docker image prune -f
docker container prune -f
docker network prune -f
docker system prune -af  # ⚠️ CUIDADO
```

### Integridad BD

```bash
docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "DBCC CHECKDB('VERCOMERP') WITH NO_INFOMSGS"
```

---

## 12. Seguridad

### Capas

1. OPNsense — Firewall perimetral
2. UFW — Firewall local (22, 80, 443)
3. Fail2ban — Bloquea IPs maliciosas
4. SSH con llaves — Sin passwords
5. Docker secrets — Secretos fuera del repo
6. Nginx — Headers HSTS, CSP, X-Frame-Options
7. App no-root — UID 1654

### Headers aplicados

```nginx
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
Referrer-Policy: strict-origin-when-cross-origin
```

### Qué NUNCA hacer

- Subir `.env` a git
- Subir `secrets/` a git
- Subir `certs/` a git
- Exponer 1433 (SQL) al host
- Correr contenedores como root

### Auditoría

```bash
sudo ss -tlnp
sudo grep "Failed password" /var/log/auth.log | tail -20
cat /etc/passwd
```

---

## 13. Chuleta de comandos

### Deploy

```bash
# Windows
.\scripts\deploy.ps1
.\scripts\deploy.ps1 -SkipBuild
.\scripts\deploy.ps1 -InitDb -ForceDb

# VM
./scripts/deploy.sh
./scripts/deploy.sh --init-db
./scripts/deploy.sh --skip-build
./scripts/deploy.sh --init-db --force-db
```

### Estado

```bash
docker compose ps
docker stats --no-stream
df -h
docker system df
```

### Logs

```bash
docker logs VERCOM-app --tail 50
docker logs VERCOM-app -f
docker logs VERCOM-nginx --tail 50
docker logs VERCOM-db --tail 50
```

### Control

```bash
docker compose restart VERCOM-app
docker compose stop
docker compose start
docker compose down
docker compose up -d
```

### Shell

```bash
docker exec -it VERCOM-app sh
docker exec -it VERCOM-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C
```

### Backup

```bash
/opt/VERCOM/scripts/backup.sh
ls -lh /opt/VERCOM/backups/
```

### Healthcheck

```bash
curl -k https://localhost/health
docker inspect VERCOM-app --format '{{.State.Health.Status}}'
docker inspect VERCOM-db --format '{{.State.Health.Status}}'
```

### Diagnóstico

```bash
docker ps -a --filter "status=exited"
docker events --since 1h
docker inspect VERCOM-app
docker exec VERCOM-app env
```

---

## 📋 Comandos esenciales

**Deploy desde Windows (1 comando):**
```powershell
cd C:\Users\Usuario\source\repos\VERCOM\VERCOM
.\scripts\deploy.ps1
```

**Deploy desde VM (1 comando):**
```bash
cd /opt/VERCOM
./scripts/deploy.sh
```

**Ver todo corriendo:**
```bash
docker compose ps
```

**Ver logs en vivo:**
```bash
docker logs VERCOM-app -f
```

---

**Fin del documento.**