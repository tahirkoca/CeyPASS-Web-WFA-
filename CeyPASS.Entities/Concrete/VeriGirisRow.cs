using System;

namespace CeyPASS.Entities.Concrete
{
    public class VeriGirisRow
    {
        public int SicilNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public DateTime Tarih { get; set; }
        public string CalismaTipi { get; set; }
        public decimal Saat { get; set; }
    }
}
