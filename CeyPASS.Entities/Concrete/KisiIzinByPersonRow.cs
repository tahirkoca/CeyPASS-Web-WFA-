using System;

namespace CeyPASS.Entities.Concrete
{
    public class KisiIzinByPersonRow
    {
        public int KisiIzinId { get; set; }
        public DateTime IzinBaslangic { get; set; }
        public DateTime IzinBitis { get; set; }
        public decimal SureSaat { get; set; }
        public string Aciklama { get; set; }
        public DateTime? IslenmeTarihi { get; set; }
        public DateTime? GuncellemeTarihi { get; set; }
        public string SaatlikIzin { get; set; }
    }
}
