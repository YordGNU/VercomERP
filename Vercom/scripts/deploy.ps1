# ============================================================
# VercomERP - Deploy automatizado
# Uso:
#   .\scripts\deploy.ps1                              # deploy local
#   .\scripts\deploy.ps1 -Servidor 192.168.1.100      # deploy remoto
# ============================================================
param(
    [string]$Servidor = "localhost",
    [string]$Usuario  = "root",
    [string]$Version  = "latest",
    [switch]$ConMonitoring
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host " Desplegando VercomERP v$Version" -ForegroundColor Cyan
Write-Host " Destino: $Servidor" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# 1. Construir imagen
Write-Host "`n[1/5] Construyendo imagen..." -ForegroundColor Yellow
docker build -t "vercom-erp:$Version" ./Vercom

# 2. Exportar
Write-Host "`n[2/5] Exportando imagen..." -ForegroundColor Yellow
$archivo = "vercom-erp-$Version.tar.gz"
docker save "vercom-erp:$Version" | gzip > $archivo
$sizeMB = [math]::Round((Get-Item $archivo).Length / 1MB, 2)
Write-Host "   Archivo: $archivo ($sizeMB MB)" -ForegroundColor Gray

# 3. Copiar al servidor (si es remoto)
if ($Servidor -ne "localhost") {
    Write-Host "`n[3/5] Copiando a ${Usuario}@${Servidor}..." -ForegroundColor Yellow
    scp $archivo "${Usuario}@${Servidor}:/tmp/"
} else {
    Write-Host "`n[3/5] Deploy local - omitiendo copia" -ForegroundColor Gray
}

# 4. Activar
Write-Host "`n[4/5] Activando..." -ForegroundColor Yellow
$monitoringFlag = if ($ConMonitoring) { "-f docker-compose.monitoring.yml" } else { "" }

if ($Servidor -ne "localhost") {
    $comando = @"
docker load < /tmp/$archivo
cd /opt/vercom
docker compose -f docker-compose.yml -f docker-compose.prod.yml $monitoringFlag up -d
docker system prune -f
"@
    ssh "${Usuario}@${Servidor}" $comando
} else {
    docker load < $archivo
    docker compose -f docker-compose.yml -f docker-compose.prod.yml $monitoringFlag up -d
}

# 5. Limpiar
Write-Host "`n[5/5] Limpiando..." -ForegroundColor Yellow
Remove-Item $archivo

Write-Host "`n✅ Despliegue completado" -ForegroundColor Green
Write-Host "   URL: https://vercom.local" -ForegroundColor Green
if ($ConMonitoring) {
    Write-Host "   Grafana: http://127.0.0.1:3000" -ForegroundColor Green
    Write-Host "   Prometheus: http://127.0.0.1:9090" -ForegroundColor Green
}