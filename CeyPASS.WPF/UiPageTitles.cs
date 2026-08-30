namespace CeyPASS.WPF;

/// <summary>
/// Yetki / sayfa anahtarlarını kullanıcıya görünen başlıklara çevirir.
/// </summary>
internal static class UiPageTitles
{
    public static string Friendly(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title ?? "";

        return title.Trim() switch
        {
            "KisiHareketler" => "Kişi Hareketleri",
            "Personeller" => "Personel Tanımlama",
            "Departmanlar" => "Departman Tanımlama",
            "Pozisyonlar" => "Pozisyon Tanımlama",
            "Firmalar" => "Firma Tanımlama",
            "Isyerler" => "İşyeri Tanımlama",
            "Izinler" => "İzinler",
            "CalismaStatuleri" => "Çalışma Statüleri",
            "Vardiyalar" => "Vardiyalar",
            "Cihazlar" => "Cihazlar",
            "ResmiTatiller" => "Resmi Tatiller",
            "AylikPuantaj" => "Aylık Puantaj",
            "Raporlar" => "Raporlar",
            "Guncelleme" => "Güncelleme Bildirimi",
            "Admin" or "AdminPanel" => "Admin Panel",
            "Dashboard" => "Ana Sayfa",
            _ => title
        };
    }
}
