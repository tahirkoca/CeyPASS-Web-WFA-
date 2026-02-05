using System;

namespace CeyPASS.Entities.Concrete
{
    public class KisiIzinListRow
    {
        public int KisiIzinId { get; set; }
        public string SicilNo { get; set; }
        public string AdSoyad { get; set; }
        public string FirmaAdi { get; set; }
        public string IzinTipi { get; set; }
        public DateTime IzinBaslangic { get; set; }
        public DateTime IzinBitis { get; set; }
        public string SureGun { get; set; }
        public double SureSaat { get; set; }
        public string SaatlikIzin { get; set; }
        public string Aciklama { get; set; }
        public DateTime? IslenmeTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
    }
}
