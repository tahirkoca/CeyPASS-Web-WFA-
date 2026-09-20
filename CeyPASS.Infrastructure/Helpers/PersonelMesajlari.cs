using System.Collections.Generic;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Personel tekrar aktif etme kullanıcı mesajları.</summary>
    public static class PersonelMesajlari
    {
        /// <summary>IT’ye yönlendiren cihaz/kart uyarı metni.</summary>
        public const string CihazKartAktiflesmeUyarisi =
            "Kartın turnike ve diğer cihazlarda tekrar aktifleşmesi için IT ekibinizle iletişime geçiniz.";

        /// <summary>Başarı mesajı: yemek limiti ve isteğe bağlı cihaz uyarısı.</summary>
        public static string TekrarAktifBasariMesaji(int? yenidenAktifYemekLimiti, bool cihazUyarisiGoster)
        {
            var parts = new List<string> { "Personel tekrar aktif edildi." };

            if (yenidenAktifYemekLimiti.HasValue && yenidenAktifYemekLimiti.Value > 0)
                parts.Add($"Günlük yemek limiti ({yenidenAktifYemekLimiti.Value}) yeniden aktifleştirildi.");

            if (cihazUyarisiGoster)
                parts.Add(CihazKartAktiflesmeUyarisi);

            return string.Join(" ", parts);
        }

        /// <summary><see cref="TekrarAktifBasariMesaji(int?, bool)"/> sonuna ek uyarı metni ekler.</summary>
        public static string TekrarAktifBasariMesaji(int? yenidenAktifYemekLimiti, bool cihazUyarisiGoster, string warningMessage)
        {
            var msg = TekrarAktifBasariMesaji(yenidenAktifYemekLimiti, cihazUyarisiGoster);
            if (!string.IsNullOrWhiteSpace(warningMessage))
                msg = msg + " " + warningMessage.Trim();
            return msg;
        }
    }
}
