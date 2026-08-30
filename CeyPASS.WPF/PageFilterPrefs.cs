namespace CeyPASS.WPF;

public sealed class PageFilterPrefs
{
    public int? FirmaId { get; set; }
    public int? IsyeriId { get; set; }
    public bool? BoolA { get; set; }
    public bool? BoolB { get; set; }
    public string? Extra { get; set; }
    public DateTime? DateA { get; set; }
    public DateTime? DateB { get; set; }
}

internal static class PageFilterPrefsStore
{
    public static PageFilterPrefs? Load(string pageKey)
        => UiUserPrefs.ReadJson<PageFilterPrefs>($"filter-{pageKey}.json");

    public static void Save(string pageKey, PageFilterPrefs prefs)
        => UiUserPrefs.WriteJson($"filter-{pageKey}.json", prefs);
}
