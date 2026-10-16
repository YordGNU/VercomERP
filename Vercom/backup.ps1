# backup.ps1 — Respaldo de VERCOMERP
$fecha = Get-Date -Format "yyyyMMdd-HHmmss"
$nombreBackup = "VERCOMERP-$fecha.bak"
$carpetaLocal = ".\backups"

# Crear carpeta local si no existe
New-Item -ItemType Directory -Force -Path $carpetaLocal | Out-Null

# Backup dentro del contenedor
docker exec VERCOM-db /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "VERCOM#2026!StrongPass" -C `
  -Q "BACKUP DATABASE [VERCOMERP] TO DISK = '/var/opt/mssql/backup/$nombreBackup' WITH FORMAT, INIT"

# Extraer al host
docker cp "VERCOM-db:/var/opt/mssql/backup/$nombreBackup" "$carpetaLocal\"

Write-Host "[OK] Backup creado: $carpetaLocal\$nombreBackup"