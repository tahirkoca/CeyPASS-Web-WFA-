namespace CeyPASS.Business.Services
{
    /// <summary>
    /// Yemek hakkı değişikliğinin yemekhane cihazlarına etkisini anlatan ortak metinler.
    /// Listener aktif yemek limiti olmayan kişiyi yemekhane cihazlarında pasif tutar; limit gelince aktif eder.
    /// </summary>
    public static class YemekhaneEtkiMesaji
    {
        public const string KaldirmaOnayi =
            "Yemek hakkı kaldırılıyor. Kişi yemekhane cihazlarında pasif olacak ve yemekhaneye giremeyecek.\n\nDevam edilsin mi?";

        public const string Verildi =
            "Yemek hakkı verildi. Kişi yemekhane cihazına yetkiliyse yaklaşık 10 sn içinde yemekhane cihazlarında aktif olacak.";

        public const string Kaldirildi =
            "Yemek hakkı kaldırıldı. Kişi yemekhane cihazlarında pasif olacak ve yemekhaneye giremeyecek.";

        public const string YemekHakkiYok =
            "Yemek hakkı yok. Kişi yemekhane cihazlarına pasif tanımlanacak.";

        /// <summary>Kayıttan önce onay sorulmalı mı (yemek hakkı kaldırılıyor).</summary>
        public static bool KaldirmaOnayiGerekir(bool oncekiYemekHakki, bool yeniYemekHakki)
            => oncekiYemekHakki && !yeniYemekHakki;

        /// <summary>Güncelleme sonrası gösterilecek metin; yemek hakkı değişmediyse null.</summary>
        public static string? Guncelleme(bool oncekiYemekHakki, bool yeniYemekHakki)
        {
            if (oncekiYemekHakki == yeniYemekHakki)
                return null;
            return yeniYemekHakki ? Verildi : Kaldirildi;
        }

        /// <summary>Yeni kayıt sonrası gösterilecek metin.</summary>
        public static string YeniKayit(bool yemekHakkiVar)
            => yemekHakkiVar ? Verildi : YemekHakkiYok;

        /// <summary>Ana mesajın sonuna varsa etki metnini ekler.</summary>
        public static string Ekle(string mesaj, string? etki)
            => string.IsNullOrWhiteSpace(etki) ? mesaj : mesaj + "\n\n" + etki;
    }
}
