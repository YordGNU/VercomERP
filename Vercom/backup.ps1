# backup.ps1 — Respaldo de VercomERP
$fecha = Get-Date -Format "yyyyMMdd-HHmmss"
$nombreBackup = "VercomERP-$fecha.bak"
$carpetaLocal = ".\backups"

# Crear carpeta local si no existe
New-Item -ItemType Directory -Force -Path $carpetaLocal | Out-Null

# Backup dentro del contenedor
docker exec vercom-db /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Vercom#2026!StrongPass" -C `
  -Q "BACKUP DATABASE [VercomERP] TO DISK = '/var/opt/mssql/backup/$nombreBackup' WITH FORMAT, INIT"

# Extraer al host
docker cp "vercom-db:/var/opt/mssql/backup/$nombreBackup" "$carpetaLocal\"

Write-Host "[OK] Backup creado: $carpetaLocal\$nombreBackup"