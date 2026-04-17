<#
.SYNOPSIS
    Auto-generates API documentation from ASP.NET Core controllers.
    
.DESCRIPTION
    Scans all controllers in the backend project and generates:
    - docs/ai/api-endpoint-index.json (route-to-file mapping)
    - docs/ai/full-api-reference.md (complete API documentation)
    
    This script should be run after adding new endpoints or modifying existing ones.
    Can be integrated with pre-commit hooks for automatic updates.

.EXAMPLE
    .\scripts\Generate-ApiDocs.ps1
    
.NOTES
    Author: MasterMind Coaching AI System
    Version: 1.0.0
    Last Updated: 2026-04-17
#>

param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Paths
$ControllersPath = Join-Path $ProjectRoot "src\backend\MasterMind.API\Controllers"
$DocsPath = Join-Path $ProjectRoot "docs\ai"
$EndpointIndexPath = Join-Path $DocsPath "api-endpoint-index.json"
$ApiReferencePath = Join-Path $DocsPath "full-api-reference.md"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MasterMind API Documentation Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ensure docs directory exists
if (-not (Test-Path $DocsPath)) {
    New-Item -ItemType Directory -Path $DocsPath -Force | Out-Null
    Write-Host "[+] Created docs/ai directory" -ForegroundColor Green
}

# Get all controller files
$controllerFiles = Get-ChildItem -Path $ControllersPath -Filter "*.cs" -ErrorAction SilentlyContinue

