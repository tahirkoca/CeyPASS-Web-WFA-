namespace CeyPASS.WFA;

/// <summary>Klavye kısayolu tanımı.</summary>
public readonly record struct ShortcutItem(string Keys, string Description);

/// <summary>
/// Sayfa / genel klavye kısayolları (F1 / Ctrl+/ paneli).
/// </summary>
internal static class ShortcutCatalog
{
    /// <summary>Tüm sayfalarda geçerli kısayollar.</summary>
    public static IReadOnlyList<ShortcutItem> Global { get; } =
    [
        new("Ctrl+/ veya F1", "Bu kısayol listesini aç"),
        new("Esc", "Diyalog / paneli kapat")
    ];

    /// <summary>Sayfa anahtarına göre ek kısayolları birleştirir.</summary>
    public static IReadOnlyList<ShortcutItem> ForPage(string? pageKey)
    {
        var list = new List<ShortcutItem>(Global);
        list.AddRange(pageKey?.Trim() switch
        {
            "Personeller" =>
            [
                new("Ctrl+F", "Tabloda ara"),
                new("Ctrl+P", "Yazdır / dışa aktar önizleme"),
                new("* Zorunlu", "Sicil, Ad Soyad, İşe Giriş, İşyeri her zaman zorunlu"),
                new("* Koşullu", "Firma/Taşeron→TC; Ziyaretçi/Araç→Kart; Yemek→Adedi")
            ],
            "KisiHareketler" =>
            [
                new("Ctrl+F", "Tabloda ara"),
                new("Ctrl+P", "Yazdır / dışa aktar önizleme"),
                new("Renk: gri", "Pasif hareket"),
                new("Renk: mavi/yeşil/pembe", "Giriş / Yemekhane / Çıkış (cihaz)"),
                new("Renk: sarı", "Elle eklenen veya güncellenen hareket")
            ],
            "Raporlar" or "Izinler" or "AylikPuantaj"
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
