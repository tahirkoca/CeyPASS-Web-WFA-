using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    public class AracKartiGuncelleModel
    {
        public int AtamaId { get; set; }
        public string AdSoyad { get; set; }
        public string TCKimlikNo { get; set; }
        public string Plaka { get; set; }
        public string ZiyaretEdilenKisi { get; set; }
        public DateTime GirisSaati { get; set; }
        public DateTime? CikisSaati { get; set; }
        public string Aciklama { get; set; }
    }
}
