namespace CeyPASS.Entities.Concrete
{
    /// <summary>İşten çıkan sicili tekrar aktifleştirme iş kuralı sonucu.</summary>
    public sealed class KisiTekrarAktifSonuc
    {
        public bool Success { get; set; }
        /// <summary>Yeniden aktifte uygulanacak günlük yemek limiti.</summary>
        public int? YenidenAktifYemekLimiti { get; set; }
        /// <summary>Cihaz senkronu gecikebilir uyarısı göster.</summary>
        public bool CihazUyarisiGoster { get; set; }
        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }

        /// <returns>Başarısız sonuç.</returns>
        public static KisiTekrarAktifSonuc Basarisiz(string errorMessage = null)
        {
            return new KisiTekrarAktifSonuc { Success = false, ErrorMessage = errorMessage };
        }

        /// <returns>Başarılı sonuç.</returns>
        public static KisiTekrarAktifSonuc Basarili(int? yemekLimiti, bool cihazUyarisiGoster, string warningMessage = null)
        {
            return new KisiTekrarAktifSonuc
            {
                Success = true,
                YenidenAktifYemekLimiti = yemekLimiti,
                CihazUyarisiGoster = cihazUyarisiGoster,
                WarningMessage = warningMessage
            };
        }
    }
}
