@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name LocalDev --start-ip-address 49.36.242.179 --end-ip-address 49.36.242.179 --output none 2>nul
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 --output none 2>nul
az staticwebapp secrets list --name mastermind-web-2404 --resource-group MasterMind-RG --query "properties.apiKey" --output tsv