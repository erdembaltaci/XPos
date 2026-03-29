# ============================================================
#  XPos - Tüm Servisleri Başlat
#  Kullanım: .\RunApps.ps1
# ============================================================

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   XPos - Servisler Baslatiliyor..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ── 1. XPos.WebAPI (Port 5029) ──────────────────────────────
Write-Host "[1/4] XPos.WebAPI baslatiliyor  → http://localhost:5029" -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "cd '$root\src\XPos.WebAPI'; Write-Host 'XPos.WebAPI baslatiliyor...' -ForegroundColor Green; dotnet run"

Start-Sleep -Seconds 2

# ── 2. XPos.Client (Blazor WASM) ───────────────────────────
Write-Host "[2/4] XPos.Client baslatiliyor → Blazor WebAssembly" -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "cd '$root\src\XPos.Client'; Write-Host 'XPos.Client baslatiliyor...' -ForegroundColor Yellow; dotnet run"

Start-Sleep -Seconds 2

# ── 3. XPos.ML (Python FastAPI - Port 5001) ─────────────────
Write-Host "[3/4] XPos.ML baslatiliyor     → http://localhost:5001" -ForegroundColor Magenta
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "cd '$root\src\XPos.ML'; Write-Host 'XPos AI Servisi baslatiliyor...' -ForegroundColor Magenta; uvicorn app:app --host 0.0.0.0 --port 5001 --reload"

Start-Sleep -Seconds 2

# ── 4. XPos.Mobile → Masaüstü (Windows) ────────────────────
#    Komut: dotnet run -f net9.0-windows10.0.19041.0
Write-Host "[4/4] XPos.Mobile (Desktop) baslatiliyor → Windows Masaustu Uygulamasi" -ForegroundColor Blue
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "cd '$root\src\XPos.Mobile'; Write-Host 'XPos Desktop (Windows) baslatiliyor...' -ForegroundColor Blue; dotnet run -f net9.0-windows10.0.19041.0"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Tum servisler baslatildi!" -ForegroundColor Cyan
Write-Host ""
Write-Host "  API       : http://localhost:5029" -ForegroundColor White
Write-Host "  Swagger   : http://localhost:5029/swagger" -ForegroundColor White
Write-Host "  AI        : http://localhost:5001" -ForegroundColor White
Write-Host "  Desktop   : MAUI Windows penceresi acilir" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ── 5. XPos.Mobile → Android (Opsiyonel) ───────────────────
#    Komut: dotnet run -f net9.0-android
#    Not  : Android emülatörü veya USB ile bağlı cihaz gerektirir.
$response = Read-Host "Android emulatorunu/cihazini da baslatmak ister misiniz? (e/h)"
if ($response -eq "e" -or $response -eq "E") {
    Write-Host "XPos.Mobile (Android) baslatiliyor..." -ForegroundColor DarkCyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", `
        "cd '$root\src\XPos.Mobile'; Write-Host 'XPos Android baslatiliyor...' -ForegroundColor DarkCyan; dotnet run -f net9.0-android"
}

Write-Host ""
Write-Host "Cikis icin bu pencereyi kapatin." -ForegroundColor DarkGray
