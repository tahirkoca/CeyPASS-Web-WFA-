namespace CeyPASS.WFA;

public readonly record struct ShortcutItem(string Keys, string Description);

/// <summary>
/// Sayfa / genel klavye kısayolları (F1 / Ctrl+/ paneli).
/// </summary>
internal static class ShortcutCatalog
{
    public static IReadOnlyList<ShortcutItem> Global { get; } =
    [
        new("Ctrl+/ veya F1", "Bu kısayol listesini aç"),
        new("Esc", "Diyalog / paneli kapat")
    ];

    public static IReadOnlyList<ShortcutItem> ForPage(string? pageKey)
    {
        var list = new List<ShortcutItem>(Global);
        list.AddRange(pageKey?.Trim() switch
        {
            "Raporlar" or "Izinler" or "KisiHareketler" or "AylikPuantaj" or "Personeller"
                or "Dashboard" =>
            [
                new("Ctrl+F", "Tabloda ara"),
                new("Ctrl+P", "Yazdır / dışa aktar önizleme")
            ],
            _ =>
            [
                new("Ctrl+F", "Tabloda ara (varsa)"),
                new("Ctrl+S", "Kaydet (form ekranlarında)")
            ]
        });
        return list;
    }
}
