using System;

namespace CeyPASS.DataAccess
{
    public class PersonelVardiyaYemekYetkileri
    {
        public int Id { get; set; }
        public int CalismaSekliId { get; set; }
        public int IsyeriId { get; set; }
        public int CihazId { get; set; }
        public TimeSpan YemekBaslangicSaati { get; set; }
        public TimeSpan YemekBitisSaati { get; set; }
        public bool AktifMi { get; set; }
    }
}
