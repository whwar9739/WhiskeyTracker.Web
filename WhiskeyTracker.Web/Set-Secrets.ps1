$ErrorActionPreference = "Stop"

Write-Host "--- Whiskey Tracker Secret Setup ---" -ForegroundColor Cyan

# 1. Google Authentication
Write-Host "`n[Google Authentication]" -ForegroundColor Yellow
$googleId = Read-Host "Enter Google Client ID"
$googleSecret = Read-Host "Enter Google Client Secret"

if (-not [string]::IsNullOrWhiteSpace($googleId)) {
    dotnet user-secrets set "Authentication:Google:ClientId" $googleId
    dotnet user-secrets set "Authentication:Google:ClientSecret" $googleSecret
    Write-Host "-> Google secrets set." -ForegroundColor Green
}

# 2. Email Settings
Write-Host "`n[Email / SMTP Settings]" -ForegroundColor Yellow
$smtpHost = Read-Host "Enter SMTP Host (e.g. smtp.gmail.com)"
$smtpPort = Read-Host "Enter SMTP Port (default 587)"
if ([string]::IsNullOrWhiteSpace($smtpPort)) { $smtpPort = "587" }
$smtpUser = Read-Host "Enter SMTP User/Email"
$smtpPass = Read-Host "Enter SMTP Password"
$senderEmail = Read-Host "Enter Sender Email (or press enter for default)"

if (-not [string]::IsNullOrWhiteSpace($smtpHost)) {
    dotnet user-secrets set "EmailSettings:Host" $smtpHost
    dotnet user-secrets set "EmailSettings:Port" $smtpPort
    dotnet user-secrets set "EmailSettings:User" $smtpUser
    dotnet user-secrets set "EmailSettings:Password" $smtpPass
    
    if (-not [string]::IsNullOrWhiteSpace($senderEmail)) {
        dotnet user-secrets set "EmailSettings:SenderEmail" $senderEmail
    }
    Write-Host "-> Email secrets set." -ForegroundColor Green
}

Write-Host "`nDone! Secrets are stored securely in your user profile." -ForegroundColor Cyan
