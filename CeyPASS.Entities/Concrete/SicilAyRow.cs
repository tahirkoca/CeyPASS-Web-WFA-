using System;

namespace CeyPASS.Entities.Concrete
{
    public class SicilAyRow
    {
        public int SicilNo { get; set; }
        public string TcKimlikNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int? Firma { get; set; }
        public int? Isyeri { get; set; }
        public int? Bolum { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public int DokPersoneliMi { get; set; }
    }
}
