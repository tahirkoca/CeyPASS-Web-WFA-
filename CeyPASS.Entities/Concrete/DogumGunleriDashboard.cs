using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Dashboard doğum günü yaklaşan personel satırı.</summary>
    public class DogumGunleriDashboard
    {
        public int PersonelId { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int FirmaId { get; set; }
        public int IsyeriId { get; set; }
        public string FirmaAdi { get; set; } = "";
        public string IsyeriAdi { get; set; } = "";
        /// <summary>İçinde bulunulan yıla göre hesaplanmış doğum günü tarihi.</summary>
        public DateTime BuYilDogumGunu { get; set; }
        public int Gun { get; set; }
        public int Ay { get; set; }
        public int Yas { get; set; }
    }
}
