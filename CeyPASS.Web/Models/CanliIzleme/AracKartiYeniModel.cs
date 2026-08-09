using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    public class AracKartiYeniModel
    {
        public string KartId { get; set; }
        public string AdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string Plaka { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public DateTime GirisSaati { get; set; } = DateTime.Now;
        public string Aciklama { get; set; }
    }
}
