using System;

namespace CeyPASS.Entities.Concrete
{
    public class PuantajsizKartAtama
    {
        public int AtamaId { get; set; }
        public string KartId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public string KartAdi { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime? Bitis { get; set; }
        public string Notlar { get; set; }
    }
}