if (-not $controllerFiles) {
    Write-Host "[!] No controller files found in $ControllersPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "[*] Found $($controllerFiles.Count) controller files" -ForegroundColor White

# Initialize endpoint index structure
$endpointIndex = @{
    version = "1.0.0"
    lastUpdated = (Get-Date -Format "yyyy-MM-dd")
    generatedBy = "Generate-ApiDocs.ps1"
    description = "Auto-generated route-to-file mapping for AI assistants"
    endpoints = @{}
    frontendViews = @{}
    stores = @{}
    services = @{}
}

# Parse each controller
$allEndpoints = @()

foreach ($file in $controllerFiles) {
    $content = Get-Content $file.FullName -Raw
    $controllerName = $file.BaseName -replace "Controller$", ""
    $relativePath = "src/backend/MasterMind.API/Controllers/$($file.Name)"
    
    if ($Verbose) {
        Write-Host "  [*] Parsing $($file.Name)..." -ForegroundColor Gray
    }
    
    # Extract route prefix
    $routeMatch = [regex]::Match($content, '\[Route\("([^"]+)"\)\]')
    $routePrefix = if ($routeMatch.Success) { $routeMatch.Groups[1].Value } else { "api/[controller]" }
    $routePrefix = $routePrefix -replace "\[controller\]", $controllerName.ToLower()
    
    # Check if controller has [Authorize]
    $hasAuthorize = $content -match '\[Authorize\]'
    
    # Extract HTTP methods and routes
    $routes = @()
    
    # Match HTTP method attributes with optional route
    $httpPatterns = @(
        @{ Method = "GET"; Pattern = '\[HttpGet(?:\("([^"]*)"\))?\]' },
        @{ Method = "POST"; Pattern = '\[HttpPost(?:\("([^"]*)"\))?\]' },
        @{ Method = "PUT"; Pattern = '\[HttpPut(?:\("([^"]*)"\))?\]' },
        @{ Method = "DELETE"; Pattern = '\[HttpDelete(?:\("([^"]*)"\))?\]' },
        @{ Method = "PATCH"; Pattern = '\[HttpPatch(?:\("([^"]*)"\))?\]' }
    )
    
    foreach ($httpPattern in $httpPatterns) {
        $matches = [regex]::Matches($content, $httpPattern.Pattern)
        foreach ($match in $matches) {
            $subRoute = $match.Groups[1].Value
            $fullRoute = if ($subRoute) { "/$routePrefix/$subRoute" } else { "/$routePrefix" }
            $fullRoute = $fullRoute -replace "//", "/"
            
            # Try to find the method name (next public method after the attribute)
            $position = $match.Index
            $afterMatch = $content.Substring($position)
            $methodMatch = [regex]::Match($afterMatch, 'public\s+(?:async\s+)?(?:Task<)?(?:ActionResult<)?[\w<>]+(?:>)?\s+(\w+)\s*\(')
            $actionName = if ($methodMatch.Success) { $methodMatch.Groups[1].Value } else { "Unknown" }
            
            $routes += @{
                method = $httpPattern.Method
                path = $fullRoute
                action = $actionName
            }
            
            $allEndpoints += @{
                controller = $controllerName
                method = $httpPattern.Method
                path = $fullRoute
                action = $actionName
                requiresAuth = $hasAuthorize
                file = $relativePath
            }
        }
    }
    
    # Add to endpoint index
    $endpointIndex.endpoints[$controllerName.ToLower()] = @{
        controller = $relativePath
        requiresAuth = $hasAuthorize
        routes = $routes
    }
}

# Add frontend mappings (static for now, could be auto-generated too)
$endpointIndex.frontendViews = @{
    auth = @{
        login = "src/frontend/mastermind-web/src/views/auth/LoginView.vue"
    }
    admin = @{
        dashboard = "src/frontend/mastermind-web/src/views/admin/DashboardView.vue"
        students = "src/frontend/mastermind-web/src/views/admin/StudentsView.vue"
        teachers = "src/frontend/mastermind-web/src/views/admin/TeachersView.vue"
        classes = "src/frontend/mastermind-web/src/views/admin/ClassesView.vue"
        attendance = "src/frontend/mastermind-web/src/views/admin/AttendanceView.vue"
        finance = "src/frontend/mastermind-web/src/views/admin/FinanceView.vue"
        sessions = "src/frontend/mastermind-web/src/views/admin/SessionsView.vue"
    }
}

$endpointIndex.stores = @{
    auth = "src/frontend/mastermind-web/src/stores/auth.ts"
    students = "src/frontend/mastermind-web/src/stores/students.ts"
    classes = "src/frontend/mastermind-web/src/stores/classes.ts"
}

$endpointIndex.services = @{
    api = "src/frontend/mastermind-web/src/services/apiService.ts"
    auth = "src/frontend/mastermind-web/src/services/authService.ts"
}

# Write endpoint index JSON
$endpointIndex | ConvertTo-Json -Depth 10 | Set-Content $EndpointIndexPath -Encoding UTF8
Write-Host "[+] Generated $EndpointIndexPath" -ForegroundColor Green

# Generate API Reference Markdown
$apiRefContent = @"
# MasterMind Coaching - Full API Reference

> **Auto-generated documentation for AI assistants**  
> Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
> Generator: Generate-ApiDocs.ps1

## Overview

| Metric | Value |
|--------|-------|
| Total Controllers | $($controllerFiles.Count) |
| Total Endpoints | $($allEndpoints.Count) |
| Protected Endpoints | $($allEndpoints | Where-Object { $_.requiresAuth } | Measure-Object | Select-Object -ExpandProperty Count) |
| Public Endpoints | $($allEndpoints | Where-Object { -not $_.requiresAuth } | Measure-Object | Select-Object -ExpandProperty Count) |

## Base URLs

| Environment | Backend API | Frontend |
|-------------|-------------|----------|
| Production | ``https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net`` | ``https://victorious-glacier-0e6507000.6.azurestaticapps.net`` |
| Development | ``http://localhost:5000`` | ``http://localhost:3000`` |

---

## Endpoints by Controller

"@

# Group endpoints by controller
$groupedEndpoints = $allEndpoints | Group-Object -Property controller

foreach ($group in $groupedEndpoints) {
    $controllerName = $group.Name
    $endpoints = $group.Group
    $firstEndpoint = $endpoints[0]
    
    $apiRefContent += @"

### $controllerName

**File:** ``$($firstEndpoint.file)``  
**Auth Required:** $(if ($firstEndpoint.requiresAuth) { "Yes" } else { "No" })

| Method | Path | Action |
|--------|------|--------|
"@
    
    foreach ($ep in $endpoints) {
        $apiRefContent += "| $($ep.method) | ``$($ep.path)`` | $($ep.action) |`n"
    }
}

$apiRefContent += @"

---

## Authentication

All protected endpoints require:
``````
Authorization: Bearer <access_token>
``````

## Error Response Format

``````json
{
  "success": false,
  "message": "Error description",
  "errorCode": "ERROR_CODE"
}
``````

## Quick Reference

### Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Internal Server Error |

---

*This file is auto-generated. Do not edit manually.*
"@

$apiRefContent | Set-Content $ApiReferencePath -Encoding UTF8
Write-Host "[+] Generated $ApiReferencePath" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Generation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Controllers scanned: $($controllerFiles.Count)" -ForegroundColor White
Write-Host "  Endpoints found: $($allEndpoints.Count)" -ForegroundColor White
Write-Host "  Protected: $($allEndpoints | Where-Object { $_.requiresAuth } | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor White
Write-Host "  Public: $($allEndpoints | Where-Object { -not $_.requiresAuth } | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor White
Write-Host ""
Write-Host "  Files generated:" -ForegroundColor White
Write-Host "    - docs/ai/api-endpoint-index.json" -ForegroundColor Gray
Write-Host "    - docs/ai/full-api-reference.md" -ForegroundColor Gray
Write-Host ""
