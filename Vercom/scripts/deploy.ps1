<#
.SYNOPSIS
    VERCOMERP - Deploy desde Windows a la VM
#>
[CmdletBinding()]
param(
    [string]$Servidor = "192.168.5.220",
    [string]$Usuario  = "administrador",
    [string]$Version = "2.0.0",
    [switch]$SkipBuild,
    [switch]$InitDb,
    [switch]$ForceDb
)

$ErrorActionPreference = "Stop"
$ProjectDir = (Resolve-Path "$PSScriptRoot\..").Path
Set-Location $ProjectDir

function Write-Step { param($n,$t) Write-Host "`n[$n/5] $t" -ForegroundColor Yellow }
function Write-Ok   { param($m) Write-Host "   OK $m" -ForegroundColor Green }
function Write-Err  { param($m) Write-Host "   XX $m" -ForegroundColor Red; exit 1 }

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host " VERCOMERP - Deploy Windows -> VM" -ForegroundColor Cyan
Write-Host " Destino:  ${Usuario}@${Servidor}" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# 1. BUILD
if (-not $SkipBuild) {
    Write-Step "1" "Construyendo imagen..."
    docker build -t "VERCOM-erp:$Version" .
    if ($LASTEXITCODE -ne 0) { Write-Err "docker build fallo" }
    Write-Ok "Imagen construida"
} else {
    Write-Step "1" "SkipBuild activo"
    docker image inspect "VERCOM-erp:$Version" *> $null
    if ($LASTEXITCODE -ne 0) { Write-Err "Imagen no existe" }
    Write-Ok "Imagen existente"
}

# 2. EXPORTAR
Write-Step "2" "Exportando imagen..."
$archivo = "VERCOM-erp-$Version.tar"
if (Test-Path $archivo) { Remove-Item $archivo -Force }
docker save "VERCOM-erp:$Version" -o $archivo
if ($LASTEXITCODE -ne 0) { Write-Err "docker save fallo" }
$sizeMB = [math]::Round((Get-Item $archivo).Length / 1MB, 2)
Write-Ok "Archivo: $archivo ($sizeMB MB)"

# 3. SCP (imagen + script bash)
Write-Step "3" "Copiando imagen y script a la VM..."
scp $archivo "${Usuario}@${Servidor}:/tmp/"
if ($LASTEXITCODE -ne 0) { Write-Err "scp imagen fallo" }
scp "$PSScriptRoot\deploy-remote.sh" "${Usuario}@${Servidor}:/tmp/deploy-remote.sh"
if ($LASTEXITCODE -ne 0) { Write-Err "scp script fallo" }
Write-Ok "Transferencia completa"

# 4. DEPLOY REMOTO
Write-Step "4" "Ejecutando deploy remoto..."

# Construir string de argumentos
$remoteArgs = ""
if ($InitDb) {
    $remoteArgs = "--init-db"
    if ($ForceDb) { $remoteArgs = "--init-db --force-db" }
}

$remoteCmd = "bash /tmp/deploy-remote.sh $archivo $remoteArgs"
Write-Host "   Ejecutando: $remoteCmd" -ForegroundColor Gray

ssh "${Usuario}@${Servidor}" $remoteCmd
if ($LASTEXITCODE -ne 0) { Write-Err "Deploy remoto fallo" }
Write-Ok "Stack desplegado"

# 5. CLEANUP
Write-Step "5" "Limpiando local..."
Remove-Item $archivo -ErrorAction SilentlyContinue
Write-Ok "Archivo temporal eliminado"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Green
Write-Host " Deploy completado" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host ""
Write-Host " App:        https://VERCOM-erp.lan" -ForegroundColor Cyan
Write-Host " Prometheus: http://${Servidor}:9090" -ForegroundColor Cyan
Write-Host " Grafana:    http://${Servidor}:3000" -ForegroundColor Cyan
Write-Host ""