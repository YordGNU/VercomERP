# post_process_inspinia_forms_rapido.ps1
# Aplica estructura Inspinia a vistas Create, Edit, Index y Delete
# Versión corregida: respeta bloque Razor inicial

param(
    [string]$ViewsRoot = ".\Views",
    [switch]$IncluirDetails = $false
)

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Aplicando plantilla Inspinia a Vercom" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Directorio de vistas: $ViewsRoot" -ForegroundColor Yellow
Write-Host ""

# Patrones a procesar
$patterns = @("Create", "Edit", "Index", "Delete")
if ($IncluirDetails) { $patterns += "Details" }

# Contadores
$processed = 0
$skipped = 0
$errors = 0

# Obtener archivos de una sola vez
$files = Get-ChildItem -Path $ViewsRoot -Filter *.cshtml -Recurse | 
    Where-Object { $_.BaseName -in $patterns }

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw

        if ($content -match "page-title-head" -and $content -match "card-body") {
            $skipped++
            continue
        }

        $relativePath = $file.FullName.Replace((Resolve-Path $ViewsRoot).Path, "").TrimStart("\")
        $pathParts = $relativePath.Split("\")
        $entityName = $pathParts[0]
        $actionName = $file.BaseName

        switch ($actionName) {
            "Create" { 
                $pageTitle = "Crear $entityName"
                $cardTitle = "Formulario de Creación"
                $breadcrumbAction = "Crear"
                $isForm = $true
            }
            "Edit" { 
                $pageTitle = "Editar $entityName"
                $cardTitle = "Formulario de Edición"
                $breadcrumbAction = "Editar"
                $isForm = $true
            }
            "Index" { 
                $pageTitle = "Gestión de $entityName"
                $cardTitle = "Listado de $entityName"
                $breadcrumbAction = "Listado"
                $isForm = $false
            }
            "Delete" { 
                $pageTitle = "Eliminar $entityName"
                $cardTitle = "Confirmar Eliminación"
                $breadcrumbAction = "Eliminar"
                $isForm = $false
            }
            "Details" { 
                $pageTitle = "Detalles de $entityName"
                $cardTitle = "Detalles del Registro"
                $breadcrumbAction = "Detalles"
                $isForm = $false
            }
            default { 
                $pageTitle = "Gestión de $entityName"
                $cardTitle = $entityName
                $breadcrumbAction = $actionName
                $isForm = $false
            }
        }

        $inspiniaHeader = @"
<div class="page-title-head d-flex align-items-center">
    <div class="flex-grow-1">
        <h4 class="page-main-title m-0">$pageTitle</h4>
    </div>
    <div class="text-end">
        <ol class="breadcrumb m-0 py-0">
            <li class="breadcrumb-item"><a href="javascript: void(0);">Vercom</a></li>
            <li class="breadcrumb-item"><a href="javascript: void(0);">$entityName</a></li>
            <li class="breadcrumb-item active">$breadcrumbAction</li>
        </ol>
    </div>
</div>

<div class="container-xxl">
    <div class="row">
        <div class="col-lg-12">
            <div class="card">
                <div class="card-header">
                    <h4 class="card-title">$cardTitle</h4>
                </div>
                <div class="card-body">
"@

        $inspiniaFooter = @"
                </div>
            </div>
        </div>
    </div>
</div>
"@

        # Limpiar scaffolding original
        $content = $content -replace '<div class="container">', ''
        $content = $content -replace '<main role="main" class="pb-3">', ''
        $content = $content -replace '<h1>.*?</h1>', ''

        if ($isForm) {
            # Para Create y Edit: insertar header antes del form
            if ($content -match '<form') {
                $content = $content -replace '<form', "$inspiniaHeader`n<form"
                $content = $content -replace '</form>', "</form>`n$inspiniaFooter"
            }
        } else {
            # Para Index, Delete y Details: respetar bloque Razor inicial
            $razorBlock = ""
            $htmlContent = $content

            if ($content -match '(?s)^(@model.*?@\{\s*.*?\})') {
                $razorBlock = $Matches[1]
                $htmlContent = $content.Substring($razorBlock.Length)
            }

            $content = $razorBlock + "`n`n" + $inspiniaHeader + $htmlContent + $inspiniaFooter

            # Transformar tabla a data-table
            $content = $content -replace '<table  class="table table-striped table-bordered">', '<div class="table-responsive"><table class="table data-table">'
            $content = $content -replace '</table>', '</table></div>'
        }

        if ($isForm) {
            $content = $content -replace 'class="form-group"', 'class="mb-3"'
            $content = $content -replace 'class="control-label"', 'class="form-label"'

            if ($content -match '<form' -and $content -notmatch '<form[^>]*class="[^"]*row') {
                $content = $content -replace '<form ', '<form class="row g-3" '
            }

            $content = $content -replace '<input type="submit" value="[^"]*" class="[^"]*" />', '<button type="submit" class="btn btn-primary">Guardar</button>'
        }

        # Guardar archivo
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $processed++

    } catch {
        $errors++
    }
}

Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DEL PROCESO" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Procesados: $processed" -ForegroundColor Green
Write-Host "  Omitidos: $skipped" -ForegroundColor DarkYellow
Write-Host "  Errores: $errors" -ForegroundColor Red
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "¡Proceso finalizado!" -ForegroundColor Cyan