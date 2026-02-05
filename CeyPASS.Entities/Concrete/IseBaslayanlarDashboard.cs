using System;

namespace CeyPASS.Entities.Concrete
{
    public class IseBaslayanlarDashboard
    {
        public int PersonelId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int FirmaId { get; set; }
        public int IsyeriId { get; set; }
        public DateTime BaslamaTarihi { get; set; }
    }
}
