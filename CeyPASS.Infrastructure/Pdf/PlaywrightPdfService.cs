using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace CeyPASS.Infrastructure.Pdf;

/// <summary>
/// Headless Chromium ile HTML→PDF (tarayıcı yazdır → PDF ile aynı motor). Dış http(s) istekleri route ile kesilir.
/// <see cref="IPlaywright"/> süreç başına bir kez; tarayıcı her istekte açılıp kapanır.
/// Uygulama kapanırken <see cref="IPlaywright"/> üzerinde <c>Dispose()</c> çağırmıyoruz (VS/debugger ile çakışma riski).
/// </summary>
public sealed class PlaywrightPdfService : IPlaywrightPdfService
{
    private readonly PlaywrightPdfOptions _opt;
    private readonly ILogger<PlaywrightPdfService>? _log;
    private readonly SemaphoreSlim _concurrency;
    private readonly SemaphoreSlim _playwrightGate = new(1, 1);

    private IPlaywright? _playwright;

    public PlaywrightPdfService(IOptions<PlaywrightPdfOptions> options, ILogger<PlaywrightPdfService>? log = null)
    {
        _opt = options.Value;
        _log = log;
        var n = Math.Max(1, _opt.MaxConcurrent);
        _concurrency = new SemaphoreSlim(n, n);
    }

    public async Task<byte[]> HtmlToPdfAsync(string html, CancellationToken cancellationToken = default)
    {
        await _concurrency.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var pw = await EnsurePlaywrightAsync(cancellationToken).ConfigureAwait(false);
            var timeoutMs = Math.Max(5000, _opt.TimeoutSeconds * 1000);

            var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--disable-dev-shm-usage",
                    "--no-sandbox",
                    "--disable-gpu",
                    "--disable-software-rasterizer",
                    "--disable-extensions",
                    "--disable-background-networking"
                }
            }).ConfigureAwait(false);

            try
            {
                var page = await browser.NewPageAsync().ConfigureAwait(false);
                try
                {
                    await page.RouteAsync("**/*", async route =>
                    {
                        var url = route.Request.Url;
                        if (IsHttpOrHttps(url))
                            await route.AbortAsync().ConfigureAwait(false);
                        else
                            await route.ContinueAsync().ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    await page.SetContentAsync(html, new PageSetContentOptions
                    {
                        WaitUntil = WaitUntilState.Load,
                        Timeout = timeoutMs
                    }).ConfigureAwait(false);

                    return await page.PdfAsync(new PagePdfOptions
                    {
                        Format = "A4",
                        PrintBackground = true,
                        Margin = new Margin { Top = "0.5cm", Right = "0.5cm", Bottom = "0.5cm", Left = "0.5cm" }
                    }).ConfigureAwait(false);
                }
                finally
                {
                    try { await page.CloseAsync().ConfigureAwait(false); } catch (Exception ex) { _log?.LogDebug(ex, "Playwright page.CloseAsync"); }
                }
            }
            finally
            {
                try { await browser.CloseAsync().ConfigureAwait(false); } catch (Exception ex) { _log?.LogDebug(ex, "Playwright browser.CloseAsync"); }
            }
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Playwright PDF üretimi başarısız.");
            throw;
        }
        finally
        {
            _concurrency.Release();
        }
    }

    private async Task<IPlaywright> EnsurePlaywrightAsync(CancellationToken cancellationToken)
    {
        if (_playwright != null) return _playwright;

        await _playwrightGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_playwright != null) return _playwright;
            _playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            _log?.LogInformation("Playwright driver başlatıldı (süreç ömrü boyunca tek örnek).");
            return _playwright;
        }
        finally
        {
            _playwrightGate.Release();
        }
    }

    private static bool IsHttpOrHttps(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
               || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}
