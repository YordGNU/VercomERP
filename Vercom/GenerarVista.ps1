# Script optimizado para generar vistas rápidamente
# Usa --no-build para evitar compilación repetida
# Omite modelos que ya tienen vistas generadas

$modelsDir = ".\Models"
$dbContext = "AppDbContext"

$modelFiles = Get-ChildItem -Path $modelsDir -Filter *.cs | Where-Object { 
    $_.Name -ne "AppDbContext.cs" -and 
    $_.Name -ne "ErrorViewModel.cs" 
}

$total = 0
$omitidos = 0
$errores = 0

foreach ($file in $modelFiles) {
    $modelName = $file.BaseName
    $modelViewsDir = ".\Views\$modelName"

    if (-not (Test-Path $modelViewsDir)) {
        New-Item -ItemType Directory -Path $modelViewsDir -Force | Out-Null
    }

    # Verificar si ya existe la vista Index
    if (Test-Path "$modelViewsDir\Index.cshtml") {
        Write-Host "  [OMITIDO] Ya existen vistas para $modelName" -ForegroundColor DarkYellow
        $omitidos++
        continue
    }

    Write-Host "Generando vistas para: $modelName" -ForegroundColor Cyan

    try {
        dotnet aspnet-codegenerator view Index List -m $modelName -dc $dbContext -outDir $modelViewsDir --useDefaultLayout --referenceScriptLibraries --no-build

        dotnet aspnet-codegenerator view Create Create -m $modelName -dc $dbContext -outDir $modelViewsDir --useDefaultLayout --referenceScriptLibraries --no-build

        dotnet aspnet-codegenerator view Edit Edit -m $modelName -dc $dbContext -outDir $modelViewsDir --useDefaultLayout --referenceScriptLibraries --no-build

       #otnet aspnet-codegenerator view Delete Delete -m $modelName -dc $dbContext -outDir $modelViewsDir --useDefaultLayout --referenceScriptLibraries --no-build

        Write-Host "  [OK] Vistas generadas para $modelName" -ForegroundColor Green
        $total++
    } catch {
        Write-Host "  [ERROR] Fallo en $modelName" -ForegroundColor Red
        $errores++
    }
}

Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  RESUMEN" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Generados: $total" -ForegroundColor Green
Write-Host "  Omitidos: $omitidos" -ForegroundColor DarkYellow
Write-Host "  Errores: $errores" -ForegroundColor Red
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Generación completada." -ForegroundColor Cyan