using System;

namespace CeyPASS.Entities.Concrete
{
    public class PersonelVardiyaYemekYetki
    {
        public int Id { get; set; }
        public int CalismaSekliId { get; set; }
        public int IsyeriId { get; set; }
        public string IsyeriAdi { get; set; }
        public int CihazId { get; set; }
        public string CihazAdi { get; set; }
        public TimeSpan YemekBaslangicSaati { get; set; }
        public TimeSpan YemekBitisSaati { get; set; }
        public bool AktifMi { get; set; }
    }
}
