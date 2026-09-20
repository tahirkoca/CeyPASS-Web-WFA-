using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Sicil ay export satırı; Logo/entegrasyon alan adları.</summary>
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
        /// <summary>0/1 doktor personeli bayrağı (kaynak sistem kodu).</summary>
        public int DokPersoneliMi { get; set; }
    }
}
