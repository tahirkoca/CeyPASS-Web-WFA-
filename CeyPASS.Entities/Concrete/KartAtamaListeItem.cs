namespace CeyPASS.Entities.Concrete
{
    /// <summary>Misafir/araç kart atama listesi durum kodu.</summary>
    public enum KartAtamaListeDurum
    {
        Hazir = 0,
        Atanmis = 1,
        Giris = 2,
        Cikis = 3
    }

    /// <summary>Kart atama listesi ham satır; durum hareketlerden türetilir.</summary>
    public class KartAtamaListeItem
    {
        public string PersonelId { get; set; } = "";
        public string KartAdi { get; set; } = "";
        public int? AtamaId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public string Plaka { get; set; }
        public KartAtamaListeDurum Durum { get; set; }

        public string DurumText =>
            Durum switch
            {
                KartAtamaListeDurum.Hazir => "HAZIR",
                KartAtamaListeDurum.Atanmis => "ATANMIŞ",
                KartAtamaListeDurum.Giris => "GİRİŞ",
                KartAtamaListeDurum.Cikis => "ÇIKIŞ",
                _ => ""
            };

        /// <summary>Liste sütununda ad + plaka birleşik metin.</summary>
        public static string FormatKisiPlaka(string misafirAdSoyad, string plaka)
        {
            var ad = (misafirAdSoyad ?? "").Trim();
            var pl = (plaka ?? "").Trim();
            if (ad.Length == 0) return pl;
            if (pl.Length == 0) return ad;
            return ad + " (" + pl + ")";
        }
    }

    /// <summary>Kart Atamaları satırı — UI istemcileri için ortak şekil.</summary>
    public class KartAtamaListeSatir
    {
        public string Tip { get; set; } = "";
        public string TipLabel { get; set; } = "";
        public string PersonelId { get; set; } = "";
        public string KartAdi { get; set; } = "";
        public string KisiPlaka { get; set; } = "";
        public int? AtamaId { get; set; }
        public string Durum { get; set; } = "";
        public string DurumText { get; set; } = "";
        public bool CihazdaAktif { get; set; }
        /// <summary>Atama açık ve cihazda aktifken kısıtlama yapılabilir.</summary>
        public bool CanKisitla { get; set; }
        public bool CanSerbestBirak { get; set; }

        /// <param name="tip">"arac" veya "misafir".</param>
        /// <param name="cihazdaAktif">Kart cihazda yüklü mü.</param>
        public static KartAtamaListeSatir From(KartAtamaListeItem x, string tip, bool cihazdaAktif)
        {
            var tipKey = string.Equals(tip, "arac", System.StringComparison.OrdinalIgnoreCase) ? "arac" : "misafir";
            var atanmis = x != null && x.Durum != KartAtamaListeDurum.Hazir;
            return new KartAtamaListeSatir
            {
                Tip = tipKey,
                TipLabel = tipKey == "arac" ? "Araç" : "Misafir",
                PersonelId = x?.PersonelId ?? "",
                KartAdi = x?.KartAdi ?? "",
                KisiPlaka = KartAtamaListeItem.FormatKisiPlaka(x?.MisafirAdSoyad, x?.Plaka),
                AtamaId = x?.AtamaId,
                Durum = x?.Durum.ToString() ?? "",
                DurumText = x?.DurumText ?? "",
                CihazdaAktif = cihazdaAktif,
                CanKisitla = atanmis && cihazdaAktif,
                CanSerbestBirak = !cihazdaAktif
            };
        }
    }

    /// <summary>Atama sonrası son hareket yönü (giriş/çıkış).</summary>
    public class PersonelSonHareketYon
    {
        public string PersonelId { get; set; } = "";
        public bool GirisMi { get; set; }
        public System.DateTime Tarih { get; set; }
    }

    /// <summary>Atama listesi durum hesaplama (test edilebilir).</summary>
    public static class KartAtamaListeDurumHelper
    {
        /// <param name="hasOpenAtama">Kartta Bitis=null atama var mı</param>
        /// <param name="girisMiAfterBaslangic">Atama başlangıcından sonraki son hareket yönü; yoksa null</param>
        /// <returns>Hesaplanan liste durumu.</returns>
        public static KartAtamaListeDurum Resolve(bool hasOpenAtama, bool? girisMiAfterBaslangic)
        {
            if (!hasOpenAtama)
                return KartAtamaListeDurum.Hazir;
            if (girisMiAfterBaslangic == null)
                return KartAtamaListeDurum.Atanmis;
            return girisMiAfterBaslangic.Value
                ? KartAtamaListeDurum.Giris
                : KartAtamaListeDurum.Cikis;
        }
    }
}
