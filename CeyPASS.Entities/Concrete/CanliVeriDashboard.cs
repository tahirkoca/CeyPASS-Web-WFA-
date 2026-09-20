using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Canlı izleme akış satırı.</summary>
    public class CanliVeriDashboard
    {
        public DateTime HareketZamani { get; set; }
        public int PersonelId { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int FirmaId { get; set; }
        public int IsyeriId { get; set; }
        /// <summary>Giriş/çıkış vb. hareket kodu metni.</summary>
        public string HareketTipi { get; set; } = "";
    }
}
