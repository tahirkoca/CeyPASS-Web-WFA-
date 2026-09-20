using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Geçmiş ziyaretçi/araç kaydı; yeni atamada otomatik doldurma için.</summary>
    public class GecmisZiyaretciItem
    {
        public string AdSoyad { get; set; } = "";
        public string TCKimlikNo { get; set; }
        public string PasaportNo { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public string Plaka { get; set; }
        public string Notlar { get; set; }
        public DateTime SonZiyaret { get; set; }

        public string Gosterim
        {
            get
            {
                var ad = (AdSoyad ?? "").Trim();
                var plaka = (Plaka ?? "").Trim();
                if (string.IsNullOrEmpty(plaka))
                    return ad;
                if (string.IsNullOrEmpty(ad))
                    return plaka;
                return $"{ad} ({plaka})";
            }
        }

        public override string ToString() => Gosterim;
    }
}
