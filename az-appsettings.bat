@echo off
set PATH=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin;%PATH%

echo === Setting App Service Configuration ===
az webapp config appsettings set --name mastermind-api-2404 --resource-group MasterMind-RG --settings ^
  ASPNETCORE_ENVIRONMENT=Production ^
  "ConnectionStrings__DefaultConnection=Server=tcp:mastermind-db-2404.database.windows.net,1433;Initial Catalog=mastermind;User ID=sqladmin;Password=MasterMind@2024Secure!;Encrypt=true;TrustServerCertificate=False;Connection Timeout=30;" ^
  "Jwt__Secret=MasterMindCoachingAzure2024ProductionSecretKeyVeryLong64Chars!" ^
  Jwt__Issuer=MasterMindCoaching ^
  Jwt__Audience=MasterMindCoaching ^
  Jwt__ExpiryMinutes=60 ^
  "Sms__ApiKey=opRX60NZDItJV3lxehfMAqjWdLTYnbBOU57zF1Ku9v8SCcQ2m4zg97pMBZoS2XT34bJAjR8YucVeWfLq" ^
  Sms__UseSandbox=false ^
  "Email__Username=themastermindcoachingclasses@gmail.com" ^
  "Email__Password=zysfypgumhnrrkou" ^
  "Cors__AllowedOrigins=https://victorious-glacier-0e6507000.6.azurestaticapps.net,http://localhost:3000,http://localhost:5173" ^
  WEBSITES_PORT=5000 ^
  --output none

echo === Settings Applied ===
echo === Verifying ===
az webapp config appsettings list --name mastermind-api-2404 --resource-group MasterMind-RG --query "[].{name:name}" --output table