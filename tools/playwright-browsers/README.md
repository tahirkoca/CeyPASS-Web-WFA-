# Playwright Chromium (İzin PDF)

İzin formları PDF’i **HTML → headless Chromium** ile üretilir (`CeyPASS.Infrastructure.Pdf.PlaywrightPdfService`). **CeyPASS.Web** ve **CeyPASS.Api** aynı servisi kullanır; çıktı tarayıcıda **Yazdır → PDF** ile aynı motoru hedefler.

## Kurulum (geliştirme)

1. Çözüm kökünde veya `CeyPASS.Web` klasöründe:

```powershell
dotnet build CeyPASS.Web\CeyPASS.Web.csproj
cd CeyPASS.Web\bin\Debug\net8.0
.\playwright.ps1 install chromium
```

Alternatif: `npx playwright install chromium` (Playwright sürümü `Microsoft.Playwright` ile uyumlu olmalı).

2. Ortam (isteğe bağlı, `launchSettings` ve `Program.cs` ile de set edilir):

- `PLAYWRIGHT_CHROMIUM_USE_HEADLESS_SHELL=1` — daha hafif headless shell.

3. **Publish / IIS:** Chromium tarayıcı dosyalarını sunucuya taşıyın ve `PLAYWRIGHT_BROWSERS_PATH` ile klasörü gösterin (örnek aşağıda).

## appsettings (`Pdf`)

```json
"Pdf": {
  "MaxConcurrent": 3,
  "TimeoutSeconds": 25
}
```

## Sorun giderme

- `Executable doesn't exist` / tarayıcı bulunamadı: `playwright install chromium` ve `PLAYWRIGHT_BROWSERS_PATH` kontrolü.
- **Visual Studio PDF sonrası IDE kapanıyorsa:** [VISUAL_STUDIO.md](./VISUAL_STUDIO.md).

## wkhtmltopdf (alternatif)

Eski/ayrı bir HTML→PDF aracı olarak [wkhtmltopdf](../wkhtmltopdf/README.md) dokümante edilmiştir; varsayılan üretim **Playwright**’tır.
