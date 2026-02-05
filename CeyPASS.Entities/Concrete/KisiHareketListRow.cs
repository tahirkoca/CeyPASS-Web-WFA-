using System;

namespace CeyPASS.Entities.Concrete
{
    public class KisiHareketListRow
    {
        public int Id { get; set; }
        public string Firma { get; set; }
        public string SicilNo { get; set; }
        public string AdSoyad { get; set; }
        public string CihazAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string Tip { get; set; }
        public DateTime KayitZamani { get; set; }
        public bool AktifMi { get; set; }
    }
}
