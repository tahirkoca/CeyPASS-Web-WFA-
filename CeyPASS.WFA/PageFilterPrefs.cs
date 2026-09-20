namespace CeyPASS.WFA;

/// <summary>Sayfa filtrelerinin kalıcı saklanması için DTO (firma, işyeri, tarih aralığı vb.).</summary>
public sealed class PageFilterPrefs
{
    /// <summary>Son seçilen firma.</summary>
    public int? FirmaId { get; set; }
    /// <summary>Son seçilen işyeri.</summary>
    public int? IsyeriId { get; set; }
    /// <summary>Sayfaya özel boolean bayrak A.</summary>
    public bool? BoolA { get; set; }
    /// <summary>Sayfaya özel boolean bayrak B.</summary>
    public bool? BoolB { get; set; }
    /// <summary>Serbest metin / ek anahtar.</summary>
    public string? Extra { get; set; }
    /// <summary>Tarih aralığı başlangıcı.</summary>
    public DateTime? DateA { get; set; }
    /// <summary>Tarih aralığı bitişi.</summary>
    public DateTime? DateB { get; set; }
}

/// <summary>PageFilterPrefs JSON okuma/yazma (%LocalAppData%\CeyPASS).</summary>
internal static class PageFilterPrefsStore
{
    /// <summary>filter-{pageKey}.json dosyasından tercihleri yükler.</summary>
    public static PageFilterPrefs? Load(string pageKey)
        => UiUserPrefs.ReadJson<PageFilterPrefs>($"filter-{pageKey}.json");

    /// <summary>filter-{pageKey}.json dosyasına tercihleri kaydeder.</summary>
    public static void Save(string pageKey, PageFilterPrefs prefs)
        => UiUserPrefs.WriteJson($"filter-{pageKey}.json", prefs);
}
