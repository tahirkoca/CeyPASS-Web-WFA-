using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    /// <summary>Yeni arac karti atama form modeli.</summary>
    public class AracKartiYeniModel
    {
        public string KartId { get; set; }
        public string AdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string PasaportNo { get; set; }
        public string Plaka { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public DateTime GirisSaati { get; set; } = DateTime.Now;
        public string Aciklama { get; set; }
    }
}
