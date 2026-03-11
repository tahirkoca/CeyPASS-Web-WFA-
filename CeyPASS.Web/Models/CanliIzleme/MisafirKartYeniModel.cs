using System;

namespace CeyPASS.Web.Models.CanliIzleme
{
    public class MisafirKartYeniModel
    {
        public string KartId { get; set; }
        public string MisafirAdSoyad { get; set; }
        public DateTime GirisSaati { get; set; } = DateTime.Now;
        public string Aciklama { get; set; }
    }
}

