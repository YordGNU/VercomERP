# Script para generar controladores para todos los modelos en la carpeta Models

$modelsDir = ".\Models"
$controllersDir = ".\Controllers"
$dbContext = "AppDbContext"

# Obtener todos los archivos .cs en Models, excluyendo archivos no deseados
$modelFiles = Get-ChildItem -Path $modelsDir -Filter *.cs | Where-Object { 
    $_.Name -ne "AppDbContext.cs" -and 
    $_.Name -ne "ErrorViewModel.cs" 
}

foreach ($file in $modelFiles) {
    $modelName = $file.BaseName
    
    # Algunos archivos tienen nombres extraños por el scaffolding anterior, 
    # asegurarse de que el nombre del modelo sea correcto (ej: "CuentaBancarium" -> "CuentaBancarium")
    
    Write-Host "Generando controlador para: $modelName"
    
    dotnet aspnet-codegenerator controller `
        -name "${modelName}Controller" `
        -m $modelName `
        -dc $dbContext `
        --relativeFolderPath Controllers `
        --useDefaultLayout `
        --referenceScriptLibraries
}
Write-Host "Generación completada."
