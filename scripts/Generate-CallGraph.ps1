<#
.SYNOPSIS
    Generates API static analysis with call graphs showing controller-service dependencies.
    
.DESCRIPTION
    Analyzes controllers and services to create dependency graphs:
    - docs/ai/api-static-analysis.json (call graphs + dependencies)
    
.EXAMPLE
    .\scripts\Generate-CallGraph.ps1
#>

param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

$ControllersPath = Join-Path $ProjectRoot "src\backend\MasterMind.API\Controllers"
$ServicesPath = Join-Path $ProjectRoot "src\backend\MasterMind.API\Services"
$DocsPath = Join-Path $ProjectRoot "docs\ai"
$OutputPath = Join-Path $DocsPath "api-static-analysis.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MasterMind Call Graph Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ensure docs directory exists
if (-not (Test-Path $DocsPath)) {
    New-Item -ItemType Directory -Path $DocsPath -Force | Out-Null
}

$analysis = @{
    version = "1.0.0"
    lastUpdated = (Get-Date -Format "yyyy-MM-dd")
    generatedBy = "Generate-CallGraph.ps1"
    controllers = @{}
    services = @{}
    dependencies = @{
        controllerToService = @()
        serviceToService = @()
        serviceToRepository = @()
    }
}

# Analyze Controllers
Write-Host "[*] Analyzing controllers..." -ForegroundColor White
$controllerFiles = Get-ChildItem -Path $ControllersPath -Filter "*.cs" -ErrorAction SilentlyContinue

foreach ($file in $controllerFiles) {
    $content = Get-Content $file.FullName -Raw
    $controllerName = $file.BaseName
    
    # Extract constructor dependencies
    $ctorMatch = [regex]::Match($content, "public\s+$controllerName\s*\(([^)]+)\)")
    $dependencies = @()
    
    if ($ctorMatch.Success) {
        $params = $ctorMatch.Groups[1].Value -split ","
        foreach ($param in $params) {
            $param = $param.Trim()
            if ($param -match "(\w+)\s+_?(\w+)") {
                $typeName = $Matches[1]
                $dependencies += $typeName
                
                # Track controller-to-service dependencies
                if ($typeName -match "^I\w+Service$") {
                    $analysis.dependencies.controllerToService += @{
                        from = $controllerName
                        to = $typeName
                        file = "src/backend/MasterMind.API/Controllers/$($file.Name)"
                    }
                }
            }
        }
    }
    
    # Count endpoints
    $getCount = ([regex]::Matches($content, '\[HttpGet')).Count
    $postCount = ([regex]::Matches($content, '\[HttpPost')).Count
    $putCount = ([regex]::Matches($content, '\[HttpPut')).Count
    $deleteCount = ([regex]::Matches($content, '\[HttpDelete')).Count
    
    $analysis.controllers[$controllerName] = @{
        file = "src/backend/MasterMind.API/Controllers/$($file.Name)"
        dependencies = $dependencies
        endpoints = @{
            GET = $getCount
            POST = $postCount
            PUT = $putCount
            DELETE = $deleteCount
            total = $getCount + $postCount + $putCount + $deleteCount
        }
        hasAuthorize = $content -match '\[Authorize\]'
        usesDbContext = $content -match '_context\.'
    }
}

# Analyze Services
Write-Host "[*] Analyzing services..." -ForegroundColor White
$serviceImplPath = Join-Path $ServicesPath "Implementations"

if (Test-Path $serviceImplPath) {
    $serviceFiles = Get-ChildItem -Path $serviceImplPath -Filter "*.cs" -ErrorAction SilentlyContinue
    
    foreach ($file in $serviceFiles) {
        $content = Get-Content $file.FullName -Raw
        $serviceName = $file.BaseName
        
        # Extract constructor dependencies
        $ctorMatch = [regex]::Match($content, "public\s+$serviceName\s*\(([^)]+)\)")
        $dependencies = @()
        
        if ($ctorMatch.Success) {
            $params = $ctorMatch.Groups[1].Value -split ","
            foreach ($param in $params) {
                $param = $param.Trim()
                if ($param -match "(\w+)\s+_?(\w+)") {
                    $typeName = $Matches[1]
                    $dependencies += $typeName
                    
                    # Track service-to-service dependencies
                    if ($typeName -match "^I\w+Service$" -and $typeName -ne "I$serviceName") {
                        $analysis.dependencies.serviceToService += @{
                            from = $serviceName
                            to = $typeName
                        }
                    }
                }
            }
        }
        
        # Count public methods
        $methodCount = ([regex]::Matches($content, 'public\s+(?:async\s+)?(?:Task<)?[\w<>]+(?:>)?\s+\w+\s*\(')).Count
        
        $analysis.services[$serviceName] = @{
            file = "src/backend/MasterMind.API/Services/Implementations/$($file.Name)"
            interface = "I$serviceName"
            dependencies = $dependencies
            publicMethods = $methodCount
            usesDbContext = $content -match '_context\.'
            usesHttpClient = $content -match 'HttpClient|IHttpClientFactory'
            usesCache = $content -match 'IMemoryCache|IDistributedCache'
        }
    }
}

# Generate summary statistics
$analysis.summary = @{
    totalControllers = $analysis.controllers.Count
    totalServices = $analysis.services.Count
    totalEndpoints = ($analysis.controllers.Values | ForEach-Object { $_.endpoints.total } | Measure-Object -Sum).Sum
    controllerServiceDependencies = $analysis.dependencies.controllerToService.Count
    serviceServiceDependencies = $analysis.dependencies.serviceToService.Count
}

# Write output
$analysis | ConvertTo-Json -Depth 10 | Set-Content $OutputPath -Encoding UTF8
Write-Host "[+] Generated $OutputPath" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Analysis Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Controllers: $($analysis.summary.totalControllers)" -ForegroundColor White
Write-Host "  Services: $($analysis.summary.totalServices)" -ForegroundColor White
Write-Host "  Total Endpoints: $($analysis.summary.totalEndpoints)" -ForegroundColor White
Write-Host "  Dependencies tracked: $($analysis.dependencies.controllerToService.Count + $analysis.dependencies.serviceToService.Count)" -ForegroundColor White
Write-Host ""
