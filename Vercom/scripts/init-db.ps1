<#
.SYNOPSIS
    VercomERP - Inicializacion de base de datos
.DESCRIPTION
    Crea la BD si no existe y aplica 01-schema.sql (estructura consolidada).
    Con -Force: dropea la BD y la recrea desde cero.
.PARAMETER WithDemo
    Carga datos demo adicionales (solo dev)
.PARAMETER Force
    Dropea la BD existente y la recrea desde cero (⚠️ DESTRUCTIVO)
#>
[CmdletBinding()]
param(
    [switch]$WithDemo,
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ProjectDir = (Resolve-Path "$PSScriptRoot\..").Path
Set-Location $ProjectDir

function Write-Log  { param($m) Write-Host "[init-db] $m" -ForegroundColor Blue }
function Write-Ok   { param($m) Write-Host "   OK $m" -ForegroundColor Green }
function Write-Info { param($m) Write-Host "   $m" -ForegroundColor Gray }
function Write-Warn { param($m) Write-Host "   !! $m" -ForegroundColor Yellow }
function Write-Err  { param($m) Write-Host "   XX $m" -ForegroundColor Red; exit 1 }

# Cargar .env
Get-Content "$ProjectDir\.env" | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith('#') -and $line.Contains('=')) {
        $parts = $line -split '=', 2
        [System.Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim(), 'Process')
    }
}

if (-not $env:MSSQL_SA_PASSWORD) { Write-Err "MSSQL_SA_PASSWORD no definida" }

$DB_NAME = "VercomERP"
$SQLCMD  = "/opt/mssql-tools18/bin/sqlcmd"

# Helper para ejecutar sqlcmd en master
function Invoke-Sql {
    param([string]$Query, [string]$Database = "master")
    $out = docker exec vercom-db $SQLCMD `
        -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -h -1 `
        -d $Database -Q $Query 2>&1
    return @{
        Output = $out
        Code   = $LASTEXITCODE
    }
}

# ============================================================
# PASO 0: -Force → dropear BD
# ============================================================
if ($Force) {
    Write-Log "Modo -Force: eliminando BD $DB_NAME si existe..."

    $r = Invoke-Sql "IF DB_ID('$DB_NAME') IS NOT NULL ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
    if ($r.Code -ne 0) {
        Write-Info "No se pudo poner SINGLE_USER (puede que la BD no exista)"
    }

    $r = Invoke-Sql "IF DB_ID('$DB_NAME') IS NOT NULL DROP DATABASE [$DB_NAME];"
    if ($r.Code -ne 0) {
        Write-Host $r.Output -ForegroundColor Red
        Write-Err "Error dropeando la BD"
    }

    Write-Ok "BD $DB_NAME eliminada"
}

# ============================================================
# PASO 1: Crear la BD si no existe
# ============================================================
Write-Log "Verificando base de datos $DB_NAME..."

$r = Invoke-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='$DB_NAME'"
$dbExists = (($r.Output -join '') -replace '\s','')

if ($dbExists -eq "1") {
    Write-Ok "Base de datos $DB_NAME ya existe"
} else {
    Write-Info "Creando base de datos $DB_NAME..."
    $r = Invoke-Sql "CREATE DATABASE [$DB_NAME]"
    if ($r.Code -ne 0) {
        Write-Host $r.Output -ForegroundColor Red
        Write-Err "Error creando base de datos"
    }
    Write-Ok "Base de datos $DB_NAME creada"
}

# ============================================================
# PASO 2: ¿Schema ya aplicado?
# ============================================================
Write-Log "Verificando si el schema ya está aplicado..."

$r = Invoke-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.schemas WHERE name='comercial'" $DB_NAME
$schemaApplied = (($r.Output -join '') -replace '\s','')

if ($schemaApplied -eq "1" -and -not $Force) {
    Write-Ok "Schema ya aplicado (skip). Usa -Force para recrear."
    exit 0
}

# ============================================================
# PASO 3: Aplicar 01-schema.sql
# ============================================================
$SchemaFile = "$ProjectDir\Database\scripts\01-schema.sql"

if (-not (Test-Path $SchemaFile)) {
    Write-Err "No existe $SchemaFile"
}

$sizeKB = [math]::Round((Get-Item $SchemaFile).Length / 1KB, 1)
Write-Log "Aplicando 01-schema.sql (${sizeKB} KB)..."

# Copiar
docker cp $SchemaFile "vercom-db:/tmp/_schema.sql" | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Err "Error copiando schema" }

# Ejecutar
$output = docker exec vercom-db $SQLCMD `
    -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -b -d $DB_NAME `
    -i /tmp/_schema.sql 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nErrores:" -ForegroundColor Red
    $output | Select-Object -Last 15 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    Write-Err "Error aplicando schema"
}

# ⚠️ NO borrar /tmp/_schema.sql — causa "Operation not permitted" (cosmético)
# El archivo se limpia al reiniciar el contenedor.

Write-Ok "Schema aplicado"

# ============================================================
# PASO 4: DEMO (opcional)
# ============================================================
if ($WithDemo) {
    $DemoFile = "$ProjectDir\Database\seed\demo-data.sql"

    if (Test-Path $DemoFile) {
        Write-Log "Aplicando demo-data.sql..."
        docker cp $DemoFile "vercom-db:/tmp/_demo.sql" | Out-Null

        $output = docker exec vercom-db $SQLCMD `
            -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -b -d $DB_NAME `
            -i /tmp/_demo.sql 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Warn "Errores en demo:"
            $output | Select-Object -Last 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        } else {
            Write-Ok "Demo aplicado"
        }
    }
}

# ============================================================
# PASO 5: Verificacion
# ============================================================
Write-Log "Verificando tablas..."
$r = Invoke-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" $DB_NAME
$count = (($r.Output -join '') -replace '\s','')

Write-Ok "Tablas creadas: $count"

if ([int]$count -lt 30) {
    Write-Warn "Menos de 30 tablas. Verifica el schema."
}

Write-Host ""
Write-Ok "Base de datos lista (estructura)"
Write-Host ""
Write-Host "   Los datos iniciales (permisos, roles, usuarios, etc.)" -ForegroundColor Cyan
Write-Host "   se crean automaticamente al arrancar la app (SeedData.cs)." -ForegroundColor Cyan
Write-Host ""