using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Hareket listesi grid satırı.</summary>
    public class KisiHareketListRow
    {
        public int Id { get; set; }
        public string Firma { get; set; }
        public string SicilNo { get; set; }
        public string AdSoyad { get; set; }
        public string CihazAdi { get; set; }
        public DateTime Tarih { get; set; }
        /// <summary>Giriş/çıkış tip kodu metni.</summary>
        public string Tip { get; set; }
        public DateTime KayitZamani { get; set; }
        public bool AktifMi { get; set; }
    }
}
