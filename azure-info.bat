@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%

echo === SQL Connection String ===
az sql db show-connection-string --name mastermind --server mastermind-db-2404 --client ado.net --output tsv

echo === SQL Firewall Rules ===
az sql server firewall-rule list --server mastermind-db-2404 --resource-group MasterMind-RG --output table

echo === Allow Azure Services ===
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 2>nul
echo Done

echo === Static Web App Token ===
az staticwebapp secrets list --name mastermind-web-2404 --resource-group MasterMind-RG --query "properties.apiKey" --output tsv

echo === Current App Settings ===
az webapp config appsettings list --name mastermind-api-2404 --resource-group MasterMind-RG --output table

echo === Resume SQL DB ===
az sql db update --name mastermind --server mastermind-db-2404 --resource-group MasterMind-RG --service-objective GP_S_Gen5_2 2>nul

echo === DB Status ===
az sql db show --name mastermind --server mastermind-db-2404 --resource-group MasterMind-RG --query "{status:status}" --output json