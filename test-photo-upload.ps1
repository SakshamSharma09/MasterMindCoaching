# Test script for photo upload API
$apiBaseUrl = "https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net"

# Step 1: Login to get JWT token
$loginBody = @{
    identifier = "admin@mastermind.com"
    otp = "123456" # This needs to be a valid OTP
    type = "Email"
} | ConvertTo-Json

Write-Host "Attempting login..."
try {
    $loginResponse = Invoke-WebRequest -Uri "$apiBaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
    $token = ($loginResponse.Content | ConvertFrom-Json).data.token
    Write-Host "Login successful, token obtained"
} catch {
    Write-Host "Login failed. You need to generate a valid OTP first."
    Write-Host "Error: $($_.Exception.Message)"
    exit 1
}

# Step 2: Create a test image file
$testImagePath = "C:\temp\test-upload.jpg"
$testImageContent = [byte[]](0xFF,0xD8,0xFF,0xE0,0x00,0x10,0x4A,0x46,0x49,0x46,0x00,0x01,0x01,0x00,0x00,0x01,0x00,0x01,0x00,0x00)
[System.IO.File]::WriteAllBytes($testImagePath, $testImageContent)
Write-Host "Test image created at $testImagePath"

# Step 3: Upload photo
Write-Host "Uploading photo..."
$fileBytes = [System.IO.File]::ReadAllBytes($testImagePath)
$fileStream = [System.IO.MemoryStream]::new($fileBytes)
$fileContent = [System.Net.Http.StreamContent]::new($fileStream)
$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$multipartContent.Add($fileContent, "file", "test-photo.jpg")

$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $uploadResponse = Invoke-WebRequest -Uri "$apiBaseUrl/api/photo/upload" -Method POST -Body $multipartContent -Headers $headers -UseBasicParsing
    Write-Host "Upload successful!"
    Write-Host $uploadResponse.Content
} catch {
    Write-Host "Upload failed."
    Write-Host "Error: $($_.Exception.Message)"
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Response: $($_.Exception.Response.GetResponseStream())"
}

# Cleanup
Remove-Item $testImagePath -ErrorAction SilentlyContinue
