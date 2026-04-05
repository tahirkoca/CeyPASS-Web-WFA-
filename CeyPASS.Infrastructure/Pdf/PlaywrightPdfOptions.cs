namespace CeyPASS.Infrastructure.Pdf;

/// <summary>Playwright (Chromium) ile HTML→PDF — appsettings: Pdf.</summary>
public sealed class PlaywrightPdfOptions
{
    public int TimeoutSeconds { get; set; } = 25;

    public int MaxConcurrent { get; set; } = 3;
}
