@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%

echo === Step 1: Update App Service Runtime to .NET 9 ===
az webapp config set --name mastermind-api-2404 --resource-group MasterMind-RG --linux-fx-version "DOTNETCORE|9.0"

echo === Step 2: Get SQL Connection String ===
az sql db show-connection-string --name mastermind --server mastermind-db-2404 --client ado.net --output tsv

echo === Step 3: Check SQL Firewall Rules ===
az sql server firewall-rule list --server mastermind-db-2404 --resource-group MasterMind-RG --output table

echo === Step 4: Allow Azure Services Access to SQL ===
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

echo === Step 5: Get Static Web App deployment token ===
az staticwebapp secrets list --name mastermind-web-2404 --resource-group MasterMind-RG --query "properties.apiKey" --output tsv

echo === Step 6: Check current app settings ===
az webapp config appsettings list --name mastermind-api-2404 --resource-group MasterMind-RG --output table

echo === DONE ===