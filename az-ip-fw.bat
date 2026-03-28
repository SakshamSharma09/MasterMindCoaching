@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%
echo === Adding local IP to SQL firewall ===
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name LocalDev --start-ip-address 49.36.242.179 --end-ip-address 49.36.242.179 --output none
echo Done adding local IP
echo === Adding Azure services rule ===
az sql server firewall-rule create --resource-group MasterMind-RG --server mastermind-db-2404 --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 --output none
echo Done adding Azure services
echo === Getting Static Web App token ===
az staticwebapp secrets list --name mastermind-web-2404 --resource-group MasterMind-RG --query "properties.apiKey" --output tsv
echo === Getting DB status ===
az sql db show --name mastermind --server mastermind-db-2404 --resource-group MasterMind-RG --query "{status:status}" --output json