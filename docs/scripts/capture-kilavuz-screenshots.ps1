# CeyPASS kilavuz ekran goruntuleri — Web (canli) + fixture (WFA/WPF/Mobile)
$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $scriptDir "..\..")
$localSettings = Join-Path $repoRoot "CeyPASS.Web\appsettings.Local.json"

if (-not (Test-Path $localSettings)) {
    Write-Error "appsettings.Local.json bulunamadi. Web icin yerel DB yapilandirmasi gerekli."
}

$cfg = Get-Content $localSettings -Raw | ConvertFrom-Json
$conn = $cfg.ConnectionStrings.DefaultConnection
if ($conn -match "Server=([^;]+).*Database=([^;]+).*User Id=([^;]+).*Password=([^;]+)") {
    $server = $Matches[1]
    $db = $Matches[2]
    $uid = $Matches[3]
    $pwd = $Matches[4]
} else {
    Write-Error "Connection string parse edilemedi."
}

$docUser = if ($env:CEYPASS_DOC_USER) { $env:CEYPASS_DOC_USER } else { "ADMIN" }
$passRow = sqlcmd -S $server -U $uid -P $pwd -d $db -Q "SET NOCOUNT ON; SELECT Sifre FROM Kullanicilar WHERE KullaniciAdi='$docUser'" -h -1 -W 2>$null
$docPass = ($passRow | Where-Object { $_ -and $_.Trim() -ne "" } | Select-Object -First 1).ToString().Trim()
if (-not $docPass) { Write-Error "Kullanici sifresi alinamadi: $docUser" }

$env:CEYPASS_DOC_USER = $docUser
$env:CEYPASS_DOC_PASS = $docPass
$env:CEYPASS_WEB_URL = if ($env:CEYPASS_WEB_URL) { $env:CEYPASS_WEB_URL } else { "http://localhost:5002" }

# Web sunucusu calisiyor mu?
try {
    $null = Invoke-WebRequest -Uri "$($env:CEYPASS_WEB_URL)/Account/Login" -UseBasicParsing -TimeoutSec 5
} catch {
    Write-Host "Web sunucusu baslatiliyor..."
    Start-Process -FilePath "dotnet" -ArgumentList "run","--project",(Join-Path $repoRoot "CeyPASS.Web\CeyPASS.Web.csproj"),"--launch-profile","http","--no-build" -WindowStyle Hidden
    Start-Sleep -Seconds 8
}

Push-Location $scriptDir
if (-not (Test-Path "node_modules\playwright")) {
    Write-Host "Playwright kuruluyor..."
    npm install playwright@1.49.0 --no-save 2>&1 | Out-Null
    npx playwright install chromium 2>&1 | Out-Null
}

Write-Host "Web ekran goruntuleri aliniyor..."
node (Join-Path $scriptDir "capture-kilavuz-screenshots.mjs")

Write-Host "Fixture ekran goruntuleri aliniyor..."
node (Join-Path $scriptDir "capture-kilavuz-fixtures.mjs")

Pop-Location
Write-Host "Tamamlandi. PNG dosyalari docs/images/ altinda."
