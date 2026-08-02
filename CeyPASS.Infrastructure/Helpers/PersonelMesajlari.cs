using System.Collections.Generic;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class PersonelMesajlari
    {
        public const string CihazKartAktiflesmeUyarisi =
            "Kartın turnike ve diğer cihazlarda tekrar aktifleşmesi için IT ekibinizle iletişime geçiniz.";

        public static string TekrarAktifBasariMesaji(int? yenidenAktifYemekLimiti, bool cihazUyarisiGoster)
        {
            var parts = new List<string> { "Personel tekrar aktif edildi." };

            if (yenidenAktifYemekLimiti.HasValue && yenidenAktifYemekLimiti.Value > 0)
                parts.Add($"Günlük yemek limiti ({yenidenAktifYemekLimiti.Value}) yeniden aktifleştirildi.");

            if (cihazUyarisiGoster)
                parts.Add(CihazKartAktiflesmeUyarisi);

            return string.Join(" ", parts);
        }

        public static string TekrarAktifBasariMesaji(int? yenidenAktifYemekLimiti, bool cihazUyarisiGoster, string warningMessage)
        {
            var msg = TekrarAktifBasariMesaji(yenidenAktifYemekLimiti, cihazUyarisiGoster);
            if (!string.IsNullOrWhiteSpace(warningMessage))
                msg = msg + " " + warningMessage.Trim();
            return msg;
        }
    }
}
