namespace CeyPASS.Infrastructure.Pdf;

/// <summary>Playwright (Chromium) ile HTML→PDF — appsettings: Pdf.</summary>
public sealed class PlaywrightPdfOptions
{
    /// <summary>HTML yükleme ve PDF üretimi zaman aşımı (saniye).</summary>
    public int TimeoutSeconds { get; set; } = 25;

    /// <summary>Eşzamanlı PDF üretim isteği üst sınırı.</summary>
    public int MaxConcurrent { get; set; } = 3;
}
