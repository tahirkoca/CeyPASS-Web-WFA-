namespace CeyPASS.WPF;

/// <summary>Sayfa yardım balonu içeriği (başlık, adımlar, isteğe bağlı pulse hedefleri).</summary>
public sealed class PageHelpTopic
{
    public required string Title { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    /// <summary>Parent UserControl içindeki x:Name listesi (pulse için).</summary>
    public IReadOnlyList<string> PulseTargetNames { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Ekran anahtarları → kullanıcı rehberi metinleri (<see cref="Controls.CeypassHelpTip"/> Topic bağlantısı).
/// </summary>
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
                "Zorunlu alanlar (*) ekleme/güncellemede görünür: Sicil No, Ad Soyad, İşe Giriş, İşyeri her zaman zorunludur.",
                "Koşullu zorunlular: Firma Personeli veya Taşeron → TC Kimlik No; Ziyaretçi veya Araç Kartı → Kart No; Yemek Hakkı → Yemek Adedi. İşaret kalkınca * da kalkar.",
                "İşten Çıkar: seçili aktif personeli çıkış tarihi ile pasife alır (puantaj bayrağı korunur).",
                "Aktif Et: işten çıkanlar listesinde seçili kişiyi tekrar aktif eder.",
                "Çoklu Sicil: kayıtlı personelde Puantaj Yapılır ve TC doluysa hedef sicil eşleştirmelerini açar; bağlantı ekleyin/düzenleyin, gerekirse modalde Tümünü pasifleştir kullanın.",
                "Filtreler: Firma / İşyeri / İşten Çıkanlar / Puantaj Yapılanlar listesini daraltır."
            ],
            PulseTargetNames = ["BtnEkle", "BtnGuncelle", "BtnIstenCikar", "BtnAktifEt", "BtnCokluSicil"]
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
                "Firma / İşyeri / personel / tarih filtreleriyle listeyi getirin.",
                "İşyeri filtresi yetkinizdeki işyerleriyle sınırlıdır; Tümü yalnızca yetkili kapsamı gösterir.",
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
                "Durum (Aktif / İşten Çıkanlar), firma, kişi ve tarih aralığı seçerek hareketleri getirin.",
                "İşten çıkanlarda Kart Tipi filtresi uygulanmaz; çıkan personelin geçmiş hareketlerine bakabilirsiniz.",
                "Pasif Hareketler işaretliyken Sil yerine Aktif Et görünür; pasif kaydı tekrar aktif edebilirsiniz.",
                "Satır renkleri: gri = pasif; mavi = Giriş; yeşil = Yemekhane; pembe = Çıkış; sarı = elle eklenen veya güncellenen hareket (Turnike/CihazId korunur).",
                "Grid’de sıralama / filtre / arama (Ctrl+F) kullanabilirsiniz.",
                "Yazdır / Excel / PDF için grid menüsünü veya yazdırmayı kullanın."
            ]
        },
        // Canlı izleme kart atama: HAZIR/ATANMIŞ/GİRİŞ/ÇIKIŞ durumları ile cihaz kısıt komutları farklı kavramlardır.
        ["CanliIzlemeKartAtama"] = new PageHelpTopic
        {
            Title = "Kart Atamaları — durumlar",
            Steps =
            [
                "HAZIR: Kart boşta; kimseye verilmemiş. Çift tıklayınca yeni atama açılır.",
                "ATANMIŞ: Kart birine verilmiş; henüz turnikede giriş/çıkış yok.",
                "GİRİŞ: Atama sonrası son hareket giriş (içeride).",
                "ÇIKIŞ: Atama sonrası son hareket çıkış.",
                "ATANMIŞ / GİRİŞ / ÇIKIŞ satırına çift tık → atamayı güncelleyin (Kartı Kısıtla’dan bağımsızdır).",
                "Satırın sağındaki Kartı Kısıtla / Kart Kısıtı Kaldır: cihaz kuyruğuna komut yazar; atamayı değiştirmez.",
                "Kısıt kanıtı yoksa kart serbest sayılır. Atanmış+serbest → Kısıtla; kısıtlı → Kısıtı Kaldır; HAZIR+serbest → ikisi kapalı.",
                "Kart teslim alınınca (çıkış verilince) kısıt otomatik kaldırılır; HAZIR kart kısıtlanamaz.",
                "‘Kart durumları’ (? yanı): misafir+araç anlık serbest/kısıtlı listesi (içeride Tümü/Misafir/Araç filtresi); seçerek veya topluca yönetin.",
                "Üstteki Misafir / Araç seçimi liste tipini değiştirir."
            ],
            PulseTargetNames = ["AtamaGrid", "CmbAtamaTip", "BtnKartDurumToplu"]
        },
        ["Guncelleme"] = new PageHelpTopic
        {
            Title = "Güncelleme Bildirimi — rehber",
            Steps =
            [
                "Yalnızca süper yönetici erişir.",
                "Sürüm notlarını doldurun; Önizleme ile e-posta içeriğini kontrol edin.",
                "Gönder ile bildirim e-postasını ilgili alıcı gruplarına iletin."
            ]
        },
        ["AdminPanel"] = new PageHelpTopic
        {
            Title = "Güncelleme Bildirimi — rehber",
            Steps =
            [
                "Yalnızca süper yönetici erişir.",
                "Sürüm notlarını doldurun; Önizleme ile e-posta içeriğini kontrol edin.",
                "Gönder ile bildirim e-postasını ilgili alıcı gruplarına iletin."
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

    /// <summary>Topic anahtarına göre rehber; bilinmeyen anahtar için null.</summary>
    public static PageHelpTopic? Get(string? topicKey)
    {
        if (string.IsNullOrWhiteSpace(topicKey)) return null;
        return Topics.TryGetValue(topicKey.Trim(), out var t) ? t : null;
    }
}
