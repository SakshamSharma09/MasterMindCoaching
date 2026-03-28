@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%
echo === Current Firewall Rules ===
az sql server firewall-rule list --server mastermind-db-2404 --resource-group MasterMind-RG --output table
echo === Allow Azure Services ===
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 --output none 2>nul
echo Done allowing Azure services