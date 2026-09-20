using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    /// <summary>Misafir kart guncelleme form modeli.</summary>
    public class MisafirKartGuncelleModel
    {
        public int AtamaId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string PasaportNo { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public DateTime GirisSaati { get; set; }
        public DateTime? CikisSaati { get; set; }
        public string Aciklama { get; set; }
    }
}

