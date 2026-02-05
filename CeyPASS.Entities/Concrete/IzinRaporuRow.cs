using System;

namespace CeyPASS.Entities.Concrete
{
    public class IzinRaporuRow
    {
        public int KisiIzinId { get; set; }
        public string SicilNo { get; set; }
        public string AdSoyad { get; set; }
        public string FirmaAdi { get; set; }
        public string IzinTipi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string SureGun { get; set; }
        public string SureSaat { get; set; }
        public bool SaatlikIzinMi { get; set; }
        public string Aciklama { get; set; }
    }
}
