using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    public class MisafirKartGuncelleModel
    {
        public int AtamaId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public DateTime GirisSaati { get; set; }
        public DateTime? CikisSaati { get; set; }
        public string Aciklama { get; set; }
    }
}

