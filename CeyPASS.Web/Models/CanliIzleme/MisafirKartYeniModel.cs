using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    /// <summary>Yeni misafir kart atama form modeli.</summary>
    public class MisafirKartYeniModel
    {
        public string KartId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string PasaportNo { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public DateTime GirisSaati { get; set; } = DateTime.Now;
        public string Aciklama { get; set; }
    }
}

