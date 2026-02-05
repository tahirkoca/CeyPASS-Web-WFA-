using System;

namespace CeyPASS.Entities.Concrete
{
    public class DogumGunleriDashboard
    {
        public int PersonelId { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int FirmaId { get; set; }
        public int IsyeriId { get; set; }
        public DateTime BuYilDogumGunu { get; set; }
        public int Gun { get; set; }
        public int Ay { get; set; }
        public int Yas { get; set; }
    }
}
