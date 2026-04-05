# Visual Studio + Playwright (PDF) — IDE kapanıyorsa

Playwright, Chromium ve Node tabanlı **driver** süreçleri başlatır. Bazı kurulumlarda Visual Studio hata ayıklayıcısı bu alt süreçlere bağlanır; süreç kapanınca IDE’nin de düşmesi görülebilir.

## Hızlı denemeler

1. **Hata ayıklayıcı olmadan çalıştır**: `Ctrl+F5` (Start Without Debugging). Kapanma kesilirse sorun büyük ihtimalle debugger + child process etkileşimidir.

2. **Visual Studio**: *Araçlar → Seçenekler → Hata Ayıklama → Genel*  
   - **“Alt işlemlere otomatik bağlan”** (Automatically attach to child processes) seçeneğini kapatın.

3. **launchSettings**: Web/API profillerine eklenen `PLAYWRIGHT_CHROMIUM_USE_HEADLESS_SHELL=1`, tam Chromium yerine headless shell kullanımını teşvik eder.

4. **Windows Güvenlik / antivirüs**: `chrome.exe` / `node.exe` benzeri süreçleri izole eden ürünler bazen kapanışta host’u etkiler.

5. **Olay Günlüğü**: *Windows Günlükleri → Uygulama* içinde aynı dakikada **Application Error** / faulting module (`chrome.exe`, `node.exe`, `iisexpress.exe`, `dotnet.exe`) kaydı var mı bakın.

## Kod tarafı

`PlaywrightPdfService` uygulama kapanırında `IPlaywright.Dispose()` **çağırmıyor** (süreç sonunda OS temizler); bu, VS ile kapanış çakışmasını azaltmak içindir.

`PLAYWRIGHT_CHROMIUM_USE_HEADLESS_SHELL=1` **Program.cs** içinde de (yalnızca ortamda yoksa) ayarlanır; böylece `bin\Debug\...\CeyPASS.Web.exe` ile doğrudan çalıştırınca da launchSettings’e ihtiyaç kalmaz.

Production IIS’te worker recycle normal şekilde süreç yenilenir; ek ayar gerekmez.
