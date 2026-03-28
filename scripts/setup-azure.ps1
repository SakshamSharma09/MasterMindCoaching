# Azure Deployment Setup Script
# Run this in Azure PowerShell or Azure CLI

# Variables - Update these values
$resourceGroupName = "MasterMind-RG"
$location = "East US"
$appServiceName = "mastermind-api-$(Get-Random)"
$staticWebAppName = "mastermind-web-$(Get-Random)"
$sqlServerName = "mastermind-sql-$(Get-Random)"
$sqlDatabaseName = "mastermind"

Write-Host "Setting up Azure resources for MasterMind Coaching..." -ForegroundColor Green

# Create Resource Group
Write-Host "Creating resource group..." -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

# Create App Service Plan (Free Tier)
Write-Host "Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create --name "$appServiceName-plan" --resource-group $resourceGroupName --sku FREE --is-linux

# Create Backend App Service
Write-Host "Creating Backend App Service..." -ForegroundColor Yellow
az webapp create --resource-group $resourceGroupName --plan "$appServiceName-plan" --name $appServiceName --runtime "DOTNETCORE|8.0"

# Create Static Web App for Frontend
Write-Host "Creating Static Web App for Frontend..." -ForegroundColor Yellow
az staticwebapp create --name $staticWebAppName --resource-group $resourceGroupName --location $location

# Create SQL Database (Basic tier - ~$5/month)
Write-Host "Creating SQL Database..." -ForegroundColor Yellow
az sql server create --name $sqlServerName --resource-group $resourceGroupName --location $location --admin-user "sqladmin" --admin-password "YourSecurePassword123!"
az sql db create --name $sqlDatabaseName --server $sqlServerName --resource-group $resourceGroupName --service-objective Basic

# Configure SQL Firewall
Write-Host "Configuring SQL Firewall..." -ForegroundColor Yellow
az sql server firewall-rule create --resource-group $resourceGroupName --server $sqlServerName --name "AllowAzure" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Get connection string
$sqlConnectionString = az sql db show-connection-string --name $sqlDatabaseName --server $sqlServerName --client ado.net --output tsv
Write-Host "SQL Connection String: $sqlConnectionString" -ForegroundColor Cyan

# Configure Backend App Settings
Write-Host "Configuring Backend App Settings..." -ForegroundColor Yellow
az webapp config appsettings set --resource-group $resourceGroupName --name $appServiceName --settings `
  "ConnectionStrings__DefaultConnection=$sqlConnectionString" `
  "JWT__SecretKey=Your-256-bit-secret-key-here-change-this" `
  "JWT__Issuer=MasterMind" `
  "JWT__Audience=MasterMind" `
  "JWT__ExpirationMinutes=60" `
  "ASPNETCORE_ENVIRONMENT=Production"

# Enable HTTPS Only
Write-Host "Enabling HTTPS Only..." -ForegroundColor Yellow
az webapp update --resource-group $resourceGroupName --name $appServiceName --https-only true

# Output deployment information
Write-Host "`n=== Deployment Information ===" -ForegroundColor Green
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor Cyan
Write-Host "Backend URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "Frontend URL: https://$staticWebAppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "SQL Server: $sqlServerName.database.windows.net" -ForegroundColor Cyan
Write-Host "SQL Database: $sqlDatabaseName" -ForegroundColor Cyan

Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Update your frontend .env with: VITE_API_BASE_URL=https://$appServiceName.azurewebsites.net" -ForegroundColor White
Write-Host "2. Set up GitHub Actions for deployment" -ForegroundColor White
Write-Host "3. Run database migrations" -ForegroundColor White
Write-Host "4. Test your application" -ForegroundColor White
