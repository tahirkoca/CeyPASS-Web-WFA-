using System;

namespace CeyPASS.WFA.UserControls.Izinler
{
    /// <summary>
    /// Yıllık izin yarım gün: UI'da 08:30-12:30 / 14:00-18:00; DB'de 225 dk (12:15 / 17:45 bitiş).
    /// </summary>
    internal static class YarimGunYillikIzinHelper
    {
        public const int YillikIzinTipId = 2;
        public const int YarimGunSureDakika = 225;
        public const int TamGunDakika = 450;

        public const string AciklamaSabah = "08:30-12:30";
        public const string AciklamaOgledenSonra = "14:00-18:00";
        public const string YarimGunAciklamaEki = " (Yarım Gün)";

        public enum Dilim
        {
            Sabah = 0,
            OgledenSonra = 1
        }

        public static string DilimComboText(Dilim d) =>
            d == Dilim.Sabah ? "Sabah (08:30-12:30)" : "Öğleden sonra (14:00-18:00)";

        public static string AciklamaMetni(Dilim d) =>
            (d == Dilim.Sabah ? AciklamaSabah : AciklamaOgledenSonra) + YarimGunAciklamaEki;

        public static void KayitZamanlari(Dilim dilim, DateTime gun, out DateTime baslangic, out DateTime bitis)
        {
            gun = gun.Date;
            if (dilim == Dilim.Sabah)
            {
                baslangic = gun.AddHours(8).AddMinutes(30);
                bitis = gun.AddHours(12).AddMinutes(15);
            }
            else
            {
                baslangic = gun.AddHours(14);
                bitis = gun.AddHours(17).AddMinutes(45);
            }
        }

        public static bool TryDilimFromAciklama(string aciklama, out Dilim dilim)
        {
            dilim = Dilim.Sabah;
            if (string.IsNullOrWhiteSpace(aciklama))
                return false;
            var t = aciklama.Trim();
            if (t.Contains("14:00", StringComparison.Ordinal) || t.Contains("14.00", StringComparison.Ordinal))
            {
                dilim = Dilim.OgledenSonra;
                return true;
            }
            if (t.Contains("08:30", StringComparison.Ordinal) || t.Contains("08.30", StringComparison.Ordinal))
            {
                dilim = Dilim.Sabah;
                return true;
            }
            return false;
        }

        public static bool KayitYarimGunYillikIzinMi(int izinId, bool saatlikIzinMi, int sureDakika, string aciklama)
        {
            if (izinId != YillikIzinTipId || !saatlikIzinMi)
                return false;
            if (sureDakika == YarimGunSureDakika)
                return true;
            return TryDilimFromAciklama(aciklama, out _);
        }

        public static decimal GunEsdegeri(int sureDakika) =>
            sureDakika <= 0 ? 0m : Math.Round(sureDakika / (decimal)TamGunDakika, 2, MidpointRounding.AwayFromZero);
    }
}
