namespace CeyPASS.Entities.Concrete
{
    public sealed class KisiTekrarAktifSonuc
    {
        public bool Success { get; set; }
        public int? YenidenAktifYemekLimiti { get; set; }
        public bool CihazUyarisiGoster { get; set; }
        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }

        public static KisiTekrarAktifSonuc Basarisiz(string errorMessage = null)
        {
            return new KisiTekrarAktifSonuc { Success = false, ErrorMessage = errorMessage };
        }

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
