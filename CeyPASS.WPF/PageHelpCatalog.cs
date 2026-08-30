namespace CeyPASS.WPF;

public sealed class PageHelpTopic
{
    public required string Title { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    /// <summary>Parent UserControl içindeki x:Name listesi (pulse için).</summary>
    public IReadOnlyList<string> PulseTargetNames { get; init; } = Array.Empty<string>();
}

public static class PageHelpCatalog
{
    private static readonly Dictionary<string, PageHelpTopic> Topics = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dashboard"] = new PageHelpTopic
        {
            Title = "Ana Sayfa — rehber",
            Steps =
            [
                "Firma seçerek KPI ve listeleri o firmaya göre yenileyin.",
                "KPI kartlarına tıklayınca ilgili rapora gidersiniz (tarih aralığı KPI dönemine göre dolar).",
                "Yenile ile verileri tekrar yükleyin."
            ]
        },
        ["Personeller"] = new PageHelpTopic
        {
            Title = "Personeller — işlemler",
            Steps =
            [
                "Ekle: formu doldurun → Kaydet. Vazgeç ile iptal edin.",
                "Güncelle: soldan kişi seçin → Güncelle → düzenleyin → Kaydet.",
                "İşten Çıkar: seçili aktif personeli çıkış tarihi ile pasife alır (puantaj bayrağı korunur).",
                "Aktif Et: işten çıkanlar listesinde seçili kişiyi tekrar aktif eder.",
                "Filtreler: Firma / İşyeri / İşten Çıkanlar / Puantaj Yapılanlar listesini daraltır."
            ],
            PulseTargetNames = ["BtnEkle", "BtnGuncelle", "BtnIstenCikar", "BtnAktifEt"]
        },
        ["Departmanlar"] = Crud("Departmanlar", "Departman"),
        ["Firmalar"] = Crud("Firmalar", "Firma"),
        ["Isyerler"] = Crud("İşyerleri", "İşyeri"),
        ["Pozisyonlar"] = Crud("Pozisyonlar", "Pozisyon"),
        ["CalismaStatuleri"] = Crud("Çalışma Statüleri", "Statü"),
        ["Cihazlar"] = Crud("Cihazlar", "Cihaz"),
        ["Vardiyalar"] = new PageHelpTopic
        {
            Title = "Vardiyalar — işlemler",
            Steps =
            [
                "Ekle: yeni vardiya için formu doldurun → Kaydet.",
                "Güncelle: listeden seçin → Güncelle → saatleri düzenleyin → Kaydet.",
                "Sil: seçili vardiyayı siler (onay ister).",
                "Saat alanlarında HH:mm formatına dikkat edin."
            ],
            PulseTargetNames = ["BtnEkle", "BtnGuncelle", "BtnSil"]
        },
        ["Izinler"] = new PageHelpTopic
        {
            Title = "İzinler — işlemler",
            Steps =
            [
                "Filtrelerle personel / tarih aralığını seçin, listeyi getirin.",
                "Yeni izin: formu doldurun → Kaydet.",
                "Düzenleme: satır seçin → güncelleyin → Kaydet / Vazgeç.",
                "İzin kağıdı / PDF işlemleri seçili kayda göre çalışır."
            ]
        },
        ["ResmiTatiller"] = new PageHelpTopic
        {
            Title = "Resmi Tatiller — işlemler",
            Steps =
            [
                "Yenile ile tatil listesini güncelleyin.",
                "Sağdaki bölümlerden tatil ekleyin veya düzenleyin.",
                "Kaydetmeden önce tarih ve açıklamayı kontrol edin."
            ]
        },
        ["Raporlar"] = new PageHelpTopic
        {
            Title = "Raporlar — işlemler",
            Steps =
            [
                "Firma (gerekirse TÜMÜ) ve rapor türünü seçin.",
                "Tarih aralığını girin; işyeri/cihaz listesi rapora göre görünür.",
                "Getir ile raporu çalıştırın.",
                "Ana sayfa KPI’sından geldiyseniz tür ve tarihler otomatik dolabilir."
            ]
        },
        ["AylikPuantaj"] = new PageHelpTopic
        {
            Title = "Aylık Puantaj — işlemler",
            Steps =
            [
                "Firma / yıl / ay seçip listeyi getirin.",
                "Satırları inceleyin; gerekirse satır düzenleme ile düzeltin.",
                "İşten çıkanlar çıkış ayında listede kalabilir (puantaj için)."
            ]
        },
        ["KisiHareket"] = new PageHelpTopic
        {
            Title = "Kişi Hareketleri — işlemler",
            Steps =
            [
                "Firma, kişi ve tarih aralığı seçerek hareketleri getirin.",
                "Pasif Hareketler işaretliyken Sil yerine Aktif Et görünür; pasif kaydı tekrar aktif edebilirsiniz.",
                "Grid’de sıralama / filtre / arama (Ctrl+F) kullanabilirsiniz.",
                "Yazdır / Excel / PDF için grid menüsünü veya yazdırmayı kullanın."
            ]
        },
        ["AdminPanel"] = new PageHelpTopic
        {
            Title = "Admin Panel — rehber",
            Steps =
            [
                "Yalnızca süper yönetici erişir.",
                "Sekmelerden kullanıcı, yetki ve sistem ayarlarını yönetin.",
                "Değişikliklerden sonra ilgili ekranları Yenile ile doğrulayın."
            ]
        }
    };

    private static PageHelpTopic Crud(string title, string entity) => new()
    {
        Title = $"{title} — işlemler",
        Steps =
        [
            $"Ekle: yeni {entity} bilgilerini doldurun → Kaydet.",
            "Güncelle: listeden seçin → Güncelle → düzenleyin → Kaydet.",
            "Sil: seçili kaydı siler (onay ister).",
            "Vazgeç ile düzenleme modundan çıkabilirsiniz."
        ],
        PulseTargetNames = ["BtnEkle", "BtnGuncelle", "BtnSil"]
    };

    public static PageHelpTopic? Get(string? topicKey)
    {
        if (string.IsNullOrWhiteSpace(topicKey)) return null;
        return Topics.TryGetValue(topicKey.Trim(), out var t) ? t : null;
    }
}
