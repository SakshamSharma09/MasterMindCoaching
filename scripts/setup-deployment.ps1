# Azure Deployment Setup Script
# Run this script to configure everything needed for deployment

Write-Host "=== MasterMind Azure Deployment Setup ===" -ForegroundColor Green

# Variables
$resourceGroupName = "MasterMind-RG"
$appServiceName = "mastermind-api-2404-eadxgpe5f7dch9f6"
$staticWebAppName = "victorious-glacier-0e6507000.6"

# Step 1: Get SQL Database Connection String
Write-Host "`n1. Getting SQL Database Connection String..." -ForegroundColor Yellow
$sqlServers = az sql server list --resource-group $resourceGroupName --query "[0].name" -o tsv
if ($sqlServers) {
    $dbName = az sql db list --resource-group $resourceGroupName --server $sqlServers --query "[0].name" -o tsv
    $connString = az sql db show-connection-string --name $dbName --server $sqlServers --client ado.net -o tsv
    Write-Host "Connection String: $connString" -ForegroundColor Cyan
    Write-Host "You'll need to add this to your App Service settings!" -ForegroundColor White
}

# Step 2: Create Azure Service Principal for GitHub
Write-Host "`n2. Creating Azure Service Principal for GitHub Actions..." -ForegroundColor Yellow
$subscriptionId = az account show --query id -o tsv
$sp = az ad sp create-for-rbac --name "MasterMind-GitHub" --role Contributor --scopes "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName" --json-auth
Write-Host "Service Principal created!" -ForegroundColor Green
Write-Host "Add this to GitHub Secrets as AZURE_CREDENTIALS:" -ForegroundColor White
Write-Host $sp -ForegroundColor Cyan

# Step 3: Get required values
Write-Host "`n3. Required Configuration Values:" -ForegroundColor Yellow
Write-Host "Backend URL: https://$appServiceName.centralindia-01.azurewebsites.net" -ForegroundColor Cyan
Write-Host "Frontend URL: https://$staticWebAppName.azurestaticapps.net" -ForegroundColor White

Write-Host "`n=== Next Steps ===" -ForegroundColor Green
Write-Host "1. Add the JSON above to GitHub Secrets as 'AZURE_CREDENTIALS'" -ForegroundColor White
Write-Host "2. Add API_BASE_URL secret to GitHub: https://$appServiceName.centralindia-01.azurewebsites.net" -ForegroundColor White
Write-Host "3. In Azure Portal, go to your App Service → Configuration → Application settings and add:" -ForegroundColor White
Write-Host "   - ConnectionStrings__DefaultConnection: [Use the connection string from step 1]" -ForegroundColor Gray
Write-Host "   - JWT__SecretKey: YourSecretKey12345678901234567890" -ForegroundColor Gray
Write-Host "   - JWT__Issuer: MasterMind" -ForegroundColor Gray
Write-Host "   - JWT__Audience: MasterMind" -ForegroundColor Gray
Write-Host "   - ASPNETCORE_ENVIRONMENT: Production" -ForegroundColor Gray
Write-Host "4. Push your code to GitHub to trigger deployment" -ForegroundColor White
