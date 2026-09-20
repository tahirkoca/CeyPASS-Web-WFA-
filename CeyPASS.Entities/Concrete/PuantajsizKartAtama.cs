using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Puantajsız (misafir/araç) kart ataması; sicil oluşturulmadan geçiş.</summary>
    public class PuantajsizKartAtama
    {
        public int AtamaId { get; set; }
        public string KartId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string PasaportNo { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public string KartAdi { get; set; }
        public DateTime Baslangic { get; set; }
        /// <summary>null ise atama hâlâ açık (içeride).</summary>
        public DateTime? Bitis { get; set; }
        public string Notlar { get; set; }
        public string Plaka { get; set; }
    }
}
